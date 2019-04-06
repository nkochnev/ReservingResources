module ReserveResource.TelegramBot

open System.IO
open ReserveResource.Rop
open ReserveResource.Types
open ReserveResource.DomainToString
open ReserveResource.Logic
open ReserveResource.TelegramBotInfrastructure
open ReserveResource.Keyboard

open Funogram.Bot
open Funogram.Api
open Funogram.Types
open Funogram.Keyboard.Inline
open ReserveResource.Keyboard.SelectResourceKeyboard

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

    let botResult data = apiUntyped config data |> Async.RunSynchronously
    let bot data = botResult data |> processResult
    
    let updateArrived ctx = 
        let userId = if ctx.Update.Message.IsSome then ctx.Update.Message.Value.From.Value.Id
                     else ctx.Update.CallbackQuery.Value.From.Id         
        let sendMessageFormatted text parseMode = (sendMessageBase (ChatId.Int(userId)) text (Some parseMode) None None None None) |> bot
        
        let showKeyboard def=InlineKeyboard.show bot userId def
        
        let say str =
            sendMessageFormatted str ParseMode.Markdown
                    
        let shout evnts =
            evnts |> Seq.map telegramBotEventsToString |> Seq.iter say
        
        let tell(str, evnts) =
            say str
            shout evnts
                            
        let printStringResult(result: RopResult<string, TelegramBotEvents>) =
            either tell shout result
                        
        let selectResourceKeyboard freeResources = SelectResourceKeyboard.create config "Что будешь бронировать?"
                                                           (fun (_,date)->say (sprintf "%A %A" date.Value.ResourceType date.Value.ResourceId)) (freeResources)
                                                           |> showKeyboard
                                                           |> succeed
        
        let onGet() = (tryGetAccountFromContext ctx)
                      |> bindR getResourceReserveStates
                      |> bindR resourceStatesToString                         
                      |> printStringResult        
                
        let onReserve() = (tryGetAccountFromContext ctx)
                        |> bindR getFreeResourceReserveStates
                        |> bindR selectResourceKeyboard
                        |> either (fun _ -> ()) (fun c -> (shout c))
                        
        let cmds=[
                cmd "/get" (fun _ ->  onGet())
                cmd "/reserve" (fun _ ->  onReserve())
            ]
        let notHandled =
            processCommands ctx (cmds @ InlineKeyboard.getRegisteredHandlers())
        if notHandled then             
            bot (sendMessage userId defaultText)            
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