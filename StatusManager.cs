using System;

namespace SOACSForgeWorks
{
    public static class StatusManager
    {
        public static void RefreshStatus(MainForm form)
        {
            if (form == null) return;
            try
            {
                form.RefreshShellStatus();
            }
            catch (Exception ex)
            {
                InventoryStore.WriteStartupLog("StatusManager.RefreshStatus failed: " + ex.Message);
            }
        }
    }
}
