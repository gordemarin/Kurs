using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ServiceCenter
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class UpdateRequestWindow : Window
    {
        private int _requestId;
        private string _userRole;

        public UpdateRequestWindow(int requestId, string role)
        {
            InitializeComponent();
            _requestId = requestId;
            _userRole = role;

            LoadRequestData();
            ApplyAccessControl();
        }

        private void ApplyAccessControl()
        {
            // Администраторы и менеджеры могут редактировать все ключевые поля
            bool canEdit = (_userRole == "Администратор" || _userRole == "Менеджер");

            StatusComboBox.IsEnabled = canEdit;
            DeviceNameTextBox.IsEnabled = canEdit;

            // Описание и данные клиента может редактировать любой, кто открыл окно
            DescriptionTextBox.IsEnabled = true; 
            FirstNameTextBox.IsEnabled = true;
            LastNameTextBox.IsEnabled = true;
            PhoneTextBox.IsEnabled = true;
        }

        private void LoadRequestData()
        {
            using (var context = new ApplicationDbContext())
            {
                var request = context.Requests
                    .Include(r => r.Client)
                    .Include(r => r.Status)
                    .FirstOrDefault(r => r.RequestID == _requestId);

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
                else
                {
                    MessageBox.Show("Заявка не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text) || StatusComboBox.SelectedItem is null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new ApplicationDbContext())
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
                    DialogResult = true; // Закрыть окно и вернуть управление
                }
                else
                {
                    MessageBox.Show("Заявка не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
