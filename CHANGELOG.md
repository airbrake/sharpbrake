Sharpbrake Changelog
====================

### master

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
