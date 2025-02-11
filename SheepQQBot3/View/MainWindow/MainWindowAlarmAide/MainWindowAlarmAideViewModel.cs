using Masuit.Tools;
using SheepQQBot3.Extensions;
using SheepQQBot3.Model.Config;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SheepQQBot3.View;

public partial class MainWindowAlarmAideViewModel : MainWindowViewModelBase
{
    /// <summary>
    /// 初始化
    /// </summary>
    public MainWindowAlarmAideViewModel()
    {
    }

    private AlarmAideConfig _selectedAlarmAideConfig;
    public AlarmAideConfig SelectedAlarmAideConfig
    {
        get => _selectedAlarmAideConfig;
        set
        {
            if (_selectedAlarmAideConfig == value)
                return;

            _selectedAlarmAideConfig = value;
            SelectedAlarmText = default;
            OnPropertyChanged(nameof(SelectedAlarmAideConfig));
        }
    }

    private KeyValuePair<int, string> _selectedAlarmText;
    /// <summary>
    /// 选中的提醒列表内容
    /// </summary>
    public KeyValuePair<int, string> SelectedAlarmText
    {
        get => _selectedAlarmText;
        set
        {
            if (_selectedAlarmText.Key == value.Key && _selectedAlarmText.Value == value.Value)
                return;

            _selectedAlarmText = value;
            OnPropertyChanged(nameof(SelectedAlarmText));
        }
    }

    private int _selectedMemberId;
    /// <summary>
    /// 选中的提醒列表内容
    /// </summary>
    public int SelectedMemberId
    {
        get => _selectedMemberId;
        set
        {
            if (_selectedMemberId == value)
                return;

            _selectedMemberId = value;
            OnPropertyChanged(nameof(SelectedMemberId));
        }
    }

    private bool _isSingleSelectedAlarmAideText;
    /// <summary>
    /// 是否单选提醒列表内容
    /// </summary>
    public bool IsSingleSelectedAlarmAideText
    {
        get => _isSingleSelectedAlarmAideText;
        set
        {
            if (_isSingleSelectedAlarmAideText == value)
                return;

            _isSingleSelectedAlarmAideText = value;
            OnPropertyChanged(nameof(IsSingleSelectedAlarmAideText));
        }
    }

    /// <summary>
    /// 闹钟助手内容列表-新增
    /// </summary>
    /// <param name="newAlarmText">新增的提醒内容</param>
    /// <param name="focusControl">设置焦点的控件</param>
    internal void OnAddAlarmAideTest(string newAlarmText, Control focusControl = null)
    {
        var alarmTexts = SelectedAlarmAideConfig.AlarmTexts;
        var newAlarmAideTextId = alarmTexts.GetSequence();
        var newAlarmAideText = new KeyValuePair<int, string>(newAlarmAideTextId, newAlarmText);
        SelectedAlarmAideConfig.AlarmTexts = alarmTexts.CopyAdd(newAlarmAideTextId, newAlarmText);
        SelectedAlarmText = newAlarmAideText;
        ConfigExtensions.SaveConfig(focusControl);
    }
}