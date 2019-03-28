module ReserveResource.Types
open System

type Team = {
    Id: Guid
    Name: string
}

type Account = {
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

type ReservingPeriod =
    | For2Hours
    | For6Hours
    | ForDay
    | For3Days

type AddingReserve = {
    Account: Account
    ReservingResource: ReservingResource
    ExpiredIn: DateTime
    ReservingPeriod: ReservingPeriod
}

type Reserve = {
    Account: Account
    ReservingResource: ReservingResource
    From: DateTime
    ExpiredIn: DateTime
    Status: ReservingStatus
}

type DbContext = {
    mutable Reserves: Reserve list;
}

type FreeResourceReserveState = ReservingResource
type BusyResourceReserveState = Reserve

type ResourceReserveState =
    | Free of FreeResourceReserveState
    | Busy of BusyResourceReserveState
    
type DomainEvents =
    //errors
    | AccountNotFoundByTelegramUser
    | ReservingResourceAlreadyBusy of BusyResourceReserveState
    //events
    | ReserveAdded of Reserve