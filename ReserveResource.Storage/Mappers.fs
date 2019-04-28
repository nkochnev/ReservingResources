module ReserveResource.Storage.Mappers

open ReserveResource.Storage.Context
open ReserveResource.Storage.Queries
open ReserveResource.Types
open FSharp.Data.Sql

let mapToTeam (team: sql.dataContext.``dbo.TeamsEntity``) : Team = 
    { Id = team.Id
      Name = team.Name}

let mapToAccount(a: sql.dataContext.``dbo.AccountsEntity``) = 
    {   Id = a.Id
        Name = a.Name
        TelegramLogin = a.TelegramLogin}

let mapToVm (resource: sql.dataContext.``dbo.ResourcesEntity``, team: sql.dataContext.``dbo.TeamsEntity``) : VirtualMachine =
    { Id = resource.Id
      Name = resource.Name
      DomainName = resource.DomainName.Value
      Team = mapToTeam (team)}

let mapToSite (resource: sql.dataContext.``dbo.ResourcesEntity``, team: sql.dataContext.``dbo.TeamsEntity``) : Site =
    { Id = resource.Id
      Name = resource.Name
      Url = resource.Url.Value
      Team = mapToTeam team}

let mapToOrg (resource: sql.dataContext.``dbo.ResourcesEntity``, team: sql.dataContext.``dbo.TeamsEntity``) : Organization =
    { Id = resource.Id
      Name = resource.Name
      Team = mapToTeam team}

let mapToAvailableToAccountResource(resource: sql.dataContext.``dbo.ResourcesEntity``, team: sql.dataContext.``dbo.TeamsEntity``) : Resource =
    match resource.ResourceType with 
        | 1 -> VM (mapToVm(resource, team))
        | 2 -> Site (mapToSite(resource, team))
        | 3 -> Organization (mapToOrg(resource, team))
        | _ -> failwithf "cannot parse ResourceType %i" resource.ResourceType

let mapToActiveReserve (reserve: sql.dataContext.``dbo.ReservesEntity``, resource: sql.dataContext.``dbo.ResourcesEntity``, team: sql.dataContext.``dbo.TeamsEntity``, account: sql.dataContext.``dbo.AccountsEntity``): ActiveBooking = 
    let resource = reserve.``dbo.Resources by Id`` |> Seq.exactlyOne
    {
         Id=  reserve.Id
         Account = mapToAccount(account)
         Resource = mapToAvailableToAccountResource (resource, team)
         From = reserve.FromDate
         ExpiredIn = reserve.ExpiresIn
    }