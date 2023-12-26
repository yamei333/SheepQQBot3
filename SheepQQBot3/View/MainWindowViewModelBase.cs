using System;
using System.Collections.Generic;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Config;
using SheepQQBot3.SDK.Api;

namespace SheepQQBot3.View;

public abstract class MainWindowViewModelBase : NotifyPropertyChangedBase
{
    protected static MainWindowViewModel _mainVm => PublicVar.Vm;

    /// <summary>
    /// 当前选中的配置, 用于修改
    /// </summary>
    public SetConfig SelectedSetConfig => _mainVm.SelectedSetConfig;

    public Dictionary<Guid, SetConfig> SetConfigs => _mainVm.SetConfigs;

    public CQAPI Api => _mainVm.CqApi;
}