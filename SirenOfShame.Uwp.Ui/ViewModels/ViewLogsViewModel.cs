using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SirenOfShame.Uwp.Ui.Annotations;
using SirenOfShame.Uwp.Watcher;

namespace SirenOfShame.Uwp.Ui.Models
{
    public sealed class ViewLogsViewModel : INotifyPropertyChanged
    {
        private List<LogEntry> _events;
        private bool _showAll;

        public List<LogEntry> Events
        {
            get { return _events; }
            set
            {
                if (Equals(value, _events)) return;
                _events = value;
                OnPropertyChanged();
            }
        }

        public bool ShowAll
        {
            get { return _showAll; }
            set
            {
                if (value == _showAll) return;
                _showAll = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}