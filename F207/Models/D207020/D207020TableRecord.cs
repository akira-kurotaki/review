using System.ComponentModel.DataAnnotations;

namespace NskWeb.Areas.F207.Models.D207020
{
    /// <summary>
    ///実行履歴画面項目モデル（実行履歴データ）
    /// </summary>
    [Serializable]
    public class D207020TableRecord
    {
        /// <summary>
        /// 支所コード
        /// </summary>
        [Required]
        [Display(Name = "支所コード")]
        public string ShishoCd { get; set; }

        /// <summary>
        /// 支所名
        /// </summary>
        [Display(Name = "支所名")]
        public string ShishoNm { get; set; }

        /// <summary>
        /// 引受方式
        /// </summary>
        [Display(Name = "引受方式")]
        public string HikiukeHoushiki { get; set; }

       
        /// <summary>
        /// 実行日
        /// </summary>
        [Display(Name = "実行日")]
        public string Jikkobi { get; set; }
    }
}
