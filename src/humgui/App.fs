namespace gumgui

open System
open System.Threading
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI
open Aardvark.UI.Primitives
open Aardvark.Base.Rendering
open gumgui.Model

// remove
open Aardvark.Geometry.Points
open Aardvark.Data.Points
open Aardvark.Data.Points.Import
open Uncodium.SimpleStore

open hum

type Message =
    | ToggleModel
    | CameraMessage of FreeFlyController.Message
    //| LoadBeigl
    | OpenStore of list<string>
    | LoadId    of string
    | Nop

module Store =
    let tryOpenStore (dir : string) =
        try 
            Result.Ok (PointCloud.OpenStore dir)
        with e -> 
            Result.Error e

    let tryOpenPointSet (m : Model) (key : string) =
        match m.store with
            | None -> Result.Error "no store"
            | Some store -> 
                try
                    match store.GetPointSet(key, CancellationToken.None) with
                        | null -> Result.Error (sprintf "could not load key: %s" key)
                        | v -> Result.Ok v
                with e -> 
                    Result.Error e.Message

module App =

    let initial = { 
        currentModel = Box; 
        cameraState = FreeFlyController.initial
        pointSet = None
        store = None
    }

    let update (m : Model) (msg : Message) =
        match msg with
            | ToggleModel -> 
                match m.currentModel with
                    | Box -> { m with currentModel = Sphere }
                    | Sphere -> { m with currentModel = Box }

            | CameraMessage msg ->
                { m with cameraState = FreeFlyController.update m.cameraState msg }

            //| LoadBeigl ->
            //    let store = @"c:\Data\test"
            //    let id = "test"
            //    let store = PointCloud.OpenStore(store)
            //    let ps = store.GetPointSet(id, CancellationToken.None)
            //    { m with pointSet = Some ps }
            | OpenStore paths -> 
                match paths with
                    | path::[] -> 
                        match Store.tryOpenStore path with
                            | Result.Ok newStore -> 
                                //m.store |> Option.iter (fun store -> store.Dispose())
                                Log.line "loaded new store: %A" paths
                                { m with store = Some newStore; pointSet = None }
                            | Result.Error err -> 
                                // to ui log
                                Log.error "%s" err.Message
                                m
                    | _ -> m
                     
            | LoadId id -> 
                match Store.tryOpenPointSet m id with
                    | Result.Ok v -> Log.line "loaded id: %s" id; { m with pointSet = Some v }
                    | Result.Error str -> Log.error "%s" str; m
                     
            | Nop -> m
                

    let dependencies = [] @ Html.semui

    let adornerMenu (sectionsAndItems : list<string * list<DomNode<'msg>>>) (rest : list<DomNode<'msg>>) =
        let pushButton() = 
            div [
                clazz "ui black big launch right attached fixed button menubutton"
                js "onclick"        "$('.sidebar').sidebar('toggle');"
                style "z-index:1"
            ] [
                i [clazz "content icon"] [] 
                span [clazz "text"] [text "Menu"]
            ]
        [
            yield 
                div [clazz "pusher"] [
                    yield pushButton()                    
                    yield! rest                    
                ]
            yield 
                Html.SemUi.menu "ui vertical inverted sidebar menu" sectionsAndItems
        ]    

    let onChooseFiles (chosen : list<string> -> 'msg) =
        let cb xs =
            match xs with
                | [] -> chosen []
                | x::[] when x <> null -> x |> Aardvark.Service.Pickler.json.UnPickleOfString |> List.map Aardvark.Service.PathUtils.ofUnixStyle |> chosen
                | _ -> failwithf "onChooseFiles: %A" xs
        onEvent "onchoose" [] cb

    let view (m : MModel) =

        let frustum = 
            Frustum.perspective 60.0 1.0 50000.0 1.0 |> Mod.constant

        
        let sg = 
            adaptive {
                let! ps = m.pointSet
                match ps with
                    | None -> return Sg.empty 
                    | Some ps -> 
                        return Scene.createSceneGraph ps [||] |> Sg.noEvents
            } |> Sg.dynamic

        let att =
            [
                style "position: fixed; left: 0; top: 0; width: 100%; height: 100%"
            ]

        let mainView = 
            FreeFlyController.controlledControl m.cameraState CameraMessage frustum (AttributeMap.ofList att) sg

        let main =
             adornerMenu [
                "View", [
                    //button [clazz "ui small dark button"; onClick (fun _ -> LoadBeigl)] [text "load beigl"]
                    //br []
                    button [
                        onChooseFiles OpenStore
                        clientEvent "onclick" ("aardvark.processEvent('__ID__', 'onchoose', aardvark.dialog.showOpenDialog({properties: ['openFile', 'openDirectory']}));") 
                    ] [text "open directory"]
                    br []
                    label [] [text "Poind cloud key:"]
                    //br []
                    input [onChange LoadId] 
                ]
             ] [mainView]

        require dependencies (
            body [] main
        )

    let app =
        {
            initial = initial
            update = update
            view = view
            threads = fun _ -> ThreadPool.empty
            unpersist = Unpersist.instance
        }