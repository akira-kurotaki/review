using CoreLibrary.Core.Base;
using ModelLibrary.Models;
using NskWeb.Areas.F000.Models.D000000;
using System.ComponentModel;
using CoreLibrary.Core.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;
using NskAppModelLibrary.Context;
using NskWeb.Common.Consts;

namespace NskWeb.Areas.F207.Models.D207030
{
    [Serializable]
    public class D207030Model : CoreViewModel
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public D207030Model()
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public D207030Model(Syokuin syokuin, List<SelectListItem> honshoShishoList)
        {
            this.VSyokuinRecords = new VSyokuin();
            this.D207030Info = new NSKPortalInfoModel();
            this.HonshoShishoList = honshoShishoList;
            this.HonshoShishoCd = honshoShishoList.FirstOrDefault()?.Value ?? string.Empty;
        }

        /// <summary>
        /// 職員マスタの検索結果
        /// </summary>
        public VSyokuin VSyokuinRecords { get; set; }
        public NSKPortalInfoModel D207030Info { get; set; }

        [DisplayName("メッセージエリア")]
        public string MessageArea { get; set; }

        [DisplayName("共済目的名称")]
        public string KyosaiMokutekiMeisho { get; set; }

        /// <summary>
        /// 更新権限フラグ
        /// </summary>
        public bool UpdateKengenFlg { get; set; }

        /// <summary>
        /// 本所・支所コード
        /// </summary>
        [DisplayName("本所・支所コード")]
        public string HonshoShishoCd { get; set; } = string.Empty;

        /// <summary>
        /// 本所・支所リスト
        /// </summary>
        [DisplayName("本所・支所リスト")]
        public List<SelectListItem> HonshoShishoList { get; set; } = new();

        /// <summary>
        /// 支所名
        /// </summary>
        [DisplayName("支所名")]
        public List<VShishoNm> ShishoNm { get; set; }

        /// <summary>
        /// 政府保険認定区分
        /// </summary>
        [DisplayName("政府保険認定区分")]
        public string SeifuHokenNinteiCd { get; set; } = string.Empty;

        /// <summary>
        /// 政府保険認定区分リスト
        /// </summary>
        [DisplayName("政府保険認定区分リスト")]
        public List<SelectListItem> SeifuHokenNinteiList { get; set; } = new();


        /// <summary>
        /// 本所・支所ドロップダウンリスト初期化（D207030）
        /// </summary>
        /// <param name="dbContext">業務DBコンテキスト</param>
        /// <param name="sessionInfo">画面用セッション情報</param>
        public void InitializeDropdonwList(NskAppContext dbContext, D207030SessionInfo sessionInfo)
        {
            // 初期化
            HonshoShishoList = new();
            ShishoNm = new();

            List<VShishoNm> shishoNms = new();

            // 「本所」の場合の分岐
            if (sessionInfo.ShishoCd == "")
            {
                if (sessionInfo.HikiukeJikkoTanniKbnHikiuke == "1")
                {
                    // 本所のみ表示
                    shishoNms.AddRange(dbContext.VShishoNms
                        .Where(x =>
                            x.TodofukenCd == sessionInfo.TodofukenCd &&
                            x.KumiaitoCd == sessionInfo.KumiaitoCd &&
                            x.ShishoCd == AppConst.HONSHO_CD)
                        .OrderBy(x => x.ShishoCd));
                }
                else if (sessionInfo.HikiukeJikkoTanniKbnHikiuke == "2" ||
                         sessionInfo.HikiukeJikkoTanniKbnHikiuke == "3")
                {
                    // 本所配下のすべての支所を表示
                    shishoNms.AddRange(dbContext.VShishoNms
                        .Where(x =>
                            x.TodofukenCd == sessionInfo.TodofukenCd &&
                            x.KumiaitoCd == sessionInfo.KumiaitoCd)
                        .OrderBy(x => x.ShishoCd));
                }
            }
            else
            {
                // 支所ユーザーの場合：自支所と利用可能支所
                var shishoCds = new List<string> { sessionInfo.ShishoCd };
                shishoCds.AddRange(sessionInfo.RiyokanoShishoList.Select(x => x.ShishoCd));

                shishoNms.AddRange(dbContext.VShishoNms
                    .Where(x =>
                        x.TodofukenCd == sessionInfo.TodofukenCd &&
                        x.KumiaitoCd == sessionInfo.KumiaitoCd &&
                        shishoCds.Contains(x.ShishoCd))
                    .OrderBy(x => x.ShishoCd));
            }

            // 選択肢の生成
            foreach (var shisho in shishoNms)
            {
                HonshoShishoList.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = $"{shisho.ShishoCd} {shisho.ShishoNm}",
                    Value = shisho.ShishoCd
                });
            }

            // 初期選択値の設定（最初の要素）
            HonshoShishoCd = shishoNms.FirstOrDefault()?.ShishoCd ?? string.Empty;
            ShishoNm = shishoNms;
        }

        [DisplayName("実行履歴")]
        public class JikkouRireki
        {
            public string ShishoNm { get; set; }
            public string JikkouDate { get; set; }
        }

        public List<JikkouRireki> JikkouRirekiList { get; set; } = new();

    }
}