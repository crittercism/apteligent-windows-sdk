Apteligent (formerly Crittercism) for Windows Phone
=========================

Getting started with Apteligent for Windows takes just three easy steps:

1. **Create the app**. Log in to [Apteligent](https://app.crittercism.com/developers/login) and create a new Windows application. You'll need a Crittercism account to create the app listing; don't worry, it's free to sign up and we don't send spam.

2. **Download the library from NuGet**. The package is called **Crittercism**, and is downloadable using the [standard procedure](http://nuget.org/packages/Crittercism). Note: installing Windows 8 NuGet packages requires a recent version of NuGet; you might have to upgrade. NuGet 2.2.31210 worked for us.

3. **Instrument your app**. After the NuGet installation completes, Crittercism will be available under the CrittercismSDK namespace. Obtain your App Id from the app's settings page (e.g. 50807ba33a47481dd5000002) and add a call to CrittercismSDK.Crittercism.Init() with your app id, to the app's initialization code (you'll probably want to do this in your Application_Launching()).

That's it! The Crittercism.Init() call installed error collection and tracked session start (app loads). The full API reference follows.

API Reference
-------------
All Crittercism calls take place to a global singleton available in CrittercismSDK.Crittercism.

* **Init**: initialize the library. Installs the basic Crittercism functionality, including app load and unhandled exception tracking. For simple apps, placing a single Init() call in the code is all that's required to use Crittercism.
  * Inputs: The application's app id, available from the "Settings" page on the Crittercism website. 
  * Returns: None

* **LeaveBreadcrumb()**: Leave a textual description of what's going on in the app, useful for remote debugging. Examples: "Clicked the red button", "Character leveded up", "Document saved". Crittercism marks the time each breadcrumb was created, and uploads the full list of breadcrumbs (with times) to the server if a crash occurs.  Breadcrumbs are limited to/truncated at 140 characters.

* **LogHandledException(Exception e)**: Record the occurrance of a handled exception. 
  * Inputs: Takes the exception that occurred, will save a detailed stack trace and metadata log of the issue for later analysis.

### Metadata functions
Crittercism "metadata" is a set of arbitrary key/value pairs attached to a user's session. Metadata is useful for tracking demographic information about the user (email address, username) or various session parameters (level of a game, points, etc.) Crittercism automatically sets many pieces of system metadata when a crash occurs, including memory and disk use, operating system version, mobile carrier, and a handful of other data.

To set metadata, use **Crittercism.SetValue(string value, string key)** to set custom data or **Crittercism.SetUsername(string username)** to set the current username.

### Opt-Out
You can optionally expose a capability in your application to allow your users to opt out of providing data to Crittercism.  Use **SetOptOutStatus(boolean optOut)** and **GetOptOutStatus()**.  How and where you expose this facility is up to you.
