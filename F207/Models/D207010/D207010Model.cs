using CoreLibrary.Core.Base;
using NskWeb.Areas.F207.Consts;

namespace NskWeb.Areas.F207.Models.D207010
{
    [Serializable]
    public class D207010Model : CoreViewModel
    {
        #region "メッセージエリア"
        /// <summary>
        /// メッセージエリア1
        /// </summary>
        public string MessageArea1 { get; set; } = string.Empty;
        /// <summary>
        /// メッセージエリア2
        /// </summary>
        public string MessageArea2 { get; set; } = string.Empty;
        /// <summary>
        /// メッセージエリア3
        /// </summary>
        public string MessageArea3 { get; set; } = string.Empty;
        #endregion

        #region "ヘッダ部"
        /// <summary>
        /// 引受年産
        /// </summary>
        public string Nensan { get; set; } = string.Empty;
        /// <summary>
        /// 共済目的
        /// </summary>
        public string KyosaiMokuteki { get; set; } = string.Empty;
        #endregion

        #region "検索条件"
        /// <summary>
        /// 検索条件
        /// </summary>
        public D207010SearchCondition SearchCondition { get; set; } = new();
        #endregion

        #region "検索結果"
        /// <summary>
        /// 検索結果
        /// </summary>
        public D207010SearchResult SearchResult { get; set; } = new();
        #endregion

        #region "合計欄"
        /// <summary>
        /// 合計欄
        /// </summary>
        public D207010TotalColumn TotalColumn { get; set; }
        #endregion

        #region "画面権限"
        /// <summary>
        /// 画面権限
        /// </summary>
        public F207Const.Authority DispKengen { get; set; }
        #endregion
    }
}
