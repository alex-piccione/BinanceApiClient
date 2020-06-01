module Client_GetTicker_test

open System
open NUnit.Framework
open FsUnit

open Alex75.Cryptocurrencies
open Alex75.BinanceApiClient

[<Category("Client")>]
type ClientTest () =

    let client = Client(settings.settings) :> IApiClientPrivate


    [<Test>]
    member __.``GetTicker`` () =
        let pair = CurrencyPair("XRP", "eur")
        let ticker = client.GetTicker(pair)

        ticker |> should not' (be null)
        ticker.Bid |> should not' (equal 0m)
        ticker.Ask |> should not' (equal 0m)
        ticker.Low.IsSome |> should be True
        ticker.High.IsSome |> should be True
        ticker.Last.IsSome |> should be True


    [<Test>]
    member __.``GetTicker [when] pair do not exists [should] raise an error`` () =
        let pair = CurrencyPair("XRP", "NNN")
        (fun () -> client.GetTicker(pair) |> ignore) |> should throw (typeof<UnsupportedPair>)