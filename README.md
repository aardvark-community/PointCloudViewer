# hum - a viewer for *hum*ongous point clouds

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
usage: humcli <command> <args...>
    view <store> <id>               shows pointcloud with given <id> in given <store>
        [-gl]                            uses OpenGL instead of Vulkan
        [-near <dist>]                   near plane distance, e.g. -near 1.0
        [-var <dist>]                    far plane distance, e.g. -far 2000.0
    info <filename>                 prints info (e.g. point count, bounding box, ...)
    import <filename> <store> <id>  imports <filename> into <store> with <id>
        [-mindist <dist>]              skips points on import, which are less than
                                         given distance from previous point, e.g. -mindist 0.001
        [-ascii <format>]              e.g. -ascii "x y z _ r g b"
                                         position      : x,y,z
                                         normal        : nx,ny,nz
                                         color         : r,g,b,a
                                         color (float) : rf, gf, bf, af
                                         intensity     : i
                                         skip          : _
```

## Licensing

This software is licensed under the [GNU Affero General Public License 3.0](https://www.gnu.org/licenses/agpl-3.0.en.html).
