module ReserveResource.DomainToString

open System
open ReserveResource.Rop
open ReserveResource.Domain

let stringArrayToString collection =
            collection |> String.concat Environment.NewLine
    
let reservingResourceToString(rr: ReservingResource) = 
        match rr with
                | VM vm-> "(vm) " + vm.Name
                | Organization org -> "(org) " + org.Name
                | Site s -> "(site) " + s.Name

let reservingResourcesToString(reservingResources: ReservingResource[]) =
        reservingResources |> Seq.map reservingResourceToString |> stringArrayToString |> succeed
    
let reservingResourceStateToString(rrs: ReservingResourceReserveState) = 
        match rrs with
                | Free f -> reservingResourceToString(f) + " free"
                | Busy b -> reservingResourceToString(b.ReservingResource) + " reserved by @" + b.ReservingByEmployee.TelegramLogin + " since from " + b.StartReserveDate.ToString()

let reservingResourceStatesToString(rrs: ReservingResourceReserveState[]) =
        rrs |> Seq.map reservingResourceStateToString |> stringArrayToString  |> succeed      

let getMessageFromDomainEvent event =
    match event with
    | UserNotFoundByTelegramAccount _ -> "Пользователь не зарегистрирован"
    | TelegramAccountIsEmpty _ -> "У пользователя нет имени пользователя"
    | ReserveAdded r -> "Бронирование для " + reservingResourceToString r.ReservingResource + " добавлено"
    | ReservingResourceAlreadyBusy r -> "Нельзя забронировать ресурс " + reservingResourceToString r.ReservingResource + ", т.к. ресурс занят"