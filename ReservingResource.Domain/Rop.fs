module ReserveResource.Rop

// Railway-Oriented-Programming
// from https://github.com/swlaschin/Railway-Oriented-Programming-Example/blob/master/src/FsRopExample/Rop.fs#L23

/// A Result is a success or failure
/// The Success case has a success value, plus a list of messages
/// The Failure case has just a list of messages
type RopResult<'TSuccess, 'TMessage> =
    | Success of 'TSuccess * 'TMessage list
    | Failure of 'TMessage list

/// create a Success with no messages
let succeed x =
    Success (x,[])

/// create a Success with a message
let succeedWithMsg x msg =
    Success (x,[msg])

/// create a Failure with a message
let fail msg =
    Failure [msg]

/// A function that applies either fSuccess or fFailure 
/// depending on the case.
let either fSuccess fFailure = function
    | Success (x,msgs) -> fSuccess (x,msgs) 
    | Failure errors -> fFailure errors
    
/// merge messages with a result
let mergeMessages msgs result =
    let fSuccess (x,msgs2) = 
        Success (x, msgs @ msgs2) 
    let fFailure errs = 
        Failure (errs @ msgs) 
    either fSuccess fFailure result

/// given a function that generates a new RopResult
/// apply it only if the result is on the Success branch
/// merge any existing messages with the new result
let bindR f result =
    let fSuccess (x,msgs) = 
        f x |> mergeMessages msgs
    let fFailure errs = 
        Failure errs 
    either fSuccess fFailure result
   

/// given an RopResult, call a unit function on the success branch
/// and pass thru the result
let successTee f result = 
    let fSuccess (x,msgs) = 
        f (x,msgs)
        Success (x,msgs) 
    let fFailure errs = Failure errs 
    either fSuccess fFailure result

/// given an RopResult, call a unit function on the failure branch
/// and pass thru the result
let failureTee f result = 
    let fSuccess (x,msgs) = Success (x,msgs) 
    let fFailure errs = 
        f errs
        Failure errs 
    either fSuccess fFailure result