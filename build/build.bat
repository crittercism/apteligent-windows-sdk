setlocal
cd ..\CrittercismWP8SDK
rem FIXME jbley this is the default location, might want to pursue something smarter than this
call "C:\Program Files (x86)\Mirosoft Visual Studio 11.0\VC\WPSDK\WP80\vcvarsphoneall.bat"

msbuild /p:Configuration=Release /target:Clean
msbuild /p:Configuration=Release

cd ..\build

rd /s /q lib
mkdir lib
mkdir lib\WindowsPhone8

copy ..\CrittercismWP8SDK\CrittercismWP8SDK\Bin\Release\CrittercismWP8SDK.dll lib\WindowsPhone8
.\NuGet.exe pack CrittercismWP8SDK.nuspec







