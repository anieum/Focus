using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Windows.Input;
using Focus.Models;
using ReactiveUI;
using System.Linq;

namespace Focus.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private FocusTask _currentTask = FocusTask.Empty;

        public string Greeting => "Welcome to Avalonia!";
        public string StartText => _currentTask == FocusTask.Empty ? ":" : $": {_currentTask.StartTime}";

        public string EndText => _currentTask == FocusTask.Empty ? ":" : $": {_currentTask.EndTime}";

        public string DistractionsText => _currentTask == FocusTask.Empty ? ":" : $": {_currentTask.Distractions}";
        public string AllowedWindowsText => _currentTask == FocusTask.Empty ? ":" : ": (not implemented yet)";

        public string UserEnteredText { get; set; }


        public MainWindowViewModel()
        {
            // todo find out what this line does exactly
            StartTaskCommand = ReactiveCommand.Create(ParseInputAndStart);
        }

        // How to bind commands to buttons: https://avaloniaui.net/docs/binding/binding-to-commands
        public ReactiveCommand<Unit, Unit> StartTaskCommand { get; }

        private void ParseInputAndStart()
        {
            System.Diagnostics.Debug.Print("Button was pressed. Text: {0}", UserEnteredText);

            // try parse input
        }

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

            TimeSpan duration;

            if (time.Count() > 1)
                return false;
            else
            {
                // maybe it was given as a duration
                // Ich werde jetzt 2 Stunden lernen
                string[] durationWordlist =
                {
                    "s", "sec", "secs", "Sekunde", "Sekunden",
                    "m", "min", "mins", "Minute", "Minuten", "mins",
                    "h", "Std", "stds", "Stunde", "Stunden"
                };

                durationWordlist = durationWordlist.Select(w => w.ToUpper()).ToArray();

                var tmpDuration = from word in words
                               where durationWordlist.Any(allowedWord => word.ToUpper().Equals(allowedWord))
                               select word;

                // There's no time given
                if (tmpDuration.Count() != 1)
                    return false;

                // Or if there is
                // TODO
                //duration = TimeSpan.Parse(tmpDuration!.First());
            }



            endTime = DateTime.Now;
            nameOfTask = "";
            return true;
        }
    }


}
