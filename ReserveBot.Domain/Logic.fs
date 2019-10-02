module ReserveBot.Logic

open System
open ReserveBot.Types
open ReserveBot.DomainToString

let allPeriods =
    [ ReservingPeriod.For2Hours; ReservingPeriod.For6Hours;
      ReservingPeriod.ForDay; ReservingPeriod.For3Days ]

let getTeam =
    function 
        | VM v -> v.Team
        | Organization o -> o.Team
        | Site s -> s.Team

let getResourceActiveBooking (resource: Resource, bookings: ActiveBooking list) = 
    bookings |> Seq.filter (fun b -> b.Resource = resource) |> Seq.tryHead

let toResourceState (resource, activeBooking) = 
    match activeBooking with 
        | Some b -> Busy (resource, b)
        | None _ -> Free resource

let getResourceStates(resources: Resource list, bookings: ActiveBooking list) : ResourceState list = 
    resources 
    |> Seq.map (fun r -> (r,getResourceActiveBooking(r, bookings)))
    |> Seq.map (fun (r, ab) -> toResourceState(r, ab))
    |> Seq.toList

let getHoursFromReservingPeriod =
    function
    | For2Hours _ -> float 2
    | For6Hours _ -> float 6
    | ForDay _ -> float 24
    | For3Days _ -> float (24 * 3)

let reserveResource(account: Account, resource: FreeResourceState, reservingPeriod: ReservingPeriod) = 
    let activeBooking = {   Id = Guid.NewGuid()
                            Account = account
                            Resource = resource
                            From = DateTime.Now
                            ExpiredIn = reservingPeriod
                                      |> getHoursFromReservingPeriod
                                      |> DateTime.Now.AddHours}
    Result.Ok (BookingAdded(resource, activeBooking))

let releasingResource(busy: BusyResourceState) = 
    let (resource, booking) = busy
    Result.Ok (ResourceReleased ((booking, DateTime.Now)))
    
    
// filtering

let isFreeAndEqualId(state, id) = 
    match state with 
        | Free f -> ((resourceToId f) = id)
        | Busy _ -> false

let filterFreeStateOnly(states) = 
    states |> Seq.choose (fun s -> match s with 
                                    | Free f -> Some f
                                    | Busy _ -> None)

                                    
let filterBusyStateOnly(states) = 
    states |> Seq.choose (fun s -> match s with 
                                    | Free _ -> None
                                    | Busy b -> Some b)

let getFreeResourceById(states, id) : Result<FreeResourceState, DomainEvents> = 
    states 
    |> filterFreeStateOnly 
    |> Seq.filter (fun freeState -> (resourceToId freeState) = id)
    |> Seq.tryHead
    |> function 
        | Some r -> Result.Ok r
        | None _ -> Result.Error (ResourceByIdNotFound id)

let getBusyResourceById(states, id) : Result<BusyResourceState, DomainEvents> = 
    states 
    |> filterBusyStateOnly 
    |> Seq.filter (fun (resource, booking) -> (resourceToId resource) = id)
    |> Seq.tryHead
    |> function 
        | Some busyResourceState -> Result.Ok busyResourceState
        | None _ -> Result.Error (ResourceByIdNotFound id)