# hum - *a viewer for **hum**ongous point clouds*

```text
usage: hum <command> <args...>
    show <store> <id>               shows pointcloud with given <id> in given <store>
    info <filename>                 prints info (e.g. point count, bounding box, ...)
    import <filename> <store> <id>  imports <filename> into <store> with <id>
        [-mindist <dist>]                skips points on import, which are less than
                                         given distance from previous point, e.g. -mindist 0.001
```

This software is licensed under the [GNU Affero General Public License 3.0](https://www.gnu.org/licenses/agpl-3.0.en.html).

## Build Instructions (Windows)

clone the repository
```sh
git clone https://github.com/aardvark-community/hum.git
cd hum
```

build the project
```sh
build
```

create the executable
```sh
publish
```

which can be found in **pub/hum.exe**