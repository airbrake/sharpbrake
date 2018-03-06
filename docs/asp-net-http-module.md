ASP.NET MVC 5 application with error reporting
==========

## Complete with [Getting Started with ASP.NET MVC 5](https://docs.microsoft.com/en-us/aspnet/mvc/overview/getting-started/introduction/getting-started) tutorial

After completing you should have a sample ASP.NET MVC application. We are going to add exception reporting to that application. You can also download the completed project [here](https://github.com/Rick-Anderson/MvcMovie5).

## Integrate Airbrake ASP.NET HTTP Module

### Enable reporting of all unhandled exceptions

1. Open [MvcMovie5](https://github.com/Rick-Anderson/MvcMovie5) in Visual Studio. Build and run solution to ensure that everything works.

2. Install the `Sharpbrake.Http.Module` package using `Package Manager Console` (View -> Other Windows -> Package Manager Console):

   ```powershell
   PM> Install-Package Sharpbrake.Http.Module
   ```

3. Go to the project's `Web.config` and add your ProjectId and ProjectKey to the `appSettings` section:

   ```xml
   <appSettings>
     <add key="Airbrake.ProjectId" value="113743"/>
     <add key="Airbrake.ProjectKey" value="81bbff95d52f8856c770bb39e827f3f6"/>
   </appSettings>
   ```

4. Add the `AirbrakeHttpModule` module to the `system.webServer` section in `Web.config`:

   ```xml
   <add name="Airbrake" type="Sharpbrake.Http.Module.AirbrakeHttpModule, Sharpbrake.Http.Module"/>
   ```

   After adding Airbrake HTTP Module, section `system.webServer` from the default MvcMovie5 project should look like:

   ```xml
   <system.webServer>
     <modules>
       <remove name="FormsAuthenticationModule"/>
       <add name="Airbrake" type="Sharpbrake.Http.Module.AirbrakeHttpModule, Sharpbrake.Http.Module"/>
     </modules>
   </system.webServer>
   ```

   You are done! Airbrake configuration is finished and your app is ready to report errors.

### Add filter to ignore "HTTP 404 Not Found" exception

Sometimes it is convenient to ignore some exceptions to reduce the noise. One prominent example is "HTTP 404 Not Found", which quite often turns out to be irrelevant. Exceptions can be ignored based on their type with the help of [filter](https://github.com/airbrake/sharpbrake#addfilter). Open up the `Global.asax.cs` file, add `using Sharpbrake.Http.Module;` and `Init()` method with the following code:

```csharp
public override void Init()
{
    base.Init();

    var airbrake = (AirbrakeHttpModule) Modules["Airbrake"];
    // add a filter to ignore "HTTP 404 Not Found" exception
    airbrake.GetNotifier().AddFilter(notice =>
    {
        var exception = notice.Exception as HttpException;
        if (exception != null && exception.GetHttpCode() == 404)
            return null;

        return notice;
    });
}
```

The filter we added checks whether the exception is of HttpException type with HTTP code 404. The return `null` statement prevents the notice to be passed further through the filter pipeline, which means such a notice is not sent to the Airbrake dashboard.

### Manual exception reporting

Sharpbrake allows sending exceptions manually anywhere in your code. Typically, `try-catch` blocks are where you want to notify. Let's demonstrate this with the help of a practical example. Go to `Controllers/AccountController` and add a `try-catch` block in the second Login method (the one that handles POST requests):

```csharp
[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
{
    if (ModelState.IsValid)
    {
        try
        {
            var user = await UserManager.FindAsync(model.UserName, model.Password);
            if (user != null)
            {
                await SignInAsync(user, model.RememberMe);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                ModelState.AddModelError("", "Invalid username or password.");
            }
        }
        catch (Exception ex)
        {
            var airbrake = (AirbrakeHttpModule)HttpContext.ApplicationInstance.Modules["Airbrake"];
            var notifier = airbrake.GetNotifier();
            var notice = notifier.BuildNotice(ex);
            notice.SetHttpContext(notice, new AspNetHttpContext(System.Web.HttpContext.Current));
            notifier.Notify(notice);
        }
    }
    // If we got this far, something failed, redisplay form
    return View(model);
}
```

In a case when the database is not available `UserManager` throws an exception that now is reported to the Airbrake dashboard as well.

### Filter out sensitive information

In previous example credentials that user has entered in Login form are also sent to Airbrake because they are a part of the current HTTP request. To filter out that sensitive information go to `Web.config` and add `password` to the `BlacklistKeys` collection:

```xml
<add key="Airbrake.ProjectId" value="113743"/>
<add key="Airbrake.ProjectKey" value="81bbff95d52f8856c770bb39e827f3f6"/>
<add key="Airbrake.BlacklistKeys" value="\bpassword\b"/>
```

Now `Password` should be filtered out:

```
"Password": "[Filtered]"
```

### Wrapping up

In this example we created an ASP.NET MVC 5 application and integrated it with Airbrake.
The application uses Airbrake ASP.NET HTTP Module to report errors automatically.
It is configured to ignore error 404's and filter out user passwords, so they're not
sent to Airbrake. This basic setup is quite common for many ASP.NET MVC projects.
Additionally, you can use manual exception reporting anywhere in the app where you need
to report handled exceptions. There are a lot of other possible ways to configure the library,
so check our README to learn more.