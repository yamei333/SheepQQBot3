namespace SheepQQBot3.Model.Setu;

public class SetuDoushiInfo
{
    public string TargetId { get; set; }

    public long ToFastTimes { get; set; }

    public long SetuCD { get; set; }

    public long BlackListCD { get; set; }

    public SetuDoushiInfo()
    {
    }

    public SetuDoushiInfo(string targetId)
    {
        TargetId = targetId;
        ToFastTimes = 0;
        SetuCD = 0;
        BlackListCD = 0;
    }
}