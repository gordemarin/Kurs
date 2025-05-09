﻿using Microsoft.EntityFrameworkCore;
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
    public partial class MainWindow : Window
    {
        private string UserRole;

        // Конструктор по умолчанию
        public MainWindow() : this("Сотрудник") // Передаем роль "Сотрудник" по умолчанию
        {
        }

        // Конструктор с параметром
        public MainWindow(string role)
        {
            InitializeComponent();
            UserRole = role;

            LoadRequests();

            if (UserRole != "Администратор")
            {
                // Ограничиваем доступ к административным функциям
            }
        }

        private void LoadRequests()
        {
            using (var context = new ApplicationDbContext())
            {
                var requests = context.Requests
                    .Include(r => r.Clients)
                    .Select(r => new
                    {
                        r.RequestID,
                        r.Clients.ClientName,
                        r.DeviceName,
                        r.Description,
                        r.Status,
                        r.RequestDate,
                        r.Phone
                        
                    }).ToList();

                RequestsDataGrid.ItemsSource = requests;
            }
        }
        private void AddRequest_Click(object sender, RoutedEventArgs e)
        {
            var addRequestWindow = new AddRequestWindow(); // Открываем окно добавления заявки
            if (addRequestWindow.ShowDialog() == true)
            {
                LoadRequests(); // Обновляем данные после добавления
            }
        }

        private void UpdateRequest_Click(object sender, RoutedEventArgs e)
        {
            if (RequestsDataGrid.SelectedItem != null)
            {
                var selectedRequest = (dynamic)RequestsDataGrid.SelectedItem; // Динамическое приведение
                var updateRequestWindow = new UpdateRequestWindow(selectedRequest.RequestID);
                if (updateRequestWindow.ShowDialog() == true)
                {
                    LoadRequests(); // Обновляем данные после изменения
                }
            }
            else
            {
                MessageBox.Show("Выберите заявку для изменения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteRequest_Click(object sender, RoutedEventArgs e)
        {
            if (RequestsDataGrid.SelectedItem != null)
            {
                var selectedRequest = (dynamic)RequestsDataGrid.SelectedItem;

                var result = MessageBox.Show($"Вы уверены, что хотите удалить заявку ID {selectedRequest.RequestID}?",
                                             "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    using (var context = new ApplicationDbContext())
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

        private void RequestsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
