using CoreLibrary.Core.Dto;
using CoreLibrary.Core.Validator;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using NskAppModelLibrary.Context;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Microsoft.AspNetCore.Mvc;
using NskAppModelLibrary.Models;
using NskCommonLibrary.Core.Consts;

namespace NskWeb.Areas.F207.Models.D207020
{
    /// <summary>
    /// 当初評価高計算処理（半相殺）画面項目モデル（検索条件部分）
    /// </summary>
    public class D207020SearchCondition
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public D207020SearchCondition()
        {
            HonshoshishoList = new List<SelectListItem>();
        }


        #region "支所区分ドロップダウンリストプロパティ"
        /// <summary>
        /// 支所区分ドロップダウンリストプロパティ
        /// </summary>
        public class HonshoshishoModel()
        {
            /// <summary>
            /// 支所コード
            /// </summary>
            public string 支所コード { get; set; }
            /// <summary>
            /// 支所名
            /// </summary>
            public string 支所名 { get; set; }
        }
        #endregion

        #region "実績履歴リストプロパティ"
        /// <summary>
        /// 実績履歴リストプロパティ
        /// </summary>
        public class JissekiList()
        {
            /// <summary>
            /// 実行日
            /// </summary>
            public string 実行日 { get; set; }

            /// <summary>
            /// 支所コード
            /// </summary>
            public string 支所コード { get; set; }
            /// <summary>
            /// 支所名
            /// </summary>
            public string 支所名 { get; set; }
        }
        #endregion


        #region "農単抜取調査プロパティ"
        public class NoutanNukiModel()
        {
            /// <summary>
            /// 引受方式コード
            /// </summary>
            public string 引受方式コード { get; set; }
            /// <summary>
            /// 支所名
            /// </summary>
            public string 支所名 { get; set; }
        }
        #endregion



        #region "「支所」を取得する。"
        /// <summary>
        /// 「支所」を取得する。
        /// </summary>
        /// <param name="dbContext">DBコンテキスト</param>
        /// <param name="sessionInfo">セッション情報</param>
        public void GetShishoLists(NskAppContext dbContext, D207020.D207020SessionInfo sessionInfo)
        {

            StringBuilder sql = new();
            var param = new List<NpgsqlParameter>();
            sql.Append($"	SELECT 	");
            sql.Append($"	    shisho_cd as 支所コード,	");
            sql.Append($"	    shisho_nm as 支所名 	");
            sql.Append($"	FROM	");
            sql.Append($"	    nouho_nsk_03.v_shisho_nm 	");
            sql.Append($"	WHERE	");
            sql.Append($"	    todofuken_cd = @TODOFUKENCD 	");
            sql.Append($"	    AND kumiaito_cd = @KUMIAITOCD 	");

            if (sessionInfo.ShishoCd == "00" && sessionInfo.ShishoJikkoHikiukeKbn == "1")
            {
                sql.Append($"AND shisho_cd = '00' ");
            }
            else if (sessionInfo.ShishoCd != "00")//支所の時(本所以外の時)
            {
                if (sessionInfo.RiyoKanouSisyoItiran.Count > 0)
                {

                    for (int i = 0; i < sessionInfo.RiyoKanouSisyoItiran.Count - 1; i++)
                    {
                        sql.Append($"AND shisho_cd = ANY(@SHISHOLIST ) ");
                        param.Add(new NpgsqlParameter("@SHISHOLIST", NpgsqlDbType.Array | NpgsqlDbType.Varchar)
                        {
                            Value = sessionInfo.RiyoKanouSisyoItiran.Select(i => i.ShishoCd).ToList()
                        });
                    }

                }
                else if(!String.IsNullOrEmpty(sessionInfo.ShishoCd) )
                {
                    sql.Append($"AND shisho_cd = @SHISHOCD  ");
                }
                ;
            }

            sql.Append($" ORDER BY shisho_cd ");

            // [セッション：都道府県コード]
            param.Add(new NpgsqlParameter("@TODOFUKENCD", sessionInfo.TodofukenCd));
            // [セッション：組合等]
            param.Add(new NpgsqlParameter("@KUMIAITOCD", sessionInfo.KumiaitoCd));
            //[セッション：支所コード]
            param.Add(new NpgsqlParameter("@SHISHOCD", sessionInfo.ShishoCd));
            //


            HonshoshishoList = dbContext.Database.SqlQueryRaw<HonshoshishoModel>(sql.ToString(), param.ToArray())
                .Select(x => new SelectListItem($"{x.支所コード} {x.支所名} ", $"{x.支所コード}"))
                .ToList();
        }
        #endregion


