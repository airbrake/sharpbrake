How to contribute
=================

Pull requests
-------------

<img align="right" src="https://s3.amazonaws.com/airbrake-github-assets/sharpbrake/pull-requests.png"/>

We love your contributions, thanks for taking the time to contribute!

It's really easy to start contributing, just follow these simple steps:

1. [Fork][fork-article] the [repo][sharpbrake]:

 ![Fork][fork]

2. Clone your forked repository and `cd` into the directory:

  ```shell
  git clone git@github.com:airbrake/sharpbrake.git
  cd sharpbrake
  ```

3. Run the test suite to make sure the tests pass (using Windows PowerShell):

  ```
  PS> .\build.ps1
  ```

  Alternatively, run `build.bat` from the Windows console or double-clicking it.
  Code coverage report can be checked in
  `sharpbrake\artifacts\<CURRENT_VERSION>\test-results\report\index.html`

4. [Create a separate branch][branch], commit your work and push it to your
   fork:

  ```
  git checkout -b my-branch
  git commit -am
  git push origin my-branch
  ```

5. Run the test suite again (new tests are always welcome).

6. [Make a pull request][pr]

Submitting issues
-----------------

Our [issue tracker][issues] is a perfect place for filing bug reports or
discussing possible features. If you report a bug, consider using the following
template (copy-paste friendly):

```
* Sharpbrake version: {YOUR VERSION}
* .NET platform version: {YOUR VERSION}
* Framework name & its version: {YOUR DATA}

#### Airbrake config

    # YOUR CONFIG
    #
    # Make sure to delete any sensitive information
    # such as your project id and project key.

#### Description

{We would be thankful if you provided steps to reproduce the issue, expected &
actual results, any code snippets or even test repositories, so we could clone
it and test}
```

<p align="center">
  <img src="https://s3.amazonaws.com/airbrake-github-assets/sharpbrake/build-better-software.png">
  <b>Build Better Software</b>
</p>

[sharpbrake]: https://github.com/airbrake/sharpbrake
[fork-article]: https://help.github.com/articles/fork-a-repo
[fork]: https://s3.amazonaws.com/airbrake-github-assets/sharpbrake/fork.png
[branch]: https://help.github.com/articles/creating-and-deleting-branches-within-your-repository/
[pr]: https://help.github.com/articles/using-pull-requests
[issues]: https://github.com/airbrake/sharpbrake/issues
