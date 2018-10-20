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

module Main = 

    [<EntryPoint;STAThread>]
    let main argv = 
    
        Console.ForegroundColor <- ConsoleColor.White
        Console.WriteLine("---------------------------------------------------------------------------------------")
        Console.Write("A viewer for ")
        Console.ForegroundColor <- ConsoleColor.Green
        Console.Write("hum")
        Console.ForegroundColor <- ConsoleColor.White
        Console.WriteLine("ongous point clouds.")
        Console.WriteLine("Copyright (c) 2018. purefunctional.io")
        Console.WriteLine("https://github.com/aardvark-community/hum")
        Console.WriteLine("---------------------------------------------------------------------------------------")
        Console.ResetColor()
        
        CmdLine.processArgs argv

        0
