using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using System;
using System.Net.Http;

namespace Nagornev.Querer.Http
{
    public static class QuererHttpResponseMessageExtensions
    {
        public static string GetText(this HttpResponseMessage response)
        {
            return response.Content.ReadAsStringAsync().Result;
        }

        public static byte[] GetBytes(this HttpResponseMessage response)
        {
            return response.Content.ReadAsByteArrayAsync().Result;
        }

        public static JToken GetJson(this HttpResponseMessage response)
        {
            return JObject.Parse(response.GetText());
        }

        public static HtmlDocument GetHtml(this HttpResponseMessage response)
        {
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(response.GetText());

            return html;
        }

        public static TProtobuffType GetProtobuff<TProtobuffType>(this HttpResponseMessage response)
        {
            return Serializer.Deserialize<TProtobuffType>(response.Content.ReadAsStreamAsync().Result);
        }

        public static TContentType GetContent<TContentType>(this HttpResponseMessage response, Func<byte[], TContentType> callback)
        {
            return callback.Invoke(response.GetBytes());
        }

        public static TContentType GetContent<TContentType>(this HttpResponseMessage response, Func<string, TContentType> callback)
        {
            return callback.Invoke(response.GetText());
        }

        public static TContentType GetContent<TContentType>(this HttpResponseMessage response, Func<JToken, TContentType> callback)
        {
            return callback.Invoke(response.GetJson());
        }

        public static TContentType GetContent<TContentType>(this HttpResponseMessage response, Func<HtmlDocument, TContentType> callback)
        {
            return callback.Invoke(response.GetHtml());
        }
    }
}
