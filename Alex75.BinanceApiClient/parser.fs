module parser

open System.Collections.Generic
open Alex75.Cryptocurrencies
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open models



let parse_account jsonString = 
    let json = JsonConvert.DeserializeObject<JObject>(jsonString)    
    let assets = new Dictionary<Currency, decimal>()
    for balance in json.["balances"].Values<JObject>() do
        let asset = balance.["asset"].Value<string>()
        let free = balance.["free"].Value<decimal>()
        let locked = balance.["locked"].Value<decimal>()
        assets.Add(Currency(asset), free + locked)             

    BalanceResponse(true, null, assets)    