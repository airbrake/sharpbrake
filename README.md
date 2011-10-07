# SharpBrake
SharpBrake is a .NET library for use with the [Airbrake](http://www.airbrakeapp.com/) exception reporting service by [Thoughtbot](http://www.thoughtbot.com/).  Airbrake allows you to easily track and get notification about exceptions that occur on your site.

The SharpBrake library can be used in two ways:

1. You can programmatically report exceptions with the extension method `SendToAirbrake()` in a `try/catch` block.
2. You can configure the `HttpModule` in `web.config`, which will catch any unhandled exceptions on your site and report them to Airbrake.

## Usage
To use the library, you'll need to build the project and drop the files *SharpBrake.dll* and *Common.Logging.dll* in your application's bin directory.  To configure the library, you'll need to edit your application's .config file to include your API key for Airbrake:

	<appSettings>
		<add key="Airbrake.ApiKey" value="1234567890abcdefg"/>
	</appSettings>

To programmatically report exceptions, all you need to do is ensure you've included the `SharpBrake` namespace, and then call the `SendToAirbrake()` extension method on the exception.  For example:

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

To use the `HttpModule`, you will just need to add it as an HttpHandler within your web.config:

	<httpModules>
		<add name="Airbrake" type="SharpBrake.NotifierHttpModule, SharpBrake"/>
	</httpModules>

## TODO
There are some important things to do on the `HttpModule` still.  Most importantly, it will be incredibly verbose on its exception handling.  In .NET, a 404 is considered an exception, so it will catch, report, and subsequently notify you of any time someone tries to access a URL that doesn't exist.

To circumvent this, plan on adding the ability to set a series of patterns that you can use to exclude exceptions based on exception type, part of the message, or something along those lines.