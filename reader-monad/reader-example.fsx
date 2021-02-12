#load "./reader-monad.fsx"
open ``Reader-monad``

// Checking that our types are lining up
let test a f = 
    reader {
        let! b = f a
        return b
    }