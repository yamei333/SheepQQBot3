using System;
using System.Collections.Generic;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;

namespace SheepQQBot3.View;

public abstract class MainWindowViewModelBase : NotifyPropertyChangedBase
{
    protected static MainWindowViewModel _mainVm => PublicVar.Vm;

    /// <summary>
    /// 当前选中的配置, 用于修改
    /// </summary>
    public SetConfig SelectedSetConfig => _mainVm.SelectedSetConfig;

    /// <summary>
    /// Bot配置
    /// </summary>
    public Dictionary<Guid, SetConfig> SetConfigs => _mainVm.SetConfigs;
}