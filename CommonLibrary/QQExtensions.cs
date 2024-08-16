using System;
using System.Linq;
using System.Windows.Media.Imaging;

namespace CommonLibrary
{
    public static class QQExtensions
    {
        public static BitmapFrame GetQQImage(long targetId)
            => BitmapFrame.Create(new Uri(GetQQImageUrl(targetId)));

        public static BitmapFrame GetQQGroupImage(long targetId)
            => BitmapFrame.Create(new Uri(GetQQGroupImageUrl(targetId)));

        public static string GetQQImageUrl(long targetId)
            => $"https://q.qlogo.cn/headimg_dl?dst_uin={targetId}&spec=40";

        public static string GetQQGroupImageUrl(long targetId)
            => $"https://p.qlogo.cn/gh/{targetId}/{targetId}/40/";

        /// <summary>
        /// QQ Gtk计算
        /// </summary>
        /// <param name="psKey">QQ pskey</param>
        /// <returns>Gtk</returns>
        public static int GetGtk(string psKey)
        {
            var hash = psKey.Aggregate(5381, (current, t) => current + (current << 5) + t);
            return hash & 0x7fffffff;
        }
    }
}