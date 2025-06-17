
namespace NskWeb.Areas.F207.Models.D207020
{
    /// <summary>
    /// 当初評価高計算処理（半相殺）画面項目モデル（検索結果部分）
    /// </summary>
    [Serializable]
    public class D207020SearchResult
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public D207020SearchResult()
        {
            TableRecords = new List<D207020TableRecord>();
            EnterCtrlFlg = "0";
        }

        /// <summary>
        /// 実行履歴一覧
        /// </summary>
        public List<D207020TableRecord> TableRecords { get; set; }

        /// <summary>
        /// 実行履歴全件数
        /// </summary>
        public int TotalCount { get; set; }
    
        /// <summary>
        /// 実行ボタン制御フラグ
        /// </summary>
        public string EnterCtrlFlg { get; set; }
    }
}
