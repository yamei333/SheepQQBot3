using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SheepQQBot3.Model.AI;

/// <summary>
/// 支持动态指定模态的请求拦截中间件
/// </summary>
public class InjectModalitiesPolicy : PipelinePolicy
{
    public OutputModality Modality { get; set; }

    public InjectModalitiesPolicy(OutputModality modality = OutputModality.TextOnly)
    {
        Modality = modality;
    }

    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        InjectModalities(message);
        ProcessNext(message, pipeline, currentIndex);
    }

    public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        await InjectModalitiesAsync(message);
        await ProcessNextAsync(message, pipeline, currentIndex);
    }

    private void InjectModalities(PipelineMessage message)
    {
        if (message.Request.Content == null) return;

        using var memoryStream = new MemoryStream();
        message.Request.Content.WriteTo(memoryStream);
        memoryStream.Position = 0;

        var jsonNode = JsonNode.Parse(memoryStream);
        if (jsonNode is JsonObject rootObject)
        {
            ApplyModality(rootObject);
            var updatedBytes = JsonSerializer.SerializeToUtf8Bytes(rootObject);
            message.Request.Content = BinaryContent.Create(BinaryData.FromBytes(updatedBytes));
        }
    }

    private async Task InjectModalitiesAsync(PipelineMessage message)
    {
        if (message.Request.Content == null) return;

        using var memoryStream = new MemoryStream();
        await message.Request.Content.WriteToAsync(memoryStream);
        memoryStream.Position = 0;

        var jsonNode = JsonNode.Parse(memoryStream);
        if (jsonNode is JsonObject rootObject)
        {
            ApplyModality(rootObject);
            var updatedBytes = JsonSerializer.SerializeToUtf8Bytes(rootObject);
            message.Request.Content = BinaryContent.Create(BinaryData.FromBytes(updatedBytes));
        }
    }

    /// <summary>
    /// MEMO : 根据配置组装对应的 JSON 字段
    /// </summary>
    private void ApplyModality(JsonObject rootObject)
    {
        switch (Modality)
        {
            case OutputModality.ImageOnly:
                // MEMO : 仅生图，不让模型返回文本说明
                rootObject["modalities"] = new JsonArray {"image"};
                break;

            case OutputModality.TextOnly:
                // MEMO : 仅返回文本对话
                rootObject["modalities"] = new JsonArray {"text"};
                break;

            case OutputModality.ImageAndText:
                // MEMO : 图文混排输出
                rootObject["modalities"] = new JsonArray {"image", "text"};
                break;
        }
    }
}

/// <summary>
/// 输出模态控制枚举
/// </summary>
public enum OutputModality
{
    /// <summary>
    /// 仅输出图片（推荐用于自动化生图落盘）
    /// </summary>
    ImageOnly,

    /// <summary>
    /// 仅输出文本（用于普通的群聊总结或日常对话）
    /// </summary>
    TextOnly,

    /// <summary>
    /// 图文同时输出（输出图片的同时附带设计说明）
    /// </summary>
    ImageAndText,
}