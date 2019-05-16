namespace Alex75.BinanceApiClient

open Alex75.Cryptocurrencies


type public Client() =


    interface IClient with

        member __.GetTicker(pair:CurrencyPair)  =
            let bid = 0m
            let ask = 0m
            new Ticker(pair.Main, pair.Other, bid, ask)

        member this.CreateMarketOrder(pair: CurrencyPair, operation:OrderDirection, amount: decimal) = 
            CreateOrderResponse(false, "not implemented")

         member this.CreateLimitOrder(pair: CurrencyPair, operation:OrderDirection, amount: decimal, price: decimal) = 
            CreateOrderResponse(false, "not implemented")