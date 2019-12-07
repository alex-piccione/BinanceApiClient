namespace Test

open System
open NUnit.Framework
open FsUnit

open Alex75.Cryptocurrencies
open Alex75.BinanceApiClient


[<Category("Client")>]
type ClientTest () =

    let client = Client(settings.settings) :> IClient

    [<Test>]
    member __.``GetExchangeInfo`` () =

        let info = client.GetExchangeInfo

        info |> should not' (be null)

    [<Test>]
    member __.``GetTicker `` () =
        let pair = CurrencyPair("XRP", "btc")
        let ticker = client.GetTicker(pair)

        ticker |> should not' (be null)

        ticker.IsSuccess |> should equal true
        ticker.Error |> should be null
        ticker.Ticker.IsSome |> should equal true
        ticker.Ticker.Value.Bid |> should not' (equal 0m)
        ticker.Ticker.Value.Ask |> should not' (equal 0m)


    [<Test>]
    member __.``GetTicker [when] pair do not exists`` () =
        let pair = CurrencyPair("XRP", "usd")
        let ticker = client.GetTicker(pair)

        ticker |> should not' (be null)

        ticker.IsSuccess |> should be False
        ticker.Error |> should not' (be null)
        ticker.Ticker.IsSome |> should equal false


    [<Test; Category("AFFECT_BALANCE")>]
    member __.``Withdraw XRP`` () =
        
        settings.readSettings() |> ignore
        let address = settings.withdrawalAddress
        let addressTag = null
        
        
        // minimum withdrawal = 50 (07/07/2019)
        let response = client.Withdraw(Currency.XRP, address, addressTag, "test", 25m)

        response |> should not' (be null)
        if not response.IsSuccess then failwith response.Error

        
        response.IsSuccess |> should be True
        response.OperationId |> should not' (be NullOrEmptyString)


