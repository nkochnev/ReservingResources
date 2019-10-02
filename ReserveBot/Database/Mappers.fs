module ReserveResource.Database.Mappers

open ReserveBot.Storage.Tables
open ReserveBot.Types

let mapToTeam (team: TeamEntity) : Team = 
    { Id = team.Id
      Name = team.Name}

let mapToAccount(a: AccountEntity) = 
    {   Id = a.Id
        Name = a.Name
        TelegramLogin = a.TelegramLogin}

let mapToVm (resource: ResourceEntity, team: TeamEntity) : VirtualMachine =
    { Id = resource.Id
      Name = resource.Name
      DomainName = resource.DomainName
      Team = mapToTeam (team)}

let mapToSite (resource: ResourceEntity, team: TeamEntity) : Site =
    { Id = resource.Id
      Name = resource.Name
      Url = resource.Url
      Team = mapToTeam team}

let mapToOrg (resource: ResourceEntity, team: TeamEntity) : Organization =
    { Id = resource.Id
      Name = resource.Name
      Team = mapToTeam team}

let mapToAvailableToAccountResource(resource: ResourceEntity) : Resource =
    match resource.ResourceType with 
        | ResourceType.VirtualMachine -> VM (mapToVm(resource, resource.Team))
        | ResourceType.Site -> Site (mapToSite(resource, resource.Team))
        | ResourceType.Organization -> Organization (mapToOrg(resource, resource.Team))
        | _ -> failwithf "cannot parse ResourceType %i" ((int32)resource.ResourceType)

let mapToActiveReserve (reserve: ReserveEntity): ActiveBooking = 
    let resource = reserve.Resource
    {
         Id=  reserve.Id
         Account = mapToAccount(reserve.ReservedByAccount)
         Resource = mapToAvailableToAccountResource resource
         From = reserve.From
         ExpiredIn = reserve.ExpiredIn
    }