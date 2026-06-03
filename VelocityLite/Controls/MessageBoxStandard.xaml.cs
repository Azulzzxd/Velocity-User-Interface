using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace VelocityLite.Controls
{
    #region Enums

    public enum DialogResult
    {
        None,
        Yes,
        No
    }

    #endregion

    #region MessageBoxStandard Control

    public partial class MessageBoxStandard : UserControl
    {
        #region Fields

        private TaskCompletionSource<DialogResult>? _tcs;

        #endregion

        #region Constructor

        public MessageBoxStandard()
        {
            InitializeComponent();

            PrimaryButton.Click += PrimaryButton_Click;
            SecondaryButton.Click += SecondaryButton_Click;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the input text from the dialog (if enabled).
        /// </summary>
        public string InputText => InputBox.Text;

        #endregion

        #region Public API

        /// <summary>
        /// Shows the dialog asynchronously and returns the selected result.
        /// </summary>
        public Task<DialogResult> ShowDialogAsync(
            string title,
            string description,
            string yesText = "Yes",
            string noText = "No",
            string? iconPath = null,
            bool showInput = false,
            string inputDefaultText = "")
        {
            _tcs = new TaskCompletionSource<DialogResult>();

            TitleText.Text = title;
            SubtitleText.Text = description;

            PrimaryButton.Content = yesText;

            SecondaryButton.Visibility =
                string.IsNullOrEmpty(noText)
                    ? Visibility.Collapsed
                    : Visibility.Visible;

            if (!string.IsNullOrEmpty(noText))
                SecondaryButton.Content = noText;

            InputPanel.Visibility = showInput
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (showInput)
            {
                InputBox.Text = inputDefaultText;
                InputBox.Focus();
                InputBox.CaretIndex = InputBox.Text.Length;
            }

            SetIcon(iconPath);

            Visibility = Visibility.Visible;

            var backdrop = GetBackdrop();
            if (backdrop != null)
            {
                backdrop.Visibility = Visibility.Visible;
                AnimateBackgroundFade(backdrop, 0, 0.5);
            }

            AnimatePopIn(DialogBorder);

            return _tcs.Task;
        }

        #endregion

        #region Backdrop Fix (IMPORTANT)

        /// <summary>
        /// Gets the global backdrop from MainWindow.
        /// Fixes multi-overlay stacking issues.
        /// </summary>
        private Border? GetBackdrop()
        {
            var window = Application.Current.Windows
                .OfType<MainWindow>()
                .FirstOrDefault();

            return window?.MessageBoxSpawnParent?
                .Children
                .OfType<Border>()
                .FirstOrDefault(x => x.Name == "GlobalBackdrop");
        }

        #endregion

        #region Close Logic

        private void HideDialog(DialogResult result)
        {
            var backdrop = GetBackdrop();

            AnimatePopOut(DialogBorder);

            AnimateBackgroundFade(backdrop, backdrop?.Opacity ?? 0, 0, () =>
            {
                if (backdrop != null)
                    backdrop.Visibility = Visibility.Collapsed;

                Visibility = Visibility.Collapsed;

                _tcs?.TrySetResult(result);

                if (Parent is Panel panel)
                    panel.Children.Remove(this);

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

        #endregion

        #region UI Helpers

        private void SetIcon(string? iconPath)
        {
            if (string.IsNullOrEmpty(iconPath))
            {
                IconImage.Visibility = Visibility.Collapsed;
                return;
            }

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

        #endregion

        #region Animations

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

        private void AnimateBackgroundFade(
            UIElement element,
            double from,
            double to,
            Action? onComplete = null,
            int durationMs = 300)
        {
            if (element == null) return;

            var anim = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(durationMs))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            if (onComplete != null)
                anim.Completed += (s, e) => onComplete();

            element.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        #endregion
    }

    #endregion
}