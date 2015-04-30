using System;
using Newtonsoft.Json.Linq;

namespace BiliDMLib
{
    public class DanmakuModel
    {
        public string CommentText { get; set; }
        public string CommentUser { get; set; }

        public DanmakuModel()
        {
        }

        public DanmakuModel(string JSON, int version = 1)
        {
            dynamic obj;
            switch (version)
            {
                case 1:
                    obj = JArray.Parse(JSON);
                    CommentText = obj[1].Value;
                    CommentUser = obj[2][1].Value;
                    break;
                case 2:
                    obj = JObject.Parse(JSON);
                    if (obj["cmd"] != "DANMU_MSG")
                    {
                        throw new Exception();
                    }
                    CommentText = obj["info"][1];
                    CommentUser = obj["info"][2][1];
                    break;
                default:
                    throw new Exception();
            }

            //TODO: 還有兩個
        }

    }


}