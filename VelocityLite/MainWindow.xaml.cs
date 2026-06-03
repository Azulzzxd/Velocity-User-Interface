// File: Veloskiddy.MainWindow.xaml.cs
// Type: Backend, CS (C#)
// Framework: .NET 10.0

using Microsoft.Win32;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Media;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VelocityAPI;
using VelocityLite.Controls;
using XamlAnimatedGif;

namespace VelocityLite
{
    /// <summary>
    /// Interaction logic for MainWindow, this code fucking sucks LOL
    /// </summary>

    #region Messsage

    /// <summary>
    ///  Public class so its ignored by the obfuscator, filled with messages for anyone opening this assembly in a decompiler,
    /// </summary>
    /// 

    public static class Velocity_Decrypt
    {
        public const string Comment = "Shitty backend for the main window. A lot of the code should be put into separate classes ";

        public const string DecompilerNote = "Also Hello decompiler! (skid) " +
                                             "(that's if you manage to deobfuscate this monstrosity of a codebase _without_ losing your mind).";

        public const string Note = "Thank my son for keeping me up during nights giving me time to work on this :P -- Azul 18/02/2026. ";

        public const string NoteLeave = "if you're wondering why I left the roblox exploiting community, check out my rant at:" +
                                        "https://gist.github.com/Azulezzz/18ff737f2d33c4fd1e448b082ae58549";

        public const string NoteLeaveA = "I'm getting married this or next year, Lucky me! : ) ";

        public const string NoteSpecial = "and if you're wondering how my son is doing, he's fine and happy" +
                                          ", he recently said his first words, he'll also be a big brother in a few months too : )";


        public const string Goodbye = "Oh well its time for you to go, how sad :C " +
                                        "Have a good day Mr -- Azul";
    }

    #endregion

    #region XAML Converts & Definitions

