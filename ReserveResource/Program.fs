module ReserveResource.Bot

open System
open ReserveResource.Rop
open ReserveResource.Domain
open ReserveResource.DomainToString
open ReserveResource.Logic

open Funogram.Bot
open Funogram.Api
open Funogram.Types

let mutable botToken = "792685377:AAEO3gKuxY_y9qKybJJhSdwsk4TMjb8jcSk"

[<EntryPoint>]
let main argv =   
       
    let processMessageBuild config =
        
        let defaultText = """⭐️Available commands:
        /get -     get reserving resource states
        /reserve - add reserve
        /release - release
        """
    
        let processResultWithValue (result: Result<'a, ApiResponseError>) =
            match result with
            | Ok v -> Some v
            | Error e ->
                printfn "Error: %s" e.Description
                None
    
        let processResult (result: Result<'a, ApiResponseError>) =
            processResultWithValue result |> ignore
    
        let botResult data = api config data |> Async.RunSynchronously
        let bot data = botResult data |> processResult
        
        let updateArrived ctx = 
            let fromId() = ctx.Update.Message.Value.From.Value.Id            
            let sendMessageFormatted text parseMode = (sendMessageBase (ChatId.Int(fromId())) text (Some parseMode) None None None None) |> bot
            
            let printString str =
                sendMessageFormatted str ParseMode.Markdown
                        
            let printDomainEvents evnts =
                evnts |> Seq.map getMessageFromDomainEvent |> Seq.iter printString
                        
            let printDomainEvent evnt =
                evnt |> getMessageFromDomainEvent |> printString
            
            let printStringWithEvents(str, evnts) =
                printString str
                printDomainEvents evnts
                        
            let printStringResult(result: RopResult<string, DomainEvents>) =
                either printStringWithEvents printDomainEvents result
            
            let printUnitResult(result: RopResult<unit, DomainEvents>) = 
                either (fun (a, b) -> printDomainEvents b) printDomainEvents result
            
            let tryGetUserFromContext =
                match ctx.Update.Message.Value.From.Value.Username with
                    | Some username ->
                        let user = getUsers() |> Seq.tryFind (fun u -> u.TelegramLogin = username)
                        match user with
                            | Some u -> succeed u
                            | None _ -> fail DomainEvents.UserNotFoundByTelegramAccount
                    | None _ -> fail DomainEvents.TelegramAccountIsEmpty
                                        
            let onGet() = tryGetUserFromContext
                          |> bindR getReservingResourceReserveStates
                          |> bindR reservingResourceStatesToString                         
                          |> printStringResult
            
            let onReserve() =  tryGetUserFromContext
                              |> bindR createAddingReserve
                              |> bindR tryAddReserve
                              |> printUnitResult
            
            let result =
                processCommands ctx [
                    cmd "/get" (fun _ ->  onGet())
                    cmd "/reserve" (fun _ ->  onReserve())
//                    cmd "/release" onRelease
                ]
    
            if result then bot (sendMessage (fromId()) defaultText)
            else ()
        updateArrived
    
    let start token =
        let config = { defaultConfig with Token = token }
        let updateArrived = processMessageBuild config
        startBot config updateArrived None    
    
    start botToken |> Async.RunSynchronously   
    
    0 // return an integer exit code