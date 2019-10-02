module ReserveResource.TelegramBot

open System.IO
open TelegramBotInfrastructure
open ReserveBot.Types
open ReserveBot.DomainToString
open ReserveBot.Logic
open ReserveResource.Database.Database
open Funogram.Bot
open Funogram.Api
open Funogram.Types
open Funogram.Keyboard.Inline
open ReserveResource.Keyboard
open ReserveBot.Storage
open SelectFreeResourceKeyboard
open SelectReservedResourceKeyboard
open Microsoft.EntityFrameworkCore

let TokenFileName = "token"

let processMessageBuild config =
    let defaultText() =
        "⭐️Доступные команды для резервирования:"
        |> appendBreakLine
        |> appendShowStatesMessage
        |> appendBreakLine
        |> appendReserveMessage
        |> appendBreakLine
        |> appengReleaseMessage

    let processResultWithValue (result : Result<'a, ApiResponseError>) =
        match result with
        | Ok v -> Some v
        | Error e ->
            printfn "Error: %s" e.Description
            None

    let processResult (result : Result<'a, ApiResponseError>) =
        processResultWithValue result |> ignore
    let botResult data = apiUntyped config data |> Async.RunSynchronously
    let bot data = botResult data |> processResult

    let updateArrived tctx =
        let userId =
            if tctx.Update.Message.IsSome then
                tctx.Update.Message.Value.From.Value.Id
            else tctx.Update.CallbackQuery.Value.From.Id
        
        let dctx = new ReserveBotContext()

        let sendMessageFormatted text parseMode =
            (sendMessageBase (ChatId.Int(userId)) text (Some parseMode) None
                 None None None) |> bot
        let showKeyboard def = InlineKeyboard.show bot userId def
        let say str = sendMessageFormatted str ParseMode.Markdown
        
        let sayResult (result : Result<string, AppEvents>) =
            match result with
            | Ok s -> say s
            | Error e -> say (handleAppEvents dctx e)

        let resultMessage (result : Result<AppEvents, AppEvents>) =
            match result with
            | Ok s -> handleAppEvents dctx s
            | Error e -> handleAppEvents dctx e
        
        let sayResults (result : Result<AppEvents, AppEvents>) =
            result |> resultMessage |> say
            
        let getStatesResult (states: seq<ResourceState>) =
            states |> resourceStatesToString |> appendBreakLine |> appendBreakLine |> appendReserveMessage
        
        let reserveResult (result: Result<DomainEvents,DomainEvents>) =
            result |> bindResult2 |> resultMessage |> appendBreakLine |> appendBreakLine |> appendShowStatesMessage
            
        let releaseResult (result: Result<DomainEvents,DomainEvents>) =
            result |> bindResult2 |> resultMessage |> appendBreakLine |> appendBreakLine |> appendShowStatesMessage
        
        let getAccountResourceStates() =
            tryGetAccountFromTlgrmContext(tctx,dctx)
            |> Result.map (fun account -> (account, getAvailableToAccountResources(dctx, account)))
            |> Result.map (fun (account, resources) -> 
                        let resourcesIds = resources |> Seq.map resourceToId |> Seq.toList
                        let activeBookings = getActiveBookings(dctx, resourcesIds)
                        (account, resources, activeBookings))
            |> Result.map (fun (account, resources, activeBookings) -> (account, getResourceStates(resources, activeBookings)))
        
        let reserve (resourceSelection : FreeResourceSelection option) =
            match resourceSelection with
            | Some r ->
                getAccountResourceStates()
                |> Result.bind (fun (account, states) -> getFreeResourceById(states, r.ResourceId) 
                                                        |> bindResult 
                                                        |> Result.map (fun freeResource -> (account, freeResource)))
                |> Result.map (fun (account, freeResource) -> reserveResource(account, freeResource, r.Period))                
                |> Result.map reserveResult
                |> ignore
            | None -> say "no selected action"
        
        let release (resourceSelection : ReservedResourceSelection option) =
            match resourceSelection with
            | Some r ->
                getAccountResourceStates()
                |> Result.bind (fun (account, states) -> getBusyResourceById(states, r.ResourceId) 
                                                        |> bindResult)
                |> Result.map (fun (busyResourceState) -> releasingResource busyResourceState)    
                |> Result.map releaseResult
                |> Result.map say
                |> ignore
            | None -> say "no selected action"
        
        let selectFreeResourceKeyboard freeResources =
            SelectFreeResourceKeyboard.create config "Что будешь резервировать?"
                (fun (_, selection) -> reserve (selection)) (freeResources)
            |> showKeyboard

        let selectReservedResourceKeyboard reservedResources =
            SelectReservedResourceKeyboard.create config "Что будем освобождать?"
                (fun (_, selection) -> release (selection)) (reservedResources)
            |> showKeyboard
        
        let onGet() =
            getAccountResourceStates()
            |> Result.map (fun (_, states) -> getStatesResult(states))
            |> sayResult

        let onReserve() =
            getAccountResourceStates()
            |> Result.map (fun (_, states) -> states |> filterFreeStateOnly)
            |> Result.map selectFreeResourceKeyboard
            |> Result.mapError (fun _ -> handleAppEvents(dctx))
            |> ignore
        
        let onRelease() =
            getAccountResourceStates()
            |> Result.map (fun (_, states) -> states |> filterBusyStateOnly |> Seq.map (fun (res, _)-> res))
            |> Result.map selectReservedResourceKeyboard
            |> Result.mapError (fun _ -> handleAppEvents(dctx))
            |> ignore    

        let cmds =
            [ cmd "/status" (fun _ -> onGet())
              cmd "/booking" (fun _ -> onReserve())
              cmd "/release" (fun _ -> onRelease())
               ]

        let notHandled =
            processCommands tctx (cmds @ InlineKeyboard.getRegisteredHandlers())
        if notHandled then bot (sendMessage userId (defaultText()))
    updateArrived

let start token =
    let config = { defaultConfig with Token = token }
    let updateArrived = processMessageBuild config
    startBot config updateArrived None

[<EntryPoint>]
let main argv =
    printfn "Bot started..."
    
    let dctx = new ReserveBotContext()
    dctx.Database.Migrate();
    dctx.Dispose();
    
    let startBot =
        if File.Exists(TokenFileName) then
            start (File.ReadAllText(TokenFileName))
        else
            printf "Please, enter bot token: "
            let token = System.Console.ReadLine()
            File.WriteAllText(TokenFileName, token)
            start token
    startBot |> Async.RunSynchronously
    0 // return an integer exit code
