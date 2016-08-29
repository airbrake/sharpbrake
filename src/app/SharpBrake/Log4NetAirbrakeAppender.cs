using log4net.Core;
using log4net.Appender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using SharpBrake.Serialization;

namespace SharpBrake {
	class Log4NetAirbrakeAppender: AppenderSkeleton {
		protected override void Append(LoggingEvent loggingEvent) {
			string sMsg = loggingEvent.RenderedMessage;
			Exception ex = loggingEvent.ExceptionObject;

			if (null == ex) {
				Exception exOut = new AirbrakeMsgException(sMsg);
				var stackTrace = new StackTrace(true);
				StackFrame[] frames = stackTrace.GetFrames();
				foreach (StackFrame frame in frames) {
					MethodBase method = frame.GetMethod();
					int lineNumber = frame.GetFileLineNumber();
					if (lineNumber == 0) {
						lineNumber = frame.GetILOffset();
					}
					string file = frame.GetFileName();
				}
				exOut.SendToAirbrake();
			} else {
				var configuration = new AirbrakeConfiguration();
				var builder = new AirbrakeNoticeBuilder(configuration);
				AirbrakeNotice notice = builder.Notice(ex);
				notice.Error.Message = sMsg + "; " + notice.Error.Message;
				var client = new AirbrakeClient();
				client.Send(notice);
			}
		}
	}
}
