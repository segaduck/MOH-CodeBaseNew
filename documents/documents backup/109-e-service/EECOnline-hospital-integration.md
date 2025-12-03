# EECOnline 醫院資料交換說明文件

## 系統概述

**EECOnline** (民眾線上申辦電子病歷服務平台) 與多家醫院系統進行資料交換，讓民眾可以線上申請並取得電子病歷。

---

## 整合醫院清單

| 醫院代碼      | 醫院名稱             | 介接方式         | 資料交換類型 |
| ------------- | -------------------- | ---------------- | ------------ |
| `1131010011H` | 亞東紀念醫院         | RESTful API      | 即時         |
| `1317040011H` | 中山醫學大學附設醫院 | SOAP Web Service | 即時         |
| `1317050017H` | 中國醫藥大學附設醫院 | (待實作)         | -            |
| 其他醫院      | 共用 API             | RESTful API      | 即時         |

---

## 資料交換流程

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           民眾申請電子病歷流程                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐          │
│  │  民眾    │     │ EECOnline │     │  醫院端   │     │ 民眾    │          │
│  │  登入    │────▶│  選擇醫院 │────▶│   API    │────▶│ 下載病歷 │          │
│  └──────────┘     └──────────┘     └──────────┘     └──────────┘          │
│                                                                             │
│  Step 1: 身分驗證 (健保卡/自然人憑證)                                       │
│  Step 2: 查詢可申請病歷列表 (A1 API)                                        │
│  Step 3: 民眾選擇病歷項目                                                   │
│  Step 4: 線上繳費                                                           │
│  Step 5: 發送病歷申請 (A2 API)                                              │
│  Step 6: 醫院回傳病歷檔案                                                   │
│  Step 7: 民眾下載病歷 (A4 API 確認已下載)                                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 一、亞東紀念醫院 (Far Eastern Memorial Hospital)

### 程式檔案位置

- `Utils/Hospital_FarEastern_Api.cs` - API 呼叫主程式
- `Utils/Hospital_FarEastern_Code.cs` - Token 產生與 AES 加解密

### API 端點

- **Domain**: `https://emrdmz.femh.org.tw:9001`
- **認證方式**: Token-based (SHA256 + AES 加密)

### API 清單

| API 代號 | 用途           | 方法         | 時機           |
| -------- | -------------- | ------------ | -------------- |
| **A1**   | 查詢可申請病歷 | `Api_A1()`   | 民眾選擇醫院時 |
| **A2-1** | 送出病歷申請   | `Api_A2_1()` | 民眾付款完成後 |
| **A4**   | 確認病歷已下載 | `Api_A4()`   | 民眾下載病歷時 |

### Token 產生流程

```csharp
// 1. 產生 SHA256 Hash
string source = "ZHQuZQRCgk5TW2qE";
string hash = SHA256.GetHash(source);

// 2. 呼叫 API 取得 Public Key
POST https://emrdmz.femh.org.tw:9001/api/KeysController1
Body: { "Pubkey": hash }
Response: { "Pubkey": "..." }

// 3. AES 加密產生最終 Token
string Token = AesCrypto.AesEncrypt(PubKey, AesKey, AesIv);
```

### A1 - 查詢可申請病歷

```csharp
public static IList<Api_A1ResultModel> Api_A1(
    string domainName,  // API 網址
    string idno,        // 身分證字號
    string birth,       // 生日 (yyyyMMdd)
    string ec_sdate,    // 病歷起始日期
    string ec_edate     // 病歷結束日期
)
```

**回傳資料結構：**
| 欄位 | 說明 |
|------|------|
| ec_no | 病歷編號 |
| ec_name | 病歷名稱 |
| ec_price | 費用 |
| ec_date | 就診日期 |
| ec_online | 是否可線上申請 (Y/N) |
| ec_dept | 科別 |
| ec_doctor | 醫師 |

### A2-1 - 送出病歷申請

```csharp
public static IList<Api_A2_1_ResultModel> Api_A2_1(
    string domainName,
    string idno,
    string birth,
    string caseNo,                          // 案件編號
    List<Api_A2_1_ParamsModel> data         // 申請項目清單
)
```

