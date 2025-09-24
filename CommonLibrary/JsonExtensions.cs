using Masuit.Tools;
using System.IO;
using System.Text;
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
    /// JsonDeserialize
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="obj"></param>
    /// <param name="filePath">文件路径</param>
    /// <param name="options"><see cref="JsonSerializerOptions"/></param>
    /// <returns>结果</returns>
    public static bool ToJsonFile<T>(this T obj, string filePath, JsonSerializerOptions options = null)
    {
        File.WriteAllText(filePath, obj.ToJsonString(options ?? DefaultJsonOptions), Encoding.UTF8);
        return true;
    }

    /// <summary>
    /// JsonDeserialize
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <returns>结果</returns>
    public static T FromJsonFile<T>(string filePath)
    {
        if (!File.Exists(filePath))
            return default;

        using var sr = File.OpenText(filePath);
        var jsonText = sr.ReadToEnd();
        return jsonText.FromJson<T>(GetJsonOptions());
    }

    public static T FromJson<T>(this string jsonText)
        => jsonText.FromJson<T>(GetJsonOptions());
}