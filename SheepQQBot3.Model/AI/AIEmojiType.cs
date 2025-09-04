using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AIEmojiType
    {
        None = 0,

        /// <summary>
        /// 做得好、干得好、太棒了
        /// </summary>
        goodjob,

        /// <summary>
        /// 不对、拒绝、NO
        /// </summary>
        no,

        /// <summary>
        /// 好的、OK
        /// </summary>
        ok,

        /// <summary>
        /// 拜托了
        /// </summary>
        onegai,

        /// <summary>
        /// 抱抱你
        /// </summary>
        baobao,

        /// <summary>
        /// 卖萌
        /// </summary>
        baowawa,

        /// <summary>
        /// 被捏脸
        /// </summary>
        beinielian,

        /// <summary>
        /// 懊悔
        /// </summary>
        kuyasi,

        /// <summary>
        /// 不好意思、抱歉、对不起
        /// </summary>
        sorry,

        /// <summary>
        /// 不回消息
        /// </summary>
        bhxx,

        /// <summary>
        /// 不是这样
        /// </summary>
        bszy,

        /// <summary>
        /// 好馋呀、流口水
        /// </summary>
        chan,

        /// <summary>
        /// 大脑过载
        /// </summary>
        dngz,

        /// <summary>
        /// 被摸头
        /// </summary>
        beimotou,

        /// <summary>
        /// 等待
        /// </summary>
        dengdai,

        /// <summary>
        /// 给你心心
        /// </summary>
        gnxx,

        /// <summary>
        /// 递玫瑰、给你花花
        /// </summary>
        dmg,

        /// <summary>
        /// 诶嘿
        /// </summary>
        eihei,

        /// <summary>
        /// 给你蛋糕、生日快乐
        /// </summary>
        gndg,

        /// <summary>
        /// 猫猫哈气
        /// </summary>
        haqi,

        /// <summary>
        /// 害怕
        /// </summary>
        haipa,

        /// <summary>
        /// 害羞
        /// </summary>
        haixiu,

        /// <summary>
        /// 喝茶, 喝咖啡, 得意
        /// </summary>
        hecha,

        /// <summary>
        /// 盒武器
        /// </summary>
        hewuqi,

        /// <summary>
        /// 黑线
        /// </summary>
        heixian,

        /// <summary>
        /// 哼
        /// </summary>
        heng,

        /// <summary>
        /// 寄
        /// </summary>
        ji,

        /// <summary>
        /// 开心
        /// </summary>
        kaixin,

        /// <summary>
        /// 看乐了
        /// </summary>
        kanlele,

        /// <summary>
        /// 哭哭
        /// </summary>
        kuku,
        kdsj,
        leimu,
        lhax,
        lhsq,
        liuhan,
        mmll,
        mjz,
        mql,
        mm,

        /// <summary>
        /// 黑化
        /// </summary>
        heihua,

        /// <summary>
        /// 你好、拒绝
        /// </summary>
        nihao,

        /// <summary>
        /// 陪我玩
        /// </summary>
        pww,

        /// <summary>
        /// 心花怒放
        /// </summary>
        xhnf,

        /// <summary>
        /// 灵魂出窍
        /// </summary>
        linghun,

        /// <summary>
        /// 亲亲
        /// </summary>
        qinqin,

        /// <summary>
        /// 亲我
        /// </summary>
        qinwo,

        /// <summary>
        /// 顷刻炼化
        /// </summary>
        qklh,

        /// <summary>
        /// 生气
        /// </summary>
        shengqi,

        /// <summary>
        /// 升天
        /// </summary>
        shengtian,

        /// <summary>
        /// 太好了
        /// </summary>
        thl,

        /// <summary>
        /// 叹气
        /// </summary>
        tanqi,

        tk,
        tp,

        /// <summary>
        /// 晚安
        /// </summary>
        wanan,

        bxl,

        /// <summary>
        /// 我的呢
        /// </summary>
        wodene,

        /// <summary>
        /// 喜欢你
        /// </summary>
        xihuanni,

        xkl,
        /// <summary>
        /// 两眼闪闪发光
        /// </summary>
        kirakira,

        dz,
        /// <summary>
        /// 疑惑
        /// </summary>
        yihuo,

        yunle,
        zako,
        zaoan,
        sonna,
        jiman,

        /// <summary>
        /// 逃了
        /// </summary>
        taole,

        meigui,

        /// <summary>
        /// 摸鱼
        /// </summary>
        moyu,

        /// <summary>
        /// 冒泡
        /// </summary>
        maopao,

        /// <summary>
        /// 情书、送情书
        /// </summary>
        qingshu,

        /// <summary>
        /// 吃蛋糕
        /// </summary>
        chidg,

        /// <summary>
        /// 含泪掏钱
        /// </summary>
        hltq,

        /// <summary>
        /// 失落
        /// </summary>
        shiluo,

        /// <summary>
        /// 画画
        /// </summary>
        huahua,

        /// <summary>
        /// 捂嘴、说错了、失言了
        /// </summary>
        wuzui,

        /// <summary>
        /// 兴奋
        /// </summary>
        xingfen,

        /// <summary>
        /// 不得了
        /// </summary>
        budeliao,

        /// <summary>
        /// 搞砸了、别骂了
        /// </summary>
        gaozale,

        /// <summary>
        /// 粘人
        /// </summary>
        nian,

        /// <summary>
        /// 拍照
        /// </summary>
        paizhao,

        /// <summary>
        /// 被捏脸
        /// </summary>
        beinl,

        /// <summary>
        /// 啊？、诶？
        /// </summary>
        what,

        /// <summary>
        /// 睡觉中、困困、zzz
        /// </summary>
        zzz,

        /// <summary>
        /// 变态
        /// </summary>
        hentai,

        /// <summary>
        /// 打call
        /// </summary>
        dacall,

        /// <summary>
        /// 摸头
        /// </summary>
        motou,

        /// <summary>
        /// 求抱抱
        /// </summary>
        qiubaobao,

        /// <summary>
        /// 探头
        /// </summary>
        tantou,

        /// <summary>
        /// 塞瑞士卷, 呜咕
        /// </summary>
        ruishijuan,

        /// <summary>
        /// 投降
        /// </summary>
        touxiang,

        /// <summary>
        /// 吐舌
        /// </summary>
        tushe,

        /// <summary>
        /// 委屈
        /// </summary>
        weiqu,

        /// <summary>
        /// 嘻嘻
        /// </summary>
        xixi,

        /// <summary>
        /// 小光板
        /// </summary>
        xgb,

        /// <summary>
        /// 在做什么呢
        /// </summary>
        zaizuosm,
    }
}