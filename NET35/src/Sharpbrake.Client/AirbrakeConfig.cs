using System;
using System.Collections.Generic;

namespace Sharpbrake.Client
{
    /// <summary>
    /// Configuration for Airbrake notifier.
    /// </summary>
    public class AirbrakeConfig
    {
        /// <summary>
        /// Name of your environment.
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// Version of the application that uses the notifier.
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// User API key is used to access to the project
        /// data through Airbrake APIs. Each user of a project has their own key.
        /// </summary>
        public string ProjectKey { get; set; }

        /// <summary>
        /// Project API key that is used to submit errors and track deploys. 
        /// This key is what you configure the notifier agent in your app to use.
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// Host name of the endpoint.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Path to logging file.
        /// </summary>
        public string LogFile { get; set; }

        /// <summary>
        /// Represents URI string of proxy provider.
        /// </summary>
        public string ProxyUri { get; set; }

        /// <summary>
        /// The credentials for proxy.
        /// </summary>
        public string ProxyUsername { get; set; }

        /// <summary>
        /// The credentials for proxy.
        /// </summary>
        public string ProxyPassword { get; set; }

        /// <summary>
        /// List of environments that should be ignored.
        /// </summary>
        public IList<string> IgnoreEnvironments { get; set; }

        /// <summary>
        /// List of the parameters which will not be filtered.
        /// If count of parameters is not zero - all not listed parameters will be filtered.
        /// </summary>
        public IList<string> WhitelistKeys { get; set; }

        /// <summary>
        /// List of the parameters which will be filtered out.
        /// </summary>
        public IList<string> BlacklistKeys { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AirbrakeConfig"/> class.
        /// </summary>
        public AirbrakeConfig()
        {
            IgnoreEnvironments = new List<string>();
            WhitelistKeys = new List<string>();
            BlacklistKeys = new List<string>();
        }

        /// <summary>
        /// Loads configuration from dictionary of key-value pairs.
        /// </summary>
        public static AirbrakeConfig Load(IDictionary<string, string> settings)
        {
            var config = new AirbrakeConfig();
            var configValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // strip "Airbrake.", "Airbrake:" from config keys
            var trimChars = new[] { '.', ':' };
            const string prefix = "Airbrake";

            foreach (var key in settings.Keys)
            {
                int pos;
                var propertyName = (pos = key.IndexOf(prefix, StringComparison.OrdinalIgnoreCase)) != -1
                    ? key.Substring(pos + prefix.Length).Trim(trimChars)
                    : key;

                if (configValues.ContainsKey(propertyName))
                    configValues[propertyName] = settings[key];
                else
                    configValues.Add(propertyName, settings[key]);
            }

            // load config values into AirbrakeConfig properties
            foreach (var property in config.GetType().GetProperties())
            {
                var propertyName = property.Name;
                if (!configValues.ContainsKey(propertyName))
                    continue;

                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(config, configValues[propertyName], null);
                }
                else if (property.PropertyType == typeof(IList<string>))
                {
                    property.SetValue(config, Utils.ParseParameter(configValues[propertyName]), null);
                }
            }

            return config;
        }
    }
}
