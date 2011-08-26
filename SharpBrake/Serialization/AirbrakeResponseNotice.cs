using System.Xml.Serialization;

namespace SharpBrake.Serialization
{
   [XmlRoot("notice")]
   public class AirbrakeResponseNotice
   {
      public int ErrorId { get; set; }
      public string Url { get; set; }
      public int Id { get; set; }
   }
}