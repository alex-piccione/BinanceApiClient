# Binance API Client

Simple client interface to facilitate the use of some **Binance** API.  
Target frameworks: _.NET Standard 2.0_, _.NET Core 3.1_ & .net 6.0  
Tested thoroughly with _XRP_ currency.  
  
Code Repository: https://github.com/alex-piccione/BinanceApiClient  
CI/CD: [Azure DevOps](https://alex75.visualstudio.com/Binance%20API%20Client)  

[![NuGet](https://img.shields.io/nuget/v/Alex75.BinanceApiClient.svg)](https://www.nuget.org/packages/Alex75.BinanceApiClient) 
[![Build Status](https://alex75.visualstudio.com/Binance%20API%20Client/_apis/build/status/.azure/master%20(2023)?branchName=master)](https://alex75.visualstudio.com/Binance%20API%20Client/_build/latest?definitionId=50&branchName=master)
![GitHub commit activity](https://img.shields.io/github/commit-activity/m/alex75it/BinanceApiClient?label=GitHub)

## Functionalities

| Function                   | Description                                                     | Status
| ---                        | ---                                                             | ---
| List Pairs                 | List pairs available on the exchange                            | Done
| Get Ticker / Get Tickers   | Get the Ask/Bid/Min/Max/Last prices of currency pair(s)         | Done
| Get Balance                | List the owned and free amount of all the not empty currencies  | Done
| Create Market Order        | Create a market order		                                       | TODO
| Create Limit Order         | Create a market order	                                         | Done
| List Open Orders           | List the active orders on the specified markets                 | Done
| List Closed orders         | List completed orders                                           | Done
| Withdraw                   | Withdraw a currency                                             | Done
| List Withdrawals           | List of withdrawals                                             | TODO



### Withdrawal Suspension

From time to time the withdrawal on a particular wallet can be suspended (for maintenance).  
This is the error message: "The current currency is not open for withdrawal".  
I don't know what HTTP Status Code is returned.  


### Known issues 

- Minimum amount for orders is unknown
  Withdraw XRP has a minimum quantity of 25 become 50 (2019-07-07).  
  Sell XRP has a minimum quantity of 80 ?!

- Withdraw crypto can be suspended.  
  
  Can't find and endpoint to know this in advance.  

## For developers
 
Binance API docs: 
- https://github.com/binance-exchange/binance-official-api-docs/blob/master/rest-api.md
- https://binance-docs.github.io/apidocs/spot/en/

[![HitCount](http://hits.dwyl.io/alex75it/alex75it/BinanceApiClient.svg)](http://hits.dwyl.io/alex75it/alex75it/BinanceApiClient)


<!--
<style>
sup { font-size:70% }
nowrap, .nowrap { white-space: nowrap}
</style>
-->

