namespace SheepQQBot3.Enums;

/// <summary>
/// 取得路径时的路径类型
/// </summary>
public enum GetPathType
{
    /// <summary>
    /// 直接路径 (D:\)
    /// </summary>
    Normal,

    /// <summary>
    /// CQCode用path (file:///)
    /// </summary>
    CQCodePath,
}