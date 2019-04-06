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

let resourceInTeam(resource: Resource, account: Account) =
    let team =
        match resource with
        | VM  v -> v.Team
        | Organization o -> o.Team
        | Site s -> s.Team
    account.InTeams |> List.contains team        

let toResourceStates(resource: Resource) =
    let lastActiveReserve = getReserves() |> Seq.tryFind (fun r -> r.Resource = resource && r.Status = ReservingStatus.Active)
    match lastActiveReserve with
        | Some r -> Busy r
        | None _ -> Free resource

let getResourceReserves(account:Account) = 
    resources() |> Seq.filter (fun r -> resourceInTeam(r, account)) |> succeed
    
let mapResourceStates rrr =
    rrr |> Seq.map (fun r -> toResourceStates(r)) |> succeed
    
let getResourceStates(account:Account) =
    getResourceReserves(account) |> bindR mapResourceStates

let isFreeResourceState =
    function
        | Free f -> Some f
        | Busy _ -> None

let filterOnlyFreeResourceState a =
    a |> Seq.choose isFreeResourceState |> succeed

let getFreeResourceStates(account: Account) =
    account |> getResourceStates |> bindR filterOnlyFreeResourceState
    
let createAddingReserve(account) =
    succeed {
        Account = account;
        Resource = gCloudVm;
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
        Resource = addingReserve.Resource
        From = DateTime.Now
        Status = ReservingStatus.Active
        ExpiredIn = addingReserve.ReservingPeriod |> getHoursFromReservingPeriod |> DateTime.Now.AddHours 
    }

let addReserveToDb reserve =
    let tmp = db.Reserves;
    db <- {db with Reserves = tmp @ [reserve]}

let tryAddReserve(addingReserve: AddingReserve) =
    let state = toResourceStates addingReserve.Resource
    function
        | Busy b -> let state = DomainEvents.ResourceAlreadyBusy b
                    fail state
        | Free f -> let reserve = mapToReserve addingReserve
                    addReserveToDb reserve
                    let event = DomainEvents.ReserveAdded reserve
                    succeedWithMsg () event
                    