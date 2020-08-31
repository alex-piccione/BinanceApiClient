using Alex75.BinanceApiClient;
using Alex75.Cryptocurrencies;
using System;

namespace Example
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Binance API Client example");


            string publicKey = "";
            string secretKey = "";


            IClient client = new Client(new Settings( TimeSpan.FromSeconds(10), publicKey, secretKey));

            Buy(client, CurrencyPair.XRP_EUR, 100);


            Console.ReadKey();
        }

        private static void Buy(IClient client, CurrencyPair pair, decimal amount)
        {
            try
            {
                var result = client.CreateMarketOrder(CreateOrderRequest.Market(OrderSide.Buy, pair, amount));

                Console.WriteLine($"Created market order. Reference: {result.Reference} Price: {result.Price}");
            }
            catch (Exception exc)
            {
                Console.WriteLine("Failed to create market order. " + exc);
            }
        }
    }
}
