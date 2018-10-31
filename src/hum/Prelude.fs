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

open Aardvark.Data.Points.Import
open System

[<AutoOpen>]
module Prelude =

    /// Print header.
    let printHeader () =
        Console.ForegroundColor <- ConsoleColor.White
        Console.WriteLine("---------------------------------------------------------------------------------------")
        Console.Write("A viewer for ")
        Console.ForegroundColor <- ConsoleColor.Green
        Console.Write("hum")
        Console.ForegroundColor <- ConsoleColor.White
        Console.WriteLine("ongous point clouds.")
        Console.WriteLine("Copyright (c) 2018. Georg Haaser, Stefan Maierhofer, Harald Steinlechner, Attila Szabo.")
        Console.WriteLine("https://github.com/aardvark-community/hum")
        Console.WriteLine("---------------------------------------------------------------------------------------")
        Console.ResetColor()
        
    /// Print usage message.
    let printUsage () =
        printfn "usage: hum <command> <args...>"
        printfn "    view <store> <id>               shows pointcloud with given <id> in given <store>"
        printfn "        [-gl]                            uses OpenGL instead of Vulkan"
        printfn "        [-vulkan]                        uses Vulkan (default)"
        printfn "        [-near <dist>]                   near plane distance, e.g. -near 1.0"
        printfn "        [-far <dist>]                    far plane distance, e.g. -far 2000.0"
        printfn "    info <filename>                 prints info (e.g. point count, bounding box, ...)"
        printfn "    import <filename> <store> <id>  imports <filename> into <store> with <id>"
        printfn "        [-mindist <dist>]              skips points on import, which are less than"
        printfn "                                         given distance from previous point, e.g. -mindist 0.001"
        printfn "        [-n <k>]                       estimate per-point normals (k-nearest neighbours)"
        printfn "        [-ascii <format>]              e.g. -ascii \"x y z _ r g b\""
        printfn "                                         position      : x,y,z"
        printfn "                                         normal        : nx,ny,nz"
        printfn "                                         color         : r,g,b,a"
        printfn "                                         color (float) : rf, gf, bf, af"
        printfn "                                         intensity     : i"
        printfn "                                         skip          : _"

    /// Init point cloud file formats.
    let initPointFileFormats () =
        Pts.PtsFormat |> ignore
        E57.E57Format |> ignore
        Yxh.YxhFormat |> ignore
        Ply.PlyFormat |> ignore
        Laszip.LaszipFormat |> ignore
  