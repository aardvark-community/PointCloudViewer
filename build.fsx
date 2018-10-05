#load @"paket-files/build/aardvark-platform/aardvark.fake/DefaultSetup.fsx"

open Fake
open System
open System.IO
open System.Diagnostics
open Aardvark.Fake

do Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

DefaultSetup.install ["src/PointCloudViewer3.sln"]

Target "Start" (fun() ->
    let param (p : DotNetCli.CommandParams) =
        { p with WorkingDir = Path.Combine("bin", "Release", "netcoreapp2.0") }

    DotNetCli.RunCommand param "PointCloudViewer3.dll"
)

Target "Run" (fun () -> Run "Start")
"Compile" ==> "Run"

entry()