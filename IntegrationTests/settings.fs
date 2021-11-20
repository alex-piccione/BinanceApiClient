module settings

open System
open Alex75.BinanceApiClient
open Microsoft.Extensions.Configuration

let mutable withdrawalAddress = ""

let readSettings () = 
    let configuration = ConfigurationBuilder().AddUserSecrets("Alex75.BinanceApiClient-79043534-af9b-475e-9b77-7bc83c77c156").Build()

    withdrawalAddress <- configuration.["withdrawal address"]
    let publicKey = configuration.["public key"]
    let secretKey = configuration.["secret key"]
 
    { TickerCacheDuration=TimeSpan.FromSeconds(60.); PublicKey=publicKey; SecretKey=secretKey; }

let settings = readSettings()