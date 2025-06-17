using CoreLibrary.Core.Attributes;
using CoreLibrary.Core.Base;
using CoreLibrary.Core.Dto;
using CoreLibrary.Core.Exceptions;
using CoreLibrary.Core.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Npgsql;
using NskAppModelLibrary.Context;
using NskCommonLibrary.Core.Consts;
using NskWeb.Areas.F000.Models.D000000;
using NskWeb.Areas.F207.Consts;
using NskWeb.Areas.F207.Models.D207020;
using NskWeb.Common.Consts;
using ReportService.Core;
using System.Text;
 

namespace NskWeb.Areas.F207.Controllers
{
    /// <summary>
    /// 当初評価高計算処理（半相殺）
    /// </summary>
    [SessionOutCheck]
    [Authorize(Roles = "nsk")]
    [Area("F207")]
    public class D207020Controller : CoreController
    {
        #region メンバー定数
        /// <summary>
        /// 画面ID(D207020)
        /// </summary>
        private static readonly string SCREEN_ID_D207020 = "D207020";

        /// <summary>
        /// セッションキー(D207020)
        /// </summary>
        private static readonly string SESS_D207020 = SCREEN_ID_D207020 + "_" + "SCREEN";

        /// <summary>
        /// 検索結果ビュー名（検索結果）
        /// </summary>
        private static readonly string RESULT_VIEW_NAME = "_D207020SearchResult";

        /// <summary>
        /// 画面名
        /// </summary>
        private static readonly string D207020_SCREEN_NM = "NSK_207020D";

        /// <summary>
        /// バッチ名
        /// </summary>
        private static readonly string D207020_BATCH_NM = "NSK_207021B";
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="viewEngine"></param>
        /// <param name="serviceClient"></param>
        public D207020Controller(ICompositeViewEngine viewEngine, ReportServiceClient serviceClient) : base(viewEngine, serviceClient)
        {
        }
        #endregion

