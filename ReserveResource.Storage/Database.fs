module ReserveResource.Storage.Database

open ReserveResource.Types
open ReserveResource.Storage.Queries
open ReserveResource.Storage.Context
open ReserveResource.Storage.Mappers
open ReserveResource.DomainToString
open FSharp.Data.Sql
open System

let getAvailableToAccountResources (ctx: sql.dataContext, account: Account) : Resource list =    
    getAvailableToAccountResourcesQuery(ctx, account.Id)
    |> Seq.map (fun (r,t) -> mapToAvailableToAccountResource(r, t))
    |> Seq.toList

let getAccount (ctx: sql.dataContext, telegramLogin) = 
    match getAccountQuery(ctx, telegramLogin) with 
        | Some accountEntity -> Result.Ok (mapToAccount accountEntity)
        | None _ -> Result.Error AccountNotFoundByTelegramUser

let getActiveBookings(ctx: sql.dataContext, resourceIds : Guid list) = 
    getActiveReservesQuery(ctx, resourceIds)
    |> Seq.map (fun (reserve, resource, team, account) -> mapToActiveReserve (reserve, resource, team, account))
    |> Seq.toList

let getResourceById (ctx: sql.dataContext, account : Account, id) =
    let resource = getAvailableToAccountResources (ctx, account) 
                    |> Seq.filter (fun r -> (resourceToId r) = id) 
                    |> Seq.tryExactlyOne
    match resource with
    | Some r -> Result.Ok r
    | None -> Result.Error(ResourceByIdNotFound id)
    
let addBooking(ctx: sql.dataContext, booking: ActiveBooking) =
    let reserve = ctx.Dbo.Reserves.Create()
    reserve.AccountId <- booking.Account.Id
    reserve.ExpiresIn <- booking.ExpiredIn
    reserve.FromDate <- booking.From
    reserve.ResourceId <- resourceToId booking.Resource
    ctx.SubmitUpdates()

let releaseBooking(ctx: sql.dataContext, booking: ActiveBooking, releasedDate: DateTime) = 
    let reserve = getReserveQuery(ctx, booking.Id)
    reserve.Released <- Some releasedDate
    ctx.SubmitUpdates()