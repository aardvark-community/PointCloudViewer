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

open Aardium
open Aardvark.Application.Slim
open Aardvark.Base
open Aardvark.Rendering.Vulkan
open Aardvark.Service
open Aardvark.UI
open Suave
open Suave.WebPart
open System

[<AutoOpen>]
module Main =

    let private startMediaApp port args =

        Ag.initialize()
        Aardvark.Init()
        Aardium.init()
    
        if args.useVulkan then
            let app = new HeadlessVulkanApplication(true)
            WebPart.startServer port [
                MutableApp.toWebPart' app.Runtime false (App.start (App.app args))
            ]
        else
            let app = new OpenGlApplication(true)
            WebPart.startServer port [
                MutableApp.toWebPart' app.Runtime false (App.start (App.app args))
            ]

        Aardium.run {
            title "hum - a viewer for humongous point clouds"
            width 1024
            height 768
            url (sprintf "http://localhost:%i/" port)
        }

    [<EntryPoint;STAThread>]
    let main argv =

        printHeader ()

        let args = Args.parse argv

        match args.command with
        | None -> printUsage ()
        | Some (Info filename) -> info filename args
        | Some (Import (filename, store, key)) -> import filename store key args
        | Some (View (store, key)) ->
            match args.port with
            | None -> view store key args
            | Some port -> startMediaApp port args
            
        0
  