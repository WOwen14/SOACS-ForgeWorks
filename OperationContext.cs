namespace SOACSForgeWorks
{
    public class OperationContext
    {
        public string OperationType { get; set; }
        public string ScannedValue { get; set; }
        public int Quantity { get; set; }
        public string Project { get; set; }
        public string Location { get; set; }
        public string Notes { get; set; }

        public OperationContext()
        {
            Quantity = 1;
        }
    }
}
