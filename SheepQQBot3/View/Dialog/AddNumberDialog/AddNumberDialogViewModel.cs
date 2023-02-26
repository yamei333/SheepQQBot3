using SheepQQBot3.Model;

namespace SheepQQBot3.View
{
    public class AddNumberDialogViewModel : NotifyPropertyChangedBase
    {
        public AddNumberDialogViewModel()
        {
            AddNumber = null;
        }

        private int? _addNumber;
        /// <summary>
        /// 新增的数字
        /// </summary>
        public int? AddNumber
        {
            get => _addNumber;
            set
            {
                if (_addNumber == value)
                    return;

                _addNumber = value;
                OnPropertyChanged(nameof(AddNumber));
            }
        }
    }
}