**回傳資料結構：**
| 欄位 | 說明 |
|------|------|
| ec_no | 病歷編號 |
| ec_success | 成功/失敗 |
| ec_reason | 失敗原因 |

### 病歷檔案處理

醫院回傳的病歷為 **AES 加密的 PDF**：

```csharp
// 解密病歷檔案
Byte[] EncryptPDF = File.ReadAllBytes(hisPath);
string Decryptstring = AesCrypto.AesDecrypt(
    Convert.ToBase64String(EncryptPDF),
    "pQ2azF4XY8R4BcgQ",  // AES Key
    "FekVt9gzVUMYdpKC"   // AES IV
);
byte[] DecryptPDFBYTE = Convert.FromBase64String(Decryptstring);
```

---

## 二、中山醫學大學附設醫院 (Chung Shan Medical University Hospital)

### 程式檔案位置

- `Utils/Hospital_csh_Api.cs` - API 呼叫主程式
- `Web References/tw.org.csh.sysint/` - SOAP Web Service 參考

### Web Service 端點

- **URL**: `https://sysint.csh.org.tw/MedRecApply/MedRecApply.asmx`
- **認證方式**: PKey Token

### Web Service 方法

| 方法名稱                                         | 用途           |
| ------------------------------------------------ | -------------- |
| `GetToken(PKey)`                                 | 取得認證 Token |
| `RefreshToken(Token)`                            | 更新 Token     |
| `GetPtEMRList(Token, idno, birth, sdate, edate)` | 查詢可申請病歷 |
| `EMRApply(Token, idno, birth, caseNo, data)`     | 送出病歷申請   |

### 呼叫流程

```csharp
// 1. 取得 Token
ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
tw.org.csh.sysint.MedRecApply wsObj = new MedRecApply();
string token = wsObj.GetToken(PKey);

// 2. 查詢可申請病歷 (A1)
string result = wsObj.GetPtEMRList(token, idno, birth, sdate, edate);
// 回傳 JSON: { "Message": "成功", "Data": [...] }

// 3. 送出病歷申請 (A2-1)
string applyResult = wsObj.EMRApply(token, idno, birth, caseNo, dataJSON);
```

### 日期格式注意

中山醫要求日期格式為 **yyyy/MM/dd** (含斜線)：

```csharp
var date1 = DateTime.ParseExact(birth, "yyyyMMdd", null).ToString("yyyy/MM/dd");
```

---

## 三、中國醫藥大學附設醫院 (China Medical University Hospital)

### 程式檔案位置

- `Utils/Hospital_cmuh_Api.cs`

### 目前狀態

⚠️ **尚未完整實作** - 僅定義了資料模型，API 呼叫邏輯尚未完成。

---

## 四、共用醫院 API (Hospital_Common_Api)

### 程式檔案位置

- `Utils/Hospital_Common_Api.cs`

### 用途

提供標準化的病歷資料交換介面，供符合規格的醫院使用。

### API 端點設定

API 網址儲存於資料庫 `SETUP` 資料表：

```sql
SELECT setup_val FROM SETUP WHERE setup_cd = 'Hospital_Common_Api' AND del_mk = 'N'
```

### API 清單

| API 路徑            | 用途           | 方法                |
| ------------------- | -------------- | ------------------- |
| `/api/login`        | 登入取得 Token | `GetLoginToken()`   |
| `/api/queryindex`   | 查詢病歷索引   | `GetQueryIndex()`   |
| `/api/querycontent` | 下載病歷內容   | `GetQueryContent()` |

### 病歷內容處理流程

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  API 回傳   │     │   Base64    │     │    XSLT     │     │    HTML     │
│  Base64 XML │────▶│   解碼 XML  │────▶│    轉換     │────▶│   顯示      │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
```

### XSL 模板對照

| 病歷類型代碼 | 說明     | XSL 檔案                              |
| ------------ | -------- | ------------------------------------- |
| 121          | 門診病歷 | `121ClinicalMedical門診病歷.xsl`      |
| 114          | 門診用藥 | `114OutPatientMedication門診用藥.xsl` |
| 113          | 血液檢驗 | `113BloodTest血液.xsl`                |
| 004          | 醫療影像 | `ImageReport醫療影像.xsl`             |
| 115          | 出院病摘 | `115Discharge出院病摘.xsl`            |

### 程式碼範例

```csharp
// 1. 登入取得 Token
var token = Hospital_Common_Api.GetLoginToken(loginUser, loginPwd);

