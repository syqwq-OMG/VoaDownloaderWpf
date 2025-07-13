// Services/VocabularyService.cs
using CsvHelper;
using SQL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VoaDownloaderWpf.Models;

namespace VoaDownloaderWpf.Services
{
    public class VocabularyService
    {
        private readonly List<dynamic> _dictionary;
        public List<WordEntry> VocabularyList { get; private set; } = new List<WordEntry>();

        public VocabularyService(string dictionaryPath)
        {
            // 使用 CsvHelper 加载字典文件
            using (var reader = new StreamReader(dictionaryPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                _dictionary = csv.GetRecords<dynamic>().ToList();
            }
        }

        public WordEntry LookUpWord(string word)
        {
            var entry = _dictionary.FirstOrDefault(d => d.word.Equals(word, StringComparison.OrdinalIgnoreCase));
            if (entry == null) return null;

            return new WordEntry
            {
                Word = entry.word,
                Meaning = entry.translation?.Replace("\\n", "\n"), // 清理换行符
                DifficultyLevel = entry.tag,
                FrequencyRank = int.TryParse(entry.frq, out int frq) ? frq : 99999,
                RecordTime = DateTime.Now
            };
        }

        public void AddToVocabularyBook(WordEntry entry)
        {
            var existing = VocabularyList.FirstOrDefault(w => w.Word.Equals(entry.Word, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.SearchTimes++;
                existing.RecordTime = DateTime.Now;
            }
            else
            {
                VocabularyList.Add(entry);
            }
        }

        // ########## 功能 4 的一部分：数据库上传 ##########
        // 用下面的版本替换掉原来的 UploadToDatabaseAsync 方法
        public async Task<string> UploadToDatabaseAsync()
        {
            // 注意：这里不再需要连接字符串参数，因为它应该由 SQLHelper 内部管理
            if (!VocabularyList.Any()) return "生词本是空的，无需上传。";

            int successCount = 0;
            int failCount = 0;

            // 将同步的数据库操作放到后台线程执行，避免UI卡顿
            await Task.Run(() =>
            {
                foreach (var word in VocabularyList)
                {
                    try
                    {
                        string sql = @"
                    MERGE tblEnglishWordList AS target
                    USING (SELECT @Word AS Word) AS source
                    ON (target.word = source.Word)
                    WHEN MATCHED THEN
                        UPDATE SET search_times = target.search_times + 1, record_time = @RecordTime
                    WHEN NOT MATCHED THEN
                        INSERT (word, meaning, record_time, search_times, difficulty_level, frequency_rank)
                        VALUES (@Word, @Meaning, @RecordTime, @SearchTimes, @DifficultyLevel, @FrequencyRank);";

                        // 创建 SqlParameter 数组，以匹配您 SQLHelper 的方法
                        var parameters = new SqlParameter[]
                        {
                            new SqlParameter("@Word", word.Word),
                            new SqlParameter("@Meaning", (object)word.Meaning ?? DBNull.Value),
                            new SqlParameter("@RecordTime", word.RecordTime),
                            new SqlParameter("@SearchTimes", word.SearchTimes),
                            new SqlParameter("@DifficultyLevel", (object)word.DifficultyLevel ?? DBNull.Value),
                            new SqlParameter("@FrequencyRank", (object)word.FrequencyRank ?? DBNull.Value)
                        };

                        // 直接调用静态方法，并传入 SQL 语句和参数数组
                        // 假设您的方法是 ExecuteNonQuery，如果不是请替换成正确的方法名
                        SQLHelper.ExecuteNonQuery(sql, parameters);

                        successCount++;
                    }
                    catch
                    {
                        failCount++;
                    }
                }
            });

            return $"上传完成！成功: {successCount}, 失败: {failCount}";
        }
    }
}