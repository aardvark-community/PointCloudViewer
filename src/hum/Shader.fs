﻿(*
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

open Aardvark.Application
open Aardvark.Base
open Aardvark.Base.Rendering
open FShade

type BillboardVertex =
    {
        [<Position>] position                           : V4d
        [<Color>] color                                 : V4d
        [<PointCoord>] texCoord                         : V2d
        [<SemanticAttribute("ViewPosition")>] viewPos   : V4d
        [<PointSize>] size                              : float
    }

module Surfaces = 
    
    let pointsprite =
        let BillboardPosExtract ( v : BillboardVertex ) =
            vertex {

                return { 
                    position = V4d(v.position.XYZ, 1.0)
                    color = v.color                    
                    texCoord = V2d.OO
                    viewPos = v.viewPos
                    size = uniform?PointSize
                }
                    
            }

        let BillboardTrafo (v : BillboardVertex) =
            vertex {
                let viewPos = uniform.ModelViewTrafo * v.position

                return {
                    position = uniform.ProjTrafo * viewPos
                    color = v.color
                    texCoord = V2d.OO
                    viewPos = viewPos
                    size = v.size
                }
            }

        //let BillboardGeometry (v : Point<BillboardVertex>) =
        //    triangle {
        //        let offset = V2d(0.02, 0.02)
        //        let viewPos = v.Value.viewPos
        //        let color = v.Value.color
        //        let vs = v.Value.sel

        //        let TL =  uniform.ProjTrafo * (viewPos + V4d(-offset.X, -offset.Y, 0.0, 0.0))
        //        let TR =  uniform.ProjTrafo * (viewPos + V4d( offset.X, -offset.Y, 0.0, 0.0))
        //        let BL =  uniform.ProjTrafo * (viewPos + V4d(-offset.X,  offset.Y, 0.0, 0.0))
        //        let BR =  uniform.ProjTrafo * (viewPos + V4d( offset.X,  offset.Y, 0.0, 0.0))
                    
        //        yield { position = TL; color = color; texCoord = V2d(0, 1); viewPos = viewPos; sel = vs }
        //        yield { position = TR; color = color; texCoord = V2d(1, 1); viewPos = viewPos; sel = vs }
        //        yield { position = BL; color = color; texCoord = V2d(0, 0); viewPos = viewPos; sel = vs }
        //        yield { position = BR; color = color; texCoord = V2d(1, 0); viewPos = viewPos; sel = vs }
        //        }

        let BillboardFragment (v : BillboardVertex) =
            fragment {
                //let dist = v.texCoord - V2d(0.5, 0.5)
                    
                //if v.color.W = 0.0 || dist.Length > 0.5 then
                //    discard()
                //    return V4d(0,0,0,0)
                //else
                return v.color
            } 

        [
            BillboardPosExtract|> toEffect
            DefaultSurfaces.trafo     |> toEffect
            DefaultSurfaces.vertexColor  |> toEffect
            BillboardFragment  |> toEffect
        ]



module Util =
    let coordinateBox (size : float) =
        let s = size

        let ppp = V3d( s, s, s)
        let ppm = V3d( s, s,-s)
        let pmp = V3d( s,-s, s)
        let pmm = V3d( s,-s,-s)
        let mpp = V3d(-s, s, s)
        let mpm = V3d(-s, s,-s)
        let mmp = V3d(-s,-s, s)
        let mmm = V3d(-s,-s,-s)

        let hi = 70
        let lo = 30

        let qf = IndexedGeometryPrimitives.Quad.solidQuadrangle' pmp ppp ppm pmm (C4b(hi,lo,lo,255))
        let qb = IndexedGeometryPrimitives.Quad.solidQuadrangle' mmp mmm mpm mpp (C4b(lo,hi,hi,255))
        let ql = IndexedGeometryPrimitives.Quad.solidQuadrangle' pmp pmm mmm mmp (C4b(lo,hi,lo,255))
        let qr = IndexedGeometryPrimitives.Quad.solidQuadrangle' ppp mpp mpm ppm (C4b(hi,lo,hi,255))
        let qu = IndexedGeometryPrimitives.Quad.solidQuadrangle' pmp ppp mpp mmp (C4b(lo,lo,hi,255))
        let qd = IndexedGeometryPrimitives.Quad.solidQuadrangle' pmm mmm mpm ppm (C4b(hi,hi,lo,255))

        [qf; qb; ql; qr; qu; qd]
    


