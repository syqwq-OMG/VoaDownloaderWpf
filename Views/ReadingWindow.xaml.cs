using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Threading;
using VoaDownloaderWpf.ViewModels;

namespace VoaDownloaderWpf.Views
{
    public partial class ReadingWindow : Window
    {
        private ReadingViewModel _viewModel;
        private DispatcherTimer _timer;
        private bool _isDragging = false; // 用于标记滑块是否正在被拖动

        public ReadingWindow(ReadingViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            // 将 RichTextBox 实例传递给 ViewModel
            _viewModel.RichTextBox = rtbContent;

            // 订阅 ViewModel 的文章切换事件
            _viewModel.CurrentArticleChanged += OnCurrentArticleChanged;

            // 初始化计时器，用于更新进度条
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += Timer_Tick;

            // 手动触发一次，加载初始文章
            OnCurrentArticleChanged(_viewModel.CurrentArticle);
        }

        // 当 ViewModel 通知文章已切换时，更新UI
        private void OnCurrentArticleChanged(ArticleViewModel newArticle)
        {
            if (newArticle == null) return;

            // 1. 更新 RichTextBox 的内容
            rtbContent.Document.Blocks.Clear();
            rtbContent.Document.Blocks.Add(new Paragraph(new Run(newArticle.Content)));

            // 2. 更新 MediaElement 的源并停止当前播放
            mediaPlayer.Stop();
            mediaPlayer.Source = new Uri(newArticle.AudioUrl);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan && !_isDragging)
            {
                lblCurrentTime.Text = mediaPlayer.Position.ToString(@"mm\:ss");
                timelineSlider.Value = mediaPlayer.Position.TotalSeconds;
            }
        }

        #region Media Player Event Handlers

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                TimeSpan ts = mediaPlayer.NaturalDuration.TimeSpan;
                timelineSlider.Maximum = ts.TotalSeconds;
                lblTotalTime.Text = ts.ToString(@"mm\:ss");
            }
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            _timer.Stop();
            PlayButton.Visibility = Visibility.Visible;
            PauseButton.Visibility = Visibility.Collapsed;
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
            _timer.Start();
            PlayButton.Visibility = Visibility.Collapsed;
            PauseButton.Visibility = Visibility.Visible;
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            _timer.Stop();
            PlayButton.Visibility = Visibility.Visible;
            PauseButton.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Slider Event Handlers

        private void TimelineSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            _isDragging = true;
            mediaPlayer.Pause();
        }

        private void TimelineSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _isDragging = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(timelineSlider.Value);
            mediaPlayer.Play();
        }

        private void TimelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // 只有在拖动时才更新时间标签
            if (_isDragging)
            {
                lblCurrentTime.Text = TimeSpan.FromSeconds(timelineSlider.Value).ToString(@"mm\:ss");
            }
        }

        #endregion
    }
}