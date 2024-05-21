namespace MemoryVaultAPI.Models
{
    public class Account
    {
        public int AccountID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; } = false;
        public List<Memory> Memories { get; set; } = new List<Memory>();
        public List<Like> Likes { get; set; } = new List<Like>();
    }
    public class AccProfile
    {
        public int AccountID { get; set; }
        public string Username { get; set; }
        public List<MemoryShortImage> PublicMemories { get; set; }

        public AccProfile(Account account, int requesterID)
        {
            this.AccountID = account.AccountID;
            this.Username = account.Username;
            this.PublicMemories = account.Memories.Where(x => x.Public == true).Select(x => new MemoryShortImage(x, x.Likes.Where(y => y.LikerID == requesterID).Any())).ToList();
        }
    }
    public class AccountRegistration
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
