using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SheepQQBot3.Enums;

namespace SheepQQBot3.View;

public class AddDialogWindowBase<T> : Window
    where T : class, INotifyPropertyChanged, new()
{
    protected DialogMode Mode { get; set; }
    protected T Vm;

    public AddDialogWindowBase(Window owner, object menuItem, DialogMode mode, string title = "")
    {
        // MEMO : 设置Window的Style
        var resourceDictionary = new ResourceDictionary
        {
            Source = new Uri("View/Style/CommonStyles.xaml", UriKind.RelativeOrAbsolute)
        };
        Style = resourceDictionary["WindowBaseStyle"] as Style;
        // MEMO : 初始化
        Owner = owner;
        Mode = mode;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Initialized += (sender, args) =>
        {
            Vm = DataContext as T;
            if (Vm == null)
                return;
        };
        // MEMO : Load时设置标题
        Loaded += (sender, args) =>
        {
            Title = $"{GetMenuHeader(menuItem)}{(string.IsNullOrEmpty(title) ? Title : title)}";
            OnLoaded(sender, args);
            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        };
    }

    protected virtual void OnLoaded(object sender, RoutedEventArgs e)
    {
    }

    private static string GetMenuHeader(object menuItem)
        => (menuItem as MenuItem)?.Header.ToString() ?? string.Empty;
}