# Binance API Client

Simple client interface to facilitate the use of some **Binance** API.  
Developed specifically for _XRP_.  
  
Target frameworks: _.NET Standard 2.0_ & _.NET Core 3.1_

[![NuGet](https://img.shields.io/nuget/v/Alex75.BinanceApiClient.svg)](https://www.nuget.org/packages/Alex75.BinanceApiClient) 
[![Build Status](https://alex75.visualstudio.com/Binance%20API%20Client/_apis/build/status/Build%20v3?branchName=master)](https://alex75.visualstudio.com/Binance%20API%20Client/_build/latest?definitionId=24&branchName=master) 
![GitHub commit activity](https://img.shields.io/github/commit-activity/m/alex75it/BinanceApiClient?label=GitHub)



## Functionalities

| Function                   | <nowrap>API Key <sup>(1)</sup></nowrap> | Description                         | Status
---                          |---      |---                                                                  |---
| List Pairs                 | No      |---                                                                  | Done
| Get Ticker / Get Tickers   | No      | Get the Ask/Bid/Min/Max/Last prices of currency pair(s)             | Done
| Get Balance (modified)     | Yes     | List the owned and free amount of all the not empty currencies  	 | Done
| Create Market Order        | Yes     | Create a market order									             | Done
| Withdraw                   | Yes     | Withdraw a currency                                                 | Done

<sup>(1)</sup> = Requires an Account API key


> Note  
> **GetBalance** changed from the previous minor version **0.8**
> Now it returns a list and does not have the properties IsSuccess and Error anymore  
> The old version is renamed *GetBalance_old*.

<!--
| Create Limit Order         | Yes     | Create a limit order									 | Not implemented
| List Open Orders           | Yes     | List open orders										 | Not implemented
| Check Order Status         | Yes     |														 | Not implemented
| Cancel Order               | Yes     | Cancel an order										 | Not implemented
| List User Transactions     | Yes     | List the User Transactions								 | Not implemented
-->



Withdraw XRP has a minimum quantity of 25.  
Sell XRP has a minimum quantity of 80 ?!

### Withdrawal Suspension

From time to time the withdrawal on a particular wallet can be suspended (for maintenance).  
This is the error message: "The current currency is not open for withdrawal".  
I don't know what HTTP Status Code is returned.  


### Known issues 

- Minimum amount for orders is unknown


## For developers

Source code on GitHub.  
CI/DD in Azure DevOps.  
  
Binance API docs: https://github.com/binance-exchange/binance-official-api-docs/blob/master/rest-api.md

  
      

[![HitCount](http://hits.dwyl.io/alex75it/alex75it/BinanceApiClient.svg)](http://hits.dwyl.io/alex75it/alex75it/BinanceApiClient)


<!--
<style>
sup { font-size:70% }
nowrap, .nowrap { white-space: nowrap}
</style>
-->

