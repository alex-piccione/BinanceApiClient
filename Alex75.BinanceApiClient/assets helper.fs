module assets_helper

open Alex75.Cryptocurrencies

(*
Stacking tokens create assets with "LD" prefix
*)

type CurrencyBalance with 
    member this.UpdateStacking (value:decimal) =
        let stacking = this.Stacking + value
        let total = this.Total + value
        CurrencyBalance(this.Currency, total, this.Free, this.InOrders, stacking, this.Lending)

let correctAssets (balances:CurrencyBalance list) =

    let isStacking balance = (balance:CurrencyBalance).Currency.UpperCase.StartsWith("LD") 

    let stackingAssets = 
        balances |> List.choose (fun b -> if isStacking(b)
                                          then Some(b.Currency.UpperCase.Substring 2, b.Total) 
                                          else None)
                 |> dict

    let checkStacking (balance:CurrencyBalance) =
        match stackingAssets.TryGetValue balance.Currency.UpperCase with
        | (true, stacking) -> balance.UpdateStacking stacking
        | _ -> balance

    let getNonZeroAssets (balance:CurrencyBalance) =
        let correctBalance = checkStacking balance
        if not(isStacking balance) && correctBalance.Total > 0m then Some(correctBalance) else None

    balances |> List.choose getNonZeroAssets
            