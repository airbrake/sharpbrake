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
   /// <summary>
   /// Represents the Hoptoad "error" element.
   /// </summary>
   [XmlInclude(typeof(HoptoadTraceLine))]
   public class HoptoadError
   {
      /// <summary>
      /// Required. Each line element describes one code location or frame in the backtrace when the
      /// error occurred.
      /// </summary>
      /// <value>
      /// Each line element describes one code location or frame in the backtrace when the error occurred.
      /// </value>
      [XmlArray("backtrace")]
      [XmlArrayItem("line")]
      public HoptoadTraceLine[] Backtrace { get; set; }

      /// <summary>
      /// Required. The class name or type of error that occurred.
      /// </summary>
      /// <value>
      /// The class name or type of error that occurred.
      /// </value>
      [XmlElement("class")]
      public string Class { get; set; }

      /// <summary>
      /// Optional. A short message describing the error that occurred.
      /// </summary>
      /// <value>
      /// A short message describing the error that occurred.
      /// </value>
      [XmlElement("message")]
      public string Message { get; set; }

      /// <summary>
      /// Returns a <see cref="System.String"/> that represents this instance.
      /// </summary>
      /// <returns>
      /// A <see cref="System.String"/> that represents this instance.
      /// </returns>
      public override string ToString()
      {
         return String.Format("{0} : {1}", GetType(), Message);
      }
   }
}