namespace SOACSForgeWorks
{
    public static class AuditManager
    {
        public static void Write(string action, InventoryItem item, string notes)
        {
            InventoryStore.AddAudit(action, item, notes);
        }
    }
}
