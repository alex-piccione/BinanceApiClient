namespace Alex75.BinanceApiClient

open System
open System.Linq
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

//"STAKING" for Locked Staking, "F_DEFI" for flexible DeFi Staking, "L_DEFI" for locked DeFi Staking
type StackingProduct = | Stacking | FixedDeFi | LockedDefi

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

    let getServerTime () = (f"%s/api/v3/time" baseUrl).GetJsonAsync<ServerTime>().Result.serverTime

    // ref: https://binance-docs.github.io/apidocs/spot/en/#endpoint-security-type
    let recvWindow = 15*1000 // recvWindow cannot exceed 60000. Default: 5000
    
      
    /// <summary>
    /// Creates a HMACSHA256 Signature based on the key and total parameters
    /// </summary>
    /// <param name="privateKey">The secret key</param>
    /// <param name="totalParams">URL Encoded values that would usually be the query string for the request</param>
    let createHMACSignature (privateKey:string, totalParams:string) =
        let messageBytes = Encoding.UTF8.GetBytes(totalParams)
        let keyBytes = Encoding.UTF8.GetBytes(privateKey)
        let hash = new HMACSHA256(keyBytes)
        let computedHash = hash.ComputeHash(messageBytes)
        BitConverter.ToString(computedHash).Replace("-", "").ToLower()

    let httpGet url querystring serverTime =
        let parameters = f"%s&timestamp=%i" querystring serverTime // (getServerTime())
        let signature = createHMACSignature(settings.SecretKey, parameters)

        let response = (f"%s?%s&signature=%s" url parameters signature)
                        .WithHeader("X-MBX-APIKEY", settings.PublicKey)
                        .AllowAnyHttpStatus().GetAsync().Result
        let jsonContent = response.Content.ReadAsStringAsync().Result
        let error = if response.IsSuccessStatusCode then null else parser.parse_error jsonContent
        (response, jsonContent, error)


    let getStacking (stacking:StackingProduct) =
        let url = f"%s/sapi/v1/staking/position" baseUrl 
        
        let product = match stacking with
                      | Stacking -> "STAKING"
                      | FixedDeFi -> "F_DEFI"
                      | LockedDefi -> "L_DEFI"

        let parameters = f"timestamp=%i&product=%s" (getServerTime()) product
        let signature = createHMACSignature(settings.SecretKey, parameters)

        let response = (f"%s?%s&signature=%s" url parameters signature)
                        .WithHeader("X-MBX-APIKEY", settings.PublicKey)
                        .AllowAnyHttpStatus().GetAsync().Result
        let jsonContent = response.Content.ReadAsStringAsync().Result

        parser.parseStackingResponse jsonContent


    interface IClient with

        member this.ListPairs() =
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
                    | "Invalid symbol." -> failwithf "Pair %s is not supported" (pair.ToString())
                    | _ -> failwith ticker_24h.Error

        member this.GetExchangeInfo = 
            let url = f"%s/api/v3/exchangeInfo" baseUrl
            // todo: parsing not implemented yet
            let response = url.GetStringAsync().Result
            response


        member this.GetBalance(): AccountBalance = 
            checkApiKeys()

            let stacking:IDictionary<string, decimal> = getStacking(Stacking)  
            
            let stackingLocked = 
                stacking.Select(fun x -> CurrencyBalance( Currency(x.Key), 0m, 0m).AddStacking(x.Value)).ToArray()
                |> List.ofArray  

            let stackingLockedDefi = 
                getStacking(LockedDefi).Select(fun x -> CurrencyBalance( Currency(x.Key), 0m, 0m).AddStacking(x.Value)).ToArray()
                |> List.ofArray   
            let stackingFixedDefi = 
                getStacking(FixedDeFi).Select(fun x -> CurrencyBalance( Currency(x.Key), 0m, 0m).AddStacking(x.Value)).ToArray()
                |> List.ofArray                

            let addStacking (balance:CurrencyBalance)  = 
                match stacking.TryGetValue balance.Currency.UpperCase with 
                | true, value -> balance.AddStacking value
                | _ -> balance

            match cache.GetAccountBalance balance_cache_time with
            | Some balance -> balance
            | _ -> 
                let url = f"%s/api/v3/account" baseUrl 

                let parameters = f"timestamp=%i" (getServerTime())
                let signature = createHMACSignature(settings.SecretKey, parameters)

                let response = (f"%s?%s&signature=%s" url parameters signature)
                                .WithHeader("X-MBX-APIKEY", settings.PublicKey)
                                .AllowAnyHttpStatus().GetAsync().Result
                let jsonContent = response.Content.ReadAsStringAsync().Result

                if response.IsSuccessStatusCode then 
                    let balances = 
                        parser.parse_account jsonContent                     
                        |> List.map addStacking             

                    let balance = new AccountBalance(balances 
                        |> List.append stackingLocked
                        |> List.append stackingLockedDefi
                        |> List.append stackingFixedDefi)
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
                               (getServerTime())
                               recvWindow

            let signature = createHMACSignature(settings.SecretKey, totalParams)
            let requestBody = totalParams + "&signature=" + signature
            
            let response = url.WithHeader("X-MBX-APIKEY", settings.PublicKey)
                                .WithHeader("Content-Type", "application/x-www-form-urlencoded")
                                .AllowHttpStatus("4xx")
                                .PostStringAsync(requestBody)
                                .Result

            let content = response.Content.ReadAsStringAsync().Result

            if response.IsSuccessStatusCode then
                let orderId, price = parser.ParseCreateOrderResponse(content)
                CreateOrderResult(orderId,price)
            else 
                let error = parser.parse_error content
                //match code with 
                //| -1121 -> message = currencypair not tradable
                failwithf "%s: %s" response.ReasonPhrase error


        member this.CreateLimitOrder(request: CreateOrderRequest): string = 
            raise (System.NotImplementedException())


        member this.ListOpenOrdersIsAvailable = false
        member this.ListOpenOrders() = 
            raise (System.NotImplementedException())

        member this.ListOpenOrdersOfCurrenciesIsAvailable = true
        member this.ListOpenOrdersOfCurrencies (pairs: CurrencyPair[]): OpenOrder[] = 
            checkApiKeys()

            // todo: purge from invalid pairs
            let validPairs = 
                (this :> IClient).ListPairs().ToArray()
                |> Array.filter (fun pair -> (pairs |> Array.contains pair))


            // The API allows to not specify the symbol but the call costs 40 times the single symbol call

            let orders = System.Collections.Concurrent.ConcurrentBag()
            let serverTime = getServerTime()  // it's ok to use the same ?
            let getOrders pair =
                let (response, jsonString, error) = httpGet (f"%s/api/v3/openOrders" baseUrl) (f"symbol=%s" (getSymbol pair)) serverTime
                if response.IsSuccessStatusCode then 
                    orders.Add(parser.ParseOpenOrders pair jsonString)
                else failwithf "Failed to retrieve orders for \"%O\". %s" pair error

            Parallel.ForEach(validPairs, getOrders) |> ignore
            orders.ToArray() |> Array.fold Array.append Array.empty<OpenOrder>


        member this.ListClosedOrdersIsAvailable = false
        member this.ListClosedOrders() = raise (System.NotImplementedException())

        member this.ListClosedOrdersOfCurrenciesIsAvailable = true
        member this.ListClosedOrdersOfCurrencies(pairs: CurrencyPair[]): ClosedOrder[] = 
            checkApiKeys()

            // todo: remove invalid pairs
            let validPairs = 
                (this :> IClient).ListPairs().ToArray()
                |> Array.filter (fun pair -> (pairs |> Array.contains pair))

            let limit = 100
            let orders = System.Collections.Concurrent.ConcurrentBag()
            let serverTime = getServerTime()  // it's ok to use the same ?
            let getOrders pair = 
                let querystring = f"symbol=%s&limit=%i" (getSymbol pair) limit
                let (response, jsonString, error) = httpGet (f"%s/api/v3/allOrders" baseUrl) querystring serverTime
                if response.IsSuccessStatusCode then 
                    orders.Add(parser.ParseClosedOrders pair jsonString)
                else failwithf "Failed to retrieve orders for \"%O\". %s" pair error

            Parallel.ForEach(validPairs, getOrders) |> ignore
            orders.ToArray() |> Array.fold Array.append Array.empty<ClosedOrder>


        member this.ListWithdralsIsAvailable = false
        member this.ListWithdrals(sinceWhen: DateTime): Withdrawal[] =



            raise (System.NotImplementedException())

        member this.ListWithdralsOfCurrenciesIsAvailable = false
        member this.ListWithdralsOfCurrencies(sinceWhen: DateTime, pairs: CurrencyPair []): Withdrawal [] = 
            raise (System.NotImplementedException())


        member this.Withdraw(wallet: Wallet, amount: float) = 
             checkApiKeys()
             // doc: https://binance-docs.github.io/apidocs/spot/en/#withdraw-user_data

             let mutable url = $"{baseUrl}/sapi/v1/capital/withdraw/apply" 

             let totalParams = 
                sprintf """coin=%s&address=%s&addressTag=%s&amount=%s&transactionFeeFlag=false&timestamp=%i&recvWindow=%i""" 
                    wallet.Currency.UpperCase
                    wallet.Address
                    (System.Net.WebUtility.UrlEncode(wallet.IdentifierText))
                    (amount.ToString(CultureInfo.InvariantCulture))
                    (getServerTime())
                    recvWindow

             let signature = createHMACSignature(settings.SecretKey, totalParams)
             let requestBody = totalParams + "&signature=" + signature

             // https://stackoverflow.com/questions/53177049/https-post-failure-c
             // documentation said POST but it only accept data in the querystring
             url <- f"%s?%s" url requestBody
            
             let httpResponse = url.WithHeader("X-MBX-APIKEY", settings.PublicKey)
                                     .WithHeader("Content-Type", "application/x-www-form-urlencoded")
                                     .AllowHttpStatus("4xx")
                                     .PostStringAsync("")  // empty because requestBody is only accepted by querystring
                                     .Result

             let content = httpResponse.Content.ReadAsStringAsync().Result


             // Binance API returns 200 when the request fails for timestamp not synchronized
             // or for permission denied...
             // so it makes not possible to decide which "model" is returned based on the HTTP status

             if httpResponse.IsSuccessStatusCode then
                 let json = JsonConvert.DeserializeObject<JObject>(content)
                 if json.ContainsKey("msg") then
                     failwith (json.["msg"].Value<string>())
                 else 
                    json.["id"].Value<string>()
             else 
                 let error = parser.parse_error content
                 failwith error