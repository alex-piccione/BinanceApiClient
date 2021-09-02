namespace Test

open System
open NUnit.Framework
open FsUnit

open Alex75.Cryptocurrencies
open Alex75.BinanceApiClient


[<Category("Client"); Category("REQUIRES_API_KEY")>]
type ClientTest_ListOrders () =

    let client = Client(settings.settings) :> IClient

    [<Test>]
    member this.``List Open Orders`` () =
        let orders = client.ListOpenOrdersOfCurrencies([|
            //CurrencyPair.ADA_BTC
            //CurrencyPair.BTC_EUR
            CurrencyPair("TRX", "XRP")
        |])
        orders |> should not' (be Null)
        
    [<Test>]
    member this.``List Open Orders [when] invalid pairs are passsed`` () =
        let orders = client.ListOpenOrdersOfCurrencies([|
            CurrencyPair("TRX", "XRP")
            CurrencyPair("XTZ", "XRP")
            CurrencyPair("AAA", "BBB")
        |])
        orders |> should not' (be Null)


    [<Test>]
    member this.``List Closed Orders`` () =
        let orders = client.ListClosedOrdersOfCurrencies([|
            //CurrencyPair.ADA_BTC
            //CurrencyPair.BTC_EUR
            CurrencyPair("TRX", "XRP")
        |])
        orders |> should not' (be Null)