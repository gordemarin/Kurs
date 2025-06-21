using Microsoft.EntityFrameworkCore;
using ServiceCenter.Models;
using System;
using System.Linq;

namespace ServiceCenter.Data
{
    /// <summary>
    /// Обновленный контекст базы данных для 3NF структуры
    /// </summary>
    public class UpdatedApplicationDbContext : DbContext
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
                .ThenInclude(e => e.Role)
                .Select(r => new RequestFullInfo
                {
                    RequestID = r.RequestID,
                    DeviceName = r.DeviceName,
                    Description = r.Description,
                    StatusName = r.Status.StatusName,
                    StatusColor = r.Status.Color,
                    RequestDate = r.RequestDate,
                    CompletedDate = r.CompletedDate,
                    Priority = r.Priority,
                    ClientName = r.Client.FullName,
                    ClientPhone = r.Client.Phone,
                    ClientEmail = r.Client.Email,
                    AssignedEmployee = r.AssignedEmployee != null ? r.AssignedEmployee.FullName : null,
                    EmployeeRole = r.AssignedEmployee?.Role?.RoleName
                });
        }

        /// <summary>
        /// Получить пользователей с ролями
        /// </summary>
        public IQueryable<UserWithRole> GetUsersWithRoles()
        {
            return Users
                .Include(u => u.Employee)
                .ThenInclude(e => e.Role)
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