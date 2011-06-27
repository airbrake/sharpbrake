using System;
using System.Xml.Serialization;

namespace HopSharp.Serialization
{
   [XmlRoot("notice")]
   public class HoptoadResponseNotice
   {
      public int ErrorId { get; set; }
      public string Url { get; set; }
      public int Id { get; set; }
   }
}