module ReserveResource.Types

open System

type Team =
    { Id : Guid
      Name : string }

type Account =
    { Id : Guid
      Name : string
      TelegramLogin : string
      InTeams : Team list }

type VirtualMachine =
    { Id : Guid
      Name : string
      Team : Team
      DomainName : string }

type Organization =
    { Id : Guid
      Name : string
      Team : Team }

type Site =
    { Id : Guid
      Name : string
      Team : Team
      Url : string }

type Resource =
    | VM of VirtualMachine
    | Organization of Organization
    | Site of Site

type FreeResource = Resource

type ReservingStatus =
    | Active = 1
    | Expired = 2

type ReservingPeriod =
    | For2Hours
    | For6Hours
    | ForDay
    | For3Days

type AddingReserve =
    { Account : Account
      Resource : FreeResource
      ReservingPeriod : ReservingPeriod }

type Reserve =
    { Account : Account
      Resource : Resource
      From : DateTime
      ExpiredIn : DateTime
      Status : ReservingStatus }

type DbContext =
    { mutable Reserves : Reserve list }

type ResourceState =
    | Free of FreeResource
    | Busy of Reserve

type DomainEvents =
    //errors
    | AccountNotFoundByTelegramUser
    | ResourceAlreadyBusy of Reserve
    | ResourceByIdNotFound of Guid
    //events
    | ReserveAdded of Reserve
