namespace SheepQQBot3.DBModel
{
    public partial class SetuDoushiInfo
    {
        public SetuDoushiInfo()
        {
        }

        public SetuDoushiInfo(long targetId)
        {
            TargetId = targetId;
            SetuDoushiLv = 0;
            SetuCD = 0;
        }
    }
}