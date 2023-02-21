using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Yamei.Common
{
    /// <summary>
    /// <see cref="IEnumerable{T}"/>的扩充方法
    /// </summary>
    public static class EnumerableExtensions
    {
        #region Inner Classes

        /// <summary>
        /// 変換した値から2つのオブジェクトを比較する<see cref="IComparer{T}"/>です。
        /// </summary>
        private class SelectionComparer<TSource, TKey> : IComparer<TSource>
        {
            /// <summary>
            /// 変換した値を比較する<see cref="Comparer{T}"/>です。
            /// </summary>
            private readonly IComparer<TKey> _defaultComparer = Comparer<TKey>.Default;

            /// <summary>
            /// 比較する値に変換する関数です。
            /// </summary>
            private readonly Func<TSource, TKey> _keySelector;

            /// <summary>
            /// インスタンスを初期化します。
            /// </summary>
            public SelectionComparer(Func<TSource, TKey> keySelector)
            {
                _keySelector = keySelector;
            }

            /// <inheritdoc />
            public int Compare(TSource x, TSource y)
            {
                var xKey = _keySelector(x);
                var yKey = _keySelector(y);

                return _defaultComparer.Compare(xKey, yKey);
            }
        }

        /// <summary>
        /// 変換した値から2つのオブジェクトが等しいかどうかを比較する<see cref="IEqualityComparer{T}"/>です。
        /// </summary>
        private class SelectionEqualityComparer<TSource, TKey> : IEqualityComparer<TSource>
        {
            /// <summary>
            /// 比較する値に変換する関数です。
            /// </summary>
            private readonly Func<TSource, TKey> _keySelector;

            /// <summary>
            /// インスタンスを初期化します。
            /// </summary>
            public SelectionEqualityComparer(Func<TSource, TKey> keySelector)
            {
                _keySelector = keySelector;
            }

            /// <inheritdoc />
            public bool Equals(TSource x, TSource y)
            {
                var xKey = _keySelector(x);
                var yKey = _keySelector(y);

                if (xKey == null)
                {
                    return yKey == null;
                }

                return xKey.Equals(yKey);
            }

            /// <inheritdoc />
            public int GetHashCode(TSource obj)
            {
                var key = _keySelector(obj);
                return key != null ? key.GetHashCode() : 0;
            }
        }

        #endregion Inner Classes

        /// <summary>
        /// 在<paramref name="sequence"/>后连接<paramref name="item"/>
        /// </summary>
        /// <typeparam name="TSource">对象类型</typeparam>
        /// <param name="sequence">连接对象</param>
        /// <param name="item"><paramref name="sequence"/>连接要素</param>
        /// <returns>被连接后的结果</returns>
        public static IEnumerable<TSource> ConcatItem<TSource>(this IEnumerable<TSource> sequence, TSource item)
        {
            foreach (var eachItem in sequence)
                yield return eachItem;

            yield return item;
        }

        /// <summary>
        /// 指定された<paramref name="keySelector"/>を使用して取得した値を比較することにより、2つのシーケンスの積集合を生成します。
        /// </summary>
        /// <typeparam name="TSource">各要素の型</typeparam>
        /// <typeparam name="TKey">比較する値の型</typeparam>
        /// <param name="first">second にも含まれる、返される一意の要素を含む<see cref="IEnumerable{T}"/></param>
        /// <param name="second">最初のシーケンスにも含まれる、返される一意の要素を含む<see cref="IEnumerable{T}"/></param>
        /// <param name="keySelector">比較する値に変換する関数</param>
        /// <returns>2つのシーケンスの積集合を構成する要素が格納されている<see cref="IEnumerable{T}"/></returns>
        public static IEnumerable<TSource> Intersect<TSource, TKey>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector)
        {
            return first.Intersect(second, new SelectionEqualityComparer<TSource, TKey>(keySelector));
        }

        /// <summary>
        /// シーケンスの要素数を比較し、相対値を示す値を返します。
        /// </summary>
        /// <typeparam name="TSource">各要素の型</typeparam>
        /// <param name="source">比較基のシーケンス</param>
        /// <param name="target">比較対象のシーケンス</param>
        /// <returns>
        /// 要素数の相対値を示す値
        /// <list type="table">
        ///		<listheader><term>戻り値</term><description>説明</description></listheader>
        ///		<item><term>0より小さい値</term><description>このシーケンスの要素数は<paramref name="target"/>より小さいことを示します。</description></item>
        ///		<item><term>0</term><description>このシーケンスの要素数は<paramref name="target"/>と等しいことを示します。</description></item>
        ///		<item><term>0より大きい値</term><description>このシーケンスの要素数は<paramref name="target"/>より大きいことを示します。</description></item>
        /// </list>
        /// </returns>
        [DebuggerStepThrough]
        public static int CompareCount<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> target)
        {
            if (target == null)
                return 1;

            if (Equals(source, target))
                return 0;

            using var sourceEnumerator = source?.GetEnumerator();
            using var targetEnumerator = target.GetEnumerator();
            while (true)
            {
                var existSource = sourceEnumerator?.MoveNext();
                var existTarget = targetEnumerator.MoveNext();

                if (existSource == false)
                    return existTarget ? -1 : 0;

                if (!existTarget)
                    return 1;
            }
        }

        /// <summary>
        /// シーケンスの要素数を比較し、相対値を示す値を返します。
        /// </summary>
        /// <typeparam name="TSource">各要素の型</typeparam>
        /// <param name="source">比較元のシーケンス</param>
        /// <param name="count">比較する数</param>
        /// <returns>
        /// 要素数の相対値を示す値
        /// <list type="table">
        ///		<listheader><term>戻り値</term><description>説明</description></listheader>
        ///		<item><term>0より小さい値</term><description>このシーケンスの要素数は<paramref name="count"/>より小さいことを示します。</description></item>
        ///		<item><term>0</term><description>このシーケンスの要素数は<paramref name="count"/>と等しいことを示します。</description></item>
        ///		<item><term>0より大きい値</term><description>このシーケンスの要素数は<paramref count="target"/>より大きいことを示します。</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>が<see langword="null"/>です。</exception>
        [DebuggerStepThrough]
        public static int CompareCount<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (count < 0)
            {
                return 1;
            }

            var index = 0;
            using (var sourceEnumerator = source.GetEnumerator())
            {
                while (sourceEnumerator.MoveNext())
                {
                    index++;
                    if (count < index)
                    {
                        return 1;
                    }
                }
            }

            return index == count ? 0 : -1;
        }

        /// <summary>
        /// 指定された<paramref name="keySelector"/>を使用して取得した値を比較することにより、シーケンスから一意の要素を返します。
        /// </summary>
        /// <typeparam name="TSource">各要素の型</typeparam>
        /// <typeparam name="TKey">比較する値の型</typeparam>
        /// <param name="source">重複する要素を削除する対象となるシーケンス</param>
        /// <param name="keySelector">比較する値に変換する関数</param>
        /// <returns>シーケンスの一意の要素を格納する<see cref="IEnumerable{T}"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>または<paramref name="keySelector"/>が<see langword="null"/>です。</exception>
        [DebuggerStepThrough]
        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.Distinct(new SelectionEqualityComparer<TSource, TKey>(keySelector));
        }

        /// <summary>
        /// 指定された<paramref name="keySelector"/>を使用して取得した値を比較することにより、2つのシーケンスの差集合を生成します。
        /// </summary>
        /// <typeparam name="TSource">各要素の型</typeparam>
        /// <typeparam name="TKey">比較する値の型</typeparam>
        /// <param name="first"><paramref name="second"/>には含まれて否外、返される要素を含むシーケンス</param>
        /// <param name="second">最初のシーケンスにも含まれ、返されたシーケンスからは削除される要素を含むシーケンス</param>
        /// <param name="keySelector">比較する値に変換する関数</param>
        /// <returns>2つのシーケンスの差集合が格納されているシーケンス<see cref="IEnumerable{T}"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="first"/>または<paramref name="keySelector"/>が<see langword="null"/>です。</exception>
        [DebuggerStepThrough]
        public static IEnumerable<TSource> Except<TSource, TKey>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector)
        {
            //Guard.ArgumentNotNull(first, nameof(first));
            //Guard.ArgumentNotNull(second, nameof(second));
            //Guard.ArgumentNotNull(keySelector, nameof(keySelector));

            return first.Except(second, new SelectionEqualityComparer<TSource, TKey>(keySelector));
        }

        /// <summary>
        /// 指定された<paramref name="keySelector"/>を使用して取得した値を比較することにより、2つの<see cref="IEnumerable{TSource}"/>の和集合を生成します。
        /// </summary>
        /// <typeparam name="TSource">各要素の型</typeparam>
        /// <typeparam name="TKey">比較する値の型</typeparam>
        /// <param name="first">和集合を構成する最初の<see cref="IEnumerable{TSource}"/></param>
        /// <param name="second">和集合を構成する2番目の<see cref="IEnumerable{TSource}"/></param>
        /// <param name="keySelector">比較する値を取得する<see cref="Func{TSource, TKey}"/></param>
        /// <returns>2つの<see cref="IEnumerable{TSource}"/>の和集合を格納する<see cref="IEnumerable{TSource}"/></returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="first"/>または<paramref name="second"/>または<paramref name="keySelector"/>が<see langword="null"/>です。
        /// </exception>
        public static IEnumerable<TSource> Union<TSource, TKey>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector)
        {
            //Guard.ArgumentNotNull(first, nameof(first));
            //Guard.ArgumentNotNull(second, nameof(second));
            //Guard.ArgumentNotNull(keySelector, nameof(keySelector));

            return first.Union(second, new SelectionEqualityComparer<TSource, TKey>(keySelector));
        }

        /// <summary>
        /// シーケンス内の要素をキーに従って並べ替えます。
        /// </summary>
        /// <typeparam name="TSource">各要素の型</typeparam>
        /// <typeparam name="TKey">比較する値の型</typeparam>
        /// <param name="source">順序付ける値のシーケンス</param>
        /// <param name="keySelector">要素からキーを抽出する関数</param>
        /// <param name="ascending">昇順の場合は<see langword="true"/>、降順の場合は<see langword="false"/></param>
        /// <returns>要素がキーに従って並べ替えられている<see cref="IOrderedEnumerable{TElement}"/></returns>
        [DebuggerStepThrough]
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, bool ascending)
        {
            return ascending
                ? source.OrderBy(keySelector)
                : source.OrderByDescending(keySelector);
        }

        /// <summary>
        /// シーケンス内の要素をキーに従って並べ替えます。
        /// </summary>
        /// <typeparam name="TSource">各要素の型</typeparam>
        /// <typeparam name="TKey">比較する値の型</typeparam>
        /// <param name="source">順序付ける値のシーケンス</param>
        /// <param name="keySelector">要素からキーを抽出する関数</param>
        /// <param name="comparer">キーを比較する<see cref="IComparer{TKey}"/></param>
        /// <param name="ascending">昇順の場合は<see langword="true"/>、降順の場合は<see langword="false"/></param>
        /// <returns>要素がキーに従って並べ替えられている<see cref="IOrderedEnumerable{TElement}"/></returns>
        [DebuggerStepThrough]
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer, bool ascending)
        {
            return ascending
                ? source.OrderBy(keySelector, comparer)
                : source.OrderByDescending(keySelector, comparer);
        }

        /// <summary>
        /// 重複している要素を返します。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="source">処理を適用する値のシーケンス</param>
        /// <returns>重複している要素のシーケンス</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>が<see langword="null"/>です。</exception>
        [DebuggerStepThrough]
        public static IEnumerable<T> Overlapped<T>(this IEnumerable<T> source)
            where T : notnull
        {
            //Guard.ArgumentNotNull(source, nameof(source));
            return OverlappedInternal(source, null);
        }

        /// <summary>
        /// 重複している要素を返します。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="source">処理を適用する値のシーケンス</param>
        /// <param name="comparer">値を比較する<see cref="IEqualityComparer{T}"/></param>
        /// <returns>重複している要素のシーケンス</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>が<see langword="null"/>です。</exception>
        [DebuggerStepThrough]
        public static IEnumerable<T> Overlapped<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
            where T : notnull
        {
            //Guard.ArgumentNotNull(source, nameof(source));
            return OverlappedInternal(source, comparer);
        }

        /// <summary>
        /// シーケンス内の後続の要素をキーに従って並べ替えます。
        /// </summary>
        /// <typeparam name="TSource">各要素の型</typeparam>
        /// <typeparam name="TKey">比較する値の型</typeparam>
        /// <param name="source">順序付ける値のシーケンス</param>
        /// <param name="keySelector">要素からキーを抽出する関数</param>
        /// <param name="ascending">昇順の場合は<see langword="true"/>、降順の場合は<see langword="false"/></param>
        /// <returns>要素がキーに従って並べ替えられている<see cref="IOrderedEnumerable{TElement}"/></returns>
        [DebuggerStepThrough]
        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(
            this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, bool ascending)
        {
            return ascending
                ? source.ThenBy(keySelector)
                : source.ThenByDescending(keySelector);
        }

        /// <summary>
        /// シーケンス内の後続の要素をキーに従って並べ替えます。
        /// </summary>
        /// <typeparam name="TSource">各要素の型</typeparam>
        /// <typeparam name="TKey">比較する値の型</typeparam>
        /// <param name="source">順序付ける値のシーケンス</param>
        /// <param name="keySelector">要素からキーを抽出する関数</param>
        /// <param name="comparer">キーを比較する<see cref="IComparer{TKey}"/></param>
        /// <param name="ascending">昇順の場合は<see langword="true"/>、降順の場合は<see langword="false"/></param>
        /// <returns>要素がキーに従って並べ替えられている<see cref="IOrderedEnumerable{TElement}"/></returns>
        [DebuggerStepThrough]
        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(
            this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer, bool ascending)
        {
            return ascending
                ? source.ThenBy(keySelector, comparer)
                : source.ThenByDescending(keySelector, comparer);
        }

        /// <summary>
        /// イテレートメソッドです。各要素に対して<paramref name="toAction"/>を実行します。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="enumerable">対象の<see cref="IEnumerable{T}"/></param>
        /// <param name="toAction">要素に対して実行するアクション</param>
        [DebuggerStepThrough]
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> toAction)
        {
            foreach (var each in enumerable)
                toAction(each);
        }

        /// <summary>
        /// 数组的ForEach方法, 和linq的ForEach统一化。
        /// </summary>
        [DebuggerStepThrough]
        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            foreach (var each in array)
                action(each);
        }

        /// <summary>
        /// ForEach方法<see cref="MatchCollection"/>的扩展。
        /// </summary>
        [DebuggerStepThrough]
        public static void ForEach(this MatchCollection enumerable, Action<Match> toAction)
        {
            foreach (var each in enumerable)
                toAction((Match)each);
        }

        /// <summary>
        /// インデックス付イテレートメソッドです。各要素に対して<paramref name="toAction"/>を実行します。
        /// インデックスを利用したい場合に利用してください。
        /// </summary>
        [DebuggerStepThrough]
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> toAction)
        {
            var i = 0;
            foreach (var each in enumerable)
                toAction(each, i++);
        }

        /// <summary>
        /// インデックス付イテレートメソッドです。各要素に対して<paramref name="toAction"/>を実行します。
        /// インデックスを利用したい場合に利用してください。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="enumerable">対象の配列</param>
        /// <param name="toAction">要素に対して実行するアクション。<c>int</c>パラメーターはアクセスインデックス</param>
        [DebuggerStepThrough]
        public static void ForEach<T>(this T[] enumerable, Action<T, int> toAction)
        {
            var i = 0;
            foreach (var each in enumerable)
                toAction(each, i++);
        }

        /// <summary>
        /// コレクション内のインデックスを返します。存在しない場合は -1 を返します。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="enumerable">対象の<see cref="IEnumerable{T}"/></param>
        /// <param name="func">要素に対する判定処理</param>
        [DebuggerStepThrough]
        public static int IndexOf<T>(this IEnumerable<T> enumerable, Predicate<T> func)
        {
            int i = 0;
            foreach (T each in enumerable)
            {
                if (func(each))
                    return i;
                i++;
            }
            return -1;
        }

        /// <summary>
        /// コレクション内のインデックスを返します。存在しない場合は -1 を返します。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="enumerable">対象の配列</param>
        /// <param name="func">要素に対する判定処理</param>
        [DebuggerStepThrough]
        public static int IndexOf<T>(this T[] enumerable, Predicate<T> func)
        {
            int i = 0;
            foreach (T each in enumerable)
            {
                if (func(each))
                    return i;
                i++;
            }
            return -1;
        }

        /// <summary>
        /// 各要素に対して副作用を与えつつ要素の一覧を返します。
        /// </summary>
        [DebuggerStepThrough]
        public static IEnumerable<T> With<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var value in enumerable)
            {
                action(value);
                yield return value;
            }
        }

        /// <summary>
        /// 要素の数が指定の数と一致するかどうかを判断します。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="source">カウントする要素が格納されているシーケンス</param>
        /// <param name="count">期待する要素数</param>
        /// <returns>要素の数が指定の数と一致するとき<see langword="true"/></returns>
        [DebuggerStepThrough]
        public static bool IsCount<T>(this IEnumerable<T> source, int count)
        {
            var index = 0;
            using (var sourceEnumerator = source.GetEnumerator())
            {
                while (sourceEnumerator.MoveNext())
                {
                    index++;
                    if (count < index)
                    {
                        break;
                    }
                }
            }

            return index == count;
        }

        /// <summary>
        /// 要素の数が指定の数以上かどうかを判断します。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="source">カウントする要素が格納されているシーケンス</param>
        /// <param name="count">期待する要素数</param>
        /// <param name="predicate">その要素をカウントするか否かを返す関数</param>
        /// <returns>要素の数が指定の数以上のとき<see langword="true"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/>が0未満です。</exception>
        public static bool IsGreaterEqual<T>(this IEnumerable<T> source, int count, Func<T, bool> predicate = null)
        {
            CheckGreaterEqual(count);

            var index = 0;
            using (var sourceEnumerator = source.GetEnumerator())
            {
                while (sourceEnumerator.MoveNext())
                {
                    if (predicate == null || predicate(sourceEnumerator.Current))
                        index++;

                    if (count == index)
                        return true;
                }
            }

            return count == 0;
        }

        /// <summary>
        /// 要素の数が指定の数以下かどうかを判断します。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="source">カウントする要素が格納されているシーケンス</param>
        /// <param name="count">期待する要素数</param>
        /// <returns>要素の数が指定の数以下のとき<see langword="true"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/>が0未満です。</exception>
        public static bool IsLesserEqual<T>(this IEnumerable<T> source, int count)
        {
            CheckGreaterEqual(count);

            var index = 0;
            using var sourceEnumerator = source.GetEnumerator();
            while (sourceEnumerator.MoveNext())
            {
                index++;
                if (count < index)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 要素が重複しているかどうかを判断します。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="source">処理を適用する値のシーケンス</param>
        /// <returns>要素が重複しているとき<see langword="true"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>が<see langword="null"/>です。</exception>
        [DebuggerStepThrough]
        public static bool IsOverlapped<T>(this IEnumerable<T> source)
        {
            var set = new HashSet<T>();
            foreach (var each in source)
            {
                if (set.Contains(each))
                    return true;

                set.Add(each);
            }

            return false;
        }

        /// <summary>
        /// 要素が重複しているかどうかを判断します。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="source">処理を適用する値のシーケンス</param>
        /// <param name="element">重複した要素（重複がない場合はデフォルト値になります）</param>
        /// <returns>要素が重複しているとき<see langword="true"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>が<see langword="null"/>です。</exception>
        [DebuggerStepThrough]
        public static bool IsOverlapped<T>(this IEnumerable<T> source, out T element)
        {
            var set = new HashSet<T>();
            foreach (var each in source)
            {
                if (set.Contains(each))
                {
                    element = each;
                    return true;
                }
                else
                {
                    set.Add(each);
                }
            }

            element = default;
            return false;
        }

        /// <summary>
        /// 詳細はオーバーロード版を参照してください。
        /// </summary>
        [DebuggerStepThrough]
        public static bool IsSingle<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Any() && !enumerable.Skip(1).Any();
        }

        /// <summary>
        /// <paramref name="enumerable" />内の要素が単一であるかどうかを確認する処理です。
        /// 単一であることを条件に処理を実行したい場合に使用してください。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="enumerable">対象の<see cref="IEnumerable{T}"/></param>
        /// <param name="predicate">一致条件</param>
        [DebuggerStepThrough]
        public static bool IsSingle<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            int count = 0;
            foreach (var each in enumerable)
            {
                if (predicate(each))
                {
                    count++;
                    if (count == 2)
                    {
                        break;
                    }
                }
            }

            return count == 1;
        }

        /// <summary>
        /// 要素が1種類かどうかを判断します。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="source">処理を適用する値のシーケンス</param>
        /// <returns>要素が1種類のとき<see langword="true"/>。要素が空の場合は<see langword="false"/>になります。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>が<see langword="null"/>です。
        /// </exception>
        [DebuggerStepThrough]
        public static bool IsSingleKind<T>(this IEnumerable<T> source)
        {
            //Guard.ArgumentNotNull(source, nameof(source));

            using (var enumerator = source.GetEnumerator())
            {
                if (enumerator.MoveNext() == false)
                {
                    return false;
                }

                T first = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (first == null)
                    {
                        if (current != null)
                        {
                            return false;
                        }
                    }
                    else if (first.Equals(current) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// シーケンスの最大値を返します。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="source">処理を適用する値のシーケンス</param>
        /// <param name="comparer">値を比較する<see cref="IEqualityComparer{T}"/></param>
        /// <returns>シーケンスの最大値</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>が<see langword="null"/>です。</exception>
        [DebuggerStepThrough]
        public static T MaxBy<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            //Guard.ArgumentNotNull(source, nameof(source));

            return MaxByInternal(source, comparer);
        }

        /// <summary>
        /// シーケンスの最大値を返します。
        /// </summary>
        /// <typeparam name="TSource">各要素の型</typeparam>
        /// <typeparam name="TKey">比較する値の型</typeparam>
        /// <param name="source">処理を適用する値のシーケンス</param>
        /// <param name="keySelector">比較する値に変換する関数</param>
        /// <returns>シーケンスの最大値</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>または<paramref name="keySelector"/>が<see langword="null"/>です。</exception>
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            //Guard.ArgumentNotNull(source, nameof(source));
            //Guard.ArgumentNotNull(keySelector, nameof(keySelector));

            return MaxByInternal(source, new SelectionComparer<TSource, TKey>(keySelector));
        }

        /// <summary>
        /// 要素内の最大値の取得を行います。
        /// 要素が存在しない場合は<paramref name="defaultValue"/>の値を返します。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="enumerable">対象の<see cref="IEnumerable{T}"/></param>
        /// <param name="defaultValue">要素が存在しない場合の既定値(未指定の場合は型の既定値)</param>
        /// <returns>最大値</returns>
        [DebuggerStepThrough]
        public static T MaxOrDefault<T>(this IEnumerable<T> enumerable, T defaultValue = default)
        {
            return enumerable.Any() ? enumerable.Max() : defaultValue;
        }

        /// <summary>
        /// 要素内の最大値の取得を行います。
        /// 要素が存在しない場合は<paramref name="defaultValue"/>の値を返します。
        /// </summary>
        /// <typeparam name="TSource">各要素の型</typeparam>
        /// <typeparam name="TResult"><paramref name="selector"/>によって返される値の型</typeparam>
        /// <param name="enumerable">対象の<see cref="IEnumerable{T}"/></param>
        /// <param name="selector">最大値を検出するための関数</param>
        /// <param name="defaultValue">要素が存在しない場合の既定値(未指定の場合は型の既定値)</param>
        /// <returns>最大値</returns>
        [DebuggerStepThrough]
        public static TResult MaxOrDefault<TSource, TResult>(
            this IEnumerable<TSource> enumerable,
            Func<TSource, TResult> selector,
            TResult defaultValue = default)
        {
            return enumerable.Any() ? enumerable.Max(selector) : defaultValue;
        }

        /// <summary>
        /// 要素内の最小値の取得を行います。
        /// 要素が存在しない場合は<paramref name="defaultValue"/>の値を返します。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="enumerable">対象の<see cref="IEnumerable{T}"/></param>
        /// <param name="defaultValue">要素が存在しない場合の既定値(未指定の場合は型の既定値)</param>
        /// <returns>最小値</returns>
        [DebuggerStepThrough]
        public static T MinOrDefault<T>(this IEnumerable<T> enumerable, T defaultValue = default)
        {
            return enumerable.Any() ? enumerable.Min() : defaultValue;
        }

        /// <summary>
        /// 要素内の最小値の取得を行います。
        /// 要素が存在しない場合は<paramref name="defaultValue"/>の値を返します。
        /// </summary>
        /// <typeparam name="TSource">各要素の型</typeparam>
        /// <typeparam name="TResult"><paramref name="selector"/>によって返される値の型</typeparam>
        /// <param name="enumerable">対象の<see cref="IEnumerable{T}"/></param>
        /// <param name="selector">最小値を検出するための関数</param>
        /// <param name="defaultValue">要素が存在しない場合の既定値(未指定の場合は型の既定値)</param>
        /// <returns>最小値</returns>
        [DebuggerStepThrough]
        public static TResult MinOrDefault<TSource, TResult>(this IEnumerable<TSource> enumerable,
                                                             Func<TSource, TResult> selector,
                                                             TResult defaultValue = default)
        {
            return enumerable.Any() ? enumerable.Min(selector) : defaultValue;
        }

        /// <summary>
        /// 再帰列挙を行うための拡張メソッドです。アルゴリズムは深さ優先探索になります。
        /// <para>
        /// <example>
        /// <code>
        /// // MEMO : ツリーにおいて。左端のノードから右方向にある「非表示かつ選択済みノード」を走査して選択解除
        /// leftestNodes
        ///     .Recursive(node => node.RightNodes)
        ///     .Where(node => node.Selected &amp; !node.Visible)
        ///     .ForEach(node => node.Selected = false);
        /// </code>
        /// </example>
        /// </para>
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="enumerable">処理を適用する<see cref="IEnumerable{T}"/>インスタンス</param>
        /// <param name="get">
        /// 親要素から子要素の集合を取得する処理です。
        /// <see langword="null"/>を返した場合は再帰を停止します。
        /// </param>
        /// <returns>ツリー構造に再帰的にアクセスされる<see cref="IEnumerable{T}"/>インスタンス</returns>
        [DebuggerStepThrough]
        public static IEnumerable<T> Recursive<T>(this IEnumerable<T> enumerable, Func<T, IEnumerable<T>> get)
        {
            foreach (T each in enumerable)
            {
                yield return each;

                var children = get(each);
                if (children != null)
                {
                    foreach (T eachRecurse in Recursive(children, get))
                    {
                        yield return eachRecurse;
                    }
                }
            }
        }

        /// <summary>
        /// 要素を返した後に指定された条件を判定し、条件が満たされる前と、その直後に出現するシーケンスの要素を返します。
        /// </summary>
        /// <typeparam name="TSource"><paramref name="source"/>の要素の型</typeparam>
        /// <param name="source">要素を返すシーケンス</param>
        /// <param name="predicate">各要素が条件を満たしているかどうかをテストする関数</param>
        /// <returns>テストに合格しなくなった要素の前と、その直後に出現する、入力シーケンスの要素</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>または<paramref name="predicate"/>が<see langword="null"/>です。</exception>
        public static IEnumerable<TSource> TakeDoWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (var item in source)
            {
                yield return item;
                if (predicate(item) == false)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// <see cref="IEnumerable{T}"/>から<see cref="HashSet{T}"/>を作成します。
        /// このメソッドが呼び出された段階で<see cref="IEnumerable{T}"/>に対する評価が行われます。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="enumerable">処理を適用する<see cref="IEnumerable{T}"/>インスタンス</param>
        /// <returns>作成されたハッシュセットを返却します。</returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
        {
            return new HashSet<T>(enumerable);
        }

        /// <summary>
        /// <see cref="IEnumerable{T}"/>から<see cref="HashSet{T}"/>を作成します。
        /// このメソッドが呼び出された段階で<see cref="IEnumerable{T}"/>に対する評価が行われます。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <param name="enumerable">処理を適用する<see cref="IEnumerable{T}"/>インスタンス</param>
        /// <param name="comparer">セット内の値を比較に使用する<see cref="IEqualityComparer{T}"/>インスタンス</param>
        /// <returns>作成されたハッシュセットを返却します。</returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable, IEqualityComparer<T> comparer)
        {
            return new HashSet<T>(enumerable, comparer);
        }

        /// <summary>
        /// 要素の指定項目を射影した<see cref="HashSet{T2}"/>を作成します。
        /// このメソッドが呼び出された段階で<see cref="IEnumerable{T}"/>に対する評価が行われます。
        /// </summary>
        /// <typeparam name="T">各要素の型</typeparam>
        /// <typeparam name="T2">ハッシュセットの要素の型</typeparam>
        /// <param name="enumerable">処理を適用する<see cref="IEnumerable{T}"/>インスタンス</param>
        /// <param name="selectFunction">射影を行うファンクション</param>
        /// <returns>作成されたハッシュセットを返却します。</returns>
        public static HashSet<T2> ToHashSet<T, T2>(this IEnumerable<T> enumerable, Func<T, T2> selectFunction)
        {
            return enumerable.Select(selectFunction).ToHashSet();
        }

        /// <summary>
        /// 指定されたサイズにしたがって、<see cref=" IEnumerable{TSource} "/>から
        /// <see cref="IEnumerable{IEnumerable}"/>を作成します。
        /// </summary>
        /// <typeparam name="TSource">作成元の型</typeparam>
        /// <param name="source">作成元の<see cref="IEnumerable{TSource}"/></param>
        /// <param name="splitSize">分割する指定のサイズ</param>
        /// <returns>分割された<see cref="IEnumerable{IEnumerable}"/></returns>
        public static IEnumerable<IEnumerable<TSource>> Split<TSource>(this IEnumerable<TSource> source, int splitSize)
        {
            var enumerator = source.GetEnumerator();
            var list = new List<TSource>();
            while (enumerator.MoveNext())
            {
                list.Add(enumerator.Current);
                if (list.Count == splitSize)
                {
                    yield return list;
                    list = new List<TSource>();
                }
            }
            if (list.Any())
            {
                yield return list;
            }
        }

        /// <summary>
        /// <see cref="IEnumerable{TSource}"/>から<see cref="List{TValue}"/>を値として格納する<see cref="Dictionary{K, V}"/>を作成します。
        /// </summary>
        /// <typeparam name="TKey"><paramref name="keySelector"/>によって返されるキーの型</typeparam>
        /// <typeparam name="TValue"><paramref name="valueSelector"/>によって返される値の型</typeparam>
        /// <typeparam name="TSource"><paramref name="source"/>の要素の型</typeparam>
        /// <param name="source">作成元の<see cref="IEnumerable{TSource}"/></param>
        /// <param name="keySelector">各要素からキーを抽出する関数</param>
        /// <param name="valueSelector">各要素から値を抽出する関数</param>
        /// <returns>キーと値の<see cref="List{TValue}"/>を格納している<see cref="Dictionary{K, V}"/></returns>
        public static Dictionary<TKey, List<TValue>> ToValueListDictionary<TKey, TValue, TSource>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
            where TKey : notnull
        {
            return source.Aggregate(
                new Dictionary<TKey, List<TValue>>(),
                (map, element) =>
                {
                    var key = keySelector(element);
                    var value = valueSelector(element);

                    if (map.TryGetValue(key, out var innerList))
                        innerList.Add(value);
                    else
                        map[key] = new List<TValue> { value };

                    return map;
                });
        }

        /// <summary>
        /// 将<see cref="Dictionary{TKey, TValue}"/>的Value转换为<see cref="List{TValue}"/>
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="valueSelector"></param>
        /// <returns>转换后的<see cref="List{TKey}"/></returns>
        [DebuggerStepThrough]
        public static List<TValue> ToValueList<TKey, TValue>
            (this IDictionary<TKey, TValue> source, Func<TKey, bool> keySelector = null, Func<TValue, bool> valueSelector = null)
        {
            return source
                .Where(each => (keySelector?.Invoke(each.Key) ?? true) && (valueSelector?.Invoke(each.Value) ?? true))
                .Select(each => each.Value).ToList();
        }

        /// <summary>
        /// <see cref="IEnumerable{TSource}"/>から<see cref="ConcurrentDictionary{TKey, TValue}"/>を作成します。
        /// </summary>
        /// <typeparam name="TKey"><paramref name="keySelector"/>によって返されるキーの型パラメーター</typeparam>
        /// <typeparam name="TValue"><paramref name="valueSelector"/>によって返される値の型パラメーター</typeparam>
        /// <typeparam name="TSource"><paramref name="source"/>の要素の型パラメーター</typeparam>
        /// <param name="source">作成元の<see cref="IEnumerable{TSource}"/></param>
        /// <param name="keySelector">各要素からキーを抽出する関数</param>
        /// <param name="valueSelector">各要素から値を抽出する関数</param>
        /// <returns>作成した<see cref="ConcurrentDictionary{TKey, TValue}"/></returns>
        public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue, TSource>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
            where TKey : notnull
        {
            return source.Aggregate(
                new ConcurrentDictionary<TKey, TValue>(),
                (map, element) =>
                {
                    var key = keySelector(element);
                    var value = valueSelector(element);
                    map.TryAdd(key, value);
                    return map;
                });
        }

        /// <summary>
        /// <see cref="ParallelQuery{TSource}"/>から<see cref="ConcurrentDictionary{TKey, TValue}"/>を作成します。
        /// 並列処理を行います。
        /// </summary>
        /// <typeparam name="TKey"><paramref name="keySelector"/>によって返されるキーの型パラメーター</typeparam>
        /// <typeparam name="TValue"><paramref name="valueSelector"/>によって返される値の型パラメーター</typeparam>
        /// <typeparam name="TSource"><paramref name="source"/>の要素の型パラメーター</typeparam>
        /// <param name="source">作成元の<see cref="ParallelQuery{TSource}"/></param>
        /// <param name="keySelector">各要素からキーを抽出する関数</param>
        /// <param name="valueSelector">各要素から値を抽出する関数</param>
        /// <returns>作成した<see cref="ConcurrentDictionary{TKey, TValue}"/></returns>
        public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue, TSource>
            (this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
            where TKey : notnull
        {
            var map = new ConcurrentDictionary<TKey, TValue>();
            source.ForAll(e => map.TryAdd(keySelector(e), valueSelector(e)));
            return map;
        }

        /// <summary>
        /// <paramref name="count"/>が0以上でない場合は例外とします。
        /// </summary>
        private static void CheckGreaterEqual(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), $"[{nameof(count)}] must be equal or greater than 0.");
        }

        /// <summary>
        /// シーケンスの最大値を返す内部メソッドです。
        /// </summary>
        private static T MaxByInternal<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            if (comparer == null)
            {
                return source.Max();
            }

            using var e = source.GetEnumerator();
            if (!e.MoveNext())
            {
                throw new InvalidOperationException("Source sequence doesn't contain any elements.");
            }

            var result = e.Current;
            while (e.MoveNext())
            {
                var current = e.Current;
                if (0 < comparer.Compare(current, result))
                {
                    result = current;
                }
            }

            return result;
        }

        /// <summary>
        /// 重複している要素を返す内部メソッドです。
        /// </summary>
        private static IEnumerable<T> OverlappedInternal<T>(
            this IEnumerable<T> source,
            IEqualityComparer<T> comparer)
            where T : notnull
        {
            var exists = new Dictionary<T, bool>(comparer);
            foreach (var each in source)
            {
                if (exists.TryGetValue(each, out var isExist))
                {
                    if (isExist)
                        continue;

                    exists[each] = true;
                    yield return each;
                }
                else
                {
                    exists.Add(each, false);
                }
            }
        }

        /// <summary>
        /// 取得枚举中的随机项目
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="enumerable">对象数组</param>
        [DebuggerStepThrough]
        public static T Random<T>(this IEnumerable<T> enumerable)
            => enumerable.OrderBy(each => Guid.NewGuid()).First();

        /// <summary>
        /// 取得枚举中的随机项目, 如果无项目则返回null
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="enumerable">对象数组</param>
        [DebuggerStepThrough]
        public static T RandomOrDefault<T>(this IEnumerable<T> enumerable)
            => enumerable.OrderBy(each => Guid.NewGuid()).FirstOrDefault();

        /// <summary>
        /// 取得数组中的随机项目
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="enumerable">对象数组</param>
        [DebuggerStepThrough]
        public static T Random<T>(this T[] enumerable)
            => enumerable.OrderBy(each => Guid.NewGuid()).First();

        /// <summary>
        /// 取得数组中的随机项目, 如果无项目则返回null
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="enumerable">对象数组</param>
        [DebuggerStepThrough]
        public static T RandomOrDefault<T>(this T[] enumerable)
            => enumerable.OrderBy(each => Guid.NewGuid()).FirstOrDefault();

        /// <summary>
        /// 指定次数循环处理
        /// </summary>
        /// <param name="count">回数</param>
        /// <param name="toAction">执行处理</param>
        [DebuggerStepThrough]
        public static void Times(this int count, Action<int> toAction)
        {
            for (var i = 0; i < count; i++)
                toAction(i);
        }

        /// <summary>
        /// List复制项目后增加新项目
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="enumerable">对象数组</param>
        /// <param name="addItem">增加的项目</param>
        [DebuggerStepThrough]
        public static List<T> CopyAdd<T>(this List<T> enumerable, T addItem)
            => new(enumerable ?? new List<T>()) { addItem };

        /// <summary>
        /// Dictionary复制项目后增加新项目
        /// </summary>
        /// <typeparam name="TKey">Key类型</typeparam>
        /// <typeparam name="TValue">Value类型</typeparam>
        /// <param name="enumerable">对象<see cref="Dictionary{TKey,TValue}"/></param>
        /// <param name="addItemKey">增加项目的Key</param>
        /// <param name="addItemValue">增加项目的Value</param>
        [DebuggerStepThrough]
        public static Dictionary<TKey, TValue> CopyAdd<TKey, TValue>(
            this Dictionary<TKey, TValue> enumerable,
            TKey addItemKey,
            TValue addItemValue)
            where TKey : notnull
        {
            enumerable ??= new Dictionary<TKey, TValue>();
            enumerable.Add(addItemKey, addItemValue);
            return new Dictionary<TKey, TValue>(enumerable);
        }

        /// <summary>
        /// ConcurrentDictionary复制项目后增加新项目
        /// </summary>
        /// <typeparam name="TKey">Key类型</typeparam>
        /// <typeparam name="TValue">Value类型</typeparam>
        /// <param name="enumerable">对象<see cref="Dictionary{TKey,TValue}"/></param>
        /// <param name="addItemKey">增加项目的Key</param>
        /// <param name="addItemValue">增加项目的Value</param>
        [DebuggerStepThrough]
        public static ConcurrentDictionary<TKey, TValue> CopyAdd<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> enumerable,
            TKey addItemKey,
            TValue addItemValue)
            where TKey : notnull
        {
            enumerable ??= new ConcurrentDictionary<TKey, TValue>();
            enumerable.TryAdd(addItemKey, addItemValue);
            return new ConcurrentDictionary<TKey, TValue>(enumerable);
        }

        /// <summary>
        /// <see cref="HashSet{T}"/>复制后增加新项目
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="hashSet">对象<see cref="HashSet{T}"/></param>
        /// <param name="addItem">增加的项目</param>
        [DebuggerStepThrough]
        public static HashSet<T> CopyAdd<T>(
            this HashSet<T> hashSet,
            T addItem)
            where T : unmanaged
        {
            return hashSet == null
                ? new HashSet<T> { addItem }
                : new HashSet<T>(hashSet) { addItem };
        }

        /// <summary>
        /// <see cref="HashSet{T}"/>复制后删除项目
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="hashSet">对象<see cref="HashSet{T}"/></param>
        /// <param name="deleteItem">删除的项目</param>
        /// <param name="deleteSuccessed">是否删除成功</param>
        [DebuggerStepThrough]
        public static HashSet<T> CopyRemove<T>(
            this HashSet<T> hashSet,
            T deleteItem,
            out bool deleteSuccessed)
            where T : unmanaged
        {
            if (hashSet == null)
            {
                deleteSuccessed = false;
                return new HashSet<T>();
            }

            deleteSuccessed = hashSet.Remove(deleteItem);
            return new HashSet<T>(hashSet);
        }

        /// <summary>
        /// <see cref="HashSet{T}"/>复制后删除项目
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="hashSet">对象<see cref="HashSet{T}"/></param>
        /// <param name="deleteItem">删除的项目</param>
        [DebuggerStepThrough]
        public static HashSet<T> CopyRemove<T>(
            this HashSet<T> hashSet,
            T deleteItem)
            where T : unmanaged
        {
            return CopyRemove(hashSet, deleteItem, out _);
        }

        /// <summary>
        /// Dictionary复制项目后编辑项目
        /// </summary>
        /// <typeparam name="TKey">Key类型</typeparam>
        /// <typeparam name="TValue">Value类型</typeparam>
        /// <param name="enumerable">对象<see cref="Dictionary{TKey,TValue}"/></param>
        /// <param name="editItemKey">编辑项目的Key</param>
        /// <param name="actionValue">编辑方法</param>
        [DebuggerStepThrough]
        public static Dictionary<TKey, TValue> CopyEdit<TKey, TValue>(
            this Dictionary<TKey, TValue> enumerable,
            TKey editItemKey,
            Action<TValue> actionValue)
            where TKey : notnull
        {
            var editItem = enumerable[editItemKey];
            actionValue(editItem);
            return new Dictionary<TKey, TValue>(enumerable);
        }

        /// <summary>
        /// ConcurrentDictionary复制项目后编辑项目
        /// </summary>
        /// <typeparam name="TKey">Key类型</typeparam>
        /// <typeparam name="TValue">Value类型</typeparam>
        /// <param name="enumerable">对象<see cref="Dictionary{TKey,TValue}"/></param>
        /// <param name="editItemKey">编辑项目的Key</param>
        /// <param name="actionValue">编辑方法</param>
        [DebuggerStepThrough]
        public static ConcurrentDictionary<TKey, TValue> CopyEdit<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> enumerable,
            TKey editItemKey,
            Action<TValue> actionValue)
            where TKey : notnull
        {
            var editItem = enumerable[editItemKey];
            actionValue(editItem);
            return new ConcurrentDictionary<TKey, TValue>(enumerable);
        }

        /// <summary>
        /// Dictionary复制项目后删除项目
        /// </summary>
        /// <typeparam name="TKey">Key类型</typeparam>
        /// <typeparam name="TValue">Value类型</typeparam>
        /// <param name="enumerable">对象<see cref="Dictionary{TKey,TValue}"/></param>
        /// <param name="deleteItemKey">删除项目的Key</param>
        [DebuggerStepThrough]
        public static Dictionary<TKey, TValue> CopyRemove<TKey, TValue>(
            this Dictionary<TKey, TValue> enumerable,
            TKey deleteItemKey)
            where TKey : notnull
        {
            enumerable.Remove(deleteItemKey);
            return new Dictionary<TKey, TValue>(enumerable);
        }

        /// <summary>
        /// ConcurrentDictionary复制项目后删除项目
        /// </summary>
        /// <typeparam name="TKey">Key类型</typeparam>
        /// <typeparam name="TValue">Value类型</typeparam>
        /// <param name="enumerable">对象<see cref="Dictionary{TKey,TValue}"/></param>
        /// <param name="deleteItemKey">删除项目的Key</param>
        [DebuggerStepThrough]
        public static ConcurrentDictionary<TKey, TValue> CopyRemove<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> enumerable,
            TKey deleteItemKey)
            where TKey : notnull
        {
            enumerable.TryRemove(deleteItemKey, out _);
            return new ConcurrentDictionary<TKey, TValue>(enumerable);
        }

        /// <summary>
        /// Dictionary 排序项目
        /// </summary>
        /// <typeparam name="TKey">Key类型</typeparam>
        /// <typeparam name="TValue">Value类型</typeparam>
        /// <param name="enumerable">对象<see cref="Dictionary{TKey,TValue}"/></param>
        /// <param name="orderByFunc">OrderBy方法</param>
        [DebuggerStepThrough]
        public static Dictionary<int, TValue> CopySort<TKey, TValue>(
            this Dictionary<TKey, TValue> enumerable,
            Func<KeyValuePair<TKey, TValue>, object> orderByFunc)
            where TKey : notnull
        {
            var index = 0;
            return new Dictionary<int, TValue>(
                enumerable
                    .OrderBy(orderByFunc)
                    .ToDictionary(each => index++, each => each.Value));
        }

        /// <summary>
        /// Dictionary 排序项目
        /// </summary>
        /// <typeparam name="TKey">Key类型</typeparam>
        /// <typeparam name="TValue">Value类型</typeparam>
        /// <param name="enumerable">对象<see cref="Dictionary{TKey,TValue}"/></param>
        [DebuggerStepThrough]
        public static ConcurrentDictionary<int, TValue> CopySort<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> enumerable,
            Func<KeyValuePair<TKey, TValue>, object> orderByFunc)
            where TKey : notnull
        {
            var index = 0;
            return new ConcurrentDictionary<int, TValue>(
                enumerable
                    .OrderBy(orderByFunc)
                    .ToConcurrentDictionary(each => index++, each => each.Value));
        }

        /// <summary>
        /// Dictionary 取得Key的最大值+1
        /// </summary>
        /// <typeparam name="TKey">Key类型</typeparam>
        /// <typeparam name="TValue">Value类型</typeparam>
        /// <param name="enumerable">对象<see cref="Dictionary{TKey,TValue}"/></param>
        [DebuggerStepThrough]
        public static int GetSequence<TKey, TValue>(this Dictionary<TKey, TValue> enumerable)
            where TKey : notnull
        {
            return typeof(TKey).Name switch
            {
                "Int32" => enumerable?.Any() == true
                    ? enumerable.Select(each => int.TryParse(each.Key.ToString(), out var keyValue) ? keyValue : 0)
                        .Max() + 1
                    : 0,
                _ => 0
            };
        }

        /// <summary>
        /// ConcurrentDictionary 取得Key的最大值+1
        /// </summary>
        /// <typeparam name="TKey">Key类型</typeparam>
        /// <typeparam name="TValue">Value类型</typeparam>
        /// <param name="enumerable">对象<see cref="ConcurrentDictionary{TKey,TValue}"/></param>
        [DebuggerStepThrough]
        public static int GetSequence<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> enumerable)
            where TKey : notnull
        {
            return typeof(TKey).Name switch
            {
                "Int32" => enumerable?.Any() == true
                    ? (enumerable.Select(each => int.TryParse(each.Key.ToString(), out var keyValue) ? keyValue : 0)
                        .Max()) + 1
                    : 0,
                _ => 0
            };
        }
    }
}