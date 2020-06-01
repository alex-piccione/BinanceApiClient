namespace Alex75.BinanceApiClient

open System
open Alex75.Cryptocurrencies
open models



[<Interface>]
type IClient =
    inherit IApiClientPrivate
    inherit IApiClientMakeOrders
  
    // parsing not implemented yet
    abstract member GetExchangeInfo: string

    //abstract member CreateMarketOrder: pair:CurrencyPair * operation:OrderSide * amount:decimal -> CreateOrderResponse
    //abstract member CreateLimitOrder: pair:CurrencyPair * operation:OrderSide * amount:decimal * price:decimal -> CreateOrderResponse

    abstract member Withdraw: currency:Currency * address:string * addressTag:string * addressDescription:string * amount:decimal -> WithdrawResponse