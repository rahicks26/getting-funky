type Reader<'env, 'out> = 'env -> 'out

[<AutoOpen>]
module Reader =
    let run dep (rm : Reader<_,_>) = rm dep

    let constant (c : 'c) : Reader<_,'c> =  fun _ -> c

    // Functor
    let map (f : 'a -> 'b) (rm : Reader<'d, 'a>): Reader<'d,'b> =
        rm >> f

    let (<?>) = map

    // Applicative-functor
    let apply (f : Reader<'d, 'a->'b>) (rm : Reader<'d, 'a>): Reader<'d, 'b> =
        fun dep ->
            let f' = run dep f
            let a  = run dep rm
            f' a

    let (<*>) = apply

    // Monad 
    let bind (rm : Reader<'d, 'a>) (f : 'a -> Reader<'d,'b>): Reader<'d, 'b> =
        fun dep ->  f (rm dep) |> run dep 

    let (>>=) = bind

    type ReaderBuilder internal () =
        member _.Bind(m, f) = m >>= f
        member _.Return(v) = constant v
        member _.ReturnFrom(v) = v
        member _.Delay(f) = f ()

    let reader = ReaderBuilder()