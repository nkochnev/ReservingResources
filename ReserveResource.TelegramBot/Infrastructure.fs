module ReserveResource.TelegramBotInfrastructure

open System
open Funogram.Bot
open Funogram.Types
open ReserveResource.Types
open ReserveResource.DomainToString
open ReserveResource.Logic

// types

type TelegramEvents = 
    | TelegramAccountIsEmpty
    | CallbackQueryIsEmpty
    | CallbackQueryCannotBeParsed

type TelegramBotEvents = 
    | DomainEvent of DomainEvents
    | TelegramEvent of TelegramEvents
    
type ReserveState = { Type:string; Id: Guid }
    
// functions

let bindResult(result: Result<'a, DomainEvents>) =    
    match result with
        | Result.Ok o -> Result.Ok o
        | Result.Error e -> Result.Error (TelegramBotEvents.DomainEvent e)

let bindResult2(result: Result<DomainEvents, DomainEvents>) =    
    match result with
        | Result.Ok o -> Result.Ok (TelegramBotEvents.DomainEvent o)
        | Result.Error e -> Result.Error (TelegramBotEvents.DomainEvent e)
    
let telegramEventToString =
     function
        | TelegramAccountIsEmpty _ -> "У пользователя нет имени пользователя"
        | CallbackQueryIsEmpty _ -> "Ошибка связи с сервером telegram"
        | CallbackQueryCannotBeParsed _ -> "Ошибка связи с сервером telegram"
    
let telegramBotEventsToString =
    function
        | DomainEvent d -> getMessageFromDomainEvent d
        | TelegramEvent t -> telegramEventToString t
        
let tryGetAccountFromContext ctx =
                match ctx.Update.Message.Value.From.Value.Username with
                    | Some username ->
                        let account = getAccounts() |> Seq.tryFind (fun u -> u.TelegramLogin = username)
                        match account with
                            | Some u -> Result.Ok u
                            | None _ -> Result.Error (DomainEvent DomainEvents.AccountNotFoundByTelegramUser)
                    | None _ -> Result.Error (TelegramEvent TelegramEvents.TelegramAccountIsEmpty)

let appendBreakLine str = 
        sprintf "%s%s" str Environment.NewLine

let appendReserveMessage str =
        sprintf "%s%s" str "/reserve - резервировать ресурс"

let appendShowStatesMessage str =
        sprintf "%s%s" str "/status - посмотреть состояния резервирования"
