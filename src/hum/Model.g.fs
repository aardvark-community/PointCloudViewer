namespace hum.Model

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open hum.Model

[<AutoOpen>]
module Mutable =

    
    
    type MModel(__initial : hum.Model.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<hum.Model.Model> = Aardvark.Base.Incremental.EqModRef<hum.Model.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<hum.Model.Model>
        let _cameraState = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.cameraState)
        let _store = MOption.Create(__initial.store)
        let _pointSet = MOption.Create(__initial.pointSet)
        let _createLod = ResetMod.Create(__initial.createLod)
        let _pointSize = ResetMod.Create(__initial.pointSize)
        let _targetPixelDistance = ResetMod.Create(__initial.targetPixelDistance)
        let _useClassificationForColoring = ResetMod.Create(__initial.useClassificationForColoring)
        
        member x.cameraState = _cameraState
        member x.store = _store :> IMod<_>
        member x.pointSet = _pointSet :> IMod<_>
        member x.createLod = _createLod :> IMod<_>
        member x.pointSize = _pointSize :> IMod<_>
        member x.targetPixelDistance = _targetPixelDistance :> IMod<_>
        member x.useClassificationForColoring = _useClassificationForColoring :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : hum.Model.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                Aardvark.UI.Primitives.Mutable.MCameraControllerState.Update(_cameraState, v.cameraState)
                MOption.Update(_store, v.store)
                MOption.Update(_pointSet, v.pointSet)
                ResetMod.Update(_createLod,v.createLod)
                ResetMod.Update(_pointSize,v.pointSize)
                ResetMod.Update(_targetPixelDistance,v.targetPixelDistance)
                ResetMod.Update(_useClassificationForColoring,v.useClassificationForColoring)
                
        
        static member Create(__initial : hum.Model.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : hum.Model.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<hum.Model.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let cameraState =
                { new Lens<hum.Model.Model, Aardvark.UI.Primitives.CameraControllerState>() with
                    override x.Get(r) = r.cameraState
                    override x.Set(r,v) = { r with cameraState = v }
                    override x.Update(r,f) = { r with cameraState = f r.cameraState }
                }
            let store =
                { new Lens<hum.Model.Model, Microsoft.FSharp.Core.Option<Aardvark.Data.Points.Storage>>() with
                    override x.Get(r) = r.store
                    override x.Set(r,v) = { r with store = v }
                    override x.Update(r,f) = { r with store = f r.store }
                }
            let pointSet =
                { new Lens<hum.Model.Model, Microsoft.FSharp.Core.Option<Aardvark.Geometry.Points.PointSet>>() with
                    override x.Get(r) = r.pointSet
                    override x.Set(r,v) = { r with pointSet = v }
                    override x.Update(r,f) = { r with pointSet = f r.pointSet }
                }
            let createLod =
                { new Lens<hum.Model.Model, System.Boolean>() with
                    override x.Get(r) = r.createLod
                    override x.Set(r,v) = { r with createLod = v }
                    override x.Update(r,f) = { r with createLod = f r.createLod }
                }
            let pointSize =
                { new Lens<hum.Model.Model, System.Double>() with
                    override x.Get(r) = r.pointSize
                    override x.Set(r,v) = { r with pointSize = v }
                    override x.Update(r,f) = { r with pointSize = f r.pointSize }
                }
            let targetPixelDistance =
                { new Lens<hum.Model.Model, System.Double>() with
                    override x.Get(r) = r.targetPixelDistance
                    override x.Set(r,v) = { r with targetPixelDistance = v }
                    override x.Update(r,f) = { r with targetPixelDistance = f r.targetPixelDistance }
                }
            let useClassificationForColoring =
                { new Lens<hum.Model.Model, System.Boolean>() with
                    override x.Get(r) = r.useClassificationForColoring
                    override x.Set(r,v) = { r with useClassificationForColoring = v }
                    override x.Update(r,f) = { r with useClassificationForColoring = f r.useClassificationForColoring }
                }
