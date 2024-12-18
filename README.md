# B4X Custom Build Action (b4x_cba)

This is a C# console application that can be used by B4X for custom build actions.

# Setup;

1. Copy b4x_cba.exe to your B4X install folder eg C:\Program Files\Anywhere Software\B4J
2. Set up some custom build actions in your project

# Usage

In your custom actions section call b4x_cba.exe with an action and other parameters

e.g. `#CustomBuildAction: 2, b4x_cba.exe, -action compileonly`

Alternatively as an ide link

e.g. `'Ctrl + click to increment version: ide://run?File=b4x_cba.exe&Args=-action&Args=updateversion`

# Usage and Supported actions;
## compileonly

b4x_cba returns a 1 exit statement to B4X which stops the app launching. This is useful for release build when you just want to compile.

NOTE: You should run this action last as it stops any further actions running.

`#CustomBuildAction: 2, b4x_cba.exe, -action compileonly`

## copy

b4x_cba will copy a file or folder from the source to the destination directory. "Files" can be used as a shortcut to the assets folder.

NOTE: Remember to sync the files if you are copying to the Files folder (to remove warning #17).

`#CustomBuildAction: 2, b4x_cba.exe, -action copy -source ObfuscatorMap.txt -destination D:\Temp`

or

`#CustomBuildAction: 2, b4x_cba.exe, -action copy -source src -destination D:\Temp`

or

`#CustomBuildAction: folders ready, b4x_cba.exe, -action copy -source D:\Temp\index.html -destination Files`

## copyjar

b4x_cba will copy the output jar to the specified destination directory

`#CustomBuildAction: 2, b4x_cba.exe, -action copyjar -destination D:\Temp`

## buildtime

b4x_cba will create a file named "build.txt" in the Files directory of your project with the current date/time. If you omit the date/time formats they will default to yyyy-MM-dd HH:mm:ss.

NOTE: The file "build.txt" does not need to exist before running.

NOTE: Remember to sync the files after the first run (to remove warning #17).

`#CustomBuildAction: folders ready, b4x_cba.exe, -action buildtime`

or

`#CustomBuildAction: folders ready, b4x_cba.exe, -action buildtime -dateformat dd/MM/yyyy -timeformat HH:mm:ss`

## updateversion

b4x_cba will create a file named "version.txt" in the Files directory of your project with an incrementing version number in format 0.0.0-9.9.9.

NOTE: The file "version.txt" does not need to exist before running.

NOTE: Remember to sync the files after the first run (to remove warning #17).

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

NOTE: The destination is a directory but the tool will create a sub folder with the project name eg if you use D:\Temp the backups will be moved to D:\Temp\ProjectName

`#CustomBuildAction: 2, b4x_cba.exe, -action moveautobackups -destination D:\Temp`

or

`'Ctrl + click to move autobackups: ide://run?File=b4x_cba.exe&Args=-action&Args=moveautobackups&Args=-destination&Args=D:\Temp`
