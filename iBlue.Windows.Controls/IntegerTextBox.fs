namespace iBlue.Windows.Controls

open System
open System.Collections
open System.Collections.Generic
open System.Collections.ObjectModel
open System.Windows
open System.Windows.Data
open System.Windows.Controls
open System.Windows.Documents
open System.ComponentModel
open System.Text.RegularExpressions
open System.Linq
open System.Windows.Input
open System.Windows.Media

type IntegerTextBox() =
    inherit TextBox()

    static let integerRegex = new Regex("^([0-5]?\d?\d?\d?\d|6[0-4]\d\d\d|65[0-4]\d\d|655[0-2]\d|6553[0-5])$")    
    static let negIntegerRegex = new Regex("^(-[0-5]?\d?\d?\d?\d|-6[0-4]\d\d\d|-65[0-4]\d\d|-655[0-2]\d|-6553[0-5])$")
    static let decimalRegex = new Regex("^\d*[0-9](|.\d*[0-9]|,\d*[0-9])?$")

    static let NegativeForegroundProperty = DependencyProperty.Register("NegativeForeground", typeof<Brush>, typeof<IntegerTextBox>, new PropertyMetadata(Brushes.Red))

    static let PositiveForegroundProperty = DependencyProperty.Register("PositiveForeground", typeof<Brush>, typeof<IntegerTextBox>, new PropertyMetadata(Brushes.Black))
    
    let mutable isNegativeState : bool = false

    member private x.GotoNegativeState() =
        let bindForeground(dp : DependencyProperty) =
            x.ClearValue(TextBox.ForegroundProperty)
            let binding = new Binding("Foreground")
            binding.Mode <- BindingMode.TwoWay
            binding.Source <- x
            x.SetBinding(dp, binding) |> ignore
            ()
        if not(isNegativeState) then
            isNegativeState <- true
            bindForeground(NegativeForegroundProperty)
        else
            isNegativeState <- false
            bindForeground(PositiveForegroundProperty)        

    member x.NegativeForeground
        with get() = x.GetValue(NegativeForegroundProperty) :?> Brush
        and set(v : Brush) = x.SetValue(NegativeForegroundProperty, v)

    member x.PositiveForeground
        with get() = x.GetValue(PositiveForegroundProperty) :?> Brush
        and set(v : Brush) = x.SetValue(PositiveForegroundProperty, v)

    override x.OnTextInput(e : TextCompositionEventArgs) =
        let checkInt(text : String) =
            let mutable res = int64(0)
            Int64.TryParse(text, ref res)

        let mutable isMatch = false
        if not(e.Text.Contains("-")) then
            isMatch <- checkInt(x.Text + e.Text)

        if e.Text.Contains("-") && x.Text.Contains("-") then
            isMatch <- false
        else if e.Text.Contains("-") then
            isMatch <- true
            x.GotoNegativeState()

        if isMatch = true then
            base.OnTextInput(e)