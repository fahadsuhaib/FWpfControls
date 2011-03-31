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

[<AllowNullLiteral>]
type MaskTextBox() =
    inherit TextBox()

    static let integerRegex = new Regex("^([0-5]?\d?\d?\d?\d|6[0-4]\d\d\d|65[0-4]\d\d|655[0-2]\d|6553[0-5])$")

    override x.OnTextInput(e : TextCompositionEventArgs) =        
        let isMatch = integerRegex.IsMatch(e.Text)
        printfn "Match %b" isMatch
        if isMatch = true then
            base.OnTextInput(e)