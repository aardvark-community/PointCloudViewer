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

open System
open System.Threading
open System.Collections.Generic
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.Rendering.Vulkan
open Aardvark.SceneGraph
open Aardvark.Application
open Aardvark.Application.Slim
open Aardvark.Geometry.Points
open Aardvark.Data.Points
open Aardvark.Data.Points.Import
open Newtonsoft.Json.Linq
open Uncodium.SimpleStore

module CmdLine = 

    let usage () =
        Log.line "usage: humcli <command> <args...>"
        Log.line "    view <store> <id>               shows pointcloud with given <id> in given <store>"
        Log.line "        [-gl]                            uses OpenGL instead of Vulkan"
        Log.line "    info <filename>                 prints info (e.g. point count, bounding box, ...)"
        Log.line "    import <filename> <store> <id>  imports <filename> into <store> with <id>"
        Log.line "        [-mindist <dist>]              skips points on import, which are less than"
        Log.line "                                         given distance from previous point, e.g. -mindist 0.001"
        Log.line "        [-ascii <format>]              e.g. -ascii \"x y z _ r g b\""
        Log.line "                                         position      : x,y,z"
        Log.line "                                         normal        : nx,ny,nz"
        Log.line "                                         color         : r,g,b,a"
        Log.line "                                         color (float) : rf, gf, bf, af"
        Log.line "                                         intensity     : i"
        Log.line "                                         skip          : _"

    let init () =
        Import.Pts.PtsFormat |> ignore
        Import.E57.E57Format |> ignore
        Import.Yxh.YxhFormat |> ignore
        Import.Ply.PlyFormat |> ignore
        Import.Laszip.LaszipFormat |> ignore
        
    let parseBounds filename =
        let json = JArray.Parse(File.readAllText filename) :> seq<JToken>
        json
            |> Seq.map (fun j -> Box3d.Parse(j.["Bounds"].ToObject<string>()))
            |> Array.ofSeq

    let getLineDefinitions (s : string) =
        let result = new List<Ascii.Token>()
        for c in s.SplitOnWhitespace() do
            match c with
            | "x" -> result.Add(Ascii.Token.PositionX)
            | "y" -> result.Add(Ascii.Token.PositionY)
            | "z" -> result.Add(Ascii.Token.PositionZ)
            | "nx" -> result.Add(Ascii.Token.NormalX)
            | "ny" -> result.Add(Ascii.Token.NormalY)
            | "nz" -> result.Add(Ascii.Token.NormalZ)
            | "i" -> result.Add(Ascii.Token.Intensity)
            | "r" -> result.Add(Ascii.Token.ColorR)
            | "g" -> result.Add(Ascii.Token.ColorG)
            | "b" -> result.Add(Ascii.Token.ColorB)
            | "a" -> result.Add(Ascii.Token.ColorA)
            | "rf" -> result.Add(Ascii.Token.ColorRf)
            | "gf" -> result.Add(Ascii.Token.ColorGf)
            | "bf" -> result.Add(Ascii.Token.ColorBf)
            | "af" -> result.Add(Ascii.Token.ColorAf)
            | "_" -> result.Add(Ascii.Token.Skip)
            | _ -> failwith "unknown token"
        result.ToArray()

    let view (store : string) (id : string) (argv : string[]) =

        let mutable useVulkan = true
        let mutable showBoundsFileName = None
        let mutable i = 0
        while i < argv.Length do
            match argv.[i] with
            | "-gl" | "-ogl" -> useVulkan <- false
            | "-sb" -> showBoundsFileName <- Some argv.[i + 1]
                       i <- i + 1
            | _ -> failwith (sprintf "unknown argument %s" argv.[i])

            i <- i + 1
            

        let boundsToShow =
            match showBoundsFileName with
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
        let initialView = CameraView.lookAt V3d.OIO V3d.Zero V3d.OOI
        let view = initialView |> DefaultCameraController.controlWithSpeed speed win.Mouse win.Keyboard win.Time
        let viewTrafo = view |> Mod.map CameraView.viewTrafo

        let proj = win.Sizes |> Mod.map (fun s -> Frustum.perspective 60.0 1.0 50000.0 (float s.X / float s.Y))
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
            | Keys.U -> 
                transact ( fun _ -> tpd.Value <- tpd.Value * 1.5)
                Log.line "TargetPixelDistance: %f" tpd.Value
            | Keys.I -> 
                transact ( fun _ -> tpd.Value <- tpd.Value / 1.5)
                Log.line "TargetPixelDistance: %f" tpd.Value
            | Keys.O -> 
                transact ( fun _ -> pss.Value <- pss.Value * 1.5)
                Log.line "PointSize: %f" pss.Value
            | Keys.P -> 
                transact ( fun _ -> pss.Value <- pss.Value / 1.5)
                Log.line "PointSize: %f" pss.Value
            | Keys.K -> 
                transact ( fun _ -> speed.Value <- speed.Value * 1.5)
                Log.line "CameraSpeed: %f" speed.Value
            | Keys.L -> 
                transact ( fun _ -> speed.Value <- speed.Value / 1.5)
                Log.line "CameraSpeed: %f" speed.Value
            | _ -> ()
        )

        win.RenderTask <- (app.Runtime.CompileRender(win.FramebufferSignature, sg))
        win.Run()

    let info (filename : string) (argv : string[]) =
        init ()
        let info = PointCloud.ParseFileInfo(filename, ImportConfig.Default)
        Console.WriteLine("filename      {0}", info.FileName)
        Console.WriteLine("format        {0}", info.Format.Description)
        Console.WriteLine("file size     {0:N0} bytes", info.FileSizeInBytes)
        Console.WriteLine("point count   {0:N0}", info.PointCount)
        Console.WriteLine("bounds        {0}", info.Bounds)

    let import (filename : string) (store : string) (id : string) (argv : string[]) =

        let mutable minDist = 0.0
        let mutable asciiFormat = None

        let mutable i = 0
        while i < argv.Length do
            match argv.[i] with
            | "-md"
            | "-mindist" -> minDist <- Double.Parse(argv.[i + 1])
                            i <- i + 1
            | "-ascii"   -> asciiFormat <- Some argv.[i + 1]
                            i <- i + 1
            | _ -> failwith (sprintf "unknown argument %s" argv.[i])

            i <- i + 1

        use store = PointCloud.OpenStore(store)

        let cfg =
            ImportConfig.Default
                .WithStorage(store)
                .WithKey(id)
                .WithVerbose(true)
                .WithMinDist(minDist)
        
        init ()
        
        let ps = match asciiFormat with
                 | None -> PointCloud.Import(filename, cfg)
                 | Some f -> let chunks = Import.Ascii.Chunks(filename, getLineDefinitions f, cfg)
                             PointCloud.Chunks(chunks, cfg)

        Console.WriteLine("point count   {0:N0}", ps.PointCount)
        Console.WriteLine("bounds        {0}", ps.BoundingBox)

    let processArgs (argv : string[]) =
        if argv.Length > 0 then
            try
                match argv.[0] with
                | "view"   -> view argv.[1] argv.[2] (Array.skip 3 argv)
                | "info"   -> info argv.[1] (Array.skip 2 argv)
                | "import" -> import argv.[1] argv.[2] argv.[3] (Array.skip 4 argv)
                | _        -> usage ()
            with e ->
                Log.line "%A" e
                usage ()
        else
            usage ()