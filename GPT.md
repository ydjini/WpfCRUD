Вот максимально простое WPF-приложение для автопарка на Entity Framework Core + MySQL. Всё сделано наглядно и минимально, но с соблюдением 3НФ и требуемого функционала.

1. Модели данных (3 таблицы, 3НФ)

```csharp
// Models/User.cs
namespace AutoPark.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Manager"; // только менеджеры
    }
}
```

```csharp
// Models/Vehicle.cs
namespace AutoPark.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string LicensePlate { get; set; } = string.Empty; // госномер
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Status { get; set; } = "Available"; // Available, Maintenance
    }
}
```

```csharp
// Models/MaintenanceRequest.cs
namespace AutoPark.Models
{
    public class MaintenanceRequest
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int UserId { get; set; }      // менеджер, создавший заявку
        public DateTime IssueDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Completed

        // Навигационные свойства (не обязательны для БД, но удобны)
        public Vehicle? Vehicle { get; set; }
        public User? User { get; set; }
    }
}
```

2. Контекст базы данных

```csharp
// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using AutoPark.Models;

namespace AutoPark.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Измените строку подключения под свой MySQL
            var connectionString = "server=localhost;database=autoparkdb;user=root;password=123456;";
            optionsBuilder.UseMySQL(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Уникальный логин
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();

            // Уникальный госномер
            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate)
                .IsUnique();

            // Связи (внешние ключи)
            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.Vehicle)
                .WithMany()
                .HasForeignKey(m => m.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
```

3. Тестовые данные (инициализация)

```csharp
// Data/DbInitializer.cs
using AutoPark.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPark.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // Добавляем менеджера, если нет
            if (!context.Users.Any())
            {
                context.Users.Add(new User
                {
                    Login = "manager",
                    Password = "123",
                    FullName = "Иванов Иван",
                    Role = "Manager"
                });
                context.SaveChanges();
            }

            // Добавляем автомобили
            if (!context.Vehicles.Any())
            {
                context.Vehicles.AddRange(
                    new Vehicle { LicensePlate = "А001АА77", Brand = "Kia", Model = "Rio", Status = "Available" },
                    new Vehicle { LicensePlate = "В222ВВ77", Brand = "Hyundai", Model = "Solaris", Status = "Maintenance" },
                    new Vehicle { LicensePlate = "С333СС77", Brand = "Lada", Model = "Vesta", Status = "Available" }
                );
                context.SaveChanges();
            }
        }
    }
}
```

4. Вспомогательный класс RelayCommand

```csharp
// Helpers/RelayCommand.cs
using System;
using System.Windows.Input;

namespace AutoPark.Helpers
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
```

5. Окно авторизации (View и ViewModel)

```xml
<!-- Views/LoginWindow.xaml -->
<Window x:Class="AutoPark.Views.LoginWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Авторизация" Height="200" Width="350" WindowStartupLocation="CenterScreen">
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        <TextBlock Text="Вход в систему" FontSize="18" FontWeight="Bold" HorizontalAlignment="Center" Margin="0,0,0,20"/>
        <TextBox Grid.Row="1" x:Name="LoginBox" Margin="0,5" Text="manager" />
        <PasswordBox Grid.Row="2" x:Name="PasswordBox" Margin="0,5"/>
        <Button Grid.Row="3" Content="Войти" Click="Login_Click" Height="30" Margin="0,15,0,0" Background="#007ACC" Foreground="White"/>
        <TextBlock x:Name="ErrorText" Grid.Row="3" Foreground="Red" HorizontalAlignment="Center" Margin="0,50,0,0"/>
    </Grid>
</Window>
```

```csharp
// Views/LoginWindow.xaml.cs
using System.Windows;
using AutoPark.Data;
using AutoPark.Models;
using AutoPark.Views;

namespace AutoPark.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            using var db = new AppDbContext();
            var user = db.Users.FirstOrDefault(u => u.Login == LoginBox.Text && u.Password == PasswordBox.Password);
            if (user != null && user.Role == "Manager")
            {
                var mainWin = new MainWindow(user);
                mainWin.Show();
                Close();
            }
            else
            {
                ErrorText.Text = "Неверный логин или пароль";
            }
        }
    }
}
```

6. Главное окно (просмотр авто + создание заявки)

