module ReserveResource.Logic

open System
open ReserveResource.Domain
open ReserveResource.HardCode

let getUsers() = [teamLeadUser;middleUser;juniorUser]

let getTeams() = [gCloudTeam]

let getReserves() = [gCloud7777ExpiredReserve; gCloud7777ExpiredReserve; gCloud9999ActiveReserve]

let getActiveReserves() =
    getReserves() |> List.filter (fun x -> x.Status = ReservingStatus.Active)

let filterByTeam(reservingResource: ReservingResource, user: User) =
    let team =
        match reservingResource with
        | VM  v -> v.Team
        | Organization o -> o.Team
        | Site s -> s.Team
    user.InTeams |> List.contains team        

let toReservingResourceReserveStates(reservingResource: ReservingResource, dbContext: DbContext) =
    let lastActiveReserve = dbContext.Reserves |> Seq.tryFind (fun r -> r.ReservingResource = reservingResource && r.Status = ReservingStatus.Active)
    let rrs =
          match lastActiveReserve with
          | Some r -> Busy {ReservingResource = reservingResource; ReservingByUser = r.User; StartReserveDate = r.From; ReservingForDate = r.ExpiredIn }
          | None _ -> Free reservingResource
    rrs

let getReservingResourceReserveStates(user:User) = 
    let rr = reservingResources() |> Seq.filter (fun r -> filterByTeam(r, user))
    rr |> Seq.map (fun r -> toReservingResourceReserveStates(r, dbContext)) |> Seq.toArray