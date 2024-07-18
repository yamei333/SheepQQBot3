using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommonLibrary;

public static class JsonExtensions
{
    public static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        IncludeFields = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static JsonSerializerOptions GetJsonOptions(bool ignoreNull)
        => ignoreNull
            ? new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            }
            : new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

    /// <summary>
    /// JsonDeserialize, 带try写入日志
    /// </summary>
    public static T Deserialize<T>(string jsonText, JsonSerializerOptions jsonSerializerOptions)
    {
        T result;
        try
        {
            result = JsonSerializer.Deserialize<T>(jsonText, jsonSerializerOptions);
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteJsonDeserializeLog(e, typeof(T).Name, jsonText);
            throw;
        }

        return result;
    }

    /// <summary>
    /// JsonDeserialize
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="jsonText">JsonText</param>
    /// <returns>结果</returns>
    public static T Deserialize<T>(string jsonText) => Deserialize<T>(jsonText, DefaultJsonOptions);

    /// <summary>
    /// JsonDeserialize
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="jsonText">JsonText</param>
    /// <param name="ignoreNull">是否忽略null值</param>
    /// <returns>结果</returns>
    public static T Deserialize<T>(string jsonText, bool ignoreNull) => Deserialize<T>(jsonText, GetJsonOptions(ignoreNull));
}