using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using Sharpbrake.Client;

namespace ConsoleApp.Net35
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

            // NotifyAsync method with response handling
            // Note: "NotifyAsync" without response handling is functionally the same as "Notify" when "LogFile" is empty (so logging is disabled) -
            // they both notify Airbrake asynchronously on exception
            Case2(settings);

            // mix of "Notify" and "NotifyAsync" methods
            Case3(settings);

            // program should not terminate before async completes otherwise all child threads are killed
            Thread.Sleep(5000);
            Console.ReadKey();
        }

        /// <summary>
        /// Uses "default" Notify method:
        /// 1) async call to endpoint;
        /// 2) if "LogFile" is not empty response is logged into file
        ///     2a) if "LogFile" is not specified as absolute path it is put into the same folder as executable
        /// 3) need to block parent thread otherwise program exits and all child threads are killed - this is
        ///    usually a case in console app like this one when program terminates right after error.
        ///    In web app we always have some kind of "main" thread so nothing should be blocked.
        /// </summary>
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
        /// Uses NotifyAsync method that allows to handle explicitly what to do with response.
        /// In our case response are output to console.
        /// </summary>
        static void Case2(IDictionary<string, string> settings)
        {
            var config = AirbrakeConfig.Load(settings);
            var notifier = new AirbrakeNotifier(config);

            notifier.NotifyCompleted += (sender, eventArgs) =>
            {
                if (eventArgs.Error != null)
                    Console.WriteLine(eventArgs.Error.Message);
                else if (eventArgs.Result != null)
                {
                    var response = eventArgs.Result;
                    Console.WriteLine("Status: {0}, Id: {1}, Url: {2}", response.Status, response.Id, response.Url);
                }
            };

            try
            {
                throw new Exception("Case 2. Exception 1. Time: " + DateTime.Now.ToString("hh.mm.ss.ffffff"));
            }
            catch (Exception ex)
            {
                notifier.NotifyAsync(ex);
            }

            try
            {
                throw new Exception("Case 2. Exception 2. Time: " + DateTime.Now.ToString("hh.mm.ss.ffffff"));
            }
            catch (Exception ex)
            {
                notifier.NotifyAsync(ex);
            }
        }

        /// <summary>
        /// Uses mix of Notify and NotifyAsync methods.
        ///  </summary>
        static void Case3(IDictionary<string, string> settings)
        {
            var config = AirbrakeConfig.Load(settings);
            var notifier = new AirbrakeNotifier(config);

            notifier.NotifyCompleted += (sender, eventArgs) =>
            {
                if (eventArgs.Error != null)
                    Console.WriteLine(eventArgs.Error.Message);
                else if (eventArgs.Result != null)
                {
                    var response = eventArgs.Result;
                    Console.WriteLine("Status: {0}, Id: {1}, Url: {2}", response.Status, response.Id, response.Url);
                }
            };

            try
            {
                throw new Exception("Case 3. Exception 1. Time: " + DateTime.Now.ToString("hh.mm.ss.ffffff"));
            }
            catch (Exception ex)
            {
                notifier.NotifyAsync(ex);
            }

            try
            {
                throw new Exception("Case 3. Exception 2. Time: " + DateTime.Now.ToString("hh.mm.ss.ffffff"));
            }
            catch (Exception ex)
            {
                notifier.Notify(ex);
            }
        }
    }
}