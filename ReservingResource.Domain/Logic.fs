module ReserveResource.Logic

open System
open ReserveResource.Rop
open ReserveResource.Types
open ReserveResource.HardCode

let mutable db = {Reserves = [gCloud7777ExpiredReserve; gCloud7777ActiveReserve; gCloud9999ActiveReserve]}

let getAccounts() = [teamLeadAccount;middleAccount;juniorAccount]

let getTeams() = [gCloudTeam]

let getReserves() = db.Reserves

let getActiveReserves() =
    getReserves() |> List.filter (fun x -> x.Status = ReservingStatus.Active)

let reservingResourceInTeam(reservingResource: ReservingResource, account: Account) =
    let team =
        match reservingResource with
        | VM  v -> v.Team
        | Organization o -> o.Team
        | Site s -> s.Team
    account.InTeams |> List.contains team        

let toReservingResourceReserveStates(reservingResource: ReservingResource) =
    let lastActiveReserve = getReserves() |> Seq.tryFind (fun r -> r.ReservingResource = reservingResource && r.Status = ReservingStatus.Active)
    match lastActiveReserve with
        | Some r -> Busy r
        | None _ -> Free reservingResource

let getReservingResourceReserves(account:Account) = 
    reservingResources() |> Seq.filter (fun r -> reservingResourceInTeam(r, account)) |> succeed
    
let mapReservingResourceReserveStates rrr =
    rrr |> Seq.map (fun r -> toReservingResourceReserveStates(r)) |> succeed
    
let getReservingResourceReserveStates(account:Account) =
    getReservingResourceReserves(account) |> bindR mapReservingResourceReserveStates

let isFreeReservingResourceReserveState =
    function
        | Free f -> Some f
        | Busy _ -> None

let filterOnlyFreeReservingResourceReserveState a =
    a |> Seq.choose isFreeReservingResourceReserveState |> succeed

let getFreeReservingResourceReserveStates(account: Account) =
    account |> getReservingResourceReserveStates |> bindR filterOnlyFreeReservingResourceReserveState
    
let createAddingReserve(account) =
    succeed {
        Account = account;
        ReservingResource = gCloudVm;
        ExpiredIn = now.AddDays(float 1);
        ReservingPeriod = ReservingPeriod.For2Hours
    }

let getHoursFromReservingPeriod =
    function
        | For2Hours _ -> float 2
        | For6Hours _ -> float 6
        | ForDay _ -> float 24
        | For3Days _ -> float (24*3)

let allPeriods =
    [ReservingPeriod.For2Hours; ReservingPeriod.For6Hours; ReservingPeriod.ForDay; ReservingPeriod.For3Days]

let mapToReserve(addingReserve: AddingReserve) =
    {
        Account = addingReserve.Account
        ReservingResource = addingReserve.ReservingResource
        From = DateTime.Now
        Status = ReservingStatus.Active
        ExpiredIn = addingReserve.ReservingPeriod |> getHoursFromReservingPeriod |> DateTime.Now.AddHours 
    }

let addReserveToDb reserve =
    let tmp = db.Reserves;
    db <- {db with Reserves = tmp @ [reserve]}

let tryAddReserve(addingReserve: AddingReserve) =
    let state = toReservingResourceReserveStates addingReserve.ReservingResource
    function
        | Busy b -> let state = DomainEvents.ReservingResourceAlreadyBusy b
                    fail state
        | Free f -> let reserve = mapToReserve addingReserve
                    addReserveToDb reserve
                    let event = DomainEvents.ReserveAdded reserve
                    succeedWithMsg () event
                    