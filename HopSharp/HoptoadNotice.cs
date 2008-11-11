using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HopSharp
{
	public class HoptoadNotice
	{
		public HoptoadNotice()
		{
			Environment = new Dictionary<string,string>();
			Session = new Dictionary<string, object>();
			Request = new Dictionary<string, object>();
		}

		[JsonProperty("api_key")]
		public string ApiKey { get; set; }

		[JsonProperty("error_class")]
		public string ErrorClass { get; set; }

		[JsonProperty("error_message")]
		public string ErrorMessage { get; set; }

		[JsonProperty("environment")]
		public IEnumerable Environment { get; set; }

		[JsonProperty("session")]
		public IEnumerable Session { get; set; }

		[JsonProperty("request")]
		public IDictionary<string, object> Request { get; set; }

		[JsonProperty("backtrace")]
		[JsonConverter(typeof(BacktraceConverter))]
		public string Backtrace { get; set; }

		public string Serialize()
		{
			return JavaScriptConvert.SerializeObject(new HoptoadNoticeSub(this));
		}
	}

	internal class HoptoadNoticeSub
	{
		public HoptoadNoticeSub(HoptoadNotice notice) { this.notice = notice; }
		public HoptoadNotice notice { get; set; }
	}

	public class BacktraceConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value)
		{
			writer.WriteStartArray();
			foreach(string line in ((string)value).Split('\n'))
				writer.WriteValue(line);
			writer.WriteEndArray();
		}

		public override object ReadJson(JsonReader reader, Type objectType) { throw new NotImplementedException(); }

		public override bool CanConvert(Type objectType) { return true; }
	}
}