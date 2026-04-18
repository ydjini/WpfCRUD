Вот несколько способов реализации удаления выделенной записи из DataGrid в WPF:

1. Базовый способ (Code-behind)

XAML:

```xml
<DataGrid x:Name="ProductsDataGrid" ItemsSource="{Binding Products}" 
          AutoGenerateColumns="False" SelectionMode="Single">
    <DataGrid.Columns>
        <DataGridTextColumn Header="ID" Binding="{Binding Id}"/>
        <DataGridTextColumn Header="Название" Binding="{Binding Name}"/>
        <DataGridTextColumn Header="Цена" Binding="{Binding Price}"/>
    </DataGrid.Columns>
</DataGrid>

<Button Content="Удалить" Click="DeleteButton_Click" 
        Background="Red" Foreground="White"/>
```

Code-behind:

```csharp
private void DeleteButton_Click(object sender, RoutedEventArgs e)
{
    if (ProductsDataGrid.SelectedItem is Product selectedProduct)
    {
        if (MessageBox.Show($"Удалить товар '{selectedProduct.Name}'?", "Подтверждение",
            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            using (var context = new ApplicationDbContext())
            {
                context.Products.Remove(selectedProduct);
                context.SaveChanges();
                
                // Обновить список
                ProductsDataGrid.ItemsSource = context.Products.ToList();
            }
        }
    }
    else
    {
        MessageBox.Show("Выберите запись для удаления", "Внимание", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
```

2. MVVM способ (рекомендуемый)

ViewModel:

```csharp
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.EntityFrameworkCore;

public class ProductsViewModel : INotifyPropertyChanged
{
    private ObservableCollection<Product> _products;
    private Product _selectedProduct;
    
    public ObservableCollection<Product> Products
    {
        get => _products;
        set { _products = value; OnPropertyChanged(); }
    }
    
    public Product SelectedProduct
    {
        get => _selectedProduct;
        set { _selectedProduct = value; OnPropertyChanged(); }
    }
    
    public RelayCommand DeleteCommand => new RelayCommand(ExecuteDelete, CanExecuteDelete);
    
    private bool CanExecuteDelete(object parameter)
    {
        return SelectedProduct != null;
    }
    
    private async void ExecuteDelete(object parameter)
    {
        var result = MessageBox.Show($"Удалить товар '{SelectedProduct.Name}'?", 
                                      "Подтверждение", 
                                      MessageBoxButton.YesNo, 
                                      MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    context.Products.Remove(SelectedProduct);
                    await context.SaveChangesAsync();
                    
                    // Обновление списка
                    await LoadProductsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    private async Task LoadProductsAsync()
    {
        using (var context = new ApplicationDbContext())
        {
            var products = await context.Products.ToListAsync();
            Products = new ObservableCollection<Product>(products);
        }
    }
}
```

XAML:

```xml
<DataGrid ItemsSource="{Binding Products}" 
          SelectedItem="{Binding SelectedProduct, UpdateSourceTrigger=PropertyChanged}"
          AutoGenerateColumns="False">
    <DataGrid.Columns>
        <DataGridTextColumn Header="ID" Binding="{Binding Id}"/>
        <DataGridTextColumn Header="Название" Binding="{Binding Name}"/>
        <DataGridTextColumn Header="Цена" Binding="{Binding Price}"/>
    </DataGrid.Columns>
</DataGrid>

<Button Content="Удалить" Command="{Binding DeleteCommand}"
        Background="Red" Foreground="White"/>
```

3. Удаление по клавише Delete

Добавьте в XAML DataGrid:

```xml
<DataGrid x:Name="ProductsDataGrid" 
          KeyDown="ProductsDataGrid_KeyDown">
```

Code-behind:

```csharp
private void ProductsDataGrid_KeyDown(object sender, KeyEventArgs e)
{
    if (e.Key == Key.Delete && ProductsDataGrid.SelectedItem != null)
    {
        DeleteSelectedItem();
    }
}
```

Или в MVVM с помощью Attached Property:

```csharp
public static class DataGridBehavior
{
    public static readonly DependencyProperty DeleteCommandProperty =
        DependencyProperty.RegisterAttached("DeleteCommand", typeof(ICommand), 
        typeof(DataGridBehavior), new PropertyMetadata(null, OnDeleteCommandChanged));
    
    public static void SetDeleteCommand(DependencyObject obj, ICommand value)
    {
        obj.SetValue(DeleteCommandProperty, value);
    }
    
    public static ICommand GetDeleteCommand(DependencyObject obj)
    {
        return (ICommand)obj.GetValue(DeleteCommandProperty);
    }
    
    private static void OnDeleteCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DataGrid dataGrid)
        {
            dataGrid.PreviewKeyDown -= DataGrid_PreviewKeyDown;
            if (e.NewValue is ICommand)
                dataGrid.PreviewKeyDown += DataGrid_PreviewKeyDown;
        }
    }
    
    private static void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && sender is DataGrid dataGrid)
        {
            var command = GetDeleteCommand(dataGrid);
            if (command?.CanExecute(dataGrid.SelectedItem) == true)
                command.Execute(dataGrid.SelectedItem);
        }
    }
}
```

