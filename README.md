# B4X Custom Build Action (b4x_cba)

This is a C# console application that can be used by B4X for custom build actions.

# Setup;

1. Copy b4x_cba.exe to your B4X install folder eg C:\Apps\B4J
2. Set up some custom build actions in your project

# Usage

In your custom actions section call b4x_cba.exe with an action and other parameters

e.g. `#CustomBuildAction: 2, D:\Dropbox\My b4x_cba.exe, -action compileonly`

# Usage and Supported actions;
## compileonly

b4x_cba returns a 1 exit statement to B4X which stops the app launching. This is useful for release build when you just want to compile.

NOTE: You should run this action last as it stops any further actions running.

`#CustomBuildAction: 2, b4x_cba.exe, -action compileonly`

## copy

b4x_cba will copy a file or folder from the source to the destination directory. "Files" can be used as a shortcut to the assets folder.

`#CustomBuildAction: 2, b4x_cba.exe, -action copy --source ObfuscatorMap.txt -destination D:\Temp`

or

`#CustomBuildAction: 2, b4x_cba.exe, -action copy --source src -destination D:\Temp`

or

`#CustomBuildAction: folders ready, b4x_cba.exe, -action copy -source D:\Temp\index.html -destination Files`

## copyjar

b4x_cba will copy the output jar to the specified directory

`#CustomBuildAction: 2, b4x_cba.exe, -action copyjar -directory D:\Temp`

## buildtime

b4x_cba will create a file called build.txt in the Files directory of your project with the current date/time. If you omit the date/time formats they will default to yyyy-MM-dd HH:mm:ss.

NOTE: Remember to sync the files after the first run.

`#CustomBuildAction: folders ready, b4x_cba.exe, -action buildtime`

or

`#CustomBuildAction: folders ready, b4x_cba.exe, -action buildtime -dateformat dd/MM/yyyy -timeformat HH:mm:ss`

## updateversion

b4x_cba will create a file called version.txt in the Files directory of your project with an incrementing version number in format 0.0.0-9.9.9

NOTE: Remember to sync the files after the first run.

`#CustomBuildAction: folders ready, b4x_cba.exe, -action updateversion`

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