        #region "実行履歴情報を取得"
        /// <summary>
        /// 実行履歴情報を取得
        /// </summary>
        /// <param name="dbContext">DBコンテキスト</param>
        /// <param name="sessionInfo">セッション情報</param>
        public List<D207020TableRecord> GetJikkoRirekiInfo(NskAppContext dbContext, D207020.D207020SessionInfo sessionInfo)
        {

            StringBuilder sql = new();
            var queryParams = new List<NpgsqlParameter>();
            sql.Append($"	SELECT 	");
            sql.Append($"   M1.shisho_cd As \"{nameof(D207020TableRecord.ShishoCd)}\", ");
            sql.Append($"   M1.shisho_nm As \"{nameof(D207020TableRecord.ShishoNm)}\" , ");
            sql.Append($"   CASE WHEN T1.当初計算日付 IS NULL THEN '未実行' ");
            sql.Append($"   ELSE TO_CHAR(T1.当初計算日付,'yyyy/mm/dd') END As \"{nameof(D207020TableRecord.Jikkobi)}\",   ");
            sql.Append($"   T1.引受方式 As \"{nameof(D207020TableRecord.HikiukeHoushiki)}\" ");
            sql.Append($"	FROM nouho_nsk_03.v_shisho_nm M1      ");
            sql.Append($"LEFT OUTER JOIN (  ");
            sql.Append($"        SELECT ");
            sql.Append($"            組合等コード ");
            sql.Append($"            , 共済目的コード ");
            sql.Append($"            , 年産 ");
            sql.Append($"            , 支所コード ");
            sql.Append($"            , MAX(当初計算日付) AS 当初計算日付 ");
            sql.Append($"            , 引受方式  ");
            sql.Append($"        FROM ");
            sql.Append($"            nouho_nsk_03.t_24010_組合員等別損害情報 ");
            sql.Append($"            WHERE ");
            sql.Append($"    組合等コード = @KUMIAITOCD  ");
            sql.Append($"    AND 年産 = @NENSAN  ");
            sql.Append($"    AND 共済目的コード =@KYOSAIMOKUTEKICD  ");
            sql.Append($"        GROUP BY ");
            sql.Append($"            組合等コード ");
            sql.Append($"            , 共済目的コード ");
            sql.Append($"            , 年産 ");
            sql.Append($"            , 支所コード ");
            sql.Append($"            , 引受方式 ");
            sql.Append($"    ) T1 ON ");
            sql.Append($"    M1.kumiaito_cd = T1.組合等コード  ");
            sql.Append($"    AND M1.shisho_cd = T1.支所コード ");
            sql.Append($"    WHERE  ");
            sql.Append($"    todofuken_cd  = @TODOFUKENCD	");
            sql.Append($"    AND kumiaito_cd  = @KUMIAITOCD ");
            //本所の時
            if (sessionInfo.ShishoCd == "00" && sessionInfo.ShishoJikkoHikiukeKbn == "1")
            {
                sql.Append($"AND T1.支所コード = '00' ");
            }
            else if (sessionInfo.ShishoCd == "00" && (sessionInfo.ShishoJikkoHikiukeKbn == "2" || sessionInfo.ShishoJikkoHikiukeKbn == "3"))
            {
                sql.Append($"  AND  M1.shisho_cd IN (  ");
                sql.Append($"           SELECT   ");
                sql.Append($"             shisho_cd  ");
                sql.Append($"           FROM  ");
                sql.Append($"      nouho_nsk_03.v_shisho_nm  ");
                sql.Append($"     WHERE	");
                sql.Append($"        todofuken_cd  = @TODOFUKENCD	");
                sql.Append($"      AND kumiaito_cd  = @KUMIAITOCD ");
                sql.Append($"      AND shisho_cd  <> '00'	");
                sql.Append($"     )	");
            }
            else if (sessionInfo.ShishoCd != "00") //支所の時(本所以外の時)
            {
                //if (sessionInfo.RiyoKanouSisyoItiran.Count > 0)
                //{

                //    for (int i = 0; i < sessionInfo.RiyoKanouSisyoItiran.Count - 1; i++)
                //    {
                //        sql.Append($"AND shisho_cd = ANY(@SHISHOLIST ) ");
                //        param.Add(new NpgsqlParameter("@SHISHOLIST", NpgsqlDbType.Array | NpgsqlDbType.Varchar)
                //        {
                //            Value = sessionInfo.RiyoKanouSisyoItiran.Select(i => i.ShishoCd).ToList()
                //        });
                //    }

                //}
                //else if (!String.IsNullOrEmpty(sessionInfo.ShishoCd))
                //{
                    sql.Append($"AND shisho_cd = @SHISHOCD  ");
                //}
                ;
            }
            sql.Append($" ORDER BY T1.支所コード ");
            // セッション[都道府県コード]
            queryParams.Add(new NpgsqlParameter("TODOFUKENCD", sessionInfo.TodofukenCd));
            // セッション[組合等コード]
            queryParams.Add(new NpgsqlParameter("KUMIAITOCD", sessionInfo.KumiaitoCd));
            //[セッション：支所コード]
            queryParams.Add(new NpgsqlParameter("SHISHOCD", sessionInfo.ShishoCd));
            //セッション[共済目的コード]
            queryParams.Add(new NpgsqlParameter("KYOSAIMOKUTEKICD", sessionInfo.KyosaiMokutekiCd));
            //セッション[年産]
            queryParams.Add(new NpgsqlParameter("NENSAN", sessionInfo.Nensan));
            List<D207020TableRecord> records = new();
            records.AddRange(dbContext.Database.SqlQueryRaw<D207020TableRecord>(sql.ToString(), queryParams.ToArray()));
            return records;

        }
        #endregion

