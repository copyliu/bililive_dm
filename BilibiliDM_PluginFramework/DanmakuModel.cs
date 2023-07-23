using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace BilibiliDM_PluginFramework
{
    /// <summary>
    ///     消息类型
    /// </summary>
    public enum MsgTypeEnum
    {
        /// <summary>
        ///     彈幕
        /// </summary>
        Comment,

        /// <summary>
        ///     禮物
        /// </summary>
        GiftSend,

        /// <summary>
        ///     禮物排名
        /// </summary>
        GiftTop,

        /// <summary>
        ///     歡迎老爷
        /// </summary>
        Welcome,

        /// <summary>
        ///     直播開始
        /// </summary>
        LiveStart,

        /// <summary>
        ///     直播結束
        /// </summary>
        LiveEnd,

        /// <summary>
        ///     其他
        /// </summary>
        Unknown,

        /// <summary>
        ///     欢迎船员
        /// </summary>
        WelcomeGuard,

        /// <summary>
        ///     购买船票（上船）
        /// </summary>
        GuardBuy,

        /// <summary>
        ///     SC
        /// </summary>
        SuperChat,

        /// <summary>
        ///     观众互动信息
        /// </summary>
        Interact,

        /// <summary>
        ///     超管警告
        /// </summary>
        Warning,

        /// <summary>
        ///     观看人数, 可能是人次?
        /// </summary>
        WatchedChange
    }

    /// <summary>
    ///     观众互动内容
    /// </summary>
    public enum InteractTypeEnum
    {
        /// <summary>
        ///     进入
        /// </summary>
        Enter = 1,

        /// <summary>
        ///     关注
        /// </summary>
        Follow = 2,

        /// <summary>
        ///     分享直播间
        /// </summary>
        Share = 3,

        /// <summary>
        ///     特别关注
        /// </summary>
        SpecialFollow = 4,

        /// <summary>
        ///     互相关注
        /// </summary>
        MutualFollow = 5
    }

    public class DanmakuModel
    {
        public static Regex EntryEffRegex = new Regex(@"\<%(.+?)%\>");

        public DanmakuModel()
        {
        }

        public DanmakuModel(string JSON, int version = 1)
        {
#pragma warning disable CS0618 // 类型或成员已过时
            RawData = JSON;
#pragma warning restore CS0618 // 类型或成员已过时
            JSON_Version = version;
            switch (version)
            {
                case 1:
                {
                    var obj = JArray.Parse(JSON);

                    CommentText = obj[1].ToString();
                    UserName = obj[2][1].ToString();
                    MsgType = MsgTypeEnum.Comment;
                    RawDataJToken = obj;
                    break;
                }
                case 2:
                {
                    JObject obj;
                    try
                    {
                        obj = JObject.Parse(JSON);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                    RawDataJToken = obj;
                    var cmd = obj["cmd"].ToString();
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
                            MsgType = MsgTypeEnum.Comment;
                            CommentText = obj["info"][1].ToString();
                            UserID_str = obj["info"][2][0].ToString();
                            try
                            {
                                UserID = Convert.ToInt32(UserID_str);
                            }
                            catch (Exception)
                            {
                                UserID = -1;
                            }

                            try
                            {
                                UserID_long = Convert.ToInt64(UserID_str);
                            }
                            catch (Exception)
                            {
                                UserID_long = -1;
                            }

                            UserName = obj["info"][2][1].ToString();
                            isAdmin = obj["info"][2][2].ToString() == "1";
                            isVIP = obj["info"][2][3].ToString() == "1";
                            UserGuardLevel = obj["info"][7].ToObject<int>();
                            break;
                        case "SEND_GIFT":
                            MsgType = MsgTypeEnum.GiftSend;
                            GiftName = obj["data"]["giftName"].ToString();
                            UserName = obj["data"]["uname"].ToString();
                            UserID_str = obj["data"]["uid"].ToString();
                            try
                            {
                                UserID = Convert.ToInt32(UserID_str);
                            }
                            catch (Exception)
                            {
                                UserID = -1;
                            }

                            try
                            {
                                UserID_long = Convert.ToInt64(UserID_str);
                            }
                            catch (Exception)
                            {
                                UserID_long = -1;
                            }

                            // Giftrcost = obj["data"]["rcost"].ToString();
                            GiftCount = obj["data"]["num"].ToObject<int>();
                            break;
                        case "COMBO_SEND":
                        {
                            MsgType = MsgTypeEnum.Unknown;
                            break;
                            // MsgType = MsgTypeEnum.GiftSend;
                            // GiftName = obj["data"]["gift_name"].ToString();
                            // UserName = obj["data"]["uname"].ToString();
                            // UserID = obj["data"]["uid"].ToObject<int>();
                            // // Giftrcost = obj["data"]["rcost"].ToString();
                            // GiftCount = obj["data"]["total_num"].ToObject<int>();
                            // break;
                        }
                        case "GIFT_TOP":
                        {
                            MsgType = MsgTypeEnum.GiftTop;
                            var alltop = obj["data"].ToList();
                            GiftRanking = new List<GiftRank>();
                            foreach (var v in alltop)
                            {
                                var item = new GiftRank();
                                item.uid_str = v["uid"].ToString();
                                item.UserName = v.Value<string>("uname");
                                item.coin = v.Value<decimal>("coin");
                                try
                                {
                                    item.uid = Convert.ToInt32(item.uid_str);
                                }
                                catch (Exception)
                                {
                                    item.uid = -1;
                                }

                                try
                                {
                                    item.uid_long = Convert.ToInt32(item.uid_str);
                                }
                                catch (Exception)
                                {
                                    item.uid_long = -1;
                                }

                                GiftRanking.Add(item);
                            }

                            break;
                        }
                        case "WELCOME":
                        {
                            MsgType = MsgTypeEnum.Welcome;
                            UserName = obj["data"]["uname"].ToString();
                            UserID_str = obj["data"]["uid"].ToString();
                            try
                            {
                                UserID = Convert.ToInt32(UserID_str);
                            }
                            catch (Exception)
                            {
                                UserID = -1;
                            }

                            try
                            {
                                UserID_long = Convert.ToInt64(UserID_str);
                            }
                            catch (Exception)
                            {
                                UserID_long = -1;
                            }

                            isVIP = true;
                            isAdmin = obj["data"]["isadmin"]?.ToString() == "1";
                            break;
                        }
                        case "WELCOME_GUARD":
                        {
                            MsgType = MsgTypeEnum.WelcomeGuard;
                            UserName = obj["data"]["username"].ToString();
                            UserID_str = obj["data"]["uid"].ToString();
                            try
                            {
                                UserID = Convert.ToInt32(UserID_str);
                            }
                            catch (Exception)
                            {
                                UserID = -1;
                            }

                            try
                            {
                                UserID_long = Convert.ToInt64(UserID_str);
                            }
                            catch (Exception)
                            {
                                UserID_long = -1;
                            }

                            UserGuardLevel = obj["data"]["guard_level"].ToObject<int>();
                            break;
                        }
                        case "ENTRY_EFFECT":
                        {
                            var msg = obj["data"]["copy_writing"] + "";
                            var match = EntryEffRegex.Match(msg);
                            if (match.Success)
                            {
                                MsgType = MsgTypeEnum.WelcomeGuard;
                                UserName = match.Groups[1].Value;
                                UserID_str = obj["data"]["uid"].ToString();
                                try
                                {
                                    UserID = Convert.ToInt32(UserID_str);
                                }
                                catch (Exception)
                                {
                                    UserID = -1;
                                }

                                try
                                {
                                    UserID_long = Convert.ToInt64(UserID_str);
                                }
                                catch (Exception)
                                {
                                    UserID_long = -1;
                                }

                                UserGuardLevel = obj["data"]["privilege_type"].ToObject<int>();
                            }
                            else
                            {
                                MsgType = MsgTypeEnum.Unknown;
                            }

                            break;
                        }
                        case "GUARD_BUY":
                        {
                            MsgType = MsgTypeEnum.GuardBuy;
                            UserID_str = obj["data"]["uid"].ToString();
                            try
                            {
                                UserID = Convert.ToInt32(UserID_str);
                            }
                            catch (Exception)
                            {
                                UserID = -1;
                            }

                            try
                            {
                                UserID_long = Convert.ToInt64(UserID_str);
                            }
                            catch (Exception)
                            {
                                UserID_long = -1;
                            }

                            UserName = obj["data"]["username"].ToString();
                            UserGuardLevel = obj["data"]["guard_level"].ToObject<int>();
                            GiftName = UserGuardLevel == 3 ? "舰长" :
                                UserGuardLevel == 2 ? "提督" :
                                UserGuardLevel == 1 ? "总督" : "";
                            GiftCount = obj["data"]["num"].ToObject<int>();
                            break;
                        }
                        case "SUPER_CHAT_MESSAGE":
                        case "SUPER_CHAT_MESSAGE_JP":
                        {
                            MsgType = MsgTypeEnum.SuperChat;
                            CommentText = obj["data"]["message"]?.ToString();
                            UserID_str = obj["data"]["uid"].ToString();
                            try
                            {
                                UserID = Convert.ToInt32(UserID_str);
                            }
                            catch (Exception)
                            {
                                UserID = -1;
                            }

                            try
                            {
                                UserID_long = Convert.ToInt64(UserID_str);
                            }
                            catch (Exception)
                            {
                                UserID_long = -1;
                            }

                            UserName = obj["data"]["user_info"]["uname"].ToString();
                            Price = obj["data"]["price"].ToObject<decimal>();
                            SCKeepTime = obj["data"]["time"].ToObject<int>();
                            break;
                        }
                        case "INTERACT_WORD":
                        {
                            MsgType = MsgTypeEnum.Interact;
                            UserName = obj["data"]["uname"].ToString();
                            UserID_str = obj["data"]["uid"].ToString();
                            try
                            {
                                UserID = Convert.ToInt32(UserID_str);
                            }
                            catch (Exception)
                            {
                                UserID = -1;
                            }

                            try
                            {
                                UserID_long = Convert.ToInt64(UserID_str);
                            }
                            catch (Exception)
                            {
                                UserID_long = -1;
                            }

                            InteractType = (InteractTypeEnum)obj["data"]["msg_type"].ToObject<int>();
                            break;
                        }
                        case "WARNING":
                        {
                            MsgType = MsgTypeEnum.Warning;
                            CommentText = obj["msg"]?.ToString();

                            break;
                        }
                        case "CUT_OFF":
                        {
                            MsgType = MsgTypeEnum.LiveEnd;
                            CommentText = obj["msg"]?.ToString();
                            break;
                        }
                        case "WATCHED_CHANGE":
                        {
                            MsgType = MsgTypeEnum.WatchedChange;
                            WatchedCount = obj["data"]["num"].ToObject<int>();
                            break;
                        }
                        default:
                        {
                            if (cmd.StartsWith("DANMU_MSG")) // "高考"fix
                            {
                                MsgType = MsgTypeEnum.Comment;
                                CommentText = obj["info"][1].ToString();
                                UserID_str = obj["info"][2][0].ToString();
                                try
                                {
                                    UserID = Convert.ToInt32(UserID_str);
                                }
                                catch (Exception)
                                {
                                    UserID = -1;
                                }

                                try
                                {
                                    UserID_long = Convert.ToInt64(UserID_str);
                                }
                                catch (Exception)
                                {
                                    UserID_long = -1;
                                }

                                UserName = obj["info"][2][1].ToString();
                                isAdmin = obj["info"][2][2].ToString() == "1";
                                isVIP = obj["info"][2][3].ToString() == "1";
                                UserGuardLevel = obj["info"][7].ToObject<int>();
                                break;
                            }

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

        /// <summary>
        ///     消息類型
        /// </summary>
        public MsgTypeEnum MsgType { get; set; }

        public InteractTypeEnum InteractType { get; set; }


        /// <summary>
        ///     彈幕內容
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Comment" />
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        public string CommentText { get; set; }

        /// <summary>
        ///     彈幕用戶
        /// </summary>
        [Obsolete("请使用 UserName", true)]
        public string CommentUser
        {
            get => UserName;
            set => UserName = value;
        }

        /// <summary>
        ///     消息触发者用户名
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Comment" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GiftSend" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Welcome" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.WelcomeGuard" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GuardBuy" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Interact" />
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     消息触发者用户ID, 弃用
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Comment" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GiftSend" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Welcome" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.WelcomeGuard" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GuardBuy" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Interact" />
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        [Obsolete("由于B站开始使用超长UID, 此字段定义已无法满足, 在int范围内的UID会继续赋值, 超范围会赋值为-1, 请使用UserID_long和UserID_str")]
        public int UserID { get; set; }

        /// <summary>
        ///     消息触发者用户ID
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Comment" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GiftSend" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Welcome" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.WelcomeGuard" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GuardBuy" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Interact" />
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        public long UserID_long { get; set; }

        /// <summary>
        ///     消息触发者用户ID
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Comment" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GiftSend" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Welcome" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.WelcomeGuard" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GuardBuy" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Interact" />
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        public string UserID_str { get; set; }

        /// <summary>
        ///     用户舰队等级
        ///     <para>0 为非船员 1 为总督 2 为提督 3 为舰长</para>
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Comment" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.WelcomeGuard" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GuardBuy" />
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        public int UserGuardLevel { get; set; }

        /// <summary>
        ///     禮物用戶
        /// </summary>
        [Obsolete("请使用 UserName", true)]
        public string GiftUser
        {
            get => UserName;
            set => UserName = value;
        }

        /// <summary>
        ///     禮物名稱
        /// </summary>
        public string GiftName { get; set; }

        /// <summary>
        ///     禮物數量
        /// </summary>
        [Obsolete("请使用 GiftCount", true)]
        public string GiftNum => GiftCount.ToString();

        /// <summary>
        ///     礼物数量
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GiftSend" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GuardBuy" />
        ///             </item>
        ///         </list>
        ///     </para>
        ///     <para>此字段也用于标识上船 <see cref="MsgTypeEnum.GuardBuy" /> 的数量（月数）</para>
        /// </summary>
        public int GiftCount { get; set; }

        /// <summary>
        ///     当前房间的礼物积分（Room Cost）
        ///     因以前出现过不传递rcost的礼物，并且用处不大，所以弃用
        /// </summary>
        [Obsolete("如有需要请自行解析RawData", true)]
        public string Giftrcost
        {
            get => "0";
            set { }
        }

        /// <summary>
        ///     禮物排行
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GiftTop" />
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        public List<GiftRank> GiftRanking { get; set; }

        /// <summary>
        ///     该用户是否为房管（包括主播）
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Comment" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.GiftSend" />
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        public bool isAdmin { get; set; }

        /// <summary>
        ///     是否VIP用戶(老爺)
        ///     <para>
        ///         此项有值的消息类型：
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Comment" />
        ///             </item>
        ///             <item>
        ///                 <see cref="MsgTypeEnum.Welcome" />
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        public bool isVIP { get; set; }

        /// <summary>
        ///     <see cref="MsgTypeEnum.LiveStart" />,<see cref="MsgTypeEnum.LiveEnd" /> 事件对应的房间号
        /// </summary>
        public string roomID { get; set; }

        /// <summary>
        ///     原始数据, 高级开发用
        /// </summary>
        [Obsolete("除非确实有需要, 请使用 RawDataJToken 避免多次解析JSON导致性能问题")]
        public string RawData { get; set; }

        /// <summary>
        ///     内部用, JSON数据版本号 通常应该是2
        /// </summary>
        public int JSON_Version { get; set; }

        /// <summary>
        ///     SC的金额, 如果以后有礼物金额也可以放进去
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        ///     SC保留时间
        /// </summary>
        public int SCKeepTime { get; set; }

        /// <summary>
        ///     观看人数 可能是人次?
        /// </summary>
        public long WatchedCount { get; set; }

        /// <summary>
        ///     原始数据, 高级开发用, 如果需要用原始的JSON数据, 建议使用这个而不是用RawData
        /// </summary>
        public JToken RawDataJToken { get; set; }
    }
}