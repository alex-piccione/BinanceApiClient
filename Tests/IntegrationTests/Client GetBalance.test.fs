namespace IntegrationTests

open NUnit.Framework
open FsUnit
open Alex75.Cryptocurrencies
open Alex75.BinanceApiClient

[<Category("Client"); Category("AFFECTS_BALANCE")>]
type GetBalance () =

    let client = Client(settings.settings) :> IApiClientPrivate

    [<Test>]
    member this.``GetBalance`` () =
        let pair = CurrencyPair("XRP", "eur")
        let balance = client.GetBalance()

        balance |> should not' (be null)
        // assumes XRP balance is not zero
        balance.HasCurrency(Currency("xrp")) |> should be True
