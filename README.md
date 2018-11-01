# hum - a viewer for *hum*ongous point clouds

**hum** let's you view and interactively explore point clouds with billions of points.

Supported file formats: `.las, .laz, .e57, .ply, .pts, .yxh, ascii`

## Quickstart

Import a point cloud file with
```
hum import laserscan.laz path/to/my/store scan1
```
which will create a hum store at `path/to/my/store` and import `laserscan.laz` using key `scan1`.
If no store exists at the given directory, then it will be created automatically.

Now you can view the point cloud with
```
hum view path/to/my/store scan1
```

**ProTip:** A store can hold multiple point clouds. Just use different keys.

**Advise** If your graphics card does not support Vulkan, use `-gl` to switch to OpenGL rendering.

## Build Instructions (Windows)

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

    info <filename>                 prints point cloud file info

    import <filename> <store> <id>  imports <filename> into <store> with <id>
        [-mindist <dist>]              skips points on import, which are less
                                         than given distance from previous point,
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

    view <store> <id>               shows point cloud with <id> in given <store>
        [-gl]                            uses OpenGL instead of Vulkan
        [-vulkan]                        uses Vulkan (default)
        [-near <dist>]                   near plane distance, default 1.0
        [-far <dist>]                    far plane distance, default 5000.0
        [-fov <degrees>]                 horizontal field-of-view, default 60.0
                                    keyboard shortcuts
                                         <A>/<D> ... left/right
                                         <W>/<S> ... forward/back
                                         <+>/<-> ... camera speed
                                         <P>/<O> ... point size (+/-)
                                         <T>/<R> ... target pixel distance (+/-)

    download <baseurl> <targetdir>  bulk download of point cloud files
                                      webpage at <baseurl> is scanned for hrefs
                                      to files with known file extensions, which
                                      are then downloaded to <targetdir>

    gui                             starts in GUI mode                            
```

## Licensing

This software is licensed under the [GNU Affero General Public License 3.0](https://www.gnu.org/licenses/agpl-3.0.en.html).
