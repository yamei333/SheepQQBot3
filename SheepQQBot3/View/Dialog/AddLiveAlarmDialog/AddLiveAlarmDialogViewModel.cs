using System.Collections.Generic;
using System.Linq;
using SheepQQBot3.Model;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.View
{
    public class AddLiveAlarmDialogViewModel : NotifyPropertyChangedBase
    {
        private AddLiveAlarmDialogConfig _selectedAddLiveAlarmDialogConfig;
        private List<AddLiveAlarmDialogConfig> _addLiveAlarmDialogConfigs;

        public AddLiveAlarmDialogViewModel()
        {
            LiveRoomId = null;
            var addLiveAlarmDialogConfigs = new List<AddLiveAlarmDialogConfig>
            {
                new AddLiveAlarmDialogConfig(LiveType.Bilibili, "B站"),
            };
            AddLiveAlarmDialogConfigs = addLiveAlarmDialogConfigs;
            SelectedAddLiveAlarmDialogConfig = addLiveAlarmDialogConfigs.First();
        }

        /// <summary>
        /// 类型
        /// </summary>
        public List<AddLiveAlarmDialogConfig> AddLiveAlarmDialogConfigs
        {
            get => _addLiveAlarmDialogConfigs;
            set
            {
                if (_addLiveAlarmDialogConfigs == value)
                    return;

                _addLiveAlarmDialogConfigs = value;
                OnPropertyChanged(nameof(AddLiveAlarmDialogConfigs));
            }
        }

        /// <summary>
        /// 当前选中类型
        /// </summary>
        public AddLiveAlarmDialogConfig SelectedAddLiveAlarmDialogConfig
        {
            get => _selectedAddLiveAlarmDialogConfig;
            set
            {
                _selectedAddLiveAlarmDialogConfig = value;
                OnPropertyChanged(nameof(SelectedAddLiveAlarmDialogConfig));
            }
        }

        private long? _liveRoomId;
        /// <summary>
        /// 直播间房间号
        /// </summary>
        public long? LiveRoomId
        {
            get => _liveRoomId;
            set
            {
                _liveRoomId = value;
                OnPropertyChanged(nameof(LiveRoomId));
            }
        }
    }
}