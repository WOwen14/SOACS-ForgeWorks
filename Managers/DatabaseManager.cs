using System.Collections.Generic;

namespace SOACSForgeWorks
{
    public static class DatabaseManager
    {
        public static List<InventoryItem> LoadItems() { return InventoryStore.LoadItems(); }
        public static List<ProjectRecord> LoadProjects() { return InventoryStore.LoadProjects(); }
        public static void Reload() { InventoryStore.Load(); }
    }
}
