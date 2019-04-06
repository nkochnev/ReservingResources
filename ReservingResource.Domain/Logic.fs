module ReserveResource.Logic

open ReserveResource
open System
open ReserveResource.Types
open ReserveResource.DomainToString
open ReserveResource.HardCode

let mutable db = {Reserves = [gCloud7777ExpiredReserve; gCloud7777ActiveReserve; gCloud9999ActiveReserve]}

let getAccounts() = [teamLeadAccount;middleAccount;juniorAccount]

let getTeams() = [gCloudTeam]

let getReserves() = db.Reserves

let getActiveReserves() =
    getReserves() |> List.filter (fun x -> x.Status = ReservingStatus.Active)

let isResourceInTeam(resource: Resource, account: Account) =
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

let getResources(account:Account) = 
    resources() |> Seq.filter (fun r -> isResourceInTeam(r, account))

let getResourceById(account:Account, id) = 
    let resource = getResources(account) |> Seq.tryFind (fun r -> (resourceToId r) = id)
    match resource with
        | Some r -> Result.Ok r
        | None -> Result.Error (ResourceByIdNotFound id)
    
let getResourceStates(account:Account) =
    getResources(account) |> Seq.map toResourceStates

let isFreeResourceState =
    function
        | Free f -> Some f
        | Busy _ -> None

let getFreeResourceStates(account: Account) =
    account |> getResourceStates |> Seq.choose isFreeResourceState
  
let checkResourceIsFree(resource: Resource) =
    let state = toResourceStates resource
    match state with
        | Busy b -> Result.Error (DomainEvents.ResourceAlreadyBusy b)
        | Free f -> Result.Ok f
    
let createAddingReserve(account: Account, resource: Resource) =
    checkResourceIsFree resource
    |> Result.map (fun freeResource -> {
                        Account = account;
                        Resource = freeResource;
                        ReservingPeriod = ReservingPeriod.For2Hours
                    })

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
    
let tryAddReserve(addingReserve: AddingReserve) : Result<DomainEvents, DomainEvents> =    
    let reserve = mapToReserve addingReserve
    addReserveToDb reserve
    Result.Ok (DomainEvents.ReserveAdded reserve)