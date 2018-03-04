.NET Core console app with error reporting
==========

## Install .NET Core SDK

At first verify that .NET Core SDK is installed by running
```sh
$ dotnet
```
If you see `dotnet: command not found`, then follow the steps described in https://www.microsoft.com/net/core to install it.

### Ubuntu 16.04

1. Add the dotnet apt-get feed

    ```sh
    $ sudo sh -c 'echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet-release/ xenial main" > /etc/apt/sources.list.d/dotnetdev.list'
    $ sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 417A0893
    $ sudo apt-get update
    ```

2. Install the .NET Core SDK

    ```sh
    $ sudo apt-get install dotnet-dev-1.0.4
    ```

## Create a sample .NET Core console app

1. Initialize a new console app

    ```sh
    $ dotnet new console -o ConsoleApp && cd ConsoleApp
    ```

2. Run your app and verify that it prints "Hello World!"

    ```sh
    $ dotnet restore && dotnet run
    ```

## Add Airbrake notifier and test exception reporting

1. Add the `Sharpbrake.Client` dependency in `ConsoleApp.csproj`

    ```sh
    $ cat ConsoleApp.csproj
    ```
    
    ```xml
    <Project Sdk="Microsoft.NET.Sdk">

      <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>netcoreapp1.1</TargetFramework>
      </PropertyGroup>

      <ItemGroup>
        <PackageReference Include="Sharpbrake.Client" Version="4.0.1" />
      </ItemGroup>

    </Project>
    ```

2. Integrate `Sharpbrake.Client` with your app

    ```sh
    $ cat Program.cs
    ```
    
    ```csharp
    using System;
    using Sharpbrake.Client;

    namespace ConsoleApp
    {
        class Program
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

3. Run the app

    ```sh
    $ dotnet restore && dotnet run
    ```

   The app should print a URL to your exception in the Airbrake dashboard.
