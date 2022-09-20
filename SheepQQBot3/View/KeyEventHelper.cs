using System;
using System.Windows.Input;

namespace SheepQQBot3.View
{
    public static class KeyEventHelper
    {
        public static void OnKeyDown(KeyEventArgs e, ModifierKeys modifierKey, Key key, Action action)
        {
            if ((Keyboard.Modifiers & modifierKey) == modifierKey && e.Key == Key.Enter)
            {
                action();
            }
        }
    }
}