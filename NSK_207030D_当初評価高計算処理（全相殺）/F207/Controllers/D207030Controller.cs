using CoreLibrary.Core.Attributes;
using CoreLibrary.Core.Base;
using CoreLibrary.Core.Dto;
using CoreLibrary.Core.Exceptions;
using CoreLibrary.Core.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.CodeAnalysis;
using ModelLibrary.Models;
using NskAppModelLibrary.Context;
using NskAppModelLibrary.Models;
using NskCommonLibrary.Core.Consts;
using NskWeb.Areas.F000.Models.D000000;
using NskWeb.Areas.F000.Models.D000999;
using NskWeb.Areas.F205.Consts;
using NskWeb.Areas.F207.Consts;
using NskWeb.Areas.F207.Models.D207030;
using NskWeb.Common.Consts;
using Pipelines.Sockets.Unofficial.Arenas;
using NskConsts = NskCommonLibrary.Core.Consts;

namespace NskWeb.Areas.F207.Controllers
{
    /// <summary>
    /// NSK_207030D_当初評価高計算処理（全相殺）
    /// </summary>
    /// <remarks>
    /// 作成日：2025/04/23
    /// 作成者：ネクスト松嶋
    /// </remarks>
    [ExcludeAuthCheck]
    [AllowAnonymous]
    [Area("F207")]
    public class D207030Controller : CoreController
    {
        public D207030Controller(ICompositeViewEngine viewEngine, IWebHostEnvironment webHostEnvironment) : base(viewEngine)
        {
        }

        public ActionResult Init()

        {
            // ログインユーザの参照・更新可否判定
            // 画面IDをキーとして、画面マスタ、画面機能権限マスタを参照し、ログインユーザに本画面の権限がない場合は業務エラー画面を表示する。
            if (!ScreenSosaUtil.CanReference(F207Const.SCREEN_ID_D207030, HttpContext))
            {
                throw new AppException("ME90003", MessageUtil.Get("ME90003"));
            }

            var syokuin = SessionUtil.Get<Syokuin>("_D9000_LOGIN_USER", HttpContext);
            if (syokuin == null)
            {
                ModelState.AddModelError("MessageArea", MessageUtil.Get("ME01033"));
                D000999Model d000999Model = GetInitModel();
                d000999Model.UserId = "";
                return View("D000999_Pre", d000999Model);
            }

            // ポータル情報取得
            var portalInfo = SessionUtil.Get<NSKPortalInfoModel>(AppConst.SESS_NSK_PORTAL, HttpContext);
            if (portalInfo == null)
            {
                throw new AppException("ME01645", MessageUtil.Get("ME01645", "NSKポータル情報"));
            }

            // SessionInfo生成（D207030用に定義された情報を集約）
            var sessionInfo = new D207030SessionInfo();
            sessionInfo.GetInfo(HttpContext);

            // DBコンテキスト取得
            var db = getJigyoDb<NskAppContext>();

            // モデル初期化
            var model = new D207030Model
            {
                VSyokuinRecords = db.VSyokuins.Where(t => t.UserId == syokuin.UserId).Single(),
                D207030Info = portalInfo
            };

            // 本所・支所ドロップダウン初期化
            model.InitializeDropdonwList(db, sessionInfo);

            // 政府保険認定区分ドロップダウンの初期化（画面設計に準拠：引受方式の条件は使用しない）
            var seifuList = db.M20160政府保険認定区分s
                .Where(x =>
                    x.組合等コード == sessionInfo.KumiaitoCd &&
                    x.年産 == sessionInfo.Nensan &&
                    x.共済目的コード == sessionInfo.KyosaiMokutekiCd)
                .OrderBy(x => x.政府保険認定区分)
                .Select(x => new SelectListItem
                {
                    Value = x.政府保険認定区分,
                    Text = $"{x.政府保険認定区分} {x.短縮名称}"
                })
                .ToList();

            if (seifuList == null || !seifuList.Any())
            {
                // 失敗時に業務エラー画面
                throw new AppException("MF00001", MessageUtil.Get("MF00001"));
            }

            // 利用可能な支所一覧が取得できなかったらエラー画面
            if (sessionInfo.RiyokanoShishoList == null || !sessionInfo.RiyokanoShishoList.Any())
            {
                throw new AppException("ME01645", MessageUtil.Get("ME01645", "パラメータ"));
            }

            model.SeifuHokenNinteiList = seifuList;
            model.SeifuHokenNinteiCd = seifuList.FirstOrDefault()?.Value ?? string.Empty;

            // 実行履歴
            model.JikkouRirekiList = model.ShishoNm.Select(x =>
            {
                var record = db.T24010組合員等別損害情報s
                    .Where(t => t.組合等コード == sessionInfo.KumiaitoCd &&
                                t.年産 == sessionInfo.Nensan &&
                                t.共済目的コード == sessionInfo.KyosaiMokutekiCd &&
                                t.支所コード == x.ShishoCd)
                    .OrderByDescending(t => t.当初計算日付)  // 当初計算日付の降順で最新を取得
                    .FirstOrDefault();

                var dateStr = record?.当初計算日付 != null
                    ? ToWarekiString(record.当初計算日付.Value)
                    : "未実行";

                return new D207030Model.JikkouRireki
                {
                    ShishoNm = x.ShishoNm,
                    JikkouDate = dateStr
                };
            }).ToList();

            // 権限フラグの設定
            var updateKengen = ScreenSosaUtil.CanUpdate(F207Const.SCREEN_ID_D207030, HttpContext);
            model.UpdateKengenFlg = updateKengen;

            // 共済目的名称の取得
            try
            {
                var kyosai = getJigyoDb<NskAppContext>().M00010共済目的名称s
                              .Where(t => t.共済目的コード == portalInfo.SKyosaiMokutekiCd)
                              .Single();

                model.KyosaiMokutekiMeisho = kyosai.共済目的名称;
            }
            catch (Exception ex)
            {
                // 失敗時に業務エラー画面
                logger.Debug(ex.StackTrace);
                throw new AppException("MF00001", MessageUtil.Get("MF00001"));
            }

            // 初期表示情報をセッションに保存する
            SessionUtil.Set(F207Const.SESS_D207030, model, HttpContext);
            return View(F207Const.SCREEN_ID_D207030, model);
        }

