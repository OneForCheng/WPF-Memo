using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Notes.Behaviors;
using Notes.Config;
using Notes.Enum;
using Button = System.Windows.Controls.Button;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Notes.Windows
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow 
    {
        #region Private properties
        private const string ConfigPathKey = "UserConfigPath";
        private const string DataFilePathKey = "DataFilePath";
        private const double InitHeight = 35;
        private const double Tolerance = 0.1;

        private double _initedHeight;
        private bool _directExit;
        private System.Windows.Forms.NotifyIcon _trayIcon;
        private double _autoHideFactor;
        private AutoHideWindowBehavior _autoHideBehavior;
        private bool _canTextChanged;
        private bool _textChanged;
        private Note _selectedNote;
        #endregion

        #region Public methods

        public double AutoHideFactor
        {
            get { return _autoHideFactor; }
            set
            {
                if (value > 0)
                {
                    _autoHideFactor = value;
                }
            }
        }

        public UserConfig UserConfigution { get; private set; }

        public ObservableCollection<Note> NoteViewModels { get; }

        public NotesContext DbContext { get; }

        #endregion

        #region Constructor
        public MainWindow()
        {
           
            InitializeComponent();

            _directExit = false;
            _canTextChanged = true;
            _autoHideFactor = 10.0;
            NoteViewModels = new ObservableCollection<Note>();
            DbContext = new NotesContext();

            //加载用户配置
            LoadConfig();
        }

        #endregion

        #region Private events

        #region 主窗口事件

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.DisableMaxmize(true); //禁用窗口最大化功能
            this.RemoveSystemMenuItems(Win32.SystemMenuItems.Restore | Win32.SystemMenuItems.Minimize | Win32.SystemMenuItems.Maximize | Win32.SystemMenuItems.SpliteLine); //去除窗口指定的系统菜单

            _autoHideBehavior = new AutoHideWindowBehavior
            {
                AutoHideFactor = AutoHideFactor,
            };
            _autoHideBehavior.Attach(this);

            _trayIcon = InitTrayIcon();//初始化系统托盘

            NoteTextArea.Focus();//文本输入框获取焦点
            DbContext.Load(ConfigurationManager.AppSettings[DataFilePathKey]);//加载记录数据

            //筛选未完成的记录
            foreach (var item in DbContext.Notes.Where(m => !m.Deleted && !m.Finished))
            {
                NoteViewModels.Add(item);
            }

            //绑定数据
            ListView.ItemsSource = NoteViewModels;

            this.SetPropertyAnimation(HeightProperty, new DoubleAnimation(InitHeight, _initedHeight, new Duration(TimeSpan.FromSeconds(2))));
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.N:
                        Execute(Actions.New);
                        break;
                    case Key.S:
                        Execute(Actions.Save);
                        break;
                    case Key.F:
                        Execute(Actions.Finish);
                        break;
                }
            }
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if(_directExit)return;
            e.Cancel = true;
            _trayIcon?.Dispose();
            SaveConfig();
            this.SetPropertyAnimation(HeightProperty, new DoubleAnimation(Height, InitHeight, new Duration(TimeSpan.FromSeconds(0.5))),
                () =>
                {
                    _directExit = true;
                    Close();
                });
        }

        private void DragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        #endregion

        #region 其他控件事件

        private void TitleLbl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Height = 540;
                Width = Math.Abs(Width - 300) < Tolerance ? 700 : 300;
            }
        }

        private void ActionBtnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var action = (Actions)button.Tag;
            Execute(action);
        }

        private void TrayIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _autoHideBehavior.Show();
            }
        }

        private void NoteTextArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_canTextChanged) return;
            _textChanged = true;
            UpdateState();
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DealSelectionEvent();
            
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DealSelectionEvent();
        }

        #endregion

        #endregion

        #region Public methods

        #endregion

        #region Private methods

        private void LoadConfig()
        {
            UserConfigution = new UserConfig();
            UserConfigution.Load(ConfigurationManager.AppSettings[ConfigPathKey]);

            Width = UserConfigution.WindowState.Width < MinWidth ? MinWidth : UserConfigution.WindowState.Width;
            Height = InitHeight;
            _initedHeight = UserConfigution.WindowState.Height < MinHeight ? MinHeight : UserConfigution.WindowState.Height;

            Top = UserConfigution.WindowState.Top;
            Left = UserConfigution.WindowState.Left;
            AutoHideFactor = UserConfigution.WindowState.AutoHideFactor;
        }

        private void SaveConfig()
        {
            UserConfigution.WindowState.Width = Width;
            UserConfigution.WindowState.Height = Height;
            UserConfigution.WindowState.Top = Top;
            UserConfigution.WindowState.Left = Left;
            UserConfigution.WindowState.AutoHideFactor = AutoHideFactor;
            UserConfigution?.SaveChanged();
        }

        private void Execute(Actions action)
        {
            switch (action)
            {
                case Actions.New:
                    if (NewBtn.IsEnabled)
                    {
                        SetNewState();
                    }
                    break;
                case Actions.Save:
                    if (SaveBtn.IsEnabled)
                    {
                        var text = "";
                        var strs = NoteTextArea.Text.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        for (var i = 0; i < strs.Length - 1; i++)
                        {
                            var str = strs[i].Trim();
                            if (str.Length != 0)
                            {
                                text += str + Environment.NewLine;
                            }
                        }
                        text += strs[strs.Length - 1];
                        if (ListView.SelectedIndex == -1)
                        {
                            var newId = DbContext.Notes.Count == 0 ? 1 : DbContext.Notes.Max(m => m.Id) + 1;
                            _selectedNote = new Note(newId, text);
                            DbContext.Notes.Add(_selectedNote);
                          
                            NoteViewModels.Add(_selectedNote);
                            ListView.SelectedIndex = NoteViewModels.Count - 1;
                        }
                        else
                        {
                            _selectedNote.Content = text;
                            _selectedNote.LastModTime = DateTime.Now;
                            
                        }
                        DbContext.SaveChanged();
                        _textChanged = false;
                        UpdateState();
                    }
                    break;
                case Actions.Finish:
                    if (FinishBtn.IsEnabled)
                    {

                        NoteViewModels.RemoveAt(ListView.SelectedIndex);

                        _selectedNote.Finished = true;
                        _selectedNote.LastModTime = DateTime.Now;
                        DbContext.SaveChanged();

                        SetNewState();
                    }
                    break;
                case Actions.Delete:
                    if (DeleteBtn.IsEnabled)
                    {

                        NoteViewModels.RemoveAt(ListView.SelectedIndex);

                        _selectedNote.Deleted = true;
                        _selectedNote.LastModTime = DateTime.Now;
                        DbContext.SaveChanged();

                        SetNewState();
                    }
                    break;
                case Actions.Close:
                    Close();
                    break;
            }
        }

        private System.Windows.Forms.NotifyIcon InitTrayIcon()
        {
            var appName = Properties.Resources.ApplicationName;
            var trayIcon = new System.Windows.Forms.NotifyIcon()
            {
                BalloonTipText = appName,
                Text = appName,
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath) /*读取程序图标，来作为托盘图标*/
            };

            trayIcon.MouseClick += TrayIcon_MouseClick;

            var contextMenu = new System.Windows.Forms.ContextMenuStrip
            {
                BackColor = Color.FromArgb(255,62,62,66),
                ForeColor = Color.White,
            };
            
            var item = new System.Windows.Forms.ToolStripMenuItem("查看日志")
            {
                BackgroundImage = Properties.Resources.BackIcon,
                Image = Properties.Resources.LookIcon,
                ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.SizeToFit,
                
            };
 
            item.Click += (sender, e) =>
            {
                var path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, @"Log\NotesLog.xml");
                if (File.Exists(path))
                {
                    Process.Start("notepad.exe", path);
                }
                else
                {
                    Extentions.ShowMessageBox("找不到日志文件!");
                }
            };
            contextMenu.Items.Add(item);

            item = new System.Windows.Forms.ToolStripMenuItem("退出")
            {
                BackgroundImage = Properties.Resources.BackIcon,
                Image = Properties.Resources.CloseIcon,
                ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.SizeToFit,
            };
            item.Click += (sender, e) => { Close(); };
            contextMenu.Items.Add(item);

            trayIcon.ContextMenuStrip = contextMenu;
            trayIcon.ShowBalloonTip(500);//设置显示提示气球时间
            trayIcon.Visible = true;

            return trayIcon;
        }

        private void SetNewState()
        {
            ListView.SelectedIndex = -1;
            _canTextChanged = false;
            NoteTextArea.Text = "";
            _canTextChanged = true;
            _textChanged = false;
            UpdateState();
        }

        private void UpdateState()
        {
            TitleLbl.Content = ListView.SelectedIndex == -1 ? "新建 - Notes" : $"记录{_selectedNote.Id} - Notes";
            if (ListView.SelectedIndex == -1)
            {
                FinishBtn.IsEnabled =
                DeleteBtn.IsEnabled = false;
                SaveBtn.IsEnabled = _textChanged && NoteTextArea.Text.Trim().Length != 0;
            }
            else
            {
                if (_selectedNote.Deleted)
                {
                    NoteTextArea.IsEnabled = false;
                    SaveBtn.IsEnabled =
                    FinishBtn.IsEnabled =
                    DeleteBtn.IsEnabled = false;
                }
                else
                {
                    if (!NoteTextArea.IsEnabled) NoteTextArea.IsEnabled = true;
                    SaveBtn.IsEnabled = _textChanged && NoteTextArea.Text.Trim().Length != 0;
                    DeleteBtn.IsEnabled = true;
                    if (_selectedNote.Finished)
                    {
                        FinishBtn.IsEnabled = false;
                    }
                    else
                    {
                        FinishBtn.IsEnabled = !_textChanged;
                    }
                }
            }
        }

        private void DealSelectionEvent()
        {
            if(ListView.SelectedIndex == -1)return;
            if (_selectedNote != null && _selectedNote.Id == NoteViewModels[ListView.SelectedIndex].Id && !_textChanged) return;
            _selectedNote = NoteViewModels[ListView.SelectedIndex];
            _canTextChanged = false;
            NoteTextArea.Text = _selectedNote.Content;
            _canTextChanged = true;
            _textChanged = false;
            UpdateState();
        }

        #endregion

       
    }
}
