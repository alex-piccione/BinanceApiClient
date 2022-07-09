module models

open Alex75.Cryptocurrencies

type ServerTime = {serverTime:int64}

[<AbstractClass>]
type Response (isSuccess:bool, error:string) = 
    member this.IsSuccess = isSuccess
    member this.Error = error

type TickerResponse (isSuccess:bool, error:string, ticker:Option<Ticker>) =
    inherit Response( isSuccess, error) 
    member __.Ticker = ticker
   

// Binance API response
type Ticker_24h(code:string, msg:string,
                lastPrice:decimal, bidPrice:decimal, askPrice:decimal, 
                openPrice:decimal, highPrice:decimal, lowPrice:decimal) =

    member this.IsSuccess = (code = null)
    member this.Error = msg

    member this.LastPrice = lastPrice
    member this.BidPrice = bidPrice
    member this.AskPrice = askPrice
    member this.OpenPrice = openPrice
    member this.HighPrice = highPrice
    member this.LowPrice = lowPrice

    member this.ToResponse pair = 

        match this.IsSuccess with 
        //| true -> TickerResponse(true, null, Some(Ticker(pair, bidPrice, askPrice, Some(lowPrice), Some(highPrice), Some(lastPrice))))
        | false -> TickerResponse(false, this.Error, None)
        | true -> 
            let ticker = Ticker(pair, bidPrice, askPrice, Some(lowPrice), Some(highPrice), Some(lastPrice))
            TickerResponse(true, null, Some(ticker))
