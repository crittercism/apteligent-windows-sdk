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

If you're rebuilding the package and want to pull this new version into your app, 
simply go back to the package manager, "uninstall" the Crittercism package, and 
re-install it.  This is easier than making a new version # for every build.

Steps to publish to nuget.org for public release

1) Audit the nuspec file - all fields!  Check version #, release notes, description, etc.
2) Audit AssemblyInfo - versions, copyright, etc.
3) Update Platform.cs, check the client string.
4) Check the README_PUBLIC.txt
5) Commit any changes from the above steps.
6) Run build.bat
7) Do last-minute manual testing of the *.nupkg by publishing locally as above.
8) Tag the release in github
9) Upload the *.nupkg to nuget.org
