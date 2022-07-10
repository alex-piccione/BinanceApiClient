module internal parser

[<assembly:System.Runtime.CompilerServices.InternalsVisibleTo("UnitTests")>] do()

open System.Linq
open System.Collections.Generic
open FSharp.Data
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Alex75.Cryptocurrencies
open models

let getDate value = System.DateTime(1970, 01, 01).AddMilliseconds( (value:JsonValue).AsFloat())

let parse_error jsonString =
    try
        let json = JsonConvert.DeserializeObject<JObject>(jsonString)
        //(json.["code"].ToString(), json.["msg"].ToString())
        sprintf "%s (code: %s)" (json["msg"].ToString()) (json["code"].ToString())
    with e -> jsonString


let parse_pairs jsonString =
    let json = JsonConvert.DeserializeObject<JObject>(jsonString)      
    let pairs = List<CurrencyPair>()
    for item in json["symbols"].Value<JToken>() do
        let b = item["baseAsset"].Value<string>()
        let q = item["quoteAsset"].Value<string>()
        pairs.Add(CurrencyPair(b, q))
    pairs

let parse_account jsonString = 
    let json = JsonConvert.DeserializeObject<JObject>(jsonString)  
    
    let getNonZeroAsset (item:JObject) = 
        let asset = item["asset"].Value<string>()
        let free = item["free"].Value<decimal>()
        let locked = item["locked"].Value<decimal>()
        let owned = free+locked  
        // skip zero amount assets
        if owned > 0m then Some(CurrencyBalance(asset, owned, free)) else None

    let allItems = List.ofArray (json["balances"].Values<JObject>().ToArray())
    let balances = allItems |> List.choose getNonZeroAsset
       
    // Stacking create "strange" assets, compact them with correct ones
    let correctedBalance = assets_helper.correctAssets(balances)

    correctedBalance


let parseStackingResponse jsonString = 
    let json = JsonConvert.DeserializeObject<JArray>(jsonString) 

    let getStacking (json:JObject) = json["asset"].Value<string>(), json["amount"].Value<decimal>()

    let sumAmount (grouped:(string * decimal) list): decimal =
        grouped |> List.fold (fun acc (x,v) -> acc + v) 0m

    json.Values<JObject>().ToArray()
    |> List.ofArray
    |> List.map getStacking
    |> List.groupBy (fun (a,_) -> a) // group by asset
    |> List.map (fun (a,vl) -> a, sumAmount(vl))  // sum the amount of asset
    |> dict

let ParseCreateOrderResponse jsonString =
    let json = JsonConvert.DeserializeObject<JObject>(jsonString) 
    json["OrderId"].Value<string>(), json["price"].Value<decimal>()


let ParseOpenOrders pair jsonString =

    let getOrder (pair:CurrencyPair) (item:JsonValue) = 

        let id = item["orderId"].AsString()
        let openTime = getDate item["time"]
        let quantity = item["origQty"].AsDecimal()
        let side = match item["side"].AsString() with
                   | "BUY" -> OrderSide.Buy
                   | "SELL" -> OrderSide.Sell
                   | x -> failwithf "Unknown side: %s " x
        let type_ = match item["type"].AsString() with
                    | "MARKET" -> OrderType.Market
                    | "LIMIT" -> OrderType.Limit
                    | x -> failwithf "Unknown type: %s " x
        let limitPrice = if type_ = OrderType.Limit then item["price"].AsDecimal() else 0m

        let note = sprintf "Status:%s" (item["status"].AsString())

        OpenOrder(id, type_, side, openTime, pair, quantity, limitPrice )

    JsonValue.Parse(jsonString).AsArray()
    |> Array.map (getOrder pair)


let ParseClosedOrders pair jsonString =

    let getOrder (pair:CurrencyPair) (item:JsonValue) = 

        let status = item["status"].AsString()

        if status = "NEW" then None
        else
            let id = item["orderId"].AsString()
            let openTime = getDate item["time"]
            let closeTime = getDate item["updateTime"]
            let quantity = item["origQty"].AsDecimal()
            let executedQuantity = item["executedQty"].AsDecimal()
            let paid = item["cummulativeQuoteQty"].AsDecimal() 

            let side = match item.["side"].AsString() with
                       | "BUY" -> OrderSide.Buy
                       | "SELL" -> OrderSide.Sell
                       | x -> failwithf "Unknown side: %s " x
            let type_ = match item.["type"].AsString() with
                        | "MARKET" -> OrderType.Market
                        | "LIMIT" -> OrderType.Limit
                        | x -> failwithf "Unknown type: %s " x
            let price = if type_ = OrderType.Limit then item.["price"].AsDecimal() else 0m

            let clientOrderId = item.["clientOrderId"].AsString()

            let fee = 0m
            // calculate fee using quantity, price, paid ...
            let note = sprintf "executedQuantity: %s - clientOrderId:%s" (executedQuantity.ToString("G27")) clientOrderId

            Some (ClosedOrder(id, type_, side, openTime, closeTime, status, null, pair, quantity, paid, price, fee, note))

    JsonValue.Parse(jsonString).AsArray()
    |> Array.choose (getOrder pair) 
    //|> Array.map (getOrder pair) 