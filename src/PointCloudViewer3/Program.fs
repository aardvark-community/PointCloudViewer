namespace PointCloudViewer3

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Application
open Aardvark.Application.Slim
open Aardvark.Geometry.Points
open Aardvark.Data.Points
open Aardvark.Data.Points.Import
open Uncodium.SimpleStore



module Main = 

    [<EntryPoint;STAThread>]
    let main argv = 

        let store = PointCloud.OpenStore(@"D:\store15")

        let cfg =
            ImportConfig.Default
                .WithStorage(store)
                .WithRandomKey()
                .WithVerbose(true)
                .WithMaxDegreeOfParallelism(1)
                .WithMinDist(0.005)
        
        let cs = Import.Pts.Chunks(@"D:\pts\JBs_Haus.pts",cfg)
        let ps = PointCloud.Chunks(cs, cfg)

        let data = Lod.OctreeLodData(ps)

        Ag.initialize()
        Aardvark.Init()
        
        use app = new OpenGlApplication()
        let win = app.CreateGameWindow(8)
        let speed = 10.0 |> Mod.init
        let initialView = CameraView.lookAt V3d.OIO V3d.Zero V3d.OOI
        let view = initialView |> DefaultCameraController.controlWithSpeed speed win.Mouse win.Keyboard win.Time
        let viewTrafo = view |> Mod.map CameraView.viewTrafo

        let proj = win.Sizes |> Mod.map (fun s -> Frustum.perspective 60.0 0.1 1000.0 (float s.X / float s.Y))
        let projTrafo = proj |> Mod.map Frustum.projTrafo
        
        let tpd = Mod.init 1.5
        let rasterizer = tpd |> Mod.map LodData.defaultRasterizeSet
        
        let info = {
            lodRasterizer           = rasterizer
            attributeTypes = 
                Map.ofList [
                    DefaultSemantic.Positions, typeof<V4f>
                    DefaultSemantic.Colors, typeof<C4b>
                ]
            freeze                  = Mod.constant false
            maxReuseRatio           = 0.5
            minReuseCount           = 1L <<< 20
            pruneInterval           = 500
            customView              = None
            customProjection        = None
            boundingBoxSurface      = None
            progressCallback        = None
        }
        
        let pointCloudSurface = Surface.pointSpriteSurface app win.FramebufferSignature

        let pss = Mod.init 3.0
    

        let bboxSg =
            ps.BoundingBox
                |> Sg.wireBox' C4b.Red
                |> Sg.trafo (Trafo3d.Translation(-ps.BoundingBox.Center) |> Mod.constant)
                |> Sg.shader {
                    do! DefaultSurfaces.trafo
                    do! DefaultSurfaces.thickLine
                    do! DefaultSurfaces.vertexColor
                }
                |> Sg.uniform "LineWidth" (Mod.constant 5.0)

        

        let pcsg = 
            Sg.pointCloud data info
                |> Sg.surface pointCloudSurface
                |> Sg.uniform "PointSize" pss
                |> Sg.uniform "ViewportSize" win.Sizes

        let coordinateCross = 
            let cross =
                IndexedGeometryPrimitives.coordinateCross (V3d.III * 2.0)
                    |> Sg.ofIndexedGeometry
                    |> Sg.translate -6.0 -6.0 -6.0
                    |> Sg.shader {
                        do! DefaultSurfaces.trafo
                        do! DefaultSurfaces.thickLine
                        do! DefaultSurfaces.vertexColor
                    }
                    |> Sg.uniform "LineWidth" (Mod.constant 2.0)


            let box =
                Util.coordinateBox 500.0
                    |> List.map Sg.ofIndexedGeometry
                    |> Sg.ofList
                    |> Sg.shader {
                        do! DefaultSurfaces.trafo
                        do! DefaultSurfaces.vertexColor
                    }

            [cross; box] |> Sg.ofList
        let sg = 
            [pcsg; bboxSg; coordinateCross] 
                |> Sg.ofList
                |> Sg.viewTrafo viewTrafo
                |> Sg.projTrafo projTrafo
                
        win.RenderTask <- (app.Runtime.CompileRender(win.FramebufferSignature, sg))
        win.Run()

        0
