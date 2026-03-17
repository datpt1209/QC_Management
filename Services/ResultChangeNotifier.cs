using System;

namespace QC_Management.Services
{
    // Simple static notifier used to signal that Results changed
    public static class ResultChangeNotifier
    {
        public static event Action? ResultsUpdated;

        public static void Notify()
        {
            try
            {
                ResultsUpdated?.Invoke();
            }
            catch
            {
                // non-fatal
            }
        }
    }
}