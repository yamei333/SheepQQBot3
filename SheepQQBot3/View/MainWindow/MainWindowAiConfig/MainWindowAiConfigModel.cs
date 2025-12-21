using System.Linq;
using static SheepQQBot3.PublicVar;

namespace SheepQQBot3.View;

public partial class MainWindowAIConfigModel : MainWindowViewModelBase
{
    /// <summary>
    /// 初始化
    /// </summary>
    public MainWindowAIConfigModel()
    {
        ListItems = GlobalAICharacter?.SystemInstruction?.Keys.ToArray() ?? [];
        SelectedValue = GlobalAICharacter?.SystemInstruction?.First().Key;
    }

    /// <summary>
    /// Items
    /// </summary>
    public string[] ListItems
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged(nameof(ListItems));
        }
    }

    /// <summary>
    /// 选中的项目
    /// </summary>
    public string SelectedValue
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged(nameof(SelectedValue));

            if (field != null)
            {
                _systemInstructionText = GlobalAICharacter?.SystemInstruction[field] ?? string.Empty;
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
            GlobalAICharacter.SystemInstruction[SelectedValue] = _systemInstructionText;
        }
    }
}