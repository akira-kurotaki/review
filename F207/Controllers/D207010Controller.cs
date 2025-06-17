using System.Data;
using System.Text.RegularExpressions;
using CoreLibrary.Core.Attributes;
using CoreLibrary.Core.Base;
using CoreLibrary.Core.Exceptions;
using CoreLibrary.Core.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore.Storage;
using NskAppModelLibrary.Context;
using NskCommonLibrary.Core.Consts;
using NskWeb.Areas.F207.Consts;
using NskWeb.Areas.F207.Models.D207010;

namespace NskWeb.Areas.F207.Controllers
{
    [SessionOutCheck]
    [Authorize(Roles = "nsk")]
    [Area("F207")]
    public class D207010Controller : CoreController
    {
        #region "定数"
        /// <summary>
        /// セッションキー(D207010)
        /// </summary>
        private const string SESS_D207010 = $"{F207Const.SCREEN_ID_D207010}_SCREEN";
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="viewEngine"></param>
        public D207010Controller(ICompositeViewEngine viewEngine) : base(viewEngine)
        {
        }

        // GET: F207/D207010
        public ActionResult Index()
        {
            if (ConfigUtil.Get(CoreConst.D0000_DISPLAY_FLAG) == "true")
            {
                // 画面表示モードを設定
                SetScreenModeFromQueryString();
            }

            return RedirectToAction("Init", F207Const.SCREEN_ID_D207010, new { area = "F207" });
        }

        /// <summary>
        /// 初期表示
        /// </summary>
        /// <returns>単当修正量入力画面表示結果</returns>
        public ActionResult Init()
        {
            // １．セッション情報をクリアする。
            SessionUtil.Remove(SESS_D207010, HttpContext);

            // １．１．権限チェック
            D207010Model model = new();
            bool refKengen = ScreenSosaUtil.CanReference(F207Const.SCREEN_ID_D207010, HttpContext);         // 参照権限
            bool updKengen = ScreenSosaUtil.CanUpdate(F207Const.SCREEN_ID_D207010, HttpContext);            // 更新権限

            // (1) ログインユーザの権限が「参照」「一部権限」「更新権限」いずれも許可されていない場合、
            model.DispKengen = F207Const.Authority.None;
            if (updKengen)
            {

                model.DispKengen = F207Const.Authority.Update;      // 更新権限
            }
            else
            if (refKengen)
            {
                model.DispKengen = F207Const.Authority.ReadOnly;    // 参照権限
            }
            else
            {
                // メッセージを設定し業務エラー画面を表示する。
                throw new AppException("ME10075", MessageUtil.Get("ME10075"));
            }

            // ２．画面表示用データを取得する。
            // ２．１．セッションから「組合等コード」「都道府県コード」「年産」「共済目的コード」を取得する。
            D207010SessionInfo sessionInfo = new();
            sessionInfo.GetInfo(HttpContext);
            NskAppContext dbContext = getJigyoDb<NskAppContext>();
            // ２．２．「類区分リスト」を取得する。
            model.SearchCondition.GetRuiKbnLists(dbContext, sessionInfo.KumiaitoCd, sessionInfo.KyosaiMokutekiCd);
            // ２．３．「引受区分リスト」を取得する。
            model.SearchCondition.GetHikiukeHoushikiLists(dbContext, sessionInfo.KumiaitoCd, sessionInfo.KyosaiMokutekiCd,
                model.SearchCondition.RuiKbnLists.First().Value);
            // ２．４．「補償割合リスト」を取得する。
            model.SearchCondition.GetHoshouWariaiLists(dbContext, sessionInfo.KumiaitoCd, sessionInfo.KyosaiMokutekiCd,
                model.SearchCondition.RuiKbnLists.First().Value, model.SearchCondition.HikiukeHoushikiLists.First().Value);
            // ２．５．「階層区分リスト」を取得する。
            model.SearchCondition.GetKaisoKbnLists(dbContext, sessionInfo.KumiaitoCd, sessionInfo.KyosaiMokutekiCd, sessionInfo.Nensan);
            // ２．６．「表示数リスト」を取得する。
            model.SearchCondition.DisplayCount = CoreConst.PAGE_SIZE;
            // ２．７．「表示順」ドロップダウンリスト項目を設定する。		
            // (1) 「表示順」ドロップダウンリスト項目に以下を設定する。
            model.SearchCondition.DisplaySortTypes = new()
            {
                new("評価地区", "HyokaChikuCd"),
                new("悉皆調査面積", "ShikkaiChosaMensaki"),
                new("平均単収差", "HeikinTanshusa"),
                new("平均単収差の左加重値", "HeikinTanshusaHidariKajuchi"),
                new("単当修正量", "TantoShuseiryo"),
                new("単当修正量の左加重値", "TantoShuseiryoHidariKajuchi")
            };

            // ３．画面項目設定
            // ３．１．[２．１．]～[２．７．] で取得した値を設定する。
            // 未設定の年産と共済目的名称を設定
            D207010Model dispModel = new();
            model.Nensan = sessionInfo.Nensan.ToString();
            model.KyosaiMokuteki = dbContext.M00010共済目的名称s
                .Where(x => x.共済目的コード == sessionInfo.KyosaiMokutekiCd)
                .Select(x => x.共済目的名称)
                .Single();

            // ３．２．モデルデータをセッションに保存する。
            // 項目活性・非活性制御設定後に保存
            SessionUtil.Set(SESS_D207010, model, HttpContext);

            // 単当修正量入力画面を表示する
            return View(F207Const.SCREEN_ID_D207010, model);
        }

