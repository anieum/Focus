using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Focus.Models
{
    class FocusTask
    {
        private static readonly DateTime NODATE = DateTime.MinValue;
        private CancellationTokenSource _ctSource;
        private CancellationToken _cToken;

        private int _distractionCounter;

        public event EventHandler TaskEnded;
        public event EventHandler TaskPausedOrContinued;

        public DateTime StartTime { get; set; }
        public DateTime PlanedEndTime { get; set; }
        public DateTime EndTime { get; private set; }
        public static readonly FocusTask Empty = new FocusTask(DateTime.Now);

        public TimeSpan TimeRemaining
        {
            get => EndTime == NODATE ? PlanedEndTime - StartTime : TimeSpan.MinValue;
        }

        public bool HasEnded
        {
            get => TimeRemaining.Ticks == 0;
        }

        public int Distractions
        {
            get => _distractionCounter;
        }


        public FocusTask(TimeSpan planedDuration) : this(DateTime.Now.Add(planedDuration))
        {  }

        public FocusTask(DateTime planedEndTime)
        {
            _ctSource = new();
            _cToken = _ctSource.Token;

            StartTime = DateTime.Now;
            PlanedEndTime = planedEndTime;
            EndTime = NODATE;
            _distractionCounter = 0;

            TaskEnded += TaskEndedRoutine;
        }

        protected virtual void OnTaskHasEnded(EventArgs e)
        {
            EventHandler handler = TaskEnded;
            handler?.Invoke(this, e);
        }

        private async Task StartCountdownAsync()
        {
            Debug.Print("Starting countdown");

            var sleepDuration = PlanedEndTime - StartTime;

            await Task.Delay(sleepDuration, _cToken);
            OnTaskHasEnded(EventArgs.Empty);
        }

        public void StopCountdown()
        {
            Debug.Print("Interrupting countdown");

            _ctSource.Cancel();
            OnTaskHasEnded(EventArgs.Empty);
        }

        private static void TaskEndedRoutine(object sender, EventArgs e)
        {
            if (sender is FocusTask task)
                task.EndTime = DateTime.Now;

            Debug.Print("Received task end signal on {0}", DateTime.Now);
        }

        public static void Main()
        {
            var task = new FocusTask(DateTime.Now.AddSeconds(5));
            task.StartCountdownAsync();

            Console.WriteLine("Enter q to exit");
            while (Console.Read() != 'q') ;

            // todo ensure that times always are >0
        }



    }
}
