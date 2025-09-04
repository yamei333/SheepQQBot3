using Masuit.Tools;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommonLibrary;

public static class JsonExtensions
{
    public static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        IncludeFields = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        AllowTrailingCommas = true,
    };

    public static JsonSerializerOptions GetJsonOptions(bool ignoreNull = true, bool writeIndented = false)
        => new()
        {
            DefaultIgnoreCondition = ignoreNull ? JsonIgnoreCondition.WhenWritingNull : JsonIgnoreCondition.Never,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = writeIndented,
            AllowTrailingCommas = true,
        };

    /// <summary>
    /// JsonDeserialize, 带try写入日志
    /// </summary>
    public static string JsonSerialize<T>(this object obj, JsonSerializerOptions jsonSerializerOptions = null)
    {
        string result;
        try
        {
            result = JsonSerializer.Serialize(obj, jsonSerializerOptions ?? DefaultJsonOptions);
        }
        catch (Exception e)
        {
            YameiLogExtensions.WriteJsonSerializeLog(e, typeof(T).Name, obj);
            throw;
        }

        return result;
    }

    /// <summary>
    /// JsonDeserialize, 带try写入日志
    /// </summary>
    public static T JsonDeserialize<T>(this string jsonText, JsonSerializerOptions jsonSerializerOptions)
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
    public static T JsonDeserialize<T>(this string jsonText) => JsonDeserialize<T>(jsonText, DefaultJsonOptions);

    /// <summary>
    /// JsonDeserialize
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="jsonText">JsonText</param>
    /// <param name="ignoreNull">是否忽略null值</param>
    /// <returns>结果</returns>
    public static T JsonDeserialize<T>(this string jsonText, bool ignoreNull) => JsonDeserialize<T>(jsonText, GetJsonOptions(ignoreNull));

    /// <summary>
    /// JsonDeserialize
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <returns>结果</returns>
    public static bool JsonSerializeToFile<T>(this T obj, string filePath)
    {
        File.WriteAllText(filePath, obj.ToJsonIgnoreNull());
        return true;
    }

    /// <summary>
    /// JsonDeserialize
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <returns>结果</returns>
    public static T JsonDeserializeFromFile<T>(string filePath)
    {
        if (!File.Exists(filePath))
            return default;

        using var sr = File.OpenText(filePath);
        var jsonText = sr.ReadToEnd();
        return jsonText.FromJson<T>(GetJsonOptions());
    }
}