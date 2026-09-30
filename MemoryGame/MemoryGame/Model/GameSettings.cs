using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MemoryGame.Model
{
    public class GameSettings : INotifyPropertyChanged
    {
        private int _timeLimit = 60;
        private int _rows = 4;
        private int _columns = 4;
        private static GameSettings _instance;

        public static GameSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameSettings();
                }
                return _instance;
            }
        }

        private GameSettings()
        {
        }

        public int TimeLimit
        {
            get => _timeLimit;
            set
            {
                if (value < 10)
                    value = 10;
                if (value > 300)
                    value = 300;

                _timeLimit = value;
                OnPropertyChanged();
            }
        }

        public int Rows
        {
            get => _rows;
            set
            {
                if (value < 2)
                    value = 2;
                if (value > 8)
                    value = 8;

                _rows = value;
                OnPropertyChanged();
            }
        }

        public int Columns
        {
            get => _columns;
            set
            {
                if (value < 2)
                    value = 2;
                if (value > 8)
                    value = 8;

                _columns = value;
                OnPropertyChanged();
            }
        }

        public int TotalCards => Rows * Columns;

        public bool IsValidConfiguration()
        {
            return TotalCards % 2 == 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}