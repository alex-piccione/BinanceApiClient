namespace Alex75.BinanceApiClient

open System
open Flurl.Http
open Alex75.Cryptocurrencies
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
                let response = ticker_24h.ToResponse(pair)
                if response.IsSuccess then cache.setTicker pair response.Ticker.Value
                response
            

        member this.CreateMarketOrder(pair: CurrencyPair, side:OrderSide, amount: decimal) = 
            CreateOrderResponse(false, "not implemented")

        member this.CreateLimitOrder(pair: CurrencyPair, side:OrderSide, amount: decimal, price: decimal) = 
            CreateOrderResponse(false, "not implemented")