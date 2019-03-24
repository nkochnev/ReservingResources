module ReserveResource.Domain
open System

type Team = {
    Id: Guid
    Name: string
}

type Employee = {
    Id: Guid
    Name: string
    TelegramLogin: string
    InTeams: Team list
}

type VirtualMachine = {
    Id: Guid
    Name: string
    Team: Team
    DomainName: string
}

type Organization = {
    Id: Guid
    Name: string
    Team: Team
}

type Site = {
    Id: Guid
    Name: string
    Team: Team
    Url: string
}

type ReservingStatus =
    | Active = 1
    | Expired = 2

type ReservingResource =
    | VM of VirtualMachine
    | Organization of Organization
    | Site of Site
    
type Reserve = {
    Employee: Employee
    ReservingResource: ReservingResource
    From: DateTime
    ExpiredIn: DateTime
    Status: ReservingStatus
}

type DbContext = {
    mutable Reserves: Reserve list;
}

type FreeReservingResourceReserveState = ReservingResource

type BusyReservingResourceReserveState = {
    ReservingResource: ReservingResource
    ReservingByEmployee: Employee
    StartReserveDate: DateTime
    ReservingForDate: DateTime
}

type ReservingResourceReserveState =
    | Free of FreeReservingResourceReserveState
    | Busy of BusyReservingResourceReserveState
    
type DomainEvents =
    //errors
    | UserNotFoundByTelegramAccount
    | TelegramAccountIsEmpty
    //events
    | ReserveAdded of Reserve
    
let getMessageFromDomainEvent event =
    match event with
    | UserNotFoundByTelegramAccount _ -> "Пользователь не зарегистрирован"
    | TelegramAccountIsEmpty _ -> "У пользователя нет имени пользователя"
    | ReserveAdded r -> "Бронирование для " + r.Employee.Name + " добавлено"