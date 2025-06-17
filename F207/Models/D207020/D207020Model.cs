using CoreLibrary.Core.Base;
using CoreLibrary.Core.Dto;
using NskWeb.Areas.F207.Consts;
using System.ComponentModel.DataAnnotations;

namespace NskWeb.Areas.F207.Models.D207020
{
    /// <summary>
    /// 当初評価高計算処理（半相殺）画面項目モデル
    /// </summary>
    [Serializable]
    public class D207020Model: CoreViewModel
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public D207020Model()
        {
            this.SearchCondition = new D207020SearchCondition();
            this.SearchResult = new D207020SearchResult();
            this.DispKengen = F207Const.Authority.None;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="syokuin">ユーザー情報（セッション）</param>
        /// <param name="shishoList">利用可能な支所一覧（セッション）</param>
        public D207020Model(Syokuin syokuin, List<Shisho> shishoList)
        {
            this.SearchCondition = new D207020SearchCondition(syokuin, shishoList);
            this.SearchResult = new D207020SearchResult();
            this.DispKengen = F207Const.Authority.None;
        }

        /// <summary>
        /// 共済目的コード
        /// </summary>
        [Display(Name = "共済目的コード")]
        public string KyosaiMokutekiCd { get; set; }/// <summary>

        /// <summary>
        /// 年産
        /// </summary>
        [Display(Name = "年産")]
        public string Nensan { get; set; }/// <summary>

        /// メッセージエリア1
        /// </summary>
        [Display(Name = "メッセージエリア1")]
        public string MessageArea1 { get; set; }

        /// <summary>
        /// 検索条件
        /// </summary>
        [Display(Name = "検索条件")]
        public D207020SearchCondition SearchCondition { get; set; }

        /// <summary>
        /// 検索結果
        /// </summary>
        [Display(Name = "検索結果")]
        public D207020SearchResult SearchResult { get; set; }

        #region "画面権限"
        /// <summary>
        /// 画面権限
        /// </summary>
        public F207Const.Authority DispKengen { get; set; }
        #endregion

        #region "項目活性・非活性制御"
        /// <summary>
        /// 本所・支所ドロップダウンリスト活性・非活性制御
        /// </summary>
        public bool IsShishoDropDownDisabled { get; set; } = false;
        /// <summary>
        /// 実行ボタン活性・非活性制御
        /// </summary>
        public bool IsJikkoBtnDisabled { get; set; } = false;
        /// <summary>
        /// 戻るボタン非活性制御
        /// </summary>
        public bool IsModoruBtnDisabled { get; set; } = false;

        /// <summary>
        /// 農単抜取調査悉皆評価反映チェックフラグ
        /// </summary>
        public int IsCheckFlg { get; set; } 



        #endregion
    }
}
