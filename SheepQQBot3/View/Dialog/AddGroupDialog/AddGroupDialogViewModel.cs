using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.View;

public class AddGroupDialogViewModel : INotifyPropertyChanged
{
    private long _targetId;
    private GroupConfig _selectedGroupConfig;
    private string _targetName;
    private List<GroupConfig> _groupConfigs;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public AddGroupDialogViewModel()
    {
        TargetId = default;
        TargetName = string.Empty;
        var groupConfigs = new List<GroupConfig>
        {
            new(BotConfigTargetType.Common, "系统"),
            new(BotConfigTargetType.Group, "群"),
            new(BotConfigTargetType.Private, "个人"),
        };
        GroupConfigs = groupConfigs;
        SelectedGroupConfig = groupConfigs.First();
    }

    /// <summary>
    /// 类型
    /// </summary>
    public List<GroupConfig> GroupConfigs
    {
        get => _groupConfigs;
        set
        {
            if (_groupConfigs == value)
                return;

            _groupConfigs = value;
            OnPropertyChanged(nameof(GroupConfigs));
        }
    }

    /// <summary>
    /// 当前选中类型
    /// </summary>
    public GroupConfig SelectedGroupConfig
    {
        get => _selectedGroupConfig;
        set
        {
            _selectedGroupConfig = value;
            OnPropertyChanged(nameof(SelectedGroupConfig));
        }
    }

    /// <summary>
    /// 配置目标ID
    /// </summary>
    public long TargetId
    {
        get => _targetId;
        set
        {
            _targetId = value;
            OnPropertyChanged(nameof(TargetId));
        }
    }

    /// <summary>
    /// 配置名称
    /// </summary>
    public string TargetName
    {
        get => _targetName;
        set
        {
            _targetName = value;
            OnPropertyChanged(nameof(TargetName));
        }
    }
}