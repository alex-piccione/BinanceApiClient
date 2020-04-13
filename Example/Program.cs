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
                var response = client.CreateMarketOrder(pair, OrderSide.Buy, amount);
                if(!response.IsSuccess)
                    Console.WriteLine("Failed to create market order. " + response.Error);
                else
                    Console.WriteLine($"Created market order. Id: {response.Id} Price: {response.Price}");
            }
            catch (Exception exc)
            {
                Console.WriteLine("Failed to create market order. " + exc);
            }
        }
    }
}
