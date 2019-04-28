module ReserveResource.Types

open System

type Team =
    { Id : Guid
      Name : string }

type Account =
    { Id : Guid
      Name : string
      TelegramLogin : string}

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

type ActiveBooking = {
      Id : Guid
      Account : Account
      Resource : Resource
      From : DateTime
      ExpiredIn : DateTime
}

type ReleasedBooking = ActiveBooking * DateTime

type FreeResourceState = Resource
type BusyResourceState = Resource * ActiveBooking

type ResourceState =
    | Free of FreeResourceState
    | Busy of BusyResourceState

type ReservingPeriod =
    | For2Hours
    | For6Hours
    | ForDay
    | For3Days

type DomainEvents =
    //errors
    | AccountNotFoundByTelegramUser
    | ResourceAlreadyBusy of ActiveBooking
    | ResourceAlreadyFree of Resource
    | ResourceByIdNotFound of Guid
    //events
    | BookingAdded of BusyResourceState
    | ResourceReleased of ReleasedBooking