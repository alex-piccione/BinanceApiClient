# Binance API Client

Simple client interface to facilitate the use of some **Binance** API.  
Developed specifically for _XRP_.  
  
Target frameworks: _.NET Standard 2.0_ & _.NET Core 3.1_

[![NuGet](https://img.shields.io/nuget/v/Alex75.BinanceApiClient.svg)](https://www.nuget.org/packages/Alex75.BinanceApiClient) 
[![Build Status](https://alex75.visualstudio.com/Binance%20API%20Client/_apis/build/status/Build%20v3?branchName=master)](https://alex75.visualstudio.com/Binance%20API%20Client/_build/latest?definitionId=24&branchName=master) 
[GitHub commit activity](https://img.shields.io/github/commit-activity/m/alex75it/BinanceApiClient?label=GitHub)



## Functionalities

| Function                   | <nowrap>API Key <sup>(1)</sup></nowrap> | Description           | Status
---                          |---      |---                                                      |---
| Get Ticker / Get Tickers   | No      | Get the Ask/Bid/Min/Max/Last prices of currency pair(s) | Done
| Get Balance                | Yes     | List the availability of all the currencies			 | Done
| Create Market Order        | Yes     | Create a market order									 | Done
| Withdraw                   | Yes     | Withdraw a currency                                     | Done

<sup>(1)</sup> = Require an Account API key

<!--
| Create Limit Order         | Yes     | Create a limit order									 | Not implemented
| List Open Orders           | Yes     | List open orders										 | Not implemented
| Check Order Status         | Yes     |														 | Not implemented
| Cancel Order               | Yes     | Cancel an order										 | Not implemented
| List User Transactions     | Yes     | List the User Transactions								 | Not implemented
-->



## XRP note

Binance does not offer the pair _XRP/USD_ nor _XRP/EUR_ (Dec 2019).  
XRP can only be exchanged using:  
- XRP/BTC
- XRP/ETH
- TRX/XRP
- ZXC/XRP

Withdraw XRP has a minimum quantity of 25.


### Known issues 

Passing the destination tag "0" fails the withdrawal with teh error "Address verification failed"; same behavior on the website.  


## For developers

Source code on GitHub.

- https://www.binance.com/userCenter/createApi.html
- https://github.com/binance-exchange/binance-official-api-docs/blob/master/rest-api.md

  
      

[![HitCount](http://hits.dwyl.io/alex75it/alex75it/BinanceApiClient.svg)](http://hits.dwyl.io/alex75it/alex75it/BinanceApiClient)


<!--
<style>
sup { font-size:70% }
nowrap, .nowrap { white-space: nowrap}
</style>
-->

