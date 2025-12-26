using System.Text.Encodings.Web;

namespace SheepQQBot3.Model.AI;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations; // 必须引用这个
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class JsonSchemaGenerator
{
    public static string Generate(Type type)
    {
        var schemaMap = GenerateSchemaMap(type, null);
        return JsonSerializer.Serialize(schemaMap, new JsonSerializerOptions
        {
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
    }

    private static Dictionary<string, object> GenerateSchemaMap(Type type, string parentDescription)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        var schema = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(parentDescription))
        {
            schema["description"] = parentDescription;
        }

        // A. 基础类型
        if (IsSimpleType(type))
        {
            schema["type"] = GetJsonTypeString(type);
            return schema;
        }

        // B. 枚举 (Enum)
        if (type.IsEnum)
        {
            schema["type"] = "string";
            string enumHelpText = GenerateEnumHelpText(type);

            if (schema.ContainsKey("description"))
                schema["description"] = schema["description"] + enumHelpText;
            else
                schema["description"] = enumHelpText.Trim();

            schema["enum"] = Enum.GetNames(type);
            return schema;
        }

        // C. 集合/数组
        if (IsCollection(type, out var elementType))
        {
            schema["type"] = "array";
            schema["items"] = GenerateSchemaMap(elementType, null);
            return schema;
        }

        // D. 复杂对象 (Class / Struct)
        // ---------------------------------------------------------
        // 这里是修改的核心区域
        // ---------------------------------------------------------
        if (type.IsClass || type.IsValueType)
        {
            schema["type"] = "object";
            var properties = new Dictionary<string, object>();
            var requiredList = new List<string>(); // 存储必填字段名

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // 1. 确定 JSON Key 名称
                var jsonAttr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                string propName = jsonAttr != null ? jsonAttr.Name : prop.Name;

                // 2. 获取描述并递归生成子 Schema
                var descAttr = prop.GetCustomAttribute<DescriptionAttribute>();
                string propDesc = descAttr?.Description;
                properties[propName] = GenerateSchemaMap(prop.PropertyType, propDesc);

                // 3. 【新增逻辑】检查 [Required] 特性
                // 只有当属性上有 [Required] 标记时，才加入必填列表
                if (prop.GetCustomAttribute<RequiredAttribute>() != null)
                    requiredList.Add(propName);
            }

            schema["properties"] = properties;

            // 4. 如果有必填项，写入 Schema
            if (requiredList.Count > 0)
                schema["required"] = requiredList;

            return schema;
        }

        return new Dictionary<string, object> { { "type", "string" } };
    }

    // --- 辅助方法 (保持不变) ---

    private static string GenerateEnumHelpText(Type enumType)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("\nAllowed Values:");
        foreach (var field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var descAttr = field.GetCustomAttribute<DescriptionAttribute>();
            sb.Append($"\n- {field.Name}");
            if (descAttr != null && !string.IsNullOrEmpty(descAttr.Description))
            {
                sb.Append($": {descAttr.Description}");
            }
        }
        return sb.ToString();
    }

    private static bool IsCollection(Type type, out Type elementType)
    {
        elementType = null;
        if (type == typeof(string)) return false;
        if (type.IsArray) { elementType = type.GetElementType(); return true; }
        if (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType) { elementType = type.GetGenericArguments()[0]; return true; }
        return false;
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(Guid);
    }

    private static string GetJsonTypeString(Type type)
    {
        if (type == typeof(string) || type == typeof(DateTime) || type == typeof(Guid)) return "string";
        if (type == typeof(bool)) return "boolean";
        if (type == typeof(int) || type == typeof(long) || type == typeof(short)) return "integer";
        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal)) return "number";
        return "string";
    }
}