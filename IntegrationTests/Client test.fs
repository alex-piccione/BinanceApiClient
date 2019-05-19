namespace Test

open System
open NUnit.Framework
open FsUnit

open Alex75.Cryptocurrencies
open Alex75.BinanceApiClient


[<Category("Client")>]
type ClientTest () =

    let settings = { TickerCacheDuration=TimeSpan.FromSeconds(20.); PublicKey=""; SecretKey=""}
    let client = Client(settings) :> IClient

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
