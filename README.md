# hum - a viewer for *hum*ongous pointclouds

**hum** let's you view and interactively explore pointclouds with billions of points.

Supported file formats: `.las, .laz, .e57, .ply, .pts, .yxh, ascii`

## Quickstart

Import a pointcloud file with
```sh
hum import laserscan.laz path/to/my/store scan1
```
which will create a hum store at `path/to/my/store` and import `laserscan.laz` using key `scan1`.
If no store exists at the given directory, then it will be created automatically.

Now you can view the pointcloud with
```sh
hum view path/to/my/store scan1
```

**ProTip:** A store can hold multiple pointclouds. Just use different keys.

**Advise** If your graphics card does not support Vulkan, use `-gl` to switch to OpenGL rendering.

## Build Instructions (Windows)

Clone the repository:
```sh
git clone https://github.com/aardvark-community/hum.git
cd hum
```

Build the project:
```sh
build
```

Deploy executables `humcli.exe` and `humgui.exe` to `./pub` directory:
```sh
publish
```

## Usage

Use `hum` to use the command line interface.

```sh
usage: hum <command> <args...>                                                  
                                                                                
    info <filename>                 prints point cloud file info                
                                                                                
    import <filename> <store> <id>  imports <filename> into <store> with <id>   
        [-mindist <dist>]              skips points on import, which are less   
                                         than given distance from previous point
                                         e.g. -mindist 0.001                    
        [-n <k>]                       estimate per-point normals               
                                         using k-nearest neighbours,            
                                         e.g. -n 16                             
        [-ascii <lineformat>]          imports custom ascii format              
                                         e.g. -ascii "x y z _ r g b"            
                                       format symbols                           
                                         position      : x, y, z                
                                         normal        : nx, ny, nz             
                                         color         : r, g, b, a             
                                         color (float) : rf, gf, bf, af         
                                         intensity     : i                      
                                         skip          : _                      
                                                                                
    view <store> <id>               shows pointcloud with <id> in given <store> 
        [-gl]                            uses OpenGL instead of Vulkan          
        [-vulkan]                        uses Vulkan (default)                  
        [-near <dist>]                   near plane distance, e.g. -near 1.0    
        [-far <dist>]                    far plane distance, e.g. -far 2000.0   
                                    keyboard shortcuts                          
                                         <A>/<D> ... left/right                 
                                         <W>/<S> ... forward/back               
                                         <+>/<-> ... camera speed               
                                         <P>/<Q> ... point size                 
                                         <T>/<R> ... target pixel distance      
                                                                                
    download <baseurl> <targetdir>  bulk download of point cloud files          
                                      webpage at <baseurl> is scanned for hrefs 
                                      to files with known file extensions, which
                                      are then downloaded to <targetdir>        
```

## Licensing

This software is licensed under the [GNU Affero General Public License 3.0](https://www.gnu.org/licenses/agpl-3.0.en.html).
