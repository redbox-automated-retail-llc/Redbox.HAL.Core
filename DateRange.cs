using Redbox.HAL.Component.Model;
using System;

namespace Redbox.HAL.Core
{
    public class DateRange : IRange<DateTime>
    {
        private string m_formatted;

        public static DateRange FromHourSpan(int start, int end)
        {
            DateTime now = DateTime.Now;
            DateTime end1 = DateTime.Parse(end.ToString() + ":00");
            DateTime start1 = DateTime.Parse(start.ToString() + ":00");
            if (now.Hour <= start)
            {
                if (end < start)
                    end1 = end1.AddDays(1.0);
                return new DateRange(start1, end1);
            }
            if (end < start)
                end1 = end1.AddDays(1.0);
            if (now >= end1)
            {
                start1 = start1.AddDays(1.0);
                end1 = end1.AddDays(1.0);
            }
            return new DateRange(start1, end1);
        }

        public static bool NowIsBetweenHours(int start, int end)
        {
            return new DateRange(start, end).Includes(DateTime.Now);
        }

        public static DateRange FromDateRange(DateTime start, DateTime end)
        {
            return new DateRange(start, end);
        }

        public bool Includes(DateTime value) => this.Start <= value && value <= this.End;

        public bool Includes(IRange<DateTime> range)
        {
            return this.Start <= range.Start && range.End <= this.End;
        }

        public override string ToString()
        {
            if (this.m_formatted == null)
                this.m_formatted = string.Format("Start {0} {1} -> End {2} {3}", (object)this.Start.ToShortDateString(), (object)this.Start.ToShortTimeString(), (object)this.End.ToShortDateString(), (object)this.End.ToShortTimeString());
            return this.m_formatted;
        }

        public bool PriorToStart(DateTime time) => time <= this.Start;

        public void ShiftOneDay()
        {
            this.Start = this.Start.AddDays(1.0);
            this.End = this.End.AddDays(1.0);
            this.m_formatted = (string)null;
        }

        public DateTime Start { get; private set; }

        public DateTime End { get; private set; }

        internal DateRange(DateTime start, DateTime end)
        {
            this.Start = start;
            this.End = end;
        }

        private DateRange(int start, int end)
        {
            DateTime now = DateTime.Now;
            this.Start = new DateTime(now.Year, now.Month, now.Day, start, 0, 0);
            this.End = new DateTime(now.Year, now.Month, now.Day, end, 0, 0);
        }
    }
}
