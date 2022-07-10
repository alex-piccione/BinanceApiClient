module UnitTests.Client

open System
open NUnit.Framework
open FsUnit
open Flurl.Http
open Alex75.BinanceApiClient
open Flurl.Http.Testing
open Alex75.Cryptocurrencies


type IFlurlRequest with
    member this.GetAsync() = ()


type stacking = { asset:string; amount:decimal }
type balanceItem = { asset:string; free:decimal; locked:decimal }
type balanceResponse = { balances: balanceItem list }

[<Test>]
let ``GetBalance [when] a Stacking exists [should] add it to Balance`` () =

    let serverTime = "{ serverTime:1000 }"        
    
    let stackingResponse: stacking list = [
        { asset = "AAA"; amount = 50.000m }
    ]

    let balanceResponse:balanceResponse = { balances =  [
        { asset = "AAA"; free = 100m; locked = 0m }
    ] }

    use httpTest = new HttpTest()
    httpTest
        .RespondWith(serverTime) // getServerTime
        .RespondWithJson(stackingResponse)    // getStacking
        .RespondWithJson(balanceResponse)  // where is this call ?
        .RespondWithJson(balanceResponse)
        .RespondWithJson("should not hapen")
        |> ignore

    let settings:Settings = { TickerCacheDuration = TimeSpan.FromSeconds(30); PublicKey = "aaa"; SecretKey = "bbb" }
    let client = Client(settings) :> IClient

    // execute
    let balance = client.GetBalance()

    balance |> should not' (be Empty)
    balance.HasCurrency(Currency("AAA")) |> should be True
    let aaa = balance[Currency("AAA")]
    aaa.Total |> should equal 150m
    aaa.Free |> should equal 100m
    aaa.NotFree |> should equal 50m
    aaa.Stacking |> should equal 50m