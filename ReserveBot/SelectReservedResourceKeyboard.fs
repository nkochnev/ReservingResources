namespace ReserveResource.Keyboard
open ReserveBot

module SelectReservedResourceKeyboard =
    open System
    open Funogram.Keyboard.Inline
    open DomainToString
    open Types
    

    type ReservedResourceSelection =
        { ResourceId : Guid }

    let private resourceSelectionToStr frs =
        match frs with
        | Some f ->
            sprintf "%A" f.ResourceId
        | None _ -> ""

    let private strToFreeResourceSelection (s : string) =
        Some {ResourceId = s |> Guid}

    let resourceToState resource =
        Some { ResourceId = (resourceToId resource)}

    let create botCfg text callback (freeResources : seq<Resource>) : KeyboardDefinition<ReservedResourceSelection option> =
        { Id = "CONFIRM"
          DisableNotification = false
          HideAfterConfirm = true
          InitialState = None
          GetMessageText = fun _ -> text
          Serialize = resourceSelectionToStr
          GetKeysByState =
              fun keys selected ->
                  let X = keys.Ignore
                  let B = keys.Change
                  let OK = keys.Confirm
                  keys {
                      for resource in freeResources |> Seq.toList do
                          yield OK(DomainToString.resourceToString resource, resourceToState resource)
                  }
          TryDeserialize = fun d -> Some(strToFreeResourceSelection d)
          DoWhenConfirmed = callback }
