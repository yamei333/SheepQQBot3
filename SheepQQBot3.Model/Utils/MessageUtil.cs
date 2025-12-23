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
    private static readonly Regex _regCQCode = RegexGenerator.CQCode();

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
            var match = _regCQCode.Match(message, processIndex);
            if (match.Success)
            {
                var cqTag = match.Groups["tag"].Value;
                if (match.Index != processIndex)
                {
                    // CQ区域前包含其他文本, 先处理文本
                    messageResult.Add(ProcessCQAreaMessage(message[processIndex..match.Index]));
                    processIndex = match.Index;
                }

                messageResult.Add(ProcessCQAreaMessage(match.Value, cqTag));
                processIndex += match.Length;
            }
            else
            {
                messageResult.Add(ProcessCQAreaMessage(message[processIndex..]));
                processIndex = message.Length;
            }
        }

        return messageResult;
    }

    public static Element ProcessCQAreaMessage(string message, string cqTag = "")
    {
        if (cqTag.IsNullOrEmpty())
            return new Element(ElementType.text, new ElementBaseData(message));

        var cqTagType = (ElementType)Enum.Parse(typeof(ElementType), cqTag, true);
        switch (cqTagType)
        {
            //case "ym_play":
            //    YameiExtensions.PlaySe(GetElementBaseData().File);
            //    return new Element(ElementType.text, new ElementBaseData(string.Empty));
            //case "ym_play3":
            //    YameiExtensions.PlaySe3(GetElementBaseData().File);
            //    return new Element(ElementType.text, new ElementBaseData(string.Empty));
            case ElementType.json:
                return new Element(cqTagType, GetElementBaseData_Json());
            case ElementType.xml:
                return new Element(cqTagType, GetElementBaseData_Xml());
            //case "image":
            //    return new Element(cqType, GetElementBaseData_Image());
            //case "File":
            //    return new Element(cqType, GetElementBaseData_File());
            default:
                return new Element(cqTagType, GetElementBaseData());
                //case "at":
                //case "face":
                //case "image":
        }

        ElementBaseData GetElementBaseData()
        {
            var subJson = GetSubJson();
            var jsonData = subJson.FromJson<ElementBaseData>();
            return jsonData;
        }

        ElementBaseData GetElementBaseData_Json()
        {
            var subIndex = cqTag.Length + 5;
            var subMessage = message.Substring(subIndex, message.Length - cqTag.Length - 6);
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
            var subIndex = cqTag.Length + 5;
            var subMessage = message.Substring(subIndex, message.Length - cqTag.Length - 6);
            var xmlString = new Regex(@"data=\<\?xml.+\>", RegexOptions.Singleline).Match(subMessage).Value;
            subMessage = subMessage.Replace(xmlString, string.Empty);
            var subJsonContent = string.Join(",", subMessage.Split(',')
                .Where(each => !each.IsNullOrEmpty()).ToArray()
                .Select(eachSubData => string.Join(':', eachSubData.Split('=')
                    .Select(eachElement => $"\"{eachElement}\"")
                    .ToArray())));
            var elementBaseData = $"{{{subJsonContent}}}".FromJson<ElementBaseData>();
            elementBaseData.Data = xmlString[5..];
            return elementBaseData;
        }

        string GetSubJson()
        {
            var subIndex = cqTag.Length + 5;
            if (subIndex >= message.Length)
                return "{}";

            var subMessage = message.Substring(subIndex, message.Length - cqTag.Length - 6);
            var subJsonContent = string.Join(",", subMessage.Split(',')
                .Select(eachSubData => string.Join(':', SplitJsonContent(eachSubData)
                    .Select(eachElement => $"\"{eachElement}\"")
                    .ToArray())));
            return $"{{{subJsonContent}}}";
        }

        static IEnumerable<string> SplitJsonContent(string jsonContent)
        {
            var firstEqual = jsonContent.IndexOf('=');
            return [jsonContent[..firstEqual], jsonContent[(firstEqual + 1)..]];
        }
    }
}