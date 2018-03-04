using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Sharpbrake.Client;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // use "ConfigurationManager" to get settings from "App.config" file
            var settings = ConfigurationManager.AppSettings.AllKeys
                .Where(key => key.StartsWith("Airbrake", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(key => key, key => ConfigurationManager.AppSettings[key]);

            // NotifyAsync + continuation
            Case1(settings);

            // NotifyAsync + async/await
            Case2(settings);

            Console.ReadKey();
        }

        /// <summary>
        /// Uses NotifyAsync + continuation to handle response explicitly.
        /// </summary>
        static void Case1(IDictionary<string, string> settings)
        {
            var config = AirbrakeConfig.Load(settings);
            var notifier = new AirbrakeNotifier(config);

            try
            {
                throw new Exception("Case 2. Exception 1. Time: " + DateTime.Now.ToString("hh.mm.ss.ffffff"));
            }
            catch (Exception ex)
            {
                var notice = notifier.CreateNotice(ex);
                notifier.NotifyAsync(notice).ContinueWith(task =>
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
        static async void Case2(IDictionary<string, string> settings)
        {
            var config = AirbrakeConfig.Load(settings);
            var notifier = new AirbrakeNotifier(config);

            try
            {
                throw new Exception("Case 3. Exception 1. Time: " + DateTime.Now.ToString("hh.mm.ss.ffffff"));
            }
            catch (Exception ex)
            {
                var notice = notifier.CreateNotice(ex);
                var response = await notifier.NotifyAsync(notice);
                if (response != null)
                    Console.WriteLine("Status: {0}, Id: {1}, Url: {2}", response.Status, response.Id, response.Url);
            }
        }
    }
}
