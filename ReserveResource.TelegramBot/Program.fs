module ReserveResource.TelegramBot

open System
open System.IO
open ReserveResource.Rop
open ReserveResource.Types
open ReserveResource.DomainToString
open ReserveResource.Logic
open ReserveResource.TelegramBotInfrastructure

open Funogram.Bot
open Funogram.Api
open Funogram.Types

let TokenFileName = "token"

let processMessageBuild config =
    
    let defaultText = """⭐️Available commands:
    /get -     get reserving resource states
    /reserve - add reserve
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
            evnts |> Seq.map telegramBotEventsToString |> Seq.iter printString
                    
        let printDomainEvent evnt =
            evnt |> telegramBotEventsToString |> printString
        
        let printStringWithEvents(str, evnts) =
            printString str
            printDomainEvents evnts
                    
        let printStringResult(result: RopResult<string, TelegramBotEvents>) =
            either printStringWithEvents printDomainEvents result
        
        let printUnitResult(result: RopResult<unit, TelegramBotEvents>) = 
            either (fun (a, b) -> printDomainEvents b) printDomainEvents result
        
        let toFreeResourceReserveStateButton(rss: FreeResourceReserveState) =
            [{
              Text = reservingResourceToString rss
              CallbackData = Some ("reserve1|" + (reservingResourceToId rss).ToString())
              Url = None
              CallbackGame = None
              SwitchInlineQuery = None
              SwitchInlineQueryCurrentChat = None
            }] |> List.toSeq           
        
        let toFreeResourceReserveStateButtons(rss: seq<FreeResourceReserveState>) =
            rss |> Seq.map toFreeResourceReserveStateButton |> succeed
                    
        let makeMarkup keyboard =
                   succeed (Markup.InlineKeyboardMarkup {InlineKeyboard = keyboard})
        
        let onGet() = (tryGetAccountFromContext ctx)
                      |> bindR getReservingResourceReserveStates
                      |> bindR reservingResourceStatesToString                         
                      |> printStringResult
        
        let onReserve() =
            let markup = (tryGetAccountFromContext ctx)
                            |> bindR getFreeReservingResourceReserveStates
                            |> bindR toFreeResourceReserveStateButtons
                            |> bindR makeMarkup
            let mes = "Сейчас для бронирования доступные следующие ресурсы"
            either (fun (a, b)-> (bot (sendMessageMarkup (fromId()) mes a))) (fun c -> (printDomainEvents c)) markup
                                
        let result =
            processCommands ctx [
                cmd "/get" (fun _ ->  onGet())
                cmd "/reserve" (fun _ ->  onReserve())
            ]

        if not(result) then ()
        else
            let defaultMsg()=bot (sendMessage (fromId()) "Не понимаю о чем ты")
            bot (sendMessage (fromId()) defaultText)
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