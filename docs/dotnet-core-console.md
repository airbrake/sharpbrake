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
    $ sudo apt-get install dotnet-dev-1.0.1
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
        <PackageReference Include="Sharpbrake.Client" Version="3.1.1" />
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
                    ProjectId = "127178",
                    ProjectKey = "e0246db6e4e9214b24ad252e3c99a0f6"
                });

                try
                {
                    throw new Exception("Oops!");
                }
                catch (Exception ex)
                {
                    var response = airbrake.NotifyAsync(ex).Result;
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
