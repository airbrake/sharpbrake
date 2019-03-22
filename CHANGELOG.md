Sharpbrake Changelog
====================

### master

### [v5.0.5][v5.0.5] (Mar 22, 2019)

* Fixed an issue with Log4Net Assemblies strong name.
  ([#106](https://github.com/airbrake/sharpbrake/pull/106))

### [v5.0.4][v5.0.4] (Mar 21, 2019)

* Fixed an issue with file and method names reported from Log4Net.
  ([#109](https://github.com/airbrake/sharpbrake/pull/109))
* Fixed an issue with security protocols.
  ([#107](https://github.com/airbrake/sharpbrake/pull/107))

### [v5.0.3][v5.0.3] (Mar 05, 2019)

* Added assemblies signing, to get assemblies with strong names
  ([#103](https://github.com/airbrake/sharpbrake/pull/103))

### [v5.0.2][v5.0.2] (Jan 31, 2019)

* Fixed an issue with context.userAddr
  ([#98](https://github.com/airbrake/sharpbrake/pull/98))

### [v5.0.1][v5.0.1] (April 16, 2018)

* Fixed broken proxy support
  ([#88](https://github.com/airbrake/sharpbrake/pull/88))

### [v5.0.0][v5.0.0] (March 07, 2018)

* Added support for error messages
  ([#84](https://github.com/airbrake/sharpbrake/pull/84))
* Updated notifier API. Check the [migration guide](docs/migration-guide-from-v4-to-v5.md)

### [v4.5.0][v4.5.0] (January 12, 2018)

* Added internal logging (tracing)
  ([#80](https://github.com/airbrake/sharpbrake/pull/80))
* Removed dependency on Newtonsoft.Json
  ([#78](https://github.com/airbrake/sharpbrake/pull/78))

### [v4.4.0][v4.4.0] (October 30, 2017)

* Added support for .NET Standard 2.0
  ([#73](https://github.com/airbrake/sharpbrake/pull/73))

### [v4.3.0][v4.3.0] (September 28, 2017)

* Added Microsoft.Extensions.Logging integration
  ([#71](https://github.com/airbrake/sharpbrake/pull/71))

### [v4.2.0][v4.2.0] (September 5, 2017)

* Added log4net integration
  ([#68](https://github.com/airbrake/sharpbrake/pull/68))

### [v4.1.0][v4.1.0] (July 28, 2017)

* Added NLog integration
  ([#65](https://github.com/airbrake/sharpbrake/pull/65))

### [v4.0.1][v4.0.1] (July 5, 2017)

* Fixed version in the notifier info
  ([#61](https://github.com/airbrake/sharpbrake/pull/61))

### [v4.0.0][v4.0.0] (July 3, 2017)

* Switched to the new csproj format. Only .NET Framework 4.5.2+
  is going to be supported onward. Refer to [sharpbrake-net35](https://github.com/airbrake/sharpbrake-net35)
  to get support for the older .NET versions
  ([#59](https://github.com/airbrake/sharpbrake/pull/59))

### [v3.1.1][v3.1.1] (May 22, 2017)

* Fixed ignored severity in the `Notify` method for .NET 3.5
  ([#57](https://github.com/airbrake/sharpbrake/pull/57))

### [v3.1.0][v3.1.0] (May 20, 2017)

* Started sending error severity (defaults to `error`)
  ([#55](https://github.com/airbrake/sharpbrake/pull/55))
* Added support for notice truncation
  ([#54](https://github.com/airbrake/sharpbrake/pull/54))

### [v3.0.3][v3.0.3] (April 21, 2017)

* Fixed missing error location info (action and component fields)
  ([#52](https://github.com/airbrake/sharpbrake/pull/52))

### [v3.0.2][v3.0.2] (April 5, 2017)

* Fixed missing notifier info, environment and app version
  in the notice context ([#47](https://github.com/airbrake/sharpbrake/pull/47))

### [v3.0.1][v3.0.1] (March 15, 2017)

* Maintenance updates

### [v3.0.0][v3.0.0] (March 14, 2017)

* Version 3 is written from scratch. See [here](https://github.com/airbrake/sharpbrake#key-features)
  for new features that have been introduced

[v3.0.0]: https://github.com/airbrake/sharpbrake/releases/tag/v3.0.0
[v3.0.1]: https://github.com/airbrake/sharpbrake/releases/tag/v3.0.1
[v3.0.2]: https://github.com/airbrake/sharpbrake/releases/tag/v3.0.2
[v3.0.3]: https://github.com/airbrake/sharpbrake/releases/tag/v3.0.3
[v3.1.0]: https://github.com/airbrake/sharpbrake/releases/tag/v3.1.0
[v3.1.1]: https://github.com/airbrake/sharpbrake/releases/tag/v3.1.1
[v4.0.0]: https://github.com/airbrake/sharpbrake/releases/tag/v4.0.0
[v4.0.1]: https://github.com/airbrake/sharpbrake/releases/tag/v4.0.1
[v4.1.0]: https://github.com/airbrake/sharpbrake/releases/tag/v4.1.0
[v4.2.0]: https://github.com/airbrake/sharpbrake/releases/tag/v4.2.0
[v4.3.0]: https://github.com/airbrake/sharpbrake/releases/tag/v4.3.0
[v4.4.0]: https://github.com/airbrake/sharpbrake/releases/tag/v4.4.0
[v4.5.0]: https://github.com/airbrake/sharpbrake/releases/tag/v4.5.0
[v5.0.0]: https://github.com/airbrake/sharpbrake/releases/tag/v5.0.0
[v5.0.1]: https://github.com/airbrake/sharpbrake/releases/tag/v5.0.1
[v5.0.2]: https://github.com/airbrake/sharpbrake/releases/tag/v5.0.2
[v5.0.3]: https://github.com/airbrake/sharpbrake/releases/tag/v5.0.3