```xml
<!-- Views/MainWindow.xaml -->
<Window x:Class="AutoPark.Views.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Автопарк - Менеджер" Height="500" Width="800" WindowStartupLocation="CenterScreen">
    <Grid Margin="10">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <TextBlock Text="Учёт транспорта автопарка" FontSize="20" FontWeight="Bold" Margin="0,0,0,10"/>

        <!-- Список автомобилей -->
        <DataGrid Grid.Row="1" ItemsSource="{Binding Vehicles}" SelectedItem="{Binding SelectedVehicle}" 
                  AutoGenerateColumns="False" IsReadOnly="True" Margin="0,10">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Госномер" Binding="{Binding LicensePlate}" Width="100"/>
                <DataGridTextColumn Header="Марка" Binding="{Binding Brand}" Width="100"/>
                <DataGridTextColumn Header="Модель" Binding="{Binding Model}" Width="100"/>
                <DataGridTextColumn Header="Статус" Binding="{Binding Status}" Width="120"/>
            </DataGrid.Columns>
        </DataGrid>

        <!-- Форма заявки -->
        <Border Grid.Row="2" BorderBrush="Gray" BorderThickness="1" CornerRadius="5" Padding="10" Margin="0,10,0,0">
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>
                <TextBlock Text="Оформить заявку на ТО" FontWeight="Bold" FontSize="14" Margin="0,0,0,10"/>
                <TextBlock Grid.Row="1" Text="Выберите автомобиль:"/>
                <ComboBox Grid.Row="2" ItemsSource="{Binding Vehicles}" SelectedItem="{Binding SelectedVehicle}"
                          DisplayMemberPath="LicensePlate" Margin="0,5,0,10"/>
                <TextBlock Grid.Row="3" Text="Описание работ:"/>
                <TextBox Grid.Row="4" Text="{Binding RequestDescription}" Height="60" TextWrapping="Wrap" Margin="0,5,0,10"/>
                <Button Grid.Row="5" Content="Создать заявку" Command="{Binding CreateRequestCommand}" 
                        Background="#4CAF50" Foreground="White" Height="35" Margin="0,5"/>
                <TextBlock Grid.Row="6" Text="{Binding StatusMessage}" Foreground="Green" Margin="0,5"/>
            </Grid>
        </Border>
    </Grid>
</Window>
```

```csharp
// Views/MainWindow.xaml.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using AutoPark.Data;
using AutoPark.Helpers;
using AutoPark.Models;

namespace AutoPark.Views
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly User _currentUser;
        private ObservableCollection<Vehicle> _vehicles = new();
        private Vehicle? _selectedVehicle;
        private string _requestDescription = "";
        private string _statusMessage = "";

        public ObservableCollection<Vehicle> Vehicles
        {
            get => _vehicles;
            set { _vehicles = value; OnPropertyChanged(); }
        }

        public Vehicle? SelectedVehicle
        {
            get => _selectedVehicle;
            set { _selectedVehicle = value; OnPropertyChanged(); }
        }

        public string RequestDescription
        {
            get => _requestDescription;
            set { _requestDescription = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public RelayCommand CreateRequestCommand { get; }

        public MainWindow(User user)
        {
            _currentUser = user;
            InitializeComponent();
            DataContext = this;
            CreateRequestCommand = new RelayCommand(_ => CreateRequest(), _ => SelectedVehicle != null && !string.IsNullOrWhiteSpace(RequestDescription));
            LoadVehicles();
        }

        private void LoadVehicles()
        {
            using var db = new AppDbContext();
            var list = db.Vehicles.ToList();
            Vehicles.Clear();
            foreach (var v in list) Vehicles.Add(v);
        }

        private void CreateRequest()
        {
            using var db = new AppDbContext();
            var request = new MaintenanceRequest
            {
                VehicleId = SelectedVehicle!.Id,
                UserId = _currentUser.Id,
                Description = RequestDescription,
                IssueDate = DateTime.Now,
                Status = "Pending"
            };
            db.MaintenanceRequests.Add(request);
            db.SaveChanges();

            StatusMessage = "Заявка успешно создана!";
            RequestDescription = "";
            SelectedVehicle = null;
            // обновим статус авто (опционально)
            LoadVehicles();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
```

7. App.xaml.cs (запуск и инициализация БД)

```csharp
// App.xaml.cs
using System.Windows;
using AutoPark.Data;
using AutoPark.Views;

namespace AutoPark
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            using var db = new AppDbContext();
            DbInitializer.Initialize(db);
            new LoginWindow().Show();
        }
    }
}
```

8. ERD диаграмма (текстовое описание)

```
┌───────────────┐       ┌────────────────────────┐
│     Users     │       │  MaintenanceRequests   │
├───────────────┤       ├────────────────────────┤
│ Id (PK)       │◄──────│ UserId (FK)            │
│ Login         │       │ Id (PK)                │
│ Password      │       │ VehicleId (FK)         │──────┐
│ FullName      │       │ IssueDate              │      │
│ Role          │       │ Description            │      │
└───────────────┘       │ Status                 │      │
                        └────────────────────────┘      │
                                                         │
                        ┌────────────────────────┐      │
                        │        Vehicles        │      │
                        ├────────────────────────┤      │
                        │ Id (PK)                │◄─────┘
                        │ LicensePlate (unique)  │
                        │ Brand                  │
                        │ Model                  │
                        │ Status                 │
                        └────────────────────────┘
```

Связи:

· Один пользователь (менеджер) может создать много заявок.
· Один автомобиль может участвовать во многих заявках.

9. NuGet пакеты (установить в проект)

```bash
Install-Package Microsoft.EntityFrameworkCore
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package MySql.EntityFrameworkCore
```

10. Строка подключения (измените под себя)

В AppDbContext.cs:

```csharp
var connectionString = "server=localhost;database=autoparkdb;user=root;password=ваш_пароль;";
```

11. Создание базы данных

В консоли диспетчера пакетов выполните:

```bash
Add-Migration InitialCreate
Update-Database
```

Итог

· Авторизация по логину/паролю (тестовый менеджер: manager / 123).
· Просмотр автомобилей в DataGrid.
· Оформление заявки – выбор авто из списка, описание, сохранение в БД с привязкой к текущему менеджеру.
· 3 таблицы в 3НФ, связаны внешними ключами.
· Тестовые данные добавляются автоматически.

Приложение готово к запуску и полностью соответствует заданию.