        #region 初期表示イベント
        /// <summary>
        /// イベント：初期化
        /// </summary>
        /// <returns>ActionResult</returns>
        [HttpGet]
        public ActionResult Init()
        {

            // モデル状態ディクショナリからすべての項目を削除します。       
            ModelState.Clear();
            // セッション情報から検索条件、検索結果件数をクリアする
            SessionUtil.Remove(SESS_D207020, HttpContext);

            // ログインユーザの参照・更新可否判定
            // 画面IDをキーとして、画面マスタ、画面機能権限マスタを参照し、ログインユーザに本画面の権限がない場合は業務エラー画面を表示する。
            if (!ScreenSosaUtil.CanReference(SCREEN_ID_D207020, HttpContext))
            {
                //「参照権限」が許可されていない場合、業務エラー画面を表示する。
                throw new AppException("ME90003", MessageUtil.Get("ME90003"));
            }

            // 利用可能な支所一覧
            var shishoList = SessionUtil.Get<List<Shisho>>(CoreConst.SESS_SHISHO_GROUP, HttpContext);

            // 画面モデルの生成
            D207020Model model = new D207020Model(Syokuin, shishoList);

            //　2.権限チェック
            bool refKengen = ScreenSosaUtil.CanReference(F207Const.SCREEN_ID_D207020, HttpContext);         // 参照権限
            bool updKengen = ScreenSosaUtil.CanUpdate(F207Const.SCREEN_ID_D207020, HttpContext);            // 更新権限

            model.DispKengen = F207Const.Authority.None;
            if (updKengen)
            {
                model.DispKengen = F207Const.Authority.Update;      // 更新権限

            }
            else if (refKengen)
            {
                model.DispKengen = F207Const.Authority.ReadOnly;    // 参照権限
            }
            else
            {
                // メッセージを設定し業務エラー画面を表示する。
                throw new AppException("ME10075", MessageUtil.Get("ME10075"));
            }

         
            // ３．１．セッションから「都道府県コード」「組合等コード」「支所コード」「年産」「共済目的」「引受計算支所実行単位区分_引受」「利用可能支所一覧」を取得する。
            D207020SessionInfo sessionInfo = new();
            sessionInfo.GetInfo(HttpContext);
            NskAppContext dbContext = getJigyoDb<NskAppContext>();
            // ３．２．[本所・支所]ドロップダウンリスト項目を取得する。
            model.SearchCondition.GetShishoLists(dbContext, sessionInfo);
            // ３．３．実行履歴リストを取得する。
            // (1) t_24010_組合員等別損害情報テーブルから実行履歴リストを取得する。
            var returnRecords = model.SearchCondition.GetJikkoRirekiInfo(dbContext, sessionInfo);

            ////共済目的コードおよび年産をセッションから取得
            NSKPortalInfoModel m = SessionUtil.Get<NSKPortalInfoModel>(AppConst.SESS_NSK_PORTAL, HttpContext);         

            //// 組合等コードを職員情報から取得
            //String KumiaitoCd = Syokuin.KumiaitoCd;
            String Nensan = sessionInfo.Nensan.ToString();
            //共済目的・年産・組合等コードが取得できなければメッセージエリア１を表示する
            if (string.IsNullOrEmpty(sessionInfo.KyosaiMokutekiCd) || string.IsNullOrEmpty(Nensan) || string.IsNullOrEmpty(sessionInfo.KumiaitoCd))
            {
                ModelState.AddModelError("MessageArea1", MessageUtil.Get("ME10098"));
            }

            //共済目的または年産を取得できれば表示する
            if (!string.IsNullOrEmpty(sessionInfo.KyosaiMokutekiCd) || !string.IsNullOrEmpty(Nensan))
            {
                model.SearchCondition.Nensan = Nensan;
                model.SearchCondition.KyosaiMokutekiCd = sessionInfo.KyosaiMokutekiCd;
                //共済目的コードが空白以外の時に名称を取得する
                if (!string.IsNullOrEmpty(sessionInfo.KyosaiMokutekiCd))
                {
                    var strKyosaiMokuteki = getJigyoDb<NskAppContext>().M00010共済目的名称s.Where(t => t.共済目的コード == sessionInfo.KyosaiMokutekiCd).SingleOrDefault();
                    model.SearchCondition.KyosaiMokutekiNm = strKyosaiMokuteki.共済目的名称.ToString();
                }
            }

            // 組合等コードを設定する
            model.SearchCondition.KumiaitoCd = sessionInfo.KumiaitoCd;
            // 引受計算実行単位区分_引受を設定する
            model.SearchCondition.ShishoJikkoHikiukeKbn = sessionInfo.ShishoJikkoHikiukeKbn;
            //実施日等設定
            model.SearchResult.TableRecords = returnRecords;

            // ドロップダウンリストの先頭データを取得
            var firstItem = model.SearchCondition.HonshoshishoList.FirstOrDefault();
            string selectValue = firstItem.Value;

            if (model.DispKengen == F207Const.Authority.ReadOnly)
            {
                if (sessionInfo.ShishoCd == "00")
                {
                    switch (sessionInfo.ShishoJikkoHikiukeKbn)
                    {

                        case "1":
                            model.IsShishoDropDownDisabled = true;
                            model.IsJikkoBtnDisabled = true;
                            break;
                        case "2":
                            model.IsJikkoBtnDisabled = true;
                            break;
                        case "3":
                            model.IsJikkoBtnDisabled = true;
                            break;
                        default:
                            break;
                    }
                    ;
                }
                else if (sessionInfo.ShishoCd != "00")
                {
                    model.IsJikkoBtnDisabled = true;
                }
                ;
            }
            else if (model.DispKengen == F207Const.Authority.Update)
            {
                if (sessionInfo.ShishoCd == "00")
                {
                    switch (sessionInfo.ShishoJikkoHikiukeKbn)
                    {

                        case "1":
                            model.IsShishoDropDownDisabled = true;
                            break;
                        default:
                            break;
                    }
                    ;
                }
                ;
            }
            ;

            // 検索条件と検索結果をセッションに保存する
            SessionUtil.Set(SESS_D207020, model, HttpContext);


            // 画面を表示する
            return View(SCREEN_ID_D207020, model);
        }
        #endregion

