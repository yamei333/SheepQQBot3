using NAudio.Wave;
using Yamei.Common;

namespace YameiLibrary
{
    public static class CommonExtensions
    {
        /// <summary>
        /// 播放声音
        /// </summary>
        /// <param name="filePath">音频文件路径</param>
        /// <param name="delay">延迟</param>
        public static void PlaySe3(string filePath, int delay = 800)
            => PlaySe(filePath, 3, delay);

        /// <summary>
        /// 播放声音
        /// </summary>
        /// <param name="filePath">音频文件路径</param>
        /// <param name="playTimes">播放次数</param>
        /// <param name="delay">延迟</param>
        public static void PlaySe(string filePath, int playTimes = 1, int delay = 500)
        {
            if (playTimes <= 0)
                throw new ArgumentOutOfRangeException(nameof(playTimes));

            //if (!File.Exists(filePath))
            //    filePath = System.Environment.CurrentDirectory

            using var waveOut = new WaveOutEvent();
            using var wavReader = new WaveFileReader(filePath);

            waveOut.Init(wavReader);
            playTimes.Times(i =>
            {
                waveOut.Play();
                if (i < playTimes - 1)
                    SpinWait.SpinUntil(() => false, delay);
            });
        }
    }
}