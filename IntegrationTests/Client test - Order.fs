namespace Test

open System
open NUnit.Framework
open FsUnit

open Alex75.Cryptocurrencies
open Alex75.BinanceApiClient


[<Category("Client")>]
type ClientTest_Order () =

    let settings = settings.readSettings
    let client = Client(settings) :> IClient

    [<Test>]
    member __.``CreateMarketOrder`` () =
        let pair = CurrencyPair("XRP", "btc")
        let response = client.CreateMarketOrder(pair, OrderSide.Buy, 50m)

        response |> should not' (be null)
        if not response.IsSuccess then failwith response.Error

        response.IsSuccess |> should equal true
        response.Id |> should be (greaterThan 0)
        response.Price |> should be (greaterThan 0)


    [<Test>]
    member __.``CreateLimitOrder`` () =
        let pair = CurrencyPair("XRP", "eur")
        let response = client.CreateLimitOrder(pair, OrderSide.Buy, 50m, 0.50m)


        response |> should not' (be null)  
        if not response.IsSuccess then failwith response.Error

        response.IsSuccess |> should equal true 
        


