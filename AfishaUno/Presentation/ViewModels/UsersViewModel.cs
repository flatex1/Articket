using System.Collections.ObjectModel;
using System.Windows.Input;
using AfishaUno.Models;
using AfishaUno.Services;

namespace AfishaUno.Presentation.ViewModels
{
    public class UsersViewModel : ObservableObject
    {
        private readonly ISupabaseService _supabaseService;
        private readonly INavigationService _navigationService;
        public ObservableCollection<User> Users { get; } = new();
        public User SelectedUser { get; set; }

        // Для диалога
        public bool IsDialogOpen { get; set; }
        public string DialogTitle { get; set; }
        public User EditingUser { get; set; } = new User();
        public bool IsPasswordVisible { get; set; }
        public string NewUserPassword { get; set; }

        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand SaveUserCommand { get; }
        public ICommand CancelDialogCommand { get; }
        public ICommand GoBackCommand { get; }

        public UsersViewModel(ISupabaseService supabaseService, INavigationService navigationService = null)
        {
            _supabaseService = supabaseService;
            _navigationService = navigationService ?? (Application.Current as App)?.Services.GetService(typeof(INavigationService)) as INavigationService;
            AddUserCommand = new RelayCommand(OnAddUser);
            EditUserCommand = new RelayCommand<User>(OnEditUser);
            DeleteUserCommand = new RelayCommand<User>(OnDeleteUser);
            SaveUserCommand = new RelayCommand(OnSaveUser);
            CancelDialogCommand = new RelayCommand(OnCancelDialog);
            GoBackCommand = new RelayCommand(OnGoBack);
            LoadUsers();
        }

        private async void LoadUsers()
        {
            var users = await _supabaseService.GetUsersAsync();
            Users.Clear();
            foreach (var user in users)
                Users.Add(user);
        }

        private void OnAddUser()
        {
            EditingUser = new User();
            DialogTitle = "Добавить пользователя";
            IsPasswordVisible = true;
            IsDialogOpen = true;
            OnPropertyChanged(nameof(EditingUser));
            OnPropertyChanged(nameof(DialogTitle));
            OnPropertyChanged(nameof(IsPasswordVisible));
            OnPropertyChanged(nameof(IsDialogOpen));
        }

        private void OnEditUser(User user)
        {
            if (user == null) return;
            EditingUser = new User
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };
            DialogTitle = "Редактировать пользователя";
            IsPasswordVisible = false;
            IsDialogOpen = true;
            OnPropertyChanged(nameof(EditingUser));
            OnPropertyChanged(nameof(DialogTitle));
            OnPropertyChanged(nameof(IsPasswordVisible));
            OnPropertyChanged(nameof(IsDialogOpen));
        }

        private async void OnDeleteUser(User user)
        {
            if (user == null) return;
            await _supabaseService.DeleteUserAsync(user.Id);
            Users.Remove(user);
        }

        private async void OnSaveUser()
        {
            if (EditingUser.Id == Guid.Empty)
            {
                // Добавление
                var newUser = await _supabaseService.AddUserAsync(EditingUser.Email, NewUserPassword, EditingUser.FullName, EditingUser.Role);
                Users.Add(newUser);
                NewUserPassword = string.Empty;
                OnPropertyChanged(nameof(NewUserPassword));
            }
            else
            {
                // Редактирование
                await _supabaseService.UpdateUserAsync(EditingUser);
                var existing = Users.FirstOrDefault(u => u.Id == EditingUser.Id);
                if (existing != null)
                {
                    existing.FullName = EditingUser.FullName;
                    existing.Email = EditingUser.Email;
                    existing.Role = EditingUser.Role;
                    OnPropertyChanged(nameof(Users));
                }
            }
            IsDialogOpen = false;
            OnPropertyChanged(nameof(IsDialogOpen));
        }

        private void OnCancelDialog()
        {
            IsDialogOpen = false;
            OnPropertyChanged(nameof(IsDialogOpen));
        }

        private void OnGoBack()
        {
            _navigationService?.GoBack();
        }
    }
} 