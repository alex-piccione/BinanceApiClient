namespace Alex75.BinanceApiClient

open System
open System.Text
open System.Threading.Tasks
open System.Collections.Generic
open System.Security.Cryptography
open System.Globalization

open Newtonsoft.Json; 
open Newtonsoft.Json.Linq
open Flurl.Http
open Alex75.Cryptocurrencies
open models



type public Settings = { TickerCacheDuration:TimeSpan; PublicKey:string; SecretKey:string; }

type CreateOrderPayload = { symbol:string; side:string; ``type``:string; quantity:decimal; timestamp:Int64}


type public Client(settings:Settings) =

    let baseUrl = "https://api.binance.com"
    let f = sprintf
   
    let cache = Cache()
    let assets_cache_time = TimeSpan.FromHours 6.0
    let balance_cache_time = TimeSpan.FromSeconds 30.0

    let checkApiKeys () =
        if String.IsNullOrEmpty settings.PublicKey || String.IsNullOrEmpty settings.SecretKey 
        then failwithf "Private methods requires API keys to be set"        

    let getSymbol (pair:CurrencyPair) = f"%O%O" pair.Main pair.Other    

    let get_timestamp () = (f"%s/api/v3/time" baseUrl).GetJsonAsync<ServerTime>().Result.serverTime
    // ref: https://binance-docs.github.io/apidocs/spot/en/#endpoint-security-type
    let recvWindow = 15*1000 // recvWindow cannot exceed 60000. Default: 5000
    
      
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

    let httpGet url querystring =
        let parameters = f"%s&timestamp=%i" querystring (get_timestamp())
        let signature = createHMACSignature(settings.SecretKey, parameters)

        let response = (f"%s?%s&signature=%s" url parameters signature)
                        .WithHeader("X-MBX-APIKEY", settings.PublicKey)
                        .AllowAnyHttpStatus().GetAsync().Result
        let jsonContent = response.Content.ReadAsStringAsync().Result
        let error = if response.IsSuccessStatusCode then null else parser.parse_error jsonContent
        (response, jsonContent, error)
    

    interface IClient with

        member this.ListPairs()  = 
            match cache.GetPairs assets_cache_time with
            | Some pairs -> pairs
            | _ -> 
                let pairs = parser.parse_pairs ( (f"%s/api/v3/exchangeInfo" baseUrl).GetStringAsync().Result )
                cache.SetPairs pairs
                pairs :> ICollection<CurrencyPair>   

        member this.GetTicker(pair: CurrencyPair): Ticker = 
            match cache.GetTicker pair settings.TickerCacheDuration with 
            | Some ticker -> ticker
            | _ ->         
                let url = f"%s/api/v3/ticker/24hr?symbol=%s" baseUrl (getSymbol pair)
                let ticker_24h = url.AllowHttpStatus("4xx").GetJsonAsync<models.Ticker_24h>().Result

                if ticker_24h.IsSuccess 
                then
                    let ticker = Ticker(pair, ticker_24h.BidPrice, ticker_24h.AskPrice, Some ticker_24h.LowPrice, Some ticker_24h.HighPrice, Some ticker_24h.LastPrice)
                    cache.SetTicker ticker     
                    ticker
                else
                    match ticker_24h.Error with 
                    | "Invalid symbol." -> raise (UnsupportedPair(pair))
                    | _ -> failwith ticker_24h.Error                          

        member this.GetExchangeInfo = 
            let url = f"%s/api/v3/exchangeInfo" baseUrl
            // todo: parsing not implemented yet
            let response = url.GetStringAsync().Result
            response



        member this.GetBalance(): AccountBalance = 
            checkApiKeys()
            match cache.GetAccountBalance balance_cache_time with
            | Some balance -> balance
            | _ -> 
                let url = f"%s/api/v3/account?" baseUrl 

                let parameters = f"timestamp=%i" (get_timestamp())
                let signature = createHMACSignature(settings.SecretKey, parameters)

                let response = (f"%s?%s&signature=%s" url parameters signature)
                                .WithHeader("X-MBX-APIKEY", settings.PublicKey)
                                .AllowAnyHttpStatus().GetAsync().Result
                let jsonContent = response.Content.ReadAsStringAsync().Result

                if response.IsSuccessStatusCode then 
                    let balance = parser.parse_account jsonContent
                    cache.SetAccountBalance balance
                    balance
                else failwith (parser.parse_error jsonContent)
                  

        member this.CreateMarketOrder(request: CreateOrderRequest): CreateOrderResult = 
            checkApiKeys()
            let url = f"%s/api/v3/order" baseUrl           
            
            let totalParams = f"""symbol=%s&side=%s&type=%s&quantity=%s&timestamp=%i&recvWindow=%i"""
                               (getSymbol request.Pair) 
                               (if request.Side = OrderSide.Buy then "BUY" else "SELL") 
                               "MARKET" 
                               (request.BuyOrSellQuantity.ToString(CultureInfo.InvariantCulture)) 
                               (get_timestamp())
                               recvWindow

            let signature = createHMACSignature(settings.SecretKey, totalParams)
            let requestBody = totalParams + "&signature=" + signature
            
            //try
            let response = url.WithHeader("X-MBX-APIKEY", settings.PublicKey)
                                .WithHeader("Content-Type", "application/x-www-form-urlencoded")
                                .AllowHttpStatus("4xx")
                                .PostStringAsync(requestBody)
                                .Result

            let content = response.Content.ReadAsStringAsync().Result

            if response.IsSuccessStatusCode then      
                let orderId, price = parser.ParseCreateOrderResponse(content)
                { reference=orderId; price=price}
            else 
                let error = parser.parse_error content
                //match code with 
                //| -1121 -> message = currencypair not tradable
                failwithf "%s: %s" response.ReasonPhrase error


        member this.CreateLimitOrder(request: CreateOrderRequest): string = 
            raise (System.NotImplementedException())



        member this.ListOpenOrdersIsAvailable(): bool = false
        member this.ListOpenOrders(): ICollection<OpenOrder> = 
            raise (System.NotImplementedException())

        member this.ListOpenOrders_2(pairs: CurrencyPair[]): OpenOrder[] = 
            checkApiKeys()

            // The API allows to not specify the symbol but the call costs 40 times the single symbol call

            let orders = System.Collections.Concurrent.ConcurrentBag()
            let getOrders pair =
                let (response, jsonString, error) = httpGet (f"%s/api/v3/openOrders" baseUrl) (f"symbol=%s" (getSymbol pair))
                if response.IsSuccessStatusCode then 
                    orders.Add(parser.ParseOpenOrders pair jsonString)
                else failwithf "Failed to retrieve orders for \"%O\". %s" pair error

            Parallel.ForEach(pairs, getOrders) |> ignore
            orders.ToArray() |> Array.fold Array.append Array.empty<OpenOrder>



        member this.Withdraw (currency, address, addressTag, addressDescription, amount) = 
            checkApiKeys()
            let mutable url = f"%s/wapi/v3/withdraw.html" baseUrl

            let mutable normalizedAddressTag = addressTag
            if currency = Currency.XRP && addressTag = "0" then normalizedAddressTag <- ""

            let totalParams = 
                sprintf """asset=%s&address=%s&addressTag=%s&amount=%s&name=%s&timestamp=%i&recvWindow=%i""" 
                        (currency.ToString().ToUpper())
                        address
                        (if String.IsNullOrEmpty(addressTag) then "" else (System.Net.WebUtility.UrlEncode(normalizedAddressTag)))
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

                let content = httpResponse.Content.ReadAsStringAsync().Result


                // fucking Binance API returns 200 when the request fails for timestamp not synchronized
                // or for permission denied...
                // so it makes not possible decide which "model" is returned based on the HTTP status

                if httpResponse.IsSuccessStatusCode then                    
                    let json = JsonConvert.DeserializeObject<JObject>(content)
                    
                    let isSuccess = json.ContainsKey("success") && json.["success"].Value<bool>()

                    if isSuccess then
                        let id =  json.["id"].Value<string>()
                        WithdrawResponse(true, null, id)
                    else 
                        let message = if json.ContainsKey("msg") then json.["msg"].Value<string>() else json.ToString()
                        WithdrawResponse(false, message, null)

                else 
                    let error = parser.parse_error content
                    WithdrawResponse(false, sprintf "%s: %s" httpResponse.ReasonPhrase error, null)

            with e -> WithdrawResponse(false, e.Message, null)

