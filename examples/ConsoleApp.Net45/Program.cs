using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Sharpbrake.Client;

namespace ConsoleApp.Net45
{
    class Program
    {
        static void Main()
        {
            // use "ConfigurationManager" to get settings from "App.config" file
            var settings = ConfigurationManager.AppSettings.AllKeys
                .Where(key => key.StartsWith("Airbrake", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(key => key, key => ConfigurationManager.AppSettings[key]);

            // "default" Notify method
            Case1(settings);

            // NotifyAsync + continuation
            Case2(settings);

            // NotifyAsync + async/await
            Case3(settings);

            Console.ReadKey();
        }

        static void Case1(IDictionary<string, string> settings)
        {
            var config = AirbrakeConfig.Load(settings);
            var notifier = new AirbrakeNotifier(config);

            try
            {
                throw new Exception("Case 1. Exception 1. Time: " + DateTime.Now.ToString("hh.mm.ss.ffffff"));
            }
            catch (Exception ex)
            {
                notifier.Notify(ex);
            }

            try
            {
                throw new Exception("Case 1. Exception 2. Time: " + DateTime.Now.ToString("hh.mm.ss.ffffff"));
            }
            catch (Exception ex)
            {
                notifier.Notify(ex);
            }
        }

        /// <summary>
        /// Uses NotifyAsync + continuation to handle response explicitly.
        /// </summary>
        static void Case2(IDictionary<string, string> settings)
        {
            var config = AirbrakeConfig.Load(settings);
            var notifier = new AirbrakeNotifier(config);

            try
            {
                throw new Exception("Case 2. Exception 1. Time: " + DateTime.Now.ToString("hh.mm.ss.ffffff"));
            }
            catch (Exception ex)
            {
                notifier.NotifyAsync(ex).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                        Console.WriteLine(task.Exception == null ? "Faulted without exception" : task.Exception.Message);
                    else
                    {
                        var response = task.Result;
                        Console.WriteLine("Status: {0}, Id: {1}, Url: {2}", response.Status, response.Id, response.Url);
                    }
                });
            }
        }

        /// <summary>
        /// Uses NotifyAsync + async/await keyword.
        /// </summary>
        static async void Case3(IDictionary<string, string> settings)
        {
            var config = AirbrakeConfig.Load(settings);
            var notifier = new AirbrakeNotifier(config);

            try
            {
                throw new Exception("Case 3. Exception 1. Time: " + DateTime.Now.ToString("hh.mm.ss.ffffff"));
            }
            catch (Exception ex)
            {
                var response = await notifier.NotifyAsync(ex);
                if (response != null)
                    Console.WriteLine("Status: {0}, Id: {1}, Url: {2}", response.Status, response.Id, response.Url);
            }
        }
    }
}