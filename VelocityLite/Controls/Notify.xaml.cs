using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace VelocityLite.Controls
{
    public partial class NotificationControl : UserControl
    {
        private CancellationTokenSource? _lifetimeCts;
        private int _remainingMs;
        private bool _isHovered;

        private static readonly Regex LinkRegex = new(@"\[(?<text>[^\]]+)\]\((?<url>[^)]+)\)",  RegexOptions.Compiled); 

        public NotificationControl()
        {
            InitializeComponent();

            MouseEnter += (_, _) => PauseLifetime();
            MouseLeave += (_, _) => ResumeLifetime();
        }


        /// <summary>
        /// Shows the notification with title, description, optional icon, slides in/out horizontally,
        /// and removes itself from the parent after sliding out.
        /// </summary>
       
        public async Task ShowAsync(string title, string description, string? iconPath = null, int durationMs = 5000)
        {
            TitleText.Text = title;
            SetDescriptionWithLinks(description);

            if (!string.IsNullOrEmpty(iconPath))
            {
                try
                {
                    IconImage.Source = new BitmapImage(
                        new Uri($"pack://application:,,,/{iconPath}", UriKind.Absolute));
                    IconImage.Visibility = Visibility.Visible;
                }
                catch
                {
                    IconImage.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                IconImage.Visibility = Visibility.Collapsed;
            }

            Visibility = Visibility.Visible;

            var easing = new CubicEase { EasingMode = EasingMode.EaseInOut };

            TranslateTransform.BeginAnimation(
                TranslateTransform.XProperty,
                new DoubleAnimation(380, 0, TimeSpan.FromMilliseconds(400)) { EasingFunction = easing });

            RootBorder.BeginAnimation(
                OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(350)) { EasingFunction = easing });

            _remainingMs = durationMs;
            _lifetimeCts = new CancellationTokenSource();

            await RunLifetimeAsync(_lifetimeCts.Token);
        }

        private void SetDescriptionWithLinks(string text)
        {
            DescriptionText.Inlines.Clear();

            int lastIndex = 0;

            foreach (Match match in LinkRegex.Matches(text))
            {
                if (match.Index > lastIndex)
                {
                    DescriptionText.Inlines.Add(
                        new Run(text.Substring(lastIndex, match.Index - lastIndex)));
                }

                var linkText = match.Groups["text"].Value;
                var linkUrl = match.Groups["url"].Value;

                var hyperlink = new Hyperlink(new Run(linkText))
                {
                    NavigateUri = Uri.TryCreate(linkUrl, UriKind.Absolute, out var uri) ? uri: null,
                    Foreground = Brushes.DeepSkyBlue,
                    Cursor = Cursors.Hand,
                    ToolTip = linkUrl
                };

                hyperlink.Click += (_, _) =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = linkUrl,
                            UseShellExecute = true
                        });
                    }
                    catch
                    {
                    }
                };

                DescriptionText.Inlines.Add(hyperlink);

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < text.Length)
            {
                DescriptionText.Inlines.Add(
                    new Run(text.Substring(lastIndex)));
            }
        }

        private async Task RunLifetimeAsync(CancellationToken token)
        {
            const int tick = 50;

            try
            {
                while (_remainingMs > 0)
                {
                    await Task.Delay(tick, token);

                    if (!_isHovered)
                        _remainingMs -= tick;
                }

                await HideAsync();
            }
            catch (TaskCanceledException)
            {
            }
        }


        private void PauseLifetime()
        {
            _isHovered = true;
        }

        private void ResumeLifetime()
        {
            _isHovered = false;
        }

        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            await HideAsync();
        }

        public void AnimateToOffset(double yOffset)
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

            var anim = new DoubleAnimation
            {
                To = yOffset,
                Duration = TimeSpan.FromMilliseconds(260),
                EasingFunction = ease
            };

            TranslateTransform.BeginAnimation(TranslateTransform.YProperty, anim);
        }


        private async Task HideAsync()
        {
            var easing = new CubicEase { EasingMode = EasingMode.EaseInOut };

            var slideOut = new DoubleAnimation
            {
                From = 0,
                To = 380,
                Duration = TimeSpan.FromMilliseconds(350),
                EasingFunction = easing
            };

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = easing
            };

            TranslateTransform.BeginAnimation(TranslateTransform.XProperty, slideOut);
            RootBorder.BeginAnimation(OpacityProperty, fadeOut);

            await Task.Delay(350);

            Visibility = Visibility.Collapsed;

            if (Parent is Panel panel)
                panel.Children.Remove(this);
        }

    }
}