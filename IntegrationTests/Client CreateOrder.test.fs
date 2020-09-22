namespace Test

open System
open NUnit.Framework
open FsUnit

open Alex75.Cryptocurrencies
open Alex75.BinanceApiClient


[<Category("Client"); Category("AFFECTS_BALANCE")>]
type ClientTest_Order () =

    let client = Client(settings.settings) :> IClient

    [<Test>]
    member this.``CreateMarketOrder Buy 50 XRP with BTC`` () =
        let pair = CurrencyPair("XRP", "btc")
        let result = client.CreateMarketOrder( CreateOrderRequest.Market(OrderSide.Buy, pair, 50m))

        result.Reference |> should not' (be NullOrEmptyString)
        result.Price |> should not' (be Null)


    [<Test>]
    member this.``CreateMarketOrder Sell 20 XRP for BTC`` () =
        //let pair = CurrencyPair("btc", "xrp") invalid symbol
        let pair = CurrencyPair("xrp", "btc")
        //let price = 0.00003016m // XRP/BTC
        //let amount = price * 20m // pay 20 XRP
        let amount = 20m  // XRP
        let result = client.CreateMarketOrder(CreateOrderRequest.Market(OrderSide.Sell, pair, amount))

        result.Reference |> should not' (be NullOrEmptyString)
        result.Price |> should not' (be Null)


    [<Test>]
    member this.``CreateMarketOrder Buy 30 XRP with EUR`` () =
        let pair = CurrencyPair("xrp", "eur")
        let amount = 30m  // XRP
        let result = client.CreateMarketOrder(CreateOrderRequest.Market(OrderSide.Buy, pair, amount))

        // with 25 {"code":-1013,"msg":"Filter failure: MIN_NOTIONAL"}

        result.Reference |> should not' (be NullOrEmptyString)
        result.Price |> should not' (be Null)


    [<Test>]
    member this.``CreateMarketOrder Sell 80 XRP for EUR`` () =
        let pair = CurrencyPair("xrp", "eur")
        let amount = 80m  // XRP
        let result = client.CreateMarketOrder(CreateOrderRequest.Market(OrderSide.Sell, pair, amount))

        // eror with 50 (80 was ok)
        // with 25 {"code":-1013,"msg":"Filter failure: MIN_NOTIONAL"}

        result.Reference |> should not' (be NullOrEmptyString)
        result.Price |> should not' (be Null)
