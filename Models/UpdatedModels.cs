using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenter.Models
{
    // =============================================
    // ОБНОВЛЕННЫЕ МОДЕЛИ ДЛЯ 3NF СТРУКТУРЫ БД
    // =============================================

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
} 