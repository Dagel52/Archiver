using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;


namespace NewArchiver
{
    internal class ProgressBar 
    {
        System.Windows.Threading.Dispatcher _dispatcher;
        System.Windows.Controls.ProgressBar _progressBar;
        TextBlock _percent;
        TextBlock _heading;
        internal long _overallSize { get; set; }
        private long _currentPosition;

        public ProgressBar(System.Windows.Threading.Dispatcher dispatcher, System.Windows.Controls.ProgressBar progressBar,
            TextBlock percent, TextBlock heading)
        {
            _dispatcher = dispatcher;
            _progressBar = progressBar;
            _percent = percent;
            _heading = heading;
            _progressBar.Visibility = Visibility.Visible;
            _percent.Visibility = Visibility.Visible;
            _heading.Visibility = Visibility.Visible;
        }


        internal void UpdatePosition(long newPosition)
        {
            if (newPosition < _currentPosition || newPosition > _overallSize)
            {
                throw new InvalidOperationException();
            }
            else
            {
                _currentPosition = newPosition;
                UpdateProgress(_currentPosition);
            }
        }

        private void UpdateProgress(long newPositon)
        {
            var value = 100 * newPositon / _overallSize;
            _dispatcher.Invoke(() =>
            {
                _progressBar.Value = value;
            });

            if (value >= 100)
            {
                _dispatcher.Invoke(() =>
                {
                    _progressBar.Visibility = Visibility.Hidden;
                    _percent.Visibility = Visibility.Hidden;
                    _heading.Visibility = Visibility.Hidden;
                });
            }
            
        }
    }
}
