#load "../reader-monad.fsx"
open ``Reader-monad``

// Explores how to handle dependencies
// via Reader Monad with interfaces
module TestWithInterface =
    // Dependency
    type IPrinter = abstract Print : string -> unit

    // Implementation
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

// Explores how to handle dependencies
// via Reader Monad with records
module TestWithRecord =
    // Dependency
    type Printer = { 
        print : string -> unit
    }

    // Implementation
    let printer = {  
        print = fun s -> printfn "%s" s; 
    }

    let print = lift1 (fun (out : Printer) -> out.print)

    let testComp msg = 
        reader {
            do! print msg
        }

    testComp "test"
    |> run printer