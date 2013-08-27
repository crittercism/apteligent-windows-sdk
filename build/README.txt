Steps to publish locally for testing
1) Run build.bat.
2) In visual studio, go to "PROJECT" then "Manage NuGet Packages" which will open a dialog.
3) Click "Settings" in the lower left corner which will bring up another dialog.
4) Under "Package Manager/Package Sources" in the tree, add a new source.  Call it "Local" 
   or somesuch and set the Source for it to this build directory (use the "..." button if needed)
5) Hit "Add" and you should a new package source pop into the list.
6) Hit "OK".  Back in the "Manage NuGet Packages" dialog, navigate to "Online/Local" and see
    the Crittercism SDK show up.  Click "install" and hopefully get a green happy checkmark!

Steps 2-6 above can also work with any old directory that has the .nuspec and .nupkg file placed into it - 
no build environment or source checkout required!


Steps to publish to nuget.org for public release

??? FIXME jbley do this!
