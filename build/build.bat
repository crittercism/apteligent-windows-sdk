ECHO ON
setlocal
rd /s /q tmp
del /q *.nupkg

cd ..\CrittercismWP8SDK
rem FIXME jbley this is the default location, might want to pursue something smarter than this
call "C:\Program Files (x86)\Microsoft Visual Studio 11.0\VC\WPSDK\WP80\vcvarsphoneall.bat"
ECHO ON

msbuild /p:Configuration=Release /target:Clean
if %errorlevel% neq 0 exit /b %errorlevel%

msbuild /p:Configuration=Release
if %errorlevel% neq 0 exit /b %errorlevel%

cd ..\build

mkdir tmp\
mkdir tmp\lib
mkdir tmp\lib\WindowsPhone8

copy ..\CrittercismWP8SDK\Bin\ARM\Release\CrittercismWP8SDK.dll tmp\lib\WindowsPhone8
copy CrittercismWP8SDK.nuspec tmp\CrittercismWP8SDK.nuspec
rem this file will show up in Visual Studio when you install the package.
copy README_PUBLIC.txt tmp\README.txt
.\NuGet.exe pack tmp\CrittercismWP8SDK.nuspec
if %errorlevel% neq 0 exit /b %errorlevel%






