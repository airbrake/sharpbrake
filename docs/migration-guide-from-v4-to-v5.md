Migration guide from v4 to v5
=============================

We're happy to announce that Sharpbrake version 5 brings support for error messages along with exceptions. This functionality comes with changes to our API and here are steps to migrate to the new version:

1. To take advantage of the new API, use the `BuildNotice` instead of `NotifyAsync` to pass `exception` object. The `SetHttpContext` method of `notifier` can be used to set `HttpContext`. Call `NotifyAsync` with `notice` returned from `BuildNotice`.

   Before:

   ```csharp
   notifier.NotifyAsync(exception, httpContext, severity);
   ```

   After:

   ```csharp
   var notice = notifier.BuildNotice(severity, exception);
   notifier.SetHttpContext(notice, httpContext);
   notifier.NotifyAsync(notice);
   ```

2. The `Notify` method was removed. It was a convenience method around `NotifyAsync` that doesn't bring any real value. The same functionality can be achieved using `NotifyAsync` + continuation:

   ```csharp
   notifier.NotifyAsync(notice).ContinueWith(responseTask =>
   {
       if (responseTask.IsFaulted)
           Console.WriteLine("Response task is faulted");
       else
       {
           // do something with the Airbrake response
           var airbrakeResponse = responseTask.Result;
       }
   });
   ```

3. Custom `ILogger` implementation passed to the `AirbrakeNotifier` constructor is not supported anymore. It's unlikely that someone would implement custom ILogger interface for the logger itself so it was removed to simplify the API. You still can log responses from Airbrake by specifying `LogFile` in the configuration.
