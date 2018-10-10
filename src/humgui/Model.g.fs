namespace gumgui.Model

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open gumgui.Model

[<AutoOpen>]
module Mutable =

    
    
    type MModel(__initial : gumgui.Model.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<gumgui.Model.Model> = Aardvark.Base.Incremental.EqModRef<gumgui.Model.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<gumgui.Model.Model>
        let _currentModel = ResetMod.Create(__initial.currentModel)
        let _cameraState = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.cameraState)
        let _store = MOption.Create(__initial.store)
        let _pointSet = MOption.Create(__initial.pointSet)
        
        member x.currentModel = _currentModel :> IMod<_>
        member x.cameraState = _cameraState
        member x.store = _store :> IMod<_>
        member x.pointSet = _pointSet :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : gumgui.Model.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_currentModel,v.currentModel)
                Aardvark.UI.Primitives.Mutable.MCameraControllerState.Update(_cameraState, v.cameraState)
                MOption.Update(_store, v.store)
                MOption.Update(_pointSet, v.pointSet)
                
        
        static member Create(__initial : gumgui.Model.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : gumgui.Model.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<gumgui.Model.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let currentModel =
                { new Lens<gumgui.Model.Model, gumgui.Model.Primitive>() with
                    override x.Get(r) = r.currentModel
                    override x.Set(r,v) = { r with currentModel = v }
                    override x.Update(r,f) = { r with currentModel = f r.currentModel }
                }
            let cameraState =
                { new Lens<gumgui.Model.Model, Aardvark.UI.Primitives.CameraControllerState>() with
                    override x.Get(r) = r.cameraState
                    override x.Set(r,v) = { r with cameraState = v }
                    override x.Update(r,f) = { r with cameraState = f r.cameraState }
                }
            let store =
                { new Lens<gumgui.Model.Model, Microsoft.FSharp.Core.Option<Aardvark.Data.Points.Storage>>() with
                    override x.Get(r) = r.store
                    override x.Set(r,v) = { r with store = v }
                    override x.Update(r,f) = { r with store = f r.store }
                }
            let pointSet =
                { new Lens<gumgui.Model.Model, Microsoft.FSharp.Core.Option<Aardvark.Geometry.Points.PointSet>>() with
                    override x.Get(r) = r.pointSet
                    override x.Set(r,v) = { r with pointSet = v }
                    override x.Update(r,f) = { r with pointSet = f r.pointSet }
                }
