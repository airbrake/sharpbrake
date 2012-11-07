# SharpBrake
SharpBrake is a .NET library for use with the [Airbrake](http://www.airbrakeapp.com/) exception reporting service by [Thoughtbot](http://www.thoughtbot.com/).  Airbrake allows you to easily track and get notification about exceptions that occur on your site.

The SharpBrake library can be used in two ways:

1. You can programmatically report exceptions with the extension method `SendToAirbrake()` in a `try/catch` block.
2. You can configure the `HttpModule` in `web.config`, which will catch any unhandled exceptions on your site and report them to Airbrake.

## Usage
First, you need to get the library down from the internets and onto your local hard drive. The preferred method to accomplish this is to install [NuGet](http://nuget.org/) and then via any of its many interfaces install the [SharpBrake Package](http://nuget.org/packages/SharpBrake).

If you want to build the library yourself, you can [fork or clone](http://help.github.com/fork-a-repo/) it and build it with Visual Studio (2008 or 2010; both are supported). Drop the files *SharpBrake.dll* and *Common.Logging.dll* into your application's bin directory and you're almost good to go.

Once SharpBrake is downloaded, built, installed or otherwise resides in your application's bin directory, you'll need to edit your application's .config file to include your API key for Airbrake as well as an environment name and optionally the Airbrake server Url (default value is set to https://api.airbrake.io/notifier_api/v2/notices)

```xml
<appSettings>
  <add key="Airbrake.ApiKey" value="1234567890abcdefg" />
  <add key="Airbrake.Environment" value="Whatever" />
  <add key="Airbrake.ServerUri" value="Airbrake server url (optional)" />
</appSettings>
```

To programmatically report exceptions, all you need to do is ensure you've included the `SharpBrake` namespace, and then call the `SendToAirbrake()` extension method on the exception.  For example:

```CSharp
using SharpBrake;

try
{
  // some code
}
catch (Exception exception)
{
  // Oh noes!
  exception.SendToAirbrake();
}
```

To use the `HttpModule`, you will just need to add it as an HttpModule within the `system.web` section of your web.config:

```xml
<httpModules>
  <add name="Airbrake" type="SharpBrake.NotifierHttpModule, SharpBrake"/>
</httpModules>
```

If you are using IIS7+ in Integrated Pipeline mode, add the HttpModule to the `system.webServer` section of your web.config:

```xml
<system.webServer>
  <validation validateIntegratedModeConfiguration="false" />
  <modules runAllManagedModulesForAllRequests="true">
    <add name="Airbrake" type="SharpBrake.NotifierHttpModule, SharpBrake"/>
  </modules>
</system.webServer>
```

## TODO

There are some important things to do on the `HttpModule` still.  Most importantly, it will be incredibly verbose on its exception handling.  In .NET, a 404 is considered an exception, so it will catch, report, and subsequently notify you of any time someone tries to access a URL that doesn't exist.

To circumvent this, plan on adding the ability to set a series of patterns that you can use to exclude exceptions based on exception type, part of the message, or something along those lines.