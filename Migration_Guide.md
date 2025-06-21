# 🚀 РУКОВОДСТВО ПО МИГРАЦИИ ДО 3NF СТРУКТУРЫ БД

## 📋 ОБЗОР ИЗМЕНЕНИЙ

### ✅ **ЧТО ИЗМЕНИЛОСЬ:**
- **Количество таблиц**: с 4 до 6 (5 основных + 1 справочная)
- **Нормализация**: приведение к третьей нормальной форме (3NF)
- **Производительность**: добавлены индексы и оптимизации
- **Функциональность**: расширенные возможности управления

### 🗄️ **НОВАЯ СТРУКТУРА ТАБЛИЦ:**

| Таблица | Назначение | Ключевые поля |
|---------|------------|---------------|
| **Users** | Аутентификация | UserID, Username, PasswordHash |
| **Employees** | Сотрудники | EmployeeID, FirstName, LastName, RoleID |
| **Roles** | Роли системы | RoleID, RoleName, Permissions |
| **Clients** | Клиенты | ClientID, FirstName, LastName, Phone |
| **Requests** | Заявки | RequestID, ClientID, StatusID, DeviceName |
| **RequestStatuses** | Статусы заявок | StatusID, StatusName, Color |

---

## 🔧 ПОШАГОВАЯ МИГРАЦИЯ

### **ШАГ 1: РЕЗЕРВНОЕ КОПИРОВАНИЕ**
```sql
-- Создайте резервную копию текущей базы данных
BACKUP DATABASE ServiceCenter1 TO DISK = 'C:\Backup\ServiceCenter1_Backup.bak'
```

### **ШАГ 2: ВЫПОЛНЕНИЕ SQL СКРИПТА**
1. Откройте SQL Server Management Studio
2. Подключитесь к серверу `DESKTOP-JSPDFF8\MSSQLSERVER1`
3. Выберите базу данных `ServiceCenter1`
4. Выполните скрипт `Database_Update_3NF.sql`

### **ШАГ 3: ОБНОВЛЕНИЕ КОДА ПРИЛОЖЕНИЯ**

#### **3.1 Обновление LoginWindow.xaml.cs**
```csharp
// ЗАМЕНИТЬ существующие модели на новые:
using ServiceCenter.Models;
using ServiceCenter.Data;

// Обновить метод аутентификации:
private void LoginButton_Click(object sender, RoutedEventArgs e)
{
    string username = UsernameTextBox.Text;
    string password = PasswordBox.Password;

    using (var context = new UpdatedApplicationDbContext())
    {
        var user = context.Users
            .Include(u => u.Employee)
            .ThenInclude(e => e.Role)
            .FirstOrDefault(u => u.Username == username);

        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            MainWindow mainWindow = new MainWindow(user.Employee.Role.RoleName);
            mainWindow.Show();
            this.Close();
        }
        else
        {
            MessageBox.Show("Неверное имя пользователя или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
```

#### **3.2 Обновление MainWindow.xaml.cs**
```csharp
// Обновить метод загрузки заявок:
private void LoadRequests()
{
    using (var context = new UpdatedApplicationDbContext())
    {
        var requests = context.GetRequestsFullInfo().ToList();
        RequestsDataGrid.ItemsSource = requests;
    }
}

// Обновить метод удаления заявки:
private void DeleteRequest_Click(object sender, RoutedEventArgs e)
{
    if (RequestsDataGrid.SelectedItem != null)
    {
        var selectedRequest = (RequestFullInfo)RequestsDataGrid.SelectedItem;
        
        var result = MessageBox.Show($"Вы уверены, что хотите удалить заявку ID {selectedRequest.RequestID}?",
                                     "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            using (var context = new UpdatedApplicationDbContext())
            {
                var request = context.Requests.Find(selectedRequest.RequestID);
                if (request != null)
                {
                    context.Requests.Remove(request);
                    context.SaveChanges();
                    LoadRequests();
                }
            }
        }
    }
    else
    {
        MessageBox.Show("Выберите заявку для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
```

#### **3.3 Обновление AddRequestWindow.xaml.cs**
```csharp
// Обновить метод сохранения заявки:
private void SaveButton_Click(object sender, RoutedEventArgs e)
{
    if (string.IsNullOrWhiteSpace(DeviceNameTextBox.Text) || 
        string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
    {
        MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }

    using (var context = new UpdatedApplicationDbContext())
    {
        string phone = PhoneTextBox.Text;
        string firstName = FirstNameTextBox.Text;
        string lastName = LastNameTextBox.Text;
        
        // Найти или создать клиента
        var client = context.FindOrCreateClient(phone, firstName, lastName);
        
        var newRequest = new Request
        {
            ClientID = client.ClientID,
            DeviceName = DeviceNameTextBox.Text,
            Description = DescriptionTextBox.Text,
            StatusID = RequestStatusConstants.New,
            Priority = (int)RequestPriority.Normal
        };

        context.Requests.Add(newRequest);
        context.SaveChanges();
    }

    DialogResult = true;
}
```

