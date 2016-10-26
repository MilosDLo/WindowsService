using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWinService
{
    public static class JsonDeserializer
    {

        public static object Deserialize(string json)
        {
            return ToObject(JToken.Parse(json));
        }

        private static object ToObject(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return token.Children<JProperty>()
                                .ToDictionary(prop => prop.Name,
                                              prop => ToObject(prop.Value));

                case JTokenType.Array:
                    return token.Select(ToObject).ToList();

                default:
                    return ((JValue)token).Value;
            }
        }

        public static List<ListOneOrder> GetListOfOrders(string json)
        {

            Dictionary<string, Object> map = (Dictionary<string, Object>)Deserialize(json);
            List<Object> list = (List<Object>)map["data"];

            List<ListOneOrder> _newList = new List<ListOneOrder>();

            foreach (Object _object in list)
            {
                Dictionary<string, Object> _oneItem = (Dictionary<string, Object>)_object;

                ListOneOrder _one = new ListOneOrder();

                var i = _oneItem["id"];
                _one.ID = i.ToString();
                _one.Status = (string)_oneItem["status"];
                _newList.Add(_one);
            }
            return _newList;
        }

    }
}
