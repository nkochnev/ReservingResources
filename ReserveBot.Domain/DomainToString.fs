module ReserveBot.DomainToString

open ReserveBot.Types
open System

let stringArrayToString collection =
    collection |> String.concat Environment.NewLine

let private fullResourceName resourceType name =
    sprintf "(%s) %s" resourceType  name

let resourceToString =
    function
    | VM vm -> fullResourceName "vm" vm.Name
    | Organization org -> fullResourceName "org" org.Name
    | Site s -> fullResourceName "site" s.Name

let resourceToId =
    function
    | VM vm -> vm.Id
    | Organization org -> org.Id
    | Site s -> s.Id

let resourcesToString resources =
    resources
    |> Seq.map resourceToString
    |> stringArrayToString

let resourceStateToString =
    function
    | Free resource -> sprintf "%s свободен" (resourceToString resource) 
    | Busy (resource, booking) -> sprintf "%s занято @%s \r\nс %s по %s" (resourceToString (booking.Resource)) booking.Account.TelegramLogin (booking.From.ToString("dd.MM hh:mm:ss")) (booking.ExpiredIn.ToString("dd.MM hh:mm:ss"))

let resourceStatesToString states =
    states
    |> Seq.map resourceStateToString
    |> stringArrayToString

let reservingPeriodToString =
    function
    | ReservingPeriod.For2Hours _ -> "2 часа"
    | ReservingPeriod.For6Hours _ -> "6 часов"
    | ReservingPeriod.ForDay _ -> "1 день"
    | ReservingPeriod.For3Days _ -> "3 дня"
