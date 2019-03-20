module ReserveResource.Domain
open System

type Team = {
    Id: Guid
    Name: string
}

type User = {
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
    User: User
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
    ReservingByUser: User
    StartReserveDate: DateTime
    ReservingForDate: DateTime
}

type ReservingResourceReserveState =
    | Free of FreeReservingResourceReserveState
    | Busy of BusyReservingResourceReserveState