        #region 戻るイベント
        /// <summary>
        /// イベント名：戻る 
        /// </summary>
        /// <returns>ActionResult</returns>
        [HttpGet]
        public ActionResult Back()
        {
            // セッション情報から検索条件、検索結果件数をクリアする
            SessionUtil.Remove(SESS_D207020, HttpContext);

            return Json(new { result = "success" });
        }
        #endregion



        #region 支所実行履歴取得メソッド(ドロップダウン選択時)
        /// <summary>
        /// メソッド：支所実行履歴を取得する(ドロップダウン選択時)
        /// </summary>
        /// <param name="model">ビューモデル</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Change(D207020Model form)
        {
            // セッションから画面モデルを取得する
            var model = SessionUtil.Get<D207020Model>(SESS_D207020, HttpContext);

            // セッションに自画面のデータが存在しない場合
            if (model == null)
            {
                throw new SystemException(MessageUtil.Get("MF00005", "セッションから画面情報を取得できませんでした"));
            }

            // モデル状態ディクショナリからすべての項目を削除します。
            ModelState.Clear();
            // テーブルをクリアする
            form.SearchResult = new D207020SearchResult();

            // sql用定数定義
            var sql = new StringBuilder();
            var parameters = new List<NpgsqlParameter>();

            // ポータルのセッションを取得
            NSKPortalInfoModel pmodel = SessionUtil.Get<NSKPortalInfoModel>(AppConst.SESS_NSK_PORTAL, HttpContext);
            // 画面の引受計算支所実行単位区分を取得
            string ShishoJikkoHikiukeKbn = pmodel.SHikiukeJikkoTanniKbnHikiuke;
            // 本所支所ドロップダウンリストで選択されている支所コードを取得
            string DropDownShishoCd = form.SearchCondition.SelectShishoCd;
            D207020SessionInfo sessionInfo = new();
            sessionInfo.GetInfo(HttpContext);
            NskAppContext dbContext = getJigyoDb<NskAppContext>();
            //選択された支所の実行履歴を取得
            model.SearchResult.TableRecords = model.SearchCondition.SelectSishoRireki(dbContext, sessionInfo, DropDownShishoCd);

            // 検索条件と検索結果をセッションに保存する
            SessionUtil.Set(SESS_D207020, model, HttpContext);

            return PartialView(RESULT_VIEW_NAME, model);
        }
        #endregion

