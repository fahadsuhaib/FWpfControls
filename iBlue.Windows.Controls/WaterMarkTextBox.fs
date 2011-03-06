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

[<AllowNullLiteral>]
type WaterMarkTextBox() as this =
    inherit Control()
    
    do this.DefaultStyleKey <- typeof<WaterMarkTextBox>
    let resourceDict = Application.LoadComponent(new Uri("/iBlue.Windows.Controls;component/Themes/generic.xaml", System.UriKind.Relative)) :?> ResourceDictionary          
    do this.Resources.MergedDictionaries.Add(resourceDict)

    let mutable searchTextBox : TextBox = null
    let mutable searchGrid : Grid = null
    let eventBag = new Dictionary<String, IDisposable>()
    let mutable isInWaterMarkTextState = false

    static let WaterMarkTextProperty = DependencyProperty.Register("WaterMarkText", typeof<string>, typeof<WaterMarkTextBox>, new PropertyMetadata(String.Empty))
    static let TextProperty = DependencyProperty.Register("Text", typeof<string>, typeof<WaterMarkTextBox>, new PropertyMetadata(String.Empty))

    member x.WaterMarkText
        with get() = x.GetValue(WaterMarkTextProperty) :?> string
        and set(v) = x.SetValue(WaterMarkTextProperty, v)

    member x.Text
        with get() = x.GetValue(TextProperty) :?> string
        and set(v) = x.SetValue(TextProperty, v)

    override this.OnApplyTemplate() =
        eventBag.Clear()
        searchGrid <- this.GetTemplateChild("PART_Grid") :?> Grid
        searchTextBox <- this.GetTemplateChild("PART_Text") :?> TextBox
        let mouseDown = searchGrid.PreviewMouseDown.Subscribe
                                (
                                    fun _ ->
                                        this.GotoNormalState()
                                        searchTextBox.Focus() |> ignore
                                )
        let lostFocus = searchGrid.LostFocus.Subscribe
                                (
                                    fun _ ->
                                        this.GotoWaterMarkVisualState()
                                )
        eventBag.Add("MouseDown", mouseDown)
        eventBag.Add("LostFocus", lostFocus)
        this.GotoWaterMarkVisualState()

    override this.OnGotFocus(e) =
        if searchTextBox <> null then
            this.GotoNormalState()
            searchTextBox.Focus() |> ignore

    member x.GotoWaterMarkVisualState() =
        if not(isInWaterMarkTextState) && x.WaterMarkText <> String.Empty && x.Text = String.Empty then
            VisualStateManager.GoToState(x, "WaterMarkVisualState", false) |> ignore
            isInWaterMarkTextState <- true

    member x.GotoNormalState() =
        if isInWaterMarkTextState then
            VisualStateManager.GoToState(x, "Normal", false) |> ignore
            isInWaterMarkTextState <- false

    interface IDisposable with
        member x.Dispose() = 
            for kvp in eventBag do
                kvp.Value.Dispose()
            eventBag.Clear()