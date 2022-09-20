namespace SheepQQBot3.Model.Fund
{
    /// <summary>
    /// 基金持仓数据
    /// </summary>
    public class FundPostionData
    {
        public string Code { get; set; }

        public string Message { get; set; }

        public FundStockData Data { get; set; }
    }

    /// <summary>
    /// 简单基金信息Json数据类型
    /// </summary>
    public class FundStockData
    {
        /// <summary>
        /// 持仓更新日期
        /// </summary>
        public DateTime Date { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// 基金持仓
        /// </summary>
        public List<List<string>> StockList { get; set; }
    }
}