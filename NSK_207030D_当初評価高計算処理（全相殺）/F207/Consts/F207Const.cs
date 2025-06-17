using System.ComponentModel;

namespace NskWeb.Areas.F207.Consts
{
    /// <summary>
    /// F207機能共通定数クラス
    /// </summary>
    public class F207Const
    {
        /// <summary>
        /// ページ0
        /// </summary>
        public const string PAGE_0 = "0";

        /// <summary>
        /// ページ1
        /// </summary>
        public const int PAGE_1 = 1;

        /// <summary>
        /// 共済目的=陸稲
        /// </summary>
        public const string KYOSAI_MOKUTEKI_RIKUTO = "20";

        /// <summary>
        /// 引受方式=全相殺
        /// </summary>
        public const string HIKIUKE_ZENSOUSAI = "3";

        // D207010

        /// <summary>
        /// 画面ID(D207010)
        /// </summary>
        public const string SCREEN_ID_D207010 = "D207010";

        /// <summary>
        /// セッションキー(D207010)
        /// </summary>
        public const string SESS_D207010 = "D207010_SCREEN";

        /// <summary>
        /// 予約を実行した処理名(207010D)
        /// </summary>
        public const string SCREEN_ID_NSK_D207010 = "NSK_207010D";

        /// <summary>
        /// バッチID(207011B)
        /// </summary>
        public const string BATCH_ID_NSK_207011B = "NSK_207011B";


        // D207020

        /// <summary>
        /// 画面ID(D207020)
        /// </summary>
        public const string SCREEN_ID_D207020 = "D207020";

        /// <summary>
        /// セッションキー(D207020)
        /// </summary>
        public const string SESS_D207020 = "D207020_SCREEN";

        /// <summary>
        /// 予約を実行した処理名(207020D)
        /// </summary>
        public const string SCREEN_ID_NSK_D207020 = "NSK_207020D";

        /// <summary>
        /// バッチID(207021B)
        /// </summary>
        public const string BATCH_ID_NSK_207021B = "NSK_207021B";


        // D207030

        /// <summary>
        /// 画面ID(D207030)
        /// </summary>
        public const string SCREEN_ID_D207030 = "D207030";

        /// <summary>
        /// セッションキー(D207030)
        /// </summary>
        public const string SESS_D207030 = "D207030_SCREEN";

        /// <summary>
        /// 予約を実行した処理名(207023D)
        /// </summary>
        public const string SCREEN_ID_NSK_D207030 = "NSK_207030D";

        /// <summary>
        /// バッチID(207031B)
        /// </summary>
        public const string BATCH_ID_NSK_207031B = "NSK_207031B";


        /// <summary>
        /// 権限
        /// </summary>
        public enum Authority
        {
            /// <summary>権限なし</summary>
            [Description("権限なし")]
            None,
            /// <summary>参照権限</summary>
            [Description("参照権限")]
            ReadOnly,
            /// <summary>一部権限</summary>
            [Description("一部権限")]
            Part,
            /// <summary>更新権限</summary>
            [Description("更新権限")]
            Update
        }
    }
}