        #region バッチ予約登録イベント
        /// <summary>
        /// イベント名：バッチ予約登録
        /// </summary>
        /// <returns>ActionResult</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Execution(D207020Model model)
        {
            // システム日時
            var systemDate = DateUtil.GetSysDateTime();

            // バッチ予約を登録する。
            string message = string.Empty;
            long batchId = 0L;
            int? insertResult = null;

            // メッセージ用の支所名称を設定
            string msgShisho = "バッチの予約登録";
            // レスポンスメッセージ用変数定義
            var responseMsg = string.Empty;

            D207020SessionInfo sessionInfo = new();
            sessionInfo.GetInfo(HttpContext);
            NskAppContext dbContext = getJigyoDb<NskAppContext>();



            //       ２．バッチ予約登録
            //２．１．バッチ条件を登録する。																																		
            //	(1) バッチ条件IDを取得する。																																
            //		Guid.NewGuid().ToString("D")の結果をバッチ条件IDとする。																																


            string BatchId = Guid.NewGuid().ToString("D");

            //	(2) バッチ条件値を生成する。																																
            //		連番 条件名称                    条件値
            //        1  年産                       [セッション：年産]
            //        2  共済目的                   [セッション：共済目的]
            //        3  支所コード                 [画面：本所・支所ドロップダウンリスト]の選択値
            //        4  引受方式コード             '2'     半相殺

            // [変数：バッチ条件表示用] に以下のように設定する。
            // 年産：[セッション：年産] 改行 共済目的：[セッション：共済目的]　改行 支所コード：・・・・
            StringBuilder sbDispBatchJoken = new();
            sbDispBatchJoken.Append($"{JoukenNameConst.JOUKEN_NENSAN}:{model.SearchCondition.Nensan}\n");
            sbDispBatchJoken.Append($"{JoukenNameConst.JOUKEN_KYOSAI_MOKUTEKI_CD}:{model.SearchCondition.KyosaiMokutekiCd}\n");
            sbDispBatchJoken.Append($"{JoukenNameConst.JOUKEN_SHISHO}:{model.SearchCondition.SelectShishoCd}\n");
            sbDispBatchJoken.Append($"{JoukenNameConst.JOUKEN_HIKIUKE_HOUSHIKI_CD}:2\n");


            // バッチ条件（表示用）
            string DropDownShishoCd = model.SearchCondition.SelectShishoCd;
            try
            {
                using (var tx = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        //           (3) バッチ条件を登録する。																														
                        //引受年産を条件テーブルに登録
                        model.SearchCondition.CreatBatchJoken(model.SearchCondition.CreatBatchJokenNensan(model.SearchCondition.Nensan, Syokuin, BatchId, systemDate, 1), dbContext);

                        //共済目的コードを条件テーブルに登録
                        model.SearchCondition.CreatBatchJoken(model.SearchCondition.CreatBatchJokenKyosaiMokuteki(model.SearchCondition.KyosaiMokutekiCd, Syokuin, BatchId, systemDate, 2), dbContext);

                        // 支所コードを条件テーブルに登録
                        model.SearchCondition.CreatBatchJoken(model.SearchCondition.CreatBatchJokenShishoCd(model.SearchCondition.SelectShishoCd, Syokuin, BatchId, systemDate, 3), dbContext);

                        //引受年産を条件テーブルに登録
                        model.SearchCondition.CreatBatchJoken(model.SearchCondition.CreatBatchJokenHikiukeCd("2", Syokuin, BatchId, systemDate, 4), dbContext);

                        // 全てinsertが完了したらコミット
                        tx.Commit();
                    }
                    catch
                    {
                        // 失敗の場合ロールバックする
                        tx.Rollback();
                        throw;
                    }
                }
                //２．２．バッチ予約を登録する。																																		
                //	バッチ予約登録（BatchUtil.InsertBatchYoyaku()）を呼び出し、バッチ登録を行う。																																	
                //		引数 値

                //           バッチ分類                 「その他」																										
                //		     システム区分               「農作物共済」																										
                //		     都道府県コード             [セッション：都道府県コード]
                //           組合等コード               [セッション：組合等コード]
                //           支所コード                 [セッション：支所コード]
                //           予約日時                   [共通部品：システム日時]
                //           予約ユーザID               [セッション：ユーザID]
                //           予約を実行した処理名       「NSK_207020D」																										
                //		     バッチ名                   「NSK_207021B」																										
                //		     バッチ条件                  ２．１．(1)で取得した値
                //           バッチ条件（表示用）		[変数：バッチ条件表示用]
                //           複数実行禁止フラグ			「複数実行不可」																										
                //		     バッチ種類（巡回、定時）	「巡回バッチ」																										
                //		     定時実行バッチ予約日時     [共通部品：システム日時]

                //           ロック対象フラグ                        「ロック対象としない」						
                insertResult = BatchUtil.InsertBatchYoyaku(AppConst.BatchBunrui.BATCH_BUNRUI_90_OTHER,
                                                   ConfigUtil.Get(CoreConst.APP_ENV_SYSTEM_KBN),
                                                   Syokuin.TodofukenCd,
                                                   Syokuin.KumiaitoCd,
                                                   sessionInfo.ShishoCd,
                                                   systemDate,
                                                   Syokuin.UserId,
                                                   D207020_SCREEN_NM,
                                                   D207020_BATCH_NM,
                                                   BatchId,
                                                   sbDispBatchJoken.ToString(),
                                                   CoreConst.FLG_OFF,
                                                   AppConst.BatchType.BATCH_TYPE_PATROL,
                                                   systemDate,
                                                   CoreConst.FLG_OFF,
                                                   ref message,
                                                   ref batchId);




            }
            catch (Exception e)
            {
                // （出力メッセージ：バッチ予約登録失敗）
                // （出力メッセージ：（メッセージID：ME01645、引数{0}に失敗しました。"））
                logger.Error("バッチ予約登録失敗");
                logger.Error(MessageUtil.Get("ME01645", msgShisho));
                logger.Error(MessageUtil.GetErrorMessage(e, CoreConst.LOG_MAX_INNER_EXCEPTION));
                return Json(new { message = MessageUtil.Get("ME01645", msgShisho) });
            }

