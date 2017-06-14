using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.WebPages;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Umbraco.Core.Logging;

namespace Our.Umbraco.EnhancedMarkdown
{
    internal static class ConversionHelpers
    {
        public static int TryConvertToInt(this string item)
        {
            int convertedValue;
            int.TryParse(item, out convertedValue);
            return convertedValue;
        }

        public static T TryConvertToModel<T>(this string rawString)
        {
            var model = default(T);

            if (!rawString.IsEmpty())
            {
                if (IsJson(rawString))
                {
                    model = ConvertFromJsonToModel(rawString, model);
                }
                else if (IsXml(rawString))
                {
                    model = ConvertFromXmlToModel(rawString, model);
                }
            }

            return model;
        }

        private static T ConvertFromJsonToModel<T>(string rawString, T model)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    Error = (sender, args) => LogHelper.Error<T>("Error converting model", args.ErrorContext.Error)
                };

                model = JsonConvert.DeserializeObject<T>(rawString, settings);
            }
            catch (JsonException exception)
            {
                LogHelper.Error<T>("Error parsing JSON", exception);
                return model;
            }

            return model;
        }

        private static T ConvertFromXmlToModel<T>(string rawString, T model)
        {
            using (var reader = XmlReader.Create(new StringReader(rawString), new XmlReaderSettings { IgnoreWhitespace = true }))
            {
                try
                {
                    reader.MoveToContent();
                    model = (T)new XmlSerializer(typeof(T)).Deserialize(reader);
                }
                catch (Exception exception)
                {
                    LogHelper.Error<T>("Error parsing XML", exception);
                    return model;
                }
            }

            return model;
        }


        public static IEnumerable<T> TryConvertToCollectionOf<T>(this string rawString)
        {
            var collection = Enumerable.Empty<T>();

            if (!string.IsNullOrEmpty(rawString) && (IsJson(rawString) || IsXml(rawString)))
            {
                var settings = new JsonSerializerSettings
                {
                    Error = (sender, args) => LogHelper.Error<T>("Error converting model collection: " + rawString, args.ErrorContext.Error)
                };

                try
                {
                    collection = JsonConvert.DeserializeObject<IEnumerable<T>>(rawString, settings);
                }
                catch (Exception exception)
                {
                    LogHelper.Error<T>("Error converting model collection: " + rawString, exception);
                }
            }

            return collection;
        }

        public static bool IsJson(string input)
        {
            bool isJson = false;

            if (!string.IsNullOrEmpty(input))
            {
                input = input.Trim();
                isJson = (input.StartsWith("{") && input.EndsWith("}")) || (input.StartsWith("[") && input.EndsWith("]"));
            }

            return isJson;
        }

        public static bool IsXml(string input)
        {
            bool isXml = false;

            if (!string.IsNullOrEmpty(input))
            {
                input = input.Trim();
                isXml = input.StartsWith("<") && input.EndsWith(">");
            }

            return isXml;
        }

        public static bool IsJsonArray(string input)
        {
            bool isJson = false;

            if (!string.IsNullOrEmpty(input))
            {
                input = input.Trim();
                isJson = (input.StartsWith("[") && input.EndsWith("]"));
            }

            return isJson;
        }

    }
}