        #region "選択した支所の実行履歴情報を取得"
        /// <summary>
        /// 選択した支所の実行履歴情報を取得
        /// </summary>
        /// <param name="dbContext">DBコンテキスト</param>
        /// <param name="sessionInfo">セッション情報</param>
        public List<D207020TableRecord> SelectSishoRireki(NskAppContext dbContext, D207020.D207020SessionInfo sessionInfo, string shishocd)
        {

            StringBuilder sql = new();
            sql.Append($"	SELECT 	");
            sql.Append($"   M1.shisho_cd As \"{nameof(D207020TableRecord.ShishoCd)}\", ");
            sql.Append($"   M1.shisho_nm As \"{nameof(D207020TableRecord.ShishoNm)}\" , ");
            sql.Append($"   CASE WHEN T1.当初計算日付 IS NULL THEN '未実行' ");
            sql.Append($"   ELSE TO_CHAR(T1.当初計算日付,'yyyy/mm/dd') END As \"{nameof(D207020TableRecord.Jikkobi)}\",   ");
            sql.Append($"   T1.引受方式 As \"{nameof(D207020TableRecord.HikiukeHoushiki)}\" ");
            sql.Append($"   FROM	");
            //sql.Append($"	    nouho_nsk_03.t_24010_組合員等別損害情報 T1 ");
            //sql.Append($" INNER JOIN nouho_nsk_03.v_shisho_nm M1 ON ");
            sql.Append($"	nouho_nsk_03.v_shisho_nm M1      ");
            sql.Append($"LEFT OUTER JOIN (  ");
            sql.Append($"        SELECT ");
            sql.Append($"            組合等コード ");
            sql.Append($"            , 共済目的コード ");
            sql.Append($"            , 年産 ");
            sql.Append($"            , 支所コード ");
            sql.Append($"            , MAX(当初計算日付) AS 当初計算日付 ");
            sql.Append($"            , 引受方式  ");
            sql.Append($"        FROM ");
            sql.Append($"            nouho_nsk_03.t_24010_組合員等別損害情報 ");
            sql.Append($"            WHERE ");
            sql.Append($"    組合等コード = @KUMIAITOCD  ");
            sql.Append($"    AND 年産 = @NENSAN  ");
            sql.Append($"    AND 共済目的コード =@KYOSAIMOKUTEKICD  ");
            sql.Append($"        GROUP BY ");
            sql.Append($"            組合等コード ");
            sql.Append($"            , 共済目的コード ");
            sql.Append($"            , 年産 ");
            sql.Append($"            , 支所コード ");
            sql.Append($"            , 引受方式 ");
            sql.Append($"    ) T1 ON ");
            sql.Append($"    M1.kumiaito_cd = T1.組合等コード  ");
            sql.Append($"    AND M1.shisho_cd = T1.支所コード ");
            sql.Append($"    WHERE  ");
            sql.Append($"    todofuken_cd  = @TODOFUKENCD	");
            sql.Append($"    AND kumiaito_cd  = @KUMIAITOCD ");


            if (shishocd == "00" && sessionInfo.ShishoJikkoHikiukeKbn == "1")
            {
                sql.Append($"AND M1.shisho_cd = '00' ");
            }
            else if (shishocd == "00" && (sessionInfo.ShishoJikkoHikiukeKbn == "2" || sessionInfo.ShishoJikkoHikiukeKbn == "3"))
            {
                sql.Append($"  AND  M1.shisho_cd  IN (  ");
             sql.Append($"           SELECT   ");
                sql.Append($"             shisho_cd  ");
                sql.Append($"           FROM  ");
                sql.Append($"      nouho_nsk_03.v_shisho_nm  ");
                sql.Append($"     WHERE	");
                sql.Append($"        todofuken_cd  = @TODOFUKENCD	");
                sql.Append($"      AND kumiaito_cd  = @KUMIAITOCD ");
                sql.Append($"      AND shisho_cd  <> '00'	");
                sql.Append($"     )	");
            }
            else if (shishocd != "00")//支所の時(本所以外の時)
            {
                sql.Append($"AND M1.shisho_Cd = @SHISHOCD ");

            }
            sql.Append($" ORDER BY T1.支所コード ");

            List<NpgsqlParameter> param = new()
            {
                new("TODOFUKENCD", sessionInfo.TodofukenCd),
                new("KUMIAITOCD", sessionInfo.KumiaitoCd),
                new("SHISHOCD", shishocd),
                new("KYOSAIMOKUTEKICD", sessionInfo.KyosaiMokutekiCd),
                new("NENSAN", sessionInfo.Nensan)
            };
            List<D207020TableRecord> records = new();
            records.AddRange(dbContext.Database.SqlQueryRaw<D207020TableRecord>(sql.ToString(), param.ToArray()));
            return records;
        }
        #endregion


