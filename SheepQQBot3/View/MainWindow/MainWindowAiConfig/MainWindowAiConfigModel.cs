using System.Linq;

namespace SheepQQBot3.View;

public partial class MainWindowAIConfigModel : MainWindowViewModelBase
{
    /// <summary>
    /// 初始化
    /// </summary>
    public MainWindowAIConfigModel()
    {
        ListItems = PublicVar.AICharacter?.SystemInstruction?.Keys.ToArray() ?? [];
        SelectedValue = PublicVar.AICharacter?.SystemInstruction?.First().Key;
    }

    private string[] _listItems;
    /// <summary>
    /// Items
    /// </summary>
    public string[] ListItems
    {
        get => _listItems;
        set
        {
            if (_listItems == value)
                return;

            _listItems = value;
            OnPropertyChanged(nameof(ListItems));
        }
    }

    private string _selectedValue;
    /// <summary>
    /// 选中的项目
    /// </summary>
    public string SelectedValue
    {
        get => _selectedValue;
        set
        {
            if (_selectedValue == value)
                return;

            _selectedValue = value;
            OnPropertyChanged(nameof(SelectedValue));

            if (_selectedValue != null)
            {
                _systemInstructionText = PublicVar.AICharacter?.SystemInstruction[_selectedValue] ?? string.Empty;
                OnPropertyChanged(nameof(SystemInstructionText));
            }
        }
    }

    private string _systemInstructionText;
    /// <summary>
    /// 文本内容
    /// </summary>
    public string SystemInstructionText
    {
        get => _systemInstructionText;
        set
        {
            if (_systemInstructionText == value)
                return;

            _systemInstructionText = value;
            OnPropertyChanged(nameof(SystemInstructionText));
            PublicVar.AICharacter.SystemInstruction[SelectedValue] = _systemInstructionText;
        }
    }
}