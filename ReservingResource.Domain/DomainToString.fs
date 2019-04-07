module ReserveResource.DomainToString

open System
open ReserveResource.Types

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
    | Free f -> sprintf "%s свободен" (resourceToString f) 
    | Busy b -> sprintf "%s занято @%s \r\nс %s по %s" (resourceToString (b.Resource))
                    b.Account.TelegramLogin (b.From.ToString()) (b.ExpiredIn.ToString())

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

let getMessageFromDomainEvent =
    function
    | AccountNotFoundByTelegramUser _ -> "Пользователь не зарегистрирован"
    | ReserveAdded r ->
        sprintf "Бронирование для %s добавлено до %s" (resourceToString r.Resource) (r.ExpiredIn.ToString())
    | ResourceReleased r -> sprintf "Ресурс %s освобожден" (resourceToString r.Resource)
    | ResourceByIdNotFound id ->
        "Ресурс с идентификатором " + id.ToString() + " не найден"
    | ResourceAlreadyBusy r ->
        "Нельзя забронировать ресурс " + resourceToString r.Resource + ", т.к. ресурс занят"
    | ResourceAlreadyFree r ->
        sprintf "Не могу освободить ресурс, %s т.к. он уже свободен" (resourceToString r)
    