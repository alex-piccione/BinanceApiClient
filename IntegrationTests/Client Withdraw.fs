namespace Tests.Client.Withdraw

open System
open NUnit.Framework
open FsUnit

open Alex75.Cryptocurrencies
open Alex75.BinanceApiClient

[<Category("Client")>]
type ClientTest () =

    let client = Client(settings.settings) :> IClient
    // minimum withdrawal = 50 (07/07/2019)
    let XRP_MIN_WITHDRAWAL = 25.0

    [<Test; Category("AFFECTS_BALANCE")>]
    member this.``Withdraw XRP`` () =

        settings.readSettings() |> ignore
        let wallet = XrpWallet(settings.withdrawalAddress)

        let response = client.Withdraw(wallet, XRP_MIN_WITHDRAWAL)
        response |> should not' (be NullOrEmptyString)

    [<Test; Category("AFFECTS_BALANCE")>]
    member this.``Withdraw XRP [when] has Destination Tag`` () =

        settings.readSettings() |> ignore
        let wallet = XrpWallet(settings.withdrawalAddress, 100)

        let response = client.Withdraw(wallet, XRP_MIN_WITHDRAWAL)
        response |> should not' (be NullOrEmptyString)


    [<Test; Category("AFFECTS_BALANCE")>]
    member this.``Withdraw XRP [when] destination tag is zero`` () =
        // tag "0" returns the error "Address verification failed (code: -4007)"
        settings.readSettings() |> ignore
        let wallet = XrpWallet(settings.withdrawalAddress, 0)

        let response = client.Withdraw(wallet, XRP_MIN_WITHDRAWAL)
        response |> should not' (be NullOrEmptyString)