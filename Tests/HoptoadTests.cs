using System;
using HopSharp;
using NUnit.Framework;

namespace Tests
{
	[TestFixture]
	public class HoptoadTests
	{
		[Test]
		public void Can_send_an_exception()
		{
			try
			{
				throw new NotImplementedException("Booo");
			}
			catch (Exception e)
			{
				// TODO would like to mock the HttpWebRequest call... maybe dig up my TypeMock license
				HoptoadClient a = new HoptoadClient();
				a.Send(e);
			}
		}

		[Test]
		public void Can_convert_HoptoadNotice_to_json()
		{
			HoptoadNotice notice = new HoptoadNotice();

			notice.ApiKey = "12345678";	
			notice.ErrorMessage = "sdlfds";
			notice.ErrorClass = "sdflshs";
			notice.Backtrace = "blah1\npoop2";

			string json = notice.Serialize();

			Console.WriteLine(json);
			Assert.AreEqual("{\"notice\":{\"api_key\":\"12345678\",\"error_class\":\"sdflshs\",\"error_message\":\"sdlfds\",\"environment\":{\"Path\":\"C:\\\\Windows\\\\system32;C:\\\\Windows;C:\\\\Windows\\\\System32\\\\Wbem;C:\\\\Program Files\\\\Microsoft SQL Server\\\\80\\\\Tools\\\\Binn\\\\;C:\\\\Program Files\\\\Microsoft SQL Server\\\\90\\\\Tools\\\\binn\\\\;C:\\\\Program Files\\\\Microsoft SQL Server\\\\90\\\\DTS\\\\Binn\\\\;C:\\\\Program Files\\\\Microsoft SQL Server\\\\90\\\\Tools\\\\Binn\\\\VSShell\\\\Common7\\\\IDE\\\\;C:\\\\Program Files\\\\Microsoft Visual Studio 8\\\\Common7\\\\IDE\\\\PrivateAssemblies\\\\;C:\\\\Program Files\\\\TortoiseSVN\\\\bin\",\"TEMP\":\"C:\\\\Users\\\\KENROB~1\\\\AppData\\\\Local\\\\Temp\",\"SESSIONNAME\":\"Console\",\"PATHEXT\":\".COM;.EXE;.BAT;.CMD;.VBS;.VBE;.JS;.JSE;.WSF;.WSH;.MSC\",\"USERDOMAIN\":\"DEV2008\",\"PROCESSOR_ARCHITECTURE\":\"x86\",\"SystemDrive\":\"C:\",\"APPDATA\":\"C:\\\\Users\\\\Ken Robertson\\\\AppData\\\\Roaming\",\"windir\":\"C:\\\\Windows\",\"LOCALAPPDATA\":\"C:\\\\Users\\\\Ken Robertson\\\\AppData\\\\Local\",\"WecVersionForRosebud.C0C\":\"2\",\"TMP\":\"C:\\\\Users\\\\KENROB~1\\\\AppData\\\\Local\\\\Temp\",\"USERPROFILE\":\"C:\\\\Users\\\\Ken Robertson\",\"ProgramFiles\":\"C:\\\\Program Files\",\"FP_NO_HOST_CHECK\":\"NO\",\"HOMEPATH\":\"\\\\Users\\\\Ken Robertson\",\"COMPUTERNAME\":\"DEV2008\",\"USERNAME\":\"krobertson\",\"NUMBER_OF_PROCESSORS\":\"2\",\"PROCESSOR_IDENTIFIER\":\"x86 Family 6 Model 15 Stepping 11, GenuineIntel\",\"__COMPAT_LAYER\":\"ElevateCreateProcess \",\"SystemRoot\":\"C:\\\\Windows\",\"ComSpec\":\"C:\\\\Windows\\\\system32\\\\cmd.exe\",\"LOGONSERVER\":\"\\\\\\\\DEV2008\",\"VisualStudioDir\":\"C:\\\\Users\\\\Ken Robertson\\\\Documents\\\\Visual Studio 2008\",\"CommonProgramFiles\":\"C:\\\\Program Files\\\\Common Files\",\"PROCESSOR_LEVEL\":\"6\",\"PROCESSOR_REVISION\":\"0f0b\",\"lib\":\"C:\\\\Program Files\\\\SQLXML 4.0\\\\bin\\\\\",\"ALLUSERSPROFILE\":\"C:\\\\ProgramData\",\"VS90COMNTOOLS\":\"c:\\\\Program Files\\\\Microsoft Visual Studio 9.0\\\\Common7\\\\Tools\\\\\",\"PUBLIC\":\"C:\\\\Users\\\\Public\",\"ProgramData\":\"C:\\\\ProgramData\",\"OS\":\"Windows_NT\",\"HOMEDRIVE\":\"C:\"},\"session\":{},\"request\":{},\"backtrace\":[\"blah1\",\"poop2\"]}}", json);
		}
	}
}