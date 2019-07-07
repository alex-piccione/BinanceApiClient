﻿namespace Alex75.BinanceApiClient

open System
open Flurl.Http
open Alex75.Cryptocurrencies
open models
open System.Text
open System.Security.Cryptography
open System.Globalization
open Newtonsoft.Json
open System.Net.Http
open System.Xml.Linq
open Newtonsoft.Json.Linq




type public Settings = { TickerCacheDuration:TimeSpan; PublicKey:string; SecretKey:string }

type CreateOrderPayload = { symbol:string; side:string; ``type``:string; quantity:decimal; timestamp:Int64}


type public Client(settings:Settings) =

    let baseUrl = "https://api.binance.com"
    let f = sprintf
    let symbol (pair:CurrencyPair) = f"%O%O" pair.Main pair.Other

    let get_timestamp = 
        #if NETSTANDARD2_0
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        #else
        DateTime.UtcNow.ConvertToUnixTime();
        #endif


    
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
    /// <returns></returns>
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
            
            let cached_ticker = cache.getTicker pair settings.TickerCacheDuration
            
            match cached_ticker.IsSome with 
            | true -> TickerResponse(true, null, Some(cached_ticker.Value))
            | _ -> 
                //let url = f"%s//api/v3/ticker/bookTicker?symbol=%s" baseUrl (symbol(pair))
                let url = f"%s/api/v1/ticker/24hr?symbol=%s" baseUrl (symbol(pair))
                let ticker_24h = url.AllowHttpStatus("4xx").GetJsonAsync<models.Ticker_24h>().Result
                let response = ticker_24h.ToResponse(pair)
                if response.IsSuccess then cache.setTicker pair response.Ticker.Value
                response           

        

        member __.CreateMarketOrder(pair: CurrencyPair, side:OrderSide, quantity: decimal) = 
            
            let url = f"%s/api/v3/order" baseUrl           
            
            //let parameters = {
            //    symbol = symbol pair
            //    side = if side = OrderSide.Buy then "BUY" else "SELL"
            //    ``type`` = "MARKET"
            //    quantity = quantity
            //    timestamp = timestamp
            //}

            let totalParams = f"""symbol=%s&side=%s&type=%s&quantity=%s&timestamp=%i&recvWindow=%i"""
                               (symbol pair) 
                               (if side = OrderSide.Buy then "BUY" else "SELL") 
                               "MARKET" 
                               (quantity.ToString(CultureInfo.InvariantCulture)) 
                               get_timestamp
                               (10*1000)

            let signature = createHMACSignature(settings.SecretKey, totalParams)
            let requestBody = totalParams + "&signature=" + signature
            
            try
                let response = url.WithHeader("X-MBX-APIKEY", settings.PublicKey)
                                  .WithHeader("Content-Type", "application/x-www-form-urlencoded")
                                  .AllowHttpStatus("4xx")
                                  .PostAsync(new StringContent(requestBody))  
                                  //.PostStringAsync(requestBody)
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


        member this.CreateLimitOrder(pair: CurrencyPair, side:OrderSide, amount: decimal, price: decimal) = 
            CreateOrderResponse(false, "not implemented", 0L, 0m)



        member this.Withdraw (currency, address, addressTag, addressDescription, amount) = 
            
            let mutable url = f"%s/wapi/v3/withdraw.html" baseUrl

            let totalParams = 
                sprintf """asset=%s&address=%s&addressTag=%s&amount=%s&recvWindow=%i&name=%s&timestamp=%i""" 
                        (currency.ToString().ToUpper())
                        address
                        (if String.IsNullOrEmpty(addressTag) then "" else (System.Net.WebUtility.UrlEncode(addressTag)))
                        (amount.ToString(CultureInfo.InvariantCulture))                        
                        (10*1000) // 10 seconds
                        addressDescription
                        get_timestamp

            let signature = createHMACSignature(settings.SecretKey, totalParams)
            let requestBody = totalParams + "&signature=" + signature


            // https://stackoverflow.com/questions/53177049/https-post-failure-c
            // documentation said POST but it only accept data in the querystring

            url <- f"%s?%s" url requestBody

            try                
                let response = url.WithHeader("X-MBX-APIKEY", settings.PublicKey)
                                  .WithHeader("Content-Type", "application/x-www-form-urlencoded")
                                  .AllowHttpStatus("4xx")
                                  .PostAsync(new StringContent(""))  // empty
                                  //.PostStringAsync(requestBody)
                                  .Result

                let data = response.Content.ReadAsStringAsync().Result

                if response.IsSuccessStatusCode then                    
                    let json = JsonConvert.DeserializeObject<JObject>(data)
                    let id = if json.["id"] = null then null else json.["id"].ToString()
                    WithdrawResponse(json.["msg"].ToString(), json.["success"].Value<bool>(), id)
                else 
                    let (code, message) = parseErrorResponse data
                    WithdrawResponse(sprintf "%s: [%s] %s" response.ReasonPhrase code message, false, null)

            with e -> WithdrawResponse(e.Message, false, null)

