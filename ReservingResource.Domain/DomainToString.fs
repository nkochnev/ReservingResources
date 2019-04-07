module ReserveResource.DomainToString

open System
open ReserveResource.Types

let stringArrayToString collection =
    collection |> String.concat Environment.NewLine

let resourceToString =
    function
    | VM vm -> "(vm) " + vm.Name
    | Organization org -> "(org) " + org.Name
    | Site s -> "(site) " + s.Name

let resourceToName =
    function
    | VM vm -> vm.Name
    | Organization org -> org.Name
    | Site s -> s.Name

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
    | Free f -> resourceToString f + " free"
    | Busy b ->
        resourceToString (b.Resource) + " reserved by @"
        + b.Account.TelegramLogin + " since from " + b.From.ToString() + " for "
        + b.ExpiredIn.ToString()

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
        "Бронирование для " + resourceToString r.Resource + " добавлено"
    | ResourceByIdNotFound id ->
        "Ресурс с идентификатором " + id.ToString() + " не найден"
    | ResourceAlreadyBusy r ->
        "Нельзя забронировать ресурс " + resourceToString r.Resource
        + ", т.к. ресурс занят"
