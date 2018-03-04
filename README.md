Sharpbrake
==========

[![Build status](https://ci.appveyor.com/api/projects/status/e4o7ffmhq6y4rhei/branch/master?svg=true)](https://ci.appveyor.com/project/airbrake/sharpbrake/branch/master)
[![Coverage](https://codecov.io/gh/airbrake/sharpbrake/branch/master/graph/badge.svg)](https://codecov.io/gh/airbrake/sharpbrake)

![The Sharpbrake notifier for C#/.NET](https://s3.amazonaws.com/airbrake-github-assets/sharpbrake/arthur-sharpbrake.jpeg)

* [Example apps](https://github.com/airbrake/sharpbrake/tree/master/examples)

Introduction
------------

[Airbrake](https://airbrake.io) is an online tool that provides robust error
tracking in most of your C#/.NET applications. In doing so, it allows you to
easily review errors, tie an error to an individual piece of code, and trace the
cause back to recent changes. The Airbrake dashboard provides easy
categorization, searching, and prioritization of errors so that when they
occur, your team can quickly determine the root cause.

Sharpbrake is a C# notifier library for Airbrake. It provides minimalist API to
send C# exceptions and error messages to the Airbrake dashboard. The library
perfectly suits any type of C# applications.

Key features
------------

* Uses the new Airbrake JSON
  (v3)<sup>[[link](https://airbrake.io/docs/#create-notice-v3)]</sup>
* SSL support (all communication with Airbrake is encrypted by default)
* Support for .NET 4.5.2 and above (including the latest .NET Core platforms)
  <sup>[[net35](#net-35-support)]</sup>
* Asynchronous error reporting<sup>[[link](#notify)]</sup>
* Logging support<sup>[[link](#logfile)]</sup>
* Flexible configuration options (configure as many Airbrake notifiers in one
  application as you want)<sup>[[link](#configuration)]</sup>
* Support for environments<sup>[[link](#environment)]</sup>
* Support for proxying<sup>[[link](#proxy)]</sup>
* Filters support (filter out sensitive or unwanted data that shouldn't be sent
  to our servers)<sup>[[link](#blacklistkeys)]</sup>
* Ability to ignore errors from specified environments<sup>[[link](#ignoreenvironments)]</sup>
* Severity support<sup>[[link](#setting-severity)]</sup>

The library comes with the following integrations:

* Web frameworks
  * ASP.NET HTTP Module<sup>[[link](#aspnet-http-module)]</sup>
  * ASP.NET Core Middleware<sup>[[link](#aspnet-core-middleware)]</sup>
* [NLog](http://nlog-project.org/) logging platform<sup>[[link](#nlog-integration)]</sup>
* [Apache log4net](http://logging.apache.org/log4net/) library<sup>[[link](#log4net-integration)]</sup>
* Provider for [Microsoft.Extensions.Logging](https://www.nuget.org/packages/Microsoft.Extensions.Logging) framework<sup>[[link](#microsoftextensionslogging-integration)]</sup>

Installation
------------

### NuGet

Package                       | Description                                            | NuGet link
------------------------------|--------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------
Sharpbrake.Client             | C# client with support for .NET 4.5.2 and above        | [![NuGet](https://img.shields.io/nuget/v/Sharpbrake.Client.svg)](https://www.nuget.org/packages/Sharpbrake.Client)
Sharpbrake.Http.Module        | HTTP module for ASP.NET request pipeline               | [![NuGet](https://img.shields.io/nuget/v/Sharpbrake.Http.Module.svg)](https://www.nuget.org/packages/Sharpbrake.Http.Module)
Sharpbrake.Http.Middleware    | Middleware component for new ASP.NET Core pipeline     | [![NuGet](https://img.shields.io/nuget/v/Sharpbrake.Http.Middleware.svg)](https://www.nuget.org/packages/Sharpbrake.Http.Middleware)
Sharpbrake.NLog               | Airbrake NLog target                                   | [![NuGet](https://img.shields.io/nuget/v/Sharpbrake.NLog.svg)](https://www.nuget.org/packages/Sharpbrake.NLog)
Sharpbrake.NLog.Web           | Airbrake NLog target for ASP.NET                       | [![NuGet](https://img.shields.io/nuget/v/Sharpbrake.NLog.Web.svg)](https://www.nuget.org/packages/Sharpbrake.NLog.Web)
Sharpbrake.Log4net            | Airbrake log4net appender                              | [![NuGet](https://img.shields.io/nuget/v/Sharpbrake.Log4net.svg)](https://www.nuget.org/packages/Sharpbrake.Log4net)
Sharpbrake.Log4net.Web        | Airbrake log4net appender for ASP.NET                  | [![NuGet](https://img.shields.io/nuget/v/Sharpbrake.Log4net.Web.svg)](https://www.nuget.org/packages/Sharpbrake.Log4net.Web)
Sharpbrake.Extensions.Logging | Airbrake provider for Microsoft.Extensions.Logging     | [![NuGet](https://img.shields.io/nuget/v/Sharpbrake.Extensions.Logging.svg)](https://www.nuget.org/packages/Sharpbrake.Extensions.Logging)

Examples
--------

### Basic example

This is the minimal example that you can use to test Sharpbrake with your
project.

```csharp
using System;
using Sharpbrake.Client;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var airbrake = new AirbrakeNotifier(new AirbrakeConfig
            {
                ProjectId = "113743",
                ProjectKey = "81bbff95d52f8856c770bb39e827f3f6"
            });

            try
            {
                throw new Exception("Oops!");
            }
            catch (Exception ex)
            {
                var notice = airbrake.CreateNotice(ex);
                var response = airbrake.NotifyAsync(notice).Result;
                Console.WriteLine("Status: {0}, Id: {1}, Url: {2}", response.Status, response.Id, response.Url);
            }
        }
    }
}
```

### .NET Core console app guide

Consult our [documentation][dotnet-core-console] to learn how to create an
Airbrake-enabled console app.

Configuration
-------------

Before using the library and its notifiers, you must configure them. In most
cases, it is sufficient to configure only one, default, notifier.

```csharp
var airbrake = new AirbrakeNotifier(new AirbrakeConfig
   {
       ProjectId = "113743",
       ProjectKey = "81bbff95d52f8856c770bb39e827f3f6"
   });
```

#### ProjectId & ProjectKey

You **must** set both `ProjectId` & `ProjectKey`.

To find your `ProjectId` and `ProjectKey` navigate to your project's _General
Settings_ and copy the values from the right sidebar.

![](https://s3.amazonaws.com/airbrake-github-assets/sharpbrake/project-id-key.png)

There are multiple ways to set these values:

* Setting explicitly:

  ```csharp
  var config = new AirbrakeConfig {
      ProjectId = "113743",
      ProjectKey = "81bbff95d52f8856c770bb39e827f3f6"
  };
  ```

* Using `App.config` or `Web.config`:

  ```xml
  <appSettings>
      <add key="Airbrake.ProjectId" value="113743" />
      <add key="Airbrake.ProjectKey" value="81bbff95d52f8856c770bb39e827f3f6" />
  </appSettings>
  ```

  ```csharp
  var settings = ConfigurationManager.AppSettings.AllKeys
      .Where(key => key.StartsWith("Airbrake", StringComparison.OrdinalIgnoreCase))
      .ToDictionary(key => key, key => ConfigurationManager.AppSettings[key]);

  var airbrakeConfiguration = AirbrakeConfig.Load(settings);
  ```

* Using `airbrake.json`. Use comma-separated values to add more than one
  argument for options that support it:

  ```json
  {
    "Airbrake": {
      "ProjectId": "113743",
      "ProjectKey": "81bbff95d52f8856c770bb39e827f3f6"
    }
  }
  ```

  ```csharp
  var path = "airbrake.json";
  var configurationBuilder = new ConfigurationBuilder()
      .SetBasePath(System.IO.Directory.GetCurrentDirectory())
      .AddJsonFile(path)
      .Build();

  var settings = configurationBuilder.AsEnumerable()
      .Where(setting => setting.Key.StartsWith("Airbrake"))
      .ToDictionary(setting => setting.Key, setting => setting.Value);

  var airbrakeConfiguration = AirbrakeConfig.Load(settings);
  ```

#### LogFile

The library can log responses from Airbrake via the `LogFile` option. The option
accepts a path to the log file. Supports relative (to your app's executable) and
absolute paths. By default, it's not set.

```csharp
var config = new AirbrakeConfig {
    LogFile = "airbrake.log"
};
```

#### Environment

Configures the environment the application is running in. Helps the Airbrake
dashboard to distinguish between errors occurring in different environments.
By default, it's not set.

```csharp
var config = new AirbrakeConfig {
    Environment = "production"
};
```

#### AppVersion

The version of your application that you can pass to differentiate errors
between multiple versions. It's not set by default.

```csharp
var config = new AirbrakeConfig {
    AppVersion = "1.0.1"
};
```

#### Host

By default, it is set to `airbrake.io`. A `host` is a web address containing a
scheme ("http" or "https"), a host and a port. You can omit the port (80 will be
assumed) and the scheme ("https" will be assumed).

```csharp
var config = new AirbrakeConfig {
    Host = "http://127.0.0.1:8000"
};
```

#### Proxy options

If your server is not able to directly reach Airbrake, you can use a built-in
proxy. By default, Sharpbrake uses a direct connection.

##### ProxyUri, ProxyUsername, ProxyPassword

```csharp
var config = new AirbrakeConfig {
    ProxyUri = "http://46.166.165.63:8080",
    ProxyUsername = "username",
    ProxyPassword = "s3kr3t"
};
```

#### IgnoreEnvironments

Setting this option allows Airbrake to filter errors occurring in unwanted
environments such as `test`. By default, it is not set, which means Sharpbrake
sends errors occurring in all environments.

```csharp
var config = new AirbrakeConfig {
    IgnoreEnvironments = new List<string> { "development" }
};
```

#### BlacklistKeys

Specifies which keys in the payload (parameters, session data, environment data,
etc.) should be filtered. Before sending an error, filtered keys will be
substituted with the `[Filtered]` label.

```csharp
var config = new AirbrakeConfig {
    BlacklistKeys = new List<string> { "password", "creditCard", "email" }
};

// The dashboard will display this parameter as filtered, but other values won't
// be affected:
//   { user: 'John',
//     password: '[Filtered]',
//     email: '[Filtered]',
//     creditCard: '[Filtered]' }
```

**Note:** `BlacklistKeys` has higher priority than `WhitelistKeys`. It means
that if you set the same value into both blacklist and whitelist - that value
will be filtered out.

#### WhitelistKeys

Specifies which keys in the payload (parameters, session data, environment data,
etc.) should _not_ be filtered. All other keys will be substituted with the
`[Filtered]` label.

```csharp
var config = new AirbrakeConfig {
    WhitelistKeys = new List<string> { "user", "email", "accountId" }
};

// The dashboard will display this parameter as is, but all other values will be
// filtered:
//   { user: 'John',
//     password: '[Filtered]',
//     email: 'john@example.com',
//     accountId: 42 }
```

#### FormatProvider

Specifies formatting information for error messages. Check [IFormatProvider][iformatprovider]
for details.

API
---

### AirbrakeConfig

#### Load

`AirbrakeConfig.Load` accepts a `Dictionary` consisting of config option names
and their values.

```csharp
// Construct a dictionary with configuration options
var settings = ConfigurationManager.AppSettings.AllKeys
    .Where(key => key.StartsWith("Airbrake", StringComparison.OrdinalIgnoreCase))
    .ToDictionary(key => key, key => ConfigurationManager.AppSettings[key]);

// Create a config from that dictionary
var config = AirbrakeConfig.Load(settings);
```

### AirbrakeNotifier

#### CreateNotice

`CreateNotice` creates a notice with error details that can be sent to the
Airbrake dashboard with the help of [`NotifyAsync`](#notifyasync):

```csharp
notifier.CreateNotice(Severity.Error, exception, "Failed with message {0}", exception.Message);
```

#### NotifyAsync

`NotifyAsync` asynchronously sends a notice with error details to the Airbrake
dashboard. It also provides control over the response object from Airbrake.
In .NET 4.5 and above you can use task-based programming model. Example of using
a continuation, which prints URL to the error in the Airbrake dashboard:

```csharp
airbrake.NotifyAsync(notice)
        .ContinueWith(response => Console.WriteLine(response.Result.Url));
```

The method also supports `async/await`:

```csharp
var response = await airbrake.NotifyAsync(notice);
Console.WriteLine(response.Url);
```

#### SetHttpContext

`SetHttpContext` attaches HTTP context properties to the notice:

```csharp
var notice = notifier.CreateNotice(ex);
notifier.SetHttpContext(notice, new AspNetCoreHttpContext(context));
notifier.NotifyAsync(notice);
```

Usually, our integrations perform this for you.

#### `AddFilter`

A notice can be customized or ignored before it is sent to Airbrake via
`AddFilter`. A lambda expression that is passed to the `AddFilter` method
accepts `Notice` that can be processed by your code. The `Notice` object is
pre-populated with errors, context and params, so you can freely modify these
values if you wish. The `Notice` object is not sent to Airbrake if the lambda
expression returns `null`:

```csharp
airbrake.AddFilter(notice =>
{
    // ignore notice if email is "test@example.com"
    if (notice.Context.User?.Email == "test@example.com")
        return null;

    // clear environment variables with "token"-related keys
    if (notice.EnvironmentVars != null)
    {
        new List<string>(notice.EnvironmentVars.Keys).ForEach(key =>
        {
            if (key.ToLowerInvariant().Contains("token"))
                notice.EnvironmentVars[key] = string.Empty;
        });
    }

    return notice;
});
```

### Notice

`Exception` and `HttpContext` are properties that can be used to retrieve the
values that the `Notice` object was built from.

#### Exception

Used to access additional exception properties. For example, if your exception is
an `HttpException`, you can ignore it if `GetHTTPCode()` returns 404:

```csharp
airbrake.AddFilter(notice =>
{
    var exception = notice.Exception as HttpException;
    if (exception != null && exception.GetHttpCode() == 404)
        return null;

    return notice;
});
```

#### HttpContext

Used to retrieve HTTP context properties:

```csharp
airbrake.AddFilter(notice =>
{
    notice.Params["response"] = notice.HttpContext.Response

    return notice;
});
```

**Note:** Notice that exceeds 64 KB is truncated before sending.

#### Setting severity

[Severity][what-is-severity] allows categorizing how severe an error is. By
default, it's set to `error`. To redefine severity, simply pass the `Severity`
as a parameter to the `CreateNotice` method. For example:

```csharp
airbrake.CreateNotice(Severity.Critical, exception);
```

ASP.NET Integration
-------------------

### ASP.NET HTTP Module

1. Install the `Sharpbrake.Http.Module` package from NuGet (you can use "Package
   Manager Console" from Visual Studio):

   ```
   PM> Install-Package Sharpbrake.Http.Module
   ```

2. Configure `appSettings` in `Web.config` ([how to configure](#configuration)):

   ```xml
   <appSettings>
       <add key="Airbrake.ProjectId" value="113743"/>
       <add key="Airbrake.ProjectKey" value="81bbff95d52f8856c770bb39e827f3f6"/>
   </appSettings>
   ```

3. Add the `AirbrakeHttpModule` module to your `system.webServer` in
   `Web.config`:

   ```xml
   <system.webServer>
       <modules>
           <add name="Airbrake" type="Sharpbrake.Http.Module.AirbrakeHttpModule, Sharpbrake.Http.Module"/>
       </modules>
   </system.webServer>
   ```

### ASP.NET Core Middleware

1. Install the `Sharpbrake.Http.Middleware` package from NuGet (you can use
   "Package Manager Console" from Visual Studio):

   ```
   PM> Install-Package Sharpbrake.Http.Middleware
   ```

2. Configure `appsettings.json`:

   ```json
   "Airbrake": {
     "ProjectId": "113743",
     "ProjectKey": "81bbff95d52f8856c770bb39e827f3f6",
   }
   ```

3. Add the middleware to your `Startup.cs`:

   ```csharp
   using Sharpbrake.Http.Middleware;
   ```

   Then make sure to add `AirbrakeMiddleware` to your pipeline by updating the
   `Configure` method:

   ```csharp
   app.UseAirbrake(Configuration.GetSection("Airbrake"));
   ```

   **Note:** In most cases you want to put `AirbrakeMiddleware` as a topmost
   middleware component to load it as early as possible. However, if you use
   `app.UseDeveloperExceptionPage` and/or `app.UseExceptionHandler`, then
   `AirbrakeMiddleware` must be put after these components.

   You can use the `IAirbrakeFeature` feature from the `Features` collection of
   `HttpContext` to access the notifier that was initialized by the middleware:

   ```csharp
   var airbrake = HttpContext.Features.Get<IAirbrakeFeature>().GetNotifier();
   ```

NLog Integration
----------------

### Airbrake NLog target

Airbrake NLog target is used to pass an error and exception from log message
to the Airbrake dashboard.

1. Install the `Sharpbrake.NLog` package from NuGet (you can use "Package
   Manager Console" from Visual Studio):

   ```
   PM> Install-Package Sharpbrake.NLog
   ```

2. Register the `Sharpbrake.NLog` assembly in the `<extensions />` section
   of the [nlog.config][nlog-config] file:

   ```xml
   <extensions>
     <add assembly="Sharpbrake.NLog"/>
   </extensions>
   ```

3. Define the Airbrake target in the `<targets />` section and appropriate
   routing rules in the `<rules />` section of `nlog.config`:

   ```xml
   <targets>
     <target name="airbrake"
             type="Airbrake"
             projectId="113743"
             projectKey="81bbff95d52f8856c770bb39e827f3f6"
             environment="live"
             ignoreEnvironments="dev"
     />
     <!-- other targets -->
   </targets>
   ```

   ```xml
   <rules>
     <logger name="*" minlevel="Error" writeTo="airbrake" />
     <!-- other rules -->
   </rules>
   ```

   Note that both `projectId` and `projectKey` are required parameters. You can
   set any configuration option supported by the Airbrake client in the declarative
   way ([how to configure](#configuration)).

   When you need to access `Notifier` programmatically (e.g. for setting up filters
   in your code) you can get it from the `AirbrakeTarget` object:

   ```csharp
   var airbrakeTarget = (AirbrakeTarget)LogManager.Configuration.AllTargets
       .FirstOrDefault(t => t.GetType() == typeof(AirbrakeTarget));
   
   airbrakeTarget?.Notifier.AddFilter(notice =>
   {
       // clear environment variables with "token"-related keys
       new List<string>(notice.EnvironmentVars.Keys).ForEach(key =>
       {
           if (key.ToLowerInvariant().Contains("token"))
               notice.EnvironmentVars[key] = "[removed]";
       });
   
       return notice;
   });
   ```

### Airbrake NLog target for ASP.NET

With Airbrake NLog target for ASP.NET you get, in addition, reporting of HTTP
context properties in web applications.

1. Install the `Sharpbrake.NLog.Web` package from NuGet (you can use "Package
   Manager Console" from Visual Studio):

   ```
   PM> Install-Package Sharpbrake.NLog.Web
   ```

2. Register the `Sharpbrake.NLog.Web` assembly in the `<extensions />` section
   of the [nlog.config][nlog-config] file:

   ```xml
   <extensions>
     <add assembly="Sharpbrake.NLog.Web"/>
   </extensions>
   ```

3. Define the Airbrake target in the `<targets />` section and appropriate
   routing rules in the `<rules />` section of `nlog.config`. All supported
   settings are the same as for the regular Airbrake target.

   ```xml
   <targets>
     <target name="airbrake"
             type="Airbrake"
             projectId="113743"
             projectKey="81bbff95d52f8856c770bb39e827f3f6"
             environment="live"
             ignoreEnvironments="dev"
     />
     <!-- other targets -->
   </targets>
   ```

   ```xml
   <rules>
     <logger name="*" minlevel="Error" writeTo="airbrake" />
     <!-- other rules -->
   </rules>
   ```

4. For ASP.NET Core you need to call the following code in the `Configure` method
   in `Startup.cs`:

   ```csharp
   app.ConfigureAirbrakeTarget();
   ```

   The `Notifier` object can be accessed from here for the additional configuration:

   ```csharp
   public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
   {
       // add NLog to ASP.NET Core
       loggerFactory.AddNLog();
       // add NLog.Web
       app.AddNLogWeb();
       // configure the Airbrake target with HTTP context accessor
       app.ConfigureAirbrakeTarget();
       // example of getting access to the Notifier instance
       var target = LogManager.Configuration.FindTargetByName<AirbrakeTarget>("airbrake");
       target?.Notifier.AddFilter(notice =>
       {
           // clear environment variables with "token"-related keys
           new List<string>(notice.EnvironmentVars.Keys).ForEach(key =>
           {
               if (key.ToLowerInvariant().Contains("token"))
                   notice.EnvironmentVars[key] = "[removed]";
           });
           return notice;
       });
       // remaining code...
   }
   ```

Log4net Integration
-------------------

### Airbrake log4net appender

Airbrake log4net appender sends an exception or/and error from the logging
event to the Airbrake dashboard.

1. Install the `Sharpbrake.Log4net` package from NuGet (you can use "Package
   Manager Console" from Visual Studio):

   ```
   PM> Install-Package Sharpbrake.Log4net
   ```

2. Define the `Sharpbrake.Log4net.AirbrakeAppender` appender in the
   [log4net configuration][log4net-config] file:

   ```xml
    <appender name="Airbrake" type="Sharpbrake.Log4net.AirbrakeAppender, Sharpbrake.Log4net">
      <projectId value="113743" />
      <projectKey value="81bbff95d52f8856c770bb39e827f3f6" />
    </appender>
   ```

3. Add Airbrake appender to the root logger:

   ```xml
    <root>
      <level value="DEBUG" />
      <appender-ref ref="Airbrake" />
      <!-- other appenders... -->
    </root>
   ```

   Note that both `projectId` and `projectKey` are required parameters. You can
   set any configuration option supported by the Airbrake client in the declarative
   way within `<appender />` section ([how to configure](#configuration)).

### Airbrake log4net appender for ASP.NET

With Airbrake log4net appender for ASP.NET you get, in addition, reporting of HTTP
context properties in web applications.

1. Install the `Sharpbrake.Log4net.Web` package from NuGet (you can use "Package
   Manager Console" from Visual Studio):

   ```
   PM> Install-Package Sharpbrake.Log4net.Web
   ```

2. Define the `Sharpbrake.Log4net.Web.AirbrakeAppender` appender in the
   [log4net configuration][log4net-config] file:

   ```xml
    <appender name="Airbrake" type="Sharpbrake.Log4net.Web.AirbrakeAppender, Sharpbrake.Log4net.Web">
      <projectId value="113743" />
      <projectKey value="81bbff95d52f8856c770bb39e827f3f6" />
    </appender>
   ```

   Note that you need to use `Sharpbrake.Log4net.Web.AirbrakeAppender` from `Sharpbrake.Log4net.Web`
   assembly and not plain `Sharpbrake.Log4net.AirbrakeAppender` (without `Web` part) to get support
   for reporting HTTP context properties.

3. Add Airbrake appender to the root logger:

   ```xml
    <root>
      <level value="DEBUG" />
      <appender-ref ref="Airbrake" />
      <!-- other appenders... -->
    </root>
   ```

4. For ASP.NET Core apps you need to set `ContextAccessor` property in appender so it can
   access `HttpContext`. You can do that directly or by calling the helper method from the
   `AspNetCoreExtensions` class:

   4.1. In `Startup.cs` add `using Sharpbrake.Log4net.Web` to get access to the
        `AspNetCoreExtensions` class.

   4.2. In the `Configure` method (`Startup.cs` file) call the `ConfigureAirbrakeAppender`
        method after setting up log4net functionality:

    ```csharp
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        var repoAssembly = Assembly.GetEntryAssembly();
        var loggerRepository = log4net.LogManager.CreateRepository(repoAssembly,
            typeof(log4net.Repository.Hierarchy.Hierarchy));
        log4net.Config.XmlConfigurator.Configure(loggerRepository,
            new FileInfo("log4net.config"));

        app.ConfigureAirbrakeAppender(repoAssembly);
        // remaining code...
    }
    ```

Microsoft.Extensions.Logging Integration
----------------------------------------

Provider notifies the Airbrake dashboard of an error with the help of
[Microsoft.Extensions.Logging](https://github.com/aspnet/Logging) methods.

1. Install the `Sharpbrake.Extensions.Logging` package from NuGet (you can use "Package
   Manager Console" from Visual Studio):

   ```
   PM> Install-Package Sharpbrake.Extensions.Logging
   ```

2. Configure the Airbrake logging provider:

   2.1. In your `Startup.cs` add the import:

   ```csharp
   using Sharpbrake.Extensions.Logging
   ```

   2.2. Add the Airbrake provider to the list of loggers:

   * For ASP.NET Core 2.x the provider can be added in the `ConfigureServices` method:

   ```csharp
   public void ConfigureServices(IServiceCollection services)
   {
       services.AddMvc();

       // adds the Airbrake provider with HTTP Context accessor
       // to the list of services
       var contextAccessor = new HttpContextAccessor();
       services.AddSingleton<IHttpContextAccessor>(contextAccessor);
       services.AddLogging(logging =>
       {
           logging.AddAirbrake(Configuration.GetSection("Airbrake"), contextAccessor);
       });
   }
   ```

   * If your project uses ASP.NET Core 1.x the provider can be added in the `Configure` method:

   ```csharp
   loggerFactory.AddAirbrake(Configuration.GetSection("Airbrake"),
       app.ApplicationServices.GetService<IHttpContextAccessor>());
   ```

   The code above assumes that the configuration options are defined in the registered configuration
   file (e.g. `appsettings.json`). Example of such configuration file:

   ```
   {
     "Logging": {
       "IncludeScopes": false,
       "LogLevel": {
         "Default": "Warning"
       }
     },
     "Airbrake": {
       "ProjectId": "113743",
       "ProjectKey": "81bbff95d52f8856c770bb39e827f3f6"
     }
   }
   ```

   To get access to the HTTP content `app.ApplicationServices.GetService<IHttpContextAccessor>()`
   is used.

   Minimum log level can be set here to filter errors with lower severities
   (defaults to `LogLevel.Error`):

   ```csharp
   loggerFactory.AddAirbrake(Configuration.GetSection("Airbrake"),
       app.ApplicationServices.GetService<IHttpContextAccessor>(),
       LogLevel.Warning);
   ```

   More advanced setup can be used to better suit your needs. Here is an example of
   how to define a notifier with additional filtering rules:

   ```csharp
   var settings = Configuration.GetSection("Airbrake")
       .GetChildren()
       .ToDictionary(setting => setting.Key, setting => setting.Value);

   var airbrakeNotifier = new AirbrakeNotifier(AirbrakeConfig.Load(settings));

   // clear environment variables with "token"-related keys
   airbrakeNotifier.AddFilter(notice =>
   {
       if (notice?.EnvironmentVars != null)
       {
         new List<string>(notice.EnvironmentVars.Keys).ForEach(key =>
         {
           if (key.ToLowerInvariant().Contains("token"))
             notice.EnvironmentVars[key] = "[removed]";
         });
       }
       return notice;
   });

   loggerFactory.AddAirbrake(airbrakeNotifier,
       app.ApplicationServices.GetService<IHttpContextAccessor>());
   ```

Debugging and Diagnostics
-------------------------

If something goes wrong with the notifier itself (a notice is not being sent,
some internal exception happens, etc.) you may want to turn on the tracing mode
and get more insights on what is going on.

Tracing to the Debug window can be enabled with the following code:

```csharp
Sharpbrake.Client.InternalLogger.Enable(msg => Debug.WriteLine(msg));
```

You can output to the file as well:

```csharp
var traceWriter = System.IO.TextWriter.Synchronized(
    System.IO.File.AppendText("AirbrakeNotifier.log"));

Sharpbrake.Client.InternalLogger.Enable(msg =>
{
    traceWriter.WriteLine(msg);
    traceWriter.Flush();
});
```

Tracing can be disabled (this is the default state) with the code:

```csharp
Sharpbrake.Client.InternalLogger.Disable();
```

.NET 3.5 Support
----------------

Please refer to [Sharpbrake for .NET 3.5][sharpbrake-net35] if your app uses
.NET Framework before 4.5.2.

Contributing
------------

* [Contribution guide](CONTRIBUTING.md)
* [Sharpbrake developer documentation](https://github.com/airbrake/sharpbrake/blob/master/docs/developer-howto.md)

License
-------

The project uses the MIT License. See [LICENSE.md](LICENSE.md) for details.

[dotnet-core-console]: https://github.com/airbrake/sharpbrake/blob/master/docs/dotnet-core-console.md
[what-is-severity]: https://airbrake.io/docs/airbrake-faq/what-is-severity/
[sharpbrake-net35]: https://github.com/airbrake/sharpbrake-net35
[nlog-config]: https://github.com/NLog/NLog/wiki/Configuration-file
[log4net-config]: https://logging.apache.org/log4net/release/manual/configuration.html
[iformatprovider]: https://docs.microsoft.com/en-us/dotnet/api/system.iformatprovider