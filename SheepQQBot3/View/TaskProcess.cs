using System;

namespace SheepQQBot3.View
{
    public static partial class TaskProcess
    {
        public static Func<bool> NoneCondition => () => false;

        /// <summary>
        /// <see cref="DayOfWeek"/>转换为数字
        /// </summary>
        private static int DayOfWeek2Int(DayOfWeek dayOfWeek)
            => dayOfWeek == DayOfWeek.Sunday ? 7 : (int)dayOfWeek;
    }
}