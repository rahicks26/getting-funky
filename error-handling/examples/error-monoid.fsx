#r "nuget: FsToolkit.ErrorHandling"

open System
open FsToolkit.ErrorHandling

type ErrorList =
    private
    | ErrorList of obj list

module ErrorList =
    let toList (ErrorList errs) = errs

    let fromError error =
        match box error with
        | :? ErrorList as error -> error
        | :? seq<obj> as errors ->  errors |> List.ofSeq |> ErrorList
        | error -> error |> List.singleton |> ErrorList

    let append error (ErrorList errorList) =
        error
        |> fromError
        |> toList
        |> List.append errorList
        |> ErrorList

    // Thinking through this one a bit more...
    let filter (ErrorList errs) =
        let errors =
            errs
            |> List.filter (fun err -> err.GetType() = typeof<'Error>)
            |> List.map unbox<'Error>

        let remaining =
            errs
            |> List.filter (fun err -> err.GetType() <> typeof<'Error>)
            |> ErrorList

        (errors, remaining)

    let toString (ErrorList errs) =
        match errs with
        | [] -> ""
        | [ head ] -> head.ToString()
        | ls ->
            let msgs =
                ls |> List.map (fun err -> err.ToString())

            let msg = String.Join("/n/t-", msgs)

            $"The following errors have been found:\n {msg}"

type Outcome<'a> =
    | Success of 'a
    | Failure of ErrorList

module Outcome =
    let fromResult =
        function
        | Ok v -> v |> Success
        | Error err -> err |> ErrorList.fromError |> Failure

    let toResult =
        function 
        | Success v -> v |> Ok
        | Failure err -> err |> Error

    let fromError err = err |> ErrorList.fromError |> Failure

    let map (f: 'a -> 'b)  = 
        function 
        | Success a -> f a |> Success
        | Failure err -> err |> Failure

    let bind o f =
        match o with
        | Success v -> v |> f 
        | Failure err -> err |> Failure

    module Operators =
        let (<?>) = map
        let (>>=) = bind

[<AutoOpen>]
module OutcomeCE =
    open Outcome.Operators

    type OutcomeBuilder() =
        member _.Zero() = ()
        member inline _.Bind(m, f) = m >>= f
        member _.Return(v) = Success v
        member _.ReturnFrom(v) = v
        member _.Delay(f) = f ()

        member _.MergeSources (o1, o2) =
            match (o1, o2) with
            | Success a, Success b -> (a, b) |> Success
            | Failure (err), Failure (ErrorList errs) -> err |> ErrorList.append errs |> Failure
            | Failure err, _ -> err |> Failure
            | _, Failure err -> err |> Failure

        member inline _.Source (o:'a Outcome) = o

    let outcome = OutcomeBuilder()

     

[<AutoOpen>]
module OutcomeCEExtensions =
    type OutcomeBuilder with 
        member inline _.Source(r: Result<'a,'err>) = r |> Outcome.fromResult

// Code example 
module test =

    type ErrorA = ErrorA
    type ErrorB = ErrorB
    type ErrorC = ErrorC

    let testerA () = ErrorA |> Error
    let testerB () = ErrorB |> Outcome.fromError
    let testerC () = ErrorC |> Error |> Outcome.fromResult


    let testerStr () = "test" |> Error

    let testerErrorList () = ["1"; "2"] |> Error

    type ErrorT = ServerError of string list

    let temp tester =
        outcome {
            let! a = testerA()
            and! b = testerB()

            let! c = testerC()
            and! s = testerStr()
            and! d = testerErrorList() |> Result.mapError ServerError
            return (a, b, c, d, s)
        } |> Outcome.toResult
