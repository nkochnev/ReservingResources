namespace ReserveResource.Keyboard

module SelectFreeResourceKeyboard =
    open System
    open Funogram.Keyboard.Inline
    open ReserveResource
    open ReserveResource.TelegramBotInfrastructure
    open ReserveResource.Types
    
    type FreeResourceSelection =
        { ResourceId : Guid
          Period : ReservingPeriod }
    
    let deserializeReservingPeriod =
        function
        | 2 -> For2Hours
        | 6 -> For6Hours
        | 24 -> ForDay
        | 72 -> For3Days
        | _ -> failwith "deserializeReservingPeriod error"

    let serializeReservingPeriod =
        function
        | For2Hours -> 2
        | For6Hours -> 6
        | ForDay -> 24
        | For3Days -> 24 * 3

    let private freeResourceSelectionToStr frs =
        match frs with
        | Some f ->
            sprintf "%i;%A" (serializeReservingPeriod f.Period) f.ResourceId
        | None _ -> ""

    let private strToFreeResourceSelection (s : string) =
        let parts = s.Split [| ';' |]
        let period =
            parts.[0]
            |> int
            |> deserializeReservingPeriod
        let id = parts.[1] |> Guid
        Some { ResourceId = id
               Period = period }

    let resourceToState rr period =
        Some { ResourceId = (DomainToString.resourceToId rr)
               Period = period }

    let create botCfg text callback (freeResources : seq<Resource>) : KeyboardDefinition<FreeResourceSelection option> =
        { Id = "CONFIRM"
          DisableNotification = false
          HideAfterConfirm = true
          InitialState = None
          GetMessageText = fun _ -> text
          Serialize = freeResourceSelectionToStr
          GetKeysByState =
              fun keys _ ->
                  let X = keys.Ignore
                  let B = keys.Change
                  let OK = keys.Confirm
                  keys {
                      for resource in freeResources |> Seq.toList do
                          yield X(DomainToString.resourceToString resource)
                          yield! Logic.allPeriods
                                 |> Seq.map (fun period ->
                                        let state =
                                            resourceToState resource period
                                        let btnName =
                                            (DomainToString.reservingPeriodToString
                                                 period)
                                        OK(btnName, state))
                  }
          TryDeserialize = fun d -> Some(strToFreeResourceSelection d)
          DoWhenConfirmed = callback }
