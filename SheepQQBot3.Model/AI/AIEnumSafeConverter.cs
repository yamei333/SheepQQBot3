using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SheepQQBot3.Model.AI;

public class AIEnumSafeConverter<T> : JsonConverter<T?> where T : struct, Enum
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;

        string? value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value)) return null;

        // 尝试解析，如果失败，不抛异常，直接返回 null
        if (Enum.TryParse<T>(value, true, out T result))
        {
            return result;
        }

        // 可以在这里记录日志：AI 返回了预料之外的值
        Console.WriteLine($"[Warning] AI returned unknown enum value: {value}");
        return null;
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is not null)
            writer.WriteStringValue(((T)value).ToString());
        else
            writer.WriteNullValue();
    }
}
