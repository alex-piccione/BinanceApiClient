module internal cache

open System
open System.Collections.Concurrent
open Alex75.Cryptocurrencies


type internal TickerCache = {date:DateTime; ticker:Ticker}
    
let cache_tickers = ConcurrentDictionary<string, TickerCache>() 

let create_key (pair:CurrencyPair) = sprintf "%O%O" pair.Main pair.Other

let getTicker currency_pair cache_duration = 
     let found, item = cache_tickers.TryGetValue (create_key currency_pair)
     if found && (DateTime.UtcNow - item.date) < cache_duration then Some item.ticker
     else None

let setTicker currency_pair ticker =

    let tickerCache = { date=DateTime.UtcNow; ticker=ticker }
    let key = create_key currency_pair
    if cache_tickers.ContainsKey key then cache_tickers.[key] <- tickerCache
    else cache_tickers.TryAdd(key, tickerCache) |> ignore

        
    