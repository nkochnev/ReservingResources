module ReserveResource.DomainToString

open System
open ReserveResource.Rop
open ReserveResource.Domain

let stringArrayToString collection =
            collection |> String.concat Environment.NewLine
    
let reservingResourceToString = 
        function
                | VM vm-> "(vm) " + vm.Name
                | Organization org -> "(org) " + org.Name
                | Site s -> "(site) " + s.Name

let reservingResourceToName(rr: ReservingResource) = 
        match rr with
                | VM vm-> vm.Name
                | Organization org -> org.Name
                | Site s -> s.Name

let reservingResourceToId = 
        function
                | VM vm-> vm.Id
                | Organization org -> org.Id
                | Site s -> s.Id

let reservingResourcesToString(reservingResources: ReservingResource[]) =
        reservingResources |> Seq.map reservingResourceToString |> stringArrayToString |> succeed
    
let reservingResourceStateToString = 
        function
                | Free f -> reservingResourceToString f + " free"
                | Busy b -> reservingResourceToString(b.ReservingResource) + " reserved by @" + b.Employee.TelegramLogin
                            + " since from " + b.From.ToString()
                            + " for " + b.ExpiredIn.ToString()

let reservingResourceStatesToString(rrs: seq<ResourceReserveState>) =
        rrs |> Seq.map reservingResourceStateToString |> stringArrayToString  |> succeed      

let reservingPeriodToString =
        function
         | ReservingPeriod.For2Hours _ -> "2 часа"
         | ReservingPeriod.For6Hours _ -> "6 часов"
         | ReservingPeriod.ForDay _ -> "1 день"
         | ReservingPeriod.For3Days _ -> "3 дня"

let getMessageFromDomainEvent =
    function
    | UserNotFoundByTelegramAccount _ -> "Пользователь не зарегистрирован"
    | TelegramAccountIsEmpty _ -> "У пользователя нет имени пользователя"
    | ReserveAdded r -> "Бронирование для " + reservingResourceToString r.ReservingResource + " добавлено"
    | ReservingResourceAlreadyBusy r -> "Нельзя забронировать ресурс " + reservingResourceToString r.ReservingResource + ", т.к. ресурс занят"