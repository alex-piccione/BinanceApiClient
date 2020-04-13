//module  ``unit tests``.parser_test
module parser_test

open System.Linq
open System.IO
open NUnit.Framework; open FsUnit
open Newtonsoft.Json; open Newtonsoft.Json.Linq
open Alex75.Cryptocurrencies


[<Test; Category("parser")>]
let ``parse pairs``() =
    let pairs = parser.parse_pairs (File.ReadAllText "data/exchangeInfo.json")
    pairs |> should contain (CurrencyPair("eth", "btc"))
        

[<Test; Category("parser")>]
let ``parse error``() =
    
    let jsonString = "{\"msg\":\"Timestamp for this request is outside of the recvWindow.\",\"success\":false}"
    
    let json = JsonConvert.DeserializeObject<JObject>(jsonString)

    //check for error
    let isSuccess = json.["success"].Value<bool>()
    let error = json.["msg"].Value<string>()

    isSuccess |> should be False
    error |> should equal "Timestamp for this request is outside of the recvWindow."


[<Test; Category("parser")>]
let ``parse account response`` () =

    let jsonString = System.IO.File.ReadAllText "data/account data.json"

    let balance = parser.parse_account jsonString

    balance |> should not' (be null)
    balance.HasCurrency(Currency.BTC) |> should be True
    balance.[Currency.BTC].AvailableAmount |> should equal 4723846.89208129 
    balance.[Currency.BTC].OwnedAmount |> should equal (4723846.89208129 + 10.0)
    //[Currency.BTC] |> should equal (4723846.89208129m + 10m)
    //response.Assets.[Currency.LTC] |> should equal (4763368.68006011m + 20m)