        /// <summary>
        /// 類区分変更
        /// </summary>
        /// <param name="ruiKbn">類区分</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ChangeRuikbn(string ruiKbn)
        {
            // ２．[類区分] に該当する「引受方式」「保証割合」を取得する。
            NskAppContext dbContext = getJigyoDb<NskAppContext>();
            D207010SessionInfo sessionInfo = new();
            sessionInfo.GetInfo(HttpContext);
            D207010Model model = SessionUtil.Get<D207010Model>(SESS_D207010, HttpContext);
            model.SearchCondition.RuiKbn = ruiKbn;

            // ２．１．「引受区分リスト」を取得する。
            model.SearchCondition.GetHikiukeHoushikiLists(dbContext, sessionInfo.KumiaitoCd, sessionInfo.KyosaiMokutekiCd,
                ruiKbn);
            // ２．２．「補償割合リスト」を取得する。
            model.SearchCondition.GetHoshouWariaiLists(dbContext, sessionInfo.KumiaitoCd, sessionInfo.KyosaiMokutekiCd,
                ruiKbn, model.SearchCondition.HikiukeHoushikiLists.First().Value);

            // ２．３．取得結果を返送する
            // (1) 「２．１．」「２．２．」で取得した「引受方式」ドロップダウンリスト、「補償割合」ドロップダウンリストをJSON化する。
            // モデルデータをセッションに保存する。
            SessionUtil.Set(SESS_D207010, model, HttpContext);

            // 出力順ドロップダウンの部分ビューをJson形式で返却する
            return PartialViewAsJson("_D207010SearchCondition", model);
        }

