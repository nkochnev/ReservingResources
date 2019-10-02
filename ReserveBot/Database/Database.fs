module ReserveResource.Database.Database

open ReserveBot.Storage
open ReserveBot.Types
open System
open ReserveResource.Database.Queries
open ReserveResource.Database.Mappers
open ReserveBot.DomainToString
open ReserveBot.Storage.Tables

let getAvailableToAccountResources (ctx: ReserveBotContext, account: Account) : Resource list =    
    getAvailableToAccountResourcesQuery(ctx, account.Id)
    |> Seq.map (mapToAvailableToAccountResource)
    |> Seq.toList

let getAccount (ctx: ReserveBotContext, telegramLogin) = 
    match getAccountQuery(ctx, telegramLogin) with 
        | Some accountEntity -> Result.Ok (mapToAccount accountEntity)
        | None _ -> Result.Error AccountNotFoundByTelegramUser

let getActiveBookings(ctx: ReserveBotContext, resourceIds : Guid list) = 
    getActiveReservesQuery(ctx, resourceIds)
    |> Seq.map (fun (reserve) -> mapToActiveReserve (reserve))
    |> Seq.toList

let getResourceById (ctx: ReserveBotContext, account : Account, id) =
    let resource = getAvailableToAccountResources (ctx, account) 
                    |> Seq.filter (fun r -> (resourceToId r) = id) 
                    |> Seq.tryExactlyOne
    match resource with
    | Some r -> Result.Ok r
    | None -> Result.Error(ResourceByIdNotFound id)
    
let addBooking(ctx: ReserveBotContext, booking: ActiveBooking) =
    let reserve = new ReserveEntity()
    reserve.ReservedByAccountId <- booking.Account.Id
    reserve.ExpiredIn <- booking.ExpiredIn
    reserve.From <- booking.From
    reserve.ResourceId <- resourceToId booking.Resource
    ctx.Reserves.Add(reserve) |> ignore;
    ctx.SaveChanges()

let releaseBooking(ctx: ReserveBotContext, booking: ActiveBooking, releasedDate: DateTime) = 
    let reserve = getReserveQuery(ctx, booking.Id)
    reserve.Released <- new Nullable<DateTime>(releasedDate)
    ctx.SaveChanges()