<div align="center">
  <a href="#" target="_blank" rel="noopener noreferrer">
    <img src="docs/images/FastPackLogo.svg" alt="FastPack Logo" width="100%">
  </a>
  <br/>
  <img src="https://img.shields.io/badge/platform-x64-blue.svg?longCache=true" alt="platform"/>
  <a href="https://github.com/quanossolutions/FastPack/blob/main/LICENSE">
    <img src="https://img.shields.io/badge/License-MIT-yellow.svg" alt="license" />
  </a>
</div>

# General

FastPack is a lightning fast deduplication (de)compressor that is particularly useful for compressing and decompressing build artifacts containing a small to high degree of duplicate files. By default file/directory timestamps as well as meta data are preserved and restored. During decompression the restoration of file/directory timestamps and meta data can be skipped.

This tool was created as an internal tool by our DevOps-Crew to compress the **~27GB** build artifacts of [Quanos](https://www.quanos.com) as fast as possible (**20s**) into a file with reasonable size (**500 MB**), so it can be attached as a build artifact to each build run and later be extracted in a fast time (**40s**).

By default FastPack uses **Deflate** for compression with a compression level of "**Optimal**". The compression algorithm and level can be specified with the pack-action. Currently the only supported compression algorithms are **Deflate**" and "**NoCompression**". **We're looking for contributors to add reasonable compression algorithms**.

# Why should I use FastPack?

* **Deduplication**: Duplicate files are only compressed once
* **Size**: Because of the deduplication, the size is mostly much smaller
* **Speed**: FastPack uses all your cores for packing and unpacking
* **Filtering**: FastPack allows include/exclude filters during pack and unpack
* **Comparison**: FastPack allows comparison of 2 archives without the need to unpack the content
* **Cross-Platform**: Run on Windows, Linux and OSX

# Sponsors

## Quanos Solutions GmbH

Quanos gave us the time to create/maintain/publish this great tool.

[![Quanos Solutions GmbH](docs/images/sponsors/quanos.gif)](http://quanos.com)

# Installation (.NET Global Tool)

```
CMD> dotnet tool install -g FastPack
```

# Installation (Nuget - Library)

```
PM> Install-Package FastPack.Lib
```

# Installation (Download)

Download is available [here](https://github.com/quanossolutions/FastPack/releases).

## Supported platforms

* **Windows** (x64)
* **Linux** (x64)
* **OSX** (x64)

## Versions

For all platforms there are 2 versions:

* **RuntimeIncluded**: This version of the executable is larger than the other one, because the runtime (.NET) is included in the executable
* **RuntimeExcluded**: This version of the executable is smaller than the other one, because the runtime (.NET) is <ins>not</ins> included in the executable. To run this executable you have to install the correct runtime first.

# Usage

For help just call FastPack without any parameters:

```
CMD> FastPack
Usage: FastPack [options]

Options:
  -a|--action
    Specifies the action to execute
    Valid values: about, help, pack, unpack, diff, info, licenses, version
    Default: pack
    Example: -a unpack
    Example: --action unpack

  -q|--quiet
    Enables quiet-mode, only errors are shown

  -lp|--looseparams
    Ignores unknown parameters

  -h|--help|-?|/?
    Shows general or action related help

  @AnyFileName
    Can be used at any position to provide a file with input parameters
    The provided file can have different formats that are detected by their file extension.
    Supported formats: JSON (*.json), XML (*.xml), Text (*.*)
    Default: Text
    Example: @params.json

Actions:
  -a about
    Shows information about FastPack and the contributors.

  -a help
    Shows this help output.

  -a pack
    Used to compress the content of directories.

  -a unpack
    Used to decompress the content of an archive.

  -a diff
    Shows differences between 2 archives.

  -a info
    Shows information of an archive.

  -a licenses
    Shows the licenses of all 3rd party components used in FastPack.

  -a version
    Shows information about the version of FastPack.
```

To show help for a specific action call:

```
CMD> FastPack --action info --help
Usage: FastPack -a info [options]

Options:
  -i|--input
    The fup file for which to show information
    Valid values: a path to a file
    Required: true
    Example: -i "C:\Users\user\file.up"

  -d|--detailed
    Flag for showing detailed information like folders and files.
    Default: false
    Required: false
    Example: -d

  -f|--format
    The output format of the manifest information.
    Valid values: Text, Json, Xml
    Default: Text
    Required: false
    Example: -f Json

  --pretty
    Pretty print of the json or xml output.
    Default: false
    Required: false
    Example: --pretty
```

FastPack uses "**actions**" to call different functionalities in FastPack. You provide the action using the action-parameter (-a|--action). The default action is "**pack**", for wich the action-parameter can be skipped. To call the **about**-action execute:

```
CMD> FastPack --action about
```

## File parameters

As this tool is primary targeted for automated usage, we also considered advanced usages. This advanced usages could result in size limits for the command line because of too many parameters:

* Windows (8191 chars)
* Linux (~2 million chars)
* MacOS (262,144 chars)

If you provide a lot of exclude/include-filters you can easily hit the limits of Windows.
For this case we provide file-parameters (@FilePathHere). You can provide multiple file-parameters at every position. Just put an "@" sign in front of an existing parameter-filename:

```
CMD> FastPack -i C:\temp -o C:\temp.fup @Performance.txt @PackFilters.json @General.xml
```

There are 3 different formats supported for the parameter-files. The formats are detected by the file extension. The fallback is the textfile format. For the above example the files could look like this

### Plain Text (@Performance.txt)

Each parameter and value has to be on its own line

```
--maxmemory
2GB
--parallelism
16
```

### JSON (@PackFilters.json)

```json
{
    "args": [
        "--include-filter",
        "dll",
        "--exclude-filter",
        "dll/**/*.txt"
    ]
}
```

### XML (@General.xml)

```xml
<?xml version="1.0" encoding="UTF-8"?>
<args>
    <arg>--no-progress</arg>
    <arg>-c</arg>
    <arg><![CDATA[
    This
    is
    some
    multiline
    text
    ]]></arg>
</args>
```

## Filter options

For the "pack" and "unpack" action you can provide a filter type (Parameter: -ft). The following filter types are available:

* [Glob](https://en.wikipedia.org/wiki/Glob_(programming)) (Default)
* [Regex](https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference) (.NET Style)
* [StartsWith](https://docs.microsoft.com/en-us/dotnet/api/system.string.startswith?view=net-6.0#system-string-startswith(system-string))

The filter type is used to evaluate the filter expressions passed via the following parameters:

* **-if | --include-filter**: The filter for which files/folders are included during pack/unpack
* **-ef | --exclude-filter**: The filter for which files/folders are excluded during pack/unpack

First the include-filters are applied. Then on the remaining set of files/folders the exclude-filters are applied. The filters are applied to the path of a file/folder relative to the input directory for the pack-action. The filters are applied to the relative path of a file/folder in the archive for the unpack-action.

By **default** the filters are **case-sensitive**. You can change this by passing the following parameter:

* **-fci | --filter-case-insensitive**: Makes the filters case insensitive

## Performance options

For the "**pack**" and "**unpack**" action you can provide parameters for **managing memory** usage as well as **cpu usage**.

### Parameter: -p | --parallelism

This parameter is used for specifying how many of your logical cpu cores are used for compressing and decompressing data. The values can be specified in different formats.

For all the non-absolute values your available logical cpu cores are used as a base for calculation. For the examples below we assume **16 logical cpu cores** as the base for calculation results.

```
CMD> FastPack ... --parallelism -25% # Result: Use 12 Cores
CMD> FastPack ... --parallelism 25%  # Result: Use 4 Cores
CMD> FastPack ... --parallelism 10  # Result: Use 10 Cores
CMD> FastPack ... --parallelism -2  # Result: Use 14 Cores
```

By **default all logical cores are used** if the "parallelism"-parameter is not provided.

### Parameter: -mm | --maxmemory

This parameter is used for specifying how many RAM is used for compressing and decompressing data. The values can be specified in different formats.

For all the non-absolute values your available free RAM is used as a base for calculation. For the examples below we assume **16 GB RAM** of free RAM as the base for calculation results.

```
CMD> FastPack ... --maxmemory -25% # Result: Use up to 12 GB of RAM
CMD> FastPack ... --maxmemory 25%  # Result: Use up to 4 GB of RAM
CMD> FastPack ... --maxmemory -1G  # Result: Use up to 15 GB of RAM
CMD> FastPack ... --maxmemory -2G  # Result: Use up to 2 GB of RAM
CMD> FastPack ... --maxmemory -500M  # Result: Use up to 500 MB of RAM
CMD> FastPack ... --maxmemory -900K  # Result: Use up to 900 KB of RAM
CMD> FastPack ... --maxmemory -900  # Result: Use up to 700 bytes of RAM
```

By **default 80% of the available free RAM is used**. Which would be 12.8GB in our example.
The RAM used is not the real RAM used for the whole application. Its only the size of RAM used for loading files into RAM for (de)compression. **The overall RAM consumption can be much higher**.

If files do not fit into the specified memory limits, they are processed in a streamed manner, which slows the overall process.

## Action: Pack

Tthe action "**pack**" is used for compressing a folders content. The pack-action is the default action if you dont provide the action-parameter (**-a|--action**).

### Compressing the content of a folder with a comment

```
CMD> FastPack -i C:\FolderToCompress -o C:\out\OutputFile.fup --comment "a comment for the archive"
```

### Compressing the content of a folder with excludes using glob as filter type

In this example the "**dll**" folder and everything below as well as the "**docs**" file or folder is excluded during compression.

```
CMD> FastPack -i C:\FolderToCompress -o C:\out\OutputFile.fup -ft glob -ef dll/** -ef docs
```

### Deduplicating the content of a folder without compression

```
CMD> FastPack -i C:\FolderToCompress -o C:\out\OutputFile.fup --compressionalgorithm NoCompression
```

### Do a dry-run of the compression with simple text output

```
CMD> FastPack -i C:\FolderToCompress -o C:\out\OutputFile.fup --dryrun
```

### Do a dry-run of the compression with a pretty and detailed json output

```
CMD> FastPack -i C:\FolderToCompress -o C:\out\OutputFile.fup --dryrun detailed --dryrunformat json --pretty
```

## Action: Unpack

The action "**unpack**" is used to uncompress an archive which was compressed by the **pack** action.

### Unpack the content of an archive

```
CMD> FastPack -a unpack -i C:\InputFile.fup -o C:\TargetFolder
```

### Unpack the content of an archive with excludes using glob as filter type

In this example the "**dll**" folder and everything below as well as the "**docs**" file or folder is excluded from unpack.

```
CMD> FastPack -a unpack -i C:\InputFile.fup -o C:\TargetFolder -ft glob -ef dll/** -ef docs
```

### Do a dry-run of the unpack with simple text output

```
CMD> FastPack -a unpack -i C:\InputFile.fup -o C:\TargetFolder --dryrun
```

### Do a dry-run of the unpack with a pretty and detailed json output

```
CMD> FastPack -a unpack -i C:\InputFile.fup -o C:\TargetFolder --dryrun detailed --dryrunformat json --pretty
```

## Action: diff

The action "**diff**" is used to compare two archives. By default only the structure (removed or added files/folders) is compared. But it can be configured to also compare size, dates and permissions. Its also possible to extract the differences to compare the content of files using a comparison tool.

### Do a simple comparison (Structure only) with text output

```
CMD> FastPack -a diff -1 C:\FirstFile.fup -2 C:\FirstFile.fup
```

### Do a simple comparison (Structure only) with text output and extract the differences

```
CMD> FastPack -a diff -1 C:\FirstFile.fup -2 C:\FirstFile.fup -x C:\DifferencesFolder
```

### Do a comparison including sizes, dates and permission with a pretty json output and extract the differences

```
CMD> FastPack -a diff -1 C:\FirstFile.fup -2 C:\FirstFile.fup -x C:\DifferencesFolder --format json --pretty -s Size -s Data -s Permission
```

## Action: info

The action "**info**" prints information for an archive. By default only some meta data of the archive is printed in plaintext to the console. But a more detailed output in different formats can be chosen, too.

### Simple info about an archive

```
CMD> FastPack -a info -i C:\archive.fup
```

### Detailed info about an archive in a pretty json format

```
CMD> FastPack -a info -i C:\archive.fup --detailed --format json --pretty
```

## Action: licenses

The action "**licenses**" prints information about all the licenses of 3rd party libraries used in FastPack.

```
CMD> FastPack -a licenses
```

## Action: about

The action "**about**" prints information about FastPack and its contributors as well as sponsors. It also allows to print the license of FastPack and 3rd parties.

```
CMD> FastPack -a about
```

## Action: version

The action "**version**" prints the current version of FastPack. A shortcut parameter "**--version**" without "**-a version**" is supported

```
CMD> FastPack --version
CMD> FastPack -a version
```