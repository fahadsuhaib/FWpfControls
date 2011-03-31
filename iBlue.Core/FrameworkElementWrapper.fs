namespace iBlue.Windows
open System
open System.Windows
open System.Windows.Data
open System.Windows.Controls
open System.Windows.Documents

/// <summary>Wrapper class that uses WPF / Silverlight DataBinding engine for getting transformed values</summary>
type FrameworkElementContextWrapper() =
    inherit FrameworkElement()

    static let ValueProperty = DependencyProperty.Register("Value", typeof<Object>, typeof<FrameworkElementContextWrapper>, new PropertyMetadata(null))
    /// Gets / Sets the value that is used for DataBinding
    member x.Value 
        with get() = x.GetValue(ValueProperty)
        and set(v) = x.SetValue(ValueProperty, v)

    /// Sets the DataBinding to the ValueProperty.
    member x.SetBindingForValue(binding : Binding) =
        let previousBinding = x.GetBindingExpression(ValueProperty)
        if previousBinding <> null then
            x.ClearValue(ValueProperty)
        x.SetBinding(ValueProperty, binding) |> ignore
