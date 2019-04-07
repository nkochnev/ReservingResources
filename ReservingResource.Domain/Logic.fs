module ReserveResource.Logic

open System
open System
open ReserveResource.Types
open ReserveResource.DomainToString
open ReserveResource.HardCode

let mutable db =
    { Reserves =
          [ gCloud7777ExpiredReserve; gCloud7777ActiveReserve;
            gCloud9999ActiveReserve ] }
let getAccounts() = [ teamLeadAccount; middleAccount; juniorAccount ]
let getTeams() = [ gCloudTeam ]
let getReserves() = db.Reserves
let getActiveReserves() =
    getReserves() |> List.filter (fun x -> x.Status = ReservingStatus.Active)

let isResourceInTeam resource account =
    let team =
        match resource with
        | VM v -> v.Team
        | Organization o -> o.Team
        | Site s -> s.Team
    account.InTeams |> List.contains team

let toResourceStates resource =
    let lastActiveReserve =
        getReserves()
        |> Seq.tryFind
               (fun r ->
               r.Resource = resource && r.Status = ReservingStatus.Active)
    match lastActiveReserve with
    | Some r -> Busy r
    | None _ -> Free resource

let getResources (account : Account) =
    resources() |> Seq.filter (fun r -> isResourceInTeam r account)

let getResourceById (account : Account, id) =
    let resource =
        getResources (account) |> Seq.tryFind (fun r -> (resourceToId r) = id)
    match resource with
    | Some r -> Result.Ok r
    | None -> Result.Error(ResourceByIdNotFound id)

let getResourceStates account =
    getResources (account) |> Seq.map toResourceStates

let isFreeResourceState =
    function
    | Free f -> Some f
    | Busy _ -> None
    
let isReservedByAccountResourceState account =
    function
    | Free f -> None
    | Busy b ->
        if (b.Account = account)
        then
            Some b
        else
            None

let getFreeResourceStates account =
    account
    |> getResourceStates
    |> Seq.choose isFreeResourceState
    
let getReservedResoures account =
    account
    |> getResourceStates
    |> Seq.choose (account |> isReservedByAccountResourceState)
    |> Seq.map (fun a -> a.Resource)

let checkResourceIsFree resource =
    let state = toResourceStates resource
    match state with
    | Busy b -> Result.Error(DomainEvents.ResourceAlreadyBusy b)
    | Free f -> Result.Ok f

let createAddingReserve account resource period =
    checkResourceIsFree resource
    |> Result.map (fun freeResource ->
           { Account = account
             Resource = freeResource
             ReservingPeriod = period })

let getHoursFromReservingPeriod =
    function
    | For2Hours _ -> float 2
    | For6Hours _ -> float 6
    | ForDay _ -> float 24
    | For3Days _ -> float (24 * 3)

let allPeriods =
    [ ReservingPeriod.For2Hours; ReservingPeriod.For6Hours;
      ReservingPeriod.ForDay; ReservingPeriod.For3Days ]

let mapToReserve (addingReserve : AddingReserve) =
    { Id = Guid.NewGuid()
      Account = addingReserve.Account
      Resource = addingReserve.Resource
      From = DateTime.Now
      Status = ReservingStatus.Active
      ExpiredIn =
          addingReserve.ReservingPeriod
          |> getHoursFromReservingPeriod
          |> DateTime.Now.AddHours }

let changeReserveInDb (reserve:Reserve) =
    let newReserves = db.Reserves |> Seq.map (fun r -> if r.Id = reserve.Id then reserve else r) |> Seq.toList
    db <- { db with Reserves = newReserves }

let addReserveToDb reserve =
    db <- { db with Reserves = (db.Reserves) @ [ reserve ] }

let tryAddReserve (addingReserve : AddingReserve) : Result<DomainEvents, DomainEvents> =
    let reserve = mapToReserve addingReserve
    addReserveToDb reserve
    Result.Ok(DomainEvents.ReserveAdded reserve)

let releaseResource resource =
    let state = toResourceStates resource
    match state with
        | Free f -> Result.Error (DomainEvents.ResourceAlreadyFree f)
        | Busy b ->
            let newReserve = {b with Status = ReservingStatus.Expired}
            changeReserveInDb newReserve
            Result.Ok (DomainEvents.ResourceReleased newReserve)