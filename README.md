Platform.VirtualFileSystem
==========================

[Read the Platform.VirtualFileSystem wiki](https://github.com/platformdotnet/Platform.VirtualFileSystem/wiki)


A virtual file system for C#, .NET and Mono.

Platform.VirtualFileSystem provides a uniform, cross-platform and managed abstraction layer for file systems. It is similar to VFS features of various operating systems like Linux except it all runs in managed code. Features include:

 * Support for Microsoft .NET and Mono.
 * Unified addressing. All files and directories are addressed using URIs. No need to concern yourself with different path separators and naming schemes on different platforms.
 * Support for layered URIs to support addressing of nested file systems (zip, overlayed and netvfs). Layed URIs are supported using square brackets. For example the URI to a.txt contained inside a zip archive located at C:\Test.zip is: `zip://[file:/C:/Test.zip]/a.txt`
 * Very simple but powerful API for working with files. Normalisation of relative paths (paths containing "." and "..") is inbuilt at a low level and thus is supported by all providers. Support for extended attributes and alternate data streams are supported on Windows (NTFS) and supporting Unix file systems (ext2fs, xfs etc).
 * Extensible file and directory services architecture (for example: `IFileHashingService` and `IFileTransferService`). All services implement `Platform.ITask` meaning they can all run the background; are monitorable (percent complete, bytes transferred etc) and can be paused, or cancelled.
 * Pluggable provider architecture makes adding new filesystems a breeze.
 * Pluggable provider architecture makes extending existing file systems easy. Filters that can modify or wrap files/directories can be added using `INodeResolutionFilter`. Operations on files can be intercepted and/or overidden using `INodeOperationFilter`.
 * Fully thread-safe.
 * Easy to detect file system changes using C# events on `IFile` and `IDirectory` interfaces.
 * Default cache provides strong object identity. Resolving the same path will return the same object instance as long as the file/directory is still referenced elsewhere.
 * Support for defining managers and providers in code or declaratively within `app.config` or `web.config`.
 

## Installation

Platform.VirtualFileSystem is available via [nuget](https://nuget.org/packages/Platform.VirtualFileSystem/). You can search for `Platform.VirtualFileSystem` using the Visual Studio NuGet plugin UI or by typing `Install-Package Platform.VirtualFileSystem` into the Visual Studio Package Manager Console. If you want zip support you should also install the zip provider using `Install-Package Platform.VirtualFileSystem.Providers.Zip`.

A package for those who don't use NuGet will be available in the future. In the mean time you can download the latest zip of the NuGet package direct from the [nuget website](http://packages.nuget.org/api/v1/package/Platform.VirtualFileSystem). Note that you will also need the [Platform.NET package](wget http://packages.nuget.org/api/v1/package/Platform.NET).

Three assemblies are required for Platform.VirtualFileSystem: `Platform.dll`, `Platform.Xml.Serialization` and `Platform.VirtualFileSystem.dll`. If you require zip support then `Platform.VirtualFileSystem.Providers.Zip.dll` and `ICSharpCode.SharpZipLib` is also required. Platform.VirtualFileSystem will automatically load the zip provider if the `Platform.VirtualFileSystem.Providers.Zip.dll` is located in the same directory as `Platform.VirtualFileSystem.dll`.


## Included FileSystem providers

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

Create a file system made up of the merged view of one or more file systems.

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

Provides a simple interface for load/saving of objects serialized into files (see `DataFileTests.cs`). Requires `Platform.VirtualFileSystem.DataFile.dll`.
	
### Network VFS Provider

Exposes all VFS file systems over TCP. Many services are supported natively (for example `IFileHashingService` would be executed remotely on the server). Required `Platform.VirtualFileSystem.Network.*.dll`.

	netvfs://hostname[file:///usr//]/local/src
	
### Zip Provider

Provides a virtualised view of zip files. Both readonly and random readwrite support zip files has been supported since version 1.0.0.45. You can also directly create a zip file from an existing directory using `ZipFileSystem.CreateZipFile`.

	zip://[file:///home/tum/Test.zip]/Directory/Textfile.txt
	
	
## Usage Examples

	// Get a directory
	var localDir = FileSystemManager.Default.ResolveDirectory("C:/Windows");
	
	// Create a view with the scheme 'windows'
	var windowsFileSystem = localDir.CreateView("windows");
	var system32 = windowsFileSystem.ResolveDirectory("System32");
	
	// Will output: windows:///System32
	Console.WriteLine(system32.Address);

	// Add the file system to the default FileSystemManager
	FileSystemManager.Default.AddFileSystem(windowsFileSystem);

	var windowsNotepad =  FileSystemManager.Default.ResolveFile("windows:///notepad.exe");

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
	
	

Refer to the [Platform.VirtualFileSystem.Tests](https://github.com/platformdotnet/Platform.VirtualFileSystem/tree/master/tests/Platform.VirtualFileSystem.Tests) project for more examples. 


## Configuring default FileSystems

The default file system manager (`FileSystemManager.Default`) contains all the standard file system providers except for Network and Zip. If you want to custom define what is available or add views or custom providers, you can extend the IFileSystemManager interface, construct and populate a separate `StandardFileSystemManager` or define file systems within your `app.config` or `web.config`. Please reference [app.config](https://raw.github.com/platformdotnet/Platform.VirtualFileSystem/master/tests/Platform.VirtualFileSystem.Tests/app.config) from the tests project to see an example of how to do the later.

## License

Platform.VirtualFileSystem is released under the terms of the new BSD license as documented [here](https://github.com/platformdotnet/Platform.VirtualFileSystem/blob/master/LICENSE). Zip support is provided by a separate assembly (`Platform.VirtualFileSystem.Providers.Zip`) which depends on `SharpZipLib` which is licensed under the GPL with an exception that permits commercial use.

## Contributing

Platform.VirtualFileSystem was written quite a few years ago and has been recently updated to modern C#. It was written in a time before type inference, automatic properties and lambda expressions so if you see legacy cruft please don't hesistate to update it.

## More

[Read the Platform.VirtualFileSystem wiki](https://github.com/platformdotnet/Platform.VirtualFileSystem/wiki)

---
Copyright © 2003-2013 Thong Nguyen (tumtumtum@gmail.com)