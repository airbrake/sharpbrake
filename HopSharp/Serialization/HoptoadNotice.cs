using System;
using System.Collections.Generic;
using System.Web.SessionState;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace HopSharp.Serialization
{
	[XmlRoot("notice", Namespace = "")]
	public class HoptoadNotice
	{
		public HoptoadNotice()
		{
			this.Version = "2.0";	
		}

		[XmlElement("api-key")]
		public string ApiKey { get; set; }

		[XmlAttribute("version")]
		public string Version { get; set; }

		[XmlElement("error")]
		public HoptoadError Error
		{
			get;
			set;
		}

		[XmlElement("notifier")]
		public HoptoadNotifier Notifier
		{
			get;
			set;
		}

		[XmlElement("server-environment")]
		public HoptoadServerEnvironment ServerEnvironment
		{ 
			get; set;
		}
	}
}