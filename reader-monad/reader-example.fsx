#load "./reader-monad.fsx"
open ``Reader-monad``

// Shows an example of handling dependencies 
// via partial application
module TestWithPartialApplication =
    let printer print s = print s

    let appliedPrinter = (printf "%s") 

    appliedPrinter "test"

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