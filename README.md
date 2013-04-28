Platform.VirtualFileSystem
==========================

Platform.VirtualFileSystem is a .NET library that provides a uniform, cross-platform and managed abstraction layer for file systems. It is similar to VFS features of various operating systems like Linux except it all runs in managed code. Features include:

 * Unified addressing. All files and directories are addressed using URIs. No need to concern yourself with different path separators and naming schemes on different platforms.
 * Support for layed URIs to support addressing ofnested file systems (zip, overlayed and netvfs). Layed URIs are provided using square brackets. For example the URI to a.txt contained inside a zip archive located at C:\Test.zip is: `zip://[file:/C:/Test.zip]/a.txt`
 * Very simple but powerful API for working with files. Normalisation of relative paths (paths containing "." and "..") is inbuilt and supported by all providers. Support for extended attributes and alternate data streams are supported on NTFS and most Unix architectures.
 * Extensible file and directory services architecture (for example: `IFileHashingService` and `IFileTransferService`).
 * Pluggable provider architecture makes adding new filesystems a breeze.
 * Pluggable provider architecture makes extending existing file systems easy. Filters that can modify or wrap files/directories can be added using `INodeResolutionFilter`. Operations on files can be intercepted and/or overidden using `INodeOperationFilter`.
 * Fully thread-safe.
 * Support for defining providers within `app.config` or `web.config`.


### Local Provider


  Provides access to the concerete file sytems of the underlying operating system. Example URIs:
  
    C:\Windows\Temp
    file:///usr/local/src
    file://c:/Windows/Temp
    
### Web Provider

  Provides read-only access to ftp and http file systems view the .NET HTTP WebRequest object. Example URIs:
  
	http://www.github.com

  
### View Provider

  Create custom file systems rooted from a directory of any other file system. This is very useful for avoiding the use of absolute concrete file system paths. For example, you can map `D:\App\Data` to a view accessible via `appdata://`. All your code will be resilient to any changes to the underlying location of the `appdata://` view. You could move `D:\App\Data` to `E:\App\Data` or even to a non-local provider without breaking any existing code. Views also add security by ensuring code can't access files above the root of the view. Example URIs:
  
	config:///A/B.txt
	mycustomview:///A/B.txt
   
### SystemInfo Provider

 Similar to the `/proc` file system on Linux, the SystemInfo provider provides simple read-only access to environment variables and read-write access to the system clock. Access to more advanced operating system settings are planned for the future.
 
	systeminfo:///EnvironmentVariables/TEMP
	systeminfo:///SystemClock/CurrentDateTime
  
### Overlayed Provider

Create a merged file system made up of the merged view of one or more file systems.

	overlayed://[file:///tmp/a;file:///tmp/b]/c
  
### Temp Provider

A provider accessible via the `temp://` scheme that provides access to temporary storage (usually backed by `/tmp` on Unix and `C:\Windows\Temp` on Windows)

	temp:///tempfile1.dat
	temp:///tempfile2.dat

### MyComputer Provider

Provides a simple and cross-platform consistent view of the disks on a system.

	mycomputer://C
	mycomputer://D
	mycomputer://C/Windows
	mycomputer://CDROM-1/i386
	mycomputer://FLOPPY-1/GAME/LEMMINGS
	
### DataFile Provider

Provides a simple interface for load/saving of objects serialized into files (see `DataFileTests.cs`).
	
### Network VFS Provider

Exposes all VFS file systems over TCP. Many services are supported natively (for example `IFileHashingService` is executed remotely on the server).

	netvfs://hostname[file:///usr//]/local/src
	
### Zip Provider

Provides a read-only virtualised view of zip files. Currently random read-write access to zip files is not supported although creation of zip files as a whole from VFS directories and files is supported via `ZipFileSystem.CreateZipFile`.

	zip://[file:///home/tum/Test.zip]/Directory/Textfile.txt
	
	
## Usage Examples

	// Get a directory
	var localDir = FileSystemManager.Default.ResolveDirectory("C:/Windows");
	
	// Create a view with the scheme 'windows'
	var windowsFileSystem = localDir.CreateView("windows");
	var system32 = windowsFileSystem.ResolveDirectory("System32");
	
	// Will output: windows://System32
	Console.WriteLine(system32.Address);

	// Add the file system to the default FileSystemManager
	FileSystemManager.Default.AddFileSystem(windowsFileSystem);

	var windowsNotepad =  FileSystemManager.Default.ResolveFile("windows://notepad.exe");

	// Will output: True
	Console.WriteLine(windowsNotepad.Exists);
	
	var root = localDir.ResolveFile("..");
	
	// Will output: file://C:/
	Console.WriteLine(root.Address);
	
	var localNotepad = dir.ResolveFile("notepad.exe");
	
	// Will output: True
	Console.WriteLine(localNotepad.Exists);
	
	// Will output: /Windows/notepad.exe
	Console.WriteLine(root.Address.GetRelativePathTo(localNotepad.Address));
	
	// Will output: ../..
	Console.WriteLine(root.Address.GetRelativePathTo(localNotepad.Address));
	
	

Refer to the Platform.VirtualFileSystem.Tests project for more examples. The `app.config` of the Tests project offers examples of how to add custom views and add inject non-default NodeProviders.


---
Copyright © 2003-2013 Thong Nguyen (tumtumtum@gmail.com)