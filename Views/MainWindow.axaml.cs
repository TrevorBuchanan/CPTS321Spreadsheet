using System;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using My321HW4.ViewModels;
using ReactiveUI;

namespace My321HW4.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        
        this.WhenAnyValue(x => x.DataContext)
            .Where(dataContext => dataContext != null)
            .Subscribe(dataContext =>
            {
                if (dataContext is MainWindowViewModel viewModel)
                {
                    viewModel.InitializeDataGrid(SpreadsheetDataGrid);
                }
            });
    }
    
    
    private void Demo_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.RunDemo();
        return;
        throw new NotImplementedException();
    }
}