        #region "「農単申告抜取調査データ」を取得する。"
        /// <summary>
        /// 「農単申告抜取調査データ」を取得する。
        /// </summary>
        /// <param name="dbContext">DBコンテキスト</param>
        /// <param name="sessionInfo">セッション情報</param>
        public int NotanDataCount(NskAppContext dbContext, D207020.D207020SessionInfo sessionInfo)
        {

            int datacount = 0;

            StringBuilder sql = new();
            sql.Append($"SELECT COUNT(*) AS \"Value\" FROM  nouho_nsk_03.t_21080_農単抜取調査 ");
            sql.Append($"     WHERE	");
            sql.Append($"       年産  = @NENSAN	");
            sql.Append($"      AND 組合等コード  = @KUMIAITOCD ");
            sql.Append($"      AND 共済目的コード  = @MOKUTEKICD ");
            sql.Append($"      AND 悉皆反映済フラグ   = '0'	");



            List<NpgsqlParameter> param = new()
            {
                new("KUMIAITOCD", sessionInfo.KumiaitoCd),
                new("MOKUTEKICD", sessionInfo.KyosaiMokutekiCd),
                new("NENSAN", sessionInfo.Nensan)
            };

            datacount = dbContext.Database.SqlQueryRaw<int>(sql.ToString(), param.ToArray()).Single();
            return datacount;
        }
        #endregion


        #region "「農単申告抜取調査データ」を取得する。"
        /// <summary>
        /// 「農単申告抜取調査データ」を取得する。
        /// </summary>
        /// <param name="dbContext">DBコンテキスト</param>
        /// <param name="sessionInfo">セッション情報</param>
        public void GetNotanData(NskAppContext dbContext, D207020.D207020SessionInfo sessionInfo)
        {

            int datacount = 0;

            StringBuilder sql = new();
            sql.Append($"SELECT ");
            sql.Append($"");
            sql.Append($" FROM  nouho_nsk_03.t_21080_農単抜取調査 ");
            sql.Append($"     WHERE	");
            sql.Append($"       年産  = @NENSAN	");
            sql.Append($"      AND 組合等コード  = @KUMIAITOCD ");
            sql.Append($"      AND 共済目的コード  = @MOKUTEKICD ");
            sql.Append($"      AND 悉皆反映済フラグ   = '0'	");



            List<NpgsqlParameter> param = new()
            {
                new("KUMIAITOCD", sessionInfo.KumiaitoCd),
                new("MOKUTEKICD", sessionInfo.KyosaiMokutekiCd),
                new("NENSAN", sessionInfo.Nensan)
            };

            NoutanList = dbContext.Database.SqlQueryRaw<NoutanNukiModel>(sql.ToString(), param.ToArray())
            .Select(x => new SelectListItem($"{x.引受方式コード}", $"{x.引受方式コード}"))
            .ToList();
        }
        #endregion



