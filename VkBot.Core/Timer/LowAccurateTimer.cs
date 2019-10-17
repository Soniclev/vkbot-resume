using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VkBot.Core.Timer
{
    public class LowAccurateTimer : ITimer
    {
        private System.Timers.Timer _timer = new System.Timers.Timer();
        private List<TimerJob> _jobs = new List<TimerJob>();

        public LowAccurateTimer()
        {
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timer.Stop();
            Awake();
            ReconfigureTimer();
        }

        private void ReconfigureTimer()
        {
            _timer.Stop();
            var sleepTime = GetSleepTime();
            if (sleepTime.HasValue)
            {
                _timer.Interval = sleepTime.Value.TotalMilliseconds;
                _timer.Start();
            }
        }

        private void AddJob(TimerJob job)
        {
            _jobs.Add(job);
            job.Enable();
            ReconfigureTimer();
        }

        public void AddNumberTimes(Action<CancellationToken> action, TimeSpan timeSpan, JobExecutionType executionType, long times = 0)
        {
            if (times > 0)
                AddJob(new Specified(action, timeSpan, executionType, times));
            else
                AddJob(new InfinityTimerJob(action, timeSpan, executionType));
        }

        public void AddOneTime(Action<CancellationToken> action, TimeSpan timeSpan)
        {
            AddJob(new Specified(action, timeSpan, JobExecutionType.OnlyOneInstance, 1));
        }

        public void AddUntil(Action<CancellationToken> action, TimeSpan timeSpan, JobExecutionType executionType, Func<bool> condition)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Action<CancellationToken> action)
        {
            return _jobs.RemoveAll(x => x.Action == action) > 0;
        }

        public bool Remove(IEnumerable<Action<CancellationToken>> actions)
        {
            return _jobs.RemoveAll(x => actions.Contains(x.Action)) > 0;
        }

        public bool RemoveAll()
        {
            if (_jobs.Count > 0)
            {
                _jobs.Clear();
                return true;
            }

            return false;
        }

        private void Awake()
        {
            var jobs = _jobs.Where(x => x.IsNeedToExecute()).ToList();
            foreach (var job in jobs)
            {
                if (!job.IsWillBeActualAfterExecute())
                    _jobs.Remove(job);
                job.Execute();
            }
        }

        private TimeSpan? GetSleepTime()
        {
            var jobs = _jobs.ToArray();
            if (!jobs.Any())
                return null;

            var sleepTime = jobs.First().GetRemainedTimeToExecution();
            for (var i = 1; i < jobs.Length; i++)
            {
                var timeSpan = jobs[i].GetRemainedTimeToExecution();
                if (timeSpan < sleepTime)
                    sleepTime = jobs[i].GetRemainedTimeToExecution();
            }

            if (sleepTime.TotalMilliseconds < 0)
                sleepTime = TimeSpan.FromMilliseconds(1);
            return sleepTime;
        }
    }
}
