namespace iBlue.Windows.Data

open System
open System.Windows
open System.Windows.Input
open System.ComponentModel

[<AllowNullLiteral>]
type ViewModelBase() =
    let propertyChanged = new Event<_, _>()

    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member x.PropertyChanged = propertyChanged.Publish

    abstract member OnPropertyChanged: string -> unit
    default x.OnPropertyChanged(propertyName : string) =
        propertyChanged.Trigger(x, new PropertyChangedEventArgs(propertyName))

[<AllowNullLiteral>]
type DelegateCommand(canExecuteParam, executeParam) =
    let mutable canExecute : Predicate<obj> = canExecuteParam
    let mutable execute : Action<obj> = executeParam
    let canExecuteChanged = new Event<_,_>()

    interface ICommand with
        member x.CanExecute(parameter : obj) =   
            if canExecute = null then
                failwith "Can Execute delegate is null"
            canExecute.DynamicInvoke(parameter) :?> bool

        member x.Execute(parameter : obj) =
            if execute = null then
                failwith "Can Execute delegate is null"
            execute.DynamicInvoke(parameter) |> ignore

        [<CLIEvent>]
        member x.CanExecuteChanged = canExecuteChanged.Publish

    member x.CanExecute
        with get() = canExecute
        and set(v) = canExecute <- v

    member x.Execute
        with get() = execute
        and set(v) = execute <- v