using Redbox.HAL.Component.Model;
using System;
using System.Timers;

namespace Redbox.HAL.Core
{
    public sealed class RedboxTimer
    {
        private readonly Timer Timer;

        public void ScheduleAtNext(int hour, int minute)
        {
            this.ScheduleAtNextInner(new TimeSpan(hour, minute, 0));
        }

        public void ScheduleAtNext(TimeSpan span) => this.ScheduleAtNextInner(span);

        public void FireOn(DateTime time)
        {
            DateTime now = DateTime.Now;
            this.StartTimer((time - now).TotalMilliseconds);
            LogHelper.Instance.Log("Timer should fire on {0} at {1}", (object)time.ToShortDateString(), (object)time.ToShortTimeString());
        }

        public void Disable()
        {
            this.Timer.Enabled = false;
            this.NextFireTime = new DateTime?();
        }

        public RedboxTimer(string name, ElapsedEventHandler handler)
        {
            this.Name = string.IsNullOrEmpty(name) ? "Not Set" : name;
            this.Timer = new Timer();
            this.Timer.Elapsed += handler;
        }

        public bool Started => this.Timer.Enabled;

        public string Name { get; private set; }

        public DateTime? NextFireTime { get; private set; }

        private void ScheduleAtNextInner(TimeSpan span)
        {
            DateTime now = DateTime.Now;
            if (now.TimeOfDay < span)
            {
                double totalMilliseconds = span.Subtract(now.TimeOfDay).TotalMilliseconds;
                this.NextFireTime = new DateTime?(new DateTime(now.Year, now.Month, now.Day, span.Hours, span.Minutes, 0));
                LogHelper.Instance.Log("Timer {0} should fire {1} {2} ( {3}ms from now ).", (object)this.Name, (object)this.NextFireTime.Value.ToShortDateString(), (object)this.NextFireTime.Value.ToShortTimeString(), (object)totalMilliseconds);
                this.StartTimer(totalMilliseconds);
            }
            else
            {
                DateTime dateTime = new DateTime(now.Year, now.Month, now.Day, span.Hours, span.Minutes, 0).AddDays(1.0);
                this.NextFireTime = new DateTime?(dateTime);
                LogHelper.Instance.Log("Timer {0} should fire on {1} {2}", (object)this.Name, (object)dateTime.ToShortDateString(), (object)dateTime.ToShortTimeString());
                this.StartTimer(dateTime.Subtract(now).TotalMilliseconds);
            }
        }

        private void StartTimer(double delta)
        {
            if (this.Timer.Enabled)
                return;
            this.Timer.Interval = delta;
            this.Timer.Start();
        }
    }
}
