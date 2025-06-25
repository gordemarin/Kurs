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
using System.ComponentModel.DataAnnotations.Schema;

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
                var user = context.Users
                    .Include(u => u.Employee)
                    .ThenInclude(emp => emp.Role)
                    .FirstOrDefault(u => u.Username == username);

                if (user != null && user.Employee != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    MainWindow mainWindow = new MainWindow(user.Employee.Role.RoleName); // Передаем роль
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверное имя пользователя или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
            this.Close();
        }
    }

    /// <summary>
    /// Роли пользователей в системе
    /// </summary>
    public class Role
    {
        [Key]
        public int RoleID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string RoleName { get; set; }
        
        [StringLength(255)]
        public string Description { get; set; }
        
        public string Permissions { get; set; }
        
        // Навигационные свойства
        public virtual ICollection<Employee> Employees { get; set; }
    }

    /// <summary>
    /// Статусы заявок
    /// </summary>
    public class RequestStatus
    {
        [Key]
        public int StatusID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string StatusName { get; set; }
        
        [StringLength(255)]
        public string Description { get; set; }
        
        [StringLength(7)]
        public string Color { get; set; } = "#000000";
        
        // Навигационные свойства
        public virtual ICollection<Request> Requests { get; set; }
    }

    /// <summary>
    /// Сотрудники сервисного центра
    /// </summary>
    public class Employee
    {
        [Key]
        public int EmployeeID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }
        
        [StringLength(20)]
        public string Phone { get; set; }
        
        [Required]
        public int RoleID { get; set; }
        
        public DateTime HireDate { get; set; } = DateTime.Now;
        
        public bool IsActive { get; set; } = true;
        
        // Вычисляемое свойство
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
        
        // Навигационные свойства
        [ForeignKey("RoleID")]
        public virtual Role Role { get; set; }
        
        public virtual User User { get; set; }
        
        public virtual ICollection<Request> AssignedRequests { get; set; }
    }

    /// <summary>
    /// Клиенты сервисного центра
    /// </summary>
    public class Client
    {
        [Key]
        public int ClientID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Phone { get; set; }
        
        [StringLength(255)]
        public string Address { get; set; }
        
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        
        // Вычисляемое свойство
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
        
        // Навигационные свойства
        public virtual ICollection<Request> Requests { get; set; }
    }

    /// <summary>
    /// Пользователи системы (аутентификация)
    /// </summary>
    public class User
    {
        [Key]
        public int UserID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }
        
        public int? EmployeeID { get; set; }
        
        // Навигационные свойства
        [ForeignKey("EmployeeID")]
        public virtual Employee Employee { get; set; }
    }

    /// <summary>
    /// Заявки на обслуживание
    /// </summary>
    public class Request
    {
        [Key]
        public int RequestID { get; set; }
        
        [Required]
        public int ClientID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DeviceName { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        public int StatusID { get; set; }
        
        public DateTime RequestDate { get; set; } = DateTime.Now;
        
        public DateTime? CompletedDate { get; set; }
        
        public int? AssignedEmployeeID { get; set; }
        
        public int Priority { get; set; } = 1;
        
        // Навигационные свойства
        [ForeignKey("ClientID")]
        public virtual Client Client { get; set; }
        
        [ForeignKey("StatusID")]
        public virtual RequestStatus Status { get; set; }
        
        [ForeignKey("AssignedEmployeeID")]
        public virtual Employee AssignedEmployee { get; set; }
    }

    // =============================================
    // ВСПОМОГАТЕЛЬНЫЕ КЛАССЫ ДЛЯ ПРЕДСТАВЛЕНИЙ
    // =============================================

    /// <summary>
    /// Представление полной информации о заявке
    /// </summary>
    public class RequestFullInfo
    {
        public int RequestID { get; set; }
        public string DeviceName { get; set; }
        public string Description { get; set; }
        public string StatusName { get; set; }
        public string StatusColor { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int Priority { get; set; }
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public string ClientEmail { get; set; }
        public string AssignedEmployee { get; set; }
        public string EmployeeRole { get; set; }
    }

    /// <summary>
    /// Представление пользователя с ролью
    /// </summary>
    public class UserWithRole
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string EmployeeName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public bool IsActive { get; set; }
    }

    // =============================================
    // ENUM ДЛЯ ПРИОРИТЕТОВ
    // =============================================

    public enum RequestPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4
    }

    // =============================================
    // КОНСТАНТЫ ДЛЯ СТАТУСОВ
    // =============================================

    public static class RequestStatusConstants
    {
        public const int New = 1;
        public const int InProgress = 2;
        public const int WaitingForParts = 3;
        public const int Ready = 4;
        public const int Completed = 5;
        public const int Cancelled = 6;
    }

    public static class RoleConstants
    {
        public const int Administrator = 1;
        public const int Employee = 2;
        public const int Manager = 3;
    }

    // =============================================
    // ОБНОВЛЕННЫЙ КОНТЕКСТ БАЗЫ ДАННЫХ
    // =============================================

    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<RequestStatus> RequestStatuses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=DESKTOP-JSPDFF8\MSSQLSERVER1;Initial Catalog=ServiceCenter1;Integrated Security=True;Trust Server Certificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // =============================================
            // КОНФИГУРАЦИЯ СВЯЗЕЙ МЕЖДУ СУЩНОСТЯМИ
            // =============================================

            // Связь User - Employee (один к одному)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Employee)
                .WithOne(e => e.User)
                .HasForeignKey<User>(u => u.EmployeeID)
                .OnDelete(DeleteBehavior.SetNull);

            // Связь Employee - Role (многие к одному)
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Role)
                .WithMany(r => r.Employees)
                .HasForeignKey(e => e.RoleID)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь Request - Client (многие к одному)
            modelBuilder.Entity<Request>()
                .HasOne(r => r.Client)
                .WithMany(c => c.Requests)
                .HasForeignKey(r => r.ClientID)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь Request - RequestStatus (многие к одному)
            modelBuilder.Entity<Request>()
                .HasOne(r => r.Status)
                .WithMany(s => s.Requests)
                .HasForeignKey(r => r.StatusID)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь Request - Employee (многие к одному) - назначенный сотрудник
            modelBuilder.Entity<Request>()
                .HasOne(r => r.AssignedEmployee)
                .WithMany(e => e.AssignedRequests)
                .HasForeignKey(r => r.AssignedEmployeeID)
                .OnDelete(DeleteBehavior.SetNull);

            // =============================================
            // КОНФИГУРАЦИЯ ИНДЕКСОВ
            // =============================================

            // Индексы для производительности
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Phone)
                .IsUnique();

            modelBuilder.Entity<Request>()
                .HasIndex(r => r.RequestDate);

            modelBuilder.Entity<Request>()
                .HasIndex(r => r.StatusID);

            modelBuilder.Entity<Request>()
                .HasIndex(r => r.ClientID);

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.RoleID);

            // =============================================
            // КОНФИГУРАЦИЯ ОГРАНИЧЕНИЙ
            // =============================================

            // Ограничения для ролей
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            // Ограничения для статусов
            modelBuilder.Entity<RequestStatus>()
                .HasIndex(s => s.StatusName)
                .IsUnique();

            // =============================================
            // ЗНАЧЕНИЯ ПО УМОЛЧАНИЮ
            // =============================================

            // Значения по умолчанию для Request
            modelBuilder.Entity<Request>()
                .Property(r => r.RequestDate)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Request>()
                .Property(r => r.Priority)
                .HasDefaultValue(1);

            // Значения по умолчанию для Employee
            modelBuilder.Entity<Employee>()
                .Property(e => e.HireDate)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Employee>()
                .Property(e => e.IsActive)
                .HasDefaultValue(true);

            // Значения по умолчанию для Client
            modelBuilder.Entity<Client>()
                .Property(c => c.RegistrationDate)
                .HasDefaultValueSql("GETDATE()");

            // Значения по умолчанию для RequestStatus
            modelBuilder.Entity<RequestStatus>()
                .Property(s => s.Color)
                .HasDefaultValue("#000000");

            base.OnModelCreating(modelBuilder);
        }

        // =============================================
        // МЕТОДЫ ДЛЯ РАБОТЫ С ДАННЫМИ
        // =============================================

        /// <summary>
        /// Получить полную информацию о заявках
        /// </summary>
        public IQueryable<RequestFullInfo> GetRequestsFullInfo()
        {
            return Requests
                .Include(r => r.Client)
                .Include(r => r.Status)
                .Include(r => r.AssignedEmployee)
                    .ThenInclude(emp => emp.Role)
                .Select(r => new RequestFullInfo
                {
                    RequestID = r.RequestID,
                    DeviceName = r.DeviceName,
                    Description = r.Description,
                    StatusName = r.Status != null ? r.Status.StatusName : "Без статуса",
                    StatusColor = r.Status != null ? r.Status.Color : "#808080",
                    RequestDate = r.RequestDate,
                    CompletedDate = r.CompletedDate,
                    Priority = r.Priority,
                    ClientName = r.Client != null ? r.Client.FullName : "Клиент не найден",
                    ClientPhone = r.Client != null ? r.Client.Phone : "",
                    ClientEmail = r.Client != null ? r.Client.Email : "",
                    AssignedEmployee = r.AssignedEmployee != null ? r.AssignedEmployee.FullName : "Не назначен",
                    EmployeeRole = r.AssignedEmployee != null && r.AssignedEmployee.Role != null ? r.AssignedEmployee.Role.RoleName : ""
                });
        }

        /// <summary>
        /// Получить пользователей с ролями
        /// </summary>
        public IQueryable<UserWithRole> GetUsersWithRoles()
        {
            return Users
                .Include(u => u.Employee)
                .ThenInclude(emp => emp.Role)
                .Where(u => u.Employee != null)
                .Select(u => new UserWithRole
                {
                    UserID = u.UserID,
                    Username = u.Username,
                    EmployeeName = u.Employee.FullName,
                    Email = u.Employee.Email,
                    Phone = u.Employee.Phone,
                    RoleName = u.Employee.Role.RoleName,
                    RoleDescription = u.Employee.Role.Description,
                    IsActive = u.Employee.IsActive
                });
        }

        /// <summary>
        /// Найти или создать клиента по телефону
        /// </summary>
        public Client FindOrCreateClient(string phone, string firstName = "Клиент", string lastName = "")
        {
            var client = Clients.FirstOrDefault(c => c.Phone == phone);
            
            if (client == null)
            {
                client = new Client
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = phone,
                    RegistrationDate = DateTime.Now
                };
                Clients.Add(client);
                SaveChanges();
            }
            
            return client;
        }

        /// <summary>
        /// Обновить статус заявки
        /// </summary>
        public void UpdateRequestStatus(int requestId, int statusId, int? assignedEmployeeId = null)
        {
            var request = Requests.Find(requestId);
            if (request != null)
            {
                request.StatusID = statusId;
                request.AssignedEmployeeID = assignedEmployeeId;
                
                // Автоматически устанавливаем дату завершения для готовых заявок
                if (statusId == RequestStatusConstants.Ready || statusId == RequestStatusConstants.Completed)
                {
                    request.CompletedDate = DateTime.Now;
                }
                
                SaveChanges();
            }
        }

        /// <summary>
        /// Получить статистику по заявкам
        /// </summary>
        public object GetRequestsStatistics()
        {
            var totalRequests = Requests.Count();
            var newRequests = Requests.Count(r => r.StatusID == RequestStatusConstants.New);
            var inProgressRequests = Requests.Count(r => r.StatusID == RequestStatusConstants.InProgress);
            var completedRequests = Requests.Count(r => r.StatusID == RequestStatusConstants.Completed);
            
            return new
            {
                Total = totalRequests,
                New = newRequests,
                InProgress = inProgressRequests,
                Completed = completedRequests,
                CompletionRate = totalRequests > 0 ? (double)completedRequests / totalRequests * 100 : 0
            };
        }
    }
}

