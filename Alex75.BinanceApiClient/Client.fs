namespace Alex75.BinanceApiClient

open System
open System.Text
open System.Security.Cryptography
open System.Globalization
open Newtonsoft.Json; open Newtonsoft.Json.Linq
open Flurl.Http

open Alex75.Cryptocurrencies
open models


type public Settings = { TickerCacheDuration:TimeSpan; PublicKey:string; SecretKey:string; }

type CreateOrderPayload = { symbol:string; side:string; ``type``:string; quantity:decimal; timestamp:Int64}


type public Client(settings:Settings) =

    let baseUrl = "https://api.binance.com"
    let f = sprintf
    let symbol (pair:CurrencyPair) = f"%O%O" pair.Main pair.Other

    let cache = Cache()

    let get_timestamp () = (f"%s/%s" baseUrl "/api/v1/time").GetJsonAsync<ServerTime>().Result.serverTime
    // ref: https://binance-docs.github.io/apidocs/spot/en/#endpoint-security-type
    let recvWindow = 10*1000 // recvWindow cannot exceed 60000. Default: 5000
    
    let parseErrorResponse (responseContent) = 
        // example: {"code":-1021,"msg":"Timestamp for this request is outside of the recvWindow."}
        try
            let json = JsonConvert.DeserializeObject<JObject>(responseContent)
            (json.["code"].ToString(), json.["msg"].ToString())
        with e -> ("???", responseContent)

  
    /// <summary>
    /// Creates a HMACSHA256 Signature based on the key and total parameters
    /// </summary>
    /// <param name="key">The secret key</param>
    /// <param name="totalParams">URL Encoded values that would usually be the query string for the request</param>
    let createHMACSignature (privateKey:string, totalParams:string) =    
        let messageBytes = Encoding.UTF8.GetBytes(totalParams)
        let keyBytes = Encoding.UTF8.GetBytes(privateKey)
        let hash = new HMACSHA256(keyBytes)
        let computedHash = hash.ComputeHash(messageBytes)
        BitConverter.ToString(computedHash).Replace("-", "").ToLower()
    

    interface IClient with

        member this.GetExchangeInfo = 
            let url = f"%s/api/v1/exchangeInfo" baseUrl
            let response = url.GetStringAsync().Result
            response


        member __.GetTicker(pair:CurrencyPair) = 
            
            let cached_ticker = cache.GetTicker pair settings.TickerCacheDuration
            
            match cached_ticker.IsSome with 
            | true -> TickerResponse(true, null, Some(cached_ticker.Value))
            | _ -> 
                //let url = f"%s//api/v3/ticker/bookTicker?symbol=%s" baseUrl (symbol(pair))
                let url = f"%s/api/v1/ticker/24hr?symbol=%s" baseUrl (symbol(pair))
                let ticker_24h = url.AllowHttpStatus("4xx").GetJsonAsync<models.Ticker_24h>().Result
                let response = ticker_24h.ToResponse(pair)
                if response.IsSuccess then cache.SetTicker response.Ticker.Value
                response           


        member __.GetBalance(): BalanceResponse = 
            let url = f"%s/api/v3/account?" baseUrl 

            let parameters = f"timestamp=%i" (get_timestamp())
            let signature = createHMACSignature(settings.SecretKey, parameters)

            let response = (f"%s?%s&signature=%s" url parameters signature)
                            .WithHeader("X-MBX-APIKEY", settings.PublicKey)
                            .AllowAnyHttpStatus().GetAsync().Result

            let jsonContent = response.Content.ReadAsStringAsync().Result

            if not response.IsSuccessStatusCode then
                let (code, error) = parseErrorResponse jsonContent    
                BalanceResponse.Fail error
            else
                parser.parse_account jsonContent
        

        member __.CreateMarketOrder(pair: CurrencyPair, side:OrderSide, quantity: decimal) = 
            
            let url = f"%s/api/v3/order" baseUrl           
            
            let totalParams = f"""symbol=%s&side=%s&type=%s&quantity=%s&timestamp=%i&recvWindow=%i"""
                               (symbol pair) 
                               (if side = OrderSide.Buy then "BUY" else "SELL") 
                               "MARKET" 
                               (quantity.ToString(CultureInfo.InvariantCulture)) 
                               (get_timestamp())
                               recvWindow

            let signature = createHMACSignature(settings.SecretKey, totalParams)
            let requestBody = totalParams + "&signature=" + signature
            
            try
                let response = url.WithHeader("X-MBX-APIKEY", settings.PublicKey)
                                  .WithHeader("Content-Type", "application/x-www-form-urlencoded")
                                  .AllowHttpStatus("4xx")
                                  .PostStringAsync(requestBody)
                                  .Result

                let data = response.Content.ReadAsStringAsync().Result

                if response.IsSuccessStatusCode then                    
                    let apiResponse = JsonConvert.DeserializeObject<models.BinanceOrderFullResponse>(data)
                    apiResponse.ToResponse()
                else 
                    let (code, message) = parseErrorResponse data
                    //match code with 
                    //| -1121 -> message = currencypair not tradable
                    CreateOrderResponse(false, sprintf "%s: [%s] %s" response.ReasonPhrase code message, 0L, 0m)

            with e -> CreateOrderResponse(false, e.Message, 0L, 0m)


        //member this.CreateLimitOrder(pair: CurrencyPair, side:OrderSide, amount: decimal, price: decimal) = 
        //    CreateOrderResponse(false, "not implemented", 0L, 0m)



        member this.Withdraw (currency, address, addressTag, addressDescription, amount) = 
            
            let mutable url = f"%s/wapi/v3/withdraw.html" baseUrl

            let totalParams = 
                sprintf """asset=%s&address=%s&addressTag=%s&amount=%s&name=%s&timestamp=%i&recvWindow=%i""" 
                        (currency.ToString().ToUpper())
                        address
                        (if String.IsNullOrEmpty(addressTag) then "" else (System.Net.WebUtility.UrlEncode(addressTag)))
                        (amount.ToString(CultureInfo.InvariantCulture))  
                        addressDescription
                        (get_timestamp())
                        recvWindow

            let signature = createHMACSignature(settings.SecretKey, totalParams)
            let requestBody = totalParams + "&signature=" + signature

            // https://stackoverflow.com/questions/53177049/https-post-failure-c
            // documentation said POST but it only accept data in the querystring
            url <- f"%s?%s" url requestBody

            try                
                let httpResponse = url.WithHeader("X-MBX-APIKEY", settings.PublicKey)
                                      .WithHeader("Content-Type", "application/x-www-form-urlencoded")
                                      .AllowHttpStatus("4xx")
                                      .PostStringAsync("")  // empty because requestBody is only accepted by querystring
                                      .Result

                let data = httpResponse.Content.ReadAsStringAsync().Result


                // fucking Binance API returns 200 when the request fail for timestamp not synchronized
                // so it makes not possible decide which "model" is returned based on the HTTP status

                if httpResponse.IsSuccessStatusCode then                    
                    let json = JsonConvert.DeserializeObject<JObject>(data)

                    let isSuccess = json.["success"].Value<bool>()

                    if isSuccess then
                        let id =  json.["id"].Value<string>()
                        WithdrawResponse(true, null, id)
                    else 
                        WithdrawResponse(false, json.["msg"].Value<string>(), null)

                else 
                    let (code, message) = parseErrorResponse data
                    WithdrawResponse(false, sprintf "%s: [%s] %s" httpResponse.ReasonPhrase code message, null)

            with e -> WithdrawResponse(false, e.Message, null)

