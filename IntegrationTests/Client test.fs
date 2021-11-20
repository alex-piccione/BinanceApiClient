namespace Test

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

    [<Test>]
    member this.``GetExchangeInfo`` () =

        let info = client.GetExchangeInfo

        info |> should not' (be null)

    [<Test>]
    member this.``GetTicker`` () =
        let pair = CurrencyPair("XRP", "btc")
        let ticker = client.GetTicker(pair)
        ticker |> should not' (be null)


    [<Test>]
    member this.``GetTicker [when] pair does not exist [should] raise an error`` () =
        let invalid_pair = CurrencyPair("XRP", "usd")  
        (fun () -> client.GetTicker(invalid_pair) |> ignore) 
        |> should throw typeof<Exception>


    [<Test; Category("REQUIRES_API_KEY")>]
    member this.``Get Balance`` () =
        settings.readSettings() |> ignore
        let response = client.GetBalance()
        response |> should not' (be null)
        //if not response.IsSuccess then failwith response.Error
        //response.Assets |> should not' (be Empty)


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