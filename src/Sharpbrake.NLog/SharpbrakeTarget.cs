using NLog;
using NLog.Config;
using NLog.Targets;
using Sharpbrake.Client;

namespace Sharpbrake.NLog
{
    [Target("Sharpbrake")]
    public sealed class SharpbrakeTarget : TargetWithLayout
    {
        private readonly AirbrakeNotifier notifier;

        public SharpbrakeTarget()
        {
            var config = new AirbrakeConfig
            {
                ProjectId = ProjectId,
                ProjectKey = ProjectKey
            };

            notifier = new AirbrakeNotifier(config);
        }

        [RequiredParameter]
        public string ProjectId { get; set; }

        [RequiredParameter]
        public string ProjectKey { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            notifier.Notify(logEvent.Exception);
        }
    }
}
