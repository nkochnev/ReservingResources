module ReserveResource.TelegramBotInfrastructure

open System
open Funogram.Bot
open Funogram.Types
open ReserveResource.Types
open ReserveResource.DomainToString
open ReserveResource.Storage.Database
open FSharp.Data.Sql

// types

type TelegramEvents = 
    | TelegramAccountIsEmpty
    | CallbackQueryIsEmpty
    | CallbackQueryCannotBeParsed

type AppEvents = 
    | DomainEvent of DomainEvents
    | TelegramEvent of TelegramEvents
   
// functions

let bindResult(result: Result<'a, DomainEvents>) =    
    match result with
        | Result.Ok o -> Result.Ok o
        | Result.Error e -> Result.Error (AppEvents.DomainEvent e)

let bindResult2(result: Result<DomainEvents, DomainEvents>) =    
    match result with
        | Result.Ok o -> Result.Ok (AppEvents.DomainEvent o)
        | Result.Error e -> Result.Error (AppEvents.DomainEvent e)
    
let handleTelegramEvents =
     function
        | TelegramAccountIsEmpty _ -> "У пользователя нет имени пользователя"
        | CallbackQueryIsEmpty _ -> "Ошибка связи с сервером telegram"
        | CallbackQueryCannotBeParsed _ -> "Ошибка связи с сервером telegram"

let handleDomainEvent dctx event =
    match event with 
        | AccountNotFoundByTelegramUser _ -> "Пользователь не зарегистрирован"
        | BookingAdded (resource, activeBooking) ->
            addBooking(dctx, activeBooking)
            sprintf "Бронирование для %s добавлено до %s" (resourceToString resource) (activeBooking.ExpiredIn.ToString())
        | ResourceReleased (booking, released) -> 
            releaseBooking(dctx, booking, released)
            sprintf "Ресурс %s освобожден" (resourceToString (booking.Resource))
        | ResourceByIdNotFound id ->
            "Ресурс с идентификатором " + id.ToString() + " не найден"
        | ResourceAlreadyBusy r ->
            "Нельзя забронировать ресурс " + resourceToString r.Resource + ", т.к. ресурс занят"
        | ResourceAlreadyFree r ->
            sprintf "Не могу освободить ресурс, %s т.к. он уже свободен" (resourceToString r)
    
let handleAppEvents dctx event =
    match event with
        | DomainEvent domainEvent -> handleDomainEvent dctx domainEvent
        | TelegramEvent telegramEvent -> handleTelegramEvents telegramEvent
        
let tryGetAccountFromTlgrmContext(tctx,dctx) =
                match tctx.Update.Message.Value.From.Value.Username with
                    | Some username -> bindResult (getAccount(dctx, username))
                    | None _ -> Result.Error (TelegramEvent TelegramEvents.TelegramAccountIsEmpty)

let appendBreakLine str = 
        sprintf "%s%s" str Environment.NewLine

let appendShowStatesMessage str =
        sprintf "%s%s" str "/status - посмотреть состояния резервирования"
        
let appendReserveMessage str =
        sprintf "%s%s" str "/booking - забронировать ресурс"

let appengReleaseMessage str =
        sprintf "%s%s" str "/release - освободить ресурс"
