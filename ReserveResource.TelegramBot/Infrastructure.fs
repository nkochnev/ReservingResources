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
    
let telegramEventToString =
     function
        | TelegramAccountIsEmpty _ -> "У пользователя нет имени пользователя"
        | CallbackQueryIsEmpty _ -> "Ошибка связи с сервером telegram"
        | CallbackQueryCannotBeParsed _ -> "Ошибка связи с сервером telegram"
    
let telegramBotEventsToString =
    function
        | DomainEvent d -> getMessageFromDomainEvent d
        | TelegramEvent t -> telegramEventToString t
        
let tryGetAccountFromContext(ctx:UpdateContext) : Result<Account, TelegramBotEvents> =
                match ctx.Update.Message.Value.From.Value.Username with
                    | Some username ->
                        let account = getAccounts() |> Seq.tryFind (fun u -> u.TelegramLogin = username)
                        match account with
                            | Some u -> Result.Ok u
                            | None _ -> Result.Error (DomainEvent DomainEvents.AccountNotFoundByTelegramUser)
                    | None _ -> Result.Error (TelegramEvent TelegramEvents.TelegramAccountIsEmpty)