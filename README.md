# B4X Custom Build Action (b4x_cba)

b4x_cba is a C# console application that contains a collection of utility actions that can be executed as custom build actions or ide links in B4X.

# Supported Actions

- compileonly - returns an error to stop b4x launching the release version of your app (always run last)
- copy - copy files or folder
- copyjar - copys the output jar to another directory
- buildtime - creates a file named build.txt with the build time
- updateversion - maintains a version.txt file with incrementing version number
- zip - zips a file or folder
- moveautobackups - moves auto backup files to another directory
- checksum - writes a SHA256 checksum of the jar file to a text file
- githubpush - pushes the project code to a GitHub repo

# Setup;

1. Copy b4x_cba.exe to your B4X install folder eg C:\Program Files\Anywhere Software\B4J
2. Set up some custom build actions in your project

# Usage

In your custom actions section call b4x_cba.exe with an action and other required parameters

`#CustomBuildAction: 2, b4x_cba.exe, -action compileonly`

Alternatively as an ide link

`'Ctrl + click to increment version: ide://run?File=b4x_cba.exe&Args=-action&Args=updateversion`

# Usage and Supported actions;
## compileonly

b4x_cba returns a 1 exit statement to B4X which stops the app launching. This is useful for release build when you just want to compile.

> [!NOTE]
> You should run this action last as it stops any further actions running.

`#CustomBuildAction: 2, b4x_cba.exe, -action compileonly`

## copy

b4x_cba will copy a file or folder from the source to the destination directory. "Files" can be used as a shortcut to the assets folder.

> [!NOTE]
> Remember to sync the files if you are copying to the Files folder (to remove warning #17).

`#CustomBuildAction: 2, b4x_cba.exe, -action copy -source ObfuscatorMap.txt -destination D:\Temp`

or

`#CustomBuildAction: 2, b4x_cba.exe, -action copy -source src -destination D:\Temp`

or

`#CustomBuildAction: folders ready, b4x_cba.exe, -action copy -source D:\Temp\index.html -destination Files`

## copyjar

b4x_cba will copy the output jar to the specified destination directory.

> [!NOTE]
> In the first release the second argument was named "directory" this has been changed to "destination" for consistency.

`#CustomBuildAction: 2, b4x_cba.exe, -action copyjar -destination D:\Temp`

## buildtime

b4x_cba will create a file named "build.txt" in the Files directory of your project with the current date/time. If you omit the date/time formats they will default to yyyy-MM-dd HH:mm:ss.

> [!NOTE]
> The file "build.txt" does not need to exist before running.
> 
> Remember to sync the files after the first run (to remove warning #17).

`#CustomBuildAction: folders ready, b4x_cba.exe, -action buildtime`

or

`#CustomBuildAction: folders ready, b4x_cba.exe, -action buildtime -dateformat dd/MM/yyyy -timeformat HH:mm:ss`

## updateversion

b4x_cba will create a file named "version.txt" in the Files directory of your project with an incrementing version number in format 0.0.0-9.9.9.

> [!NOTE]
> The file "version.txt" does not need to exist before running.
> 
> Remember to sync the files after the first run (to remove warning #17).

`#CustomBuildAction: folders ready, b4x_cba.exe, -action updateversion`

or

`'Ctrl + click to increment version: ide://run?File=b4x_cba.exe&Args=-action&Args=updateversion`

## zip

b4x_cba will zip a file or folder from the source to the destination. "Files" can be used as a shortcut to the assets folder. If the source is a file and destination filename is ommitted then the filename will be used but with a .zip extension.

`#CustomBuildAction: 2, b4x_cba.exe, -action zip -source ObfuscatorMap.txt -destination D:\Temp\`

or

`#CustomBuildAction: 2, b4x_cba.exe, -action zip -source ObfuscatorMap.txt -destination D:\Temp\Cool.zip`

or

`#CustomBuildAction: 2, b4x_cba.exe, -action zip -source src -destination D:\Temp\Backup.zip`

or

`#CustomBuildAction: 2, b4x_cba.exe, -action zip -source src -destination D:\Temp`

or

`#CustomBuildAction: 2, b4x_cba.exe, -action zip -source Files -destination D:\Temp`

## moveautobackups

b4x_cba will move all auto backup files from the projects "AutoBackups" folder to another location.

> [!NOTE]
> The destination is a directory but the tool will create a sub folder with the project name eg if you use D:\Temp the backups will be moved to D:\Temp\ProjectName

`#CustomBuildAction: 2, b4x_cba.exe, -action moveautobackups -destination D:\Temp`

or

`'Ctrl + click to move autobackups: ide://run?File=b4x_cba.exe&Args=-action&Args=moveautobackups&Args=-destination&Args=D:\Temp`

## checksum

b4x_cba will calulate a SHA256 checksum of the output jar file and write it to a text file

> [!NOTE]
> If the destination is a folder then the jar name will be used for the checksum eg example.jar will result in example_checksum.txt

`#CustomBuildAction: 2, b4x_cba.exe, -action checksum -destination D:\Release`

or

`#CustomBuildAction: 2, b4x_cba.exe, -action checksum -destination D:\Temp\checksum.txt`

## githubpush

b4x_cba will push the project to a GitHub repository.

`Ctrl + click to push to GitHub: ide://run?File=b4x_cba.exe&Args=-action&Args=githubpush`

> [!NOTE]
> This action uses GitHub REST so it doesnt require any other software
> It uses an API key for authetication
> You need to add config to the project attributes region of your project eg
> 'github_repository_owner=ope-nz
> 'github_repository_name=deleteme
> 'github_branch=dev