        #region バッチ登録イベント
        /// <summary>
        /// イベント名：バッチ登録
        /// </summary>
        /// <returns>ActionResult</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatBatchReport([Bind("HonshoShishoCd,SeifuHokenNinteiCd")] D207030Model form, string ShishoCdCsv)
        {
            logger.Debug("START CreatBatchReport");
            var selectedShisho = form.HonshoShishoCd;
            var model = SessionUtil.Get<D207030Model>(F207Const.SESS_D207030, HttpContext);
            logger.Debug(model == null);
            DbConnectionInfo dbConnectionInfo =
                DBUtil.GetDbConnectionInfo(ConfigUtil.Get("SystemKbn")
                , Syokuin.TodofukenCd
                , Syokuin.KumiaitoCd
                , Syokuin.ShishoCd);

            // セッションに自画面のデータが存在しない場合
            if (model == null)
            {
                return Json(new { success = false, message = MessageUtil.Get("ME01645", "セッション情報の取得") });
            }
            // 画面から支所が取得できなかった場合
            if (string.IsNullOrEmpty(selectedShisho))
            {
                // ステータス：失敗、エラーメッセージ返却
                return Json(new { success = false, message = MessageUtil.Get("ME00001", "支所","") });
            }

            // NSKポータル情報の取得
            NSKPortalInfoModel md = SessionUtil.Get<NSKPortalInfoModel>(AppConst.SESS_NSK_PORTAL, HttpContext);

            // バッチ予約状況取得引数の設定
            BatchUtil.GetBatchYoyakuListParam param = new()
            {
                SystemKbn = ConfigUtil.Get(CoreConst.APP_ENV_SYSTEM_KBN),
                TodofukenCd = Syokuin.TodofukenCd,
                KumiaitoCd = Syokuin.KumiaitoCd,
                ShishoCd = Syokuin.ShishoCd,
                BatchNm = F207Const.BATCH_ID_NSK_207031B
            };

            // 総件数取得フラグ
            bool boolAllCntFlg = false;
            // 件数（出力パラメータ）
            int intAllCnt = 0;
            // エラーメッセージ（出力パラメータ）
            string message = string.Empty;

            // バッチ予約状況取得（BatchUtil.GetBatchYoyakuList()）を呼び出し、バッチ予約状況を取得する。
            List<BatchYoyaku> batchYoyakuList = BatchUtil.GetBatchYoyakuList(param, boolAllCntFlg, ref intAllCnt, ref message);

            // バッチ予約が存在し、かつ未実行のバッチが含まれている場合、未実行リストを作成する
            if (intAllCnt >= 1 &&
                    batchYoyakuList
                        .Where(b => b.BatchStatus == BatchUtil.BATCH_STATUS_WAITING)
                        .ToList() is var waitingList && waitingList.Any())
            {
                using (var db1 = new NskAppContext(dbConnectionInfo.ConnectionString, dbConnectionInfo.DefaultSchema))
                {
                    var jokenIds = waitingList
                        .Select(w => w.BatchJoken)
                        .Where(id => id != null)
                        .Distinct()
                        .ToList();

                    bool hasMatching = db1.T01050バッチ条件s
                        .Any(t =>
                            jokenIds.Contains(t.バッチ条件id) &&
                            t.条件名称 == JoukenNameConst.JOUKEN_SHISHO &&
                            t.条件値 == ShishoCdCsv &&
                            db1.T01050バッチ条件s.Any(t2 =>
                                t2.バッチ条件id == t.バッチ条件id &&
                                t2.条件名称 == JoukenNameConst.JOUKEN_KYOSAI_MOKUTEKI_CD &&
                                t2.条件値 == md.SKyosaiMokutekiCd) &&
                            db1.T01050バッチ条件s.Any(t3 =>
                                t3.バッチ条件id == t.バッチ条件id &&
                                t3.条件名称 == JoukenNameConst.JOUKEN_NENSAN &&
                                t3.条件値 == md.SNensanHyoka));

                    if (hasMatching)
                    {
                        return Json(new
                        {
                            success = false,
                            message = MessageUtil.Get("ME10019", "当初評価高計算処理（全相殺）")
                        });
                    }
                }
            }

            // ユーザIDの取得
            var userId = Syokuin.UserId;
            // システム日時
            var systemDate = DateUtil.GetSysDateTime();

            // 条件IDを取得する
            string strJoukenId = Guid.NewGuid().ToString();
            string strJoukenErrorMsg = string.Empty;

            // 画面から渡された支所コードCSV（例: "00, 01"）
            var shishoCdCsv = HttpContext.Request.Form["ShishoCdCsv"].ToString();
            logger.Debug($"[★ログ] 受信したShishoCdCsv: '{shishoCdCsv}'");

            // CSV文字列をInsertTJoukenに渡す
            strJoukenErrorMsg = InsertTJouken(dbConnectionInfo, model, strJoukenId, ShishoCdCsv, form.SeifuHokenNinteiCd, md);

            if (!string.IsNullOrEmpty(strJoukenErrorMsg))
            {
                return Json(new { success = false, message = strJoukenErrorMsg });
            }

            // バッチ予約登録
            var refMsg = string.Empty;
            long batchId = 0;
            // バッチ条件（表示用）作成
            var displayJouken = NskConsts.JoukenNameConst.JOUKEN_NENSAN + "、" + NskConsts.JoukenNameConst.JOUKEN_KYOSAI_MOKUTEKI_CD + "、" + NskConsts.JoukenNameConst.JOUKEN_SHISHO + "、"  + "政府保険認定区分"; // TODO: 定数定義があれば差し替え

            // バッチ予約登録
            int? result = null;
            try
            {
                logger.Info("バッチ予約登録処理を実行する。");
                result = BatchUtil.InsertBatchYoyaku(AppConst.BatchBunrui.BATCH_BUNRUI_90_OTHER,
                ConfigUtil.Get(CoreLibrary.Core.Consts.CoreConst.APP_ENV_SYSTEM_KBN),
                Syokuin.TodofukenCd,
                Syokuin.KumiaitoCd,
                Syokuin.ShishoCd,
                DateUtil.GetSysDateTime(),
                Syokuin.UserId,
                F207Const.SCREEN_ID_NSK_D207030,
                F207Const.BATCH_ID_NSK_207031B,
                strJoukenId,
                displayJouken,
                AppConst.FLG_OFF,
                AppConst.BatchType.BATCH_TYPE_PATROL,
                null,
                AppConst.FLG_OFF,
                ref refMsg,
                ref batchId,
                F207Const.SCREEN_ID_NSK_D207030 + Syokuin.TodofukenCd
                );
            }
            catch (Exception e)
            {
                // （出力メッセージ：バッチ予約登録失敗）
                // （出力メッセージ：（メッセージID：ME90008、引数{0}："（"+｢４．２．３．｣で返却されたメッセージ + "）"））
                logger.Error("バッチ予約登録失敗1");
                logger.Error(MessageUtil.Get("ME90008", "（" + refMsg + "）"));
                //logger.Error(MessageUtil.GetErrorMessage(e, CoreConst.LOG_MAX_INNER_EXCEPTION));
                logger.Error(MessageUtil.GetErrorMessage(e, 10));
                return Json(new { success = false, message = MessageUtil.Get("ME90005") });
            }

            // 処理結果がエラーだった場合
            if (result == 0)
            {
                // （出力メッセージ：バッチ予約登録失敗）
                // （出力メッセージ：（メッセージID：ME90008、引数{0}："（"+｢４．２．３．｣で返却されたメッセージ + "）"））
                logger.Error("バッチ予約登録失敗2");
                logger.Error(MessageUtil.Get("ME90008", "（" + refMsg + "）"));
                return Json(new { success = false, message = MessageUtil.Get("ME90005") });
            }


            // （出力メッセージ：バッチ予約登録成功）
            logger.Info("バッチ予約登録成功");
            return Json(new { success = true, message = MessageUtil.Get("MI00004", "当初評価高計算処理（全相殺）バッチの予約") });
        }
        #endregion

