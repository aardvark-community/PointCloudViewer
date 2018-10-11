open gumgui

open Aardium
open Aardvark.Application.Slim
open Aardvark.Base
open Aardvark.Rendering.Vulkan
open Aardvark.Service
open Aardvark.UI
open Suave
open Suave.WebPart
open System

[<EntryPoint>]
let main args =

    let mutable port = 4321
    let mutable useVulkan = true
    let mutable i = 0
    while i < args.Length do
        match args.[i] with
        | "-port" | "-p" -> i <- i + 1; port <- Int32.Parse args.[i]
        | "-gl" | "-ogl" -> useVulkan <- false
        | _ -> Log.warn "unknown argument: %s" args.[i]

        i <- i + 1

    Ag.initialize()
    Aardvark.Init()
    Aardium.init()
    
    if useVulkan then

        let app = new HeadlessVulkanApplication(true)
        WebPart.startServer port [
            MutableApp.toWebPart' app.Runtime false (App.start App.app)
        ]

    else

        let app = new OpenGlApplication(true)
        WebPart.startServer port [
            MutableApp.toWebPart' app.Runtime false (App.start App.app)
        ]

    Aardium.run {
        title "hum - a viewer for humongous point clouds"
        width 1024
        height 768
        url (sprintf "http://localhost:%i/" port)
    }

    0