#### **3.4 Обновление UpdateRequestWindow.xaml.cs**
```csharp
// Обновить метод загрузки данных заявки:
private void LoadRequestData(int requestId)
{
    using (var context = new UpdatedApplicationDbContext())
    {
        var request = context.Requests
            .Include(r => r.Client)
            .Include(r => r.Status)
            .FirstOrDefault(r => r.RequestID == requestId);

        if (request != null)
        {
            DeviceNameTextBox.Text = request.DeviceName;
            DescriptionTextBox.Text = request.Description;
            PhoneTextBox.Text = request.Client.Phone;
            FirstNameTextBox.Text = request.Client.FirstName;
            LastNameTextBox.Text = request.Client.LastName;
            
            // Загрузить статусы в ComboBox
            var statuses = context.RequestStatuses.ToList();
            StatusComboBox.ItemsSource = statuses;
            StatusComboBox.DisplayMemberPath = "StatusName";
            StatusComboBox.SelectedValuePath = "StatusID";
            StatusComboBox.SelectedValue = request.StatusID;
        }
    }
}

// Обновить метод сохранения изменений:
private void SaveButton_Click(object sender, RoutedEventArgs e)
{
    using (var context = new UpdatedApplicationDbContext())
    {
        var request = context.Requests.Find(_requestId);
        if (request != null)
        {
            request.DeviceName = DeviceNameTextBox.Text;
            request.Description = DescriptionTextBox.Text;
            request.StatusID = (int)StatusComboBox.SelectedValue;
            
            // Обновить данные клиента
            var client = context.Clients.Find(request.ClientID);
            if (client != null)
            {
                client.FirstName = FirstNameTextBox.Text;
                client.LastName = LastNameTextBox.Text;
                client.Phone = PhoneTextBox.Text;
            }
            
            context.SaveChanges();
        }
    }
    
    DialogResult = true;
}
```

---

## 🎯 НОВЫЕ ВОЗМОЖНОСТИ

### **1. Управление статусами заявок**
```csharp
// Получить все статусы
var statuses = context.RequestStatuses.ToList();

// Обновить статус заявки
context.UpdateRequestStatus(requestId, RequestStatusConstants.InProgress, employeeId);
```

### **2. Статистика заявок**
```csharp
// Получить статистику
var stats = context.GetRequestsStatistics();
```

### **3. Расширенная информация о заявках**
```csharp
// Получить полную информацию с цветами статусов
var requests = context.GetRequestsFullInfo().ToList();
```

### **4. Управление приоритетами**
```csharp
// Установить приоритет заявки
request.Priority = (int)RequestPriority.High;
```

---

## ⚠️ ВАЖНЫЕ ЗАМЕЧАНИЯ

### **🔒 БЕЗОПАСНОСТЬ:**
- Все существующие пароли сохраняются
- Роли пользователей автоматически мигрируются
- Данные заявок полностью переносятся

### **🔄 ОБРАТНАЯ СОВМЕСТИМОСТЬ:**
- Старые методы работы с данными заменяются на новые
- Все существующие функции сохраняются
- Добавляются новые возможности

### **📊 ПРОИЗВОДИТЕЛЬНОСТЬ:**
- Добавлены индексы для быстрого поиска
- Оптимизированы запросы через представления
- Улучшена структура данных

---

## 🚨 ПРОБЛЕМЫ И РЕШЕНИЯ

### **Проблема**: Ошибка подключения к базе данных
**Решение**: Проверьте строку подключения в `UpdatedApplicationDbContext.cs`

### **Проблема**: Не отображаются заявки
**Решение**: Убедитесь, что миграция данных прошла успешно

### **Проблема**: Ошибки компиляции
**Решение**: Добавьте using директивы для новых пространств имен

---

## 📞 ПОДДЕРЖКА

При возникновении проблем:
1. Проверьте логи SQL Server
2. Убедитесь в корректности миграции данных
3. Проверьте права доступа к базе данных

---

## 🎉 РЕЗУЛЬТАТ

После успешной миграции вы получите:
- ✅ Базу данных в третьей нормальной форме
- ✅ 6 таблиц вместо 4
- ✅ Улучшенную производительность
- ✅ Расширенные возможности управления
- ✅ Сохранение всей функциональности приложения

**Ваше приложение готово к работе с новой структурой! 🚀** 