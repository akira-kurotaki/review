using System.ComponentModel.DataAnnotations;

namespace NskWeb.Areas.F207.Models.D207010
{
    /// <summary>
    /// 引受情報計算結果
    /// </summary>
    [Serializable]
    public class D207010TotalColumn
    {
        /// <summary>
        /// 悉皆調査面積合計
        /// </summary>
        [Display(Name = "悉皆調査面積合計")]
        public decimal? ShikkaiChosaMensakiGokei { get; set; }
        /// <summary>
        /// 平均単収差左加重値合計
        /// </summary>
        [Display(Name = "平均単収差左加重値合計")]
        public decimal? HeikinTanshusaHidariKajuchiGokei { get; set; }
        /// <summary>
        /// 単当修正量左加重値合計
        /// </summary>
        [Display(Name = "単当修正量左加重値合計")]
        public decimal? TantoShuseiryoHidariKajuchiGokei { get; set; }

        public D207010TotalColumn() { }
    }
}