Использование в XAML:

```xml
<DataGrid ItemsSource="{Binding Products}" 
          SelectedItem="{Binding SelectedProduct}"
          local:DataGridBehavior.DeleteCommand="{Binding DeleteCommand}">
```

4. Удаление с обновлением через Reload

```csharp
private async void DeleteSelectedItem()
{
    if (ProductsDataGrid.SelectedItem is Product selectedProduct)
    {
        if (MessageBox.Show("Удалить выбранную запись?", "Подтверждение", 
            MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            using (var context = new ApplicationDbContext())
            {
                var productToDelete = await context.Products.FindAsync(selectedProduct.Id);
                if (productToDelete != null)
                {
                    context.Products.Remove(productToDelete);
                    await context.SaveChangesAsync();
                    
                    // Обновление DataGrid
                    await RefreshDataGrid();
                }
            }
        }
    }
}

private async Task RefreshDataGrid()
{
    using (var context = new ApplicationDbContext())
    {
        ProductsDataGrid.ItemsSource = await context.Products.ToListAsync();
    }
}
```

5. Удаление нескольких записей (множественный выбор)

XAML:

```xml
<DataGrid x:Name="ProductsDataGrid" 
          SelectionMode="Extended" 
          SelectionUnit="FullRow">
```

Code-behind:

```csharp
private void DeleteMultiple_Click(object sender, RoutedEventArgs e)
{
    var selectedItems = ProductsDataGrid.SelectedItems.Cast<Product>().ToList();
    
    if (selectedItems.Count == 0)
    {
        MessageBox.Show("Выберите записи для удаления");
        return;
    }
    
    var result = MessageBox.Show($"Удалить {selectedItems.Count} записей?", 
                                  "Подтверждение", 
                                  MessageBoxButton.YesNo);
    
    if (result == MessageBoxResult.Yes)
    {
        using (var context = new ApplicationDbContext())
        {
            context.Products.RemoveRange(selectedItems);
            context.SaveChanges();
            
            // Обновление
            ProductsDataGrid.ItemsSource = context.Products.ToList();
        }
    }
}
```

6. Удаление с подтверждением и прогрессом

```csharp
private async void DeleteWithProgress_Click(object sender, RoutedEventArgs e)
{
    if (ProductsDataGrid.SelectedItem is not Product selectedProduct) 
        return;
    
    // Кастомное окно подтверждения
    var dialogResult = MessageBox.Show(
        $"Удалить товар:\nID: {selectedProduct.Id}\nНазвание: {selectedProduct.Name}\nЦена: {selectedProduct.Price}",
        "Подтверждение удаления",
        MessageBoxButton.YesNo,
        MessageBoxImage.Warning);
    
    if (dialogResult == MessageBoxResult.Yes)
    {
        // Показываем индикатор загрузки
        LoadingOverlay.Visibility = Visibility.Visible;
        
        try
        {
            await Task.Run(() =>
            {
                using (var context = new ApplicationDbContext())
                {
                    context.Entry(selectedProduct).State = EntityState.Deleted;
                    context.SaveChanges();
                }
            });
            
            // Обновление UI
            await RefreshDataGridAsync();
            
            MessageBox.Show("Запись успешно удалена!", "Успех", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Нельзя удалить запись, так как она связана с другими данными\n{ex.Message}", 
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }
}
```

7. Полный пример с обработкой ошибок

```csharp
public class DataGridDeleteHandler
{
    public static async Task<bool> DeleteSelectedItemAsync<T>(
        DataGrid dataGrid, 
        Func<T, Task> deleteAction,
        string itemName = "запись") where T : class
    {
        if (dataGrid.SelectedItem is not T selectedItem)
        {
            MessageBox.Show("Выберите запись для удаления", "Внимание", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        
        var result = MessageBox.Show($"Удалить {itemName}?", "Подтверждение", 
                                      MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        if (result != MessageBoxResult.Yes) 
            return false;
        
        try
        {
            await deleteAction(selectedItem);
            MessageBox.Show("Запись успешно удалена", "Успех", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }
}

// Использование:
private async void DeleteButton_Click(object sender, RoutedEventArgs e)
{
    var deleted = await DataGridDeleteHandler.DeleteSelectedItemAsync<Product>(
        ProductsDataGrid,
        async (product) =>
        {
            using (var context = new ApplicationDbContext())
            {
                context.Products.Remove(product);
                await context.SaveChangesAsync();
            }
        },
        $"товар '{((Product)ProductsDataGrid.SelectedItem)?.Name}'"
    );
    
    if (deleted)
    {
        await RefreshDataGridAsync();
    }
}
```

Выберите подходящий способ в зависимости от вашей архитектуры (MVVM или code-behind) и требований к функциональности.