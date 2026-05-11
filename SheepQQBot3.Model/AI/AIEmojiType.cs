using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        [Display(Name = "无")]
        None = 0,

        #region 正面情绪 / 开心 / 期待

        [Description("做得好、干得好、太棒了")]
        [Display(Name = "GoodJob")]
        goodjob,

        [Description("好的、OK")]
        [Display(Name = "OK")]
        ok,

        [Description("开心、幸福")]
        [Display(Name = "开心")]
        kaixin,

        [Description("太好了")]
        [Display(Name = "太好了")]
        taihaole,

        [Description("兴奋")]
        [Display(Name = "兴奋")]
        xingfen,

        [Description("两眼闪闪发光")]
        [Display(Name = "闪闪发光")]
        kirakira,

        [Description("嘻嘻")]
        [Display(Name = "嘻嘻")]
        xixi,

        [Description("心花怒放")]
        [Display(Name = "心花怒放")]
        xinhuanufang,

        [Description("我真帅气、爱你哟、比心")]
        [Display(Name = "玫瑰")]
        meigui,

        [Description("打Call")]
        [Display(Name = "打Call")]
        dacall,

        [Description("看乐了")]
        [Display(Name = "乐了")]
        kanlele,

        [Description("诶嘿")]
        [Display(Name = "诶嘿")]
        eihei,

        [Description("早上好、早安")]
        [Display(Name = "早安")]
        zaoan,

        #endregion 正面情绪 / 开心 / 期待

        #region 互动 / 亲密 / 撒娇

        [Description("拜托了")]
        [Display(Name = "拜托了")]
        onegai,

        [Description("抱抱你")]
        [Display(Name = "抱抱")]
        baobao,

        [Description("卖萌")]
        [Display(Name = "卖萌")]
        baowawa,

        [Description("给你心心")]
        [Display(Name = "比心")]
        geinixinxin,

        [Description("递玫瑰、给你花花")]
        [Display(Name = "给你花花")]
        geinihuahua,

        [Description("给你蛋糕、生日快乐")]
        [Display(Name = "给你蛋糕")]
        geinidangao,

        [Description("陪我玩")]
        [Display(Name = "陪我玩")]
        peiwowan,

        [Description("亲亲")]
        [Display(Name = "亲亲")]
        qinqin,

        [Description("亲我")]
        [Display(Name = "亲我")]
        qinwo,

        [Description("喜欢你")]
        [Display(Name = "喜欢你")]
        xihuanni,

        [Description("粘人")]
        [Display(Name = "粘")]
        nian,

        [Description("情书、送情书")]
        [Display(Name = "情书")]
        qingshu,

        [Description("求抱抱")]
        [Display(Name = "求抱抱")]
        qiubaobao,

        [Description("探头")]
        [Display(Name = "探头")]
        tantou,

        [Description("好馋呀、流口水")]
        [Display(Name = "馋")]
        chan,

        [Description("想你")]
        [Display(Name = "想你")]
        xiangni,

        #endregion 互动 / 亲密 / 撒娇

        #region 负面 / 生气 / 拒绝

        [Description("不对、NO")]
        [Display(Name = "NO")]
        no,

        [Description("不回消息是吧")]
        [Display(Name = "不回消息")]
        bhxx,

        [Description("哼")]
        [Display(Name = "哼")]
        heng,

        [Description("骂骂咧咧")]
        [Display(Name = "骂骂咧咧")]
        mamalielie,

        [Description("黑化")]
        [Display(Name = "黑化")]
        heihua,

        [Description("炸毛、破防、气急败坏")]
        [Display(Name = "气急败坏")]
        qijibaihuai,

        [Description("赌气、闹别扭、生闷气")]
        [Display(Name = "赌气")]
        duqi,

        [Description("杂鱼、嘲笑")]
        [Display(Name = "杂鱼")]
        zako,

        [Description("变态")]
        [Display(Name = "变态")]
        hentai,

        [Description("猫猫哈气、哈气")]
        [Display(Name = "哈气")]
        haqi,

        [Description("你好")]
        [Display(Name = "你好")]
        nihao,

        [Description("猫出入禁止")]
        [Display(Name = "猫出入禁止")]
        mjz,

        #endregion 负面 / 生气 / 拒绝

        #region 难过 / 委屈 / 害怕 / 道歉

        [Description("懊悔")]
        [Display(Name = "懊悔、令人遗憾")]
        kuyasi,

        [Description("不好意思、抱歉、对不起")]
        [Display(Name = "抱歉")]
        sorry,

        [Description("害怕、抖")]
        [Display(Name = "抖")]
        haipa,

        [Description("哭哭")]
        [Display(Name = "哭哭")]
        kuku,

        [Description("泪目、快要哭了")]
        [Display(Name = "泪目")]
        leimu,

        [Description("失落")]
        [Display(Name = "失落")]
        shiluo,

        [Description("委屈")]
        [Display(Name = "委屈")]
        weiqu,

        [Description("含泪掏钱")]
        [Display(Name = "含泪掏钱")]
        hanleitaoqian,

        [Description("没钱了、贫穷")]
        [Display(Name = "贫穷")]
        meiqianle,

        #endregion 难过 / 委屈 / 害怕 / 道歉

        #region 害羞 / 脸红

        [Description("害羞")]
        [Display(Name = "害羞")]
        haixiu,

        [Description("脸红(爱心)")]
        [Display(Name = "脸红(爱心)")]
        lhax,

        [Description("脸红(生气)")]
        [Display(Name = "脸红(生气)")]
        lhsq,

        [Description("被捏脸")]
        [Display(Name = "被捏脸")]
        beinielian,

        [Description("被摸头")]
        [Display(Name = "被摸头")]
        beimotou,

        [Description("摸头")]
        [Display(Name = "摸头")]
        motou,

        [Description("捂嘴、说错了、失言了")]
        [Display(Name = "捂嘴")]
        wuzui,

        #endregion 害羞 / 脸红

        #region 惊讶 / 疑惑 / 混乱

        [Description("不是这样")]
        [Display(Name = "不是这样")]
        bushizheyang,

        [Description("大脑过载")]
        [Display(Name = "大脑过载")]
        danaoguozai,

        [Description("疑惑")]
        [Display(Name = "疑惑")]
        yihuo,

        [Description("晕了")]
        [Display(Name = "晕了")]
        yunle,

        [Description("怎么会这样、哦不")]
        [Display(Name = "怎么会这样")]
        sonna,

        [Description("不得了")]
        [Display(Name = "不得了")]
        budeliao,

        [Description("搞砸了、别骂了")]
        [Display(Name = "搞砸了")]
        gaozale,

        [Description("啊？、诶？、呆住")]
        [Display(Name = "啊？")]
        what,

        [Description("灵魂出窍")]
        [Display(Name = "灵魂出窍")]
        linghun,

        [Description("升天")]
        [Display(Name = "升天")]
        shengtian,

        [Description("顷刻炼化")]
        [Display(Name = "顷刻炼化")]
        qingkelianhua,

        [Description("我的呢")]
        [Display(Name = "我的呢")]
        wodene,

        #endregion 惊讶 / 疑惑 / 混乱

        #region 状态 / 日常 / 动作

        [Description("盯、等待")]
        [Display(Name = "盯")]
        dengdai,

        [Description("喝茶, 喝咖啡, 得意")]
        [Display(Name = "喝茶")]
        hecha,

        [Description("盒武器")]
        [Display(Name = "盒")]
        hewuqi,

        [Description("黑线")]
        [Display(Name = "黑线")]
        heixian,

        [Description("寄")]
        [Display(Name = "寄")]
        ji,

        [Description("快点睡觉")]
        [Display(Name = "快点睡觉")]
        kuaidianshuijiao,

        [Description("流汗")]
        [Display(Name = "流汗")]
        liuhan,

        [Description("喵、喵喵")]
        [Display(Name = "喵喵")]
        miaomiao,

        [Description("叹气")]
        [Display(Name = "叹气")]
        tanqi,

        [Description("偷看")]
        [Display(Name = "偷看")]
        toukan,

        [Description("调皮")]
        [Display(Name = "调皮")]
        tiaopi,

        [Description("晚安")]
        [Display(Name = "晚安")]
        wanan,

        [Description("睡觉中、困困、zzz")]
        [Display(Name = "zzz")]
        zzz,

        [Description("不行了")]
        [Display(Name = "我不行了")]
        buxingle,

        [Description("辛苦了")]
        [Display(Name = "辛苦了")]
        xinkule,

        [Description("炫耀、得意")]
        [Display(Name = "得意")]
        jiman,

        [Description("逃了")]
        [Display(Name = "逃了")]
        taole,

        [Description("拍照、臭美")]
        [Display(Name = "拍照")]
        paizhao,

        [Description("摸鱼")]
        [Display(Name = "摸鱼")]
        moyu,

        [Description("冒泡")]
        [Display(Name = "冒泡")]
        maopao,

        [Description("吃蛋糕")]
        [Display(Name = "吃蛋糕")]
        chidg,

        [Description("画画")]
        [Display(Name = "画画")]
        huahua,

        [Description("塞瑞士卷, 呜咕")]
        [Display(Name = "塞瑞士卷")]
        ruishijuan,

        [Description("投降")]
        [Display(Name = "投降喵")]
        touxiang,

        [Description("吐舌")]
        [Display(Name = "吐舌")]
        tushe,

        [Description("小光板")]
        [Display(Name = "小光板")]
        xgb,

        [Description("在做什么呢")]
        [Display(Name = "在做什么呢")]
        zaizuosm,

        [Description("比耶(墨镜)")]
        [Display(Name = "耶")]
        ye,

        [Description("喝牛奶")]
        [Display(Name = "喝牛奶")]
        heniunai,

        #endregion 状态 / 日常 / 动作

        [Description("0分")]
        [Display(Name = "0分")]
        fen_0,

        [Description("10分")]
        [Display(Name = "10分")]
        fen_10,

        [Description("20分")]
        [Display(Name = "20分")]
        fen_20,

        [Description("30分")]
        [Display(Name = "30分")]
        fen_30,

        [Description("40分")]
        [Display(Name = "40分")]
        fen_40,

        [Description("50分")]
        [Display(Name = "50分")]
        fen_50,

        [Description("60分")]
        [Display(Name = "60分")]
        fen_60,

        [Description("70分")]
        [Display(Name = "70分")]
        fen_70,

        [Description("80分")]
        [Display(Name = "80分")]
        fen_80,

        [Description("90分")]
        [Display(Name = "90分")]
        fen_90,

        [Description("100分")]
        [Display(Name = "100分")]
        fen_100,

        [Description("令人窒息")]
        [Display(Name = "窒息")]
        zhixi,

        [Description("红包")]
        [Display(Name = "红包")]
        hongbao,

        [Description("吃饼干")]
        [Display(Name = "吃饼干")]
        chibing,

        [Description("举喇叭")]
        [Display(Name = "举喇叭")]
        laba,

        [Description("上吊、被吊住")]
        [Display(Name = "吊")]
        shangdiao,

        [Description("捡到宝了、捡到钱了")]
        [Display(Name = "捡到宝了")]
        jiandaoqian,

        [Description("使用券、哈莉使用券")]
        [Display(Name = "使用券")]
        shiyongquan,

        [Description("没人带我玩、练游戏")]
        [Display(Name = "没人带我玩")]
        meirenwan,

        [Description("被揪舌头")]
        [Display(Name = "揪舌头")]
        beijiushetou,

        [Description("跟你爆了")]
        [Display(Name = "跟你爆了")]
        gennibaole,

        [Description("干嘛……")]
        [Display(Name = "干嘛……")]
        ganma,

        [Description("干什么!!")]
        [Display(Name = "干什么!!")]
        ganshenme,

        [Description("这家伙在说什么呢")]
        [Display(Name = "在说什么")]
        zaishuoshenme,

        [Description("来财")]
        [Display(Name = "来财")]
        laicai,

        [Description("变成宠物、当狗、被拿捏")]
        [Display(Name = "变成宠物")]
        bianchengchongwu,

        [Description("放大镜")]
        [Display(Name = "放大镜")]
        fangdajing,

        [Description("时间差不多咯")]
        [Display(Name = "时间差不多咯")]
        timeover,

        [Description("打搅了")]
        [Display(Name = "打搅了")]
        dajiaole,

        [Description("何意味")]
        [Display(Name = "何意味")]
        heyiwei,

        [Description("可怜巴巴")]
        [Display(Name = "可怜巴巴")]
        kelianbaba,

        [Description("看手机")]
        [Display(Name = "看手机")]
        kanshouji,

        [Description("全都不会、全死光了就我没死")]
        [Display(Name = "全都不会")]
        quandoubuhui,

        [Description("这么强?!")]
        [Display(Name = "这么强?!")]
        zhemeqiang,

        [Description("摸摸群友")]
        [Display(Name = "摸摸群友")]
        momoqunyou,

        [Description("震撼美味(贬义)")]
        [Display(Name = "震撼美味")]
        zhenghanmeiwei,
    }
}