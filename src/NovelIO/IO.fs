﻿(*
   Copyright 2015-2016 Philip Curzon

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*)

namespace NovelFS.NovelIO

open System.IO
open System.Net

/// A value of type IO<'a> represents an action which, when performed (e.g. by calling the IO.run function), does some I/O which results in a value of type 'a.
type IO<'a> = 
    private 
    |Return of 'a
    |SyncIO of (unit -> IO<'a> )
    |AsyncIO of (Async<IO<'a>>)

/// Pure IO Functions
module IO =
    // ------- RUN ------- //

    let rec private runUntilAsync io =
        match io with
        |Return a -> async.Return <| Return a
        |SyncIO f -> runUntilAsync (f())
        |AsyncIO a -> a

    let rec private runAsyncIO io =
        match io with
        |Return a -> async.Return a
        |SyncIO f -> runAsyncIO <| f()
        |AsyncIO a -> async.Bind (a, runAsyncIO)

    let rec private  runRec (io : IO<'a>) : Async<'a> =
        async{
            let! io' = runUntilAsync io
            match io' with
            |Return res -> return res
            |_ -> return! runRec io'
        }

    /// Runs the IO actions and evaluates the result
    let run io = runRec io |> Async.RunSynchronously

    // ------- MONAD ------- //

    /// Return a value as an IO action
    let return' x = Return x

    /// Creates an IO action from an effectful computation, this simply takes a side effecting function and brings it into IO
    let fromEffectful f = SyncIO (fun () -> return' <| f())

    /// Monadic bind for IO action, this is used to combine and sequence IO actions
    let bind x f =
        let rec bindRec x' =
            match x' with
            |Return a -> f a
            |SyncIO (g) -> SyncIO (fun () ->  bindRec <| g())
            |AsyncIO (a) -> AsyncIO (async.Bind(a, async.Return << bindRec))
        bindRec x

    /// Lift an async computation into IO
    let liftAsync a = AsyncIO <| async.Bind(a, async.Return << Return)
            
    /// Removes a level of IO structure
    let join x = bind x id

    /// Computation Expression builder for IO actions
    type IOBuilder() =
        /// Return a value as an IO action
        member this.Return a : IO<'a> = return' a
        /// Bare return for IO values
        member this.ReturnFrom a : IO<'a> = a
        /// Monadic bind for IO action, this is used to combine and sequence IO action
        member this.Bind (x : IO<'a>, f : 'a -> IO<'b>) = bind x f
        /// Delays a function of type unit -> IO<'a> as an IO<'a>
        member this.Delay f : IO<'a> = f()
        /// Combine an IO action of type unit an IO action of type 'a into a combined IO action of type 'a
        member this.Combine(f1, f2) =
            bind f1 (fun () -> f2)
        /// The zero IO action
        member this.Zero() = return' ()
        /// Definition of while loops within IO computation expressions
        member this.While(guard, body) =
            match guard() with
            |false -> this.Zero()
            |true -> bind (body) (fun () -> this.While(guard, body))

    // For use within this module, later we need to define this again in an auto-open module
    let private io = IOBuilder()

    // ------- FUNCTOR ------- //

    /// Takes a function which transforms a value to another value and an IO action which produces 
    /// the first value, producing a new IO action which produces the second value
    let map f x = bind x (return' << f)

    // ------- APPLICATIVE ------- //

    /// Takes an IO action which produces a function that maps from a value to another value and an IO action
    /// which produces the first value, producing a new IO action which produces the second value.  This is like 
    /// map but the mapping function is contained within IO.
    let apply (f : IO<'a -> 'b>) (x : IO<'a>) =
        bind f (fun fe -> map fe x)

    /// Lift a value into IO.  Equivalent to return.
    let pure' x = Return x

    // ------- OPERATORS ------- //

    module Operators =
        /// Apply operator for IO actions
        let inline (<*>) (f : IO<'a -> 'b>) (x : IO<'a>) = apply f x
        /// Sequence actions, discarding the value of the first argument.
        let inline ( >>. ) u v = return' (const' id) <*> u <*> v
        /// Sequence actions, discarding the value of the second argument.
        let inline ( .>> ) u v = return' const' <*> u <*> v
        /// Monadic bind operator for IO actions
        let inline (>>=) x f = bind x f
        /// Left to right Kleisli composition of IO actions, allows composition of binding functions
        let inline (>=>) f g x = f x >>= g
        /// Right to left Kleisli composition of IO actions, allows composition of binding functions
        let inline (<=<) f g x = flip (>=>) f g x
        /// Map operator for IO actions
        let inline (<!>) f x = map f x

    open Operators

    /// Takes a function which transforms two values in another value and two IO actions which produce the first two
    /// values, producing a new IO action which produces the result of the function application
    let lift2 f x1 x2 = f <!> x1 <*> x2

    // ----- GENERAL ----- //

    /// Allows you to supply an effect which acquires acquires a resource, an effect which releases that research and an action to perform during the resource's lifetime
    let bracket act fClnUp fBind =
        io {
            let! a = act
            return! fromEffectful (fun _ ->
                try 
                    run <| fBind a
                finally
                    ignore << run <| fClnUp a)
        }

    /// Allows a supplied IO action to be executed on the thread pool, returning a task from which you can
    /// observe the result
    let forkTask<'a> (io : IO<'a>) =
        fromEffectful (fun _ -> 
            match io with
            |Return a -> System.Threading.Tasks.Task.FromResult(a)
            |SyncIO act -> System.Threading.Tasks.Task.Run(fun _ -> run io)
            |AsyncIO aIO -> Async.StartAsTask <| runAsyncIO io)

    /// Allows a supplied IO action to be executed on the thread pool
    let forkIO<'a> (io : IO<'a>) = map (ignore) (forkTask io)

    /// Allows the awaiting of a result from a forked Task
    let awaitTask task = liftAsync <| Async.AwaitTask task

    /// Map each element of a list to a monadic action, evaluate these actions from left to right and collect the results as a sequence.
    let traverse mFunc lst =
        let consF x ys = lift2 (listCons) (mFunc x) ys
        List.foldBack (consF) lst (return' [])

    /// Map each element of a list to a monadic action of options, evaluate these actions from left to right and collect the results which are 'Some' as a sequence.
    let chooseM mFunc sequ =
        let consF = function
            |Some v -> listCons (v)
            |None -> id
        List.foldBack (lift2 consF << mFunc) sequ (return' [])

    /// Filters a sequence based upon a monadic predicate, collecting the results as a sequence
    let filterM pred sequ =
        List.foldBack (fun x -> lift2 (fun flg -> if flg then (listCons x) else id) (pred x)) sequ (return' [])

    /// Map each element of a list to a monadic action, evaluate these actions from left to right and ignore the results.
    let iterM (mFunc : 'a -> IO<'b>) (sequ : seq<'a>) =
        SyncIO (fun _ ->
            use enmr = sequ.GetEnumerator()
            let rec iterMRec() =
                io {
                    match enmr.MoveNext() with
                    |true -> 
                        let! res = mFunc (enmr.Current)
                        return! iterMRec() //must use return! (not do!) for tail call
                    |false -> return ()
                }
            iterMRec())

    /// Analogous to fold, except that the results are encapsulated within IO
    let foldM accFunc acc sequ =
        let f' x k z = accFunc z x >>= k
        List.foldBack (f') sequ return' acc

    /// Evaluate each action in the list from left to right and collect the results as a list.
    let sequence seq = traverse id seq

    /// Performs the action mFunc n times, gathering the results.
    let replicateM mFunc n = sequence (List.init n (const' mFunc))

    /// As replicateM but ignores the results
    let repeatM mFunc n  = replicateM mFunc n >>= (return' << ignore)

    /// IOBuilder extensions so that iterM can be used to define For
    type IOBuilder with
        /// Definition of for loops within IO computation expressions
        member this.For (sequence : seq<_>, body) =
            iterM body sequence

    // ------ LOOPS ------ //

    /// IO looping constructs
    module Loops =
        /// Take elements repeatedly while a condition is met
        let takeWhileM p xs =
            let rec takeWhileMRec p xs =
                io {
                    match xs with
                    |[] -> return []
                    |x::xs ->
                        let! q = p x
                        match q with
                        |true -> return! (takeWhileMRec p xs) >>= (fun xs' -> return' (x::xs'))
                        |false -> return [] 
                }
            takeWhileMRec p xs

        /// Drop elements repeatedly while a condition is met
        let skipWhileM p xs =
            let rec skipWhileMRec p xs =
                io {
                    match xs with
                    |[] -> return []
                    |x::xs ->
                        let! q = p x
                        match q with
                        |true -> return! skipWhileMRec p xs
                        |false -> return x::xs
                }
            skipWhileMRec p xs

        /// Execute an action repeatedly as long as the given boolean IO action returns true
        let whileM (pAct : IO<bool>) (f : IO<'a>) =
            let rec whileMRec acc =
                io {
                    let! p = pAct
                    match p with
                    |true -> 
                        let! x = f
                        return! whileMRec (x::acc)
                    |false -> return acc
                }
            whileMRec [] 
            |> map (List.rev)

        /// Execute an action repeatedly as long as the given boolean IO action returns true
        let iterWhileM (pAct : IO<bool>) (act : IO<'a>) =
            let rec whileMRec() =
                io { // check the predicate action
                    let! p = pAct 
                    match p with
                    |true -> // unwrap the current action value then recurse
                        let! x = act
                        return! whileMRec()
                    |false -> return () // finished
                }
            whileMRec ()

        /// Execute an action repeatedly until the given boolean IO action returns true
        let untilM (pAct : IO<bool>) (f : IO<'a>) = whileM (not <!> pAct) f

        /// Execute an action repeatedly until the given boolean IO action returns true
        let iterUntilM (pAct : IO<bool>) (f : IO<'a>) = iterWhileM (not <!> pAct) f

        /// As long as the supplied "Maybe" expression returns "Some _", each element will be bound using the value contained in the 'Some'.
        /// Results are collected into a sequence.
        let whileSome act binder =
            let rec whileSomeRec acc =
                io {
                    let! p = act
                    match p with
                    |Some x -> 
                        let! x' = binder x
                        return! whileSomeRec (x' :: acc)
                    |None -> return acc
                }
            whileSomeRec []
            |> map (List.rev)

        /// Yields the result of applying f until p holds.
        let rec iterateUntilM p f v =
            io {
                match p v with
                |true -> return v
                |false ->
                    let! v' = f v 
                    return! iterateUntilM p f v'
            }

        /// Execute an action repeatedly until its result satisfies a predicate and return that result (discarding all others).
        let iterateUntil p x = x >>= iterateUntilM p (const' x)

        /// Execute an action repeatedly until its result fails to satisfy a predicate and return that result (discarding all others).
        let iterateWhile p x = iterateUntil (not << p) x

        /// Repeatedly evaluates the second argument while the value satisfies the given predicate, and returns a list of all
        /// values that satisfied the predicate.  Discards the final one (which failed the predicate).
        let unfoldWhileM p (f : IO<'a>) =
            let rec unfoldWhileMRec acc =
                io {
                    let! x = f
                    match p x with
                    |true -> return! unfoldWhileMRec (x::acc)
                    |false -> return acc
                }
            unfoldWhileMRec []
            |> map (List.rev)

        /// Does the action f forever
        let forever f = iterateWhile (const' true) f

    // ------ Parallel ------ //

    /// Parallel IO combinators
    [<RequireQualifiedAccess>]
    module Parallel =
        /// Executes the given IO actions in parallel and ignores the result.
        let iterSequence (ios : IO<_> list)  =
            let allIOTasks = 
                ios
                |> Array.ofList
                |> Array.map (forkIO)
                |> List.ofArray
                |> sequence
            allIOTasks >>= (return' << ignore)

        /// Executes the given IO actions in parallel.
        let sequence (ios : IO<'a> list)  =
            let allIOTasks =
                ios
                |> Array.ofList
                |> Array.map (forkTask)
                |> List.ofArray
                |> sequence
                |> map (System.Threading.Tasks.Task.WhenAll)
            map (List.ofArray) (allIOTasks >>= awaitTask)

        /// Map each element in a list to a monadic action and then run all of those monadic actions in parallel.
        let traverse (f : 'a -> IO<'b>) sequ =
            List.map f sequ
            |> sequence

/// Module to provide the definition of the io computation expression
[<AutoOpen>]
module IOBuilders =
    /// IO computation expression builder
    let io = IO.IOBuilder()
            

