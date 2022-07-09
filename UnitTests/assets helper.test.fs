//namespace UnitTests.Parder

module UnitTests.``Assets Helper``

open NUnit.Framework
open FsUnit
open Alex75.Cryptocurrencies

[<Test>]
let ``correctAssets set non-fixed Stacking into correct asset as Stacking``() =

    // Stacking assets are returned in a separate row and thecurrency is prefixed with "LD"
    let balance = [
        CurrencyBalance(Currency("AAA"), 100, 100, 0, 0, 0) // free asset
        CurrencyBalance(Currency("LDAAA"), 50, 50, 0, 0, 0) // Stacking asset
    ]

    let normalizedBalance = assets_helper.correctAssets(balance)
        
    normalizedBalance.Length |> should equal 1
    let currencyBalance = normalizedBalance.Item(0)
    currencyBalance.Currency |> should equal (Currency("AAA"))
    currencyBalance.Total |> should equal 150
    currencyBalance.Free |> should equal 100
    currencyBalance.Stacking |> should equal 50
