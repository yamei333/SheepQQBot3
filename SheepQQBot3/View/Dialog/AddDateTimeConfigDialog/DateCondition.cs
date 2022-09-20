namespace SheepQQBot3.View
{
    public class DateCondition
    {
        public string ConditionText { get; set; }
        public bool IsAlarm { get; set; }
        public int Value { get; set; }

        public DateCondition(string conditionText, bool isAlarm, int value)
        {
            ConditionText = conditionText;
            IsAlarm = isAlarm;
            Value = value;
        }
    }
}