        #region バッチ条件ID取得
        /// <summary>
        /// イベント名：バッチ条件IDの取得
        /// </summary>
        /// /// <returns>バッチ条件ID</returns>
        protected string GetBatchJoken()
        {
            var BatchJokenId = System.Guid.NewGuid().ToString("D");

            return BatchJokenId;
        }
        #endregion

        #region バッチ条件テーブル登録イベント
        /// <summary>
        /// イベント名：バッチ条件テーブル登録
        /// </summary>
        /// <param name="t01050BatchJoken">バッチ条件テーブルモデル</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void CreatBatchJoken(T01050バッチ条件 t01050BatchJoken, NskAppContext jigyodb)
        {
            // insertの条件を設定
            jigyodb.T01050バッチ条件s.Add(t01050BatchJoken);

            // insertの実行
            jigyodb.SaveChanges();
        }
        #endregion

        #region バッチ条件登録(引受年産)
        /// <summary>
        /// イベント名：バッチ条件登録(引受年産)
        /// </summary>
        /// <param name="Nensan">引受年産</param>
        /// <param name="syokuin">職員情報</param>
        /// <param name="systemDate">システム日時</param>
        /// <param name="i">連番</param>
        /// /// <returns>T01050バッチ条件引受年産</returns>
        public T01050バッチ条件 CreatBatchJokenNensan(string Nensan, Syokuin syokuin, string BatchJokenId, DateTime systemDate, int i)
        {
            T01050バッチ条件 t01050BatchJoken = new()
            {
                // バッチ条件id
                バッチ条件id = BatchJokenId,
                // 連番
                連番 = i,
                // 条件名称
                条件名称 = JoukenNameConst.JOUKEN_NENSAN,
                // 表示用条件値
                表示用条件値 = JoukenNameConst.JOUKEN_NENSAN,
                // 条件値
                条件値 = Nensan,
                // 登録日時
                登録日時 = systemDate,
                // 登録ユーザid
                登録ユーザid = syokuin.UserId,
                // 更新日時
                更新日時 = systemDate,
                // 更新ユーザid
                更新ユーザid = syokuin.UserId,
            };

            return t01050BatchJoken;
        }
        #endregion

        #region バッチ条件登録(共済目的)
        /// <summary>
        /// イベント名：バッチ条件登録(共済目的)
        /// </summary>
        /// <param name="KyosaiMokutekiCd">共済目的コード</param>
        /// <param name="syokuin">職員情報</param>
        /// <param name="systemDate">システム日時</param>
        /// <param name="i">連番</param>
        /// /// <returns>T01050バッチ条件共済目的コード</returns>
        public T01050バッチ条件 CreatBatchJokenKyosaiMokuteki(string KyosaiMokutekiCd, Syokuin syokuin, string BatchJokenId, DateTime systemDate, int i)
        {
            T01050バッチ条件 t01050BatchJoken = new()
            {
                // バッチ条件id
                バッチ条件id = BatchJokenId,
                // 連番
                連番 = i,
                // 条件名称
                条件名称 = JoukenNameConst.JOUKEN_KYOSAI_MOKUTEKI_CD,
                // 表示用条件値
                表示用条件値 = JoukenNameConst.JOUKEN_KYOSAI_MOKUTEKI_CD,
                // 条件値
                条件値 = KyosaiMokutekiCd,
                // 登録日時
                登録日時 = systemDate,
                // 登録ユーザid
                登録ユーザid = syokuin.UserId,
                // 更新日時
                更新日時 = systemDate,
                // 更新ユーザid
                更新ユーザid = syokuin.UserId,
            };

            return t01050BatchJoken;
        }
        #endregion