    /// <summary>
    ///  a converter that converts null values to Visibility.Collapsed and non-null values to Visibility.Visible -- used in XAML bindings & styles
    /// </summary>

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EditorTabControl : TabControl
    {
        public event RoutedEventHandler? AddTabClicked;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("AddTabButton") is Button button)
                button.Click += (s, e) => AddTabClicked?.Invoke(this, e);
        }
    }

    #region Converter 
    public class IsRootConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TreeViewItem tvi)
            {
                ItemsControl parent = ItemsControl.ItemsControlFromItemContainer(tvi);
                return parent is TreeView;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
                return v == Visibility.Visible;
            return false;
        }
    }

    #endregion

    public class AnimatedTabPanel : Panel
    {
        protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
        {
            double totalWidth = 0;
            double maxHeight = 0;

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);
                totalWidth += child.DesiredSize.Width;
                maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
            }

            double finalWidth = double.IsInfinity(availableSize.Width) ? totalWidth : availableSize.Width;

            return new System.Windows.Size(finalWidth, maxHeight);
        }


        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            double offset = 0;
            foreach (UIElement child in InternalChildren)
            {
                double width = child.DesiredSize.Width;

                if (child.RenderTransform is not TranslateTransform tt)
                {
                    tt = new TranslateTransform();
                    child.RenderTransform = tt;
                }

                var anim = new DoubleAnimation
                {
                    To = offset,
                    Duration = TimeSpan.FromSeconds(0.25),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                tt.BeginAnimation(TranslateTransform.XProperty, anim);

                child.Arrange(new Rect(0, 0, width, finalSize.Height));
                offset += width;
            }

            return finalSize;
        }
    }

    #endregion

    #region Explorer Converter

    public class CategoryOrderConverter : IValueConverter
    {
        private static readonly Dictionary<string, int> OrderMap = new()
        {
            ["FAVOURITES"] = 1,
            ["COMMUNITY"] = 2,
            ["FILES"] = 5,
        };


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string category)
            {
                if (OrderMap.TryGetValue(category, out var order))
                {
                    return order;
                }
                else
                {
                   
                    return 4;
                }
            }
            return int.MaxValue; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CategoryComparer : IComparer
    {
        private static readonly Dictionary<string, int> OrderMap = new()
        {
            ["FAVOURITES"] = 1,
            ["SCRIPTBLOX (CLOUD)"] = 2,
            ["COMMUNITY"] = 3,
            ["FILES"] = 5,
        };

        public int Compare(object? x, object? y)
        {
            string? s1 = x as string;
            string? s2 = y as string;

            if (s1 == null && s2 == null) return 0;
            if (s1 == null) return -1;
            if (s2 == null) return 1;

            int orderX = OrderMap.TryGetValue(s1, out var ox) ? ox : 4;
            int orderY = OrderMap.TryGetValue(s2, out var oy) ? oy : 4;

            return orderX.CompareTo(orderY);
        }
    }
    #endregion

    public partial class MainWindow : Window
    {
        #region Definitions

        /// <summary>
        /// Stores application state, UI references, client tracking data,
        /// networking components, timers, and configuration objects used
        /// throughout the main window.
        /// </summary>

        private int TabCounter = 0;
        private VelAPI velocityApi = new VelAPI();
        private bool _isRefreshing = false;
        private ListCollectionView? _explorerView;
        private AppSettings _appSettings;
        private bool _isInitializing = false;
        private TabItem? _tabToRename;
        private CancellationTokenSource? _autoInjectCts = null;
        private readonly Dictionary<int, ClientEntry> _clientEntries = new();
        private bool _isAutoInjectRunning = false;
        private Border? _currentVisiblePage = null;
        private MemoryStream? _gifStream;
        private readonly HashSet<TabItem> _openedTabs = new();
        private DispatcherTimer? _versionCheckTimer;
        private ClientWebSocket _webSocket = new();
        private string localVersion = "0.0.0";
        private string apiVersion = "0.0.0";
        private DispatcherTimer _clientScanTimer;
        private SettingItem? _previousSelectedItem;
        private Border? _selectedButton;

        private string? _logFilePath;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            Titlebar.Background = _titlebarBgBrush;
            Titlebar.BorderBrush = _titlebarBorderBrush;

            SplashScreen.Visibility = Visibility.Visible;

            SetTimeBasedSplash();

            // _ = LoadScriptsAsync(); // Not adding this right now - sorry! -- azul 07/01/2026

            #region Main window events

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            this.Activated += Window_Activated;
            this.Deactivated += Window_Deactivated;

            _appSettings = SettingsManager.Load();


            _clientScanTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _clientScanTimer.Tick += ClientScanTimer_Tick;

            #endregion

        }

        #region Version Management

        /// <summary>
        /// Starts the background timer responsible for periodically
        /// checking for new Velocity releases.
        /// </summary>
        
        #region Version Checking

        private void StartVersionCheckTimer()
        {
            _versionCheckTimer = new DispatcherTimer();
            _versionCheckTimer.Interval = TimeSpan.FromSeconds(60);
            _versionCheckTimer.Tick += async (s, e) => await CheckVersionAsync();
            _versionCheckTimer.Start();
        }

        /// <summary>
        /// Retrieves the latest version information from the API and
        /// compares it against the locally installed version.
        /// </summary>
        private async System.Threading.Tasks.Task CheckVersionAsync()
        {
            ReadLocalVersion();

            try
            {
                using HttpClient httpClient = new();

                string jsonString =
                    await httpClient.GetStringAsync("https://realvelocity.xyz/status.json");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var status =
                    JsonSerializer.Deserialize<StatusJson>(jsonString, options);

                if (status?.currentVelocityVersion != null)
                {
                    apiVersion = status.currentVelocityVersion.Trim();
                    CompareAndUpdateStatus();
                }
                else
                {
                    SetStatusUnknown("Could not read remote version");
                }
            }
            catch (Exception ex)
            {
                SetStatusUnknown($"Error fetching version: {ex.Message}");
            }
        }

        #endregion

        /// <summary>
        /// Reads version information from the local installation files.
        /// </summary>
        
        #region Local Version
        private void ReadLocalVersion()
        {
            try
            {
                string dir = Directory.GetCurrentDirectory();
                string versionFile =
                    System.IO.Path.Combine(dir, "bin", "current_version.txt");

                if (File.Exists(versionFile))
                {
                    localVersion = File.ReadAllText(versionFile).Trim();
                }
                else
                {
                    localVersion = "0.0.0";
                }
            }
            catch
            {
                localVersion = "0.0.0";
            }
        }

        #endregion

        /// <summary>
        /// Compares local and remote versions and updates the UI
        /// to reflect the current update status.
        /// </summary>
        
        #region Status Updates

        private void CompareAndUpdateStatus()
        {
            bool parsedLocal =
                Version.TryParse(localVersion, out Version? localVer);

            bool parsedApi =
                Version.TryParse(apiVersion, out Version? apiVer);

            Dispatcher.Invoke(() =>
            {
                if (UpToDateStatus == null || VelVersionLocal == null)
                    return;

                if (parsedLocal && parsedApi && localVer >= apiVer)
                {
                    UpToDateStatus.Text = "You are up to date!";
                    UpToDateStatus.Foreground = Brushes.White;

                    if (!string.Equals(
                            VelVersionLocal.Text.Trim(),
                            localVersion,
                            StringComparison.Ordinal))
                    {
                        VelVersionLocal.Text = localVersion;
                    }
                }
                else if (parsedLocal && parsedApi)
                {
                    UpToDateStatus.Text =
                        "You are not up to date! Restart Velocity to update";

                    UpToDateStatus.Foreground = Brushes.OrangeRed;
                }
                else
                {
                    bool same = string.Equals(
                        localVersion,
                        apiVersion,
                        StringComparison.OrdinalIgnoreCase);

                    UpToDateStatus.Text = same
                        ? "You are up to date!"
                        : "You are not up to date! Restart Velocity to update";

                    UpToDateStatus.Foreground = same
                        ? Brushes.LimeGreen
                        : Brushes.OrangeRed;

                    if (same &&
                        !string.Equals(
                            VelVersionLocal.Text.Trim(),
                            localVersion,
                            StringComparison.Ordinal))
                    {
                        VelVersionLocal.Text = localVersion;
                    }
                }
            });
        }

        /// <summary>
        /// Displays an unknown or error state when version
        /// information cannot be determined.
        /// </summary>
        private void SetStatusUnknown(string message)
        {
            Dispatcher.Invoke(() =>
            {
                UpToDateStatus.Text = message;
                UpToDateStatus.Foreground = Brushes.Gray;
            });
        }

        #endregion

        #endregion

        #region Client Management

        #region Client Models

        /// <summary>
        /// Represents a detected Roblox client and stores
        /// its process identifier and selection state.
        /// </summary>
        public class ClientEntry
        {
            public int Pid { get; set; }
            public bool IsSelected { get; set; }
        }

        #endregion

        #region Client Discovery

        /// <summary>
        /// Scans for running Roblox clients, removes closed clients,
        /// and creates entries for newly detected clients.
        /// </summary>
        private void ClientScanTimer_Tick(object? sender, EventArgs e)
        {
            var processes = Process.GetProcessesByName("RobloxPlayerBeta");

            foreach (var pid in _clientEntries.Keys.ToList())
            {
                if (!processes.Any(p => p.Id == pid))
                {
                    _clientEntries.Remove(pid);
                    RemoveClientControl(pid);
                }
            }

            foreach (var process in processes)
            {
                if (_clientEntries.ContainsKey(process.Id))
                    continue;

                var entry = new ClientEntry
                {
                    Pid = process.Id
                };

                _clientEntries[process.Id] = entry;

                AddClientControl(entry);
            }
        }

        /// <summary>
        /// Returns the list of target process IDs that should receive
        /// actions based on the current selection settings.
        /// </summary>
        
        private List<int> GetTargetPids()
        {
            if (_appSettings.AutomaticSelection)
            {
                return Process
                    .GetProcessesByName("RobloxPlayerBeta")
                    .Where(p => !p.HasExited)
                    .Select(p => p.Id)
                    .ToList();
            }

            return _clientEntries
                .Values
                .Where(e => e.IsSelected)
                .Select(e => e.Pid)
                .ToList();
        }

        #endregion

        #region Client UI

        /// <summary>
        /// Creates and displays a client entry inside the
        /// client picker panel.
        /// </summary>
        private void AddClientControl(ClientEntry entry)
        {
            if (ClientPickerPanel == null || entry == null)
                return;

            var border = new Border
            {
                BorderBrush = (Brush)(new BrushConverter().ConvertFrom("#191919") ?? Brushes.Gray),
                BorderThickness = new Thickness(0, 0, 0, 1.3),
                Margin = new Thickness(10, 0, 10, 10),
                Padding = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Center,
                Tag = entry.Pid,
            };

            var row = new Grid
            {
                VerticalAlignment = VerticalAlignment.Center
            };

            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var icon = new System.Windows.Controls.Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/Resource/Client/BloxRoblox.png")),
                Width = 32,
                Height = 32,
                Margin = new Thickness(0, 0, 16, 10),
                VerticalAlignment = VerticalAlignment.Center
            };

            System.Windows.Media.RenderOptions.SetBitmapScalingMode(
                icon,
                System.Windows.Media.BitmapScalingMode.HighQuality);

            Grid.SetColumn(icon, 0);

            var textStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center
            };

            var interFont =
                TryFindResource("Inter") as FontFamily ??
                new FontFamily("Segoe UI");

            var pidText = new TextBlock
            {
                Text = $"RobloxPlayerBeta_{entry.Pid}",
                Foreground = Brushes.White,
                FontFamily = interFont,
                FontWeight = FontWeights.SemiBold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 4)
            };

            var isAttached = velocityApi?.IsAttached(entry.Pid) ?? false;

            var statusText = new TextBlock
            {
                Text = isAttached ? "Attached" : "Not Attached",
                Foreground = isAttached ? Brushes.LightGreen : Brushes.Gray,
                FontFamily = interFont,
                FontWeight = FontWeights.Normal,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 10)
            };

            textStack.Children.Add(pidText);
            textStack.Children.Add(statusText);

            Grid.SetColumn(textStack, 1);

            var toggleStyle =
                TryFindResource("ToggleSwitchStyle") as Style;

            var toggle = new CheckBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                Style = toggleStyle,
                IsChecked = entry.IsSelected,
                Margin = new Thickness(20, 0, 0, 10)
            };

            toggle.Checked += (_, _) => entry.IsSelected = true;
            toggle.Unchecked += (_, _) => entry.IsSelected = false;

            Grid.SetColumn(toggle, 2);

            row.Children.Add(icon);
            row.Children.Add(textStack);
            row.Children.Add(toggle);

            border.Child = row;

            ClientPickerPanel.Children.Add(border);
        }

        /// <summary>
        /// Removes a client entry from the client picker panel
        /// when the associated process closes.
        /// </summary>
        
        private void RemoveClientControl(int pid)
        {
            if (ClientPickerPanel == null)
                return;

            var element = ClientPickerPanel.Children
                .OfType<Border>()
                .FirstOrDefault(b => (int?)b.Tag == pid);

            if (element != null)
                ClientPickerPanel.Children.Remove(element);
        }

        #endregion

        #endregion

        #region Global Announcements

        #region Announcement Models

        /// <summary>
        /// Represents a global announcement received from the
        /// announcement service and displayed to users.
        /// </summary>
        public class GlobalAnnouncement
        {
            public string? Title { get; set; } = "";
            public string? Body { get; set; } = "";
            public int Duration { get; set; } = 5;
        }

        #endregion

        #region Message Processing

        /// <summary>
        /// Decodes an incoming announcement message and displays
        /// it as an in-app notification.
        /// </summary>
        private void HandleSocketMessage(string base64)
        {
            try
            {
                var jsonBytes = Convert.FromBase64String(base64);
                var json = Encoding.UTF8.GetString(jsonBytes);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var annc = JsonSerializer.Deserialize<GlobalAnnouncement>(
                    json,
                    options);

                if (annc != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _ = ShowToastAnnouncement(annc);
                    });
                }
            }
            catch
            {
            }
        }

        #endregion

        #region Notifications

        /// <summary>
        /// Displays a toast notification for a received
        /// global announcement and plays a notification sound.
        /// </summary>
        private async Task ShowToastAnnouncement(GlobalAnnouncement annc)
        {
            try
            {
                var toast = new NotificationControl
                {
                    Margin = new Thickness(0, 0, 0, 10),
                    IsHitTestVisible = true
                };

                var player =
                    new SoundPlayer(@"C:\Windows\Media\Windows Notify System Generic.wav");

                player.Play();

                toast.MouseLeftButtonUp += (_, _) => { };

                if (App.Overlay != null)
                {
                    App.Overlay.ShowToast(toast);

                    string title = annc.Title ?? "Notification";
                    string body = annc.Body ?? "";

                    await toast.ShowAsync(
                        title,
                        body,
                        iconPath: "Resource/Vel/Vel_meteor-HQ.png",
                        durationMs: annc.Duration
                    );

                    App.Overlay.RemoveToast(toast);
                }
            }
            catch
            {
            }
        }

        #endregion

        #region WebSocket Connection

        /// <summary>
        /// Maintains a persistent connection to the announcement
        /// service and listens for incoming global notifications.
        /// Automatically reconnects if the connection is lost.
        /// </summary>
        private async Task ListenForAnnouncementsAsync()
        {
            while (true)
            {
                try
                {
                    await _webSocket.ConnectAsync(
                        new Uri("wss://realvelocity.xyz/socket/global_announcement"),
                        CancellationToken.None);

                    var buffer = new byte[4096];

                    while (_webSocket.State == WebSocketState.Open)
                    {
                        var result = await _webSocket.ReceiveAsync(
                            new ArraySegment<byte>(buffer),
                            CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _webSocket.CloseAsync(
                                WebSocketCloseStatus.NormalClosure,
                                "Closed by client",
                                CancellationToken.None);

                            break;
                        }

                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            var base64 =
                                Encoding.UTF8.GetString(buffer, 0, result.Count);

                            HandleSocketMessage(base64);
                        }
                    }
                }
                catch
                {
                }

                await Task.Delay(2000);

                _webSocket = new ClientWebSocket();
            }
        }

        #endregion

        #endregion

        #region Settings

        public class AppSettings
            {
                public bool AutoInject { get; set; }
                public bool SaveScripts { get; set; } = true;
                public bool UnlockFPS { get; set; }
                public bool TopMost { get; set; }
                public bool SaveWindowSize { get; set; }
                public bool IsNew { get; set; } = true;
                public string HideVelocityMode { get; set; } = "Never";
                public bool SaveOutputLocally { get; set; }
                public bool ExtendedLogging { get; set; }
                public bool StartMinimized { get; set; } = false;
                public bool EnableGifSupport { get; set; } = false;
                public bool FetchRobloxVersionAuto { get; set; } = false;
               public bool AutomaticSelection { get; set; } = true;
               public string AccentColor { get; set; } = "#2D7DFF";
               public bool AutoMaticBackUps { get; set; } = true;
                public bool EnableBackground { get; set; } = false;
                public string BgImgUrl { get; set; } = "";
                public double WindowWidth { get; set; } = 920;  
                public double WindowHeight { get; set; } = 620;

                public bool PreserveUiElementSize { get; set; } = false;
                public double SavedExplorerWidth { get; set; } = 310;   
                public double SavedTerminalHeight { get; set; } = 170;

               public double BackgroundImageTransparency { get; set; } = 0.3; 

        }

        private async Task LoadBackgroundImageAsync(string imageUrl)
        {
            if (!_appSettings.EnableBackground)
            {
                CleanupBackground();
                return;
            }

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                FadeOutBackground();
                CleanupBackground();
                return;
            }

            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();

                var bytes = await response.Content.ReadAsByteArrayAsync();

                _gifStream?.Dispose();
                _gifStream = null;
                AnimationBehavior.SetSourceStream(BackgroundImage, null);
                BackgroundImage.Source = null;

                if (_appSettings.EnableGifSupport)
                {
                    _gifStream = new MemoryStream(bytes);
                    _gifStream.Position = 0;
                    AnimationBehavior.SetSourceStream(BackgroundImage, _gifStream);
                }
                else
                {
                    using var ms = new MemoryStream(bytes);

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    BackgroundImage.Source = bitmap;
                }

                BackgroundImage.Visibility = Visibility.Visible;

                AnimateOpacity(BackgroundImage, ImageTransparencySlider.Value, 300);

                if (ImageTransparencySlider.Value <= 0.01)
                {
                    var announcement = new GlobalAnnouncement
                    {
                        Title = "Background Transparency",
                        Body = "The background image is loaded with 0 opacity. You might want to increase the transparency slider or disable the background since it can cause performance issues.",
                        Duration = 10000
                    };

                    _ = ShowToastAnnouncement(announcement);
                }
            }
            catch
            {
                CleanupBackground();
            }
        }

        private void ResetImageTransparencyButton_Click(object sender, RoutedEventArgs e)
        {
            ImageTransparencySlider.Value = 0.3;
            _appSettings.BackgroundImageTransparency = 0.3;
            SettingsManager.Save(_appSettings);
        }

        private void ImageTransparencySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!this.IsLoaded || BackgroundImage == null || _appSettings == null)
                return;

            AnimateOpacity(BackgroundImage, e.NewValue);

            _appSettings.BackgroundImageTransparency = e.NewValue;
            SettingsManager.Save(_appSettings);
        }

        private void CleanupBackground()
        {
            BackgroundImage.Visibility = Visibility.Collapsed;
            BackgroundImage.Source = null;
            AnimationBehavior.SetSourceStream(BackgroundImage, null);

            _gifStream?.Dispose();
            _gifStream = null;
        }

        private void FadeOutBackground()
        {
            var fadeOut = new DoubleAnimation(BackgroundImage.Opacity, 0, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (s, e) => BackgroundImage.Visibility = Visibility.Collapsed;
            BackgroundImage.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private async void ToggleEnableBackground_Checked(object sender, RoutedEventArgs e)
            {
                if (_isInitializing) return;

                _appSettings.EnableBackground = true;
                SettingsManager.Save(_appSettings);

                BackgroundUrlContainer.Visibility = Visibility.Visible;

            await LoadBackgroundImageAsync(_appSettings.BgImgUrl);

                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                var slide = new ThicknessAnimation(
                    new Thickness(10, -10, 10, 0),
                    new Thickness(10, 10, 10, 0),
                    TimeSpan.FromMilliseconds(150))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                var heightAnim = new DoubleAnimation(0, 41, TimeSpan.FromMilliseconds(300))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };

                BackgroundUrlContainer.BeginAnimation(Border.OpacityProperty, fadeIn);
                BackgroundUrlContainer.BeginAnimation(Border.MarginProperty, slide);
                BackgroundUrlContainer.BeginAnimation(Border.HeightProperty, heightAnim);
            }

            private void ToggleEnableBackground_Unchecked(object sender, RoutedEventArgs e)
            {
                if (_isInitializing) return;

                _appSettings.EnableBackground = false;
                SettingsManager.Save(_appSettings);

                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };
                var slide = new ThicknessAnimation(
                    new Thickness(10, 10, 10, 0),
                    new Thickness(10, -10, 10, 0),
                    TimeSpan.FromMilliseconds(150))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };

                var heightAnim = new DoubleAnimation(41, 0, TimeSpan.FromMilliseconds(300))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };

                heightAnim.Completed += (s, _) =>
                {
                    BackgroundUrlContainer.Visibility = Visibility.Collapsed;
                    BackgroundImage.Source = null;
                    BackgroundImage.Visibility = Visibility.Collapsed;
                };

                BackgroundUrlContainer.BeginAnimation(Border.OpacityProperty, fadeOut);
                BackgroundUrlContainer.BeginAnimation(Border.MarginProperty, slide);
                BackgroundUrlContainer.BeginAnimation(Border.HeightProperty, heightAnim);
            }


            private async void BackgroundUrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
            {
                if (_isInitializing) return;

                _appSettings.BgImgUrl = BackgroundUrlTextBox.Text;
                SettingsManager.Save(_appSettings);

                await LoadBackgroundImageAsync(_appSettings.BgImgUrl);
            }


            private void ToggleSaveWindowSize_CheckedChanged(object sender, RoutedEventArgs e)
            {
                if (_isInitializing) return;

                _appSettings.SaveWindowSize = ToggleSaveWindowSize.IsChecked == true;
                SettingsManager.Save(_appSettings);
            }

        private async void SetToggleStatesFromSettings()
            {
                if (!this.IsLoaded)
                    return;

                _isInitializing = true;

                ToggleAutoInject.IsChecked = _appSettings.AutoInject;
                ToggleSaveScripts.IsChecked = _appSettings.SaveScripts;
                ToggleAutoBackups.IsChecked = _appSettings.AutoMaticBackUps;
                ToggleAutoReport.IsChecked = _appSettings.UnlockFPS;
                ToggleTopMost.IsChecked = _appSettings.TopMost;
                SettingsToggleGifSupport.IsChecked = _appSettings.EnableGifSupport;

                ToggleHideVelocity.IsChecked = _appSettings.HideVelocityMode == "Always";

                DiscordRPCToggle.IsChecked = _appSettings.SaveOutputLocally;
                TogleExtLog.IsChecked = _appSettings.ExtendedLogging;
                ToggleStartMinimized.IsChecked = _appSettings.StartMinimized;
                ToggleAutoPick.IsChecked = _appSettings.AutomaticSelection;
                ToggleSaveWindowSize.IsChecked = _appSettings.SaveWindowSize;
                ToggleEnablePreserve.IsChecked = _appSettings.PreserveUiElementSize;
                ToggleEnableBackground.IsChecked = _appSettings.EnableBackground;

                BackgroundUrlTextBox.Text = _appSettings.BgImgUrl ?? "";

                BackgroundUrlContainer.Visibility = _appSettings.EnableBackground ? Visibility.Visible : Visibility.Collapsed;
                BackgroundUrlContainer.Opacity = _appSettings.EnableBackground ? 1 : 0;

                ImageTransparencySlider.Value = _appSettings.BackgroundImageTransparency;

                _isInitializing = false;

                await LoadTabsAsync();

        }


        private void ToggleHideVelocity_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded || _isInitializing)
                return;

            bool isChecked = ToggleHideVelocity.IsChecked == true;

            _appSettings.HideVelocityMode = isChecked ? "Always" : "Never";
            SettingsManager.Save(_appSettings);

            if (this.IsLoaded)
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                SetExcludeFromCapture(hwnd, isChecked);
            }

            if (_appSettings.ExtendedLogging)
                WriteToTerminal($"Hide Velocity from capture toggled: {_appSettings.HideVelocityMode}");
        }


        private void ToggleAutoPick_Checked(object sender, RoutedEventArgs e)
        {
            _appSettings.AutomaticSelection = true;
            SettingsManager.Save(_appSettings);

            SettingsNotice.Visibility = Visibility.Visible;
            ClientPickerPanel.Visibility = Visibility.Collapsed;

            _clientScanTimer.Stop();
        }

        private async void ToggleGifSupport_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded || _isInitializing)
                return;

            _appSettings.EnableGifSupport = SettingsToggleGifSupport.IsChecked == true;
            SettingsManager.Save(_appSettings);

            if (_appSettings.ExtendedLogging)
                WriteToTerminal($"GIF support toggled: {_appSettings.EnableGifSupport}");

            if (_appSettings.EnableBackground)
            {
                await LoadBackgroundImageAsync(_appSettings.BgImgUrl);
            }
        }

        private void ToggleAutoPick_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing)
                return;

            _appSettings.AutomaticSelection = false;
            SettingsManager.Save(_appSettings);

            SettingsNotice.Visibility = Visibility.Collapsed;
            ClientPickerPanel.Visibility = Visibility.Visible;

            _clientScanTimer.Start();

            ClientScanTimer_Tick(null, EventArgs.Empty);
        }

        private void Toggle_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded || _isInitializing)
                return;

            _appSettings.AutoInject = ToggleAutoInject.IsChecked == true;
            _appSettings.SaveScripts = ToggleSaveScripts.IsChecked == true;
            _appSettings.UnlockFPS = ToggleAutoReport.IsChecked == true;

            _appSettings.TopMost = ToggleTopMost.IsChecked == true;
            this.Topmost = _appSettings.TopMost;

            _appSettings.PreserveUiElementSize = ToggleEnablePreserve.IsChecked == true;

            _appSettings.SaveOutputLocally = DiscordRPCToggle.IsChecked == true;
            _appSettings.AutomaticSelection = ToggleAutoPick.IsChecked == true; 
            _appSettings.ExtendedLogging = TogleExtLog.IsChecked == true;
            _appSettings.StartMinimized = ToggleStartMinimized.IsChecked == true;

            SettingsManager.Save(_appSettings);

            if (ToggleAutoInject.IsChecked == true)
            {
                StartAutoInjectWatcher();
            }
            else
            {
                StopAutoInjectWatcher();
            }
        }

        public static class SettingsManager
        {
            private static readonly string ConfigPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Velocity Ui", "config.json");

            public static AppSettings Load()
            {
                try
                {
                    if (!File.Exists(ConfigPath))
                        return new AppSettings();

                    string json = File.ReadAllText(ConfigPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                catch
                {
                    return new AppSettings();
                }
            }

            public static void Save(AppSettings settings)
            {
                try
                {
                    string? dir = System.IO.Path.GetDirectoryName(ConfigPath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    string? json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(ConfigPath, json);
                }
                catch
                {
                }
            }
        }

        #endregion

        #region Changelogs Fetcher

        /// <summary>
        /// Represents the remote status JSON returned by the Velocity API,
        /// including current version and changelog text.
        /// </summary>
        private class StatusJson
        {
            public string? currentVelocityVersion { get; set; }
            public string? changeLog { get; set; }
        }

        /// <summary>
        /// Fetches the latest changelog from the remote API and renders it into the UI.
        /// </summary>
        async Task LoadChangelogAsync()
        {
            try
            {
                using var httpClient = new HttpClient();
                string jsonString = await httpClient.GetStringAsync("https://realvelocity.xyz/status.json");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var status = JsonSerializer.Deserialize<StatusJson>(jsonString, options);

                if (status != null)
                {
                    VersionsContainer.Children.Clear();

                    var versionBlock = CreateVersionBlock($"Version {status.currentVelocityVersion}");

                    string[] lines = (status.changeLog ?? string.Empty)
                        .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    var entriesPanel = versionBlock.Children[1] as StackPanel;

                    if (entriesPanel != null)
                    {
                        foreach (var line in lines)
                        {
                            AddChangelogEntry(entriesPanel, line.Trim());
                        }

                        VersionsContainer.Children.Add(versionBlock);
                    }

                    LastFetchedTextBlock.Text =
                        $"Last fetched at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                }
            }
            catch (HttpRequestException ex)
            {
                _ = ShowMessageBoxInGridAsync(
                    "Changelog Error",
                    $"Failed to fetch changelog.\n\n{ex.Message}",
                    primaryButtonText: "Ok",
                    iconPath: "Resource/Msg/BoxType_Error.png"
                );
            }
        }

        #endregion

        #region Changelog UI Builders

        /// <summary>
        /// Creates a UI block for a single version entry.
        /// </summary>
        StackPanel CreateVersionBlock(string versionTitle)
        {
            var versionStack = new StackPanel()
            {
                Margin = new Thickness(0, 20, 0, 25)
            };

            var versionText = new TextBlock()
            {
                Text = versionTitle,
                FontSize = 20,
                Opacity = 0.8,
                Foreground = Brushes.White,
                Margin = new Thickness(10, 0, 0, 10)
            };

            var entriesPanel = new StackPanel()
            {
                Margin = new Thickness(15, 0, 0, 0)
            };

            versionStack.Children.Add(versionText);
            versionStack.Children.Add(entriesPanel);

            return versionStack;
        }

        /// <summary>
        /// Adds a formatted changelog entry to a version section.
        /// </summary>
        void AddChangelogEntry(StackPanel entriesPanel, string entryText)
        {
            var entry = new TextBlock()
            {
                Text = entryText.StartsWith("[+]") || entryText.StartsWith("[!]")
                    ? entryText
                    : "• " + entryText,

                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 10, 0, 0),
                FontFamily = new FontFamily("JetBrains Mono")
            };

            entriesPanel.Children.Add(entry);
        }

        #endregion

        #region UI Buttons

        /// <summary>
        /// Reloads the changelog from the server.
        /// </summary>
        private async void ReloadExplorer_Click(object sender, RoutedEventArgs e)
        {
            await LoadChangelogAsync();
        }

        /// <summary>
        /// Opens the official Discord invite link.
        /// </summary>
        private void DiscordButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://discord.com/invite/velocityide");
        }

        /// <summary>
        /// Opens the official website.
        /// </summary>
        private void WebsiteButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://realvelocity.xyz");
        }

        /// <summary>
        /// Opens a URL in the system browser.
        /// </summary>
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                });
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to open URL: {ex.Message}");
            }
        }

        #endregion

        #region Window System

        /// <summary>
        /// Updates splash screen text with optional delay for loading UX.
        /// </summary>
        private async Task SetSplashStatusAsync(string text, int delayMs = 150)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                StatusBlockSplash.Text = text;
            });

            if (delayMs > 0)
                await Task.Delay(delayMs);
        }

        /// <summary>
        /// Sets splash image depending on time of day (morning, afternoon, evening, night).
        /// </summary>
        private void SetTimeBasedSplash()
        {
            var hour = DateTime.Now.Hour;

            string splash =
                hour >= 5 && hour < 12 ? "Moring.png" :
                hour >= 12 && hour < 18 ? "Afternoon.png" :
                hour >= 18 && hour < 21 ? "Evening.png" :
                hour >= 22 && hour < 4 ? "Night.png" :
                "Night.png";

            SplashImage.Source = new BitmapImage(new Uri(
                $"pack://application:,,,/Resource/Splash/{splash}",
                UriKind.Absolute));
        }

        #endregion

        #region Loaded Initialization

        /// <summary>
        /// Ensures required WebView2 dependency exists.
        /// </summary>
        public static void CheckWebView2Dll()
        {
            string dllName = "WebView2Loader.dll";
            string currentDir = AppContext.BaseDirectory;
            string dllPath = System.IO.Path.Combine(currentDir, dllName);

            if (!File.Exists(dllPath))
            {
                // Removed legacy warning (no longer required)
            }
        }

        /// <summary>
        /// Main application startup logic.
        /// Initializes services, UI, settings, API communication, and background systems.
        /// </summary>
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await VelocityLite.Controls.DefenderControl.CheckAndWarnAsync();
            CheckWebView2Dll();
            StartVersionCheckTimer();
            velocityApi.StartCommunication();

            if (_appSettings.ExtendedLogging)
                WriteToTerminal("Started API communications");

            #region Toggle Events Wiring

            SettingsToggleGifSupport.Checked += ToggleGifSupport_CheckedChanged;
            SettingsToggleGifSupport.Unchecked += ToggleGifSupport_CheckedChanged;

            ToggleAutoInject.Checked += Toggle_CheckedChanged;
            ToggleAutoInject.Unchecked += Toggle_CheckedChanged;

            ToggleHideVelocity.Checked += ToggleHideVelocity_CheckedChanged;
            ToggleHideVelocity.Unchecked += ToggleHideVelocity_CheckedChanged;

            ToggleSaveScripts.Checked += Toggle_CheckedChanged;
            ToggleSaveScripts.Unchecked += Toggle_CheckedChanged;

            ToggleAutoReport.Checked += Toggle_CheckedChanged;
            ToggleAutoReport.Unchecked += Toggle_CheckedChanged;

            ToggleTopMost.Checked += Toggle_CheckedChanged;
            ToggleTopMost.Unchecked += Toggle_CheckedChanged;

            ToggleEnablePreserve.Checked += Toggle_CheckedChanged;
            ToggleEnablePreserve.Unchecked += Toggle_CheckedChanged;

            DiscordRPCToggle.Checked += Toggle_CheckedChanged;
            DiscordRPCToggle.Unchecked += Toggle_CheckedChanged;

            TogleExtLog.Checked += Toggle_CheckedChanged;
            TogleExtLog.Unchecked += Toggle_CheckedChanged;

            ToggleEnableBackground.Checked += ToggleEnableBackground_Checked;
            ToggleEnableBackground.Unchecked += ToggleEnableBackground_Unchecked;

            ToggleSaveWindowSize.Checked += ToggleSaveWindowSize_CheckedChanged;
            ToggleSaveWindowSize.Unchecked += ToggleSaveWindowSize_CheckedChanged;

            ToggleAutoPick.Checked += ToggleAutoPick_Checked;
            ToggleAutoPick.Unchecked += ToggleAutoPick_Unchecked;

            #endregion

            await SetSplashStatusAsync("Fetching Version..");
            await CheckVersionAsync();

            await SetSplashStatusAsync("Fetching Changelogs..");
            await LoadChangelogAsync();

            await SetSplashStatusAsync("Preparing Editor..");

            try
            {
                await EnsureMonacoEditorExtracted();

                if (_appSettings.ExtendedLogging)
                    WriteToTerminal("Monaco Editor ensured/extracted.");
            }
            catch (Exception ex)
            {
                _ = ShowMessageBoxInGridAsync(
                    "Startup Error",
                    $"Failed to open link: {ex.Message}",
                    primaryButtonText: "Ok",
                    iconPath: "Resource/Msg/BoxType_Error.png"
                );
            }

            await SetSplashStatusAsync("Restoring Interface..");
            await SelectItem(HomeButton, BlueBarHome, BlueBarHomeFullyFilled);

            var hwnd = new WindowInteropHelper(this).Handle;
            SetExcludeFromCapture(hwnd, _appSettings.HideVelocityMode == "Always");

            LoadAccentColorOnStartup();
            SetToggleStatesFromSettings();

            if (_appSettings.ExtendedLogging)
                WriteToTerminal("Set toggle states from settings.");

            if (_appSettings.SaveWindowSize && this.WindowState == WindowState.Normal)
            {
                this.Width = _appSettings.WindowWidth;
                this.Height = _appSettings.WindowHeight;
            }

            if (!_appSettings.AutomaticSelection)
            {
                SettingsNotice.Visibility = Visibility.Collapsed;
                ClientPickerPanel.Visibility = Visibility.Visible;
                _clientScanTimer.Start();
                ClientScanTimer_Tick(null, EventArgs.Empty);
            }
            else
            {
                SettingsNotice.Visibility = Visibility.Visible;
                ClientPickerPanel.Visibility = Visibility.Collapsed;
            }

            if (_appSettings.EnableBackground)
                await LoadBackgroundImageAsync(_appSettings.BgImgUrl);
            else
                BackgroundImage.Visibility = Visibility.Collapsed;

            if (_appSettings.PreserveUiElementSize)
            {
                PrimaryHost.ColumnDefinitions[0].Width =
                    new GridLength(_appSettings.SavedExplorerWidth, GridUnitType.Pixel);

                PrimaryHostInside.RowDefinitions[2].Height =
                    new GridLength(_appSettings.SavedTerminalHeight, GridUnitType.Pixel);
            }

            this.Topmost = _appSettings.TopMost;

            if (_appSettings.SaveOutputLocally)
            {
                string logsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                Directory.CreateDirectory(logsFolder);
                _logFilePath = Path.Combine(logsFolder, $"{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            }

            if (_appSettings.StartMinimized)
                this.WindowState = WindowState.Minimized;

            string explorerPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");
            LoadFiles(explorerPath);

            await LoadSettings();
            GeneralBorder.Visibility = Visibility.Visible;

            TabCtrl.AddTabClicked += (s, e) =>
            {
                ++this.TabCounter;
                CreateEditorInstance($"New Tab {this.TabCounter}", "", "pack://application:,,,/Resource/Exp/Script-File_Alt.png");
            };

            if (_appSettings.AutoInject)
                StartAutoInjectWatcher();

            await Task.Delay(1000);

            if (_appSettings.IsNew)
            {
                WelcomeMessage.Visibility = Visibility.Visible;
                WelcomeBackDrop.Opacity = 0;
                WelcomeBorderMessageHost.Opacity = 0;

                var openSb = (Storyboard)FindResource("OpenWelcomeStoryboard");
                openSb.Begin();
            }
            else
            {
                WelcomeMessage.Visibility = Visibility.Collapsed;
            }

            LoadVersionText();
            AnimateOpacity(TitleRightSideSplashScreen, 0);

            await SetSplashStatusAsync("Welcome to Velocity!");
            await AnimatePageOut(SplashScreen, 300);

            _ = ListenForAnnouncementsAsync();
        }

        #endregion

        #region Process Cleanup

        /// <summary>
        /// Safely kills Decompiler process if it is running.
        /// </summary>
        async Task KillDecompilerIfRunning()
        {
            foreach (var proc in Process.GetProcessesByName("Decompiler"))
            {
                try
                {
                    proc.Kill(true);
                    proc.WaitForExit(2000);
                }
                catch (Win32Exception ex) when (ex.NativeErrorCode == 5)
                {
                    MessageBox.Show(
                        $"Velocity can't access Decompiler.exe.\n\n{ex.Message}",
                        "Access Denied",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to terminate Decompiler.exe.\n\n{ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return;
                }
            }
        }

        #endregion

        #region Custom MessageBox System

        /// <summary>
        /// Displays a custom in-app message box or fallback system dialog.
        /// </summary>
        private static async Task<VelocityLite.Controls.DialogResult> ShowMessageBoxInGridAsync(
            string title,
            string message,
            string primaryButtonText = "Ok",
            string? secondaryButtonText = null,
            string? iconPath = null)
        {
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            var parentGrid = mainWindow?.MessageBoxSpawnParent;

            if (parentGrid == null)
            {
                MessageBoxButton buttons =
                    secondaryButtonText == null ? MessageBoxButton.OK : MessageBoxButton.YesNo;

                var msgResult = MessageBox.Show(message, title, buttons, MessageBoxImage.Information);

                return msgResult switch
                {
                    MessageBoxResult.Yes => VelocityLite.Controls.DialogResult.Yes,
                    MessageBoxResult.No => VelocityLite.Controls.DialogResult.No,
                    _ => VelocityLite.Controls.DialogResult.None
                };
            }

            var dialog = new VelocityLite.Controls.MessageBoxStandard();
            Grid.SetZIndex(dialog, 999);
            parentGrid.Children.Add(dialog);

            var result = await dialog.ShowDialogAsync(
                title,
                message,
                primaryButtonText,
                secondaryButtonText,
                iconPath
            );

            if (parentGrid.Children.Contains(dialog))
                parentGrid.Children.Remove(dialog);

            return result;
        }

        #endregion

        #region Window Closing

        /// <summary>
        /// Handles safe shutdown of application and saves persistent settings.
        /// </summary>
        private async void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            try
            {
                e.Cancel = true;

                if (_appSettings.SaveWindowSize && this.WindowState == WindowState.Normal)
                {
                    _appSettings.WindowWidth = this.Width;
                    _appSettings.WindowHeight = this.Height;
                }

                if (_appSettings.PreserveUiElementSize)
                {
                    _appSettings.SavedExplorerWidth = ExplorerBorder.ActualWidth;
                    _appSettings.SavedTerminalHeight = BottomHostTerminal.ActualHeight;
                }

                SettingsManager.Save(_appSettings);

                StopAutoInjectWatcher();
                WriteToTerminal("Stopping Velocity API communication...");
                velocityApi.StopCommunication();

                await KillDecompilerIfRunning();
                await Task.Delay(200);

                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while closing: {ex.Message}", "Shutdown Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                Environment.Exit(0);
            }
        }

        #endregion

        #region Window Focus Effects

        private readonly SolidColorBrush _titlebarBgBrush =
            new SolidColorBrush(Color.FromRgb(8, 8, 8));

        private readonly SolidColorBrush _titlebarBorderBrush =
            new SolidColorBrush(Color.FromRgb(25, 25, 25));

        private void AnimateBrushColor(SolidColorBrush brush, Color to, int durationMs = 220)
        {
            var animation = new ColorAnimation
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        private void Window_Deactivated(object? sender, EventArgs e)
        {
            AnimateBrushColor(_titlebarBgBrush, Color.FromRgb(28, 28, 28), 580);
            AnimateBrushColor(_titlebarBorderBrush, Color.FromRgb(32, 32, 32), 580);
            AnimateOpacity(Tilebarcontent, 0.5, 260);
        }

        private void Window_Activated(object? sender, EventArgs e)
        {
            AnimateBrushColor(_titlebarBgBrush, Color.FromRgb(8, 8, 8), 580);
            AnimateBrushColor(_titlebarBorderBrush, Color.FromRgb(25, 25, 25), 580);
            AnimateOpacity(Tilebarcontent, 1, 250);
        }

        #endregion

        #region Sidebar
        /// <summary>
        /// Handles sidebar navigation, animations, and page switching between
        /// Home, Scripts, Changelog, and Cloud views.
        /// </summary>

        #region Buttons

        /// <summary>
        /// Switches UI to the Changelog page and animates all other hosts out.
        /// </summary>
        private async Task ShowChangeLogHost(int durationMs = 200)
        {
            if (PrimaryHost.Visibility == Visibility.Visible)
                await AnimatePageOut(PrimaryHost, durationMs);

            if (ScriptHubHost.Visibility == Visibility.Visible)
                await AnimatePageOut(ScriptHubHost, durationMs);

            if (ScriptsHost.Visibility == Visibility.Visible)
                await AnimatePageOut(ScriptsHost, durationMs);

            AnimatePageIn(ChangeLogHost, durationMs + 50);
        }

        /// <summary>
        /// Switches UI to the Scripts page and hides all other pages.
        /// </summary>
        private async Task ShowScriptsHost(int durationMs = 200)
        {
            if (PrimaryHost.Visibility == Visibility.Visible)
                await AnimatePageOut(PrimaryHost, durationMs);

            if (ChangeLogHost.Visibility == Visibility.Visible)
                await AnimatePageOut(ChangeLogHost, durationMs);

            if (ScriptHubHost.Visibility == Visibility.Visible)
                await AnimatePageOut(ScriptHubHost, durationMs);

            AnimatePageIn(ScriptsHost, durationMs + 50);
        }

        /// <summary>
        /// Switches UI back to the Home/Primary page and hides all other pages.
        /// </summary>
        private async Task ShowPrimaryHost(int durationMs = 200)
        {
            if (ChangeLogHost.Visibility == Visibility.Visible)
                await AnimatePageOut(ChangeLogHost, durationMs);

            if (ScriptsHost.Visibility == Visibility.Visible)
                await AnimatePageOut(ScriptsHost, durationMs);

            if (ScriptHubHost.Visibility == Visibility.Visible)
                await AnimatePageOut(ScriptHubHost, durationMs);

            AnimatePageIn(PrimaryHost, durationMs + 50);
        }

        #endregion

        #region Home Button Events

        /// <summary>
        /// Handles click on Home button and switches to Home page.
        /// </summary>
        private async void HomeButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            await SelectItem(HomeButton, BlueBarHome, BlueBarHomeFullyFilled);
            await ShowPrimaryHost();
        }

        /// <summary>
        /// Hover enter animation for Home button.
        /// </summary>
        private void HomeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Sidebar_MouseEnter(HomeButton, BlueBarHome, BlueBarHomeFullyFilled);
        }

        /// <summary>
        /// Hover leave animation for Home button.
        /// </summary>
        private void HomeButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Sidebar_MouseLeave(HomeButton, BlueBarHome, BlueBarHomeFullyFilled);
        }

        #endregion

        #region Script Button Events

        private void ScriptButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Sidebar_MouseEnter(ScriptButton, BlueBarScripts, BlueBarScriptsFullyFilled);
        }

        private void ScriptButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Sidebar_MouseLeave(ScriptButton, BlueBarScripts, BlueBarScriptsFullyFilled);
        }

        /// <summary>
        /// Switches to Scripts page when clicked.
        /// </summary>
        private async void ScriptButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            await SelectItem(ScriptButton, BlueBarScripts, BlueBarScriptsFullyFilled);
            await ShowScriptsHost();
        }

        #endregion

        #region Changelog Button Events

        private void ChangeLogsNewsButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Sidebar_MouseEnter(ChangeLogsNewsButton, BlueBarNewsChangeLogs, BlueBarNewsChangeLogsFullyFilled);
        }

        private void ChangeLogsNewsButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Sidebar_MouseLeave(ChangeLogsNewsButton, BlueBarNewsChangeLogs, BlueBarNewsChangeLogsFullyFilled);
        }

        /// <summary>
        /// Opens Changelog page.
        /// </summary>
        private async void ChangeLogsNewsButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            await SelectItem(ChangeLogsNewsButton, BlueBarNewsChangeLogs, BlueBarNewsChangeLogsFullyFilled);
            await ShowChangeLogHost();
        }

        #endregion

        #region Cloud Button Events

        private void CloudButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Sidebar_MouseEnter(CloudButton, BlueBarCloud, BlueBarCloudFullyFilled);
        }

        private void CloudButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Sidebar_MouseLeave(CloudButton, BlueBarCloud, BlueBarCloudFullyFilled);
        }

        /// <summary>
        /// Opens ScriptHub/Cloud page.
        /// </summary>
        private async void CloudButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            await SelectItem(CloudButton, BlueBarCloud, BlueBarCloudFullyFilled);

            if (PrimaryHost.Visibility == Visibility.Visible)
                await AnimatePageOut(PrimaryHost);

            if (ChangeLogHost.Visibility == Visibility.Visible)
                await AnimatePageOut(ChangeLogHost);

            if (ScriptsHost.Visibility == Visibility.Visible)
                await AnimatePageOut(ScriptsHost);

            AnimatePageIn(ScriptHubHost);
        }

        #endregion

        #region Sidebar Hover Animations

        /// <summary>
        /// Handles hover enter animation for sidebar items.
        /// </summary>
        private void Sidebar_MouseEnter(Border button, Border animatedBar, Border fullBar)
        {
            if (_selectedButton == button || fullBar.Visibility == Visibility.Visible)
                return;

            AnimateBlueBar(animatedBar, 0.6, 0.6, 160);
        }

        /// <summary>
        /// Handles hover leave animation for sidebar items.
        /// </summary>
        private void Sidebar_MouseLeave(Border button, Border animatedBar, Border fullBar)
        {
            if (_selectedButton == button || fullBar.Visibility == Visibility.Visible)
                return;

            AnimateBlueBar(animatedBar, 0, 0, 160);
        }

        /// <summary>
        /// Animates sidebar blue highlight bar scaling and opacity.
        /// </summary>
        private void AnimateBlueBar(Border bar, double scaleY, double opacity, int ms = 380)
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseInOut };

            var scaleAnim = new DoubleAnimation
            {
                To = scaleY,
                Duration = TimeSpan.FromMilliseconds(ms),
                EasingFunction = ease,
                FillBehavior = FillBehavior.Stop
            };

            var opacityAnim = new DoubleAnimation
            {
                To = opacity,
                Duration = TimeSpan.FromMilliseconds(ms),
                EasingFunction = ease,
                FillBehavior = FillBehavior.Stop
            };

            scaleAnim.Completed += (_, __) =>
            {
                ((ScaleTransform)bar.RenderTransform).ScaleY = scaleY;
                bar.Opacity = opacity;
            };

            ((ScaleTransform)bar.RenderTransform)
                .BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);

            bar.BeginAnimation(Border.OpacityProperty, opacityAnim);
        }

        #endregion

        #region Sidebar Selection Logic

        /// <summary>
        /// Sets the selected sidebar item, updates UI highlight, and animates transitions.
        /// </summary>
        private async Task SelectItem(Border button, Border animatedBar, Border fullBar)
        {
            if (_selectedButton != null)
                _selectedButton.Background = Brushes.Transparent;

            BlueBarHomeFullyFilled.Visibility = Visibility.Collapsed;
            BlueBarNewsChangeLogsFullyFilled.Visibility = Visibility.Collapsed;
            BlueBarCloudFullyFilled.Visibility = Visibility.Collapsed;
            BlueBarScriptsFullyFilled.Visibility = Visibility.Collapsed;

            AnimateBlueBar(BlueBarHome, 0, 0, 140);
            AnimateBlueBar(BlueBarNewsChangeLogs, 0, 0, 140);
            AnimateBlueBar(BlueBarCloud, 0, 0, 140);
            AnimateBlueBar(BlueBarScripts, 0, 0, 140);

            _selectedButton = button;
            button.Background = new SolidColorBrush(Color.FromRgb(18, 18, 18));

            AnimateBlueBar(animatedBar, 1, 1);

            await Task.Delay(200);
            fullBar.Visibility = Visibility.Visible;
        }

        #endregion

        #endregion

        #region Titlebar
        /// <summary>
        /// Handles custom window titlebar behavior including drag, minimize, maximize, and close actions.
        /// This replaces default Windows chrome behavior with a custom UI implementation.
        /// </summary>

        #region Chrome functions

        /// <summary>
        /// Handles dragging the window, and double-click maximize/restore behavior.
        /// </summary>
        private void ChromeDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (e.ClickCount == 2)
            {
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;

                return;
            }

            DragMove();
        }

        /// <summary>
        /// Minimizes the application window.
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Toggles between maximized and normal window state.
        /// </summary>
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// Closes the application after attempting to save open tabs.
        /// </summary>
        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await SaveTabsAsync();
            }
            catch (Exception)
            {
            }

            Application.Current.Shutdown();
        }

        #endregion

        #endregion

        #region Auto Inject
        
        /// <summary>
        /// Handles automatic injection monitoring for selected or active Roblox processes.
        /// Continuously scans target PIDs and attempts to attach Velocity when conditions are met.
        /// </summary>

        #region Watcher Control

        /// <summary>
        /// Starts the background watcher that monitors Roblox processes and auto-injects when ready.
        /// Prevents multiple watcher instances from running simultaneously.
        /// </summary>
        private void StartAutoInjectWatcher()
        {
            if (_isAutoInjectRunning)
                return;

            _autoInjectCts = new CancellationTokenSource();
            CancellationToken token = _autoInjectCts.Token;
            _isAutoInjectRunning = true;

            Task.Run(async () =>
            {
                WriteToTerminal("Watcher Started");

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        var targetPids = GetTargetPids();

                        if (targetPids.Count == 0)
                        {
                            await Task.Delay(2000, token);
                            continue;
                        }

                        foreach (int pid in targetPids)
                        {
                            if (token.IsCancellationRequested)
                                return;

                            Process? proc;
                            try
                            {
                                proc = Process.GetProcessById(pid);
                            }
                            catch
                            {
                                continue;
                            }

                            if (proc.HasExited)
                            {
                                Application.Current.Dispatcher.Invoke(() => UpdateClientStatus(pid, "Not Attached"));
                                continue;
                            }

                            if (velocityApi.IsAttached(pid))
                            {
                                Application.Current.Dispatcher.Invoke(() => UpdateClientStatus(pid, "Attached"));
                                continue;
                            }

                            int attempts = 0;
                            bool printedWaiting = false;

                            while (proc.MainWindowHandle == IntPtr.Zero && attempts < 30)
                            {
                                if (token.IsCancellationRequested)
                                    return;

                                if (!printedWaiting)
                                {
                                    WriteToTerminal("Waiting for HWND..");
                                    Application.Current.Dispatcher.Invoke(() => UpdateClientStatus(pid, "Waiting for HWND.."));
                                    printedWaiting = true;
                                }

                                await Task.Delay(500, token);
                                attempts++;
                                proc.Refresh();
                            }

                            if (proc.MainWindowHandle != IntPtr.Zero)
                            {
                                WriteToTerminal($"Found HWND: {proc.MainWindowHandle}");
                                WriteToTerminal($"Injecting into {pid}...");

                                Application.Current.Dispatcher.Invoke(() => UpdateClientStatus(pid, "Injecting"));

                                try
                                {
                                    await velocityApi.Attach(pid);

                                    WriteToTerminal($"Injected into {pid}");

                                    Application.Current.Dispatcher.Invoke(() => UpdateClientStatus(pid, "Attached"));
                                }
                                catch (Exception ex)
                                {
                                    WriteToTerminal($"Failed to inject into {pid}: {ex.Message}");

                                    Application.Current.Dispatcher.Invoke(() => UpdateClientStatus(pid, "Not Attached"));
                                }
                            }
                        }

                        await Task.Delay(2000, token);
                    }
                }
                catch (TaskCanceledException)
                {
                }
                finally
                {
                    _isAutoInjectRunning = false;
                    WriteToTerminal("Watcher stopped.");
                }
            }, token);
        }

        #endregion

        #region UI Status Updates

        /// <summary>
        /// Updates the UI status text for a specific client PID in the client picker list.
        /// </summary>
        private void UpdateClientStatus(int pid, string status)
        {
            Dispatcher.Invoke(() =>
            {
                var border = ClientPickerPanel.Children
                    .OfType<Border>()
                    .FirstOrDefault(b => b.Tag is int tag && tag == pid);

                if (border?.Child is not Grid row)
                    return;

                var textStack = row.Children
                    .OfType<StackPanel>()
                    .FirstOrDefault();

                if (textStack?.Children.Count < 2)
                    return;

                if (textStack?.Children[1] is TextBlock statusText)
                {
                    statusText.Text = status;

                    statusText.Foreground =
                        status == "Attached" ? Brushes.LimeGreen :
                        status == "Injecting" || status.StartsWith("Waiting for HWND") ? Brushes.Orange :
                        Brushes.Gray;
                }
            });
        }

        #endregion

        #region Watcher Stop

        /// <summary>
        /// Stops the auto-inject watcher and cleans up cancellation resources.
        /// </summary>
        private void StopAutoInjectWatcher()
        {
            if (_autoInjectCts != null)
            {
                _autoInjectCts.Cancel();
                _autoInjectCts.Dispose();
                _autoInjectCts = null;
            }
        }

        #endregion

        #endregion

        #region Elements

        #region Settings Menu

        #region Buttons and switches

        private void CloseTheme_Click(object sender, RoutedEventArgs e)
        {
            var sb = FindResource("CloseThemeStoryboard") as Storyboard;
            if (sb == null)
                return;

            EventHandler? handler = null;

            handler = (s, _) =>
            {
                ThemeHost.Visibility = Visibility.Collapsed;

                ThemeHostPrimary.Opacity = 1;
                ThemeBackdrop.Opacity = 0.6;
                ThemeScale.ScaleX = 1;
                ThemeScale.ScaleY = 1;

                sb.Completed -= handler;
            };

            sb.Completed += handler;
            sb.Begin();
        }

        private void OpenTheme()
        {
            ThemeHost.Visibility = Visibility.Visible;

            var sb = (Storyboard)FindResource("OpenThemeStoryboard");
            sb.Begin();
        }

        private void OpenTheme_Click(object sender, RoutedEventArgs e)
        {
            if (ThemeHost.Visibility == Visibility.Visible)
                return;

            ThemeHostPrimary.Opacity = 0;
            ThemeBackdrop.Opacity = 0;
            ThemeScale.ScaleX = 0.94;
            ThemeScale.ScaleY = 0.94;

            OpenTheme();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            var closeSb = (Storyboard)TryFindResource("CloseWelcomeStoryboard");
            if (closeSb == null)
            {
                MessageBox.Show("Failed to find resource 'CloseWelcomeStoryboard'.");
                return;
            }

            EventHandler? handler = null;
            handler = (s, args) =>
            {
                WelcomeMessage.Visibility = Visibility.Collapsed;

                _appSettings.IsNew = false;
                SettingsManager.Save(_appSettings);

                closeSb.Completed -= handler; 
            };

            closeSb.Completed += handler;
            closeSb.Begin();
        }

        private void CloseSettings_Click(object sender, RoutedEventArgs e)
        {
            var sb = (Storyboard)FindResource("CloseSettingsStoryboard");

            sb.Completed += (_, _) =>
            {
                SettingsHost.Visibility = Visibility.Collapsed;

                SettingsHostBorder.Opacity = 1;
                SettingsBackDrop.Opacity = 0.6;
                SettingsScale.ScaleX = 1;
                SettingsScale.ScaleY = 1;
            };

            sb.Begin();
        }

        private void OpenSettings()
        {
            SettingsHost.Visibility = Visibility.Visible;

            var sb = (Storyboard)FindResource("OpenSettingsStoryboard");
            sb.Begin();
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsHost.Visibility == Visibility.Visible)
                return;

            SettingsHostBorder.Opacity = 0;
            SettingsBackDrop.Opacity = 0;
            SettingsScale.ScaleX = 0.94;
            SettingsScale.ScaleY = 0.94;

            OpenSettings();
        }

        public class SettingItem
        {
            public string Name { get; set; }
            public string Icon { get; set; }
            public string Category { get; set; }

            public SettingItem(string name, string category, string icon)
            {
                Name = name;
                Category = category;
                Icon = icon;
            }
        }

        private async Task LoadSettings()
        {
            var items = new List<SettingItem>();

            var sections = new Dictionary<string, string[]>
    {
        { "GENERAL", new[] { "General", "Interface" } },
        { "DEBUG", new[] { "Logs", "Help & Support" } }
    };

            string LoggingIcon = @"Resource\Settings\Settings_Logging.png";
            string WindowIcon = @"Resource\Settings\Settings_Window.png";
            string SupportIcon = @"Resource\Settings\Settings_Support.png";
            string ModuleIcon = @"Resource\Settings\Settings_Module.png";

            var iconPresets = new Dictionary<string, string>
    {
        { "General", ModuleIcon },
        { "Interface", WindowIcon },
        { "Logs", LoggingIcon },
        { "Help & Support", SupportIcon }
    };

            foreach (var section in sections)
            {
                foreach (var itemName in section.Value)
                {
                    string iconPath = iconPresets.ContainsKey(itemName) ? iconPresets[itemName] : LoggingIcon;
                    items.Add(new SettingItem(itemName, section.Key, iconPath));
                }
            }

            var collectionView = new ListCollectionView(items);
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));

            SettingsListBox.ItemsSource = collectionView;
            var defaultItem = items.FirstOrDefault(i => i.Name == "General");
            if (defaultItem != null)
            {
                SettingsListBox.SelectedItem = defaultItem;

                GeneralBorder.Visibility = Visibility.Visible;
                _currentVisiblePage = GeneralBorder;
            }
        }

        private void SettingsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SettingsListBox.SelectedItem is SettingItem selectedItem)
            {
                switch (selectedItem.Name)
                {
                    case "General":
                        ShowSettingsPage(GeneralBorder);
                        break;

                    case "Interface":
                        ShowSettingsPage(InterfaceBorder);
                        break;

                    case "Logs":
                        ShowSettingsPage(DebugBorderSettings);
                        break;

                    case "Help & Support":
                        var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                        if (mainWindow?.MessageBoxSpawnParent == null)
                        {
                            var result = MessageBox.Show(
                                "Hello!\n\nWould you like to join the Discord server for support?\n\nPlease note: support is available in English only.",
                                "Join Discord",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question
                            );

                            if (result == MessageBoxResult.Yes)
                            {
                                try
                                {
                                    Process.Start(new ProcessStartInfo("https://discord.com/invite/velocityide")
                                    {
                                        UseShellExecute = true
                                    });
                                }
                                catch (Exception ex)
                                {
                                    _ = ShowMessageBoxInGridAsync(
                                        "Startup Error",
                                        $"Failed to open link: {ex.Message}",
                                        primaryButtonText: "Ok",
                                        iconPath: "Resource/Msg/BoxType_Error.png"
                                    );
                                }
                            }
                            break;
                        }

                        var discordDialog = new MessageBoxStandard();
                        Grid.SetZIndex(discordDialog, 999);
                        mainWindow.MessageBoxSpawnParent.Children.Add(discordDialog);

                        _ = discordDialog.ShowDialogAsync(
                            "Join Discord",
                            "Would you like to join the Discord server for support?\n\nPlease note: support is available in English only.",
                            yesText: "Yes",
                            noText: "No",
                            iconPath: "Resource/Sidebar/Discord.png"
                        ).ContinueWith(async t =>
                        {
                            if (t.Result == VelocityLite.Controls.DialogResult.Yes)
                            {
                                try
                                {
                                    Process.Start(new ProcessStartInfo("https://discord.com/invite/velocityide")
                                    {
                                        UseShellExecute = true
                                    });
                                }
                                catch (Exception ex)
                                {
                                    await ShowMessageBoxInGridAsync(
                                        "Startup Error",
                                        $"Failed to open link: {ex.Message}",
                                        primaryButtonText: "Ok",
                                        iconPath: "Resource/Msg/BoxType_Error.png"
                                    );
                                }
                            }

                            if (mainWindow.MessageBoxSpawnParent.Children.Contains(discordDialog))
                                mainWindow.MessageBoxSpawnParent.Children.Remove(discordDialog);

                        }, TaskScheduler.FromCurrentSynchronizationContext());
                        break;
                }

                _previousSelectedItem = selectedItem;
            }
        }

        private void ShowSettingsPage(Border pageToShow)
        {
            if (_currentVisiblePage == pageToShow) return;

            if (_currentVisiblePage != null)
                AnimatePageOut(_currentVisiblePage);

            AnimatePageIn(pageToShow);

            _currentVisiblePage = pageToShow;
        }

        private void AnimatePageIn(UIElement element, int durationMs = 350)
        {
            if (element == null) return;

            element.Visibility = Visibility.Visible;

            if (element.RenderTransform is not TranslateTransform transform)
                element.RenderTransform = transform = new TranslateTransform();

            transform.Y = 20;
            element.Opacity = 0;

            element.Dispatcher.BeginInvoke(() =>
            {
                AnimateSlide(element, 20, 0, durationMs);
                AnimateOpacity(element, 1, durationMs + 50);
            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private Task Fade(UIElement element, double to, double duration = 0.25)
        {
            var tcs = new TaskCompletionSource<bool>();

            var anim = new DoubleAnimation
            {
                To = to,
                Duration = TimeSpan.FromSeconds(duration),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            anim.Completed += (_, __) => tcs.SetResult(true);
            element.BeginAnimation(UIElement.OpacityProperty, anim);

            return tcs.Task;
        }

        private void AnimateRenamePopIn(UIElement element, int durationMs = 350)
        {
            if (element == null) return;

            element.Visibility = Visibility.Visible;

            if (element.RenderTransform is not TranslateTransform transform)
                element.RenderTransform = transform = new TranslateTransform();

            transform.Y = 20;
            element.Opacity = 0;

            element.Dispatcher.BeginInvoke(() =>
            {
                var slideAnim = new DoubleAnimation
                {
                    From = 30,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(durationMs),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                transform.BeginAnimation(TranslateTransform.YProperty, slideAnim);

                var opacityAnim = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(durationMs),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                element.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void AnimateRenamePopOut(UIElement element, int durationMs = 250)
        {
            if (element == null) return;

            if (element.RenderTransform is not TranslateTransform transform)
                element.RenderTransform = transform = new TranslateTransform();

            var slideAnim = new DoubleAnimation
            {
                From = 0,
                To = -30, 
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            transform.BeginAnimation(TranslateTransform.YProperty, slideAnim);

            var opacityAnim = new DoubleAnimation
            {
                From = element.Opacity,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            opacityAnim.Completed += (s, e) => element.Visibility = Visibility.Collapsed;
            element.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
        }
        private void AnimateOpacity(UIElement element, double to, int durationMs = 300)
        {
            DoubleAnimation opacityAnim = new DoubleAnimation
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            element.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
        }

        private Task AnimatePageOut(UIElement element, int durationMs = 250)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (element == null)
            {
                tcs.SetResult(true);
                return tcs.Task;
            }

            if (element.RenderTransform is not TranslateTransform transform)
                element.RenderTransform = transform = new TranslateTransform();

            var dispatcherTask = element.Dispatcher.InvokeAsync(() =>
            {
                var opacityAnim = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(durationMs),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };

                opacityAnim.Completed += (s, e) =>
                {
                    element.Visibility = Visibility.Collapsed;
                    transform.Y = 0;   
                    element.Opacity = 1; 
                    tcs.TrySetResult(true);
                };

                element.BeginAnimation(UIElement.OpacityProperty, opacityAnim);

                AnimateSlide(element, 0, -20, durationMs);
            }, System.Windows.Threading.DispatcherPriority.Loaded);

            return tcs.Task;
        }

        private void AnimateSlide(UIElement element, double fromY, double toY, int durationMs = 350)
        {
            if (element.RenderTransform is not TranslateTransform transform)
                element.RenderTransform = transform = new TranslateTransform();

            DoubleAnimation slideAnim = new DoubleAnimation
            {
                From = fromY,
                To = toY,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            transform.BeginAnimation(TranslateTransform.YProperty, slideAnim);
        }

        #endregion

        #region Comboboxes

        private void HideVelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        #endregion

        #endregion

        #region AccentColorPicker logic

        private Color? GetCurrentAccentColor()
        {
            if (SettingsAccentColorPreview.Background is SolidColorBrush brush)
            {
                return brush.Color;
            }
            return null;
        }


        private void SettingsAccentColorContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var initialColor = GetCurrentAccentColor();

            var color = ShowWin32ColorPicker(this, initialColor);

            if (color == null)
                return;

            ApplyAccentColor(color.Value);
            SaveAccentColor(color.Value);
        }

        private SolidColorBrush? _accentBrush;

        private void ApplyAccentColor(Color color)
        {
            _accentBrush = new SolidColorBrush(color);
            _accentBrush.Freeze();


            BlueBarHome.Background = _accentBrush;
            BlueBarHomeFullyFilled.Background = _accentBrush;

            BlueBarCloud.Background = _accentBrush;
            BlueBarCloudFullyFilled.Background = _accentBrush;

            BlueBarScripts.Background = _accentBrush;
            BlueBarScriptsFullyFilled.Background = _accentBrush;

            BlueBarNewsChangeLogs.Background = _accentBrush;
            BlueBarNewsChangeLogsFullyFilled.Background = _accentBrush;

            SettingsAccentColorPreview.Background = _accentBrush;

            SettingsAccentColorPreview.Background = _accentBrush;
            SettingsAccentColorPreview.BorderBrush = _accentBrush;

            SettingsAccentColorPreview.BorderBrush = _accentBrush;

            Application.Current.Resources["TabTopBarBrush"] = _accentBrush;
            Application.Current.Resources["TabTopBarColor"] = color;

            Application.Current.Resources["AccentColorBrush"] = new SolidColorBrush(color);
        }

        private readonly Color DefaultAccentColor = (Color)ColorConverter.ConvertFromString("#2D7DFF");
        private void ResetAccentColorButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyAccentColor(DefaultAccentColor);
            SaveAccentColor(DefaultAccentColor);
        }

        private void LoadAccentColorOnStartup()
        {
            if (string.IsNullOrEmpty(_appSettings.AccentColor))
            {
                ApplyAccentColor(DefaultAccentColor);
            }
            else
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(_appSettings.AccentColor);
                    ApplyAccentColor(color);
                }
                catch
                {
                    ApplyAccentColor(DefaultAccentColor);
                }
            }
        }


        private void SaveAccentColor(Color color)
        {
            string hexColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            _appSettings.AccentColor = hexColor;
            SettingsManager.Save(_appSettings);
        }

        #endregion

        #region File Explorer

  
        #region Class
        public class FileItem : INotifyPropertyChanged
        {
            public string? Name { get; set; }
            public string? Icon { get; set; }
            public bool IsFavourite { get; set; }
            public string? Category { get; set; }
            public string? ScriptSource { get; set; }

            public string? FullPath { get; set; }

            private bool _isLoading;
            public bool IsLoading
            {
                get => _isLoading;
                set
                {
                    if (_isLoading != value)
                    {
                        _isLoading = value;
                        OnPropertyChanged(nameof(IsLoading));
                    }
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Buttons

        private void SpinReloadIcon()
        {
            if (ReloadImage.RenderTransform is not RotateTransform rotate)
                return;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            rotate.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        private void ReloadExplroer_Click(object sender, RoutedEventArgs e)
        {
            SpinReloadIcon();

            string explorerPath = System.IO.Path.Combine(
                Directory.GetCurrentDirectory(), "Scripts");

            LoadFiles(explorerPath);
        }


        #endregion

        #region Loader
        private void LoadFiles(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return;

            _isRefreshing = true;

            var items = new List<FileItem>();

            var favFolder = System.IO.Path.Combine(folderPath, "Favourites");
            if (Directory.Exists(favFolder))
            {
                var favFiles = Directory.GetFiles(favFolder, "*.*", SearchOption.TopDirectoryOnly)
                    .Select(f => new FileItem
                    {
                        Name = System.IO.Path.GetFileName(f),
                        Icon = "/Resource/Exp/Star_Gold.png",
                        IsFavourite = true,
                        Category = "FAVOURITES",
                        FullPath = f
                    });
                items.AddRange(favFiles.OrderBy(f => f.Name));
            }

            foreach (var dir in Directory.GetDirectories(folderPath))
            {
                var dirName = System.IO.Path.GetFileName(dir);

                if (dirName.Equals("Favourites", StringComparison.OrdinalIgnoreCase))
                    continue;

                bool isCommunity = dirName.Equals("Community", StringComparison.OrdinalIgnoreCase);

                var dirFiles = Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly)
       .Select(f => new FileItem
       {
           Name = Path.GetFileName(f),
           Icon = isCommunity
               ? "/Resource/Exp/CommunityGlobe.png"
               : GetIconForFile(f),
           IsFavourite = false,
           Category = isCommunity
               ? "COMMUNITY"
               : dirName.ToUpperInvariant(),
           FullPath = f
       });

                items.AddRange(dirFiles.OrderBy(f => f.Name));
            }


            var rootFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => !f.Contains("Favourites") && !f.EndsWith("ScriptBloxOnline.json", StringComparison.OrdinalIgnoreCase))
                .Select(f => new FileItem
                {
                    Name = System.IO.Path.GetFileName(f),
                    Icon = GetIconForFile(f),
                    IsFavourite = false,
                    Category = "FILES",
                    FullPath = f
                });

            items.AddRange(rootFiles.OrderBy(f => f.Name));

            _explorerView = new ListCollectionView(items);

            _explorerView.GroupDescriptions.Clear();
            _explorerView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));

            _explorerView.GroupDescriptions[0].CustomSort = new CategoryComparer();

            _explorerView.SortDescriptions.Clear();
            _explorerView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            ExplorerListBox.ItemsSource = _explorerView;

            ExplorerListBox.SelectedIndex = -1;

            _isRefreshing = false;
        }

        private string GetIconForFile(string filePath)
        {
            string ext = System.IO.Path.GetExtension(filePath).ToLower();
            return ext switch
            {
                ".txt" => "/Resource/Exp/TXT-File.png",
                ".lua" => "/Resource/Exp/Script-File_Alt.png",
                ".luau" => "/Resource/Exp/Script-File_Alt.png",
                _ => "/Resource/Exp//File-Default.png"
            };
        }
        #endregion

        #region Selection Changed
        private async void ExplorerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isRefreshing) return;

            if (Mouse.RightButton == MouseButtonState.Pressed)
            {
                return;
            }

            if (!ExplorerListBox.IsLoaded) return;

            if (ExplorerListBox.SelectedItem is not FileItem selectedFile || string.IsNullOrEmpty(selectedFile.FullPath))
                return;

            if (!File.Exists(selectedFile.FullPath))
            {
                ExplorerListBox.SelectedIndex = -1;
                return;
            }

            string fileContent;
            try
            {
                fileContent = await File.ReadAllTextAsync(selectedFile.FullPath);
            }
            catch (Exception)
            {
                ExplorerListBox.SelectedIndex = -1;
                return;
            }

            CreateEditorInstance(selectedFile.Name!, fileContent, selectedFile.Icon ?? "/Assets/Explorer/File-Default.png");
            ExplorerListBox.SelectedIndex = -1;
        }

        private void ExplorerListBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = VisualUpwardSearch<ListBoxItem>(e.OriginalSource as DependencyObject);
            if (listBoxItem != null)
            {
                listBoxItem.IsSelected = true;
                listBoxItem.Focus();
            }
        }

        private static T? VisualUpwardSearch<T>(DependencyObject? source) where T : DependencyObject
        {
            while (source != null && source is not T)
                source = VisualTreeHelper.GetParent(source);

            return source as T;
        }


        #endregion

        #region Search Indexing

        private bool ExplorerFilter(object obj)
        {
            if (obj is not FileItem item)
                return false;

            if (string.IsNullOrWhiteSpace(SearchTextBoxExp.Text))
                return true;

            return item.Name?.IndexOf(
                SearchTextBoxExp.Text,
                StringComparison.OrdinalIgnoreCase
            ) >= 0;
        }


        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_explorerView == null)
                return;

            _explorerView.Filter = ExplorerFilter;
            _explorerView.Refresh();
        }

        #endregion 

        #endregion

        #region Control Buttons

        private bool isInjecting = false;

        private async void InjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (isInjecting)
            {
                WriteToTerminal("Already injecting!");
                return;
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string injectorPath = System.IO.Path.Combine(baseDir, "Bin", "erto3e4rortoergn.exe");
            string versionFile = System.IO.Path.Combine(baseDir, "Bin", "current_version.txt");

            if (!File.Exists(injectorPath))
            {
                try
                {
                    if (File.Exists(versionFile))
                        File.Delete(versionFile);
                }
                catch (Exception ex)
                {
                    WriteToTerminal($"Failed to clean version file: {ex.Message}");
                }

                _ = ShowMessageBoxInGridAsync("Injector Missing","The injector is missing!\n\nPlease re-open Velocity to reinstall it.\n\nYou should disable your antivirus or add an exclusion to prevent this from happening again.",primaryButtonText: "Ok", iconPath: "Resource/Msg/BoxType_Error.png");


                var announcement = new GlobalAnnouncement
                {
                    Title = "Ready to reinstall",
                    Body = "Simply restart Velocity to re-download the injector.",
                    Duration = 15000
                };

                _ = ShowToastAnnouncement(announcement);

                WriteToTerminal("The injector is missing!");
                WriteToTerminal("Scheduling a reinstall on run..");

                return;
            }

            isInjecting = true;

            await Task.Run(async () =>
            {
                var targetPids = GetTargetPids();

                if (targetPids.Count == 0)
                {
                    WriteToTerminal(
                        _appSettings.AutomaticSelection
                            ? "Open Roblox first."
                            : "No clients selected."
                    );

                    isInjecting = false;
                    return;
                }

                WriteToTerminal($"Targeting {targetPids.Count} instance(s)");

                bool allAttached = targetPids.All(pid =>
                {
                    try
                    {
                        return velocityApi.IsAttached(pid);
                    }
                    catch
                    {
                        return false;
                    }
                });

                if (allAttached)
                {
                    WriteToTerminal("Already injected into all target instances!");
                    isInjecting = false;
                    return;
                }

                List<int> injectedPids = new();

                foreach (int pid in targetPids)
                {
                    try
                    {
                        if (velocityApi.IsAttached(pid))
                            continue;

                        Application.Current.Dispatcher.Invoke(() =>
                            UpdateClientStatus(pid, "Injecting")
                        );

                        WriteToTerminal($"Injecting into {pid}...");

                        await velocityApi.Attach(pid);
                        injectedPids.Add(pid);

                        Application.Current.Dispatcher.Invoke(() =>
                            UpdateClientStatus(pid, "Attached")
                        );

                        WriteToTerminal($"Injected into {pid}");
                    }
                    catch (Exception ex)
                    {
                        WriteToTerminal($"Failed to inject into {pid}: {ex.Message}");

                        Application.Current.Dispatcher.Invoke(() =>
                            UpdateClientStatus(pid, "Not Attached")
                        );
                    }
                }

                if (injectedPids.Count == 0)
                    WriteToTerminal("No new instances were injected.");

                isInjecting = false;
            });
        }

        private async void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            var targetPids = GetTargetPids();

            bool anyAttached = targetPids.Any(pid =>
            {
                try
                {
                    return velocityApi.IsAttached(pid);
                }
                catch
                {
                    return false;
                }
            });

            if (!anyAttached)
            {
                WriteToTerminal("Inject first.");
                return;
            }

            if (TabCtrl.SelectedItem is TabItem tabItem &&
                tabItem.Content is Grid contentGrid)
            {
                var editor = contentGrid.Children.OfType<WebViewAPI>().FirstOrDefault();

                if (editor != null)
                {
                    string scriptContent = await editor.GetText();

                    if (string.IsNullOrWhiteSpace(scriptContent))
                    {
                        WriteToTerminal("No script to execute.");
                        return;
                    }

                    try
                    {
                        int executedCount = 0;

                        foreach (int pid in targetPids)
                        {
                            if (velocityApi.IsAttached(pid))
                            {
                                velocityApi.Execute(scriptContent, pid);
                                executedCount++;
                            }
                        }

                        WriteToTerminal(
                            executedCount > 0
                                ? $"Script executed on {executedCount} client(s)."
                                : "No attached clients found."
                        );
                    }
                    catch (Exception ex)
                    {
                        WriteToTerminal($"Failed to execute script: {ex.Message}");
                    }
                }
            }
        }

        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (TabCtrl.SelectedItem is TabItem activeTab && activeTab.Content is Grid grid)
            {
                foreach (var child in grid.Children)
                {
                    if (child is WebViewAPI editor)
                    {
                        try
                        {
                            await editor.SetText("");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Editor] Failed to clear tab: {ex.Message}");
                        }
                        break;
                    }
                }
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (TabCtrl.SelectedItem is not TabItem activeTab || activeTab.Content is not Grid grid)
                return;

            WebViewAPI? editor = null;
            foreach (var child in grid.Children)
            {
                if (child is WebViewAPI e2)
                {
                    editor = e2;
                    break;
                }
            }

            if (editor == null)
                return;

            string text = await editor.GetText();

            if (string.IsNullOrWhiteSpace(text))
                return;

            var saveDialog = new SaveFileDialog
            {
                Title = "Save your script",
                Filter = "All Files (*.*)|*.*|Lua Script (*.lua)|*.lua|Luau Script (*.luau)|*.luau|Text File (*.txt)|*.txt",
                FilterIndex = 2,
                FileName = activeTab.Header?.ToString() ?? "NewScript"
            };

            bool? result = saveDialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    await File.WriteAllTextAsync(saveDialog.FileName, text);
                }
                catch
                {
                }
            }
        }


        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Title = "Open a script file",
                Filter = "All Files (*.*)|*.*|Storm Script (*.str)|*.str|Lua Script (*.lua)|*.lua|Luau Script (*.luau)|*.luau|Text File (*.txt)|*.txt",
                FilterIndex = 0,
                Multiselect = false
            };

            bool? result = openDialog.ShowDialog();

            if (result != true)
                return;

            string filePath = openDialog.FileName;
            string fileName = System.IO.Path.GetFileName(filePath);

            string fileContent;
            try
            {
                fileContent = await File.ReadAllTextAsync(filePath);
            }
            catch
            {
                return;
            }

            if (TabCtrl.Items.Count == 0)
            {
                CreateEditorInstance(fileName, fileContent, "Assets/Explorer/Script-File_Alt.png");
                return;
            }

            if (TabCtrl.SelectedItem is TabItem activeTab && activeTab.Content is Grid grid)
            {
                WebViewAPI? editor = null;
                foreach (var child in grid.Children)
                {
                    if (child is WebViewAPI e2)
                    {
                        editor = e2;
                        break;
                    }
                }

                if (editor != null)
                {
                    var fadeOut = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds(150),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                    };

                    var fadeIn = new DoubleAnimation
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(150),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                    };

                    var fadeTcs = new TaskCompletionSource<bool>();
                    fadeOut.Completed += async (s2, e2) =>
                    {
                        try
                        {
                            await editor.SetText(fileContent);
                            activeTab.Header = fileName;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Editor] Failed to load file: {ex.Message}");
                        }

                        editor.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                        fadeTcs.SetResult(true);
                    };

                    editor.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                    await fadeTcs.Task;
                }
                else
                {
                    CreateEditorInstance(fileName, fileContent, "Assets/Explorer/Script-File_Alt.png");
                }
            }
            else
            {
                CreateEditorInstance(fileName, fileContent, "Assets/Explorer/Script-File_Alt.png");
            }
        }

        private void CopyTerminalOutput_Click(object sender, RoutedEventArgs e)
        {
            if (TerminalBox.Document != null)
            {
                TextRange textRange = new TextRange(TerminalBox.Document.ContentStart, TerminalBox.Document.ContentEnd);
                string text = textRange.Text;

                if (!string.IsNullOrEmpty(text))
                {
                    Clipboard.SetText(text);
                }

                WriteToTerminal("Copied to clipboard!");    
            }
        }

        private void SaveTerminalOutput_Click(object sender, RoutedEventArgs e)
        {
            if (TerminalBox.Document != null)
            {
                TextRange textRange = new TextRange(TerminalBox.Document.ContentStart, TerminalBox.Document.ContentEnd);
                string text = textRange.Text;

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*",
                    DefaultExt = ".txt",
                    FileName = "TerminalOutput.txt"
                };

                bool? result = saveFileDialog.ShowDialog();

                if (result == true)
                {
                    try
                    {
                        File.WriteAllText(saveFileDialog.FileName, text);
                    }
                    catch (Exception ex)
                    {
                        _ = ShowMessageBoxInGridAsync("Error", $"Failed to save file:\n{ex.Message}", primaryButtonText: "Ok", iconPath: "Resource/Msg/BoxType_Error.png");
                    }
                }
            }
        }

        #endregion

        #region Terminal Buttons

        private void ClearTerminal_Click(object sender, RoutedEventArgs e)
        {
            TerminalBox.Document.Blocks.Clear();
        }

        #endregion

        #endregion  

        #region Methods

        #region Terminal

        public void WriteToTerminal(string message)
        {
            TerminalBox.Dispatcher.Invoke(() =>
            {
                if (TerminalBox.Document == null)
                    TerminalBox.Document = new FlowDocument
                    {
                        PagePadding = new Thickness(0),
                        ColumnWidth = double.PositiveInfinity,
                        TextAlignment = TextAlignment.Left,
                        LineHeight = 18
                    };

                var doc = TerminalBox.Document;

                Paragraph paragraph;
                if (doc.Blocks.FirstBlock is Paragraph p)
                    paragraph = p;
                else
                {
                    paragraph = new Paragraph { Margin = new Thickness(0) };
                    doc.Blocks.Add(paragraph);
                }

                string timeStamp = DateTime.Now.ToString("HH:mm:ss");

                paragraph.Inlines.Add(new Run($"[{timeStamp}] - ")
                {
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x80, 255, 255, 255))
                });

                paragraph.Inlines.Add(new Run(message)
                {
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xCC, 255, 255, 255))
                });

                paragraph.Inlines.Add(new LineBreak());
                TerminalBox.ScrollToEnd();

                if (_appSettings.SaveOutputLocally && !string.IsNullOrEmpty(_logFilePath))
                {
                    try
                    {
                        string lineToWrite = $"[{timeStamp}] - {message}{Environment.NewLine}";
                        File.AppendAllText(_logFilePath, lineToWrite);
                    }
                    catch (Exception ex)
                    {
                        WriteToTerminal($"Failed to save log: {ex.Message}");
                    }
                }
            });
        }

        public void WriteToTerminal(string message, string? linkText, string? linkUrl)
        {
            TerminalBox.Dispatcher.Invoke(() =>
            {
                if (TerminalBox.Document == null)
                    TerminalBox.Document = new FlowDocument
                    {
                        PagePadding = new Thickness(0),
                        ColumnWidth = double.PositiveInfinity,
                        TextAlignment = TextAlignment.Left,
                        LineHeight = 18
                    };

                var doc = TerminalBox.Document;

                Paragraph paragraph;
                if (doc.Blocks.FirstBlock is Paragraph p)
                    paragraph = p;
                else
                {
                    paragraph = new Paragraph { Margin = new Thickness(0) };
                    doc.Blocks.Add(paragraph);
                }

                paragraph.Inlines.Add(new Run($"[{DateTime.Now:HH:mm:ss}] - ")
                {
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x80, 255, 255, 255))
                });

                paragraph.Inlines.Add(new Run(message + " ")
                {
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xCC, 255, 255, 255))
                });

                if (!string.IsNullOrEmpty(linkText) && !string.IsNullOrEmpty(linkUrl))
                {
                    Hyperlink hyperlink = new Hyperlink(new Run(linkText))
                    {
                        Foreground = Brushes.DodgerBlue,
                        TextDecorations = TextDecorations.Underline,
                        NavigateUri = new Uri(linkUrl),
                        ToolTip = "Download Version from rdd.weao.gg"
                    };

                    hyperlink.Cursor = Cursors.Hand;

                    hyperlink.RequestNavigate += (_, e) =>
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
                            {
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            WriteToTerminal($"Failed to open link: {ex.Message}");
                        }
                    };

                    paragraph.Inlines.Add(hyperlink);
                }

                paragraph.Inlines.Add(new LineBreak());
                TerminalBox.ScrollToEnd();
            });
        }

        #endregion

        #region Version Fetching

        #region Classes

        public async Task WriteSupportedRblxVersionAsync(string exploit)
        {
            try
            {
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("WEAO-3PService");

                string url = $"https://weao.xyz/api/status/exploits/{exploit}";
                string json = await client.GetStringAsync(url);

                using JsonDocument doc = JsonDocument.Parse(json);
                string? rbxVersion = doc.RootElement.GetProperty("rbxversion").GetString();

                if (string.IsNullOrEmpty(rbxVersion))
                {
                    WriteToTerminal("Supported version: Unknown");
                    return;
                }

                string redirectUrl = $"https://rdd.weao.gg/?channel=LIVE&binaryType=WindowsPlayer&version={rbxVersion}&includeLauncher=true";

                WriteToTerminal("Supported version:", rbxVersion, redirectUrl);
            }
            catch (Exception ex)
            {
              //  var announcement = new GlobalAnnouncement
              //  {
               //     Title = "Error Fetching Version",
                //    Body = "This does not mean you can not use Velocity! Check the terminal for more information on what failed.",
                 //   Duration = 15000
                // };

                WriteToTerminal($"(NOTE: If you see this Error, it does NOT mean you can not use Velocity!");
                WriteToTerminal($"Failed to fetch supported version: {ex.Message}");

               // _ = ShowToastAnnouncement(announcement);

            }
        }


        #endregion

        private void LoadVersionText()
        {
            try
            {
                string dir = Directory.GetCurrentDirectory();
                string versionFile = System.IO.Path.Combine(dir, "bin", "current_version.txt");

                if (File.Exists(versionFile))
                {
                    string versionText = File.ReadAllText(versionFile).Trim();
                    VelVersionLocal.Text = versionText;
                }
                else
                {
                    VelVersionLocal.Text = "0.0.0";
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Tab Creation
        public void CreateEditorInstance(string title, string content = "", string? iconPath = null)
        {
            var MonacoTextEditor = new WebViewAPI
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Visibility = Visibility.Hidden
            };

            string htmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Velocity Ui", "Monaco Editor", "index.html");

            if (File.Exists(htmlFilePath))
            {
                MonacoTextEditor.Source = new Uri(htmlFilePath);
            }
            else
            {
                _ = ShowMessageBoxInGridAsync("Error", $"Monaco HTML not found at: {htmlFilePath}", primaryButtonText: "Ok", iconPath: "Resource/Msg/BoxType_Error.png");
                return;
            }

            if (MonacoTextEditor.Parent is Panel oldPanel) oldPanel.Children.Remove(MonacoTextEditor);
            else if (MonacoTextEditor.Parent is ContentControl oldContent) oldContent.Content = null;

            Grid contentGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Opacity = 0
            };
            contentGrid.Children.Add(MonacoTextEditor);

            TabItem tabItem = new TabItem
            {
                Header = title,
                Content = contentGrid,
                Style = this.TryFindResource("Tab") as Style,
                Tag = string.IsNullOrEmpty(iconPath) ? "/Resource/FileImage/file_type_lua.png" : iconPath,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch
            };

            this.TabCtrl.Items.Add(tabItem);
            this.TabCtrl.SelectedItem = tabItem;

            if (TabCtrl.Items.Count == 1)
            {
                _ = Fade(EmptyStatePanel, 0);
            }

            tabItem.Loaded += async (s, e) =>
            {
                var closeButton = FindCloseButtonInTemplate(tabItem);
                if (closeButton != null)
                {
                    closeButton.Click += async (s2, e2) =>
                    {
                        var rootGrid = tabItem.Template.FindName("RootGrid", tabItem) as Grid;
                        Task tabAnimTask = Task.CompletedTask;
                        if (rootGrid != null)
                        {
                            var slideAnim = new DoubleAnimation
                            {
                                To = -50,
                                Duration = TimeSpan.FromSeconds(0.25),
                                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                            };
                            var fadeTab = new DoubleAnimation
                            {
                                To = 0,
                                Duration = TimeSpan.FromSeconds(0.25),
                                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                            };
                            var tcs = new TaskCompletionSource<bool>();
                            fadeTab.Completed += (a, b) => tcs.SetResult(true);

                            rootGrid.RenderTransform = new TranslateTransform();
                            rootGrid.RenderTransform.BeginAnimation(TranslateTransform.XProperty, slideAnim);
                            rootGrid.BeginAnimation(UIElement.OpacityProperty, fadeTab);
                            tabAnimTask = tcs.Task;
                        }

                        var contentGridLocal = tabItem.Content as Grid;
                        Task editorAnimTask = Task.CompletedTask;
                        if (contentGridLocal != null)
                        {
                            var editorFade = new DoubleAnimation
                            {
                                To = 0,
                                Duration = TimeSpan.FromSeconds(0.25),
                                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                            };
                            var tcs = new TaskCompletionSource<bool>();
                            editorFade.Completed += (a, b) => tcs.SetResult(true);
                            contentGridLocal.BeginAnimation(UIElement.OpacityProperty, editorFade);
                            editorAnimTask = tcs.Task;
                        }

                        await Task.WhenAll(tabAnimTask, editorAnimTask);

                        if (contentGridLocal != null)
                        {
                            var editor = contentGridLocal.Children.OfType<WebViewAPI>().FirstOrDefault();
                            if (editor != null)
                            {
                                await editor.SetText("");
                                editor.Visibility = Visibility.Hidden;
                            }
                        }

                        TabCtrl.Items.Remove(tabItem);

                        if (TabCtrl.Items.Count == 0)
                        {
                            await Fade(EmptyStatePanel, 1);
                        }
                    };
                }

                var cm = tabItem.Template.FindName("TabContextMenu", tabItem) as ContextMenu;
                if (cm != null)
                {
                    tabItem.ContextMenu = cm;
                    cm.Opened += (sender2, args2) =>
                    {
                        RegisterContextMenuItems(cm, tabItem);
                    };
                }

                var readyTcs = new TaskCompletionSource<bool>();
                EventHandler? readyHandler = null;
                readyHandler = (s2, args2) =>
                {
                    _openedTabs.Add(tabItem);
                    MonacoTextEditor.EditorReady -= readyHandler;
                    readyTcs.SetResult(true);
                };

                MonacoTextEditor.EditorReady += readyHandler;
                await readyTcs.Task;

                await MonacoTextEditor.SetText(content);
                MonacoTextEditor.Visibility = Visibility.Visible;

                var fadeInEditor = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.25),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                contentGrid.BeginAnimation(UIElement.OpacityProperty, fadeInEditor);
            };

            tabItem.PreviewMouseRightButtonDown += (s, e) =>
            {
                e.Handled = true;
                if (tabItem.ContextMenu != null)
                {
                    tabItem.ContextMenu.PlacementTarget = tabItem;
                    tabItem.ContextMenu.Placement = PlacementMode.MousePoint;
                    tabItem.ContextMenu.IsOpen = true;
                }
            };

            void RegisterContextMenuItems(ContextMenu cm, TabItem tab)
            {
                void AttachHandler(MenuItem? item, string name)
                {
                    if (item != null)
                    {
                        item.Click -= MenuItemClickHandler;
                        item.Click += MenuItemClickHandler;
                    }
                }

                AttachHandler(FindVisualChild<MenuItem>(cm, "DuplicateMenuItem"), "DuplicateMenuItem");
                AttachHandler(FindVisualChild<MenuItem>(cm, "RenameMenuItem"), "RenameMenuItem");
                AttachHandler(FindVisualChild<MenuItem>(cm, "ExecuteMenuItem"), "ExecuteMenuItem");
                AttachHandler(FindVisualChild<MenuItem>(cm, "CloseOtherMenuItem"), "CloseOtherMenuItem");
            }

            async void MenuItemClickHandler(object sender2, RoutedEventArgs args2)
            {
                if (sender2 is MenuItem item)
                {
                    if (item.Parent is ContextMenu cm)
                    {
                        if (cm.PlacementTarget is TabItem tab)
                        {
                            string? tag = item.Tag as string;
                            if (tag == null) return;

                            switch (tag)
                            {
                                case "CopyyTab":
                                    var editor = (tab.Content as Grid)?.Children[0] as WebViewAPI;
                                    var existingText = editor != null ? await editor.GetText() : "";

                                    CreateEditorInstance("Copy of " + tab.Header.ToString(), existingText, tab.Tag?.ToString());
                                    break;

                                case "CloseAllButThis":
                                    var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                                    if (mainWindow?.MessageBoxSpawnParent == null)
                                    {
                                        if (MessageBox.Show(
                                            $"Are you sure you want to close all other tabs except '{tab.Header}'?",
                                            "Close Other Tabs",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Warning
                                        ) != MessageBoxResult.Yes)
                                            return;
                                    }
                                    else
                                    {
                                        var dialog = new MessageBoxStandard();
                                        Grid.SetZIndex(dialog, 999);
                                        mainWindow.MessageBoxSpawnParent.Children.Add(dialog);

                                        var result = await dialog.ShowDialogAsync(
                                            "Close Other Tabs",
                                            $"Are you sure you want to close all other tabs except '{tab.Header}'?",
                                            "Yes",
                                            "No",
                                            "Resource/Msg/BoxType_Warn.png"
                                        );

                                        if (result != VelocityLite.Controls.DialogResult.Yes)
                                        {
                                            mainWindow.MessageBoxSpawnParent.Children.Remove(dialog);
                                            return;
                                        }

                                        mainWindow.MessageBoxSpawnParent.Children.Remove(dialog);
                                    }

                                    var others = TabCtrl.Items.Cast<TabItem>()
                                        .Where(t => t != tab)
                                        .ToList();

                                    foreach (var t in others)
                                        TabCtrl.Items.Remove(t);
                                    break;

                                case "RenameTab":
                                    ShowRenamePopup(tab);
                                    break;

                                case "ExecuteTab":
                                    await ExecuteTabScriptAsync(tab);
                                    break;
                            }
                        }
                    }
                }
            }

            async Task ExecuteTabScriptAsync(TabItem tab)
            {
                Process[] robloxProcesses = Process.GetProcessesByName("RobloxPlayerBeta");

                bool anyAttached = robloxProcesses.Any(p => !p.HasExited && velocityApi.IsAttached(p.Id));

                if (!anyAttached)
                {
                    WriteToTerminal("Inject first!");
                    return;
                }

                if (tab.Content is Grid contentGrid)
                {
                    var editor = contentGrid.Children.OfType<WebViewAPI>().FirstOrDefault();
                    if (editor != null)
                    {
                        string scriptContent = await editor.GetText();
                        if (string.IsNullOrWhiteSpace(scriptContent))
                        {
                            WriteToTerminal("No script to execute.");
                            return;
                        }

                        try
                        {
                            velocityApi.Execute(scriptContent);
                            WriteToTerminal("Script Executed");
                        }
                        catch (Exception ex)
                        {
                            WriteToTerminal($"Failed to execute script: {ex.Message}");
                        }
                    }
                }
            }

            T? FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
            {
                if (parent == null) return null;
                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child is T tChild && tChild.Name == name)
                        return tChild;
                    var result = FindVisualChild<T>(child, name);
                    if (result != null) return result;
                }
                return null;
            }

            MenuItem? FindMenuItemByName(ItemsControl container, string name)
            {
                foreach (var item in container.Items)
                {
                    if (item is MenuItem mi && mi.Name == name)
                        return mi;
                    if (item is MenuItem mi2 && mi2.HasItems)
                    {
                        var found = FindMenuItemByName(mi2, name);
                        if (found != null)
                            return found;
                    }
                }
                return null;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (_tabToRename != null)
            {
                _tabToRename.Header = RenameTabTextBox.Text;
            }
            HideRenamePopup();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            HideRenamePopup();
        }

        private void ShowRenamePopup(TabItem tab)
        {
            _tabToRename = tab;
            RenameTabTextBox.Text = tab.Header?.ToString() ?? "";

            RenamePopup.Visibility = Visibility.Visible;
            AnimateRenamePopIn(CoreBorderRename);
            AnimateOpacity(ShadowBlockRenameTab, 0.5);
        }

        private async void HideRenamePopup()
        {
            AnimateRenamePopOut(CoreBorderRename);
            AnimateOpacity(ShadowBlockRenameTab, 0);
            _tabToRename = null;

            await Task.Delay(400);
            RenamePopup.Visibility = Visibility.Collapsed;
        }

        private System.Windows.Controls.Button? FindCloseButtonInTemplate(TabItem tabItem)
        {
            return tabItem.Template.FindName("CloseButton", tabItem) as System.Windows.Controls.Button;
        }
        #endregion

        #region Tab close

        private void CloseHandler(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.TemplatedParent is TabItem tab)
            {
                RemoveTab(tab);
            }
        }
        private async void RemoveTab(TabItem tabItem)
        {
            if (tabItem?.Content is Grid contentGrid)
            {
                var editor = contentGrid.Children.OfType<VelocityLite.Controls.WebViewAPI>().FirstOrDefault();
                if (editor != null)
                {
                    await editor.SetText("");
                    editor.Visibility = Visibility.Hidden;
                }
            }

            this.TabCtrl.Items.Remove(tabItem);
        }

        #endregion

        #region Monaco 

        private async Task EnsureMonacoEditorExtracted()
        {
            string targetDir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Velocity Ui",
                "Monaco Editor"
            );

            if (Directory.Exists(targetDir) &&
                Directory.EnumerateFileSystemEntries(targetDir).Any())
                return;

            Directory.CreateDirectory(targetDir);

            Assembly assembly = Assembly.GetExecutingAssembly();

            const string resourceName = "VelocityLite.Monaco Editor.zip";

            using Stream? zipStream = assembly.GetManifestResourceStream(resourceName);
            if (zipStream == null)
                throw new FileNotFoundException($"Embedded resource not found: {resourceName}");

            using var zip = new ZipArchive(zipStream, ZipArchiveMode.Read);

            int totalFiles = zip.Entries.Count(e => !string.IsNullOrEmpty(e.Name));
            int currentFileIndex = 0;

            await SetSplashStatusAsync($"Extracting Monaco Editor... 0/{totalFiles} (0%)");

            foreach (var entry in zip.Entries)
            {
                string destinationPath = System.IO.Path.Combine(targetDir, entry.FullName);

                if (string.IsNullOrEmpty(entry.Name))
                {
                    Directory.CreateDirectory(destinationPath);
                    continue;
                }

                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destinationPath)!);

                currentFileIndex++;
                int percent = (int)((currentFileIndex / (double)totalFiles) * 100);

                await SetSplashStatusAsync($"Extracting... {entry.FullName} {currentFileIndex}/{totalFiles} ({percent}%)");

                entry.ExtractToFile(destinationPath, overwrite: true);
            }

            await SetSplashStatusAsync("Monaco Editor extraction complete.");
        }

        #endregion

        #region Tab Save

        public class SavedTab
        {
            public string? Title { get; set; } = "";
            public string? IconPath { get; set; } = "";
            public string? Content { get; set; } = "";
        }

        #region Tab Persistence

        private async Task SaveTabsAsync()
        {
            if (!_appSettings.SaveScripts)
                return;

            var existingTabs = await TabSessionManager.LoadAsync();

            var existingTabsDict = new Dictionary<string, SavedTab>();
            foreach (var t in existingTabs)
            {
                if (!string.IsNullOrEmpty(t.Title))
                    existingTabsDict[t.Title] = t;
            }

            var tabsToSave = new List<SavedTab>();

            foreach (TabItem tab in TabCtrl.Items)
            {
                string title = tab.Header?.ToString() ?? "Untitled";

                if (_openedTabs.Contains(tab))
                {
                    string content = "";
                    if (tab.Content is Grid grid)
                    {
                        var editor = grid.Children.OfType<WebViewAPI>().FirstOrDefault();
                        if (editor != null)
                        {
                            content = await editor.GetText();
                        }
                    }

                    tabsToSave.Add(new SavedTab
                    {
                        Title = title,
                        IconPath = tab.Tag?.ToString() ?? "",
                        Content = content
                    });
                }
                else
                {
                    if (existingTabsDict.TryGetValue(title, out var existingTab))
                    {
                        tabsToSave.Add(existingTab);
                    }
                    else
                    {
                        tabsToSave.Add(new SavedTab
                        {
                            Title = title,
                            IconPath = tab.Tag?.ToString() ?? "",
                            Content = ""
                        });
                    }
                }
            }

            await TabSessionManager.SaveAsync(tabsToSave);
        }

        private async Task LoadTabsAsync()
        {
            if (!_appSettings.SaveScripts)
                return;

            var tabs = await TabSessionManager.LoadAsync();

            foreach (var tab in tabs)
            {
                CreateEditorInstance(tab.Title ?? "Untitled", tab.Content ?? "", tab.IconPath);
            }
        }

        #endregion

        #endregion

        #endregion

        #region HWND

        #region Win32 HWND (fucking bullshit)

        private Color? ShowWin32ColorPicker(Window owner, Color? initialColor = null)
        {
            int[] customColors = new int[16];
            var handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            var cc = new CHOOSECOLOR
            {
                lStructSize = Marshal.SizeOf<CHOOSECOLOR>(),
                hwndOwner = handle,
                lpCustColors = Marshal.AllocHGlobal(sizeof(int) * 16),
                Flags = 0x00000001 | 0x00000002  
            };

            Marshal.Copy(customColors, 0, cc.lpCustColors, 16);

            if (initialColor.HasValue)
            {
                Color c = initialColor.Value;
                cc.rgbResult = c.R | (c.G << 8) | (c.B << 16);
            }

            if (!ChooseColor(ref cc))
            {
                Marshal.FreeHGlobal(cc.lpCustColors);
                return null;
            }

            byte r = (byte)(cc.rgbResult & 0xFF);
            byte g = (byte)((cc.rgbResult >> 8) & 0xFF);
            byte b = (byte)((cc.rgbResult >> 16) & 0xFF);

            Marshal.FreeHGlobal(cc.lpCustColors);

            return Color.FromRgb(r, g, b);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (_appSettings.ExtendedLogging)
                WriteToTerminal("Patching Window..");

            var hwnd = new WindowInteropHelper(this).Handle;

            if (_appSettings.ExtendedLogging)
                WriteToTerminal($"HWND ->: {hwnd}");

            HwndSource.FromHwnd(hwnd)?.AddHook(WndProc);

            EnableDwmNonClientRendering(hwnd);

            int style = GetWindowLong(hwnd, GWL_STYLE);

            style |= WS_SYSMENU | WS_CAPTION | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_THICKFRAME;
            SetWindowLong(hwnd, GWL_STYLE, style);
           // EnableDropShadow(hwnd);

            if (_appSettings.ExtendedLogging)
                WriteToTerminal("HWND patched");
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            var hwnd = new WindowInteropHelper(this).Handle;

            if (_appSettings.ExtendedLogging)
                WriteToTerminal("HWND_OnRendered triggered");

            if (IsWindows11OrGreater())
            {
                SetWindowCornerPreference(hwnd);
                // EnableDropShadow(hwnd);
            }
            else
            {
              //  EnableDropShadow(hwnd);
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            var hwnd = new WindowInteropHelper(this).Handle;

            if (_appSettings.ExtendedLogging)
                WriteToTerminal("HWND_StateChanged triggered");

            if (IsWindows11OrGreater())
            {
                SetWindowCornerPreference(hwnd);
            }

            Task.Delay(50).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() => EnableDropShadow(hwnd));
            });
        }

    
        internal enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DWMWCP_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3
        }

        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private static bool IsWindows11OrGreater()
        {
            var version = Environment.OSVersion.Version;
            return (version.Major >= 10 && version.Build >= 22000);
        }

        private void SetWindowCornerPreference(IntPtr hwnd)
        {
            if (!IsWindows11OrGreater())
                return;

            int preference = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
            DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
        }


        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_GETMINMAXINFO:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
                case WM_DPICHANGED:
                    var newRect = Marshal.PtrToStructure<RECT>(lParam);
                    SetWindowPos(hwnd, IntPtr.Zero,
                        newRect.left, newRect.top,
                        newRect.right - newRect.left,
                        newRect.bottom - newRect.top,
                        SWP_NOZORDER | SWP_NOACTIVATE);
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }

        private void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            var mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                GetMonitorInfo(monitor, ref monitorInfo);

                var workArea = monitorInfo.rcWork;
                var monitorArea = monitorInfo.rcMonitor;

                uint dpiX = 96, dpiY = 96;
                GetDpiForMonitor(monitor, Monitor_DPI_Type.MDT_EFFECTIVE_DPI, out dpiX, out dpiY);
                double scaleX = dpiX / 96.0;
                double scaleY = dpiY / 96.0;

                int frameX = (int)(SystemParameters.WindowNonClientFrameThickness.Left * scaleX);
                int frameY = (int)(SystemParameters.WindowNonClientFrameThickness.Top * scaleY);

                mmi.ptMaxPosition.x = workArea.left;
                mmi.ptMaxPosition.y = workArea.top;
                mmi.ptMaxSize.x = workArea.right - workArea.left;
                mmi.ptMaxSize.y = (workArea.bottom - workArea.top) - frameY + 25;

            }

            mmi.ptMinTrackSize.x = 750;
            mmi.ptMinTrackSize.y = 498;

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        private void EnableDropShadow(IntPtr hwnd)
        {
            int style = GetClassLong(hwnd, GCL_STYLE);
            style |= CS_DROPSHADOW;
            SetClassLong(hwnd, GCL_STYLE, style);
        }


        #endregion

        #region Win32 Smooth Scrolling

        public class NativeScrollInterceptor
        {
            private readonly ScrollViewer _scrollViewer;
            private double _targetOffset;

            public NativeScrollInterceptor(ScrollViewer scrollViewer, Window window)
            {
                _scrollViewer = scrollViewer;
                _targetOffset = scrollViewer.VerticalOffset;

                var hwnd = new WindowInteropHelper(window).EnsureHandle();
                HwndSource source = HwndSource.FromHwnd(hwnd);
                source.AddHook(WndProc);

                CompositionTarget.Rendering += OnRender;
            }

            private const int WM_MOUSEWHEEL = 0x020A;

            private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                if (msg == WM_MOUSEWHEEL)
                {
                    int delta = unchecked((short)((wParam.ToInt64() >> 16) & 0xFFFF));
                    _targetOffset -= delta;

                    _targetOffset = Math.Max(0, Math.Min(_scrollViewer.ScrollableHeight, _targetOffset));
                    handled = true;
                }

                return IntPtr.Zero;
            }

            private void OnRender(object? sender, EventArgs e)
            {
                double current = _scrollViewer.VerticalOffset;
                double smoothFactor = 0.2;
                double newOffset = current + (_targetOffset - current) * smoothFactor;

                if (Math.Abs(newOffset - current) > 0.1)
                {
                    _scrollViewer.ScrollToVerticalOffset(newOffset);
                }
            }
        }

        #endregion

        #region Win32 API & Structs

        /// <summary>
        /// Defines Win32 API functions, constants, and structs used for DWM HWND patches and window styling 
        /// </summary>

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x00080000;
        private const int WS_CAPTION = 0x00C00000;
        private const int CS_DROPSHADOW = 0x00020000;
        private const int GCL_STYLE = -26;
        private const int WS_THICKFRAME = 0x00040000;
        private const int WS_MINIMIZEBOX = 0x00020000;
        private const int WS_MAXIMIZEBOX = 0x00010000;

        private const int DWMWA_NCRENDERING_POLICY = 2;
        private const int DWMNCRP_ENABLED = 2;

        private const int WM_GETMINMAXINFO = 0x0024;
        private const int WM_DPICHANGED = 0x02E0;
        private const int MONITOR_DEFAULTTONEAREST = 0x00000002;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint WDA_NONE = 0x00000000;
        private const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

        private bool SetExcludeFromCapture(IntPtr hwnd, bool enable)
        {
            uint affinity = enable ? WDA_EXCLUDEFROMCAPTURE : WDA_NONE;

            if (_appSettings.ExtendedLogging)
                WriteToTerminal($"Setting Window Affinity -> {(enable ? "ALL" : "NONE")}");

            return SetWindowDisplayAffinity(hwnd, affinity);
        }

        /// <summary>
        /// DLL Imports used for Win32 API calls and DWM HWND patches 
        /// </summary>

        [DllImport("user32.dll")]
        private static extern bool SetWindowDisplayAffinity(IntPtr hwnd, uint dwAffinity);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern bool Rectangle(IntPtr hdc, int left, int top, int right, int bottom);

        [DllImport("user32.dll")]
        private static extern int GetClassLong(IntPtr hwnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetClassLong(IntPtr hwnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, Monitor_DPI_Type dpiType, out uint dpiX, out uint dpiY);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);

        [StructLayout(LayoutKind.Sequential)]
        public struct CHOOSECOLOR
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public int rgbResult;
            public IntPtr lpCustColors;
            public int Flags;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public IntPtr lpTemplateName;
        }

        [DllImport("comdlg32.dll", CharSet = CharSet.Auto)]
        private static extern bool ChooseColor(ref CHOOSECOLOR cc);

        private enum Monitor_DPI_Type { MDT_EFFECTIVE_DPI = 0 }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int x, y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int left, top, right, bottom; }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        private void EnableDwmNonClientRendering(IntPtr hwnd)
        {
            int val = DWMNCRP_ENABLED;
            DwmSetWindowAttribute(hwnd, DWMWA_NCRENDERING_POLICY, ref val, sizeof(int));
        }

        #endregion

        #endregion

    }
}