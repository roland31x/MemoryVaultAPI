using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Newtonsoft.Json;

namespace MemoryVaultAPI.Models
{
    public class Memory
    {
        public int MemoryID { get; set; }
        public string Name { get; set; }
        public DateTime PostDate { get; set; }
        public string Description { get; set; }
        public bool Public { get; set; }
        public List<Image> Images { get; set; } = new List<Image>();
        public List<Like> Likes { get; set; } = new List<Like>();
        [JsonIgnore]
        public Account Owner { get; set; }
        public int OwnerID { get; set; }
        public string OwnerName { get { return Owner.Username; } }
    }
    public class MemoryShort
    {
        public int MemoryID { get; set; }
        public string Name { get; set; }
        public bool Public { get; set; }
        public string OwnerName { get; set; }
        public int OwnerID { get; set; }
        public int Likes { get; set; }
        public MemoryShort(Memory memory)
        {
            this.MemoryID = memory.MemoryID;
            this.Name = memory.Name;
            this.Public = memory.Public;
            this.OwnerName = memory.OwnerName;
            this.OwnerID = memory.OwnerID;
            this.Likes = memory.Likes.Count;
        }
    }

    public class MemoryShortImage
    {
        public int MemoryID { get; set; }
        public string Name { get; set; }
        public bool Public { get; set; }
        public DateTime PostDate { get; set; }
        public string OwnerName { get; set; }
        public Image MainImage { get; set; }
        public int OwnerID { get; set; }
        public int Likes { get; set; }
        public bool LikedByUser { get; set; }
        public MemoryShortImage(Memory memory, bool likedByUser)
        {
            this.MemoryID = memory.MemoryID;
            this.Name = memory.Name;
            this.Public = memory.Public;
            this.PostDate = memory.PostDate;
            this.OwnerName = memory.OwnerName;
            this.OwnerID = memory.OwnerID;
            this.Likes = memory.Likes.Count;
            this.MainImage = memory.Images[0];
            LikedByUser = likedByUser;
        }
    }

    public class Image
    {
        public byte[] bytes { get; set; }
        public Image(byte[] bytes)
        {
            this.bytes = bytes;
        }
    }

    public class Like
    {
        [JsonIgnore]
        public Account Liker { get; set; }
        public int LikerID { get; set; }
        [JsonIgnore]
        public Memory Memory { get; set; }
        public int MemoryID { get; set; }
    }

    public class MemoryEntry
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[][] Images { get; set; }
        public string Public { get; set; }
    }
}
