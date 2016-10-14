using System;
using BackyLogic;

namespace Backy
{
    internal class ConsoleProgress : IMultiStepProgress
    {
        private string _lastText;
        private int _currentValue;
        private int _maxValue;

        public void Increment()
        {
            Console.Write($"\r{_lastText}".PadRight(Console.BufferWidth, ' '));
            Console.Write($"\r{_lastText}: {++_currentValue}/{_maxValue}");
        }

        public void StartBoundedStep(string text, int maxValue)
        {
            _currentValue = 0;
            _maxValue = maxValue;
            _lastText = text;
            Console.Write("\n" + _lastText);
        }

        public void StartStepWithoutProgress(string text)
        {
            Console.Write($"\n{text}");
        }

        public void StartUnboundedStep(string text, Func<int, string> projection = null)
        {
            _lastText = text;
            Console.Write($"\n{text}");
        }

        public void UpdateProgress(int currentValue)
        {
            Console.Write($"\r{_lastText}".PadRight(Console.BufferWidth, ' '));
            Console.Write($"\r{_lastText}: {currentValue}");
        }
    }
}