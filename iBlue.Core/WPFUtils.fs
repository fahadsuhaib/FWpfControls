namespace iBlue.Windows

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Controls.Primitives
open System.Windows.Media
open System.ComponentModel

module WpfUtils = 

    let (?) (source:obj) (s:string) =
        match source with 
        | :? ResourceDictionary as r ->  r.[s] :?> 'T
        | :? Control as source -> 
            match source.FindName(s) with 
            | null -> invalidOp (sprintf "dynamic lookup of Xaml component %s failed" s)
            | :? 'T as x -> x
            | _ -> invalidOp (sprintf "dynamic lookup of Xaml component %s failed because the component found was of type %A instead of type %A"  s (s.GetType()) typeof<'T>)
        | _ -> invalidOp (sprintf "dynamic lookup of Xaml component %s failed because the source object was of type %A. It must be a control of a resource dictionary" s (source.GetType()))

    let LoadXAML<'a>(uri) =
        Application.LoadComponent(uri) :?> 'a

    let LoadXAMLComponent(comp, uri) =
        do Application.LoadComponent(comp, uri)

    let GetValue<'a>(value : string) =
        let converter = TypeDescriptor.GetConverter(typeof<'a>)        
        match converter with
        | c ->
            let typeValue = c.ConvertFromInvariantString(value) :?> 'a
            Some(typeValue)        