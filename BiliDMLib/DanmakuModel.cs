using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BiliDMLib
{
    public enum MsgTypeEnum
    {
        Comment,GiftSend,GiftTop,Welcome
    }

    public class DanmakuModel
    {
        public string CommentText { get; set; }
        public string CommentUser { get; set; }

        public MsgTypeEnum MsgType { get; set; }

        public string GiftUser { get; set; }
        public string GiftName { get; set; }
        public string GiftNum { get; set; }
        public string Giftrcost { get; set; }
        public List<GiftRank> GiftRanking { get; set; }
        public bool isAdmin { get; set; }
        public bool isVIP { get; set; }
        public DanmakuModel()
        {
        }

        public DanmakuModel(string JSON, int version = 1)
        {

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
                            MsgType=MsgTypeEnum.Welcome;
                            CommentUser = obj["data"]["uname"].ToString();
                            isVIP = true;
                            isAdmin = obj["data"]["isadmin"].ToString() == "1";
                            break;

                        }
                            default:
                                throw new Exception();
                    }

                    break;
                }

                default:
                    throw new Exception();
            }

           
        }

    }



}