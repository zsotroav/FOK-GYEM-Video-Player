# FOK-GYEM Video Player

This repository contains the code for the multi-threaded video player
application used to play [Bad
Apple!!](https://www.youtube.com/watch?v=5NXSMdUH_Cg) and other videos on our
custom controller boards for FOK-GYEM flip-dot displays.

Controller board driver:
[zsotrav/FOK-GYEM](https://github.com/zsotroav/FOK-GYEM) (serial/master branch)

## File format
The application uses pre-spliced, scaled-down, black and white images to display
the frames. Basic preprocessing is needed to have every module's images in a
`path/to/dir/<ID>/<FRAME>.png` name format, where `<ID>` is the display ID and
`<FRAME>` is the frame number. All modules' image folders should be in the same
base directory.

The images should be exactly the size of the modules (7x24, for example) and
contain only black (`#000`) or white (`#FFF`) pixels. Other colors may or may
not display properly, as the processing of those is considered undefined
behaviour and may change at any point in the future.

> Note: This is a rather inefficient way of storing frame data; a compiled or
> binary file format would be more ideal for display performance, but would
> require substantially more preprocessing. This trade-off between efficiency
> and required processing was chosen (for now) due to simplicity.