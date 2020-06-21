module internal parser

[<assembly:System.Runtime.CompilerServices.InternalsVisibleTo("UnitTests")>] do()

open System.Collections.Generic
open Alex75.Cryptocurrencies
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open models
open FSharp.Data


let getDate value = System.DateTime(1970, 01, 01).AddMilliseconds( (value:JsonValue).AsFloat())

let parse_error jsonString =
    try
        let json = JsonConvert.DeserializeObject<JObject>(jsonString)
        //(json.["code"].ToString(), json.["msg"].ToString())
        sprintf "%s (code: %s)" (json.["msg"].ToString()) (json.["code"].ToString())
    with e -> jsonString



let parse_pairs jsonString =
    let json = JsonConvert.DeserializeObject<JObject>(jsonString)      
    let pairs = List<CurrencyPair>()
    for item in json.["symbols"].Value<JToken>() do
        let b = item.["baseAsset"].Value<string>()
        let q = item.["quoteAsset"].Value<string>()
        pairs.Add(CurrencyPair(b, q))
    pairs

let parse_account jsonString = 
    let json = JsonConvert.DeserializeObject<JObject>(jsonString)    
    let balance = new List<CurrencyBalance>()
    for item in json.["balances"].Values<JObject>() do
        let asset = item.["asset"].Value<string>()
        let free = item.["free"].Value<decimal>()
        let locked = item.["locked"].Value<decimal>()
        let owned = free+locked
        if owned > 0m then balance.Add(CurrencyBalance(asset, owned, free))        
    new AccountBalance(balance)

let parse_account_old jsonString = 
    let json = JsonConvert.DeserializeObject<JObject>(jsonString)    
    let assets = new Dictionary<Currency, decimal>()
    for balance in json.["balances"].Values<JObject>() do
        let asset = balance.["asset"].Value<string>()
        let free = balance.["free"].Value<decimal>()
        let locked = balance.["locked"].Value<decimal>()
        if (free + locked) > 0m then assets.Add(Currency(asset), free + locked)             

    BalanceResponse(true, null, assets)    


let ParseCreateOrderResponse jsonString =
    let json = JsonConvert.DeserializeObject<JObject>(jsonString) 
    (json.["OrderId"].Value<string>(), json.["price"].Value<decimal>())


let ParseOpenOrders pair jsonString =

    let getOrder (pair:CurrencyPair) (item:JsonValue) = 

        let id = item.["orderId"].AsString()
        let openTime = getDate item.["time"]
        let quantity = item.["origQty"].AsDecimal()
        let side = match item.["side"].AsString() with
                   | "BUY" -> OrderSide.Buy
                   | "SELL" -> OrderSide.Sell
                   | x -> failwithf "Unknown side: %s " x
        let type_ = match item.["type"].AsString() with
                    | "MARKET" -> OrderType.Market
                    | "LIMIT" -> OrderType.Limit
                    | x -> failwithf "Unknown type: %s " x
        let limitPrice = if type_ = OrderType.Limit then item.["price"].AsDecimal() else 0m

        let note = sprintf "Status:%s" (item.["status"].AsString())

        OpenOrder(id, type_, side, openTime, pair, quantity, limitPrice )

    JsonValue.Parse(jsonString).AsArray()
    |> Array.map (getOrder pair)

