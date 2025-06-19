using CoreLibrary.Core.Consts;
using CoreLibrary.Core.Utility;        
using NskWeb.Areas.F000.Models.D000000;
using NskWeb.Common.Consts;
using CoreLibrary.Core.Dto;

namespace NskWeb.Areas.F207.Models.D207030
{
    /// <summary>
    /// セッション情報（D207030）
    /// </summary>
    public class D207030SessionInfo
    {
        /// <summary>
        /// 組合等コード
        /// </summary>
        public string KumiaitoCd { get; set; } = string.Empty;

        /// <summary>
        /// 都道府県コード
        /// </summary>
        public string TodofukenCd { get; set; } = string.Empty;

        /// <summary>
        /// 支所コード
        /// </summary>
        public string ShishoCd { get; set; } = string.Empty;

        /// <summary>
        /// 年産（引受）
        /// </summary>
        public int Nensan { get; set; }

        /// <summary>
        /// 共済目的コード
        /// </summary>
        public string KyosaiMokutekiCd { get; set; } = string.Empty;

        /// <summary>
        /// 引受計算支所実行単位区分（引受）
        /// </summary>
        public string HikiukeJikkoTanniKbnHikiuke { get; set; } = string.Empty;

        /// <summary>
        /// 利用可能支所一覧
        /// </summary>
        public List<Shisho> RiyokanoShishoList { get; set; } = new();

        /// <summary>
        /// セッション情報を取得して各プロパティにセットする
        /// </summary>
        /// <param name="context">HTTPコンテキスト</param>
        public void GetInfo(HttpContext context)
        {
            // ログイン職員情報
            var syokuin = SessionUtil.Get<Syokuin>(CoreConst.SESS_LOGIN_USER, context) ?? new Syokuin();

            // ポータル情報（年産・共済目的など）
            var portalModel = SessionUtil.Get<NSKPortalInfoModel>(AppConst.SESS_NSK_PORTAL, context);

            // セッションから情報を取得
            TodofukenCd = syokuin.TodofukenCd;
            KumiaitoCd = syokuin.KumiaitoCd;
            ShishoCd = syokuin.ShishoCd;
            Nensan = int.TryParse(portalModel?.SNensanHyoka, out var nen) ? nen : 0;
            KyosaiMokutekiCd = portalModel?.SKyosaiMokutekiCd ?? string.Empty;
            HikiukeJikkoTanniKbnHikiuke = portalModel?.SHikiukeJikkoTanniKbnHikiuke ?? string.Empty;

            // 利用可能支所一覧（支所グループ）
            RiyokanoShishoList = SessionUtil.Get<List<Shisho>>(CoreConst.SESS_SHISHO_GROUP, context) ?? new();
        }
    }
}
