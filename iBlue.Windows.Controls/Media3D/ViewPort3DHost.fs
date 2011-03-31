namespace iBlue.Windows.Controls

open System
open System.Windows
open System.Windows.Controls
open System.Collections.Generic
open System.Windows.Data
open System.Windows.Media
open System.Windows.Media.Media3D
open System.Windows.Input

[<AllowNullLiteral>]
type ViewPort3DHost() as this =
    inherit Control()

    do this.DefaultStyleKey <- typeof<ViewPort3DHost>
    let resourceDict = Application.LoadComponent(new Uri("/iBlue.Windows.Controls;component/Themes/ViewPort3DHost.xaml", System.UriKind.Relative)) :?> ResourceDictionary
    do this.Resources.MergedDictionaries.Add(resourceDict)

    static let viewPort3DContentMetadata = 
        new PropertyMetadata
            (null, new PropertyChangedCallback
                ( fun dpo args ->
                    (
                        let host = dpo :?> ViewPort3DHost
                        if args.NewValue <> null && host.IsLoaded then
                            host.OnViewPort3DContentChanged(args.NewValue :?> Viewport3D)
                    )
                )
            )
    static let ViewPort3DContentProperty = DependencyProperty.Register("ViewPort3DContent", typeof<Viewport3D>, typeof<ViewPort3DHost>, viewPort3DContentMetadata)

    let mutable wrapperEl : Border = null
    let mutable previousPosition2D = new Point(0., 0.)
    let mutable previousPosition3D = new Vector3D(0., 0., 1.)
    let mutable isLoaded = false
    let transform = new Transform3DGroup()
    let scale     = new ScaleTransform3D()
    let rotation  = new AxisAngleRotation3D()
    do transform.Children.Add(scale)
    do transform.Children.Add(new RotateTransform3D(rotation))
    let eventBag = new Dictionary<string, IDisposable>()

    member private x.Track(currentPosition : Point) =
        let currentPosition3D = x.ProjectTo3D(wrapperEl.ActualWidth, wrapperEl.ActualHeight, currentPosition)
        let axis = Vector3D.CrossProduct(previousPosition3D, currentPosition3D)
        let angle = Vector3D.AngleBetween(previousPosition3D, currentPosition3D)
        let delta = new Quaternion(axis, -angle)
        let mutable q = new Quaternion(rotation.Axis, rotation.Angle)
        q <- q * delta
        rotation.Axis <- q.Axis
        rotation.Angle <- q.Angle
        previousPosition3D <- currentPosition3D

    member private x.ProjectTo3D(width : float, height : float, point : Point) =
        let mutable x = point.X / (width / 2.)
        let mutable y = point.Y / (height / 2.)

        x <- x - 1. // translate 0,0 to center
        y <- 1. - y // flip +Y so it is up instead of down

        let z2 = 1. - x * x - y * y
        let z  = if z2 > 0. then Math.Sqrt(z2) else 0.

        new Vector3D(x, y, z)

    member private x.Zoom(currentPosition : Point) =
        let yDelta = currentPosition.Y - previousPosition2D.Y
        let scaleFactor = Math.Exp(yDelta / 100.)
        scale.ScaleX <- scale.ScaleX * scaleFactor
        scale.ScaleY <- scale.ScaleY * scaleFactor
        scale.ScaleZ <- scale.ScaleZ * scaleFactor

    member private x.Reset() =
        scale.ScaleX <- 1.
        scale.ScaleY <- 1.
        scale.ScaleZ <- 1.
        rotation.Axis <- new Vector3D(0., 1., 0.)
        rotation.Angle <- 0.

    member private x.OnViewPort3DContentChanged(viewPort3D : Viewport3D) =
        viewPort3D.Camera.Transform <- null
        viewPort3D.Camera.Transform <- transform

    member x.ViewPort3DContent
        with get() = x.GetValue(ViewPort3DContentProperty) :?> Viewport3D
        and set(v : Viewport3D) = x.SetValue(ViewPort3DContentProperty, v)

    member private x.IsLoaded with get() = isLoaded

    override x.OnApplyTemplate() =
        isLoaded <- true
        if x.ViewPort3DContent <> null then
            x.OnViewPort3DContentChanged(x.ViewPort3DContent)
        eventBag.Clear()
        wrapperEl <- x.GetTemplateChild("PART_WrapperElement") :?> Border
        let mouseDown = 
            wrapperEl.MouseDown.Subscribe(
                    fun (e : MouseButtonEventArgs) ->
                        Mouse.Capture(wrapperEl, CaptureMode.None) |> ignore
                        previousPosition2D <- e.GetPosition(wrapperEl :> IInputElement)
                        previousPosition3D <- x.ProjectTo3D(wrapperEl.ActualWidth, wrapperEl.ActualHeight, previousPosition2D)                        
                )
        eventBag.Add("MouseDown", mouseDown)
        let mouseUp = 
            wrapperEl.MouseUp.Subscribe(
                        fun _ -> 
                            Mouse.Capture(wrapperEl, CaptureMode.None) |> ignore
                    )
        eventBag.Add("MouseUp", mouseUp)

        let mouseMove =
            wrapperEl.MouseMove.Subscribe(
                    fun (e : MouseEventArgs) ->
                        let currentPosition = e.GetPosition(wrapperEl :> IInputElement)
                        if e.LeftButton = MouseButtonState.Pressed then
                            x.Track(currentPosition)
                        else if e.RightButton = MouseButtonState.Pressed then
                            x.Zoom(currentPosition)

                        previousPosition2D <- currentPosition
                )
        eventBag.Add("MouseMove", mouseMove)

        base.OnApplyTemplate()

    interface IDisposable with
        member x.Dispose() = 
            for kvp in eventBag do
                kvp.Value.Dispose()
            eventBag.Clear()