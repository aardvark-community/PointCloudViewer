A viewer for **hum**ongous point clouds.

```shell
usage: hum <command> <args...>
    show <store> <id>               shows pointcloud with given <id> in given <store>
    info <filename>                 prints info (e.g. point count, bounding box, ...)
    import <filename> <store> <id>  imports <filename> into <store> with <id>
        [-mindist <dist>]                skips points on import, which are less than
                                         given distance from previous point, e.g. -mindist 0.001
```

This software is licensed under [AGPL-3.0](https://www.gnu.org/licenses/agpl-3.0.en.html).