        #region Macの濁点や半濁点を変換する
        /// <summary>
        /// Macの濁点や半濁点を変換する
        /// </summary>
        /// <param name="str">変更対象</param>
        /// <returns>返還結果</returns>
        private string ChangeMacDakuten(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            string result = str;
            // NFCかどうかを判定
            if (!result.IsNormalized())
            {
                result = result.Normalize();
            }
            return result;
        }
        #endregion

        #region 条件登録メッソド
        /// <summary>
        /// メッソド:条件登録
        /// </summary>
        /// <param name="model">画面モデル</param>
        /// <param name="joukenId">条件ID</param>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>登録結果</returns>
        private string InsertTJouken(DbConnectionInfo dbConnectionInfo, D207030Model model, string joukenId, string shishoCdCsv, string seifuhokenCd, NSKPortalInfoModel md)
        {
            // ユーザID
            var userId = Syokuin.UserId;
            // システム日時
            var systemDate = DateUtil.GetSysDateTime();

            // 連番を手動で初期化
            int serialNumber = 0;

            // DbContext を一度だけ使用する
            using (var db1 = new NskAppContext(dbConnectionInfo.ConnectionString, dbConnectionInfo.DefaultSchema))
            {
                // トランザクションを開始
                var transaction = db1.Database.BeginTransaction();

                try
                {
                    // 条件情報を追加する共通処理
                    // 条件1：年産
                    var nensan = new T01050バッチ条件
                    {
                        バッチ条件id = joukenId,
                        連番 = ++serialNumber,
                        条件名称 = JoukenNameConst.JOUKEN_NENSAN,
                        表示用条件値 = JoukenNameConst.JOUKEN_NENSAN,
                        条件値 = md.SNensanHyoka,
                        登録日時 = systemDate,
                        登録ユーザid = userId,
                        更新日時 = systemDate,
                        更新ユーザid = userId
                    };
                    db1.T01050バッチ条件s.Add(nensan);

                    // 条件2：共済目的コード
                    var kyosai = new T01050バッチ条件
                    {
                        バッチ条件id = joukenId,
                        連番 = ++serialNumber,
                        条件名称 = JoukenNameConst.JOUKEN_KYOSAI_MOKUTEKI_CD,
                        表示用条件値 = JoukenNameConst.JOUKEN_KYOSAI_MOKUTEKI_CD,
                        条件値 = md.SKyosaiMokutekiCd,
                        登録日時 = systemDate,
                        登録ユーザid = userId,
                        更新日時 = systemDate,
                        更新ユーザid = userId
                    };
                    db1.T01050バッチ条件s.Add(kyosai);

                    // 条件3：支所コード（複数 → カンマ区切りで結合）
                    var shisho = new T01050バッチ条件
                    {
                        バッチ条件id = joukenId,
                        連番 = ++serialNumber,
                        条件名称 = JoukenNameConst.JOUKEN_SHISHO,
                        表示用条件値 = JoukenNameConst.JOUKEN_SHISHO,
                        条件値 = shishoCdCsv,
                        登録日時 = systemDate,
                        登録ユーザid = userId,
                        更新日時 = systemDate,
                        更新ユーザid = userId
                    };
                    db1.T01050バッチ条件s.Add(shisho);

                    // 条件4：政府保険認定区分
                    var seifuhoken = new T01050バッチ条件
                    {
                        バッチ条件id = joukenId,
                        連番 = ++serialNumber,
                        条件名称 = "政府保険認定区分",  // TODO: 定数定義があれば差し替え
                        表示用条件値 = "政府保険認定区分",  // TODO: 定数定義があれば差し替え
                        条件値 = seifuhokenCd,
                        登録日時 = systemDate,
                        登録ユーザid = userId,
                        更新日時 = systemDate,
                        更新ユーザid = userId
                    };
                    db1.T01050バッチ条件s.Add(seifuhoken);

                    // すべてのエンティティを一括保存
                    db1.SaveChanges();

                    // トランザクションコミット
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    logger.Debug($"Exception: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        logger.Debug($"Inner Exception: {ex.InnerException.Message}");
                        if (ex.InnerException.InnerException != null)
                        {
                            logger.Debug($"Inner Exception (Level 2): {ex.InnerException.InnerException.Message}");
                        }
                    }
                    logger.Debug($"StackTrace: {ex.StackTrace}");
                    logger.Error("バッチ条件登録失敗");
                    transaction.Rollback();
                    return MessageUtil.Get("ME90005");
                }
            }
            return string.Empty;
        }

