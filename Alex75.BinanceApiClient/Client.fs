namespace Alex75.BinanceApiClient

open Alex75.Cryptocurrencies
open Flurl.Http
open System
open models


type public Settings = { TickerCacheDuration:TimeSpan; PublicKey:string; SecretKey:string }


type public Client(settings:Settings) =

    let baseUrl = "https://api.binance.com"
    let f = sprintf
    let symbol (pair:CurrencyPair) = f"%O%O" pair.Main pair.Other


    interface IClient with

        member __.GetTicker(pair:CurrencyPair)  = 
            
            let cached_ticker = cache.getTicker pair settings.TickerCacheDuration
            
            match cached_ticker.IsSome with 
            | true -> TickerResponse(true, null, Some(cached_ticker.Value))
            | _ -> 
                //let url = f"%s//api/v3/ticker/bookTicker?symbol=%s" baseUrl (symbol(pair))
                let url = f"%s/api/v1/ticker/24hr?symbol=%s" baseUrl (symbol(pair))
                let ticker_24h = url.AllowHttpStatus("4xx").GetJsonAsync<models.Ticker_24h>().Result
                ticker_24h.ToResponse(pair)
            

        member this.CreateMarketOrder(pair: CurrencyPair, operation:OrderDirection, amount: decimal) = 
            CreateOrderResponse(false, "not implemented")

         member this.CreateLimitOrder(pair: CurrencyPair, operation:OrderDirection, amount: decimal, price: decimal) = 
            CreateOrderResponse(false, "not implemented")