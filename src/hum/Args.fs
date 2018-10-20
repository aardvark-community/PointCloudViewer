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
open System.Globalization

type ArgsCommand =
    /// Info filename
    | Info of string
    /// Import (filename, store, key)
    | Import of string * string * string
    /// View (store, key)
    | View of string * string
    
type Args =
    {
        command             : Option<ArgsCommand>
        useVulkan           : bool
        port                : Option<int>

        /// import command: minDist
        minDist             : Option<float>
        /// import command: format for ascii parser
        asciiFormat         : Option<Ascii.Token[]>

        /// view command: renders bounding boxes from given file 
        showBoundsFileName  : Option<string>

        /// near plane distance
        nearPlane           : float
        /// far plane distance
        farPlane            : float
    }

module Args =

    let private defaultArgs = {  
        command = None
        useVulkan = true
        port = None
        minDist = None
        asciiFormat = None
        showBoundsFileName = None
        nearPlane = 1.0
        farPlane = 50000.0
    }
    
    (* parse ascii-parser format string *)
    let rec private parseAsciiFormat' xs rs =
        match xs with
        | [] -> List.rev rs
        | x :: xs -> let r = match x with
                             | "x" -> Ascii.Token.PositionX
                             | "y" -> Ascii.Token.PositionY
                             | "z" -> Ascii.Token.PositionZ
                             | "nx" -> Ascii.Token.NormalX 
                             | "ny" -> Ascii.Token.NormalY 
                             | "nz" -> Ascii.Token.NormalZ 
                             | "i" ->  Ascii.Token.Intensity
                             | "r" ->  Ascii.Token.ColorR
                             | "g" ->  Ascii.Token.ColorG
                             | "b" ->  Ascii.Token.ColorB
                             | "a" ->  Ascii.Token.ColorA
                             | "rf" -> Ascii.Token.ColorRf
                             | "gf" -> Ascii.Token.ColorGf
                             | "bf" -> Ascii.Token.ColorBf
                             | "af" -> Ascii.Token.ColorAf
                             | "_" ->  Ascii.Token.Skip
                             | _ -> failwith "unknown token"
                     parseAsciiFormat' xs (r :: rs)

    let private parseAsciiFormat (s : string) =
        Array.ofList (parseAsciiFormat' (List.ofArray (s.Split(' '))) [])

    (* parse command line args *)
    let rec private parse' (a : Args) (argv : string list) : Args =
        match argv with
        | [] -> a
        | "info" :: filename :: xs
            -> parse' { a with command = Some (Info filename) } xs

        | "import" :: filename :: store :: key :: xs
            -> parse' { a with command = Some (Import (filename, store, key)) } xs

        | "view" :: store :: key :: xs
            -> parse' { a with command = Some (View (store, key)) } xs

        | "-opengl" :: xs
        | "-ogl" :: xs
        | "-gl" :: xs           -> parse' { a with useVulkan = false } xs
        | "-vulkan" :: xs       -> parse' { a with useVulkan = true } xs

        | "-port" :: x :: xs
        | "-p" :: x :: xs       -> parse' { a with port = Some (Int32.Parse x) } xs
        | "-port" :: []         
        | "-p" :: []            -> failwith "missing argument: -p <???>"

        | "-mindist" :: x :: xs
        | "-md" :: x :: xs      -> parse' { a with minDist = Some (Double.Parse(x, CultureInfo.InvariantCulture)) } xs
        | "-mindist" :: []      
        | "-md" :: []           -> failwith "missing argument: -md <???>"

        | "-ascii" :: f :: xs   -> parse' { a with asciiFormat = Some (parseAsciiFormat f) } xs
        | "-ascii" :: []        -> failwith "missing argument: -ascii <???>"

        | "-near" :: x :: xs    -> parse' { a with nearPlane = Double.Parse(x, CultureInfo.InvariantCulture) } xs
        | "-near" :: []         -> failwith "missing argument: -near <???>"

        | "-far" :: x :: xs     -> parse' { a with farPlane = Double.Parse(x, CultureInfo.InvariantCulture) } xs
        | "-far" :: []          -> failwith "missing argument: -far <???>"
        
        | "-sb" :: fn :: xs     -> parse' { a with showBoundsFileName = Some fn } xs
        | "-sb" :: []           -> failwith "missing argument: -sb <???>"

        | x :: _                -> printf "unknown argument '%s'" x
                                   printUsage ()
                                   Environment.Exit(1)
                                   failwith "never reached, but makes compiler happy ;-)"
        
    /// Parses command line arguments.
    let parse (argv : string[]) : Args =
        parse' defaultArgs (List.ofArray argv)
