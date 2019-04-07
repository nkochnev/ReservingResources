module ReserveResource.TelegramBot

open System.IO
open ReserveResource.DomainToString
open ReserveResource.Logic
open ReserveResource.TelegramBotInfrastructure
open Funogram.Bot
open Funogram.Api
open Funogram.Types
open Funogram.Keyboard.Inline
open ReserveResource.Keyboard
open ReserveResource.Keyboard.SelectFreeResourceKeyboard
open ReserveResource.Keyboard.SelectReservedResourceKeyboard
open ReserveResource.Types

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

    let updateArrived ctx =
        let userId =
            if ctx.Update.Message.IsSome then
                ctx.Update.Message.Value.From.Value.Id
            else ctx.Update.CallbackQuery.Value.From.Id

        let sendMessageFormatted text parseMode =
            (sendMessageBase (ChatId.Int(userId)) text (Some parseMode) None
                 None None None) |> bot
        let showKeyboard def = InlineKeyboard.show bot userId def
        let say str = sendMessageFormatted str ParseMode.Markdown

        let sayResult (result : Result<string, TelegramBotEvents>) =
            match result with
            | Ok s -> say s
            | Error e -> say (telegramBotEventsToString e)

        let resultMessage (result : Result<TelegramBotEvents, TelegramBotEvents>) =
            match result with
            | Ok s -> telegramBotEventsToString s
            | Error e -> telegramBotEventsToString e
        
        let sayResults (result : Result<TelegramBotEvents, TelegramBotEvents>) =
            result |> resultMessage |> say
            
        let getStatesResult (states: seq<ResourceState>) =
            states |> resourceStatesToString |> appendBreakLine |> appendBreakLine |> appendReserveMessage

        let reserveResult (result: Result<DomainEvents,DomainEvents>) =
            result |> bindResult2 |> resultMessage |> appendBreakLine |> appendBreakLine |> appendShowStatesMessage
            
        let releaseResult (result: Result<DomainEvents,DomainEvents>) =
            result |> bindResult2 |> resultMessage |> appendBreakLine |> appendBreakLine |> appendShowStatesMessage
        
        let reserve (resourceSelection : FreeResourceSelection option) =
            match resourceSelection with
            | Some r ->
                ctx
                |> tryGetAccountFromContext
                |> Result.bind
                       (fun account ->
                       let r = getResourceById (account, r.ResourceId)
                       match r with
                       | Ok o -> Result.Ok(account, o)
                       | Error e ->
                           Result.Error(TelegramBotEvents.DomainEvent e))
                |> Result.bind
                       (fun (account, resource) -> (createAddingReserve account resource r.Period) |> bindResult )
                |> Result.map tryAddReserve
                |> Result.map reserveResult
                |> Result.map say
                |> ignore
            | None -> say "no selected action"
        
        let release (resourceSelection : ReservedResourceSelection option) =
            match resourceSelection with
            | Some r ->
                ctx
                |> tryGetAccountFromContext
                |> Result.bind (fun account -> getResourceById(account, r.ResourceId) |> bindResult)
                |> Result.map releaseResource
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
            ctx
            |> tryGetAccountFromContext
            |> Result.map getResourceStates
            |> Result.map getStatesResult
            |> sayResult

        let onReserve() =
            ctx
            |> tryGetAccountFromContext
            |> Result.map getFreeResourceStates
            |> Result.map selectFreeResourceKeyboard
            |> Result.mapError telegramBotEventsToString
            |> Result.mapError say
            |> ignore
        
        let onRelease() =
            ctx
            |> tryGetAccountFromContext
            |> Result.map getReservedResoures
            |> Result.map selectReservedResourceKeyboard
            |> Result.mapError telegramBotEventsToString
            |> Result.mapError say
            |> ignore    

        let cmds =
            [ cmd "/status" (fun _ -> onGet())
              cmd "/reserve" (fun _ -> onReserve())
              cmd "/release" (fun _ -> onRelease())
               ]

        let notHandled =
            processCommands ctx (cmds @ InlineKeyboard.getRegisteredHandlers())
        if notHandled then bot (sendMessage userId (defaultText()))
    updateArrived

let start token =
    let config = { defaultConfig with Token = token }
    let updateArrived = processMessageBuild config
    startBot config updateArrived None

[<EntryPoint>]
let main argv =
    printfn "Bot started..."
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
