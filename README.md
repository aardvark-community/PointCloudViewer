# hum – a viewer for **hum**ongous point clouds

Supported file formats are `.las, .laz, .e57, .ply, .pts, .yxh, ascii`.

## Quickstart

Import a point cloud file
```
hum import mylaserscan.laz path/to/my/store scan1
```
and view it like this:
```
hum view path/to/my/store scan1
```

**ProTip:** A store can hold multiple point clouds. Just use different keys.

**Advise:** If your graphics card does not support Vulkan, use `-gl` to switch to OpenGL rendering.

## Build Instructions

Clone the repository:
```
git clone https://github.com/aardvark-community/hum.git
cd hum
```

Build the project:
```
build
```

Deploys executable `hum.exe` to `./pub` directory:
```
publish
```

## Usage

```
usage: hum <command> <args...>                                                        
                                                                                      
    info <filename>                 print point cloud file info                       
                                                                                      
    import <filename> <store> <id>  import <filename> into <store> with <id>          
        [-mindist <dist>]              skip points on import, which are less          
                                         than given distance from previous point,     
                                         e.g. -mindist 0.001                          
        [-n <k>]                       estimate per-point normals                     
                                         using k-nearest neighbours,                  
                                         e.g. -n 16                                   
        [-ascii <lineformat>]          import custom ascii format                     
                                         e.g. -ascii "x y z _ r g b"                  
                                       format symbols                                 
                                         position      : x, y, z                      
                                         normal        : nx, ny, nz                   
                                         color         : r, g, b, a                   
                                         color (float) : rf, gf, bf, af               
                                         intensity     : i                            
                                         skip          : _                            
                                                                                      
    view <store> <id>               show point cloud with <id> in given <store>       
        [-gl]                            use OpenGL instead of Vulkan                 
        [-vulkan]                        use Vulkan (default)                         
        [-near <dist>]                   near plane distance, default 1.0             
        [-far <dist>]                    far plane distance, default 5000.0           
        [-fov <degrees>]                 horizontal field-of-view, default 60.0       
                                    keyboard shortcuts                                
                                         <A>/<D> ... left/right                       
                                         <W>/<S> ... forward/back                     
                                         <+>/<-> ... camera speed                     
                                         <P>/<O> ... point size (+/-)                 
                                         <T>/<R> ... target pixel distance (+/-)      
                                         <C> ....... cycle color scheme               
                                                     (colors, classification, normals)
                                         <↑>/<↓> ... octree level visualization (+/-) 
                                                                                      
    download <baseurl> <targetdir>  bulk download of point cloud files                
                                      scans webpage at <baseurl> for hrefs to         
                                      files with known file extensions and            
                                      download to <targetdir>                         
                                                                                      
    gui                             start in GUI mode                                                       
```

## Licensing

This software is licensed under the [GNU Affero General Public License 3.0](https://www.gnu.org/licenses/agpl-3.0.en.html).
