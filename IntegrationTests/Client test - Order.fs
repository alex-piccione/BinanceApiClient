namespace Test

open System
open NUnit.Framework
open FsUnit

open Alex75.Cryptocurrencies
open Alex75.BinanceApiClient


[<Category("Client")>]
type ClientTest_Order () =

    let client = Client(settings.settings) :> IClient

    [<Test; Category("AFFECT_BALANCE")>]
    member __.``CreateMarketOrder Buy 50 XRP with BTC`` () =
        let pair = CurrencyPair("XRP", "btc")
        let response = client.CreateMarketOrder(pair, OrderSide.Buy, 50m)

        response |> should not' (be null)
        if not response.IsSuccess then failwith response.Error

        response.IsSuccess |> should equal true
        response.Id |> should be (greaterThan 0)
        response.Price |> should be (greaterThan 0)

    //[<Test; Category("AFFECT_BALANCE")>]
    //member __.``CreateMarketOrder Buy BTC with 20 XRP`` () =

    // the pair BTC/XRP does not exists, this order must be executed on the other way (Sell XRP for BTC)

        //let pair = CurrencyPair("btc", "xrp") invalid symbol
        //let pair = CurrencyPair("xrp", "btc")
        //let price = 0.00003016m // XRP/BTC
        //let amount = price * 20m // pay 20 XRP
        //let response = client.CreateMarketOrder(pair, OrderSide.Sell, amount)

        //response |> should not' (be null)
        //if not response.IsSuccess then failwith response.Error

        //response.IsSuccess |> should equal true
        //response.Id |> should be (greaterThan 0)
        //response.Price |> should be (greaterThan 0)

    [<Test; Category("AFFECT_BALANCE")>]
    member __.``CreateMarketOrder Sell 20 XRP for BTC`` () =
        //let pair = CurrencyPair("btc", "xrp") invalid symbol
        let pair = CurrencyPair("xrp", "btc")
        //let price = 0.00003016m // XRP/BTC
        //let amount = price * 20m // pay 20 XRP
        let amount = 20m  // XRP
        let response = client.CreateMarketOrder(pair, OrderSide.Sell, amount)

        response |> should not' (be null)
        if not response.IsSuccess then failwith response.Error

        response.IsSuccess |> should equal true
        response.Id |> should be (greaterThan 0)
        //response.Price |> should be (greaterThan 0) it returns "0"


    //[<Test; Category("AFFECT_BALANCE")>]
    //member __.``CreateLimitOrder`` () =
    //    let pair = CurrencyPair("XRP", "eur")
    //    let response = client.CreateLimitOrder(pair, OrderSide.Buy, 50m, 0.50m)


    //    response |> should not' (be null)  
    //    if not response.IsSuccess then failwith response.Error

    //    response.IsSuccess |> should equal true 
        


