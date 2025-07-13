// ArticleViewModel.cs

namespace VoaDownloaderWpf
{
    public class ArticleViewModel : BaseViewModel
    {
        private bool _isSelected;

        public string Title { get; set; }
        public string Url { get; set; }

        // ########## 新增的属性 ##########
        public string Content { get; set; }
        public string AudioUrl { get; set; }
        // ##############################

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}