# DuplicateFinder

Console application that searches for duplicate files.

## Requirements

* [Microsoft .NET](https://en.wikipedia.org/wiki/.NET_Framework) 4+ for [Windows](https://en.wikipedia.org/wiki/Microsoft_Windows) systems **-OR-**
* [Mono framework](https://en.wikipedia.org/wiki/Mono_%28software%29) for [Linux](https://en.wikipedia.org/wiki/Linux) or [MacOS](https://en.wikipedia.org/wiki/Mac_OS)

## How to use

### Syntax

```dos
DuplicateFinder DIRS OPTIONS
```

### Example

Scans a directory recursive:

```dos
DuplicateFinder C:\dir_with_files /r
```

You can define more than one directory:

```dos
DuplicateFinder C:\dir1 D:\dir2 E:\3rd_directory
```

### Options

Option |  Aliases  | Description  | Example  
------------ | ------------- | ------------- | -------------
/?  | /h, /help  | Show help screen.  | `/?`
/b  | /bash  | Create a [bash script](https://en.wikipedia.org/wiki/Bash_%28Unix_shell%29) called `DuplicateFinder_del_duplicates.sh` in the current directory that deletes the found duplicates.  | `/b`
/d  | /del, /delete  | Delete duplicates.  | `/d`
/p  | /php  | Create a [PHP file](https://en.wikipedia.org/wiki/PHP) called `DuplicateFinder_del_duplicates.php` in the current directory that deletes the found duplicates.  | `/p`
/r  | /recursive  | Also scan subdirectories.  | `/r`
/wb  | /batch  | Create a [Windows batch](https://en.wikipedia.org/wiki/Batch_file) file called `DuplicateFinder_del_duplicates.cmd` in the current directory that deletes the found duplicates.  | `/wb`

You can define custom output files when you use `/b`, `/p` or `/wb` option:

```dos
DuplicateFinder C:\dir_to_scan /wb:C:\myBatchFile.cmd
```

