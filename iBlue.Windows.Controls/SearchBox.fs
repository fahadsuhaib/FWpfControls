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
open iBlue.Core.PropertyDescriptorExtensions

type SearchBoxColumn() =
    inherit DependencyObject()

    static let MappingNameProperty = DependencyProperty.Register("MappingName", typeof<string>, typeof<SearchBoxColumn>)
    static let HeaderTextProperty = DependencyProperty.Register("HeaderText", typeof<string>, typeof<SearchBoxColumn>)
    static let ExcludeFromFullSearchProperty = DependencyProperty.Register("ExcludeFromFullSearch", typeof<bool>, typeof<SearchBoxColumn>, new PropertyMetadata(false))

    member x.MappingName
        with get() = x.GetValue(MappingNameProperty) :?> string
        and set(v : String) = x.SetValue(HeaderTextProperty, v)

    member x.HeaderText
        with get() = x.GetValue(HeaderTextProperty) :?> string
        and set(v : String) = x.SetValue(HeaderTextProperty, v)

    member x.ExcludeFromFullSearch
        with get() = x.GetValue(ExcludeFromFullSearchProperty) :?> bool
        and set(v : bool) = x.SetValue(ExcludeFromFullSearchProperty, v)

    override x.ToString() =
        String.Format("MappingName - {0} / HeaderText - {1}")

// Name:-searchtext - searches name column
// searchtext - searches tweet column
// PostedAt:-searchtext - searches posted at column
[<AllowNullLiteral>]
type SearchBox() as this =
    inherit Control()
    
    do this.DefaultStyleKey <- typeof<SearchBox>
    let resourceDict = Application.LoadComponent(new Uri("/iBlue.Windows.Controls;component/Themes/generic.xaml", System.UriKind.Relative)) :?> ResourceDictionary          
    do this.Resources.MergedDictionaries.Add(resourceDict)

    static let searchRegex = new Regex(@"(.*?:-)")

    static let itemsSourceMetadata = 
        new PropertyMetadata
            ( null, new PropertyChangedCallback
                ( fun dpo args ->
                    (
                        let box = dpo :?> SearchBox
                        if args.NewValue <> null then
                            box.OnItemsSourceChanged(args.NewValue :?> IEnumerable)
                    )
                )                 
            )
    static let ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof<IEnumerable>, typeof<SearchBox>, itemsSourceMetadata)

    static let searchTextMetadata =
        new PropertyMetadata
            ( String.Empty, new PropertyChangedCallback
                ( fun dpo args ->
                    (
                        let box = dpo :?> SearchBox
                        if args.NewValue <> null then
                            box.OnSearchTextChanged(args.NewValue.ToString())
                    )
                )
            )
    static let SearchTextProperty = DependencyProperty.Register("SearchText", typeof<string>, typeof<SearchBox>, searchTextMetadata)

    static let waterMarkTextMetadata = new PropertyMetadata(null)
    static let WaterMarkTextProperty = DependencyProperty.Register("WaterMarkText", typeof<string>, typeof<SearchBox>, waterMarkTextMetadata)

    let columns = new ObservableCollection<SearchBoxColumn>()
    let mutable waterMarkTextBox : WaterMarkTextBox = null
    let mutable properties : PropertyDescriptorCollection = null

    member x.ItemsSource 
        with get() = x.GetValue(ItemsSourceProperty) :?> IEnumerable
        and set(v : IEnumerable) = x.SetValue(ItemsSourceProperty, v)

    member x.SearchText
        with get() = x.GetValue(SearchTextProperty) :?> string
        and set(v : String) = x.SetValue(SearchTextProperty, v)

    member x.Columns
        with get() = columns

    member x.WaterMarkText
        with get() = x.GetValue(WaterMarkTextProperty) :?> string
        and set(v : String) = x.SetValue(WaterMarkTextProperty, v)

    member private x.OnItemsSourceChanged(itemsSource : IEnumerable) =
        let view = x.GetDefaultView()
        if view <> null then
            properties <- TypeDescriptor.GetProperties((view.SourceCollection :?> IList).[0])

    member private x.OnSearchTextChanged(searchText : string) =
        let view =  x.GetDefaultView()
        if view <> null && properties <> null then
            let m = searchRegex.Match(searchText)
            let mutable resultText = searchText.Substring(m.Length, searchText.Length - m.Length)
            if m.Success && resultText <> String.Empty then
                let columnToSearch = m.Value.Substring(0, m.Value.IndexOf(":-"))
                let col = x.Columns
                                |> Seq.find(fun i -> i.HeaderText = columnToSearch || i.MappingName = columnToSearch)
                let predicateMatch(predicateType : string, filterText) =
                    new Predicate<_>
                        (
                            fun (record:obj) -> 
                                let recordVal = properties.GetValue(col.MappingName, record)
                                let mutable result = false
                                if recordVal <> null then
                                    result <- 
                                        match predicateType with
                                        | "StartsWith" -> recordVal.ToString().ToLower().StartsWith(filterText)
                                        | "EndsWith"   -> recordVal.ToString().ToLower().EndsWith(filterText)
                                        | _            -> recordVal.ToString().ToLower().Contains(filterText)
                                result
                        )
                if resultText.StartsWith("%") then
                    resultText <- resultText.Substring(1, resultText.Length - 1)
                    if resultText <> String.Empty then
                        view.Filter <- predicateMatch("StartsWith", resultText.ToLower())
                else if resultText.EndsWith("%") then
                    resultText <- resultText.Substring(0, resultText.Length - 1)
                    if resultText <> String.Empty then
                        view.Filter <- predicateMatch("EndsWith", resultText.Substring(0, resultText.Length - 1).ToLower())
                else
                    view.Filter <- predicateMatch("Contains", resultText.ToLower())
            else if searchText <> String.Empty then
                let predicateMatch(filterText : string) =
                    new Predicate<_>
                        (
                            fun (record:obj) -> 
                                let mutable result = false
                                let mutable shouldBreak = false
                                let mutable i = 0
                                while not(shouldBreak) && i < x.Columns.Count do
                                    let col = x.Columns.[i]
                                    if not(col.ExcludeFromFullSearch) then
                                        let recordVal = properties.GetValue(col.MappingName, record)
                                        if recordVal <> null then
                                            result <- recordVal.ToString().ToLower().Contains(filterText)
                                        if result then shouldBreak <- true
                                    i <- i + 1
                                result
                        )
                view.Filter <- predicateMatch(searchText.ToLower())
            else
                if view.Filter <> null then
                    view.Filter <- null

    member private x.GetDefaultView() : ICollectionView =
        CollectionViewSource.GetDefaultView(x.ItemsSource)

    override this.OnApplyTemplate() =
        waterMarkTextBox <- this.GetTemplateChild("PART_WaterMarkTextBox") :?> WaterMarkTextBox

    override this.OnGotFocus(e) =
        if waterMarkTextBox <> null then
            waterMarkTextBox.Focus() |> ignore
