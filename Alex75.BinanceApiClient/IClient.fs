namespace Alex75.BinanceApiClient

open models
open Alex75.Cryptocurrencies


[<Interface>]
type IClient =
    
    abstract member GetTicker: pair:CurrencyPair -> TickerResponse

    // parsing not implemented yet
    abstract member GetExchangeInfo: string

    abstract member CreateMarketOrder: pair:CurrencyPair * operation:OrderSide * amount:decimal -> CreateOrderResponse
    //abstract member CreateLimitOrder: pair:CurrencyPair * operation:OrderSide * amount:decimal * price:decimal -> CreateOrderResponse

    abstract member Withdraw: currency:Currency * address:string * addressTag:string * addressDescription:string * amount:decimal -> WithdrawResponse