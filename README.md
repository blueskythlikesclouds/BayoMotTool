# BayoMotTool

This is a tool that converts Bayonetta 1 motions to Bayonetta 2 & 3, and vice versa. You can get the latest version from the [Releases](https://github.com/blueskythlikesclouds/BayoMotTool/releases) page.

## Usage

```
BayoMotTool [options] [input motion file] [output motion file] [json bone config]
```

Output file does not need to be specified. In this case, the input motion file will be replaced with the converted file.

The tool can take both little endian (PC/Switch) and big endian (Xbox 360/PS3/Wii U) files as input. Saving as big endian requires the option `-b` or `--big-endian` to be present in command line arguments.

The conversion works both ways. If the input motion file is from Bayonetta 2 & 3, the tool converts it to a Bayonetta 1 motion, using the provided .json file for bone remapping and retargeting. If it is from Bayonetta 1, then it converts it to a Bayonetta 2 & 3 motion, applying the provided .json file in the same but reversed way.


### Examples

#### Converting from Bayonetta 3 (Switch) to Bayonetta 1 (PC)

```
BayoMotTool pl0000_0000.mot pl0010_000.mot Bayonetta3.json
```

#### Converting from Bayonetta 1 (PC) to Bayonetta 2 (Wii U)

```
BayoMotTool -b pl0010_000.mot pl000f_2601.mot Bayonetta2.json
```

If you don't want to deal with the command line, you can drag and drop your motion file onto the respective .bat file for quick conversion.

### Remarks

The tool has only been tested on humanoid skeletons. Enemies, demon masquerades etc. might not work as expected with the provided bone config .json files. An empty bone config .json that does not do any retargeting whatsoever might function correctly if bone ID remapping can be prevented when porting over models to Bayonetta 1. This is going to be tackled in the future.