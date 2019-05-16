namespace Alex75.BinanceApiClient

open Alex75.Cryptocurrencies


type IClient =
    
    abstract member GetTicker: pair:CurrencyPair -> Ticker

    abstract member CreateMarketOrder: pair:CurrencyPair * operation:OrderDirection * amount:decimal -> CreateOrderResponse
    abstract member CreateLimitOrder: pair:CurrencyPair * operation:OrderDirection * amount:decimal * price:decimal -> CreateOrderResponse