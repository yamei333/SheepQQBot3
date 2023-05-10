using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonLibrary;

/// <summary>
/// 带权重的随机数
/// </summary>
/// <typeparam name="T"></typeparam>
public class RandomWeight<T>
    where T : notnull
{
    /// <summary>
    /// 无限取得次数
    /// </summary>
    public const int MAXCOUNT = -1;

    /// <summary>
    /// 权重
    /// </summary>
    public int Weight { get; }

    /// <summary>
    /// 数据
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// 最大获取次数
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public RandomWeight(int weight, T value, int count = MAXCOUNT)
    {
        Weight = weight;
        Value = value;
        Count = count;
    }
}

/// <summary>
/// <see cref="RandomWeight{T}"/>类的拓展
/// </summary>
public static class RandomWeightUtil
{
    /// <summary>
    /// 取得带权重的随机数
    /// </summary>
    public static bool TryGetRandomWeight<T>(
        this List<RandomWeight<T>> randomWeights,
        out RandomWeight<T> result)
        where T : notnull
    {
        if (randomWeights?.Any() != true)
        {
            result = null;
            return false;
        }

        //累加结算总权重
        var totalWeight = randomWeights.Sum(each => each.Weight);

        //在0~total范围内随机
        var cursor = 0;
        var random = new Random().Next(0, totalWeight);
        foreach (var item in randomWeights)
        {
            //累加当前权重
            cursor += item.Weight;
            //判断随机数
            if (cursor <= random)
                continue;

            // MEMO : 减少次数
            if (item.Count > 0)
                item.Count--;
            // MEMO : 0次时移除
            if (item.Count == 0)
                randomWeights.Remove(item);

            result = item;
            return true;
        }

        result = null;
        return false;
    }
}