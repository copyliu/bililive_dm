using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BilibiliDM_PluginFramework
{
    public enum MsgTypeEnum
    {
        /// <summary>
        /// 彈幕
        /// </summary>
        Comment,

        /// <summary>
        /// 禮物
        /// </summary>
        GiftSend,

        /// <summary>
        /// 禮物排名
        /// </summary>
        GiftTop,

        /// <summary>
        /// 歡迎老爷
        /// </summary>
        Welcome,

        /// <summary>
        /// 直播開始
        /// </summary>
        LiveStart,

        /// <summary>
        /// 直播結束
        /// </summary>
        LiveEnd,
        /// <summary>
        /// 其他
        /// </summary>
        Unknown,
        /// <summary>
        /// 欢迎船员
        /// </summary>
        WelcomeGuard,
        /// <summary>
        /// 购买船票（上船）
        /// </summary>
        GuardBuy

    }

    public class DanmakuModel
    {
        /// <summary>
        /// 消息類型
        /// </summary>
        public MsgTypeEnum MsgType { get; set; }

        /// <summary>
        /// 彈幕內容
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="MsgTypeEnum.Comment"/></item>
        /// </list></para>
        /// </summary>
        public string CommentText { get; set; }

        /// <summary>
        /// 彈幕用戶
        /// </summary>
        [Obsolete("请使用 UserName")]
        public string CommentUser
        {
            get { return UserName; }
            set { UserName = value; }
        }

        /// <summary>
        /// 消息触发者用户名
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="MsgTypeEnum.Comment"/></item>
        /// <item><see cref="MsgTypeEnum.GiftSend"/></item>
        /// <item><see cref="MsgTypeEnum.Welcome"/></item>
        /// <item><see cref="MsgTypeEnum.WelcomeGuard"/></item>
        /// <item><see cref="MsgTypeEnum.GuardBuy"/></item>
        /// </list></para>
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 消息触发者用户ID
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="MsgTypeEnum.Comment"/></item>
        /// <item><see cref="MsgTypeEnum.GiftSend"/></item>
        /// <item><see cref="MsgTypeEnum.Welcome"/></item>
        /// <item><see cref="MsgTypeEnum.WelcomeGuard"/></item>
        /// <item><see cref="MsgTypeEnum.GuardBuy"/></item>
        /// </list></para>
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 用户舰队等级
        /// <para>0 为非船员 1 为总督 2 为提督 3 为舰长</para>
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="MsgTypeEnum.Comment"/></item>
        /// <item><see cref="MsgTypeEnum.WelcomeGuard"/></item>
        /// <item><see cref="MsgTypeEnum.GuardBuy"/></item>
        /// </list></para>
        /// </summary>
        public int UserGuardLevel { get; set; }

        /// <summary>
        /// 禮物用戶
        /// </summary>
        [Obsolete("请使用 UserName")]
        public string GiftUser
        {
            get { return UserName; }
            set { UserName = value; }
        }

        /// <summary>
        /// 禮物名稱
        /// </summary>
        public string GiftName { get; set; }

        /// <summary>
        /// 禮物數量
        /// </summary>
        [Obsolete("请使用 GiftCount")]
        public string GiftNum { get { return GiftCount.ToString(); } }

        /// <summary>
        /// 礼物数量
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="MsgTypeEnum.GiftSend"/></item>
        /// <item><see cref="MsgTypeEnum.GuardBuy"/></item>
        /// </list></para>
        /// <para>此字段也用于标识上船 <see cref="MsgTypeEnum.GuardBuy"/> 的数量（月数）</para>
        /// </summary>
        public int GiftCount { get; set; }

        /// <summary>
        /// 当前房间的礼物积分（Room Cost）
        /// 因以前出现过不传递rcost的礼物，并且用处不大，所以弃用
        /// </summary>
        [Obsolete("如有需要请自行解析RawData",true)]
        public string Giftrcost { get { return "0"; } set { } }

        /// <summary>
        /// 禮物排行
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="MsgTypeEnum.GiftTop"/></item>
        /// </list></para>
        /// </summary>
        public List<GiftRank> GiftRanking { get; set; }

        /// <summary>
        /// 该用户是否为房管（包括主播）
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="MsgTypeEnum.Comment"/></item>
        /// <item><see cref="MsgTypeEnum.GiftSend"/></item>
        /// </list></para>
        /// </summary>
        public bool isAdmin { get; set; }

        /// <summary>
        /// 是否VIP用戶(老爺)
        /// <para>此项有值的消息类型：<list type="bullet">
        /// <item><see cref="MsgTypeEnum.Comment"/></item>
        /// <item><see cref="MsgTypeEnum.Welcome"/></item>
        /// </list></para>
        /// </summary>
        public bool isVIP { get; set; }

        /// <summary>
        /// <see cref="MsgTypeEnum.LiveStart"/>,<see cref="MsgTypeEnum.LiveEnd"/> 事件对应的房间号
        /// </summary>
        public string roomID { get; set; }

        /// <summary>
        /// 原始数据, 高级开发用
        /// </summary>
        public string RawData { get; set; }

        /// <summary>
        /// 内部用, JSON数据版本号 通常应该是2
        /// </summary>
        public int JSON_Version { get; set; }

        public DanmakuModel()
        {
        }

        public DanmakuModel(string JSON, int version = 1)
        {
            RawData = JSON;
            JSON_Version = version;
            switch(version)
            {
                case 1:
                    {
                        var obj = JArray.Parse(JSON);

                        CommentText = obj[1].ToString();
                        UserName = obj[2][1].ToString();
                        MsgType = MsgTypeEnum.Comment;
                        break;
                    }
                case 2:
                    {
                        var obj = JObject.Parse(JSON);

                        string cmd = obj["cmd"].ToString();
                        switch(cmd)
                        {
                            case "LIVE":
                                MsgType = MsgTypeEnum.LiveStart;
                                roomID = obj["roomid"].ToString();
                                break;
                            case "PREPARING":
                                MsgType = MsgTypeEnum.LiveEnd;
                                roomID = obj["roomid"].ToString();
                                break;
                            case "DANMU_MSG":
                                MsgType = MsgTypeEnum.Comment;
                                CommentText = obj["info"][1].ToString();
                                UserID = obj["info"][2][0].ToObject<int>();
                                UserName = obj["info"][2][1].ToString();
                                isAdmin = obj["info"][2][2].ToString() == "1";
                                isVIP = obj["info"][2][3].ToString() == "1";
                                UserGuardLevel = obj["info"][7].ToObject<int>();
                                break;
                            case "SEND_GIFT":
                                MsgType = MsgTypeEnum.GiftSend;
                                GiftName = obj["data"]["giftName"].ToString();
                                UserName = obj["data"]["uname"].ToString();
                                UserID = obj["data"]["uid"].ToObject<int>();
                                // Giftrcost = obj["data"]["rcost"].ToString();
                                GiftCount = obj["data"]["num"].ToObject<int>();
                                break;
                            case "GIFT_TOP":
                                {
                                    MsgType = MsgTypeEnum.GiftTop;
                                    var alltop = obj["data"].ToList();
                                    GiftRanking = new List<GiftRank>();
                                    foreach(var v in alltop)
                                    {
                                        GiftRanking.Add(new GiftRank()
                                        {
                                            uid = v.Value<int>("uid"),
                                            UserName = v.Value<string>("uname"),
                                            coin = v.Value<decimal>("coin")

                                        });
                                    }
                                    break;
                                }
                            case "WELCOME":
                                {
                                    MsgType = MsgTypeEnum.Welcome;
                                    UserName = obj["data"]["uname"].ToString();
                                    UserID = obj["data"]["uid"].ToObject<int>();
                                    isVIP = true;
                                    isAdmin = obj["data"]["isadmin"].ToString() == "1";
                                    break;

                                }
                            case "WELCOME_GUARD":
                                {
                                    MsgType = MsgTypeEnum.WelcomeGuard;
                                    UserName = obj["data"]["username"].ToString();
                                    UserID = obj["data"]["uid"].ToObject<int>();
                                    UserGuardLevel = obj["data"]["guard_level"].ToObject<int>();
                                    break;
                                }
                            case "GUARD_BUY":
                                {
                                    MsgType = MsgTypeEnum.GuardBuy;
                                    UserID = obj["data"]["uid"].ToObject<int>();
                                    UserName = obj["data"]["username"].ToString();
                                    UserGuardLevel = obj["data"]["guard_level"].ToObject<int>();
                                    GiftName = UserGuardLevel == 3 ? "舰长" : UserGuardLevel == 2 ? "提督" : UserGuardLevel == 1 ? "总督" : "";
                                    GiftCount = obj["data"]["num"].ToObject<int>();
                                    break;
                                }
                            default:
                                {
                                    MsgType = MsgTypeEnum.Unknown;
                                    break;
                                }
                        }

                        break;
                    }

                default:
                    throw new Exception();
            }


        }

    }



}