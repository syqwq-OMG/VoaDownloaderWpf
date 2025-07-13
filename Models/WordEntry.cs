// Models/WordEntry.cs
using System;

namespace VoaDownloaderWpf.Models
{
    public class WordEntry
    {
        public string Word { get; set; }
        public string Meaning { get; set; }
        public string DifficultyLevel { get; set; }
        public int FrequencyRank { get; set; }
        public DateTime RecordTime { get; set; }
        public int SearchTimes { get; set; } = 1; // 默认为1次
    }
}