using System;
using System.Configuration;
using System.Web;

namespace HopSharp
{
   /// <summary>
   /// Configuration class for Hoptoad.
   /// </summary>
    public class HoptoadConfiguration
    {
       /// <summary>
       /// Initializes a new instance of the <see cref="HoptoadConfiguration"/> class.
       /// </summary>
        public HoptoadConfiguration()
        {
            ApiKey = ConfigurationManager.AppSettings["Hoptoad:ApiKey"];
            EnvironmentName = ConfigurationManager.AppSettings["Hoptoad:Environment"];

            ProjectRoot = HttpContext.Current != null
               ? HttpContext.Current.Request.ApplicationPath
               : Environment.CurrentDirectory;
        }


        /// <summary>
        /// Gets or sets the project root. By default set to  <see cref="HttpRequest.ApplicationPath"/>
        /// if <see cref="HttpContext.Current"/> is not null, else <see cref="Environment.CurrentDirectory"/>. 
        /// </summary>
        /// <remarks>
        /// Only set this if you need to override the default project root.
        /// </remarks>
        /// <value>
        /// The project root.
        /// </value>
        public string ProjectRoot { get; set; }
       
        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        /// <value>
        /// The API key.
        /// </value>
        public string ApiKey { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the environment.
        /// </summary>
        /// <value>
        /// The name of the environment.
        /// </value>
        public string EnvironmentName { get; set; }
    }
}