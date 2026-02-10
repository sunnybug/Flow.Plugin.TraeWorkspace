using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Flow.Launcher.Plugin;

namespace Flow.Plugin.TraeWorkspace
{
    public partial class SettingsView : UserControl
    {
        private readonly PluginInitContext _context;
        private readonly Settings _settings;

        public SettingsView(PluginInitContext context, Settings settings)
        {
            InitializeComponent();
            _context = context;
            _settings = settings;
            LoadSettings();
            SetupEventHandlers();
        }

        private void LoadSettings()
        {
            DiscoverWorkspacesCheckBox.IsChecked = _settings.DiscoverWorkspaces;
            DiscoverMachinesCheckBox.IsChecked = _settings.DiscoverMachines;
            CustomWorkspacesListBox.ItemsSource = _settings.CustomWorkspaces;
        }

        private void SetupEventHandlers()
        {
            DiscoverWorkspacesCheckBox.Checked += (sender, e) =>
            {
                _settings.DiscoverWorkspaces = true;
                _context.API.SaveSettingJsonStorage<Settings>();
            };

            DiscoverWorkspacesCheckBox.Unchecked += (sender, e) =>
            {
                _settings.DiscoverWorkspaces = false;
                _context.API.SaveSettingJsonStorage<Settings>();
            };

            DiscoverMachinesCheckBox.Checked += (sender, e) =>
            {
                _settings.DiscoverMachines = true;
                _context.API.SaveSettingJsonStorage<Settings>();
            };

            DiscoverMachinesCheckBox.Unchecked += (sender, e) =>
            {
                _settings.DiscoverMachines = false;
                _context.API.SaveSettingJsonStorage<Settings>();
            };

            AddWorkspaceButton.Click += (sender, e) =>
            {
                var dialog = new OpenFolderDialog
                {
                    Title = "选择工作区文件夹"
                };

                if (dialog.ShowDialog() == true)
                {
                    _settings.CustomWorkspaces.Add(dialog.FolderName);
                    CustomWorkspacesListBox.ItemsSource = null;
                    CustomWorkspacesListBox.ItemsSource = _settings.CustomWorkspaces;
                    _context.API.SaveSettingJsonStorage<Settings>();
                }
            };

            RemoveWorkspaceButton.Click += (sender, e) =>
            {
                if (CustomWorkspacesListBox.SelectedItem is string selectedWorkspace)
                {
                    _settings.CustomWorkspaces.Remove(selectedWorkspace);
                    CustomWorkspacesListBox.ItemsSource = null;
                    CustomWorkspacesListBox.ItemsSource = _settings.CustomWorkspaces;
                    _context.API.SaveSettingJsonStorage<Settings>();
                }
            };
        }
    }
}