        #region バッチ条件登録(本所支所コード)
        /// <summary>
        /// イベント名：バッチ条件登録(支所コード)
        /// </summary>
        /// <param name="ShishoCd">画面の支所コード</param>
        /// <param name="syokuin">職員情報</param>
        /// <param name="systemDate">システム日時</param>
        /// <param name="i">連番</param>
        /// /// <returns>T01050バッチ条件本所支所コード</returns>
        public T01050バッチ条件 CreatBatchJokenShishoCd(string ShishoCd, Syokuin syokuin, string BatchJokenId, DateTime systemDate, int i)
        {
            T01050バッチ条件 t01050BatchJoken = new()
            {
                // バッチ条件id
                バッチ条件id = BatchJokenId,
                // 連番
                連番 = i,
                // 条件名称
                条件名称 = JoukenNameConst.JOUKEN_SHISHO,
                // 表示用条件値
                表示用条件値 = JoukenNameConst.JOUKEN_SHISHO,
                // 条件値
                条件値 = ShishoCd,
                // 登録日時
                登録日時 = systemDate,
                // 登録ユーザid
                登録ユーザid = syokuin.UserId,
                // 更新日時
                更新日時 = systemDate,
                // 更新ユーザid
                更新ユーザid = syokuin.UserId,
            };

            return t01050BatchJoken;
        }
        #endregion

        #region バッチ条件登録(引受方式コード)
        /// <summary>
        /// イベント名：バッチ条件登録(引受方式コード)
        /// </summary>
        /// <param name="HikiukeCd">取得した引受方式コード</param>
        /// <param name="syokuin">職員情報</param>
        /// <param name="systemDate">システム日時</param>
        /// <param name="i">連番</param>
        /// /// <returns>T01050バッチ条件本所支所コード</returns>
        public T01050バッチ条件 CreatBatchJokenHikiukeCd(string HikiukeCd, Syokuin syokuin, string BatchJokenId, DateTime systemDate, int i)
        {
            T01050バッチ条件 t01050BatchJoken = new()
            {
                // バッチ条件id
                バッチ条件id = BatchJokenId,
                // 連番
                連番 = i,
                // 条件名称
                条件名称 = JoukenNameConst.JOUKEN_HIKIUKE_HOUSHIKI_CD,
                // 表示用条件値
                表示用条件値 = JoukenNameConst.JOUKEN_HIKIUKE_HOUSHIKI_CD,
                // 条件値
                条件値 = HikiukeCd,
                // 登録日時
                登録日時 = systemDate,
                // 登録ユーザid
                登録ユーザid = syokuin.UserId,
                // 更新日時
                更新日時 = systemDate,
                // 更新ユーザid
                更新ユーザid = syokuin.UserId,
            };

            return t01050BatchJoken;
        }
        #endregion




        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="syokuin">ユーザー情報（セッション）</param>
        /// <param name="shishoList">利用可能な支所一覧（セッション）</param>
        public D207020SearchCondition(Syokuin syokuin, List<Shisho> shishoList)
        {
        }

        /// <summary>
        /// 年産
        /// </summary>
        [Display(Name = "年産")]
        [FullStringLength(4)]
        public string Nensan { get; set; }

        /// <summary>
        /// 共済目的コード
        /// </summary>
        [Display(Name = "共済目的コード")]
        [FullStringLength(2)]
        public string KyosaiMokutekiCd { get; set; }

        /// <summary>
        /// 共済目的名称
        /// </summary>
        [Display(Name = "共済目的名称")]
        [WithinStringLength(20)]
        public string KyosaiMokutekiNm { get; set; }

        /// <summary>
        /// メッセージエリア1
        /// </summary>
        [Display(Name = "メッセージエリア1")]
        public string MessageArea1 { get; set; }

        /// <summary>
        /// 組合等コード
        /// </summary>
        [Display(Name = "組合等コード")]
        public string KumiaitoCd { get; set; }

        /// <summary>
        /// 引受計算支所実行単位区分_引受
        /// </summary>
        [Display(Name = "引受計算支所実行単位区分_引受")]
        public string ShishoJikkoHikiukeKbn { get; set; }

        /// <summary>
        /// 本所・支所リスト
        /// </summary>
        public List<SelectListItem> HonshoshishoList { get; set; }
        
        /// <summary>
        /// 選択支所コード
        /// </summary>
        [Display(Name = "選択支所コード")]
        [Required]
        public string SelectShishoCd { get; set; }

        /// <summary>
        /// 選択支所名称
        /// </summary>
        [Display(Name = "選択支所名称")]
        public string SelectShishoNm { get; set; }


        /// <summary>
        /// 農単申告抜取調査データリスト
        /// </summary>        
        public List<SelectListItem> NoutanList { get; set; }


    }
}
