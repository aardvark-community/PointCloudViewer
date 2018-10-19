namespace humgui.Model

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open humgui.Model

[<AutoOpen>]
module Mutable =

    
    
    type MModel(__initial : humgui.Model.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<humgui.Model.Model> = Aardvark.Base.Incremental.EqModRef<humgui.Model.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<humgui.Model.Model>
        let _cameraState = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.cameraState)
        let _store = MOption.Create(__initial.store)
        let _pointSet = MOption.Create(__initial.pointSet)
        let _createLod = ResetMod.Create(__initial.createLod)
        
        member x.cameraState = _cameraState
        member x.store = _store :> IMod<_>
        member x.pointSet = _pointSet :> IMod<_>
        member x.createLod = _createLod :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : humgui.Model.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                Aardvark.UI.Primitives.Mutable.MCameraControllerState.Update(_cameraState, v.cameraState)
                MOption.Update(_store, v.store)
                MOption.Update(_pointSet, v.pointSet)
                ResetMod.Update(_createLod,v.createLod)
                
        
        static member Create(__initial : humgui.Model.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : humgui.Model.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<humgui.Model.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let cameraState =
                { new Lens<humgui.Model.Model, Aardvark.UI.Primitives.CameraControllerState>() with
                    override x.Get(r) = r.cameraState
                    override x.Set(r,v) = { r with cameraState = v }
                    override x.Update(r,f) = { r with cameraState = f r.cameraState }
                }
            let store =
                { new Lens<humgui.Model.Model, Microsoft.FSharp.Core.Option<Aardvark.Data.Points.Storage>>() with
                    override x.Get(r) = r.store
                    override x.Set(r,v) = { r with store = v }
                    override x.Update(r,f) = { r with store = f r.store }
                }
            let pointSet =
                { new Lens<humgui.Model.Model, Microsoft.FSharp.Core.Option<Aardvark.Geometry.Points.PointSet>>() with
                    override x.Get(r) = r.pointSet
                    override x.Set(r,v) = { r with pointSet = v }
                    override x.Update(r,f) = { r with pointSet = f r.pointSet }
                }
            let createLod =
                { new Lens<humgui.Model.Model, System.Boolean>() with
                    override x.Get(r) = r.createLod
                    override x.Set(r,v) = { r with createLod = v }
                    override x.Update(r,f) = { r with createLod = f r.createLod }
                }
