using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Azure.Identity;
using System.ComponentModel.DataAnnotations;

namespace ServiceCenter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            using (var context = new ApplicationDbContext())
            {
                var requests = context.Users
                .Include(r => r.Employees) // Загружаем связанные данные клиента
                .ToList();

                var user = context.Users.FirstOrDefault(u => u.Username == username);

                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    MainWindow mainWindow = new MainWindow(user.Employees.Role); // Передаем роль
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверное имя пользователя или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

}

    public class Users
    {
    [Key]
    public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public Employees Employees { get; set; }
        public int? EmployeeID { get; set; }
    }

public class Employees
{
    [Key]
    public int EmployeeID { get; set; }
    public string name { get; set; }
    public string Role { get; set; }
    public Users Users { get; set; }
}

   public class Clients
{
    [Key]
    public int ClientID  { get; set; }
    public string ClientName { get; set; }
    public string phone { get; set; }
    public ICollection<Request> Requests { get; set; } // навигационное свойство
}

    public class Request
    {
    [Key]
    public int RequestID { get; set; }
        public string DeviceName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime RequestDate { get; set; }
        public Clients Clients { get; set; }
        public string Phone { get; set; }
        public int ClientID { get; set; }

        
}

    public class ApplicationDbContext : DbContext
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<Clients> Clients { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Employees> Employees { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=DESKTOP-JSPDFF8\MSSQLSERVER1;Initial Catalog=ServiceCenter1;Integrated Security=True;Trust Server Certificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        modelBuilder.Entity<Request>()
        .HasOne(r => r.Clients)    // У Request есть один Client
        .WithMany(c => c.Requests) // У Client много Requests
        .HasForeignKey(r => r.ClientID); // Внешний ключ в Request

        modelBuilder.Entity<Users>()
            .HasOne(u => u.Employees)      // У User есть один Employee
            .WithOne(e => e.Users)         // У Employee есть один User
            .HasForeignKey<Users>(u => u.EmployeeID);
        }
}

