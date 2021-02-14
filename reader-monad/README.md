# Reader Monads

Reader monads are a cleaver way for us to set aside a bit of work to be done until we are ready to provide the resources to do that work. Now I know that is just a bit abstract, so lets dig into a few examples. 

## Printer Example

Suppose that we know we need a print function, but we don't know what kind. All we really know is that our function needs to print something. Well this is pretty easy to model.

```fs
let printStuff print =
    let stuff = "Hi I'm stuff"
    // ... do some things to with stuff ...
    print stuff
```

Now we can do this reader monad in much the same way, we just don't need to muddy up our parameters:

```fs
let print = lift1 (fun (out : IPrinter) -> out.Print)

let printStuff () =
    reader {
        let stuff = "Hi I'm stuff"
        // ... do some things to with stuff ...
        do! print stuff
    }
    
testComp "test"
|> run printer
```

An interesting thing happens here we moved the problem from being an input problem, where we tell the caller we need something to an output problem. Now the caller will have to figure out how to print. If you have spent any time in the OO space you will likely see this as a way to have inversion of control. 

## Building a generic Reader Monad

Below is all that you need for the simple case we described

```fs
type Reader<'env, 'out> = 'env -> 'out

[<AutoOpen>]
module Reader =
    let run dep (rm : Reader<_,_>) = rm dep

    let constant (c : 'c) : Reader<_,'c> =  fun _ -> c

    // Monad 
    let bind (rm : Reader<'d, 'a>) (f : 'a -> Reader<'d,'b>): Reader<'d, 'b> =
        fun dep ->  f (rm dep) |> run dep 

    let (>>=) = bind

    // Lifters
    let lift1 (f : 'd -> 'a -> 'out) : 'a -> Reader<'d, 'out> =
        fun a dep -> f dep a

    type ReaderBuilder internal () =
        member _.Zero () = ()
        member _.Bind(m, f) = m >>= f
        member _.Return(v) = constant v
        member _.ReturnFrom(v) = v
        member _.Delay(f) = f ()

    let reader = ReaderBuilder()
```

Now if we want to use this we will need a few types to flush this out

```fs
//
type IPrinter = abstract Print : string -> unit

let printer = 
    { new IPrinter with
        member _.Print s = printfn "%s" s
    }

let print = lift1 (fun (out : IPrinter) -> out.Print)

let testComp msg = 
    reader {
        do! print msg
    }

testComp "test"
|> run printer

```

This example pulls from the example shown in <https://gist.github.com/CarstenKoenig/8f7574e02049a0ec6715>. The plan is to take this a bit further and to explore some common use case.
