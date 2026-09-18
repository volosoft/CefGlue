using System;
using Avalonia.Threading;
using Xilium.CefGlue.Common.Handlers;

namespace Xilium.CefGlue.Avalonia
{
    internal class AvaloniaBrowserProcessHandler : BrowserProcessHandler
    {
        private readonly object _schedule = new object();
        private DispatcherTimer _current;

        protected override void OnScheduleMessagePumpWork(long delayMs)
        {
            lock (_schedule)
            {
                _current?.Stop();

                if (delayMs <= 0)
                {
                    delayMs = 1;
                }

                // CEF raises this from one of its own threads. Avalonia 12 binds a DispatcherTimer to
                // the dispatcher of the creating thread, so the UI dispatcher has to be passed explicitly.
                var timer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher.UIThread)
                {
                    Interval = TimeSpan.FromMilliseconds(delayMs)
                };

                timer.Tick += (_, _) => CefRuntime.DoMessageLoopWork();
                timer.Start();

                _current = timer;
            }
        }
    }
}
