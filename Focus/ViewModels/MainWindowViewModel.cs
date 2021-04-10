using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Windows.Input;
using Focus.Models;
using ReactiveUI;
using System.Linq;
using System.Text.RegularExpressions;

namespace Focus.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private static readonly int STOPPED = 0;
        private static readonly int RUNNING = 1;
        private static readonly int PAUSED = 2;
        

        private FocusTask _currentTask = FocusTask.Empty;
        private int _taskState = STOPPED; // state 0: not yet started / stopped; state 1: running; state 2: paused

        public string Greeting => "Welcome to Avalonia!";
        public string StartText => _currentTask == FocusTask.Empty ? ":" : $":     {_currentTask.StartTime}";

        public string EndText => _currentTask == FocusTask.Empty ? ":" : $":     {_currentTask.PlanedEndTime}";

        public string DistractionsText => _currentTask == FocusTask.Empty ? ":" : $":     {_currentTask.Distractions}";
        public string AllowedWindowsText => _currentTask == FocusTask.Empty ? ":" : ":     (not implemented yet)";
        public bool TaskIsPaused => _taskState == PAUSED;
        public bool TaskIsFinished => _taskState == STOPPED && _currentTask.HasEnded;

        public string UserEnteredText { get; set; }

        private FocusTask CurrentTask
        {
            get => _currentTask;
            set
            {
                _currentTask = value;
                this.RaisePropertyChanged(nameof(StartText));
                this.RaisePropertyChanged(nameof(EndText));
                this.RaisePropertyChanged(nameof(DistractionsText));
                this.RaisePropertyChanged(nameof(AllowedWindowsText));
            }
        }

        public int TaskState
        {
            get => _taskState;
            set {
                this.RaiseAndSetIfChanged(ref _taskState, value);
                this.RaisePropertyChanged(nameof(FocusTaskIsStartable));
                this.RaisePropertyChanged(nameof(FocusTaskIsPausable));
                this.RaisePropertyChanged(nameof(FocusTaskIsStoppable));
                this.RaisePropertyChanged(nameof(TaskIsPaused));
                this.RaisePropertyChanged(nameof(TaskIsFinished));
                this.RaisePropertyChanged(nameof(EndText));
            }
        }

        public bool FocusTaskIsStartable => _taskState == STOPPED || _taskState == PAUSED; // _currentTask == FocusTask.Empty || _currentTask.IsPaused
        
        public bool FocusTaskIsPausable => _taskState == RUNNING;
        public bool FocusTaskIsStoppable => _taskState == RUNNING || _taskState == PAUSED;

        public MainWindowViewModel()
        {
            // todo find out what this line does exactly
            StartTaskCommand = ReactiveCommand.Create(ParseInputAndStartFocusTask);
            PauseTaskCommand = ReactiveCommand.Create(PauseFocusTask);
            StopTaskCommand = ReactiveCommand.Create(StopFocusTask);
        }

        // How to bind commands to buttons: https://avaloniaui.net/docs/binding/binding-to-commands
        public ReactiveCommand<Unit, Unit> StartTaskCommand { get; }
        public ReactiveCommand<Unit, Unit> PauseTaskCommand { get; }
        public ReactiveCommand<Unit, Unit> StopTaskCommand { get; }



        private void ParseInputAndStartFocusTask()
        {
            System.Diagnostics.Debug.Print("Startbutton was pressed. Text: {0}. State: {1}", UserEnteredText, RUNNING);
            
            if (TaskState != PAUSED)
            {
                string taskName;
                DateTime endTime;

                if (TryParseInput(UserEnteredText, out endTime, out taskName))
                {
                    CurrentTask = new FocusTask(planedEndTime: endTime);

                }
            }
            else
            {
                _currentTask.ResumeCountdown();
            }

            TaskState = RUNNING;
            
            // try parse input

            // update TaskState if input is valid

        }

        private void PauseFocusTask()
        {
            System.Diagnostics.Debug.Print("Pausebutton was pressed. State: {0}", TaskState);

            _currentTask.PauseCountdown();
            
            TaskState = PAUSED;
        }

        private void StopFocusTask()
        {
            System.Diagnostics.Debug.Print("Stopbutton was pressed. State: {0}", TaskState);

            _currentTask.StopCountdown();

            // Always update ui last. If this was implemented properly an update of the ui would update the _currentTask object. <- Todo
            TaskState = STOPPED;
        }

        // Todo: Should be static and part of the model
        private bool TryParseInput(string text, out DateTime endTime, out string nameOfTask)
        {
            // Ich werde bis um 15:00 Uhr
            endTime = DateTime.Now;
            nameOfTask = "";

            if (string.IsNullOrWhiteSpace(text))
                return false;
            

            var words = text.Trim().Split(" ")
                .Select(word => word.EndsWith(".") ? word.Substring(0, word.Length -1) : word)
                .Where(word => !string.IsNullOrWhiteSpace(word));

            // check if there is a time given
            var time = from word in words
                       where word.Contains(":")
                       select word;

            // TODO! time could be null
            if (time.Any())
            {
                endTime = DateTime.Parse(time.FirstOrDefault());

                if (endTime < DateTime.Now)
                    endTime = endTime.AddDays(1);
            }



            TimeSpan duration = TimeSpan.Zero;

            if (time.Count() > 1)
                return false;

            if (time.Count() == 0 && !FocusTask.TryExtractDuration(text, out duration))
                return false;

            endTime = DateTime.Now.Add(duration);


            
            nameOfTask = "ExampleName";
            return true;
        }
    }


}
