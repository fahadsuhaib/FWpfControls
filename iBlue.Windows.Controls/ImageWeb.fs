namespace iBlue.Windows.Controls

open System
open System.Collections.Generic
open System.IO
open System.Windows
open System.Windows.Controls
open System.Windows.Data
open Microsoft.FSharp.Core.Operators
open System.Windows.Media.Imaging
open System.Net
open Microsoft.FSharp.Control.WebExtensions
open iBlue.Core.ThreadingExtensions

type ImageWeb() =
    inherit System.Windows.Controls.Image()

    let webRequestCreated = new Event<WebRequest>()
    let error             = new Event<System.Exception>()
    let syncContext       = System.Threading.SynchronizationContext.CaptureCurrent()

    static let imageUrlMetadata : PropertyMetadata =
        new PropertyMetadata(String.Empty,
            new PropertyChangedCallback
                (
                    fun dpo args ->
                    (
                        let image = dpo :?> ImageWeb
                        if args.NewValue <> null then
                            image.OnImageUrlPropertyChanged(args.NewValue.ToString())
                    )
                ))

    static let ImageUrlProperty = DependencyProperty.Register("ImageUrl", typeof<String>, typeof<ImageWeb>, imageUrlMetadata)

    member private x.OnImageUrlPropertyChanged(imageUrl : string) =
        //"http://a0.twimg.com/profile_images/1017267710/thumb_poolga_2010_06_normal.png"
        x.Source <- null
        let httpAsyncImageDownload(url : string) =
            async{
                let req = WebRequest.Create(url)
                // raise the event to let users add authorization on the request headers
                syncContext.RaiseEvent webRequestCreated req
                use! resp = req.AsyncGetResponse()
                use stream = resp.GetResponseStream()
                let totalLength = int32(resp.ContentLength)
                let rec readAllBytes(buffer : byte[], stream : Stream, startPointer : int, totalLength : int) =
                    if totalLength = startPointer then
                        ()
                    else
                        let bytesRead = stream.Read(buffer, startPointer, totalLength - startPointer)
                        readAllBytes(buffer, stream, bytesRead + startPointer, totalLength)
                let buffer = Array.zeroCreate(totalLength)
                readAllBytes(buffer, stream, 0, buffer.Length)
                return buffer
            }

        if imageUrl <> String.Empty then
            let loadImage(bytes : byte[]) = 
                try
                    let bitmap = new BitmapImage()
                    bitmap.CacheOption <- BitmapCacheOption.OnLoad
                    bitmap.CreateOptions <- BitmapCreateOptions.IgnoreColorProfile
                    bitmap.BeginInit()
                    bitmap.StreamSource <- new MemoryStream(bytes)
                    bitmap.EndInit()
                    bitmap.Freeze()                    
                    x.Source <- bitmap
                with
                    | e -> 
                        //printfn "%s" e.Message
                        error.Trigger(e)

            Async.StartWithContinuations(httpAsyncImageDownload(imageUrl),
                (fun (bytes : byte[]) ->
                    x.Dispatcher.BeginInvoke(new Action(fun _ ->
                        loadImage(bytes)
                    )) |> ignore
                ),
                (fun exn -> syncContext.RaiseEvent error exn),
                (fun _ -> ()))

    member x.ImageUrl
        with get() =
            x.GetValue(ImageUrlProperty) :?> string
        and set(v : String) =
            x.SetValue(ImageUrlProperty, v)

    member x.WebRequestCreated = webRequestCreated.Publish
    member x.Error = error.Publish
