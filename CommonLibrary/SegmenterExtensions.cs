using System.Collections.Generic;
using JiebaNet.Analyser;
using JiebaNet.Segmenter;

namespace CommonLibrary
{
    public static class SegmenterExtensions
    {
        private static readonly JiebaSegmenter _jiebaSegmenter = new();

        private static readonly TfidfExtractor _tfidfExtractor = new();

        private static readonly TextRankExtractor _textRankExtractor = new();

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