using ES.Areas.Admin.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Services;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Spire.Doc;
using Spire.Doc.Documents;
using ES.Controllers;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_012001Controller : BaseController
    {
        /// <summary>
        /// 012001 檔案應用 秘書室
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="srvid"></param>
        /// <returns></returns>
        public ActionResult Index(string appid, string srvid)
        {
            Apply_012001FormModel model = new Apply_012001FormModel();
            model = GetApply012001Data(appid);
            return View("Index", model); ;
        }

        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Save(Apply_012001FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            result.status = false;
            var ErrorMsg = "";

            if (model.FLOW_CD == "2" && model.FileCheck.TONotNullString() == "")
            {
                ErrorMsg = "請至少選擇一種補件項目 !";
            }
            else if (model.FLOW_CD == "8" && model.NOTE.TONotNullString() == "")
            {
                ErrorMsg = "請輸入退件原因!";
            }

            if (ErrorMsg == "")
            {
                // 存檔
                ErrorMsg = dao.AppendApply012001(model);
            }

            if (ErrorMsg == "")
            {
                result.status = true;
                result.message = "存檔成功 !";
            }
            else { result.message = ErrorMsg; }


            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 套印申請書
        /// </summary>
        /// <param name="APP_ID"></param>
        public void PreviewApplyForm(string APP_ID)
        {
            Apply_012001FormModel vm = new Apply_012001FormModel();
            vm = GetApply012001Data(APP_ID);
            var alFontName = "標楷體";
            var alFontSize = 14;
            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document();
                Section s = doc.AddSection();
                Paragraph para1 = s.AddParagraph();
                para1.AppendText("衛生福利部檔案應用申請書");
                Paragraph para2 = s.AddParagraph();
                para2.AppendText("申請書編號：_______________");
                #region para1 para2 樣式
                //樣式
                ParagraphStyle style1 = new ParagraphStyle(doc);
                style1.Name = "titleStyle";
                style1.CharacterFormat.FontName = alFontName;
                style1.CharacterFormat.FontSize = 18;
                doc.Styles.Add(style1);
                para1.ApplyStyle("titleStyle");
                para1.Format.HorizontalAlignment = HorizontalAlignment.Center;

                ParagraphStyle style2 = new ParagraphStyle(doc);
                style2.Name = "titleStyle2";
                style2.CharacterFormat.FontName = alFontName;
                style2.CharacterFormat.FontSize = alFontSize;
                doc.Styles.Add(style2);
                para2.ApplyStyle("titleStyle2");
                para2.Format.HorizontalAlignment = HorizontalAlignment.Right;
                #endregion

                #region 申請表格
                //加入table
                Spire.Doc.Table sTable = s.AddTable(true);
                var t = vm.APPFIL.Count;
                sTable.ResetCells(8 + t, 4);
                s.Tables[0].Rows[0].Cells[3].Width = 200;
                #region 合併表格
                //合併表格
                sTable.ApplyHorizontalMerge(3, 0, 3);
                sTable.ApplyHorizontalMerge(5 + t, 0, 3);
                sTable.ApplyHorizontalMerge(6 + t, 1, 3);
                sTable.ApplyHorizontalMerge(7 + t, 0, 3);
                #endregion

                #region 基本資料
                //表頭欄位
                Spire.Doc.Fields.TextRange range1 = sTable[0, 0].AddParagraph().AppendText("姓名");
                CharacterFormat(range1, alFontName, alFontSize);
                Spire.Doc.Fields.TextRange range2 = sTable[0, 1].AddParagraph().AppendText("出生年月日");
                CharacterFormat(range2, alFontName, alFontSize);
                Spire.Doc.Fields.TextRange range3 = sTable[0, 2].AddParagraph().AppendText("身分證明文件字號");
                CharacterFormat(range3, alFontName, 12);
                Spire.Doc.Fields.TextRange range4 = sTable[0, 3].AddParagraph().AppendText("地址、連絡電話");
                CharacterFormat(range4, alFontName, alFontSize);

                //申請人姓名
                Spire.Doc.Fields.TextRange range5 = sTable[1, 0].AddParagraph().AppendText("申請人：" + vm.NAME);
                CharacterFormat(range5, alFontName, alFontSize);

                //申請人出生年月日
                Spire.Doc.Fields.TextRange range6 = sTable[1, 1].AddParagraph().AppendText(vm.BIRTHDAY_AD);
                CharacterFormat(range6, alFontName, alFontSize);

                //身分證明文件字號
                Spire.Doc.Fields.TextRange range7 = sTable[1, 2].AddParagraph().AppendText(vm.IDN);
                CharacterFormat(range7, alFontName, alFontSize);

                //地址、連絡電話
                Spire.Doc.Fields.TextRange range8 = sTable[1, 3].AddParagraph().AppendText($"地址：{vm.ADDR_CODE}{vm.ADDR_CODE_ADDR}{vm.ADDR_CODE_DETAIL} \r\n電話：{vm.TEL}");
                CharacterFormat(range8, alFontName, alFontSize);

                #region 代理資料
                var ename = string.Empty; // 代理人姓名
                var erelation = string.Empty; // 代理人關係
                var ebirthday = string.Empty; // 代理人生日
                var eidn = string.Empty; // 代理人身分證
                var eaddr = string.Empty; // 代理人地址
                var etel = string.Empty; // 代理人電話
                var uname = string.Empty; // 法人名稱
                var uaddr = string.Empty; // 法人地址
                var utel = string.Empty; // 法人電話
                // 代理、自辦
                if (vm.APP_ROLE == "1" && vm.A_AGENT == "0")
                {
                    ename = vm.NPIN_E_NAME; // 代理人姓名
                    erelation = vm.NPIN_AE_RELATION; // 代理人關係
                    ebirthday = vm.NPIN_E_BIRTHDAY_AD; // 代理人生日
                    eidn = vm.NPIN_E_IDN; // 代理人身分證
                    eaddr = $"{vm.NPIN_E_ADDR_CODE} {vm.NPIN_E_ADDR_CODE_ADDR}{vm.NPIN_E_ADDR_CODE_DETAIL}"; // 代理人地址
                    etel = vm.NPIN_E_TEL; // 代理人電話
                }
                else if (vm.APP_ROLE == "1" && vm.A_AGENT == "1")
                {
                    uname = vm.LPIN_E_UNIT_NAME; // 法人名稱
                    uaddr = $"{vm.LPIN_E_UNIT_ADDR_CODE} {vm.LPIN_E_UNIT_ADDR_CODE_ADDR}{vm.LPIN_E_UNIT_ADDR_CODE_DETAIL}"; // 法人地址
                    utel = string.Empty; // 法人電話
                }

                //代理人
                Spire.Doc.Fields.TextRange range9 = sTable[2, 0].AddParagraph().AppendText($"※代理人：{ename} \r\n與申請人之關係（{erelation}）\r\n【請併附委任書】");
                CharacterFormat(range9, alFontName, 12);

                // 代理人出生年月日
                Spire.Doc.Fields.TextRange range10 = sTable[2, 1].AddParagraph().AppendText(ebirthday);
                CharacterFormat(range10, alFontName, alFontSize);

                // 代理人身分證明文件字號
                Spire.Doc.Fields.TextRange range11 = sTable[2, 2].AddParagraph().AppendText(eidn);
                CharacterFormat(range11, alFontName, alFontSize);

                // 代理人地址、連絡電話
                Spire.Doc.Fields.TextRange range12 = sTable[2, 3].AddParagraph().AppendText($"地址：{eaddr}\r\n電話：{etel}");
                CharacterFormat(range12, alFontName, alFontSize);

                // 法人、團體、事務所或營業所代理人
                Spire.Doc.Fields.TextRange range13 = sTable[3, 0].AddParagraph().AppendText("※法人、團體、事務所或營業所【請併附登記證明文件影本】\r\n" +
                    $"名稱：{uname}\r\n地址：{uaddr} \r\n電話：{utel}");
                CharacterFormat(range13, alFontName, alFontSize);
                #endregion
                #endregion

                #region 動態表格
                // 申請檔案動態列表表頭
                Spire.Doc.Fields.TextRange range14 = sTable[4, 0].AddParagraph().AppendText("序號");
                CharacterFormat(range14, alFontName, alFontSize);
                Spire.Doc.Fields.TextRange range15 = sTable[4, 1].AddParagraph().AppendText("檔號/公文文號");
                CharacterFormat(range15, alFontName, alFontSize);
                Spire.Doc.Fields.TextRange range16 = sTable[4, 2].AddParagraph().AppendText("檔案名稱內容要旨");
                CharacterFormat(range16, alFontName, alFontSize);
                Spire.Doc.Fields.TextRange range17 = sTable[4, 3].AddParagraph().AppendText("申請項目（可複選）");
                CharacterFormat(range17, alFontName, alFontSize);

                var rowS = 5;
                // 動態申請項目寫入
                if (vm.APPFIL != null && vm.APPFIL.Count > 0)
                {
                    foreach (var item in vm.APPFIL)
                    {
                        var colS = 0;
                        var rangeItem0 = sTable[rowS, colS].AddParagraph().AppendText(item.SEQ_NO.TONotNullString());
                        CharacterFormat(rangeItem0, alFontName, alFontSize);
                        colS++;
                        var rangeItem1 = sTable[rowS, colS].AddParagraph().AppendText(item.FILENUM.TONotNullString());
                        CharacterFormat(rangeItem1, alFontName, alFontSize);
                        colS++;
                        var rangeItem2 = sTable[rowS, colS].AddParagraph().AppendText(item.FILENAME.TONotNullString());
                        CharacterFormat(rangeItem2, alFontName, alFontSize);
                        colS++;
                        var checklist = "□閱覽、抄錄　□複製";
                        if (item.CHECKNO_Lst.Contains("閱覽") && item.CHECKNO_Lst.Contains("複製"))
                        {
                            checklist = "█閱覽、抄錄　█複製";
                        }
                        else if (item.CHECKNO_Lst.Contains("閱覽"))
                        {
                            checklist = "█閱覽、抄錄　□複製";
                        }
                        else if (item.CHECKNO_Lst.Contains("複製"))
                        {
                            checklist = "□閱覽、抄錄　█複製";
                        }
                        var rangeItem3 = sTable[rowS, colS].AddParagraph().AppendText(checklist);
                        CharacterFormat(rangeItem3, alFontName, alFontSize);
                        // 下一列
                        rowS++;
                    }
                }
                #endregion

                #region 申請事由 申請目的
                // ＊使用檔卷原件事由
                Spire.Doc.Fields.TextRange range18 = sTable[rowS, 0].AddParagraph().AppendText($"※序號______有使用檔案原件之必要，事由：{vm.APP_REASON}");
                CharacterFormat(range18, alFontName, alFontSize);
                rowS++;
                // 申請目的
                Spire.Doc.Fields.TextRange range19 = sTable[rowS, 0].AddParagraph().AppendText("申請目的");
                CharacterFormat(range19, alFontName, alFontSize);
                var checkNos = "□歷史考證 □學術研究 □事證稽憑 □業務參考 □權益保障 \r\n□其他（請敘明目的）：";
                if (!string.IsNullOrWhiteSpace(vm.CHECKNO_ITEMS))
                {
                    /* "歷史考證、";"學術研究、";"事證稽憑、";"業務參考、";"權益保障、";"其他(" + item.NOTE.TONotNullString() + ")";*/
                    var checkNoList = vm.CHECKNO_ITEMS.ToSplit('、');
                    foreach (var item in checkNoList)
                    {
                        switch (item)
                        {
                            case "歷史考證":
                                checkNos = checkNos.Replace("□歷史考證", "█歷史考證");
                                break;
                            case "學術研究":
                                checkNos = checkNos.Replace("□學術研究", "█學術研究");
                                break;
                            case "事證稽憑":
                                checkNos = checkNos.Replace("□事證稽憑", "█事證稽憑");
                                break;
                            case "業務參考":
                                checkNos = checkNos.Replace("□業務參考", "█業務參考");
                                break;
                            case "權益保障":
                                checkNos = checkNos.Replace("□權益保障", "█權益保障");
                                break;
                            default:
                                checkNos = checkNos.Replace("□其他（請敘明目的）：", $"█{item}");
                                break;
                        }
                    }
                }
                Spire.Doc.Fields.TextRange range20 = sTable[rowS, 1].AddParagraph().AppendText(checkNos);
                CharacterFormat(range20, alFontName, alFontSize);
                rowS++;
                #endregion

                #region 申請人簽章
                // 申請人簽章
                var signArea = string.Empty;
                signArea += "此致\r\n";
                signArea += "　　　　　衛生福利部\r\n";
                signArea += "　　　　　　　　　　　　　　　　　　　　申請人簽章：本案為線上申辦\r\n";
                signArea += "　　　　　　　　　　　　　　　　　　　※代理人簽章：本案為線上申辦\r\n";
                signArea += $"　　　　　　　　　　　　　　　　　　　　申請日期：{vm.APP_TIME_AD}";
                Spire.Doc.Fields.TextRange range21 = sTable[rowS, 0].AddParagraph().AppendText(signArea);
                CharacterFormat(range21, alFontName, alFontSize);
                #endregion

                #endregion

                #region 頁尾
                Paragraph para13 = s.AddParagraph();
                para13.AppendText("申請檔案應用請詳閱後附填寫須知\r\n");

                Paragraph paral4 = s.AddParagraph();
                paral4.AppendText("填寫須知");

                Paragraph paral5 = s.AddParagraph();
                paral5.AppendText(@"
一、申請應用本部檔案，請詳閱「衛生福利部檔案應用申請作業須知」，並填寫本申請書或以書面載明規定事項。
二、本申請書各欄位請完整填具，標記「※」者，請依需要加填。
三、申請人委任意定代理人代為申請時，應檢具委任書；申請人如係法定代理者，應檢具相關證明文件備供查驗；申請人為法人、團體者，應附登記證明文件影本；申請案件屬個人隱私資料者，並應檢具身分關係證明文件。
四、申請人應依本部所定時間、處所應用檔案，並檢具本部核准通知函及身分證明文件備供查驗；無法依所定時間應用檔案時，應事先告知本部，並另行約定檔案應用之時間。
五、本部檔案應用准駁依檔案法第十八條、政府資訊公開法第十八條、行政程序法第四十六條及其他法令之規定辦理。
六、檔案應用，以提供複製品為原則；有使用原件之必要者，應於申請時記載其事由。
七、應用本部檔案，應依國家發展委員會檔案管理局訂定之「檔案閱覽抄錄複製收費標準」規定繳納費用。
八、填具本申請書或以書面載明規定事項，得以親送、寄送或傳真方式向本部提出申請。
 地址：115204 臺北市南港區忠孝東路六段488號
 電話：(02)8590 - 6548
 傳真：(02)8590 - 6000
");
                #endregion

                #region 樣式

                ParagraphStyle style3 = new ParagraphStyle(doc);
                style3.Name = "titleStyle3";
                style3.CharacterFormat.FontName = alFontName;
                style3.CharacterFormat.FontSize = 14;
                style3.CharacterFormat.Bold = true;
                doc.Styles.Add(style3);
                para13.ApplyStyle("titleStyle3");
                para13.Format.HorizontalAlignment = HorizontalAlignment.Left;
                paral5.ApplyStyle("titleStyle3");
                paral5.Format.HorizontalAlignment = HorizontalAlignment.Left;

                ParagraphStyle style4 = new ParagraphStyle(doc);
                style4.Name = "titleStyle4";
                style4.CharacterFormat.FontName = alFontName;
                style4.CharacterFormat.FontSize = 16;
                style4.CharacterFormat.Bold = true;
                style4.CharacterFormat.UnderlineStyle = UnderlineStyle.Single;
                doc.Styles.Add(style4);
                paral4.ApplyStyle("titleStyle4");
                paral4.Format.HorizontalAlignment = HorizontalAlignment.Center;

                #endregion

                #region 邊界
                Section sec = doc.Sections[0];
                sec.PageSetup.PageSize = PageSize.A4;

                sec.PageSetup.Margins.Top = 71.88f;
                sec.PageSetup.Margins.Bottom = 71.88f;
                sec.PageSetup.Margins.Left = 60.12f;
                sec.PageSetup.Margins.Right = 60.12f;
                #endregion
                doc.SaveToStream(ms, FileFormat.Docx2013);
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=檔案應用申請書.docx");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }

        /// <summary>
        /// 套表列印預設樣式
        /// </summary>
        /// <param name="item"></param>
        /// <param name="FontName">字型</param>
        /// <param name="FontSize">字體大小</param>
        public void CharacterFormat(Spire.Doc.Fields.TextRange item, string FontName, float FontSize)
        {
            item.CharacterFormat.FontName = FontName;
            item.CharacterFormat.FontSize = FontSize;
        }

        public Apply_012001FormModel GetApply012001Data(string appid)
        {
            var APP_ID = appid;
            BackApplyDAO dao = new BackApplyDAO();
            Apply_012001FormModel model = new Apply_012001FormModel();
            model.APPFIL = new List<APPLY_012001_APPFILModel>();

            #region 案件內容
            // 案件基本資訊
            ApplyModel aly = new ApplyModel();
            aly.APP_ID = APP_ID;
            var alydata = dao.GetRow(aly);
            model.APP_ID = APP_ID;
            model.SRV_ID = "012001";
            model.APP_TIME = alydata.APP_TIME;
            model.APP_EXT_DATE = alydata.APP_EXT_DATE;
            model.PRO_ACC = alydata.PRO_ACC;
            model.FLOW_CD = alydata.FLOW_CD;
            model.MOHW_CASE_NO = alydata.MOHW_CASE_NO;
            model.NOTE = alydata.NOTICE_NOTE.TONotNullString();
            TblAPPLY_012001 a010 = new TblAPPLY_012001();
            a010.APP_ID = APP_ID;
            var a010data = dao.GetRow(a010);

            //model.APP_REASON = a010data.APP_REASON;
            model.APP_ROLE = a010data.APP_ROLE;
            model.NAME = a010data.NAME;
            model.BIRTHDAY = a010data.BIRTHDAY;
            model.IDN = a010data.IDN;
            model.ADDR_CODE = a010data.ADDR_CODE;
            model.ADDR = a010data.ADDR;
            model.TEL = a010data.TEL;
            model.MAIL = a010data.MAIL;
            model.A_AGENT = a010data.A_AGENT;
            model.NPIN_E_NAME = a010data.E_NAME;
            model.NPIN_AE_RELATION = a010data.AE_RELATION;
            model.E_BIRTHDAY = a010data.E_BIRTHDAY;
            model.NPIN_E_IDN = a010data.E_IDN;
            model.NPIN_E_ADDR_CODE = a010data.E_ADDR_CODE;
            model.NPIN_E_ADDR = a010data.E_ADDR;
            model.NPIN_E_TEL = a010data.E_TEL;
            model.NPIN_E_MAIL = a010data.E_MAIL;
            model.LPIN_E_UNIT_ADDR_CODE = a010data.E_UNIT_ADDR_CODE;
            model.LPIN_E_UNIT_ADDR = a010data.E_UNIT_ADDR;
            model.LPIN_E_UNIT_NAME = a010data.E_UNIT_NAME;
            model.CHECK_FLAG = a010data.CHECK_FLAG;

            // CHECK 特殊處理
            var CHECKNO_Str = "";
            TblAPPLY_012001_CHK chk = new TblAPPLY_012001_CHK();
            chk.APP_ID = APP_ID;
            chk.TYPE = "0";
            chk.SEQ_NO = 0;
            var afldata = dao.GetRowList(chk);
            if (afldata.ToCount() > 0)
            {
                foreach (var item in afldata)
                {
                    switch (item.CHECKNO)
                    {
                        case "3":
                            CHECKNO_Str += "歷史考證、";
                            break;
                        case "4":
                            CHECKNO_Str += "學術研究、";
                            break;
                        case "5":
                            CHECKNO_Str += "事證稽憑、";
                            break;
                        case "6":
                            CHECKNO_Str += "業務參考、";
                            break;
                        case "7":
                            CHECKNO_Str += "權益保障、";
                            break;
                        case "8":
                            CHECKNO_Str += "其他(" + item.NOTE.TONotNullString() + ")";
                            break;
                    }
                }
            }
            model.CHECKNO_ITEMS = CHECKNO_Str.Substring(CHECKNO_Str.Length - 1, 1) == "、" ? CHECKNO_Str.Substring(0, CHECKNO_Str.Length - 1) : CHECKNO_Str;

            TblAPPLY_012001_APPFIL apl = new TblAPPLY_012001_APPFIL();
            apl.APP_ID = APP_ID;
            var apldata = dao.GetRowList(apl);
            var i = 1;
            CHECKNO_Str = "";
            foreach (var item in apldata)
            {
                APPLY_012001_APPFILModel afl = new APPLY_012001_APPFILModel();
                afl.APP_ID = APP_ID;
                afl.SEQ_NO = item.SEQ_NO.TOInt32();
                afl.NUMCNT = item.NUMCNT;
                afl.FILENAME = item.FILENAME;
                afl.FILENUM = item.FILENUM;
                chk = new TblAPPLY_012001_CHK();
                chk.APP_ID = APP_ID;
                chk.TYPE = "1";
                chk.SEQ_NO = i;
                var aflresult = dao.GetRowList(chk);
                if (aflresult.ToCount() > 0)
                {
                    foreach (var l in aflresult)
                    {
                        switch (l.CHECKNO)
                        {
                            case "1":
                                CHECKNO_Str += "閱覽、抄錄，";
                                break;
                            case "2":
                                CHECKNO_Str += "複製";
                                break;
                        }
                    }
                }
                model.APPFIL.Add(afl);
                model.APPFIL[i - 1].CHECKNO_Lst = CHECKNO_Str.Substring(CHECKNO_Str.Length - 1, 1) == "，" ? CHECKNO_Str.Substring(0, CHECKNO_Str.Length - 1) : CHECKNO_Str;
                CHECKNO_Str = "";
                i++;
            }
            #endregion

            // 取檔案
            model.FileList = dao.GetFileList_012001(model.APP_ID);

            return model;
        }
    }
}
