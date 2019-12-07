module parser_test

open NUnit.Framework; open FsUnit
open Newtonsoft.Json; open Newtonsoft.Json.Linq



[<Test>]
let ``parse error``() =
    
    let jsonString = "{\"msg\":\"Timestamp for this request is outside of the recvWindow.\",\"success\":false}"
    
    let json = JsonConvert.DeserializeObject<JObject>(jsonString)

    //check for error
    let isSuccess = json.["success"].Value<bool>()
    let error = json.["msg"].Value<string>()

    isSuccess |> should be False
    error |> should equal "Timestamp for this request is outside of the recvWindow."


