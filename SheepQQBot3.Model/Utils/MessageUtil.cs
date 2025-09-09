using CommonLibrary;
using Masuit.Tools;
using SheepQQBot3.Model.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SheepQQBot3.Model;

/// <summary>
/// Message相关方法
/// </summary>
public static class MessageUtil
{
    private static readonly Regex _regGetCQArea = RegexGenerator.GetCQArea();
    private static readonly Regex _regGetCQCode = RegexGenerator.GetCQCode();
    private static readonly Regex _regRemoveSubType = RegexGenerator.CQCodeRemoveFileSize();

    /// <summary>
    /// 将消息处理为CQ码消息
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static List<Element> ProcessCQMessage(string message)
    {
        var processIndex = 0;
        var messageResult = new List<Element>();
        while (processIndex < message.Length - 1)
        {
            var cqAreaResult = _regGetCQArea.Match(message, processIndex);
            if (cqAreaResult.Success)
            {
                if (cqAreaResult.Index != processIndex)
                {
                    // CQ区域前包含其他文本, 先处理文本
                    messageResult.Add(ProcessCQAreaMessage(message[processIndex..cqAreaResult.Index]));
                    processIndex = cqAreaResult.Index;
                }

                messageResult.Add(ProcessCQAreaMessage(cqAreaResult.Value));
                processIndex += cqAreaResult.Length;
            }
            else
            {
                messageResult.Add(ProcessCQAreaMessage(message[processIndex..]));
                processIndex = message.Length;
            }
        }

        return messageResult;
    }

    public static Element ProcessCQAreaMessage(string message)
    {
        var cqCode = _regGetCQCode.Match(message).Value;
        if (cqCode.IsNullOrEmpty())
            return new Element(ElementType.text, new ElementBaseData(message));

        var cqType = (ElementType)Enum.Parse(typeof(ElementType), cqCode, true);
        switch (cqType)
        {
            //case "ym_play":
            //    YameiExtensions.PlaySe(GetElementBaseData().File);
            //    return new Element(ElementType.text, new ElementBaseData(string.Empty));
            //case "ym_play3":
            //    YameiExtensions.PlaySe3(GetElementBaseData().File);
            //    return new Element(ElementType.text, new ElementBaseData(string.Empty));
            case ElementType.json:
                return new Element(cqType, GetElementBaseData_Json());
            case ElementType.xml:
                return new Element(cqType, GetElementBaseData_Xml());
            //case "image":
            //    return new Element(cqType, GetElementBaseData_Image());
            //case "File":
            //    return new Element(cqType, GetElementBaseData_File());
            default:
                return new Element(cqType, GetElementBaseData());
                //case "at":
                //case "face":
                //case "image":
        }

        ElementBaseData GetElementBaseData()
        {
            var subJson = GetSubJson();
            var jsonData = subJson.JsonDeserialize<ElementBaseData>();
            return jsonData;
        }

        ElementBaseData GetElementBaseData_Json()
        {
            var subIndex = cqCode.Length + 5;
            var subMessage = message.Substring(subIndex, message.Length - cqCode.Length - 6);
            return new ElementBaseData
            {
                Data = subMessage[5..],
            };
        }

        //ElementBaseData GetElementBaseData_Image()
        //{
        //    message = _regRemoveUrl.Replace(message, string.Empty);
        //    message = _regRemoveSubType.Replace(message, string.Empty);
        //    cqCode = _regGetCQCode.Match(message);
        //    return GetElementBaseData();
        //}

        ElementBaseData GetElementBaseData_Xml()
        {
            var subIndex = cqCode.Length + 5;
            var subMessage = message.Substring(subIndex, message.Length - cqCode.Length - 6);
            var xmlString = new Regex(@"data=\<\?xml.+\>", RegexOptions.Singleline).Match(subMessage).Value;
            subMessage = subMessage.Replace(xmlString, string.Empty);
            var subJsonContent = string.Join(",", subMessage.Split(',')
                .Where(each => !each.IsNullOrEmpty()).ToArray()
                .Select(eachSubData => string.Join(':', eachSubData.Split('=')
                    .Select(eachElement => $"\"{eachElement}\"")
                    .ToArray())));
            var elementBaseData = $"{{{subJsonContent}}}".JsonDeserialize<ElementBaseData>();
            elementBaseData.Data = xmlString[5..];
            return elementBaseData;
        }

        string GetSubJson()
        {
            var subIndex = cqCode.Length + 5;
            if (subIndex >= message.Length)
                return "{}";

            var subMessage = message.Substring(subIndex, message.Length - cqCode.Length - 6);
            var subJsonContent = string.Join(",", subMessage.Split(',')
                .Select(eachSubData => string.Join(":", SplitJsonContent(eachSubData)
                    .Select(eachElement => $"\"{eachElement}\"")
                    .ToArray())));
            return $"{{{subJsonContent}}}";
        }

        IEnumerable<string> SplitJsonContent(string jsonContent)
        {
            var firstEqual = jsonContent.IndexOf('=');
            return new[] { jsonContent[..firstEqual], jsonContent[(firstEqual + 1)..] };
        }
    }
}