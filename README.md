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

`#CustomBuildAction: 2, b4x_cba.exe, -action compileonly`

b4x_cba returns a 1 exit statement to B4X which stops the app launching. This is useful for release build when you just want to compile.

NOTE: You should run this action last as it stops any further actions running.

## copyjar

`#CustomBuildAction: 2, b4x_cba.exe, -action copyjar -directory D:\Temp`

b4x_cba will copy the output jar to the specified directory

## buildtime

`#CustomBuildAction: folders ready, b4x_cba.exe, -action buildtime`

or

`#CustomBuildAction: folders ready, b4x_cba.exe, -action buildtime -dateformat dd/MM/yyyy -timeformat HH:mm:ss`

b4x_cba will create a file called build.txt in the Files directory of your project with the current date/time. If you omit the date/time formats they will default to yyyy-MM-dd HH:mm:ss.

NOTE: Remember to sync the files after the first run.

## updateversion

`#CustomBuildAction: folders ready, b4x_cba.exe, -action updateversion`

b4x_cba will create a file called version.txt in the Files directory of your project with an incrementing version number in format 0.0.0-9.9.9

NOTE: Remember to sync the files after the first run.
