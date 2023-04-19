using System;
using Yamei.Common;

namespace SheepQQBot3.DbModel
{
    public partial class SetuDoushiInfo
    {
        public long CalcSetuDoushiLv(DateTime dateNow)
        {
            var setuDoushiLv = SetuDoushiLv;
            if (setuDoushiLv <= 0)
                return setuDoushiLv;

            var setuCd = SetuCD.ToDateTime();
            if (setuCd != DateTime.MinValue)
                return setuDoushiLv;

            var changeLvTimePoint = (long)((dateNow - setuCd).TotalMinutes / 90);
            while (setuDoushiLv > 0 && changeLvTimePoint >= setuDoushiLv)
            {
                changeLvTimePoint -= setuDoushiLv;
                setuDoushiLv--;
            }

            return setuDoushiLv;
        }

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