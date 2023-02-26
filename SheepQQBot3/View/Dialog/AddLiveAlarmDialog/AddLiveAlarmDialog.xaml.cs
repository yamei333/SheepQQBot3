using System.Linq;
using System.Windows;
using SheepQQBot3.Enums;
using SheepQQBot3.Model.Enums;

namespace SheepQQBot3.View
{
    /// <summary>
    /// AddLiveAlarmDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddLiveAlarmDialog
        : AddDialogWindowBase<AddLiveAlarmDialogViewModel>
    {
        /// <summary>
        /// 直播间房间号
        /// </summary>
        public long LiveRoomId { get; set; }

        /// <summary>
        /// 直播平台(类型)
        /// </summary>
        public LiveType LiveType { get; set; }

        /// <inheritdoc />
        public AddLiveAlarmDialog(Window owner, object menuItem, DialogMode mode)
            : base(owner, menuItem, mode)
            => InitializeComponent();

        /// <inheritdoc />
        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Mode != DialogMode.Edit)
                return;

            Vm.LiveRoomId = LiveRoomId;
            Vm.SelectedAddLiveAlarmDialogConfig = Vm.AddLiveAlarmDialogConfigs
                .First(each => each.LiveType == LiveType);

            if (Mode == DialogMode.Add)
                TxtLiveRoomId.SelectAll();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            LiveRoomId = Vm.LiveRoomId.GetValueOrDefault();
            LiveType = Vm.SelectedAddLiveAlarmDialogConfig.LiveType;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}