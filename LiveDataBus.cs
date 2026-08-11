using System;

namespace SOACSForgeWorks
{
    public static class LiveDataBus
    {
        public static event EventHandler DataChanged;
        public static DateTime LastChangeUtc { get; private set; }
        public static bool ApplicationReady { get; private set; }
        private static bool pendingChange;
        private static bool notifying;

        public static void SetApplicationReady(bool ready)
        {
            ApplicationReady = ready;
            if (ready && pendingChange)
            {
                pendingChange = false;
                NotifyDataChanged();
            }
        }

        public static void NotifyDataChanged()
        {
            LastChangeUtc = DateTime.UtcNow;
            if (!ApplicationReady)
            {
                pendingChange = true;
                return;
            }

            if (notifying)
            {
                pendingChange = true;
                return;
            }

            var handler = DataChanged;
            if (handler == null) return;

            notifying = true;
            try
            {
                handler(null, EventArgs.Empty);
            }
            finally
            {
                notifying = false;
            }
        }
    }

    public interface ILiveRefreshable
    {
        void RefreshData();
    }
}
