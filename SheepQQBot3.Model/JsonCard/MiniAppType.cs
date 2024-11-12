using System.ComponentModel.DataAnnotations;

namespace SheepQQBot3.Model.JsonCard
{
    public enum MiniAppType
    {
        [Display(Name = "bili")]
        Bilibili,

        [Display(Name = "weibo")]
        WeiBo,
    }
}