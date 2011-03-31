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
open iBlue.Windows

/// AutoCompleteMode enum
type AutoCompleteMode =
    /// Mode that searches only the text that is found in the ItemsSource
    | FullText = 0
    /// Mode that allows extra text apart from the search text found in the ItemsSource
    | AllowExtraText = 1
//    | Implicit = 2

[<AllowNullLiteral>]
type AutoCompleteTextBox() =
    inherit TextBox()

    static let itemsSourceMetadata =
        new PropertyMetadata
            ( null, new PropertyChangedCallback
                ( fun dpo args ->
                    (
                        let acTextBox = dpo :?> AutoCompleteTextBox
                        if args.NewValue <> null then
                            acTextBox.OnItemsSourceChanged(args.NewValue :?> IEnumerable)
                    )                    
                )
            )
    static let ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof<IEnumerable>, typeof<AutoCompleteTextBox>, itemsSourceMetadata)

    static let autocompleteModeMetadata =
        new PropertyMetadata
            ( AutoCompleteMode.FullText, new PropertyChangedCallback
                ( fun dpo args ->
                    (
                    )
                )
            )
    static let AutoCompleteModeProperty = DependencyProperty.Register("AutoCompleteMode", typeof<AutoCompleteMode>, typeof<AutoCompleteTextBox>, autocompleteModeMetadata)

    static let MatchCaseProperty = DependencyProperty.Register("MatchCase", typeof<bool>, typeof<AutoCompleteTextBox>, new PropertyMetadata(false))

    let displayMemberContext                   = new FrameworkElementContextWrapper()
    let valueMemberContext                     = new FrameworkElementContextWrapper()
    let mutable displayMemberBinding : Binding = null
    let mutable valueMemberBinding   : Binding = null
    let mutable isApplyingText       : bool    = false
    let mutable actualText           : String  = String.Empty
    let mutable shouldProcessKey     : bool    = false
    let mutable view                 : ICollectionView = null

    let getValueFromBinding(record : obj) = 
        displayMemberContext.DataContext <- record
        displayMemberContext.Value

    member private x.OnFilterPredicate(enteredText : String) =
        new Predicate<_>
            (
                fun (record : obj) ->
                    let recordVal = getValueFromBinding(record)
                    if recordVal <> null then                      
                        if x.MatchCase then  
                            recordVal.ToString().StartsWith(enteredText)
                        else 
                            recordVal.ToString().ToLower().StartsWith(enteredText.ToLower())
                    else
                        false
            )

    member private x.OnItemsSourceChanged(itemsSource : IEnumerable) =
        printfn "ItemsSource changed"
        let view = CollectionViewSource.GetDefaultView(x.ItemsSource) :?> ListCollectionView
        view.Filter <- null

    member x.ItemsSource
        with get() = x.GetValue(ItemsSourceProperty) :?> IEnumerable
        and set(v : IEnumerable) = x.SetValue(ItemsSourceProperty, v)

    member x.DisplayMemberBinding
        with get() = displayMemberBinding
        and set(v) = 
            displayMemberBinding <- v
            displayMemberContext.SetBindingForValue(v)

    member x.ValueMemberBinding
        with get() = valueMemberBinding
        and set(v) =
            valueMemberBinding <- v
            valueMemberContext.SetBindingForValue(v)

    member this.AutoCompleteMode
        with get() = this.GetValue(AutoCompleteModeProperty) :?> AutoCompleteMode
        and set(v : AutoCompleteMode) = this.SetValue(AutoCompleteModeProperty, v)

    member x.MatchCase
        with get() = x.GetValue(MatchCaseProperty) :?> bool
        and set(v : bool) = x.SetValue(MatchCaseProperty, v)

    override x.OnPreviewKeyDown(e : KeyEventArgs) =
        // Console.WriteLine("Text {0}", e.Key.ToString())
        // backspace / delete - based on caretindex / selectionlength update the actualtext
        match e.Key with
        | Key.Back -> 
            shouldProcessKey <- true
        | Key.Delete ->
            shouldProcessKey <- true
        | Key.Space ->
            shouldProcessKey <- true
        | _ -> ()
                
    override x.OnTextInput(e : TextCompositionEventArgs) =
        if x.SelectionLength = x.Text.Length then
            // sometimes if the text is fully selected and text input is fired then we clear out the text
            actualText <- String.Empty
        actualText <- actualText + e.Text
        if x.AutoCompleteMode = AutoCompleteMode.FullText then
            let text = x.FindTextFromSource(actualText)
            if text = String.Empty then
                e.Handled <- true
        base.OnTextInput(e)

    override x.OnTextChanged(e : TextChangedEventArgs) =
        if isApplyingText then
            base.OnTextChanged(e)

        if shouldProcessKey then
            actualText <- x.Text
            shouldProcessKey <- false
        else
            let text = x.FindTextFromSource(actualText)
            // type a character "A"
            // this should be validated against the underlying source based on DisplayMember
            if text <> String.Empty then
                isApplyingText <- true
                x.Text <- text
                x.CaretIndex <- actualText.Length
                x.Select(actualText.Length, x.Text.Length - 1)
                isApplyingText <- false
        base.OnTextChanged(e)

    member private x.FindTextFromSource(enteredText : String) : String =
        view.Filter <- x.OnFilterPredicate(enteredText)
        let mutable result = String.Empty
        let listView = view :?> ListCollectionView
        if listView.Count > 0 then
            let record = listView.GetItemAt(0)
            let recordVal = getValueFromBinding(record)
            if recordVal <> null then
                result <- recordVal.ToString()
        result
