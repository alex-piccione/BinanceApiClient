module models

open Alex75.Cryptocurrencies



type ServerTime = {serverTime:int64}

[<AbstractClass>]
type Response (isSuccess:bool, error:string) = 
    member __.IsSuccess = isSuccess
    member __.Error = error

type TickerResponse (isSuccess:bool, error:string, ticker:Option<Ticker>) = //{ IsSuccess:bool; Error:string; Ticker:Ticker }
    inherit Response( isSuccess, error) 
    member __.Ticker = ticker

   

// Binance API response
type Ticker_24h(code:string, msg:string,
                lastPrice:decimal, bidPrice:decimal, askPrice:decimal, 
                openPrice:decimal, highPrice:decimal, lowPrice:decimal) =

    member __.IsSuccess = (code = null)
    member __.Error = msg

    member __.LastPrice = lastPrice
    member __.BidPrice = bidPrice
    member __.AskPrice = askPrice
    member __.OpenPrice = openPrice
    member __.HighPrice = highPrice
    member __.LowPrice = lowPrice

    member __.ToResponse pair = 

        match __.IsSuccess with 
        //| true -> TickerResponse(true, null, Some(Ticker(pair, bidPrice, askPrice, Some(lowPrice), Some(highPrice), Some(lastPrice))))
        | false -> TickerResponse(false, __.Error, None)
        | true -> 
            let ticker = Ticker(pair, bidPrice, askPrice, Some(lowPrice), Some(highPrice), Some(lastPrice))
            TickerResponse(true, null, Some(ticker))
