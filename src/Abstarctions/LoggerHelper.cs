using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lib.Logging.Abstractions
{
    public static class LogHelper
    {
        private const string regexContextPlaceholder = @"\{\D[^}]*\}";
        private const string emptyPlaceholderValue = "<?>";


        public static string Format(IDictionary<string, object> scope, LogLevel level, string message = null)
        {
            StringBuilder messageBuilder = new StringBuilder();
            if (scope.Count >= 0)
            {
                messageBuilder.AppendFormat("{0}********{1} Trace******** {2}", Environment.NewLine, level.ToString().ToUpper(), Environment.NewLine);
                foreach (KeyValuePair<string, object> entry in scope)
                {
                    messageBuilder.AppendFormat("{0} : {1} {2}", entry.Key, entry.Value, Environment.NewLine);
                }

                if (!string.IsNullOrWhiteSpace(message))
                {
                    message = Interpolate(message, scope);
                    messageBuilder.AppendLine(message);
                }
                messageBuilder.AppendLine("***************************");
            }
            else
            {
                // NOTE: No Context, no formatting is being made
                message = RemovePlaceholders(message);
                return message;
            }

            return messageBuilder.ToString();
        }

        private static string Interpolate(string message, IDictionary<string, object> items)
        {
            StringBuilder messageBuilder = null;
            string placeholder = string.Empty;

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            messageBuilder = new StringBuilder(message);

            foreach (KeyValuePair<string, object> item in items)
            {
                placeholder = $"{{{item.Key}}}";
                messageBuilder.Replace(placeholder, item.Value.ToString());
            }

            return RemovePlaceholders(messageBuilder.ToString());
        }

        private static string RemovePlaceholders(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                message = Regex.Replace(message, regexContextPlaceholder, emptyPlaceholderValue);
            }

            return message;
        }

        /// <summary>
        /// convert the dictionary to string and will format the output like [{key:Value} {Key:Value}]
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static string Format<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                return "";
            return dictionary.ToJson();
        }

        //public static string Format<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> enumerable, TKey[] excludeKeys)
        //{
        //    if (enumerable == null) return string.Empty;
        //    var newList = enumerable?.Where(w => !excludeKeys.Contains(w.Key));
        //    return Format(newList?.ToDictionary(x => x.Key, x => x.Value.ToJson()));
        //}


        public static Dictionary<TKey, object> Format<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> enumerable, TKey[] excludeKeys)
        {
            if (enumerable == null) return null;
            var newList = enumerable?.Where(w => !excludeKeys.Contains(w.Key));
            return newList?.ToDictionary(x => x.Key, x => x.Value.AsDictionary());
        }


        public static string ToJson<T>(this T source , Formatting formatting = Formatting.None)
        {
            try
            {
                var json = JsonConvert.SerializeObject(source, formatting);
                return json;
            }
            catch (Exception e)
            {

                return e.Message;
            }

        }

        public static object AsDictionary<T>(this T source)
        {

            try
            {
                var json = source.ToJson();

                return JsonConvert.DeserializeObject<Dictionary<string, object>>(json, new JsonConverter[] { new DictionaryConvertor() });

            }
            catch
            {

                return source;
            }
            
        }

        public static InternalException ConvertToInternalException(this Exception exception, string requestId = "")
        {
            if (exception == null)
            {
                return null;
            }

            if (exception is InternalException internalException)
            {
                return internalException;
            }

            if (string.IsNullOrWhiteSpace(requestId))
            {
                return new InternalException(exception);
            }

            return new InternalException(exception, requestId);

        }
    }
}


