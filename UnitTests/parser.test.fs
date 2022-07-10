module UnitTests.Parser

open System
open System.IO
open System.Reflection
open NUnit.Framework; open FsUnit
open Newtonsoft.Json; open Newtonsoft.Json.Linq
open Alex75.Cryptocurrencies

let assembly = Assembly.GetExecutingAssembly()
let rootNamespace = assembly.GetName().Name
let getText file = (new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"{rootNamespace}.data.{file}"))).ReadToEnd()
let readData file = System.IO.File.ReadAllText("data/" + file)

[<Test>]
let ``parse pairs``() =
    let pairs = parser.parse_pairs (readData "exchangeInfo.json")
    pairs |> should contain (CurrencyPair("eth", "btc"))


[<Test>]
let ``parse error``() =    
    let jsonString = "{\"msg\":\"Timestamp for this request is outside of the recvWindow.\",\"success\":false}"    
    let json = JsonConvert.DeserializeObject<JObject>(jsonString)

    //check for error
    let isSuccess = json.["success"].Value<bool>()
    let error = json.["msg"].Value<string>()

    isSuccess |> should be False
    error |> should equal "Timestamp for this request is outside of the recvWindow."


[<Test>]
let ``parse account response`` () =
    let balances = parser.parse_account (readData "account data.json")

    balances |> should not' (be Empty)
    let balance = new AccountBalance(balances)
    balance |> should not' (be null)
    balance.HasCurrency(Currency.BTC) |> should be True
    balance[Currency.BTC].Free |> should equal 4723846.89208129
    balance[Currency.BTC].NotFree |> should equal 10.0
    balance[Currency.BTC].Total |> should equal (4723846.89208129 + 10.0)


[<Test>]
let ``Parse Open Orders`` () =
    let pair = CurrencyPair("TRX", "XRP")
    let orders = parser.ParseOpenOrders pair (readData "list open orders.json")

    orders.Length |> should equal 1
    orders.[0].Id |> should equal "38835532"
    orders.[0].OpenTime |> should (equalWithin (TimeSpan.FromSeconds(1.))) (DateTime(2020,06,21, 10,58,12))
    orders.[0].Pair |> should equal (CurrencyPair("TRX", "XRP"))
    orders.[0].Type |> should equal OrderType.Limit
    orders.[0].Side |> should equal OrderSide.Buy
    orders.[0].BuyOrSellQuantity |> should equal 1176.4
    orders.[0].LimitPrice |> should equal 0.08500



[<Test>]
let ``Parse Closed Orders`` () =
    let pair = CurrencyPair("TRX", "XRP")
    let orders = parser.ParseClosedOrders pair (readData "list closed orders.json")

    orders.Length |> should equal 1
    orders.[0].Id |> should equal "38835532"
    orders.[0].OpenTime |> should (equalWithin (TimeSpan.FromSeconds(1.))) (DateTime(2020,06,21, 10,58,12))
    orders.[0].CloseTime |> should (equalWithin (TimeSpan.FromSeconds(1.))) (DateTime(2020,06,21, 11,38,31))
    orders.[0].Pair |> should equal (CurrencyPair("TRX", "XRP"))
    orders.[0].Type |> should equal OrderType.Limit
    orders.[0].Side |> should equal OrderSide.Buy
    orders.[0].BuyOrSellQuantity |> should equal 1176.4
    orders.[0].PaidOrReceivedQuantity |> should equal 99.994
    orders.[0].Price |> should equal 0.085
    orders.[0].Fee |> should equal 0
    orders.[0].Status |> should equal "FILLED"   
    orders.[0].Reason |> should be Null
    orders.[0].Note |> should equal "executedQuantity: 1176.4 - clientOrderId:web_ebedcd0f5af94fca947644dc35aff7f4"


[<Test>]
let ``parse Stacking`` () =
    let json = getText "product Stacking.json"

    let result = parser.parseStackingResponse  json

    result.Keys.Count |> should equal 3
    result.ContainsKey("BNB") |> should be True
    result.ContainsKey("DOT") |> should be True
    result.ContainsKey("ADA") |> should be True

    result["BNB"] |> should equal (0.15217204 + 0.5)
    result["ADA"] |> should equal 0.1081
    result["DOT"] |> should equal (51.00851999m + 10m + 10m)