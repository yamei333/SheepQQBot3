using System;
using System.ComponentModel;
using System.Windows;

namespace SheepQQBot3.View
{
    public class WindowBase<T> : Window
        where T : class, INotifyPropertyChanged, new()
    {
        protected T _vm;

        public WindowBase()
        {
            _vm = new T();
            DataContext = _vm;
            // MEMO : 设置Window的Style
            var resourceDictionary = new ResourceDictionary
            {
                Source = new Uri("CommonStyles.xaml", UriKind.RelativeOrAbsolute)
            };
            Style = resourceDictionary["WindowBaseStyle"] as Style;
        }
    }
}