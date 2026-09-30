using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MemoryGame.Commands;
using MemoryGame.Model;
using MemoryGame.Services;
using MemoryGame.View;

namespace MemoryGame.ViewModel
{
    public class SignInViewModel : INotifyPropertyChanged
    {
        private readonly UserDataService _userDataService;

        #region Constructor
        public SignInViewModel()
        {
            _userDataService = new UserDataService();

            _avatarFiles = Directory.Exists(AvatarPath)
                ? Directory.GetFiles(AvatarPath, "*.jpg")
                    .Concat(Directory.GetFiles(AvatarPath, "*.png"))
                    .Concat(Directory.GetFiles(AvatarPath, "*.jpeg"))
                    .ToList()
                : new List<string>();
            _currentAvatarIndex = 0;
            LoadCurrentAvatar();

            Users = _userDataService.LoadUsers();

            SelectedUser = Users.FirstOrDefault();

            NextAvatarCommand = new RelayCommand(NextAvatar, CanNavigateAvatar);
            PreviousAvatarCommand = new RelayCommand(PreviousAvatar, CanNavigateAvatar);
            PlayCommand = new RelayCommand(Play, CanPlayGame);
            NewUserCommand = new RelayCommand(ShowNewUserDialog);
            DeleteUserCommand = new RelayCommand(ShowDeleteConfirmation, CanDeleteUser);
            ExitCommand = new RelayCommand(Exit);
            SaveNewUserCommand = new RelayCommand(SaveNewUser, CanSaveNewUser);
            CancelNewUserCommand = new RelayCommand(CancelNewUser);
            ConfirmDeleteUserCommand = new RelayCommand(ConfirmDeleteUser);
            CancelDeleteUserCommand = new RelayCommand(CancelDeleteUser);
        }
        #endregion

        #region Properties
        private Visibility _newUserDialogVisibility = Visibility.Collapsed;
        public Visibility NewUserDialogVisibility
        {
            get => _newUserDialogVisibility;
            set
            {
                _newUserDialogVisibility = value;
                OnPropertyChanged();
            }
        }

        private string _newUsername;
        public string NewUsername
        {
            get => _newUsername;
            set
            {
                _newUsername = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private Visibility _deleteConfirmationVisibility = Visibility.Collapsed;
        public Visibility DeleteConfirmationVisibility
        {
            get => _deleteConfirmationVisibility;
            set
            {
                _deleteConfirmationVisibility = value;
                OnPropertyChanged();
            }
        }

        private string _deleteConfirmationMessage;
        public string DeleteConfirmationMessage
        {
            get => _deleteConfirmationMessage;
            set
            {
                _deleteConfirmationMessage = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region User Management
        private User _selectedUser;
        public ObservableCollection<User> Users { get; }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand NewUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand SaveNewUserCommand { get; }
        public ICommand CancelNewUserCommand { get; }
        public ICommand ConfirmDeleteUserCommand { get; }
        public ICommand CancelDeleteUserCommand { get; }

        private void Exit()
        {
            Application.Current.Shutdown();
        }

        #region New User Methods
        private void ShowNewUserDialog()
        {
            NewUsername = string.Empty;
            NewUserDialogVisibility = Visibility.Visible;
        }

        private void CancelNewUser()
        {
            NewUserDialogVisibility = Visibility.Collapsed;
        }

        private bool CanSaveNewUser()
        {
            return !string.IsNullOrWhiteSpace(NewUsername);
        }

        private void SaveNewUser()
        {
            if (string.IsNullOrWhiteSpace(NewUsername))
                return;

            var username = NewUsername.Trim();

            if (Users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("A user with this name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string avatarPath = _avatarFiles.Count > 0 ? _avatarFiles[_currentAvatarIndex] : string.Empty;
            var newUser = new User(username, avatarPath);
            Users.Add(newUser);
            SelectedUser = newUser;

            _userDataService.SaveUser(newUser);

            NewUserDialogVisibility = Visibility.Collapsed;
        }
        #endregion

        #region Delete User Methods
        private void ShowDeleteConfirmation()
        {
            if (SelectedUser == null) return;

            DeleteConfirmationMessage = $"Are you sure you want to delete user '{SelectedUser.Username}'?";
            DeleteConfirmationVisibility = Visibility.Visible;
        }

        private void CancelDeleteUser()
        {
            DeleteConfirmationVisibility = Visibility.Collapsed;
        }

        private void ConfirmDeleteUser()
        {
            if (SelectedUser == null) return;

            string username = SelectedUser.Username;
            Users.Remove(SelectedUser);
            SelectedUser = Users.FirstOrDefault();

            _userDataService.DeleteUser(username);

            DeleteConfirmationVisibility = Visibility.Collapsed;
        }

        private bool CanDeleteUser()
        {
            return SelectedUser != null;
        }
        #endregion
        #endregion

        #region Avatar Navigation
        private static string AvatarPath => Path.Combine(AppContext.BaseDirectory, "res", "images", "avatars");
        private List<string> _avatarFiles;
        private int _currentAvatarIndex;
        private BitmapImage _currentAvatar;
        public BitmapImage CurrentAvatar
        {
            get => _currentAvatar;
            private set
            {
                _currentAvatar = value;
                OnPropertyChanged();
            }
        }
        public ICommand NextAvatarCommand { get; }
        public ICommand PreviousAvatarCommand { get; }
        private void LoadCurrentAvatar()
        {
            if (_avatarFiles == null || _avatarFiles.Count == 0)
            {
                CurrentAvatar = null;
                return;
            }
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(_avatarFiles[_currentAvatarIndex], UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                CurrentAvatar = bitmap;
            }
            catch (Exception ex)
            {
                CurrentAvatar = null;
            }
        }
        private void NextAvatar()
        {
            _currentAvatarIndex = (_currentAvatarIndex + 1) % _avatarFiles.Count;
            LoadCurrentAvatar();
            UpdateSelectedUserAvatar();
        }
        private void PreviousAvatar()
        {
            _currentAvatarIndex = (_currentAvatarIndex - 1 + _avatarFiles.Count) % _avatarFiles.Count;
            LoadCurrentAvatar();
            UpdateSelectedUserAvatar();
        }
        private bool CanNavigateAvatar()
        {
            return _avatarFiles != null && _avatarFiles.Count > 1;
        }

        private void UpdateSelectedUserAvatar()
        {
            if (SelectedUser != null && _avatarFiles != null && _avatarFiles.Count > 0)
            {
                SelectedUser.AvatarPath = _avatarFiles[_currentAvatarIndex];

                _userDataService.SaveUser(SelectedUser);
            }
        }
        #endregion

        #region Play
        public ICommand PlayCommand { get; }
        private bool CanPlayGame()
        {
            return SelectedUser != null;
        }
        private void Play()
        {
            if (SelectedUser == null)
            {
                MessageBox.Show("Please select a user to play.", "No User Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            App.Current.Properties["CurrentUser"] = SelectedUser;

            var signInWindow = Application.Current.Windows
                    .OfType<SignInWindow>()
                    .FirstOrDefault();
            MenuWindow menuWindow = new MenuWindow();
            menuWindow.Show();
            signInWindow.Close();
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}