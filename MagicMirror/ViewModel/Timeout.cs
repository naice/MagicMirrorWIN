using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace MagicMirror.ViewModel
{
    public class Timeout
    {
        DispatcherTimer _timer;

        /// <summary>
        /// Sets the duration of the timeout, stops the timeout if it is running
        /// </summary>
        public TimeSpan Duration {
            get { return _timer.Interval; }
            set
            {
                Stop();

                _timer.Interval = value;
            }
        }

        /// <summary>
        /// True if the timeout is engaded. 
        /// </summary>
        public bool IsRunning { get { return _timer != null ? _timer.IsEnabled : false; } }

        /// <summary>
        /// Action to execute
        /// </summary>
        public Action Action { get; set; }

        public Timeout()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += Tick;
        }

        public Timeout(Action action) : this()
        {
            this.Action = action;
        }

        private void Tick(object sender, object e)
        {
            Action?.Invoke();
        }

        /// <summary>
        /// Reset the timeout and start again
        /// </summary>
        public void Start(Action action = null)
        {
            Stop();

            _timer.Interval = Duration;
            _timer.Start();
        }

        /// <summary>
        /// Stops the Timeout
        /// </summary>
        public void Stop()
        {
            if (_timer.IsEnabled)
                _timer.Stop();
        }
    }
}
