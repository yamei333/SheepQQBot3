using MessagePack;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model;

public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
{
    /// <inheritdoc/>
    [field: IgnoreMember]
    [field: JsonIgnore]
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// 值变化时调用, 用于通知界面
    /// </summary>
    /// <param name="propertyName">属性名</param>
    public void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}