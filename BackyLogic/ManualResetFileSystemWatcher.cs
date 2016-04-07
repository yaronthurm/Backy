using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{
    public class ManualResetFileSystemWatcher
    {
        private DateTime _lastEventTime;
        private bool _shouldRaiseEvents;
        private object _locker = new object();
        private FileSystemWatcher _watcher = new FileSystemWatcher();

        public int Age
        {
            get { return (int)((DateTime.Now - _lastEventTime).TotalSeconds); }
        }

        public string Path
        {
            get { return _watcher.Path; }
        }

        public event Action<ManualResetFileSystemWatcher> ChangeDetected;


        public ManualResetFileSystemWatcher(string watchedDirectory)
        {
            _watcher.Changed += (s, e) => OnChange();
            _watcher.Created += (s, e) => OnChange();
            _watcher.Deleted += (s, e) => OnChange();
            _watcher.Renamed += (s, e) => OnChange();
            _watcher.IncludeSubdirectories = true;
            _watcher.Path = watchedDirectory;
            _watcher.EnableRaisingEvents = true;
        }


        private void OnChange() {
            lock (_locker)
            {
                if (ChangeDetected == null || !_shouldRaiseEvents)
                    return;

                _shouldRaiseEvents = false;
                _lastEventTime = DateTime.Now;
                ChangeDetected(this);
            }
        }


        public void StartListen()
        {
            lock (_locker)
            {
                _shouldRaiseEvents = true;
            }
        }

        public void StopListen()
        {
            lock (_locker)
            {
                _shouldRaiseEvents = false;
            }
        }

    }
}
