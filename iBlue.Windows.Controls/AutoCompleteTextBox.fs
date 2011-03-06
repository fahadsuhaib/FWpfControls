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

type AutoCompleteMode =
    | FullText = 0
    | Implicit = 1
    | AllowExtraText = 2

[<AllowNullLiteral>]
type AutoCompleteTextBox() as this =
    inherit Control()

    do this.DefaultStyleKey <- typeof<AutoCompleteTextBox>
    let resourceDict = Application.LoadComponent(new Uri("/iBlue.Windows.Controls;component/Themes/AutoCompleteTextBoxTemplate.xaml", System.UriKind.Relative)) :?> ResourceDictionary          
    do this.Resources.MergedDictionaries.Add(resourceDict)

    static let itemsSourceMetadata =
        new PropertyMetadata
            ( null, new PropertyChangedCallback
                ( fun dpo args ->
                    (
                        
                    )                    
                )
            )
    static let ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof<IEnumerable>, typeof<AutoCompleteTextBox>, itemsSourceMetadata)

    static let displayMemberBindingMetadata =
        new PropertyMetadata
            ( null, new PropertyChangedCallback
                ( fun dpo args ->
                    (
                    )
                )
            )
    static let DisplayMemberBindingProperty = DependencyProperty.Register("DisplayMemberBinding", typeof<Binding>, typeof<AutoCompleteTextBox>, displayMemberBindingMetadata)

    static let valueMemberBindingMetadata =
        new PropertyMetadata
            ( null, new PropertyChangedCallback
                ( fun dpo args ->
                    (
                    )
                )
            )
    static let ValueMemberBindingProperty = DependencyProperty.Register("ValueMemberBinding", typeof<Binding>, typeof<AutoCompleteTextBox>, valueMemberBindingMetadata)

    static let autocompleteModeMetadata =
        new PropertyMetadata
            ( AutoCompleteMode.FullText, new PropertyChangedCallback
                ( fun dpo args ->
                    (
                    )
                )
            )
    static let AutoCompleteModeProperty = DependencyProperty.Register("AutoCompleteMode", typeof<AutoCompleteMode>, typeof<AutoCompleteTextBox>, autocompleteModeMetadata)

    let mutable textBox : TextBox = null
    override this.OnApplyTemplate() =
        textBox <- this.GetTemplateChild("PART_TextBox") :?> TextBox

    member x.ItemsSource
        with get() = x.GetValue(ItemsSourceProperty) :?> IEnumerable
        and set(v) = x.SetValue(ItemsSourceProperty, v)

    member x.DisplayMemberBinding
        with get() = x.GetValue(DisplayMemberBindingProperty) :?> Binding
        and set(v) = x.SetValue(DisplayMemberBindingProperty, v)

    member x.ValueMemberBinding
        with get() = x.GetValue(ValueMemberBindingProperty) :?> Binding
        and set(v) = x.SetValue(ValueMemberBindingProperty, v)

    member x.AutoCompleteMode
        with get() = x.GetValue(AutoCompleteModeProperty) :?> AutoCompleteMode
        and set(v) = x.SetValue(AutoCompleteModeProperty, v)