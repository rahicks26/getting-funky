# Reader Monads

Reader monads are a cleaver way for us to set aside a bit of work to be done until we are ready to provide the resources to do that work. Now I know that is just a bit abstract, so lets dig into a few examples.

If you open up the `./reader-monad.fsx` you will find a generic reader monad that can be used for working with a wide range of types. We will start by digging into how we can use this code, before we really focus on what we are doing here.

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

printStuff ()
|> Reader.run (fun str -> printf "%s" str)
```

An interesting thing happens here we moved the problem from being an input problem, where we tell the caller we need something. Now the caller will have to figure out how to print. If you have spent any time in the OO space you will likely see this as a way to have inversion of control.

Okay, but our solution doubled the amount code required, so can we do any better? Let's come back to that, but first we need to understand what this simpler Reader is doing.
