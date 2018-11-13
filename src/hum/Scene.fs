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

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.Geometry.Points
open Aardvark.SceneGraph
open hum.Model
        
module Scene =
   
    let createSceneGraph (m : MModel) (ps : PointSet) (boundsToShow : Box3d[]) : ISg =

        let data = Lod.OctreeLodData(ps)
        let center = ps.BoundingBox.Center
        
        let rasterizer = m.targetPixelDistance |> Mod.map LodData.defaultRasterizeSet
        
        let info = {
            lodRasterizer           = rasterizer
            attributeTypes = 
                Map.ofList [
                    DefaultSemantic.Positions, typeof<V4f>
                    DefaultSemantic.Colors, typeof<C4b>
                    DefaultSemantic.Normals, typeof<V3f>
                    DefaultSemantic.Label, typeof<int>
                ]
            freeze                  = Mod.constant false
            maxReuseRatio           = 0.5
            minReuseCount           = 1L <<< 20
            pruneInterval           = 500
            customView              = None
            customProjection        = None
            boundingBoxSurface      = None
            progressCallback        = None
        }
    
        let bboxSg =
            ps.BoundingBox
                |> Sg.wireBox' C4b.Red
                |> Sg.trafo (Trafo3d.Translation(-ps.BoundingBox.Center) |> Mod.constant)
                |> Sg.shader {
                    do! DefaultSurfaces.trafo
                    do! DefaultSurfaces.thickLine
                    do! DefaultSurfaces.vertexColor
                }
                |> Sg.uniform "LineWidth" (Mod.constant 5.0)
       
        let pcsg = 
            Sg.pointCloud data info
                |> Sg.effect Surfaces.pointsprite
                |> Sg.uniform "PointSize" m.pointSize
                |> Sg.uniform "PointColoring" m.coloring
                //|> Sg.uniform "ViewportSize" win.Sizes TODO

        let coordinateCross = 
            let cross =
                IndexedGeometryPrimitives.coordinateCross (V3d.III * 2.0)
                    |> Sg.ofIndexedGeometry
                    |> Sg.translate -6.0 -6.0 -6.0
                    |> Sg.shader {
                        do! DefaultSurfaces.trafo
                        do! DefaultSurfaces.thickLine
                        do! DefaultSurfaces.vertexColor
                    }
                    |> Sg.uniform "LineWidth" (Mod.constant 2.0)


            let box =
                Util.coordinateBox 500.0
                    |> List.map Sg.ofIndexedGeometry
                    |> Sg.ofList
                    |> Sg.shader {
                        do! DefaultSurfaces.trafo
                        do! DefaultSurfaces.vertexColor
                    }

            [cross; box] |> Sg.ofList

        // show bounding boxes (-sb)
        let c = center - V3d(0.0, 0.0, 50.0)
        let box2lines (box : Box3d) =
            [|
                Line3d(V3d(box.Min.X, box.Min.Y, box.Min.Z) - c, V3d(box.Max.X, box.Min.Y, box.Min.Z) - c);
                Line3d(V3d(box.Max.X, box.Min.Y, box.Min.Z) - c, V3d(box.Max.X, box.Max.Y, box.Min.Z) - c);
                Line3d(V3d(box.Max.X, box.Max.Y, box.Min.Z) - c, V3d(box.Min.X, box.Max.Y, box.Min.Z) - c);
                Line3d(V3d(box.Min.X, box.Max.Y, box.Min.Z) - c, V3d(box.Min.X, box.Min.Y, box.Min.Z) - c);
                                                                                                        
                Line3d(V3d(box.Min.X, box.Min.Y, box.Max.Z) - c, V3d(box.Max.X, box.Min.Y, box.Max.Z) - c);
                Line3d(V3d(box.Max.X, box.Min.Y, box.Max.Z) - c, V3d(box.Max.X, box.Max.Y, box.Max.Z) - c);
                Line3d(V3d(box.Max.X, box.Max.Y, box.Max.Z) - c, V3d(box.Min.X, box.Max.Y, box.Max.Z) - c);
                Line3d(V3d(box.Min.X, box.Max.Y, box.Max.Z) - c, V3d(box.Min.X, box.Min.Y, box.Max.Z) - c);
                                                                                                        
                Line3d(V3d(box.Min.X, box.Min.Y, box.Min.Z) - c, V3d(box.Min.X, box.Min.Y, box.Max.Z) - c);
                Line3d(V3d(box.Max.X, box.Min.Y, box.Min.Z) - c, V3d(box.Max.X, box.Min.Y, box.Max.Z) - c);
                Line3d(V3d(box.Max.X, box.Max.Y, box.Min.Z) - c, V3d(box.Max.X, box.Max.Y, box.Max.Z) - c);
                Line3d(V3d(box.Min.X, box.Max.Y, box.Min.Z) - c, V3d(box.Min.X, box.Max.Y, box.Max.Z) - c)
            |]
        let wireBoxes (boxes : Box3d[]) = boxes |> Array.collect box2lines
        let boundsToShowCorrected = Mod.constant (wireBoxes boundsToShow)
        let boundsToShowSg =
            boundsToShowCorrected
            |> Sg.lines (Mod.constant C4b.Gray)
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.thickLine
                do! DefaultSurfaces.vertexColor
            }
            |> Sg.uniform "LineWidth" (Mod.constant 1.0)

        // scene graph
        let sg = 
            [pcsg; bboxSg; boundsToShowSg] // coordinateCross
                |> Sg.ofList
                    
        sg
