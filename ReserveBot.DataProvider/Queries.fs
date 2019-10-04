module ReserveBot.DataProvider.Queries

open System
open System.Linq
open ReserveBot.Storage

let selectMany (ab:'a -> 'b seq) (abc:'a -> 'b -> 'c) input =
    input 

let getAccountTeamsQuery (ctx: ReserveBotContext, accountId: Guid) =
    ctx.AccountInTeams.Where(fun x -> x.AccountId = accountId).Select(fun x -> x.Team)

let getAvailableToAccountResourcesQuery (ctx: ReserveBotContext, accountId: Guid) =
    let teamIds = getAccountTeamsQuery(ctx, accountId).Select(fun c -> c.Id).ToArray();
    ctx.Resources.Where(fun x -> teamIds.Contains x.TeamId).ToList()

let getActiveReservesQuery (ctx: ReserveBotContext, resourcesIds: Guid list) =
    ctx.Reserves.Where(fun x-> x.Released.HasValue = false && resourcesIds.Contains(x.ResourceId))
    
let getTeamByIdQuery(ctx: ReserveBotContext, id) = 
    ctx.Teams.SingleOrDefault(fun x-> x.Id = id) |> Option.ofObj

let getAccountQuery(ctx: ReserveBotContext, telegramLogin: string) =
    ctx.Accounts.SingleOrDefault(fun x-> x.TelegramLogin.ToLower() = telegramLogin.ToLower()) |> Option.ofObj

let getReserveQuery(ctx: ReserveBotContext, reserveId) =
    ctx.Reserves.Single(fun x-> x.Id = reserveId)