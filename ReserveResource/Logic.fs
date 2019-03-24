module ReserveResource.Logic

open System
open ReserveResource.Rop
open ReserveResource.Domain
open ReserveResource.HardCode

let mutable db = {Reserves = [gCloud7777ExpiredReserve; gCloud7777ActiveReserve; gCloud9999ActiveReserve]}

let getUsers() = [teamLeadUser;middleUser;juniorUser]

let getTeams() = [gCloudTeam]

let getReserves() = db.Reserves

let getActiveReserves() =
    getReserves() |> List.filter (fun x -> x.Status = ReservingStatus.Active)

let reservingResourceInTeam(reservingResource: ReservingResource, user: Employee) =
    let team =
        match reservingResource with
        | VM  v -> v.Team
        | Organization o -> o.Team
        | Site s -> s.Team
    user.InTeams |> List.contains team        

let toReservingResourceReserveStates(reservingResource: ReservingResource) =
    let lastActiveReserve = getReserves() |> Seq.tryFind (fun r -> r.ReservingResource = reservingResource && r.Status = ReservingStatus.Active)
    match lastActiveReserve with
        | Some r -> Busy {ReservingResource = reservingResource; ReservingByEmployee = r.Employee; StartReserveDate = r.From; ReservingForDate = r.ExpiredIn }
        | None _ -> Free reservingResource

let getReservingResourceReserveStates(user:Employee) =
     reservingResources()
     |> Seq.filter (fun r -> reservingResourceInTeam(r, user))
     |> Seq.map (fun r -> toReservingResourceReserveStates(r))
     |> Seq.toArray
     |> succeed

let createReserve(user) =
    succeed {
        Employee = user;
        ReservingResource = gCloudVm;
        From = DateTime.Now;
        ExpiredIn = now.AddDays(float 1);
        Status = ReservingStatus.Active
    }    

let addReserve(addingReserve) =
    let tmp = db.Reserves;
    db <- {db with Reserves = tmp @ [addingReserve]}
    let event = DomainEvents.ReserveAdded addingReserve
    succeedWithMsg () event