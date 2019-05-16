namespace Alex75.BinanceApiClient

type OrderDirection =
    | Buy
    | Sell

type CreateOrderResponse(isSuccess:bool, error:string) =
    member this.IsSuccess = isSuccess
    member this.Error = error