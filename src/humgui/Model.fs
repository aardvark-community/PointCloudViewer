namespace gumgui.Model

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI.Primitives
open Aardvark.Geometry.Points
open Aardvark.Data.Points

type Primitive =
    | Box
    | Sphere


[<DomainType>]
type Model =
    {
        currentModel    : Primitive
        cameraState     : CameraControllerState

        store           : Option<Storage>
        pointSet        : Option<PointSet>
    }