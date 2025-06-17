using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NskAppModelLibrary.Context;
using NskCommonLibrary.Core.Consts;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NskWeb.Areas.F207.Models.D207010
{
    /// <summary>
    /// 検索条件
    /// </summary>
    public class D207010SearchCondition
    {
        #region "条件項目"
        /// <summary>
        /// 類区分
        /// </summary>
        [Required]
        [Display(Name = "類区分")]
        public string RuiKbn { get; set; } = string.Empty;
        /// <summary>
        /// 引受方式
        /// </summary>
        [Required]
        [Display(Name = "引受方式")]
        public string HikiukeHoushiki { get; set; } = string.Empty;
        /// <summary>
        /// 補償割合
        /// </summary>
        [Required]
        [Display(Name = "補償割合")]
        public string HoshouWariai { get; set; } = string.Empty;
        /// <summary>
        /// 階層区分
        /// </summary>
        [Required]
        [Display(Name = "階層区分")]
        public string KaisoKbn { get; set; } = string.Empty;
        #endregion

        #region "条件項目ドロップダウンリスト"
        /// <summary>
        /// 類区分ドロップダウンリスト
        /// </summary>
        public List<SelectListItem> RuiKbnLists { get; set; }
        /// <summary>
        /// 引受方式ドロップダウンリスト
        /// </summary>
        public List<SelectListItem> HikiukeHoushikiLists { get; set; }
        /// <summary>
        /// 補償割合ドロップダウンリスト
        /// </summary>
        public List<SelectListItem> HoshouWariaiLists { get; set; }
        /// <summary>
        /// 階層区分ドロップダウンリスト
        /// </summary>
        public List<SelectListItem> KaisoKbnLists { get; set; }
        #endregion

        #region "表示数・表示順"
        /// <summary>
        /// 表示数
        /// </summary>
        [Required]
        [Display(Name = "表示数")]
        public int DisplayCount { get; set; }

        /// <summary>
        /// 表示順キー１
        /// </summary>
        public string DisplaySort1 { get; set; } = string.Empty;
        /// <summary>
        /// 表示順１
        /// </summary>
        public CoreConst.SortOrder DisplaySortOrder1 { get; set; } = CoreConst.SortOrder.DESC;
        /// <summary>
        /// 表示順キー２
        /// </summary>
        public string DisplaySort2 { get; set; }
        /// <summary>
        /// 表示順２
        /// </summary>
        public CoreConst.SortOrder DisplaySortOrder2 { get; set; } = CoreConst.SortOrder.DESC;
        /// <summary>
        /// 表示順キー３
        /// </summary>
        public string DisplaySort3 { get; set; }
        /// <summary>
        /// 表示順３
        /// </summary>
        public CoreConst.SortOrder DisplaySortOrder3 { get; set; } = CoreConst.SortOrder.DESC;
        #endregion

        #region "表示順ドロップダウンリスト"
        /// <summary>
        /// 表示順ドロップダウンリスト
        /// </summary>
        public List<SelectListItem> DisplaySortTypes { get; set; }
        #endregion

        /// <summary>
        /// 画面入力値をこのこのクラスに反映する
        /// </summary>
        /// <param name="src"></param>
        public void ApplyInput(D207010SearchCondition src)
        {
            this.RuiKbn = src.RuiKbn;
            this.HikiukeHoushiki = src.HikiukeHoushiki;
            this.HoshouWariai = src.HoshouWariai;
            this.KaisoKbn = src.KaisoKbn;
            this.DisplayCount = src.DisplayCount;
            this.DisplaySort1 = src.DisplaySort1;
            this.DisplaySortOrder1 = src.DisplaySortOrder1;
            this.DisplaySort2 = src.DisplaySort2;
            this.DisplaySortOrder2 = src.DisplaySortOrder2;
            this.DisplaySort3 = src.DisplaySort3;
            this.DisplaySortOrder3 = src.DisplaySortOrder3;
        }

        #region "類区分ドロップダウンリストプロパティ"
        /// <summary>
        /// 類区分ドロップダウンリストプロパティ
        /// </summary>
        public class RuiKbnListModel()
        {
            /// <summary>
            /// 類区分
            /// </summary>
            public string 類区分 { get; set; }
            /// <summary>
            /// 類短縮名称
            /// </summary>
            public string 類短縮名称 { get; set; }
        }
        #endregion

        #region "条件項目ドロップダウンリストの取得"
        /// <summary>
        /// 類区分ドロップダウンリストの取得
        /// </summary>
        /// <param name="dbContext">DBコンテキスト</param>
        /// <param name="kumiaitoCd">組合等コード</param>
        /// <param name="kyosaiMokutekiCd">共済目的コード</param>
        public void GetRuiKbnLists(NskAppContext dbContext, string kumiaitoCd, string kyosaiMokutekiCd)
        {
            // (1) m_10170_選択引受方式、m_00020_類名称テーブルから類区分リストを取得する。
            StringBuilder sql = new();
            sql.Append($"	SELECT DISTINCT	");
            sql.Append($"	    T1.類区分	");
            sql.Append($"	    , M1.類短縮名称 	");
            sql.Append($"	FROM	");
            sql.Append($"	    m_10170_選択引受方式 T1 	");
            sql.Append($"	    INNER JOIN m_00020_類名称 M1 	");
            sql.Append($"	        ON T1.共済目的コード = M1.共済目的コード 	");
            sql.Append($"	        AND T1.類区分 = M1.類区分 	");
            sql.Append($"	WHERE	");
            sql.Append($"	    T1.類区分 <> '0' 	");
            sql.Append($"	    AND M1.加入区分インデックス以外フラグ = '1' 	");
            sql.Append($"	    AND T1.組合等コード = @組合等コード 	");
            sql.Append($"	    AND T1.共済目的コード = @共済目的コード 	");
            sql.Append($"	ORDER BY	");
            sql.Append($"	    T1.類区分	");

            List<NpgsqlParameter> queryParams = new()
            {
                new("組合等コード", kumiaitoCd),
                new("共済目的コード", kyosaiMokutekiCd)
            };

            RuiKbnLists = dbContext.Database.SqlQueryRaw<RuiKbnListModel>(sql.ToString(), queryParams.ToArray())
                .Select(x => new SelectListItem($"{x.類区分} {x.類短縮名称}", $"{x.類区分}"))
                .ToList();
        }

        /// <summary>
        /// 引受方式ドロップダウンリストの取得
        /// </summary>
        /// <param name="dbContext">DBコンテキスト</param>
        /// <param name="kumiaitoCd">組合等コード</param>
        /// <param name="kyosaiMokutekiCd">共済目的コード</param>
        public void GetHikiukeHoushikiLists(NskAppContext dbContext, string kumiaitoCd, string kyosaiMokutekiCd, string ruiKbn)
        {
            // (1) m_10170_選択引受方式テーブルから引受区分リストを取得する。
            HikiukeHoushikiLists = dbContext.M10170選択引受方式s
                .Where(x =>
                    (x.引受方式 != "6") &&
                    (x.引受方式 != "1") &&
                    (x.組合等コード == kumiaitoCd) &&
                    (x.共済目的コード == kyosaiMokutekiCd) &&
                    (x.類区分 == ruiKbn))
                .Select(x => new { x.引受方式, x.引受方式名称 })
                .Distinct()
                .OrderBy(x => x.引受方式)
                .Select(x => new SelectListItem($"{x.引受方式} {x.引受方式名称}", $"{x.引受方式}"))
                .ToList();
        }

        /// <summary>
        /// 補填割合ドロップダウンリストの取得
        /// </summary>
        /// <param name="dbContext">DBコンテキスト</param>
        /// <param name="kumiaitoCd">組合等コード</param>
        /// <param name="kyosaiMokutekiCd">共済目的コード</param>
        public void GetHoshouWariaiLists(NskAppContext dbContext, string kumiaitoCd, string kyosaiMokutekiCd, string ruiKbn, string hikiukeHoushiki)
        {
            // (1) m_10170_選択引受方式テーブルから補償割合リストを取得する。
            HoshouWariaiLists = dbContext.M10170選択引受方式s
                .Where(x =>
                    (x.補償割合コード != "0") &&
                    (x.組合等コード == kumiaitoCd) &&
                    (x.共済目的コード == kyosaiMokutekiCd) &&
                    (x.類区分 == ruiKbn) &&
                    (x.引受方式 == hikiukeHoushiki))
                .Select(x => new { x.補償割合コード, x.補償割合名称 })
                .Distinct()
                .OrderByDescending(x => x.補償割合コード)
                .Select(x => new SelectListItem($"{x.補償割合コード} {x.補償割合名称}", $"{x.補償割合コード}"))
                .ToList();
        }

        /// <summary>
        /// 階層区分ドロップダウンリストの取得
        /// </summary>
        /// <param name="dbContext">DBコンテキスト</param>
        /// <param name="kumiaitoCd">組合等コード</param>
        /// <param name="kyosaiMokutekiCd">共済目的コード</param>
        /// <param name="nensan">年産</param>
        public void GetKaisoKbnLists(NskAppContext dbContext, string kumiaitoCd, string kyosaiMokutekiCd, int nensan)
        {
            // (1) m_20120_階層区分テーブルから階層区分リストを取得する。
            KaisoKbnLists = dbContext.M20120階層区分s
                .Where(x =>
                    (x.組合等コード == kumiaitoCd) &&
                    (x.共済目的コード == kyosaiMokutekiCd) &&
                    (x.年産 == nensan))
                .Select(x => new { x.階層区分, x.階層区分名 })
                .Distinct()
                .Select(x => new SelectListItem($"{x.階層区分} {x.階層区分名}", $"{x.階層区分}"))
                .ToList();
        }
        #endregion
    }
}