// 2. 查詢病歷索引
var indexList = Hospital_Common_Api.GetQueryIndex(patientIdNo, startDate, endDate, token);

// 3. 下載病歷內容 (Base64 XML)
var base64Xml = Hospital_Common_Api.GetQueryContent(token, guid, patientIdNo, accessionNum, hospitalId, templateId);

// 4. 轉換為 HTML 並存入資料庫
Hospital_Common_Api.TransXMLtoHTML(hisType, guid, patientIdNo, accessionNum, hospitalId, templateId);
```

---

## 資料表結構

### EEC_Hospital_Api (醫院 API 設定)

| 欄位            | 說明                  |
| --------------- | --------------------- |
| keyid           | 自動編號              |
| hospital_code   | 醫院代碼              |
| hospital_name   | 醫院名稱              |
| hospital_apikey | API 代號 (A1, A2, A4) |
| hospital_domain | API Domain Name       |

### EEC_ApplyDetailPrice_ApiData (病歷 API 資料)

| 欄位         | 說明                      |
| ------------ | ------------------------- |
| keyid        | 自動編號                  |
| master_keyid | 關聯 EEC_ApplyDetailPrice |
| PatientIdNo  | 病患身分證                |
| HospitalId   | 醫院代碼                  |
| Guid         | 病歷 GUID                 |
| TemplateId   | 模板代碼                  |
| Report_XML   | 病歷 XML (Base64)         |
| Report_HTML  | 轉換後的 HTML             |

---

## 安全性設定

### TLS/SSL 協定

所有 API 呼叫都強制使用 TLS 1.2：

```csharp
ServicePointManager.SecurityProtocol =
    SecurityProtocolType.Ssl3 |
    SecurityProtocolType.Tls |
    SecurityProtocolType.Tls11 |
    SecurityProtocolType.Tls12;
```

### 憑證驗證

某些醫院 API 需要略過憑證驗證 (測試環境)：

```csharp
ServicePointManager.ServerCertificateValidationCallback +=
    (sender, cert, chain, sslPolicyErrors) => true;
```

⚠️ **注意：** 正式環境應移除此設定，啟用完整憑證驗證。

---

## 錯誤處理與通知

### 申請失敗通知

當病歷申請部分失敗時，系統會寄送郵件通知醫院：

```csharp
if (okSuccessNo.ToCount() != apiParamsData.ToCount())
{
    var dataHosp = dao.GetRow(new TblEEC_Hospital() { code = hospital_code });
    if (dataHosp.Email != "")
    {
        MailMessage mail = CommonsServices.NewMail(
            ConfigModel.MailSenderAddr,
            dataHosp.Email,
            "病歷請求發送失敗通知",
            strBody
        );
        CommonsServices.SendMail(mail);
    }
}
```

---

## 部署注意事項

### 1. API 設定

在資料庫 `SETUP` 資料表中設定各醫院 API 網址：

| setup_cd                | 說明                   |
| ----------------------- | ---------------------- |
| Hospital_Common_Api     | 共用 API 網址          |
| Hospital_FarEastern_Api | 亞東病歷檔案存放路徑   |
| Hospital_csh_Api        | 中山醫病歷檔案存放路徑 |

### 2. 醫院 API 設定

在 `EEC_Hospital_Api` 資料表中設定各醫院的 API 端點：

```sql
INSERT INTO EEC_Hospital_Api (hospital_code, hospital_name, hospital_apikey, hospital_domain)
VALUES ('1131010011H', '亞東紀念醫院', 'A1', 'https://emrdmz.femh.org.tw:9001/api/A1');
```

### 3. XSL 模板

確保 `~/Uploads/XSLTemplate/` 目錄下有所需的 XSL 轉換檔案。

### 4. 防火牆設定

開放對以下外部 IP/Port 的連線：

- 亞東醫院: `emrdmz.femh.org.tw:9001`
- 中山醫院: `sysint.csh.org.tw:443`

---

## 更新紀錄

| 日期       | 說明     |
| ---------- | -------- |
| 2025-11-28 | 初版建立 |
