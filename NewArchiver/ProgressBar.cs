using System;
using System.Windows;
using System.Windows.Controls;


namespace NewArchiver
{
    internal class ProgressBar 
    {
        private readonly System.Windows.Threading.Dispatcher Dispatcher;
        private readonly System.Windows.Controls.ProgressBar Progress;
        private readonly TextBlock Percent;
        private readonly TextBlock Heading;
        private long _overallSize;
        private long _currentPosition;

        internal long OverallSize
        {
            private get => _overallSize;
            set
            {
                if (_overallSize == 0)
                    _overallSize = value;
            }
        }

        public ProgressBar(System.Windows.Threading.Dispatcher dispatcher, System.Windows.Controls.ProgressBar progressBar,
            TextBlock percent, TextBlock heading)
        {
            Dispatcher = dispatcher;
            Progress = progressBar;
            Percent = percent;
            Heading = heading;
            Progress.Visibility = Visibility.Visible;
            Percent.Visibility = Visibility.Visible;
            Heading.Visibility = Visibility.Visible;
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
            Dispatcher.Invoke(() =>
            {
                Progress.Value = value;
            });

            if (value >= 100)
            {
                Dispatcher.Invoke(() =>
                {
                    Progress.Visibility = Visibility.Hidden;
                    Percent.Visibility = Visibility.Hidden;
                    Heading.Visibility = Visibility.Hidden;
                });
            }
            
        }
    }
}
