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
        private bool _isPaused;

        private DateTime _pauseStartTime;
        private DateTime _pauseEndTime;

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

        public bool IsPaused
        {
            get => _isPaused;
        }

        public bool HasEnded
        {
            get => EndTime != NODATE;
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
            _isPaused = false;

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

        public void PauseCountdown()
        {
            if (IsPaused || HasEnded)
                return;

            _pauseStartTime = DateTime.Now;
        }

        public void ResumeCountdown()
        {
            if (IsPaused)
                return;

            _pauseEndTime = DateTime.Now;

            PlanedEndTime = PlanedEndTime.Add(_pauseEndTime - _pauseStartTime);
        }

        public void StopCountdown()
        {
            Debug.Print("Interrupting countdown");

            _ctSource.Cancel();
            OnTaskHasEnded(EventArgs.Empty);
        }

        public int IncrementDistractions()
        {
            return ++_distractionCounter;
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

        public static bool TryExtractDuration(string text, out TimeSpan duration)
        {
            duration = TimeSpan.Zero;

            if (string.IsNullOrWhiteSpace(text))
                return false;


            var words = text.Trim().Split(" ")
                .Select(word => word.EndsWith(".") ? word.Substring(0, word.Length - 1) : word)
                .Where(word => !string.IsNullOrWhiteSpace(word));

            // maybe it was given as a duration
            // Ich werde jetzt 2 Stunden lernen

            // this is terrible coding practice
            TimeWord[] durationWordlist =
            {
                    new TimeWord("s", 1),
                    new TimeWord("sec", 1),
                    new TimeWord("secs", 1),
                    new TimeWord("Sekunde", 1),
                    new TimeWord("Sekunden", 1),

                    new TimeWord("m", 60),
                    new TimeWord("min", 60),
                    new TimeWord("mins", 60),
                    new TimeWord("Minute", 60),
                    new TimeWord("Minuten", 60),

                    new TimeWord("mins", 60),
                    new TimeWord("h", 3600),
                    new TimeWord("Std", 3600),
                    new TimeWord("stds", 3600),
                    new TimeWord("Stunde", 3600),
                    new TimeWord("Stunden", 3600)
                };

            string[] upperCaseWords = durationWordlist.Select(w => w.Word.ToUpper()).ToArray();

            var tmpDuration = from word in words.Select((Value, Index) => new { Index, Value })
                              where upperCaseWords.Any(allowedWord => word.Value.ToUpper().Equals(allowedWord))
                              select new { word.Value, word.Index };

            // There's no time given
            if (tmpDuration.Count() != 1)
                return false;

            var result = tmpDuration.First();

            if (result.Index == 0)
                return false;


            string textDuration = words.ElementAt(result.Index - 1);
            int parsedDuration = -1;

            if (!int.TryParse(textDuration, out parsedDuration))
                return false;

            int durationMultiplier = TimeWord.TryTranslate(result.Value);

            if (durationMultiplier == 0)
                return false;

            duration = TimeSpan.FromSeconds(durationMultiplier * parsedDuration);
            return true;
        }



    }
}
