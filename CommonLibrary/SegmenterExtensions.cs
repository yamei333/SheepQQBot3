using JiebaNet.Analyser;
using JiebaNet.Segmenter;
using System.Collections.Generic;

namespace CommonLibrary
{
    public static class SegmenterExtensions
    {
        private static readonly JiebaSegmenter _jiebaSegmenter;

        private static readonly TfidfExtractor _tfidfExtractor;

        private static readonly TextRankExtractor _textRankExtractor;

        static SegmenterExtensions()
        {
            _jiebaSegmenter = new JiebaSegmenter();
            _tfidfExtractor = new TfidfExtractor(_jiebaSegmenter);
            _textRankExtractor = new TextRankExtractor();
        }

        /// <summary>
        /// 添加新词
        /// </summary>
        public static void AddWord(string word, int freq = 0, string tag = null)
            => _jiebaSegmenter.AddWord(word, freq, tag);

        public static IEnumerable<string> SegmenterCut(this string cutText)
        {
            return _jiebaSegmenter.Cut(cutText);
        }

        public static IEnumerable<WordWeightPair> ExtractTagsWithWeight_Idf(this string cutText)
        {
            return _tfidfExtractor.ExtractTagsWithWeight(cutText);
        }

        public static IEnumerable<WordWeightPair> ExtractTagsWithWeight_TextTank(this string cutText)
        {
            return _textRankExtractor.ExtractTagsWithWeight(cutText);
        }
    }
}