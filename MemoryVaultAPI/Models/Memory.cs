namespace MemoryVaultAPI.Models
{
    public class Memory
    {
        public int MemoryID { get; set; }
        public string Name { get; set; }
        public DateTime PostDate { get; set; }
        public string Description { get; set; }
        public bool Public { get; set; }
        public Account Owner { get; set; }
        public int OwnerID { get; set; }
    }
}
