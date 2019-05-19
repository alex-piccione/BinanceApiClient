namespace Alex75.BinanceApiClient

open models
open Alex75.Cryptocurrencies



type IClient =
    
    abstract member GetTicker: pair:CurrencyPair -> TickerResponse

    abstract member CreateMarketOrder: pair:CurrencyPair * operation:OrderSide * amount:decimal -> CreateOrderResponse
    abstract member CreateLimitOrder: pair:CurrencyPair * operation:OrderSide * amount:decimal * price:decimal -> CreateOrderResponse