        /// <summary>
        /// 引受方式変更
        /// </summary>
        /// <param name="ruiKbn">類区分</param>
        /// <param name="hikiukeHoushiki">引受方式</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ChangeHikiukeHoshiki(string ruiKbn, string hikiukeHoushiki)
        {
            // ２．[引受方式] に該当する「補償割合」を取得する。
            NskAppContext dbContext = getJigyoDb<NskAppContext>();
            D207010SessionInfo sessionInfo = new();
            sessionInfo.GetInfo(HttpContext);
            D207010Model model = SessionUtil.Get<D207010Model>(SESS_D207010, HttpContext);
            model.SearchCondition.RuiKbn = ruiKbn;
            model.SearchCondition.HikiukeHoushiki = hikiukeHoushiki;

            // ２．１．「補償割合リスト」を取得する。
            model.SearchCondition.GetHoshouWariaiLists(dbContext, sessionInfo.KumiaitoCd, sessionInfo.KyosaiMokutekiCd,
                model.SearchCondition.RuiKbn, model.SearchCondition.HikiukeHoushiki);

            // ２．２．取得結果を返送する
            // (1) 「２．１．」で取得した「補償割合」ドロップダウンリストをJSON化する。
            // モデルデータをセッションに保存する。
            SessionUtil.Set(SESS_D207010, model, HttpContext);

            // 出力順ドロップダウンの部分ビューをJson形式で返却する
            return PartialViewAsJson("_D207010SearchCondition", model);
        }

        /// <summary>
        /// 検索
        /// </summary>
        /// <param name="dispModel">画面モデル</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Search(D207010Model dispModel)
        {
            D207010SessionInfo sessionInfo = new();
            sessionInfo.GetInfo(HttpContext);
            D207010Model model = SessionUtil.Get<D207010Model>(SESS_D207010, HttpContext);
            // 画面入力値をセッションモデルに反映
            model.SearchCondition.ApplyInput(dispModel.SearchCondition);

            // ３．検索処理
            // ３．１．検索結果クリア
            // 保持している検索結果、合計欄をクリアする。
            model.MessageArea2 = string.Empty;
            model.SearchResult = new(model.SearchCondition);
            model.TotalColumn = new();

            // ３．２．検索処理実行
            // ３．２．１．単当修正量入力情報を取得する。
            // t_23120_単当修正量テーブルから
            // [画面：類区分]、[画面：引受方式]、[画面：補償割合]、[画面：階層区分] に該当する単当修正量情報を取得する。
            NskAppContext dbContext = getJigyoDb<NskAppContext>();
            model.SearchResult.DisplayCount = model.SearchCondition.DisplayCount;
            model.SearchResult.GetPageDataList(dbContext, sessionInfo, F207Const.PAGE_1);

            // ３．３．検索結果のチェック
            // ３．３．１．検索結果0件の場合、エラーメッセージを設定する。
            string message = string.Empty;
            if (model.SearchResult.Pager.TotalCount == 0)
            {
                model.MessageArea2 = MessageUtil.Get("MI00011");
                ModelState.AddModelError("MessageArea2", MessageUtil.Get("MI00011"));
            }
            // 検索結果数が表示最大数より小さい場合にレコードカウントを反映。
            int cnt = model.SearchResult.DisplayCount;
            if (cnt > model.SearchResult.AllRecCount )
            {
                cnt = model.SearchResult.AllRecCount;
            }
            //検索結果が1件以上ある場合、画面でチェックボックスを動作させる為に値を置換してセットする。
            for (int x = 0; x < cnt; x++)
            {
                if (model.SearchResult.DispRecords[x].ShuseiNashiKbnInt == 1)
                {
                    model.SearchResult.DispRecords[x].ShuseiNashiKbn = true;
                }
                else
                {
                    model.SearchResult.DispRecords[x].ShuseiNashiKbn = false;
                }
            }


            // ３．４．セッションの入力データ（単当修正量リスト）を更新する
            SessionUtil.Set(SESS_D207010, model, HttpContext);

            // ３．５．取得結果を返送する
            // (1) 検索結果部分ビューをJSON化する。
            // (2) 合計欄部分ビューをJSON化する。
            return Json(new
            {
                message = model.MessageArea2,
                resultArea1 = PartialViewAsJson("_D207010SearchResult", model).Value,
                resultArea2 = PartialViewAsJson("_D207010TotalColumn", model).Value
            });
        }

