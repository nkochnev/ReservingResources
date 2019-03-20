open System
open ReserveResource.Domain
open ReserveResource.HardCode
open ReserveResource.Logic

[<EntryPoint>]
let main argv =   
    
//    let gCloudVmActiveReserve = {
//        User = juniorUser;
//        ReservingResource = gCloudVm;
//        From = now.AddMinutes(float -10);
//        ExpiredIn = now.AddDays(float 1);
//        Status = ReservingStatus.Active
//    }
//
//    let reserves = dbContext.Reserves;
//    let dbContext = {dbContext with Reserves = reserves @ [gCloudVmActiveReserve]}        
    
    let currentUser = teamLeadUser
    
    let showReservingResource(rr: ReservingResource) = 
        match rr with
                | VM vm-> "(vm) " + vm.Name
                | Organization org -> "(org) " + org.Name
                | Site s -> "(site) " + s.Name
    
    let showReservingResourceState(rrs: ReservingResourceReserveState) = 
        match rrs with
                | Free f -> showReservingResource(f) + " free"
                | Busy b -> showReservingResource(b.ReservingResource) + " reserved by " + b.ReservingByUser.TelegramLogin + " since from " + b.StartReserveDate.ToString()
        
    let str = getReservingResourceReserveStates(currentUser)
            |> Seq.map showReservingResourceState
            |> String.concat Environment.NewLine
    printfn "%A" str
    
//    Console.Write("enter command: ")
//    let s = Console.ReadLine()
    
//    printfn "%A" s
    
    0 // return an integer exit code