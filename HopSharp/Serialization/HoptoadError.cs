#region License

// --------------------------------------------------
// Copyright © 2003-2011 OKB. All Rights Reserved.
// 
// This software is proprietary information of OKB.
// USE IS SUBJECT TO LICENSE TERMS.
// --------------------------------------------------

#endregion

using System;
using System.Xml.Serialization;

namespace HopSharp.Serialization
{
   [XmlInclude(typeof(TraceLine))]
   public class HoptoadError
   {
      [XmlArray("backtrace")]
      [XmlArrayItem("line")]
      public TraceLine[] Backtrace { get; set; }

      [XmlElement("class")]
      public string Class { get; set; }

      [XmlElement("message")]
      public string Message { get; set; }


      public override string ToString()
      {
         return String.Format("{0} : {1}", GetType(), Message);
      }
   }
}