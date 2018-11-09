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
open Aardvark.Base
open Aardvark.Base.Rendering
open Aardvark.Geometry.Points
open Aardvark.Base.Incremental

module Lod =
    open hum.Model
    
    type OctreeILodDataNode( globalCenter : V3d, node : PersistentRef<PointSetNode>, level : int ) =
        member x.Identifier = node.Value.Id
        member x.Node = node.Value :> obj
        member x.Level = level
        member x.Bounds = node.Value.BoundingBox - globalCenter
        member x.LocalPointCount = node.Value.LodPointCount
        member x.Children = 
            if node.Value.IsNotLeaf then
                let sn = node.Value.Subnodes 
                let filtered = 
                    sn |> Array.choose ( fun node -> 
                        if node = null then
                            None
                        else
                            Some (OctreeILodDataNode(globalCenter,node,level+1) :> ILodDataNode)
                    )
                Some filtered
            else
                None

        override x.GetHashCode() = x.Identifier.GetHashCode()
        override x.Equals o =
            match o with
                | :? OctreeILodDataNode as o -> x.Identifier = o.Identifier
                | _ -> false

        interface ILodDataNode with
            member x.Id = x.Node
            member x.Level = level
            member x.Bounds = x.Bounds
            member x.LocalPointCount = x.LocalPointCount
            member x.Children = x.Children

    type OctreeLodData(ps : PointSet) =
        
        let globalCenter = ps.BoundingBox.Center
        
        let root = lazy ( OctreeILodDataNode(globalCenter, ps.Root,0) :> ILodDataNode )
        
        member x.BoundingBox = ps.BoundingBox

        member x.RootNode() = root.Value

        member x.Dependencies = []

        member x.GetData (node : ILodDataNode) : Async<Option<IndexedGeometry>> =
            async {
                let realNode = unbox<PointSetNode> node.Id
                let shiftGlobal = realNode.Center - globalCenter
                
                let pos = realNode.LodPositions.Value  |> Array.map(fun p -> V4f(V3f(shiftGlobal + (V3d p)), 1.0f))
                //let normals = 
                //    match realNode.HasNormals with
                //    | true -> realNode.LodNormals.Value |> Array.map(fun p -> V4f(p, 0.0f))
                //    | false -> [| |]

                //let labels =
                //    match realNode.HasLodClassifications with
                //    | true -> realNode.LodClassifications.Value |> Array.map (fun c -> int(c))
                //    | false -> [| |]
        
                let colors = 
                    match realNode.HasLodColors with
                    | true -> realNode.LodColors.Value
                    | false -> [| |]
                    
                //let col = match realNode.HasLodColors, realNode.HasLodClassifications with
                //          | true , _ -> realNode.LodColors.Value
                //          | _    , true  -> realNode.LodClassifications.Value |> Array.map (fun c -> colorScheme.[int(c)])
                //          | false, false -> realNode.LodPositions.Value |> Array.map (fun _ -> C4b.Gray)
                
                let r = 
                    IndexedGeometry(
                        Mode = IndexedGeometryMode.PointList,
                        IndexedAttributes =
                            SymDict.ofList [
                                DefaultSemantic.Positions, pos :> Array
                                DefaultSemantic.Colors, colors :> Array
                            ]
                    )
                return Some r
                }

        interface ILodData with
            member x.BoundingBox = x.BoundingBox
            member x.RootNode() = x.RootNode()
            member x.Dependencies = x.Dependencies
            member x.GetData node = x.GetData node

