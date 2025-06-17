using CoreLibrary.Core.Dto;
using CoreLibrary.Core.Utility;
using NskCommonLibrary.Core.Consts;
using NskWeb.Areas.F000.Models.D000000;
using NskWeb.Common.Consts;
using NskWeb.Common.Models;

namespace NskWeb.Areas.F207.Models.D207010
{
    /// <summary>
    /// セッション情報
    /// </summary>
    public class D207010SessionInfo : BaseSessionInfo
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
        /// セッション情報取得
        /// </summary>
        /// <param name="context"></param>
        public void GetInfo(HttpContext context)
        {
            Syokuin syokuin = SessionUtil.Get<Syokuin>(CoreConst.SESS_LOGIN_USER, context) ?? new Syokuin();
            NSKPortalInfoModel potalModel = SessionUtil.Get<NSKPortalInfoModel>(AppConst.SESS_NSK_PORTAL, context);

            // 「組合等コード」
            KumiaitoCd = syokuin.KumiaitoCd;
            // 「都道府県コード」
            TodofukenCd = syokuin.TodofukenCd;
            // 「年産」
            Nensan = int.TryParse(potalModel?.SNensanHikiuke, out int nensan) ? nensan : 0;
            // 「共済目的コード」
            KyosaiMokutekiCd = potalModel?.SKyosaiMokutekiCd;
        }
    }
}