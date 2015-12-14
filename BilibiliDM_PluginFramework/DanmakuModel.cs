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
        /// 歡迎
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
        Unknown
    }

    public class DanmakuModel
    {
        /// <summary>
        /// 彈幕內容
        /// </summary>
        public string CommentText { get; set; }

        /// <summary>
        /// 彈幕用戶
        /// </summary>
        public string CommentUser { get; set; }

        /// <summary>
        /// 消息類型
        /// </summary>
        public MsgTypeEnum MsgType { get; set; }

        /// <summary>
        /// 禮物用戶
        /// </summary>
        public string GiftUser { get; set; }

        /// <summary>
        /// 禮物名稱
        /// </summary>
        public string GiftName { get; set; }

        /// <summary>
        /// 禮物數量
        /// </summary>
        public string GiftNum { get; set; }

        /// <summary>
        /// 不明字段
        /// </summary>
        public string Giftrcost { get; set; }

        /// <summary>
        /// 禮物排行
        /// </summary>
        public List<GiftRank> GiftRanking { get; set; }

        /// <summary>
        /// 該用戶是否為管理員
        /// </summary>
        public bool isAdmin { get; set; }

        /// <summary>
        /// 是否VIP用戶(老爺)
        /// </summary>
        public bool isVIP { get; set; }
        /// <summary>
        /// LiveStart,LiveEnd 事件对应的房间号
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
            switch (version)
            {
                case 1:
                {
                    var obj = JArray.Parse(JSON);

                    CommentText = obj[1].ToString();
                    CommentUser = obj[2][1].ToString();
                    MsgType = MsgTypeEnum.Comment;
                    break;
                }
                case 2:
                {
                    var obj = JObject.Parse(JSON);

                    string cmd = obj["cmd"].ToString();
                    switch (cmd)
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
                            CommentText = obj["info"][1].ToString();
                            CommentUser = obj["info"][2][1].ToString();
                            isAdmin = obj["info"][2][2].ToString() == "1";
                            isVIP = obj["info"][2][3].ToString() == "1";
                            MsgType = MsgTypeEnum.Comment;
                            break;
                        case "SEND_GIFT":
                            MsgType = MsgTypeEnum.GiftSend;
                            GiftName = obj["data"]["giftName"].ToString();
                            GiftUser = obj["data"]["uname"].ToString();
                            Giftrcost = obj["data"]["rcost"].ToString();
                            GiftNum = obj["data"]["num"].ToString();
                            break;
                        case "GIFT_TOP":
                        {
                            MsgType = MsgTypeEnum.GiftTop;
                            var alltop = obj["data"].ToList();
                            GiftRanking = new List<GiftRank>();
                            foreach (var v in alltop)
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
                            CommentUser = obj["data"]["uname"].ToString();
                            isVIP = true;
                            isAdmin = obj["data"]["isadmin"].ToString() == "1";
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