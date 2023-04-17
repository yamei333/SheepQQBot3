using System.Windows;

namespace SheepQQBot3.View;

public static class MainWindowUtil
{
    /// <summary>
    /// 显示删除确认
    /// </summary>
    /// <returns>是否点击OK</returns>
    public static bool ShowDeleteDialog()
        => MessageBox.Show("确定要删除吗?", "确认", MessageBoxButton.OKCancel,
            MessageBoxImage.Question, MessageBoxResult.OK) == MessageBoxResult.OK;
}