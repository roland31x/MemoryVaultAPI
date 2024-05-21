﻿using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

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
        public Account Owner { get; set; }
        public int OwnerID { get; set; }
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
        public Account Liker { get; set; }
        public int LikerID { get; set; }
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