        /// <summary>
        /// 検索結果ページャー
        /// </summary>
        /// <param name="id">ページID</param>
        /// <returns>ActionResult</returns>
        [HttpGet]
        public ActionResult ResultPager(string id)
        {
            // ページIDは数値以外のデータの場合
            if (!Regex.IsMatch(id, @"^[0-9]+$") || F207Const.PAGE_0 == id)
            {
                return BadRequest();
            }

            // セッションから単当修正量入力モデルを取得する
            D207010Model model = SessionUtil.Get<D207010Model>(SESS_D207010, HttpContext);

            // セッションに自画面のデータが存在しない場合
            if (model is null)
            {
                throw new AppException("MF00005", MessageUtil.Get("MF00005", "セッションから画面情報を取得できませんでした"));
            }

            // モデル状態ディクショナリからすべての項目を削除します。
            ModelState.Clear();

            // 検索結果を取得する
            NskAppContext dbContext = getJigyoDb<NskAppContext>();
            D207010SessionInfo sessionInfo = new();
            sessionInfo.GetInfo(HttpContext);
            model.SearchResult.GetPageDataList(dbContext, sessionInfo, int.Parse(id));
            // 検索結果数が表示最大数より小さい場合にレコードカウントを反映。
            int cnt = model.SearchResult.DispRecords.Count;
            if (cnt > model.SearchResult.AllRecCount)
            {
                cnt = model.SearchResult.AllRecCount;
            }
            //検索結果が1件以上ある場合、画面でチェックボックスを動作させる為に値を置換してセットする。

            for (int x = 0; x < cnt; x++)
            {
                if (model.SearchResult.DispRecords[x].ShuseiNashiKbnInt == 1)
                {
                    model.SearchResult.DispRecords[x].ShuseiNashiKbn = true;
                }
                else
                {
                    model.SearchResult.DispRecords[x].ShuseiNashiKbn = false;
                }
            }

            // 検索条件と検索結果をセッションに保存する
            SessionUtil.Set(SESS_D207010, model, HttpContext);

            return PartialViewAsJson("_D207010SearchResult", model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispModel"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        [HttpPost]
        public ActionResult Regist(D207010Model dispModel)
        {
            IDbContextTransaction? transaction = null;
            string message = string.Empty;

            try
            {
                D207010SessionInfo sessionInfo = new();
                sessionInfo.GetInfo(HttpContext);

                // セッションからモデルを取得する
                D207010Model model = SessionUtil.Get<D207010Model>(SESS_D207010, HttpContext);

                // セッションにデータが存在しない場合
                if (model is null)
                {
                    throw new AppException("MF00005", MessageUtil.Get("MF00005", "セッションから画面情報を取得できませんでした"));
                }

                // 画面入力値をセッションモデルに反映
                model.SearchCondition.ApplyInput(dispModel.SearchCondition);

                NskAppContext dbContext = getJigyoDb<NskAppContext>();
                transaction = dbContext.Database.BeginTransaction();

                int execCount = 0;
                int index = 0;
                // 更新レコード取得
                List<D207010ResultRecord> recs = model.SearchResult.DispRecords;
                foreach (D207010ResultRecord record in recs)
                {

                    record.ApplyInput(dispModel.SearchResult, index);
                    //２．１．入力データでt_22120_単当修正量テーブルを更新する。
                    execCount += record.Update(ref dbContext, sessionInfo, GetUserId(), DateUtil.GetSysDateTime(), record);
                    index += 1;
                }


                // 1件以上レコードがある場合
                if (execCount > 0)
                {
                    transaction.CommitAsync();
                    message = MessageUtil.Get("MI00004", "登録");
                }
            }
            catch (Exception ex)
            {
                transaction?.RollbackAsync();

                if (string.IsNullOrEmpty(message))
                {
                    if (ex is DBConcurrencyException)
                    {
                        // 排他エラーが含まれる場合は、以下のメッセージを表示する。
                        // [変数：エラーメッセージ] にエラーメッセージを設定
                        message = MessageUtil.Get("ME10081");
                    }
                    else
                    {
                        //  [変数：エラーメッセージ] にエラーメッセージを設定
                        message = MessageUtil.Get("MF00001");
                    }
                }
            }

            return Json(new { message });
        }

        #region 戻るイベント
        /// <summary>
        /// 戻る
        /// ポータルへ遷移する。
        /// </summary>
        /// <returns>ActionResult</returns>
        [HttpGet]
        public ActionResult Back()
        {
            // ポータル

            return Json(new { result = "success" });
        }
        #endregion


        ///<summary>
        ///計算ボタンイベント
        /// </summary>
        /// <param name="dispModel"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        [HttpPost]
        public ActionResult Calc(D207010Model dispModel)
        {
            D207010SessionInfo sessionInfo = new();
            sessionInfo.GetInfo(HttpContext);
            D207010Model model = SessionUtil.Get<D207010Model>(SESS_D207010, HttpContext);
            // 画面入力値をセッションモデルに反映
            model.SearchCondition.ApplyInput(dispModel.SearchCondition);

            model.MessageArea3 = string.Empty;
            model.TotalColumn = new();

            string message = string.Empty;

            try
            {
                //検索結果で画面に表示されている内容を取得
                int allRecordCnt = model.SearchResult.AllRecCount;//検索された全てのデータを取得
                List<D207010ResultRecord> dispRecords = model.SearchResult.DispRecords; //表示されているデータを取得

                //１．検索結果有無チェック

                decimal shikkaisum = 0; //画面：悉皆調査面積合計
                decimal heikintansum = 0; //平均単収差の左加重値合計
                decimal tantoShuseisum = 0; //単当修正量の左加重値合計

                if (allRecordCnt == 0)
                {
                    //検索結果がない場合、合計欄は空にする。
                    //元々の画面表示がない状態にする。
                    model.TotalColumn.ShikkaiChosaMensakiGokei = null;
                    model.TotalColumn.HeikinTanshusaHidariKajuchiGokei = null;
                    model.TotalColumn.TantoShuseiryoHidariKajuchiGokei = null;
                }
                else
                {
                    //検索結果がある場合、合計欄を計算する。
                    //２．合計欄計算
                    for (int i = 0; i < dispRecords.Count; i++)
                    {
                        //(1)[画面：悉皆調査面積]の合計を[画面：悉皆調査面積合計]に設定する。
                        shikkaisum += dispRecords[i].ShikkaiChosaMensaki;
                        //(2)[画面：悉皆調査面積] × [画面：平均単収差] した結果の合計を[画面：平均単収差の左加重値合計]に設定する。
                        heikintansum += dispRecords[i].ShikkaiChosaMensaki * dispRecords[i].HeikinTanshusa;
                        //(3)[画面：悉皆調査面積] × [画面：単当修正量] した結果の合計を[画面：単当修正量の左加重値合計]に設定する。            
                        tantoShuseisum += dispRecords[i].ShikkaiChosaMensaki * dispRecords[i].TantoShuseiryo;
                    }

                    model.TotalColumn.ShikkaiChosaMensakiGokei = shikkaisum;
                    model.TotalColumn.HeikinTanshusaHidariKajuchiGokei = heikintansum;
                    model.TotalColumn.TantoShuseiryoHidariKajuchiGokei = tantoShuseisum;
                }
            ;

                // セッションの入力データを更新する
                SessionUtil.Set(SESS_D207010, model, HttpContext);

            }
            catch (Exception ex)
            {
                //計算時エラーがある場合
                //計算時にエラーが発生した場合、メッセージエリア3にエラーメッセージを表示する。
                //動作[画面：メッセージエリア3] 以下のエラーメッセージを設定
                //"ME10033"エラーが発生しました。

                message = MessageUtil.Get("ME10033");
                model.MessageArea3 = message;

            }
            // 取得結果を返送する
            // 合計欄部分ビューをJSON化する。
            //計算時にエラーが発生しなかった場合、画面に計算結果を算出
            return Json(new
            {
                message = model.MessageArea3,
                resultArea2 = PartialViewAsJson("_D207010TotalColumn", model).Value
            });
        }




    }
}
