# hum - *a viewer for **hum**ongous point clouds*

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

## Command Line Interface

Use `humcli` (or simply `hum`) to use the command line interface.

```sh
usage: humcli <command> <args...>
    show <store> <id>               shows pointcloud with given <id> in given <store>
    info <filename>                 prints info (e.g. point count, bounding box, ...)
    import <filename> <store> <id>  imports <filename> into <store> with <id>
        [-mindist <dist>]                skips points on import, which are less than
                                         given distance from previous point, e.g. -mindist 0.001
```

## GUI

Use `humgui` to start the graphical user interface.

## Licensing

This software is licensed under the [GNU Affero General Public License 3.0](https://www.gnu.org/licenses/agpl-3.0.en.html).