            // モデル状態ディクショナリからすべての項目を削除します。
            ModelState.Clear();

            // 処理結果がエラーだった場合
            if (insertResult == 0)
            {
                // （出力メッセージ：バッチ予約登録失敗）
                // （出力メッセージ：（メッセージID：ME90008、引数{0}に失敗しました。）"））
                logger.Error("バッチ予約登録失敗");
                logger.Error(MessageUtil.Get("ME01645", msgShisho));
                responseMsg = MessageUtil.Get("ME01645", msgShisho);
            }
            else
            {
                // バッチ予約完了メッセージ
                logger.Info("バッチ予約登録成功");
                logger.Info(MessageUtil.Get("MI00004", msgShisho));
                responseMsg = MessageUtil.Get("MI00004", msgShisho);
            }

            return Json(new { message = responseMsg });
        }
        #endregion


        #region "農単抜取調査悉皆評価反映チェック"
        /// <summary>'農単抜取調査悉皆評価反映チェック
        /// イベント名：'農単抜取調査悉皆評価反映チェック
        /// </summary>
        /// <returns>ActionResult</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckExecution(D207020Model model)
        {
            // システム日時
            var systemDate = DateUtil.GetSysDateTime();

            //フラグ初期化
            model.IsCheckFlg = 0;

            // バッチ予約を登録する。
            string message = string.Empty;



            // メッセージ用の支所名称を設定
            //string msgShisho = model.SearchCondition.SelectShishoNm + " バッチ登録";
            //string msgShisho = "バッチの予約登録";
            // レスポンスメッセージ用変数定義
            var responseMsg = string.Empty;

            D207020SessionInfo sessionInfo = new();
            sessionInfo.GetInfo(HttpContext);
            NskAppContext dbContext = getJigyoDb<NskAppContext>();
            //２．農単抜取調査悉皆評価反映チェックを行う。
            int datacount = model.SearchCondition.NotanDataCount(dbContext, sessionInfo);

            // モデル状態ディクショナリからすべての項目を削除します。
            ModelState.Clear();

            // 処理結果がエラーだった場合
            if (datacount > 0)
            {

                // （出力メッセージ：（メッセージID：ME10135、未反映の農単申告抜取調査データがあります。農単申告抜取調査データ反映処理を実行してください。））
                logger.Error("未反映の農単申告抜取調査データがあります。農単申告抜取調査データ反映処理を実行してください。");
                logger.Error(MessageUtil.Get("ME10135"));
                responseMsg = MessageUtil.Get("ME10135");
                //フラグがtrueならダイアログは非表示
                model.IsCheckFlg = 1;

            }
            // 検索条件と検索結果をセッションに保存する
            SessionUtil.Set(SESS_D207020, model, HttpContext);


            return Json(new { message = responseMsg, flg = model.IsCheckFlg });
        }
        #endregion

    }
}