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


    [<Test; Category("SKIP_ON_DEPLOY"); Category("AFFECTS_BALANCE")>]
    member this.``Withdraw XRP`` () =
        
        settings.readSettings() |> ignore
        let address = settings.withdrawalAddress
        let addressTag = null        
        
        // minimum withdrawal = 50 (07/07/2019)
        let response = client.Withdraw(Currency.XRP, address, addressTag, "test", 25m)

        response |> should not' (be null)
        if not response.IsSuccess then failwith response.Error
        
        response.IsSuccess |> should be True
        response.OperationId |> should not' (be NullOrEmptyString)


    [<Test; Category("SKIP_ON_DEPLOY"); Category("AFFECTS_BALANCE")>]
    member this.``Withdraw XRP [when] destimation tag is zero`` () =

        settings.readSettings() |> ignore
        let address = settings.withdrawalAddress
        let addressTag = "0"

        let response = client.Withdraw(Currency.XRP, address, addressTag, "test", 25m)

        response |> should not' (be null)
        if not response.IsSuccess then failwith response.Error