        #endregion

        /// <summary>
        /// 初期モデルの取得メソッド。
        /// </summary>
        /// <returns>初期モデル</returns>
        private D000999Model GetInitModel()
        {
            D000999Model model = new D000999Model();

            List<MTodofuken> todofukenList = TodofukenUtil.GetTodofukenList().ToList();
            if (todofukenList.Count() > 0)
            {
                model.TodofukenCd = todofukenList[0].TodofukenCd;
                model.TodofukenNm = todofukenList[0].TodofukenNm;
                List<MKumiaito> kumiaitoList = KumiaitoUtil.GetKumiaitoList(model.TodofukenCd);
                if (kumiaitoList.Count() > 0)
                {
                    model.KumiaitoCd = kumiaitoList[0].KumiaitoCd;
                    model.KumiaitoNm = kumiaitoList[0].KumiaitoNm;
                    List<MShishoNm> shishoList = ShishoUtil.GetShishoList(model.TodofukenCd, model.KumiaitoCd);
                    if (shishoList.Count() > 0)
                    {
                        model.ShishoCd = shishoList[0].ShishoCd;
                        model.ShishoNm = shishoList[0].ShishoNm;
                    }
                }
            }

            model.ScreenMode = "1";

            return model;
        }

        /// <summary>
        /// 和暦形式（例：R07/04/24）に変換するヘルパー
        /// </summary>
        private string ToWarekiString(DateTime? dt)
        {
            if (dt == null)
            {
                return "未実行";
            }

            var culture = new System.Globalization.CultureInfo("ja-JP", false);
            culture.DateTimeFormat.Calendar = new System.Globalization.JapaneseCalendar();

            return dt.Value.ToString("ggyy/MM/dd", culture)
                .Replace("平成", "H")
                .Replace("令和", "R")
                .Replace("昭和", "S");
        }
    }
}