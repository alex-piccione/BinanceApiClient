using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Alex75.BinanceApiClient;
using Alex75.Cryptocurrencies;

namespace Example
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Binance API Client example");

            var configuration = new ConfigurationBuilder().AddUserSecrets("Alex75.BinanceApiClient-79043534-af9b-475e-9b77-7bc83c77c156").Build();
            var withdrawalAddress = configuration["withdrawal address"];
            var publicKey = configuration["public key"];
            var secretKey = configuration["secret key"];

            IClient client = new Client(new Settings( TimeSpan.FromSeconds(10), publicKey, secretKey));

            ShowBalance(client);

            //Buy(client, CurrencyPair.XRP_EUR, 100);

            Console.ReadKey();
        }

        private static void ShowBalance(IClient client)
        {
            try
            {
                var balance = client.GetBalance().NotEmpty();

                Console.WriteLine($"Load Balance");

                Console.WriteLine($"Currency        | total                 | Free             | NotFree             | Stacking                  | Lending    ");
                foreach (var currency in balance.Keys)
                {
                    var currencyBalance = balance[currency].Value;
                    Console.WriteLine($"{currency,-10} {currencyBalance.Total, +16}\t {currencyBalance.Free, +16}\t {currencyBalance.NotFree, +16}\t {currencyBalance.Stacking, 16}\t {currencyBalance.Lending, 16}");
                }

                Console.WriteLine("----- ");
            }
            catch (Exception exc)
            {
                Console.WriteLine("Failed to load Balance. " + exc);
            }
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
