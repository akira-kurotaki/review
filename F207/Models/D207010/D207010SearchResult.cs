using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NskAppModelLibrary.Context;
using NskCommonLibrary.Core.Consts;
using NskWeb.Common.Models;

namespace NskWeb.Areas.F207.Models.D207010
{
    /// <summary>
    /// 検索結果
    /// </summary>
    public class D207010SearchResult : BasePager<D207010ResultRecord>
    {
        #region "検索条件"
        /// <summary>
        /// 検索条件
        /// </summary>
        public D207010SearchCondition SearchCondition { get; set; } = new();
        #endregion

        #region "コンストラクタ"
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public D207010SearchResult() { }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="searchCondition">検索条件</param>
        public D207010SearchResult(D207010SearchCondition searchCondition)
        {
            SearchCondition = searchCondition;
        }
        #endregion

        /// <summary>
        /// 検索結果の取得
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public override List<D207010ResultRecord> GetResult(NskAppContext dbContext, BaseSessionInfo session)
        {

            StringBuilder sql = new();
            sql.Append($"	SELECT	");
            sql.Append($"	    T1.合併時識別コード	AS {nameof(D207010ResultRecord.GappeijiShikibetuCd)} ");
            sql.Append($"	    , T1.類区分	AS {nameof(D207010ResultRecord.RuiKbn)} ");
            sql.Append($"	    , T1.引受方式 AS {nameof(D207010ResultRecord.HikiukeHoshiki)} ");
            sql.Append($"	    , T1.補償割合コード	AS {nameof(D207010ResultRecord.HoshoWariaiCd)} ");
            sql.Append($"	    , T1.評価地区コード	AS {nameof(D207010ResultRecord.HyokaChikuCd)} ");
            sql.Append($"	    , T1.階層区分 AS {nameof(D207010ResultRecord.KaisoKbn)}");
            sql.Append($"	    , M1.評価地区名	AS {nameof(D207010ResultRecord.HyokaChikuNm)} ");
            sql.Append($"	    , T1.悉皆調査面積	AS {nameof(D207010ResultRecord.ShikkaiChosaMensaki)} ");
            sql.Append($"	    , T1.平均単収差	AS {nameof(D207010ResultRecord.HeikinTanshusa)} ");
            sql.Append($"	    , (COALESCE(T1.悉皆調査面積, 0) * COALESCE(T1.平均単収差, 0)) AS {nameof(D207010ResultRecord.HeikinTanshusaHidariKajuchi)}	");
            sql.Append($"	    , T1.単当修正量	AS {nameof(D207010ResultRecord.TantoShuseiryo)} ");
            sql.Append($"	    , T1.修正無しフラグ	AS {nameof(D207010ResultRecord.ShuseiNashiKbnInt)} ");
            sql.Append($"	    , (COALESCE(T1.悉皆調査面積, 0) * COALESCE(T1.単当修正量, 0)) AS {nameof(D207010ResultRecord.TantoShuseiryoHidariKajuchi)}	");
            sql.Append($"	    , cast('' || T1.xmin as integer) AS {nameof(D207010ResultRecord.Xmin)} ");
            sql.Append($"	FROM	");
            sql.Append($"	    t_23120_単当修正量 T1 	");
            sql.Append($"	    LEFT OUTER JOIN m_20130_評価地区 M1 	");
            sql.Append($"	        ON T1.組合等コード = M1.組合等コード 	");
            sql.Append($"	        AND T1.年産 = M1.年産 	");
            sql.Append($"	        AND T1.共済目的コード = M1.共済目的コード 	");
            sql.Append($"	        AND T1.評価地区コード = M1.評価地区コード 	");
            sql.Append($"	WHERE	");
            sql.Append($"	    T1.組合等コード = @セッション_組合等コード 	");
            sql.Append($"	    AND T1.年産 = @セッション_年産 	");
            sql.Append($"	    AND T1.共済目的コード = @セッション_共済目的コード 	");

            D207010SessionInfo sessionInfo = (D207010SessionInfo)session;
            List<NpgsqlParameter> queryParams = new()
            {
                new("セッション_組合等コード", sessionInfo.KumiaitoCd),
                new("セッション_年産", sessionInfo.Nensan),
                new("セッション_共済目的コード", sessionInfo.KyosaiMokutekiCd)
            };

            if (!string.IsNullOrEmpty(SearchCondition.RuiKbn))
            {
                sql.Append($"	    AND T1.類区分 = @画面_類区分	");
                queryParams.Add(new("画面_類区分", SearchCondition.RuiKbn));
            }
            if (!string.IsNullOrEmpty(SearchCondition.HikiukeHoushiki))
            {
                sql.Append($"	    AND T1.引受方式 = @画面_引受方式	");
                queryParams.Add(new("画面_引受方式", SearchCondition.HikiukeHoushiki));
            }
            if (!string.IsNullOrEmpty(SearchCondition.HoshouWariai))
            {
                sql.Append($"	    AND T1.補償割合コード = @画面_補償割合コード	");
                queryParams.Add(new("画面_補償割合コード", SearchCondition.HoshouWariai));
            }
            if (!string.IsNullOrEmpty(SearchCondition.KaisoKbn))
            {
                sql.Append($"	    AND T1.階層区分 = @画面_階層区分	");
                queryParams.Add(new("画面_階層区分", SearchCondition.KaisoKbn));
            }

            if (!(string.IsNullOrEmpty(SearchCondition.DisplaySort1) &&
                string.IsNullOrEmpty(SearchCondition.DisplaySort2) &&
                string.IsNullOrEmpty(SearchCondition.DisplaySort3)))
            {
                sql.Append($"	ORDER BY	");
               if (!string.IsNullOrEmpty(SearchCondition.DisplaySort1))
                {
                    string dispsort1 = SearchCondition.DisplaySort1;

                    if (SearchCondition.DisplaySortOrder1 == CoreConst.SortOrder.DESC)
                    {
                        sql.Append($" {dispsort1} DESC ");
                    }
                    else if (SearchCondition.DisplaySortOrder1 == CoreConst.SortOrder.ASC)
                    {
                        sql.Append($" {dispsort1} ASC ");
                    }
                }

                if (!string.IsNullOrEmpty(SearchCondition.DisplaySort2))
                {
                    if (!string.IsNullOrEmpty(SearchCondition.DisplaySort1))
                    {
                        sql.Append($"	    ,	");
                    }
                    string dispsort2 = SearchCondition.DisplaySort2;
                    sql.Append($"	   {dispsort2} ");
                    //param.Add(new("画面_表示順キー２", $"T1.{SearchCondition.DisplaySort2}"));

                    if (SearchCondition.DisplaySortOrder2 == CoreConst.SortOrder.DESC)
                    {
                        sql.Append($"	    DESC ");
                    }
                    else if (SearchCondition.DisplaySortOrder2 == CoreConst.SortOrder.ASC)
                    {
                        sql.Append($"	    ASC ");
                    }
                }

                if (!string.IsNullOrEmpty(SearchCondition.DisplaySort3))
                {
                    if (!(string.IsNullOrEmpty(SearchCondition.DisplaySort1) && string.IsNullOrEmpty(SearchCondition.DisplaySort2)))
                    {
                        sql.Append($"	    ,	");
                    }
                    string dispsort3 = SearchCondition.DisplaySort3;
                    sql.Append($"	   {dispsort3} ");
                    //param.Add(new("画面_表示順キー３", $"T1.{SearchCondition.DisplaySort3}"));

                    if (SearchCondition.DisplaySortOrder3 == CoreConst.SortOrder.DESC)
                    {
                        sql.Append($"	    DESC ");
                    }
                    else if (SearchCondition.DisplaySortOrder3 == CoreConst.SortOrder.ASC)
                    {
                        sql.Append($"	    ASC ");
                    }
                }
            }

            List<D207010ResultRecord> records = dbContext.Database.SqlQueryRaw<D207010ResultRecord>(sql.ToString(), queryParams.ToArray()).ToList();
            return records;
        }

        /// <summary>
        /// 更新対象レコード取得
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="sessionInfo"></param>
        /// <returns></returns>
        public override List<D207010ResultRecord> GetUpdateRecs(ref NskAppContext dbContext, BaseSessionInfo sessionInfo)
        {
            return [];
        }
    }
}
