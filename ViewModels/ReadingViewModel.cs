// ViewModels/ReadingViewModel.cs
using Markdig;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using VoaDownloaderWpf.Models;
using VoaDownloaderWpf.Services;

namespace VoaDownloaderWpf.ViewModels
{
    public class ReadingViewModel : BaseViewModel
    {
        private readonly List<ArticleViewModel> _articles;
        private int _currentIndex;
        private VocabularyService _vocabService;
        public event Action<ArticleViewModel> CurrentArticleChanged;

        // 用于接收从 View 传来的 RichTextBox 实例
        public RichTextBox RichTextBox { get; set; }

        private ArticleViewModel _currentArticle;
        public ArticleViewModel CurrentArticle
        {
            get => _currentArticle;
            set { _currentArticle = value; OnPropertyChanged(); OnPropertyChanged(nameof(ArticleProgressText)); }
        }

        public string ArticleProgressText => $"{_currentIndex + 1} / {_articles.Count}";

        public ICommand NextArticleCommand { get; }
        public ICommand PreviousArticleCommand { get; }
        public ICommand HighlightCommand { get; }
        public ICommand AddToVocabCommand { get; }
        public ICommand ExportMarkdownCommand { get; }

        public ReadingViewModel(List<ArticleViewModel> articles, VocabularyService vocabService)
        {
            _articles = articles;
            _vocabService = vocabService;
            _currentIndex = 0;
            CurrentArticle = _articles.Any() ? _articles[0] : null;

            NextArticleCommand = new RelayCommand(_ => GoToNextArticle(), _ => _currentIndex < _articles.Count - 1);
            PreviousArticleCommand = new RelayCommand(_ => GoToPreviousArticle(), _ => _currentIndex > 0);
            HighlightCommand = new RelayCommand(_ => HighlightSelection());
            AddToVocabCommand = new RelayCommand(_ => AddSelectionToVocab());
            ExportMarkdownCommand = new RelayCommand(_ => ExportToMarkdown());
        }

        private void GoToNextArticle()
        {
            if (_currentIndex < _articles.Count - 1)
            {
                _currentIndex++;
                CurrentArticle = _articles[_currentIndex];
                // ########## 触发事件 ##########
                CurrentArticleChanged?.Invoke(CurrentArticle);
                InitializeAiContextAsync();
            }
        }

        private void GoToPreviousArticle()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                CurrentArticle = _articles[_currentIndex];
                // ########## 触发事件 ##########
                CurrentArticleChanged?.Invoke(CurrentArticle);
                InitializeAiContextAsync();
            }
        }

        private void HighlightSelection()
        {
            if (RichTextBox == null) return;
            TextSelection selection = RichTextBox.Selection;
            if (!selection.IsEmpty)
            {
                selection.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
            }
        }

        private void AddSelectionToVocab()
        {
            if (RichTextBox == null) return;
            string selectedWord = RichTextBox.Selection.Text.Trim();
            if (string.IsNullOrEmpty(selectedWord))
            {
                MessageBox.Show("请先选择一个单词。");
                return;
            }

            var entry = _vocabService.LookUpWord(selectedWord);
            if (entry != null)
            {
                _vocabService.AddToVocabularyBook(entry);
                MessageBox.Show($"'{entry.Word}' 已成功加入生词本！\n\n释义: {entry.Meaning}");
            }
            else
            {
                MessageBox.Show($"在本地词典中未找到单词 '{selectedWord}'。");
            }
        }

        private void ExportToMarkdown()
        {
            if (RichTextBox == null) return;

            var sfd = new SaveFileDialog
            {
                Filter = "Markdown File (*.md)|*.md",
                FileName = $"{CurrentArticle.Title}_笔记.md"
            };

            if (sfd.ShowDialog() != true) return;

            var sb = new StringBuilder();
            var doc = RichTextBox.Document;

            foreach (var block in doc.Blocks)
            {
                if (block is Paragraph para)
                {
                    foreach (var inline in para.Inlines)
                    {
                        var text = new TextRange(inline.ContentStart, inline.ContentEnd).Text;
                        if (inline.Background is SolidColorBrush brush && brush.Color == Colors.Yellow)
                        {
                            // 将高亮单词用下划线、粗体、斜体标记
                            sb.Append($"_**_{text}_**_");
                        }
                        else
                        {
                            sb.Append(text);
                        }
                    }
                    sb.AppendLine("\n");
                }
            }

            // 添加生词本汇总
            if (_vocabService.VocabularyList.Any())
            {
                sb.AppendLine("---");
                sb.AppendLine("### 生词汇总");
                sb.AppendLine("| 单词 | 释义 | 难度 |");
                sb.AppendLine("| --- | --- | --- |");
                foreach (var word in _vocabService.VocabularyList)
                {
                    sb.AppendLine($"| {word.Word} | {word.Meaning.Replace("\n", "<br>")} | {word.DifficultyLevel} |");
                }
            }

            File.WriteAllText(sfd.FileName, sb.ToString());
            MessageBox.Show($"笔记已成功导出到: {sfd.FileName}");
        }

        // 添加到 ReadingViewModel.cs 类中

        private async Task InitializeAiContextAsync()
        {
            //// 检查 _aiService 是否为 null，以增加代码健壮性
            //if (_aiService == null || CurrentArticle == null) return;

            //IsAiBusy = true;
            //AiConversationHistory = "正在初始化AI助手，请稍候...";

            //string initialPrompt = $"你是一个专业的英语语法和词汇老师。下面的文章是我们的学习材料，请基于它来回答我后续的问题：\n\n---\n{CurrentArticle.Content}\n---\n\n准备好了吗？";

            //try
            //{
            //    await _aiService.SetInitialContextAsync(initialPrompt);
            //    AiConversationHistory = $"AI助手已就绪。当前学习文章: '{CurrentArticle.Title}'.\n你可以开始提问了。";
            //}
            //catch (Exception ex)
            //{
            //    AiConversationHistory = $"AI助手初始化失败: {ex.Message}";
            //}
            //finally
            //{
            //    IsAiBusy = false;
            //}
        }


    }
}