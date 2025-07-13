// Converters/ValueConverters.cs
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using VoaDownloaderWpf.Models;

namespace VoaDownloaderWpf.Converters
{
    // 这个转换器之前在 MainWindow.xaml.cs 中，现在移到这里
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is bool boolValue && boolValue);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // 新增：根据发送者返回不同的背景画刷
    public class SenderToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sender = (MessageSender)value;
            return sender switch
            {
                MessageSender.User => new SolidColorBrush(Color.FromRgb(210, 230, 255)), // 淡蓝色
                MessageSender.AI => new SolidColorBrush(Colors.WhiteSmoke),
                _ => new SolidColorBrush(Colors.Transparent),
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // 新增：根据发送者返回不同的对齐方式
    public class SenderToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sender = (MessageSender)value;
            return sender == MessageSender.User ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}