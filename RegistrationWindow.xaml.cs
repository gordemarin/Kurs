using System;
using System.Linq;
using System.Windows;

namespace ServiceCenter
{
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var firstName = FirstNameTextBox.Text;
            var lastName = LastNameTextBox.Text;
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;
            var confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new ApplicationDbContext())
            {
                if (context.Users.Any(u => u.Username == username))
                {
                    MessageBox.Show("Пользователь с таким именем уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newEmployee = new Employee
                {
                    FirstName = firstName,
                    LastName = lastName,
                    RoleID = RoleConstants.Employee, // Базовая роль
                    IsActive = true,
                    HireDate = DateTime.Now
                };
                context.Employees.Add(newEmployee);
                context.SaveChanges(); 

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                var newUser = new User
                {
                    Username = username,
                    PasswordHash = passwordHash,
                    EmployeeID = newEmployee.EmployeeID
                };
                context.Users.Add(newUser);
                context.SaveChanges();

                MessageBox.Show("Регистрация прошла успешно! Теперь вы можете войти.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
