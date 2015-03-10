ECHO ON
setlocal
rd /s /q tmp
del /q *.nupkg

cd ..
call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\vcvarsall.bat"
ECHO ON

msbuild /p:Configuration=Release /target:Clean
if %errorlevel% neq 0 exit /b %errorlevel%

msbuild /p:Configuration=Release
if %errorlevel% neq 0 exit /b %errorlevel%

cd build

mkdir tmp\
if %errorlevel% neq 0 exit /b %errorlevel%

mkdir tmp\lib
mkdir tmp\lib\netcore451
mkdir tmp\lib\net40
mkdir tmp\lib\wpa81
mkdir tmp\lib\windowsphone8

copy ..\CrittercismSDK\CrittercismSDK.Windows\bin\Release\CrittercismSDK.dll tmp\lib\netcore451
if %errorlevel% neq 0 exit /b %errorlevel%

copy ..\CrittercismSDK\CrittercismSDK.WindowsDesktop\bin\Release\CrittercismSDK.dll tmp\lib\net40
if %errorlevel% neq 0 exit /b %errorlevel%

copy ..\CrittercismSDK\CrittercismSDK.WindowsPhone\bin\Release\CrittercismSDK.dll tmp\lib\wpa81
if %errorlevel% neq 0 exit /b %errorlevel%

copy ..\CrittercismSDK\CrittercismSDK.WindowsPhoneSilverlight\Bin\Release\CrittercismSDK.dll tmp\lib\windowsphone8
if %errorlevel% neq 0 exit /b %errorlevel%

copy CrittercismSDK.nuspec tmp\CrittercismSDK.nuspec
if %errorlevel% neq 0 exit /b %errorlevel%

rem this file will show up in Visual Studio when you install the package.
copy README_PUBLIC.txt tmp\README.txt
if %errorlevel% neq 0 exit /b %errorlevel%

.\NuGet.exe pack tmp\CrittercismSDK.nuspec
if %errorlevel% neq 0 exit /b %errorlevel%






