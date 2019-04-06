namespace ReserveResource.Keyboard

module SelectResourceKeyboard =
    open System.Linq
    open System
    open System.Text.RegularExpressions
    open Funogram.Keyboard.Inline
    open ReserveResource
    open ReserveResource.Types
        
    type FreeResourceType =
        | VM 
        | Organization 
        | Site
    
    type FreeResourceSelection = {
        ResourceType: FreeResourceType
        ResourceId: Guid
        ResourceName: string
    }
        
    let serializeFreeResourceType =
        function
            | VM -> "VM"
            | Organization -> "ORG"
            | Site -> "SITE"
    
    let deserializeFreeResourceType =
        function
            | "VM" -> VM
            | "ORG" -> Organization
            | "SITE" -> Site
            | _ -> failwith "cannot deserialize value to FreeResourceType"
    
    let private freeResourceSelectionToStr frs =
        match frs with
            | Some f -> sprintf "%s;%A" (serializeFreeResourceType f.ResourceType) f.ResourceId
            | None _ -> ""
        
    let private strToFreeResourceSelection (s:string)=
                let parts = s.Split [|';'|]
                let t= parts.[0] |> deserializeFreeResourceType
                let id= parts.[1]|> Guid
                Some {ResourceType = t; ResourceId = id; ResourceName = "1"}
    
    let resourceToState (rr:ReservingResource) : FreeResourceSelection =
        match rr with
            | ReservingResource.VM vm -> {ResourceType = VM; ResourceId = vm.Id; ResourceName = DomainToString.reservingResourceToString rr}
            | ReservingResource.Site site -> {ResourceType = Site; ResourceId = site.Id; ResourceName = DomainToString.reservingResourceToString rr}
            | ReservingResource.Organization org -> {ResourceType = Organization; ResourceId = org.Id; ResourceName = DomainToString.reservingResourceToString rr}
    
//    let serializeReservingPeriod =
//        function
//            | For2Hours -> 2
//            | For6Hours -> 6
//            | ForDay -> 24
//            | For3Days -> 24 * 3
    
    let create botCfg text callback (getFreeResource: seq<FreeResourceReserveState>): KeyboardDefinition<FreeResourceSelection option>={
        Id="CONFIRM"
        DisableNotification=false
        HideAfterConfirm=true
        InitialState=None
        GetMessageText=fun _-> text
        Serialize=freeResourceSelectionToStr
        GetKeysByState=
               fun keys selected->
                   let X= keys.Ignore
                   let B=keys.Change
                   let OK=keys.Confirm
                   keys {     
                          let states = getFreeResource
                                       |> Seq.map resourceToState
                                       |> Seq.toList
                          for state in states do 
                            yield OK(state.ResourceName, Some state)
                   }
        TryDeserialize=fun d-> Some (strToFreeResourceSelection d)
        DoWhenConfirmed=callback
    }