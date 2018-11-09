(*
    Copyright (c) 2018. Attila Szabo, Georg Haaser, Harald Steinlechner, Stefan Maierhofer.
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*)
namespace hum

open Aardvark.Application
open Aardvark.Application.Slim
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.Data.Points
open Aardvark.Data.Points.Import
open Aardvark.Geometry
open Aardvark.Geometry.Points
open Aardvark.Rendering.Vulkan
open Aardvark.SceneGraph
open Newtonsoft.Json.Linq
open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Net
open System.Threading
open Uncodium.SimpleStore

[<AutoOpen>]
module CmdLine = 
    open Uncodium
    open System.Diagnostics

    let parseBounds filename =
        let json = JArray.Parse(File.readAllText filename) :> seq<JToken>
        json
            |> Seq.map (fun j -> Box3d.Parse(j.["Bounds"].ToObject<string>()))
            |> Array.ofSeq
            
    let view (store : string) (id : string) (args : Args) =
    
        let boundsToShow =
            match args.showBoundsFileName with
            | Some filename -> parseBounds filename
            | None -> [| |]
            
        //init ()

        let store = PointCloud.OpenStore(store)
        let ps = store.GetPointSet(id, CancellationToken.None)
        let data = Lod.OctreeLodData(ps)
        let center = ps.BoundingBox.Center

        Ag.initialize()
        Aardvark.Init()
        
        use app = new OpenGlApplication()
        let win = app.CreateGameWindow(8)
        win.Title <- "hum - a viewer for humongous point clouds"
        
        let speed = 10.0 |> Mod.init
        let coloring = Model.PointColoring.Colors |> Mod.init

        let initialView = CameraView.lookAt V3d.OIO V3d.Zero V3d.OOI
        let view = initialView |> DefaultCameraController.controlWithSpeed speed win.Mouse win.Keyboard win.Time
        let viewTrafo = view |> Mod.map CameraView.viewTrafo

        let proj = win.Sizes |> Mod.map (fun s -> Frustum.perspective args.fov args.nearPlane args.farPlane (float s.X / float s.Y))
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
                |> Sg.effect Surfaces.pointsprite
                |> Sg.uniform "PointSize" pss
                |> Sg.uniform "ViewportSize" win.Sizes
                |> Sg.uniform "PointColoring" coloring

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

        // show bounding boxes (-sb)
        let c = center - V3d(0.0, 0.0, 50.0)
        let box2lines (box : Box3d) =
            [|
                Line3d(V3d(box.Min.X, box.Min.Y, box.Min.Z) - c, V3d(box.Max.X, box.Min.Y, box.Min.Z) - c);
                Line3d(V3d(box.Max.X, box.Min.Y, box.Min.Z) - c, V3d(box.Max.X, box.Max.Y, box.Min.Z) - c);
                Line3d(V3d(box.Max.X, box.Max.Y, box.Min.Z) - c, V3d(box.Min.X, box.Max.Y, box.Min.Z) - c);
                Line3d(V3d(box.Min.X, box.Max.Y, box.Min.Z) - c, V3d(box.Min.X, box.Min.Y, box.Min.Z) - c);
                                                                                                        
                Line3d(V3d(box.Min.X, box.Min.Y, box.Max.Z) - c, V3d(box.Max.X, box.Min.Y, box.Max.Z) - c);
                Line3d(V3d(box.Max.X, box.Min.Y, box.Max.Z) - c, V3d(box.Max.X, box.Max.Y, box.Max.Z) - c);
                Line3d(V3d(box.Max.X, box.Max.Y, box.Max.Z) - c, V3d(box.Min.X, box.Max.Y, box.Max.Z) - c);
                Line3d(V3d(box.Min.X, box.Max.Y, box.Max.Z) - c, V3d(box.Min.X, box.Min.Y, box.Max.Z) - c);
                                                                                                        
                Line3d(V3d(box.Min.X, box.Min.Y, box.Min.Z) - c, V3d(box.Min.X, box.Min.Y, box.Max.Z) - c);
                Line3d(V3d(box.Max.X, box.Min.Y, box.Min.Z) - c, V3d(box.Max.X, box.Min.Y, box.Max.Z) - c);
                Line3d(V3d(box.Max.X, box.Max.Y, box.Min.Z) - c, V3d(box.Max.X, box.Max.Y, box.Max.Z) - c);
                Line3d(V3d(box.Min.X, box.Max.Y, box.Min.Z) - c, V3d(box.Min.X, box.Max.Y, box.Max.Z) - c)
            |]
        let wireBoxes (boxes : Box3d[]) = boxes |> Array.collect box2lines
        let boundsToShowCorrected = Mod.constant (wireBoxes boundsToShow)
        let boundsToShowSg =
            boundsToShowCorrected
            |> Sg.lines (Mod.constant C4b.Gray)
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.thickLine
                do! DefaultSurfaces.vertexColor
            }
            |> Sg.uniform "LineWidth" (Mod.constant 1.0)

        // scene graph
        let sg = 
            [pcsg; bboxSg; boundsToShowSg] // coordinateCross
                |> Sg.ofList
                |> Sg.viewTrafo viewTrafo
                |> Sg.projTrafo projTrafo
        
        // keyboard bindings
        win.Keyboard.Down.Values.Add ( fun k ->
            match k with
            | Keys.P -> 
                transact ( fun _ -> pss.Value <- pss.Value * 1.25)
                Log.line "PointSize: %f" pss.Value
            | Keys.O -> 
                transact ( fun _ -> pss.Value <- pss.Value / 1.25)
                Log.line "PointSize: %f" pss.Value

            | Keys.T -> 
                transact ( fun _ -> tpd.Value <- tpd.Value * 1.25)
                Log.line "TargetPixelDistance: %f" tpd.Value
            | Keys.R -> 
                transact ( fun _ -> tpd.Value <- tpd.Value / 1.25)
                Log.line "TargetPixelDistance: %f" tpd.Value
            
            | Keys.OemPlus -> 
                transact ( fun _ -> speed.Value <- speed.Value * 1.25)
                Log.line "CameraSpeed: %f" speed.Value
            | Keys.OemMinus ->
                transact ( fun _ -> speed.Value <- speed.Value / 1.25)
                Log.line "CameraSpeed: %f" speed.Value

            | Keys.C ->
                transact ( fun _ ->
                    coloring.Value <-
                        match coloring.Value with
                        | Model.PointColoring.Colors -> Model.PointColoring.Labels
                        | Model.PointColoring.Labels -> Model.PointColoring.Normals
                        | Model.PointColoring.Normals -> Model.PointColoring.Colors
                        | _ -> Model.PointColoring.Colors
                )
                Log.line "PointColoring: %s" (coloring.Value.ToString())
                
            | _ -> ()
        )

        win.RenderTask <- (app.Runtime.CompileRender(win.FramebufferSignature, sg))
        win.Run()

    let info (filename : string) (args : Args) =
        initPointFileFormats ()
        let info = PointCloud.ParseFileInfo(filename, ImportConfig.Default)
        Console.WriteLine("filename      {0}", info.FileName)
        Console.WriteLine("format        {0}", info.Format.Description)
        Console.WriteLine("file size     {0:N0} bytes", info.FileSizeInBytes)
        Console.WriteLine("point count   {0:N0}", info.PointCount)
        Console.WriteLine("bounds        {0}", info.Bounds)

    let private foo (chunks : seq<Chunk>) =
        chunks
        |> Seq.map (fun chunk ->
        
            let k = 27
            let ps = chunk.Positions.ToArray()
            let cs = chunk.Colors.ToArray()

            let sw = Stopwatch()
            sw.Restart()
            let kd = ps.CreateRkdTree(Metric.Euclidean, 0.0)
            sw.Stop()
            printfn "kd-tree creation: %A" sw.Elapsed

            sw.Restart()
            let median = ps.Sum() / float ps.Length
            let cs = ps
                     |> Array.map2 (fun (c : C4b) p ->
                     
                        if p.Z < median.Z then
                            c
                        else
                            let closest = kd.GetClosest(p, 0.30, k)
                            
                            if closest.Count < 8 then
                                c
                            else
                                let mutable q = M33d.Zero
                                let mutable w = V3d.Zero
                                let mutable cvm = ps.ComputeCovarianceMatrix(closest, p)
                                let mutable o : int[] = null
                                if Eigensystems.Dsyevh3asc(&cvm, &q, &w, &o) then
                                    let flatness = 1.0 - w.[o.[0]] / w.[o.[1]]
                                    let verticality = V3d.Dot(q.Column(o.[0]).Normalized.Abs, V3d.ZAxis)
                                    if flatness > 0.99 && verticality > 0.7 then C4b(255uy, c.G / 2uy, c.B / 2uy) else c
                                else
                                    c
                        ) cs
            sw.Stop()
            printfn "classification: %A" sw.Elapsed
            chunk.WithColors(cs)
            )

    let private mapReduce cfg (chunks : seq<Chunk>) = PointCloud.Chunks(chunks, cfg)
    let private batchImport dirname (cfg : ImportConfig) (args : Args) =
        getKnownFileExtensions ()
        |> Seq.collect (fun x -> Directory.EnumerateFiles(dirname, "*" + x, SearchOption.AllDirectories))
        |> Seq.skip args.skip
        |> Seq.truncate args.take
        |> Seq.collect (fun f ->
            printfn "importing file %s" f
            Import.Laszip.Chunks(f, cfg)
            )
        |> mapReduce cfg

    let import (filename : string) (store : string) (id : string) (args : Args) =
    
        let isBatchImport = try Directory.Exists(filename) with _ -> false

        let filename =
            if filename.StartsWith "http" then
                if not (Directory.Exists(store)) then
                    Directory.CreateDirectory(store) |> ignore
                let wc = new WebClient()
                let fn = Uri(filename).Segments.Last()
                let targetFilename = Path.Combine(store, fn)
                printfn "downloading %s to %s" filename targetFilename
                wc.DownloadFile(filename, targetFilename)
                targetFilename
            else
                filename
            
        use store = PointCloud.OpenStore(store)
        
        let cfg =
            ImportConfig.Default
                .WithStorage(store)
                .WithKey(id)
                .WithVerbose(true)
                .WithMaxChunkPointCount(10000000)
                .WithMinDist(match args.minDist with | None -> 0.0 | Some x -> x)
        
        initPointFileFormats ()
        
        let ps =
            match isBatchImport, args.asciiFormat with
            
            // single file, known format
            | false, None   -> let sw = Stopwatch()
                               sw.Restart()
                               let chunks = Import.Ply.Chunks(filename, cfg) |> foo
                               sw.Stop()
                               printfn "parsing: %A" sw.Elapsed
                               PointCloud.Chunks(chunks, cfg)
                               
                               // PointCloud.Import(filename, cfg)
                 
            // single file, custom ascii
            | false, Some f -> let chunks = Import.Ascii.Chunks(filename, f, cfg)
                               PointCloud.Chunks(chunks, cfg)

            // batch, known formats
            | true, None    -> batchImport filename cfg args

            // batch, custom ascii
            | _             -> failwith "batch import with custom ascii format is not supported"

        Console.WriteLine("point count   {0:N0}", ps.PointCount)
        Console.WriteLine("bounds        {0}", ps.BoundingBox)

    let download (baseurl : string) (targetdir : string) (args : Args) =

        let xs = Download.listHrefsForKnownFormats baseurl
        Console.WriteLine("found {0:N0} point cloud files", xs.Count())
        Download.batchDownload baseurl targetdir xs