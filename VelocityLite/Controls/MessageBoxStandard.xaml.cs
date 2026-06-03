using System;
using System.Printing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Windows.UI.ViewManagement;

namespace VelocityLite.Controls
{
    public enum DialogResult
    {
        None,
        Yes,
        No
    }

    public partial class MessageBoxStandard : UserControl
    {
        private TaskCompletionSource<DialogResult>? _tcs;

        public MessageBoxStandard()
        {
            InitializeComponent();
            PrimaryButton.Click += PrimaryButton_Click;
            SecondaryButton.Click += SecondaryButton_Click;
        }

        public string InputText
        {
            get => InputBox.Text;
        }

        /// <summary>
        /// Shows the dialog asynchronously and returns which button was pressed.
        /// </summary>
        public Task<DialogResult> ShowDialogAsync(
           string title, string description,
           string yesText = "Yes", string noText = "No",
           string? iconPath = null, bool showInput = false,
           string inputDefaultText = "")
        {
            _tcs = new TaskCompletionSource<DialogResult>();

            TitleText.Text = title;
            SubtitleText.Text = description;
            PrimaryButton.Content = yesText;

            InputPanel.Visibility = showInput ? Visibility.Visible : Visibility.Collapsed;
            if (showInput)
            {
                InputBox.Text = inputDefaultText;
                InputBox.Focus();
                InputBox.CaretIndex = InputBox.Text.Length;
            }

            SecondaryButton.Visibility = string.IsNullOrEmpty(noText) ? Visibility.Collapsed : Visibility.Visible;
            if (!string.IsNullOrEmpty(noText))
                SecondaryButton.Content = noText;

            if (!string.IsNullOrEmpty(iconPath))
            {
                try
                {
                    IconImage.Source = new BitmapImage(new Uri($"pack://application:,,,/{iconPath}", UriKind.Absolute));
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

            // Only show the backdrop if it's not already visible
            if (OverlayBackground.Visibility != Visibility.Visible)
            {
                OverlayBackground.Visibility = Visibility.Visible;
                AnimateBackgroundFade(OverlayBackground, 0, 0.5);
            }

            AnimatePopIn(DialogBorder);

            return _tcs.Task;
        }

        /// <summary>
        /// Hides the dialog and returns the result.
        /// </summary>
        private void HideDialog(DialogResult result)
        {
            AnimatePopOut(DialogBorder);

            AnimateBackgroundFade(OverlayBackground, OverlayBackground.Opacity, 0, () =>
            {
                Visibility = Visibility.Collapsed;

                _tcs?.TrySetResult(result);

                if (this.Parent is Panel panel)
                {
                    panel.Children.Remove(this);

                    if (!panel.Children.OfType<MessageBoxStandard>().Any())
                    {
                        OverlayBackground.Visibility = Visibility.Collapsed;
                    }
                }
            });
        }

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            HideDialog(DialogResult.Yes);
        }

        private void SecondaryButton_Click(object sender, RoutedEventArgs e)
        {
            HideDialog(DialogResult.No);
        }

        private void AnimatePopIn(UIElement element, int durationMs = 180)
        {
            element.Visibility = Visibility.Visible;

            if (element.RenderTransform is not ScaleTransform scale)
                element.RenderTransform = scale = new ScaleTransform(0.94, 0.94);

            element.Opacity = 0;

            var ease = new QuinticEase { EasingMode = EasingMode.EaseOut };

            scale.BeginAnimation(ScaleTransform.ScaleXProperty,
                new DoubleAnimation(0.94, 1, TimeSpan.FromMilliseconds(durationMs))
                { EasingFunction = ease });

            scale.BeginAnimation(ScaleTransform.ScaleYProperty,
                new DoubleAnimation(0.94, 1, TimeSpan.FromMilliseconds(durationMs))
                { EasingFunction = ease });

            element.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(durationMs)));
        }

        private void AnimatePopOut(UIElement element, int durationMs = 180)
        {
            if (element.RenderTransform is not ScaleTransform scale)
                element.RenderTransform = scale = new ScaleTransform(1, 1);

            var ease = new QuinticEase { EasingMode = EasingMode.EaseIn };

            scale.BeginAnimation(ScaleTransform.ScaleXProperty,
                new DoubleAnimation(1, 0.94, TimeSpan.FromMilliseconds(durationMs))
                { EasingFunction = ease });

            scale.BeginAnimation(ScaleTransform.ScaleYProperty,
                new DoubleAnimation(1, 0.94, TimeSpan.FromMilliseconds(durationMs))
                { EasingFunction = ease });

            var opacityAnim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(durationMs))
            {
                EasingFunction = ease
            };

            opacityAnim.Completed += (s, e) =>
            {
                element.Visibility = Visibility.Collapsed;
            };

            element.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
        }

        private void AnimateBackgroundFade(UIElement element, double from, double to, Action? onComplete = null, int durationMs = 300)
        {
            var anim = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(durationMs))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            if (onComplete != null)
                anim.Completed += (s, e) => onComplete();
            element.BeginAnimation(UIElement.OpacityProperty, anim);
        }
    }
}
