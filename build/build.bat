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
mkdir tmp\lib
mkdir tmp\lib\CrittercismSDK

copy ..\CrittercismSDK\CrittercismSDK.WindowsPhoneSilverlight\Bin\ARM\Release\CrittercismSDK.WindowsPhoneSilverlight.dll tmp\lib\CrittercismSDK
copy ..\CrittercismSDK\CrittercismSDK.Windows\bin\x86\Release\CrittercismSDK.Windows.dll tmp\lib\CrittercismSDK
copy ..\CrittercismSDK\CrittercismSDK.WindowsDesktop\bin\Release\CrittercismSDK.WindowsDesktop.dll tmp\lib\CrittercismSDK
copy ..\CrittercismSDK\CrittercismSDK.WindowsPhone\bin\ARM\Release\CrittercismSDK.WindowsPhone.dll tmp\lib\CrittercismSDK

copy CrittercismSDK.nuspec tmp\CrittercismSDK.nuspec
rem this file will show up in Visual Studio when you install the package.
copy README_PUBLIC.txt tmp\README.txt
.\NuGet.exe pack tmp\CrittercismSDK.nuspec
if %errorlevel% neq 0 exit /b %errorlevel%






