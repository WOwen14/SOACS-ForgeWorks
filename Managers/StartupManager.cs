using System;

namespace SOACSForgeWorks
{
    public static class StartupManager
    {
        public static void InitializeAfterShown(MainForm form)
        {
            if (form == null) return;
            try
            {
                StatusManager.RefreshStatus(form);
                InventoryStore.WriteStartupLog("Startup phase complete: main window shown and shell status refreshed.");
            }
            catch (Exception ex)
            {
                InventoryStore.WriteStartupLog("StartupManager.InitializeAfterShown failed: " + ex.Message);
            }
        }
    }
}
