using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    /// <summary>
    /// 表情包枚举定义
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AIEmojiType
    {
        [Description("无")]
        None = 0,

        #region 正面情绪 / 开心 / 期待

        [Description("做得好、干得好、太棒了")]
        goodjob,

        [Description("好的、OK")]
        ok,

        [Description("开心、幸福")]
        kaixin,

        [Description("太好了")]
        thl,

        [Description("兴奋")]
        xingfen,

        [Description("两眼闪闪发光")]
        kirakira,

        [Description("嘻嘻")]
        xixi,

        [Description("心花怒放")]
        xhnf,

        [Description("我真帅气、爱你哟、比心")]
        meigui,

        [Description("打call")]
        dacall,

        [Description("看乐了")]
        kanlele,

        [Description("诶嘿")]
        eihei,

        [Description("早上好、早安")]
        zaoan,

        #endregion 正面情绪 / 开心 / 期待

        #region 互动 / 亲密 / 撒娇

        [Description("拜托了")]
        onegai,

        [Description("抱抱你")]
        baobao,

        [Description("卖萌")]
        baowawa,

        [Description("给你心心")]
        gnxx,

        [Description("递玫瑰、给你花花")]
        dmg,

        [Description("给你蛋糕、生日快乐")]
        gndg,

        [Description("陪我玩")]
        pww,

        [Description("亲亲")]
        qinqin,

        [Description("亲我")]
        qinwo,

        [Description("喜欢你")]
        xihuanni,

        [Description("粘人")]
        nian,

        [Description("情书、送情书")]
        qingshu,

        [Description("求抱抱")]
        qiubaobao,

        [Description("探头")]
        tantou,

        [Description("好馋呀、流口水")]
        chan,

        #endregion 互动 / 亲密 / 撒娇

        #region 负面 / 生气 / 拒绝

        [Description("不对、拒绝、NO")]
        no,

        [Description("不回消息")]
        bhxx,

        [Description("哼")]
        heng,

        [Description("骂骂咧咧")]
        mmll,

        [Description("黑化")]
        heihua,

        [Description("生气")]
        shengqi,

        [Description("杂鱼、嘲笑")]
        zako,

        [Description("变态")]
        hentai,

        [Description("猫猫哈气")]
        haqi,

        [Description("你好、拒绝")]
        nihao,

        [Description("猫出入禁止")]
        mjz,

        #endregion 负面 / 生气 / 拒绝

        #region 难过 / 委屈 / 害怕 / 道歉

        [Description("懊悔")]
        kuyasi,

        [Description("不好意思、抱歉、对不起")]
        sorry,

        [Description("害怕")]
        haipa,

        [Description("哭哭")]
        kuku,

        [Description("泪目、快要哭了")]
        leimu,

        [Description("失落")]
        shiluo,

        [Description("委屈")]
        weiqu,

        [Description("含泪掏钱")]
        hltq,

        [Description("没钱了、贫穷")]
        mql,

        #endregion 难过 / 委屈 / 害怕 / 道歉

        #region 害羞 / 脸红

        [Description("害羞")]
        haixiu,

        [Description("脸红(爱心)")]
        lhax,

        [Description("脸红(生气)")]
        lhsq,

        [Description("被捏脸")]
        beinielian,

        [Description("被捏脸")]
        beinl,

        [Description("被摸头")]
        beimotou,

        [Description("摸头")]
        motou,

        [Description("捂嘴、说错了、失言了")]
        wuzui,

        #endregion 害羞 / 脸红

        #region 惊讶 / 疑惑 / 混乱

        [Description("不是这样")]
        bszy,

        [Description("大脑过载")]
        dngz,

        [Description("疑惑")]
        yihuo,

        [Description("晕了")]
        yunle,

        [Description("怎么会这样")]
        sonna,

        [Description("不得了")]
        budeliao,

        [Description("搞砸了、别骂了")]
        gaozale,

        [Description("啊？、诶？")]
        what,

        [Description("呆住")]
        dz,

        [Description("灵魂出窍")]
        linghun,

        [Description("升天")]
        shengtian,

        [Description("顷刻炼化")]
        qklh,

        [Description("我的呢")]
        wodene,

        #endregion 惊讶 / 疑惑 / 混乱

        #region 状态 / 日常 / 动作

        [Description("等待")]
        dengdai,

        [Description("喝茶, 喝咖啡, 得意")]
        hecha,

        [Description("盒武器")]
        hewuqi,

        [Description("黑线")]
        heixian,

        [Description("寄")]
        ji,

        [Description("快点睡觉")]
        kdsj,

        [Description("流汗")]
        liuhan,

        [Description("喵")]
        mm,

        [Description("叹气")]
        tanqi,

        [Description("偷看")]
        tk,

        [Description("调皮")]
        tp,

        [Description("晚安")]
        wanan,

        [Description("睡觉中、困困、zzz")]
        zzz,

        [Description("不行了")]
        bxl,

        [Description("辛苦了")]
        xkl,

        [Description("炫耀、得意")]
        jiman,

        [Description("逃了")]
        taole,

        [Description("拍照")]
        paizhao,

        [Description("摸鱼")]
        moyu,

        [Description("冒泡")]
        maopao,

        [Description("吃蛋糕")]
        chidg,

        [Description("画画")]
        huahua,

        [Description("塞瑞士卷, 呜咕")]
        ruishijuan,

        [Description("投降")]
        touxiang,

        [Description("吐舌")]
        tushe,

        [Description("小光板")]
        xgb,

        [Description("在做什么呢")]
        zaizuosm

        #endregion 状态 / 日常 / 动作
    }
}