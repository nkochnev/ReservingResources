module ReserveResource.Storage.Queries

open ReserveResource.Storage.Context
open System
open System.Linq

let getAccountTeamsQuery (ctx: sql.dataContext, accountId: Guid) =
    query {
        for team in ctx.Dbo.Teams do
            for accountInTeam in team.``dbo.AccountInTeam by Id`` do
                for acc in accountInTeam.``dbo.Accounts by Id`` do
                    where (acc.Id = accountId)
                    select team
        } |> Seq.toArray

let getAvailableToAccountResourcesQuery (ctx: sql.dataContext, accountId: Guid) = 
    query {
        for team in ctx.Dbo.Teams do
            for accountInTeam in team.``dbo.AccountInTeam by Id`` do
                for acc in accountInTeam.``dbo.Accounts by Id`` do
                    for resource in team.``dbo.Resources by Id`` do
                        where (acc.Id = accountId)
                        select (resource, team)
           } |> Seq.toArray

let getActiveReservesQuery (ctx: sql.dataContext, resourcesIds: Guid list) = 
    query {
        for resource in ctx.Dbo.Resources do
            for team in resource.``dbo.Teams by Id`` do
                for reserve in resource.``dbo.Reserves by Id`` do   
                    for account in reserve.``dbo.Accounts by Id`` do
                        where (resourcesIds.Contains(resource.Id) && reserve.Released.IsNone)
                        select (reserve, resource, team, account)
        } |> Seq.toArray

let getTeamByIdQuery(ctx: sql.dataContext, id) = 
    query {
        for team in ctx.Dbo.Teams do
            where (team.Id = id)
            select team
            exactlyOne
    }

let getAccountQuery(ctx: sql.dataContext, telegramLogin) =
    query {
        for account in ctx.Dbo.Accounts do
            where (account.TelegramLogin = telegramLogin)
            select (account)            
    } |> Seq.tryExactlyOne

let getReserveQuery(ctx: sql.dataContext, reserveId) =
    query {
        for reserve in ctx.Dbo.Reserves do
            where (reserve.Id = reserveId)
            select (reserve)            
    } |> Seq.exactlyOne