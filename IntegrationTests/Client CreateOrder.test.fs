namespace Test

open System
open NUnit.Framework
open FsUnit

open Alex75.Cryptocurrencies
open Alex75.BinanceApiClient


[<Category("Client")>]
type ClientTest_Order () =

    let client = Client(settings.settings) :> IClient

    [<Test; Category("AFFECTS_BALANCE")>]
    member this.``CreateMarketOrder Buy 50 XRP with BTC`` () =
        let pair = CurrencyPair("XRP", "btc")
        let result = client.CreateMarketOrder( CreateOrderRequest.Market(OrderSide.Buy, pair, 50m))

        result.Reference |> should not' (be NullOrEmptyString)
        result.Price |> should not' (be Null)


    [<Test; Category("AFFECTS_BALANCE")>]
    member this.``CreateMarketOrder Sell 20 XRP for BTC`` () =
        //let pair = CurrencyPair("btc", "xrp") invalid symbol
        let pair = CurrencyPair("xrp", "btc")
        //let price = 0.00003016m // XRP/BTC
        //let amount = price * 20m // pay 20 XRP
        let amount = 20m  // XRP
        let result = client.CreateMarketOrder(CreateOrderRequest.Market(OrderSide.Sell, pair, amount))

        result.Reference |> should not' (be NullOrEmptyString)
        result.Price |> should not' (be Null)


    [<Test; Category("AFFECTS_BALANCE")>]
    member this.``CreateMarketOrder Buy 30 XRP with EUR`` () =
        let pair = CurrencyPair("xrp", "eur")
        let amount = 30m  // XRP
        let result = client.CreateMarketOrder(CreateOrderRequest.Market(OrderSide.Buy, pair, amount))

        // with 25 {"code":-1013,"msg":"Filter failure: MIN_NOTIONAL"}

        result.Reference |> should not' (be NullOrEmptyString)
        result.Price |> should not' (be Null)


    [<Test; Category("AFFECTS_BALANCE")>]
    member this.``CreateMarketOrder Sell 80 XRP for EUR`` () =
        let pair = CurrencyPair("xrp", "eur")
        let amount = 80m  // XRP
        let result = client.CreateMarketOrder(CreateOrderRequest.Market(OrderSide.Sell, pair, amount))

        // eror with 50 (80 was ok)
        // with 25 {"code":-1013,"msg":"Filter failure: MIN_NOTIONAL"}

        result.Reference |> should not' (be NullOrEmptyString)
        result.Price |> should not' (be Null)


    //[<Test; Category("AFFECTS_BALANCE")>]
    //member __.``CreateLimitOrder`` () =
    //    let pair = CurrencyPair("XRP", "eur")
    //    let response = client.CreateLimitOrder(pair, OrderSide.Buy, 50m, 0.50m)


    //    response |> should not' (be null)  
    //    if not response.IsSuccess then failwith response.Error

    //    response.IsSuccess |> should equal true 
        


