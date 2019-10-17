using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VkBot.Core.Timer
{
    abstract class TimerJob
    {
        public Action<CancellationToken> Action { get; set; }
        public TimeSpan Span { get; set; }
        public bool IsRunning { get; set; }
        private DateTime _startedTime;
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly JobExecutionType _executionType;

        protected TimerJob(Action<CancellationToken> action, TimeSpan span, JobExecutionType executionType)
        {
            Action = action;
            Span = span;
            _executionType = executionType;
        }

        public abstract bool IsActual();

        public abstract bool IsWillBeActualAfterExecute();

        public TimeSpan GetRemainedTimeToExecution()
        {
            return _startedTime.Add(Span) - DateTime.Now;
        }

        public bool IsNeedToExecute()
        {
            return GetRemainedTimeToExecution() < TimeSpan.Zero;
        }

        public virtual void Execute()
        {
            IsRunning = true;
            Task.Run(() =>
            {
                switch (_executionType)
                {
                    case JobExecutionType.OnlyOneInstance:
                        if (_task != null && !_task.IsCompleted)
                            _task.Wait();
                        _cancellationTokenSource = new CancellationTokenSource();
                        _task = Task.Run(() =>
                        {
                            Action.Invoke(_cancellationTokenSource.Token);
                            IsRunning = false;
                        });
                        break;
                    case JobExecutionType.KillPreviousInstance:
                        _cancellationTokenSource?.Cancel();
                        _cancellationTokenSource = new CancellationTokenSource();
                        _task = Task.Run(() =>
                        {
                            Action.Invoke(_cancellationTokenSource.Token);
                            IsRunning = false;
                        });
                        break;
                    case JobExecutionType.AllowSeveralInstances:
                        _cancellationTokenSource = new CancellationTokenSource();
                        _task = Task.Run(() =>
                        {
                            Action.Invoke(_cancellationTokenSource.Token);
                            IsRunning = false;
                        });
                        break;
                }
            });
            _startedTime = DateTime.Now;
        }

        public void Enable()
        {
            _startedTime = DateTime.Now;
        }
    }
}
