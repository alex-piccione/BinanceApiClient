module settings

open System
open System.IO
open System.Text.RegularExpressions

open Alex75.BinanceApiClient

let readSettings = 
    let mutable seconds = 0.
    let mutable publicKey = ""
    let mutable secretKey = ""
    for line in File.ReadAllLines("settings.yaml") do
        match line with 
        | text  when Regex.IsMatch(text, "TickerCacheDuration: [\d]+") -> seconds <- float(Regex.Match(text, "TickerCacheDuration: ([\d]+)").Groups.[1].Value)
        | text  when Regex.IsMatch(text, "PublicKey: \"[\w\d]+\"") -> publicKey <- Regex.Match(text, "PublicKey: \"([\w\d]+)\"").Groups.[1].Value
        | text  when Regex.IsMatch(text, "SecretKey: \"[\w\d]+\"") -> secretKey <- Regex.Match(text, "SecretKey: \"([\w\d]+)\"").Groups.[1].Value
        | _ -> ()
    { TickerCacheDuration=TimeSpan.FromSeconds(seconds); PublicKey=publicKey; SecretKey=secretKey }

let settings = readSettings