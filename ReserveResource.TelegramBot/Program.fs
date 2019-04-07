module ReserveResource.TelegramBot

open System.IO
open ReserveResource.DomainToString
open ReserveResource.Logic
open ReserveResource.TelegramBotInfrastructure
open ReserveResource.Keyboard
open Funogram.Bot
open Funogram.Api
open Funogram.Types
open Funogram.Keyboard.Inline
open ReserveResource.Keyboard.SelectResourceKeyboard
open ReserveResource.Types

let TokenFileName = "token"

let processMessageBuild config =
    let defaultText() =
        "⭐️Доступные команды для резервирования:"
        |> appendBreakLine
        |> appendShowStatesMessage
        |> appendBreakLine
        |> appendReserveMessage

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
        
        let reserve (freeResourceSelection : FreeResourceSelection option) =
            match freeResourceSelection with
            | Some f ->
                ctx
                |> tryGetAccountFromContext
                |> Result.bind
                       (fun account ->
                       let r = getResourceById (account, f.ResourceId)
                       match r with
                       | Ok o -> Result.Ok(account, o)
                       | Error e ->
                           Result.Error(TelegramBotEvents.DomainEvent e))
                |> Result.bind
                       (fun (account, resource) -> (createAddingReserve account resource f.Period) |> bindResult )
                |> Result.map tryAddReserve
                |> Result.map reserveResult
                |> Result.map say
                |> ignore
            | None -> say "no selected action"

        let selectResourceKeyboard freeResources =
            SelectResourceKeyboard.create config "Что будешь резервировать?"
                (fun (_, date) -> reserve (date)) (freeResources)
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
            |> Result.map selectResourceKeyboard
            |> Result.mapError telegramBotEventsToString
            |> Result.mapError say
            |> ignore

        let cmds =
            [ cmd "/status" (fun _ -> onGet())
              cmd "/reserve" (fun _ -> onReserve()) ]

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
