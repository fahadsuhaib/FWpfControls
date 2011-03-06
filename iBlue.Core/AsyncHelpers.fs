namespace iBlue.Core

open System
open System.Net
open System.IO
open Microsoft.FSharp.Control.WebExtensions
open System.Threading
open System.Windows.Threading

module ThreadingExtensions =
    type SynchronizationContext with
        /// A standard helper extension method to raise an event on the GUI thread
        member syncContext.RaiseEvent (event: Event<_>) args =
            //let mutable syncContext : SynchronizationContext = null
            syncContext.Post((fun _ -> event.Trigger args), state=null)
     
        /// A standard helper extension method to capture the current synchronization context.
        /// If none is present, use a context that executes work in the thread pool.
        /// use WPF context

        static member CaptureCurrent () =
            if SynchronizationContext.Current = null then
                new DispatcherSynchronizationContext()
                //new SynchronizationContext()
            else
                SynchronizationContext.Current :?> DispatcherSynchronizationContext
                //null
