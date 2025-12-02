using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EECOnline.DataLayers;
using EECOnline.Models.Entities;
using EECOnline.Services;

namespace EECOnline.Models
{
    /// <summary>
    /// 提供跨Session共用資料的存取類
    /// </summary>
    public class ApplicationModel: ApplicationBaseModel
    {
        #region Private Methods
        private static object _lock = new object();
        private static ApplicationModel _instance = null;

        private ApplicationModel()
        {

        }


        private static ApplicationModel GetInstance()
        {
            lock (_lock)
            {
                if(_instance == null)
                {
                    _instance = new ApplicationModel();
                }
                return _instance;
            }
        }
        #endregion

        #region 取得系統清單

        /// <summary>
        /// 取得系統最外層清單(沒有PRGID)
        /// </summary>
        /// <returns></returns>
        public static IList<TblAMFUNCM> GetClamFuncsOutAll()
        {
            const string _KEY = "TblCLAMFUNCMAll";
            ApplicationModel model = GetInstance();
            object value = model.GetApplicationVar(_KEY);

            if (value != null && value is IList<TblAMFUNCM>)
            {
                // 已存在, 直接返回
                return (IList<TblAMFUNCM>)value;
            }
            else
            {
                // 不存在或過期, 從DB中載入
                BaseDAO dao = new BaseDAO();
                // 載出所有程式代碼(顯示在清單的)
                IList<TblAMFUNCM> list = dao.QueryForListAll<TblAMFUNCM>("Login.getClamFuncsOutAll", null);

                // 將 list 儲存至 ApplictionModel 中
                // 並設定有效時間至系統時間當天的 23:59:59 
                // (也就是隔天的 00:00:00 失效)
                DateTime now = DateTime.Now;
                now = now.AddDays(1);
                DateTime expire = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

                model.SetApplicationVar(_KEY, list, expire);
                return list;
            }
        }

        /// <summary>
        /// 取得系統已啟用的全部功能清單(已排序)
        /// </summary>
        /// <returns></returns>
        public static IList<TblAMFUNCM> GetClamFuncsAll()
        {
            const string _KEY = "TblCLAMFUNCM";
            ApplicationModel model = GetInstance();
            object value = model.GetApplicationVar(_KEY);

            if (value != null && value is IList<TblAMFUNCM>)
            {
                // 已存在, 直接返回
                return (IList<TblAMFUNCM>)value;
            }
            else
            {
                // 不存在或過期, 從DB中載入
                BaseDAO dao = new BaseDAO();
                // 載出所有程式代碼(顯示在清單的)
                IList<TblAMFUNCM> list = dao.QueryForListAll<TblAMFUNCM>("Login.getClamFuncsAll", null);

                // 將 list 儲存至 ApplictionModel 中
                // 並設定有效時間至系統時間當天的 23:59:59 
                // (也就是隔天的 00:00:00 失效)
                DateTime now = DateTime.Now;
                now = now.AddDays(1);
                DateTime expire = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

                model.SetApplicationVar(_KEY, list, expire);
                return list;
            }
        }

        #endregion

        #region 取得路徑抬頭
        /// <summary>
        /// 組合路徑_主要
        /// </summary>
        /// <returns></returns>
        public static string GetHeader()
        {
            SessionModel sm = SessionModel.Get();
            var dao = new BaseDAO();
            var FUNCMSTR = "首頁";

            if (sm.LastActionFunc != null)
            {
                var SYSID = sm.LastActionFunc.sysid;
                var MODULES = sm.LastActionFunc.modules;
                var SUBMODULES = sm.LastActionFunc.submodules;
                var PRGID = sm.LastActionFunc.prgid;

                // 第一層名稱
                var firstnm =  GetFUNCM(SYSID," ").prgname;
                // 第一層名稱
                var secnm = GetFUNCM(SYSID, PRGID).prgname;
                // 組字串
                FUNCMSTR = "<a href ='/Login/C102M'> 首頁 </a> / <a href='#'>"+ firstnm + "</a> / <a href = '/"+ PRGID + "'>" + secnm + "</a>";                
            }
            return FUNCMSTR;
        }

        /// <summary>
        /// 取得程式代碼相關物件
        /// </summary>
        /// <param name="SYSID"></param>
        /// <returns></returns>
        public static TblAMFUNCM GetFUNCM(string SYSID, string PRGID, string MODULES = " ", string SUBMODULES = " ")
        {
            SessionModel sm = SessionModel.Get();
            TblAMFUNCM where = new TblAMFUNCM();
            where.sysid = SYSID;
            where.prgid = PRGID;
            var dao = new BaseDAO();
            var AMFUNCM = dao.GetRow(where);

            return AMFUNCM;
        }
        #endregion


    }
}