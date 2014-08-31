using Newtonsoft.Json.Linq;

namespace BiliDMLib
{
    public class DanmakuModel
    {
        public string CommentText { get; set; }
        public string CommentUser { get; set; }
        public DanmakuModel(){}

        public DanmakuModel(string JSON)
        {
            dynamic obj = JArray.Parse(JSON);
            CommentText= obj[1].Value;
            CommentUser = obj[2][1].Value;
            //TODO: 還有兩個

        }
    }
}