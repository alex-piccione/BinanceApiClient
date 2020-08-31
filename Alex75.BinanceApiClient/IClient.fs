namespace Alex75.BinanceApiClient

open System
open Alex75.Cryptocurrencies
open models



[<Interface>]
type IClient =
    inherit IApiClientPrivate
    inherit IApiClientMakeOrders
    inherit IApiClientListOrders
    //inherit IApiClientWithdrawals
  
    // parsing not implemented yet
    abstract member GetExchangeInfo: string

    abstract member Withdraw: currency:Currency * address:string * addressTag:string * addressDescription:string * amount:decimal -> WithdrawResponse
    abstract member ListWithdrawals: Withdrawal[]