using CoreLibrary.Core.Dto;
using CoreLibrary.Core.Utility;
using NskCommonLibrary.Core.Consts;
using NskWeb.Areas.F000.Models.D000000;
using NskWeb.Common.Consts;
using NskWeb.Common.Models;

namespace NskWeb.Areas.F207.Models.D207020
{
    /// <summary>
    /// セッション情報
    /// </summary>
    public class D207020SessionInfo : BaseSessionInfo
    {
        /// <summary>
        /// 都道府県コード
        /// </summary>
        public string TodofukenCd { get; set; } = string.Empty;
        /// <summary>
        /// 組合等コード
        /// </summary>
        public string KumiaitoCd { get; set; } = string.Empty;
        /// <summary>
        /// 年産
        /// </summary>
        public int Nensan { get; set; }
        /// <summary>
        /// 共済目的
        /// </summary>
        public string KyosaiMokutekiCd { get; set; } = string.Empty;

        /// <summary>
        /// 支所コード
        /// </summary>
        public string ShishoCd { get; set; } = string.Empty;

        /// <summary>
        /// 「引受計算支所実行単位区分_引受」
        /// </summary>
        public string ShishoJikkoHikiukeKbn { get; set; } = string.Empty;

        /// <summary>
        /// 「利用可能支所一覧」
        /// </summary>
        public List<Shisho> RiyoKanouSisyoItiran { get; set; }




        /// <summary>
        /// セッション情報取得
        /// </summary>
        /// <param name="context"></param>
        public void GetInfo(HttpContext context)
        {
            Syokuin syokuin = SessionUtil.Get<Syokuin>(CoreConst.SESS_LOGIN_USER, context) ?? new Syokuin();
            NSKPortalInfoModel potalModel = SessionUtil.Get<NSKPortalInfoModel>(AppConst.SESS_NSK_PORTAL, context);
            List<Shisho> shishoList = SessionUtil.Get<List<Shisho>>(CoreConst.SESS_SHISHO_GROUP, context);
            // 「組合等コード」
            KumiaitoCd = syokuin.KumiaitoCd;
            // 「都道府県コード」
            TodofukenCd = syokuin.TodofukenCd;
            // 「年産」
            Nensan = int.TryParse(potalModel?.SNensanHikiuke, out int nensan) ? nensan : 0;
            // 「共済目的コード」
            KyosaiMokutekiCd = potalModel?.SKyosaiMokutekiCd;
            // 「支所コード」
            ShishoCd = syokuin.ShishoCd;
            // 「引受計算支所実行単位区分_引受」
            ShishoJikkoHikiukeKbn = potalModel.SHikiukeJikkoTanniKbnHikiuke;
            // 「利用可能支所一覧」
            RiyoKanouSisyoItiran = shishoList;

        }
    }
}