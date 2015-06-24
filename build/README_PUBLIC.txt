Thanks for installing Crittercism's Windows SDK!

Now all you need to do is:
1. Sign up for an account with Crittercism:
     https://www.crittercism.com/sign-up/
   Don't worry, it's free to sign up and we don't send spam.
2. Obtain your App Id from the Crittercism portal's app settings page.
   Your App Id will be a 24 or 40 digit character string. 
3. Add the following code to your app's Application_Launching(), OnLaunched(),
   Application_Startup(), or Main() method.
     Crittercism.Init("YOUR APP ID GOES HERE");

At this point, your app is enabled to monitor app loads and crashes.
Additional features require adding more code to your project.

You can find about additional Crittercism features like breadcrumbs
and logging handled exceptions in Crittercism's Windows SDK documentation:
    http://docs.crittercism.com/windows/windows.html
