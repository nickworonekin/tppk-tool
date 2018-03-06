# TppkTool

A command line app for creating and extracting TPPK archives in Puyo Puyo Tetris PC.

## Usage
### Create
```
tppktool create <output> <files> [options]
```

#### Arguments

**output**

The name of the TPPK archive to create.

**files**

The DDS files to add. This argument supports wildcards, folders, and a combination of both. As an example, all of the following are accepted by this argument:

* **00_ba32703e.dds** (Adds this file to the archive)
* ***.dds** (Adds all files with the dds file extension in the current folder to the archive)
* **arle** (Adds all DDS files in the arle folder to the archive)
* **arle/*.dds** (Adds all files with the dds file extension in the arle folder to the archive)

#### Options

**-?** or **-h** or **--help**

Shows help information for the create command.

### Extract
```
tppktool extract <input> [options]
```

#### Arguments

**input**

The name of the TPPK archive to extract.

#### Options

**-o** or **--output**

The name of the folder to extract the TPPK archive to. If omitted, the files will be extracted to the current folder.

**-?** or **-h** or **--help**

Shows help information for the extract command.

## Batch files

This app comes with two batch files, create.cmd and extract.cmd, to easily create and extract TPPK archives.

### create.cmd
To use this batch file, drag the DDS files and/or folders containing the DDS files you want to add to the TPPK archive. The batch file will will ask you to name the TPPK archive when creating it.

### extract.cmd
To use this batch file, drag the TPPK archive you want to extract. The batch file will ask you for the name of the folder to extract the files to. If left blank, the files will be extracted to the current folder.

## License
TppkTool is licensed under the [MIT license](LICENSE.md).