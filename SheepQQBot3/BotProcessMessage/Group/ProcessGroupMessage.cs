using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SheepQQBot3.DbModel.JiebaDb;

namespace SheepQQBot3.BotProcessMessage.Group;

public static partial class ProcessGroupMessage
{
    /// <summary>
    /// Resources目录
    /// </summary>
    private const string PATH_RESOURCES = "Resources";

    /// <summary>
    /// idf文件名
    /// </summary>
    private const string FILE_IDF = "idf.txt";

    /// <summary>
    /// stopwords文件名
    /// </summary>
    private const string FILE_STOPWORDS = "stopwords.txt";

    static ProcessGroupMessage()
    {
        var jiebaDb = PublicVar.JiebaDb;
        var idfs = jiebaDb.Idfs;
        if (!idfs.Any())
        {
            var fs = File.OpenRead(Path.Combine(PATH_RESOURCES, FILE_IDF));
            var sr = new StreamReader(fs, Encoding.UTF8);
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var idfConfig = line!.Split(PublicVar.SPACE);
                idfs.Add(new Idf(idfConfig[0], decimal.Parse(idfConfig[1])));
            }

            jiebaDb.SaveChanges();
        }

        var stopWords = jiebaDb.StopWords;
        if (!stopWords.Any())
        {
            var fs = File.OpenRead(Path.Combine(PATH_RESOURCES, FILE_STOPWORDS));
            var sr = new StreamReader(fs, Encoding.UTF8);
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (stopWords.Find(line) == null)
                    stopWords.Add(new StopWord(line));
            }

            jiebaDb.SaveChanges();
        }
    }

    /// <summary>
    /// 取得字符串第1, 2个字符
    /// <para>失败时返回Z</para>
    /// </summary>
    /// <param name="input">字符串</param>
    /// <returns>
    /// 字符串第1个字符
    /// 失败时返回Z
    /// </returns>
    private static (char, char) GetStartChar(string input)
    {
        var startChar1 = 'Z';
        var startChar2 = 'Z';
        var isNull = string.IsNullOrEmpty(input);
        if (!isNull)
        {
            startChar1 = input[0];
            if (input.Length >= 2)
                startChar2 = input[1];
        }

        return (char.ToUpper(startChar1, CultureInfo.CurrentCulture),
            char.ToUpper(startChar2, CultureInfo.CurrentCulture));
    }
}