ASP.NET Core MVC application with error reporting
==========

## Create a web app using "ASP.NET Core Web Application" template from Visual Studio 2017

You can follow [Getting started with ASP.NET Core MVC](https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/start-mvc) tutorial to create sample "MvcMovie" application.

## Integrate Airbrake ASP.NET Core Middleware

### Enable reporting of all unhandled exceptions

1. Install the `Sharpbrake.Http.Middleware` package using `Package Manager Console` (View -> Other Windows -> Package Manager Console):

   ```powershell
   PM> Install-Package Sharpbrake.Http.Middleware
   ```

2. Update the `appsettings.json` file with your ProjectId and ProjectKey:

   ```json
   "Airbrake": {
     "ProjectId": "113743",
     "ProjectKey": "81bbff95d52f8856c770bb39e827f3f6"
   }
   ```

3. Go to the `Startup.cs` file and add `using Sharpbrake.Http.Middleware;` to access types from the middleware namespace.

4. Update the `Configure` method from `Startup.cs` to add the Airbrake middleware to the request pipeline:

   ```csharp
   public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
   {
       loggerFactory.AddConsole(Configuration.GetSection("Logging"));
       loggerFactory.AddDebug();

       if (env.IsDevelopment())
       {
           app.UseDeveloperExceptionPage();
           app.UseBrowserLink();
       }
       else
       {
           app.UseExceptionHandler("/Home/Error");
       }

       // adds the Airbrake middleware to the request pipeline
       app.UseAirbrake(Configuration.GetSection("Airbrake"));

       // the rest code...
   }
   ```

   You are done! Airbrake configuration is finished and your app is ready to report errors.

### Manual exception reporting

Sharpbrake allows sending exceptions manually anywhere in your code. Typically, `try-catch` blocks are where you want to notify. Let's show this with an example. Go to `Controllers/HomeController` and add a `try-catch` block with a `throw new Exception();` statement in the Contact method (don't forget also to add `using Sharpbrake.Http.Middleware;` to the `HomeController` class):

```csharp
public IActionResult Contact()
{
    ViewData["Message"] = "Your contact page.";

    try
    {
        throw new Exception();
    }
    catch (Exception ex)
    {
        var airbrakeFeature = HttpContext.Features.Get<IAirbrakeFeature>();
        if (airbrakeFeature != null)
        {
            var notifier = airbrakeFeature.GetNotifier();
            var notice = notifier.BuildNotice(ex);
            notice.SetHttpContext(notice, new AspNetCoreHttpContext(HttpContext));
            notifier.Notify(notice);
        }
    }

    return View();
}
```

Here, the `IAirbrakeFeature` feature from the `Features` collection is used to access the notifier that was initialized by the middleware.

### Filter out sensitive information

Let's demonstrate how you can filter out some sensitive information using "MS-ASPNETCORE-TOKEN" header as an example. To accomplish that go to `appsettings.json` and add `MS-ASPNETCORE-TOKEN` to the `BlacklistKeys` collection:

```json
"Airbrake": {
  "ProjectId": "113743",
  "ProjectKey": "81bbff95d52f8856c770bb39e827f3f6",
  "BlacklistKeys": "MS-ASPNETCORE-TOKEN"
}
```

Now the value of `MS-ASPNETCORE-TOKEN` is not sent to the Airbrake dashboard:

```
"MS-ASPNETCORE-TOKEN": "[Filtered]"
```

### Wrapping up

In this example we created an ASP.NET Core MVC application and integrated it with Airbrake.
The application uses Airbrake ASP.NET Core Middleware to report all unhandled errors automatically.
Additionally, you can use manual exception reporting anywhere in the app where you need
to report handled exceptions. There are a lot of other possible ways to configure the library,
so check our README to learn more.
