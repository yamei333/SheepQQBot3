namespace SheepQQBot3.Model.AI;
using OpenRouter.NET.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

/// <summary>
/// 用于修补 OpenRouter.NET 生成的简陋 Schema，
/// 强行注入 C# 的 [Description] 和 [JsonPropertyName]，并自动展开 Enum 定义。
/// </summary>
public static class SchemaDeepPatcher
{
    /// <summary>
    /// 主入口：生成带 Description 和 Enum 详情的完整 Schema
    /// </summary>
    /// <param name="rootType">你的响应类类型，例如 typeof(AIChatResponse)</param>
    /// <returns>可以直接传给 Tools 的 Schema 对象</returns>
    public static object Generate(Type rootType)
    {
        // 1. 调用那个闭源库生成基础骨架 (假设 SchemaGenerator 是你反编译看到的那个类)
        //    如果那个库不在当前命名空间，记得 using 一下
        var rootSchema = SchemaGenerator.GenerateSchema(rootType);

        // 2. 开始深层递归手术
        //    SchemaGenerator 返回的本质上是 Dictionary<string, object>
        if (rootSchema is Dictionary<string, object> schemaDict)
        {
            InjectRecursively(rootType, schemaDict);
        }

        return rootSchema;
    }

    /// <summary>
    /// 递归注入逻辑
    /// </summary>
    private static void InjectRecursively(Type type, Dictionary<string, object> schema)
    {
        // 如果是 Nullable<T> (比如 int?)，取出的底层类型 T
        type = Nullable.GetUnderlyingType(type) ?? type;

        // 只有当 Schema 里有 "properties" 字段时，才说明这是一个 Object，需要遍历属性
        if (schema.TryGetValue("properties", out var propsObj) && propsObj is Dictionary<string, object> propsDict)
        {
            // 遍历 C# 类的所有公共属性
            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                // A. 确定 JSON Key 的名字
                //    优先读 [JsonPropertyName("xxx")]，没写就用属性名
                var jsonAttr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                string jsonKey = jsonAttr != null ? jsonAttr.Name : prop.Name;

                // B. 在 Schema 里找到对应的字段定义
                if (propsDict.TryGetValue(jsonKey, out var fieldSchemaObj) && fieldSchemaObj is Dictionary<string, object> fieldSchema)
                {
                    // --- 核心操作 1: 注入 Description ---
                    
                    // 1.1 获取属性本身写的 [Description]
                    var descAttr = prop.GetCustomAttribute<DescriptionAttribute>();
                    string finalDescription = descAttr?.Description ?? "";

                    // 1.2 获取属性的真实类型 (处理可空类型)
                    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    // --- 核心操作 2: 自动展开 Enum (表情包神器) ---
                    if (propType.IsEnum)
                    {
                        // 如果这个字段是 Enum，把它的所有选项和中文注释提取出来
                        string enumHelpText = GenerateEnumHelpText(propType);
                        
                        // 拼接到原本的 Description 后面
                        if (!string.IsNullOrEmpty(finalDescription))
                            finalDescription += "\n"; // 换行
                        
                        finalDescription += enumHelpText;
                    }

                    // 1.3 如果拼凑出了描述，写入 Schema
                    if (!string.IsNullOrEmpty(finalDescription))
                    {
                        fieldSchema["description"] = finalDescription;
                    }

                    // --- 核心操作 3: 递归钻取 (处理嵌套对象和数组) ---

                    // 情况 A: 这是一个集合/数组 (例如 Contents[])
                    if (IsCollection(propType, out var elementType))
                    {
                        // 集合在 Schema 里是 "items": { ... }，我们要钻进去修补 items 里的结构
                        if (fieldSchema.TryGetValue("items", out var itemsObj) && itemsObj is Dictionary<string, object> itemsSchema)
                        {
                            InjectRecursively(elementType, itemsSchema);
                        }
                    }
                    // 情况 B: 这是一个嵌套的类 (例如 ChatMessageInfo)
                    // 排除掉 string (它也是类但不需要递归) 和系统类型
                    else if (propType.IsClass && propType != typeof(string))
                    {
                        InjectRecursively(propType, fieldSchema);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 辅助方法：生成 Enum 的说明文档
    /// 输出格式示例：
    /// Available Options:
    /// - kaixin: 开心、幸福
    /// - shengqi: 生气
    /// </summary>
    private static string GenerateEnumHelpText(Type enumType)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("\nAllowed Values:\n");

        foreach (var field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            // 获取枚举值的 [Description]
            var descAttr = field.GetCustomAttribute<DescriptionAttribute>();
            
            // 格式: - 枚举名: 描述
            sb.Append($"- {field.Name}");
            if (descAttr != null && !string.IsNullOrEmpty(descAttr.Description))
            {
                sb.Append($": {descAttr.Description}");
            }
            sb.Append("\n");
        }
        return sb.ToString();
    }

    /// <summary>
    /// 辅助方法：判断是否为集合，并获取元素类型
    /// </summary>
    private static bool IsCollection(Type type, out Type elementType)
    {
        elementType = null;
        if (type == typeof(string)) return false; // 字符串不算集合

        // 1. 数组
        if (type.IsArray)
        {
            elementType = type.GetElementType();
            return true;
        }

        // 2. List<T> 或 IEnumerable<T>
        if (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType)
        {
            elementType = type.GetGenericArguments()[0];
            return true;
        }

        return false;
    }
}