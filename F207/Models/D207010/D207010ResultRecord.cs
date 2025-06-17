using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NskAppModelLibrary.Context;
using NskWeb.Common.Models;

namespace NskWeb.Areas.F207.Models.D207010
{
    /// <summary>
    /// 検索結果レコード
    /// </summary>
    public class D207010ResultRecord : BasePagerRecord
    {
        /// <summary>
        /// 合併時識別コード
        /// </summary>
        public string GappeijiShikibetuCd { get; set; } = string.Empty;
        /// <summary>
        /// 類区分
        /// </summary>
        public string RuiKbn { get; set; } = string.Empty;
        /// <summary>
        /// 引受方式
        /// </summary>
        public string HikiukeHoshiki { get; set; } = string.Empty;
        /// <summary>
        /// 補償割合コード
        /// </summary>
        public string HoshoWariaiCd { get; set; } = string.Empty;
        /// <summary>
        /// 評価地区コード
        /// </summary>
        public string HyokaChikuCd { get; set; } = string.Empty;
        /// <summary>
        /// 階層区分
        /// </summary>
        public string KaisoKbn { get; set; } = string.Empty;
        /// <summary>
        /// 評価地区名
        /// </summary>
        public string HyokaChikuNm { get; set; } = string.Empty;
        /// <summary>
        /// 悉皆調査面積
        /// </summary>
        public decimal ShikkaiChosaMensaki { get; set; }
        /// <summary>
        /// 平均単収差
        /// </summary>
        public int HeikinTanshusa { get; set; }
        /// <summary>
        /// 平均単収差の左加重値
        /// </summary>
        public decimal HeikinTanshusaHidariKajuchi { get; set; }
        /// <summary>
        /// 単当修正量
        /// </summary>
        public int TantoShuseiryo { get; set; }


        public int ShuseiNashiKbnInt { get; set; }

        [NotMapped]
        public bool ShuseiNashiKbn { get; set; }

        /// <summary>
        /// 単当修正量の左加重値
        /// </summary>
        public decimal TantoShuseiryoHidariKajuchi { get; set; }
        /// <summary>
        /// Xmin
        /// </summary>
        public uint? Xmin { get; set; }

        public int Update(ref NskAppContext dbContext, D207010SessionInfo sessionInfo,
            string userId, DateTime? sysDateTime, D207010ResultRecord UpdRecords)
        {
            StringBuilder sql = new();
            sql.Append($"	UPDATE t_23120_単当修正量 SET	");
            sql.Append($"	    組合等コード = @セッション_組合等コード	");
            sql.Append($"	    , 年産 = @セッション_年産	");
            sql.Append($"	    , 共済目的コード = @セッション_共済目的コード	");
            sql.Append($"	    , 合併時識別コード = @画面_隠し項目_合併時識別コード	");
            sql.Append($"	    , 類区分 = @画面_隠し項目_類区分	");
            sql.Append($"	    , 引受方式 = @画面_隠し項目_引受方式	");
            sql.Append($"	    , 補償割合コード = @画面_隠し項目_補償割合コード	");
            sql.Append($"	    , 評価地区コード = @画面_隠し項目_評価地区コード	");
            sql.Append($"	    , 階層区分 = @画面_隠し項目_階層区分	");
            sql.Append($"	    , 悉皆調査面積 = @画面_悉皆調査面積	");
            sql.Append($"	    , 平均単収差 = @画面_平均単収差	");
            sql.Append($"	    , 単当修正量 = @画面_単当修正量	");
            sql.Append($"	    , 修正無しフラグ = @画面_修正無し区分	");
            sql.Append($"	    , 更新日時 = @共通部品_システム日時	");
            sql.Append($"	    , 更新ユーザid = @セッション_ユーザID	");
            sql.Append($"	WHERE	");
            sql.Append($"	    組合等コード = @セッション_組合等コード 	");
            sql.Append($"	    AND 年産 = @セッション_年産 	");
            sql.Append($"	    AND 共済目的コード = @セッション_共済目的コード 	");
            sql.Append($"	    AND 合併時識別コード = @画面_隠し項目_合併時識別コード 	");
            sql.Append($"	    AND 類区分 = @画面_隠し項目_類区分 	");
            sql.Append($"	    AND 引受方式 = @画面_隠し項目_引受方式 	");
            sql.Append($"	    AND 補償割合コード = @画面_隠し項目_補償割合コード 	");
            sql.Append($"	    AND 評価地区コード = @画面_隠し項目_評価地区コード 	");
            sql.Append($"	    AND 階層区分 = @画面_隠し項目_階層区分 	");
            sql.Append($"	    AND xmin = @xmin	");

            List<NpgsqlParameter> queryParams = new()
            {
                new("セッション_組合等コード", sessionInfo.KumiaitoCd),
                new("セッション_年産", sessionInfo.Nensan),
                new("セッション_共済目的コード", sessionInfo.KyosaiMokutekiCd),
                new("画面_隠し項目_合併時識別コード", GappeijiShikibetuCd),
                new("画面_隠し項目_類区分", RuiKbn),
                new("画面_隠し項目_引受方式", HikiukeHoshiki),
                new("画面_隠し項目_補償割合コード", HoshoWariaiCd),
                new("画面_隠し項目_評価地区コード", HyokaChikuCd),
                new("画面_隠し項目_階層区分", KaisoKbn),
                new("画面_悉皆調査面積", ShikkaiChosaMensaki),
                new("画面_平均単収差", HeikinTanshusa),
                new("画面_単当修正量", TantoShuseiryo),
                new("画面_修正無し区分", ShuseiNashiKbnInt),
                new("共通部品_システム日時", sysDateTime),
                new("セッション_ユーザID", userId),
            };

            NpgsqlParameter xminParam = new("xmin", NpgsqlTypes.NpgsqlDbType.Xid) { Value = UpdRecords.Xmin };
            queryParams.Add(xminParam);

            int cnt = dbContext.Database.ExecuteSqlRaw(sql.ToString(), queryParams.ToArray());
            if (cnt == 0)
            {
                throw new DBConcurrencyException();
            }
            return cnt;
        }

        /// <summary>
        /// 画面入力値をこのこのクラスに反映する
        /// </summary>
        /// <param name="src"></param>
        public void ApplyInput(D207010SearchResult src, int index)
        {
            this.ShikkaiChosaMensaki = src.DispRecords[index].ShikkaiChosaMensaki;
            this.HeikinTanshusa = src.DispRecords[index].HeikinTanshusa;
            this.TantoShuseiryo = src.DispRecords[index].TantoShuseiryo;
            if (src.DispRecords[index].ShuseiNashiKbn)
            {
                this.ShuseiNashiKbnInt = 1;
            }
            else
            {
                this.ShuseiNashiKbnInt = 0;
            }
            
        }


    }
}
