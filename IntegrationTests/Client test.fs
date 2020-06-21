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
    member __.``GetTicker`` () =
        let pair = CurrencyPair("XRP", "btc")
        let ticker = client.GetTicker(pair)
        ticker |> should not' (be null)


    [<Test>]
    member __.``GetTicker [when] pair do not exists [should] raise an error`` () =
        let invalid_pair = CurrencyPair("XRP", "usd")  
        (fun () -> client.GetTicker(invalid_pair) |> ignore) 
        |> should throw typeof<UnsupportedPair>


    [<Test; Category("REQUIRES_API_KEY")>]
    member __.``Get Balance`` () =        
        settings.readSettings() |> ignore       
        let response = client.GetBalance()
        response |> should not' (be null)
        //if not response.IsSuccess then failwith response.Error        
        //response.Assets |> should not' (be Empty)


    [<Test; Category("SKIP_ON_DEPLOY"); Category("AFFECTS_BALANCE")>]
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


    [<Test; Category("SKIP_ON_DEPLOY"); Category("AFFECTS_BALANCE")>]
    member __.``Withdraw XRP [when] destimation tag is zero`` () =

        settings.readSettings() |> ignore
        let address = "rGU5P1T5KVhNXUs8RG2c9DkxzopenDmdFj"
        let addressTag = "0"  

        let response = client.Withdraw(Currency.XRP, address, addressTag, "test", 25m)

        response |> should not' (be null)
        if not response.IsSuccess then failwith response.Error