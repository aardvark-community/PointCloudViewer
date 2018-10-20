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
    | Info
    | Import
    | View
    
type Args =
    {
        command     : Option<ArgsCommand>
        useVulkan   : bool
        port        : Option<int>
        minDist     : Option<float>
        asciiFormat : Option<Ascii.Token[]>
    }

module Args =

    let private defaultArgs = {  
        command = None
        useVulkan = true
        port = None
        minDist = None
        asciiFormat = None
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
        | x :: xs ->
            let a = match x with
                    | "info" ->
                        { a with command = Some Info }

                    | "import" ->
                        { a with command = Some Import }

                    | "view" ->
                        { a with command = Some View }

                    | "-opengl" | "-ogl" | "-gl" ->
                        { a with useVulkan = false }

                    | "-vulkan" ->
                        { a with useVulkan = true }

                    | "-port" | "-p" ->
                        { a with port = Some (Int32.Parse x) }

                    | "-mindist" | "-md" ->
                        { a with minDist = Some (Double.Parse(x, CultureInfo.InvariantCulture)) }

                    | "-ascii" ->
                        match xs with
                        | [] -> failwith "missing argument: -ascii <???>"
                        | f :: xs -> { a with asciiFormat = Some (parseAsciiFormat f) }

                    | _
                        -> failwith (sprintf "unknown argument %s" x)
            parse' a xs
        
        

    /// Parses command line arguments.
    let parse (argv : string[]) : Args =
        parse' defaultArgs (List.ofArray argv)
