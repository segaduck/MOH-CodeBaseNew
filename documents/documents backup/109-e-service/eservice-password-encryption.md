# e-Service 系統密碼加密說明文件

## 系統概述

**e-Service** (醫事線上申辦系統) 採用多種密碼加密方式，本文件詳細說明各加密方法及其用途。

---

## 加密方式總覽

| 加密方式             | 用途             | 輸出格式    | 長度    |
| -------------------- | ---------------- | ----------- | ------- |
| **SHA256 + Base64**  | 主要登入密碼     | Base64 字串 | 44 字元 |
| **Unix Crypt (DES)** | 忘記密碼臨時密碼 | 可列印字元  | 13 字元 |
| **SHA256 + Hex**     | 備用方式         | 16 進位小寫 | 64 字元 |
| **MD5**              | 其他用途         | 16 進位小寫 | 32 字元 |
| **HMAC-SHA256**      | API 簽章         | 16 進位小寫 | 64 字元 |

---

## 一、主要密碼加密：SHA256 + Base64

### 程式檔案位置

- `Utils/DataUtils.cs`

### 加密方法

```csharp
public static string Crypt256(string textToEncrypt)
{
    SHA256 sha256 = new SHA256CryptoServiceProvider();
    byte[] source = Encoding.Default.GetBytes(textToEncrypt);  // 使用系統預設編碼
    byte[] crypto = sha256.ComputeHash(source);
    string result = Convert.ToBase64String(crypto);  // 轉為 Base64
    return result;
}
```

### 特性說明

| 項目     | 說明                                                    |
| -------- | ------------------------------------------------------- |
| 演算法   | SHA-256 (安全雜湊演算法)                                |
| 編碼方式 | `Encoding.Default` (系統預設編碼，通常為 Big5 或 UTF-8) |
| 輸出格式 | Base64 編碼                                             |
| 輸出長度 | 44 字元 (含結尾的 `=` 填充字元)                         |
| 是否可逆 | ❌ 不可逆 (單向雜湊)                                    |
| 是否加鹽 | ❌ 無鹽值                                               |

### 使用情境

1. **會員登入驗證**

```csharp
// LoginController.cs - 前台登入
userPwd_encry = DataUtils.Crypt256(form.UserPwd);
userInfo = dao.LoginValidate(userId, userPwd_encry);
```

2. **後台管理員登入 (ADMIN_CDC)**

```csharp
// AccountAction.cs - 後台登入
var psd256 = DataUtils.Crypt256(cdcpass);
string sql = @"SELECT ACC_NO FROM ADMIN_CDC WHERE ACC_NO = @ACC_NO AND ACC_PSWD = @ACC_PSWD";
```

3. **密碼重設**

```csharp
// AccountAction.cs - 密碼重設為帳號
DataUtils.AddParameters(cmd, "PSWD", DataUtils.Crypt256(model.ActionId));
```

4. **首次登入檢測** (密碼 = 帳號)

```csharp
if (DataUtils.Crypt256(userInfo.Member.ACC_NO) == userInfo.Member.PSWD)
{
    result.message += "請更新密碼。\n";
}
```

### 範例

| 原始密碼   | SHA256 + Base64 雜湊值                         |
| ---------- | ---------------------------------------------- |
| `123456`   | `jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=` |
| `password` | `XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg=` |
| `admin`    | `jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=` |

---

## 二、備用加密：SHA256 + Hex (BitConverter)

### 加密方法

```csharp
public static string Crypt256BitConverter(string textToEncrypt)
{
    byte[] source = Encoding.Default.GetBytes(textToEncrypt);
    using (SHA256CryptoServiceProvider csp = new SHA256CryptoServiceProvider())
    {
        byte[] hashMessage = csp.ComputeHash(source);
        return BitConverter.ToString(hashMessage).Replace("-", string.Empty).ToLower();
    }
}
```

### 特性說明

| 項目     | 說明            |
| -------- | --------------- |
| 演算法   | SHA-256         |
| 輸出格式 | 16 進位小寫字串 |
| 輸出長度 | 64 字元         |

### 範例

| 原始密碼 | SHA256 + Hex 雜湊值                                                |
| -------- | ------------------------------------------------------------------ |
| `123456` | `8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92` |

---

## 三、臨時密碼：Unix Crypt (DES)

### 程式檔案位置

- `Utils/UnixCrypt.cs`

### 加密方法

```csharp
public static string Crypt(string textToEncrypt)
{
    return UnixCrypt.Crypt(textToEncrypt);
}

public static string Crypt(string encryptionSalt, string textToEncrypt)
{
    return UnixCrypt.Crypt(encryptionSalt, textToEncrypt);
}
```

### 特性說明

| 項目     | 說明                       |
| -------- | -------------------------- |
| 演算法   | Unix DES-based Crypt       |
| 鹽值     | 前 2 字元 (隨機產生或指定) |
| 輸出長度 | 13 字元                    |
| 來源     | Java 移植版本              |

### 使用情境

**忘記密碼 - 產生臨時密碼**

```csharp
// LoginController.cs
string newPassword = DataUtils.Crypt(DateTime.Now.ToString("yyyyMMddHHmmssffffff"));
```

### 範例

| 原始密碼 | 鹽值 | Unix Crypt 雜湊值 |
| -------- | ---- | ----------------- |
| `test`   | `ab` | `ab8OXL0SVX0LA`   |

---

## 四、HMAC-SHA256 (API 簽章)

### 加密方法

```csharp
public static string CryptHMAC256(string textToEncrypt, string key)
{
    var encoding = new System.Text.UTF8Encoding();
    byte[] keyByte = encoding.GetBytes(key);
    byte[] messageBytes = encoding.GetBytes(textToEncrypt);
    using (var hmacSHA256 = new HMACSHA256(keyByte))
    {
        byte[] hashMessage = hmacSHA256.ComputeHash(messageBytes);
        return BitConverter.ToString(hashMessage).Replace("-", "").ToLower();
    }
}
```

### 特性說明

| 項目     | 說明        |
| -------- | ----------- |
| 演算法   | HMAC-SHA256 |
| 編碼方式 | UTF-8       |
| 需要金鑰 | ✅ 是       |
| 輸出格式 | 16 進位小寫 |
| 輸出長度 | 64 字元     |

---

## 五、MD5 (其他用途)

### 加密方法

```csharp
public static string CryptMD5(string str)
{
    MD5 md5 = MD5.Create();
    byte[] ba = Encoding.Default.GetBytes(str);
    byte[] md55 = md5.ComputeHash(ba);
    string STR = "";
    for (int I = 0; I < md55.Length; I++)
    {
        STR += md55[I].ToString("x2").ToLower();
    }
    return STR;
}
```

### 特性說明

| 項目     | 說明                |
| -------- | ------------------- |
| 演算法   | MD5                 |
| 輸出格式 | 16 進位小寫         |
| 輸出長度 | 32 字元             |
| 安全性   | ⚠️ 已不建議用於密碼 |

---

## 資料庫密碼欄位

### MEMBER 資料表 (前台會員)

| 欄位 | 說明     | 加密方式        |
| ---- | -------- | --------------- |
| PSWD | 會員密碼 | SHA256 + Base64 |

### ADMIN_CDC 資料表 (後台管理員)

| 欄位     | 說明       | 加密方式        |
| -------- | ---------- | --------------- |
| ACC_PSWD | 管理員密碼 | SHA256 + Base64 |

---

## 登入驗證流程

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              前台會員登入流程                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   使用者輸入             加密處理              資料庫比對                    │
│  ┌──────────┐         ┌──────────┐         ┌──────────┐                    │
│  │ 明文密碼  │ ──────▶ │ SHA256   │ ──────▶ │  MEMBER  │                    │
│  │ "123456" │         │ + Base64 │         │   PSWD   │                    │
│  └──────────┘         └──────────┘         └──────────┘                    │
│                              │                   │                          │
│                              ▼                   ▼                          │
│                     "jZae727K08Ka..."    "jZae727K08Ka..."                  │
│                              │                   │                          │
│                              └───────比對────────┘                          │
│                                      │                                      │
│                               ┌──────┴──────┐                              │
│                               │             │                              │
│                            相同          不同                               │
│                               │             │                              │
│                          登入成功      登入失敗                             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 安全性注意事項

### ⚠️ 目前實作的安全考量

1. **無鹽值 (Salt)**

   - 目前密碼雜湊未使用鹽值
   - 相同密碼會產生相同雜湊值
   - 易受彩虹表攻擊

2. **編碼一致性**

   - 使用 `Encoding.Default` 可能因系統環境不同而產生不同結果
   - 建議統一使用 `Encoding.UTF8`

3. **Unix Crypt**
   - 基於 DES 演算法，已被視為不安全
   - 僅用於產生臨時密碼

### ✅ 建議改進方向

1. 使用 **bcrypt** 或 **Argon2** 取代 SHA256
2. 加入隨機鹽值 (Salt)
3. 實作密碼強度檢查
4. 定期要求更換密碼

---

## 程式碼位置索引

| 檔案                                           | 說明            |
| ---------------------------------------------- | --------------- |
| `Utils/DataUtils.cs`                           | 所有加密方法    |
| `Utils/UnixCrypt.cs`                           | Unix Crypt 實作 |
| `Controllers/LoginController.cs`               | 前台登入驗證    |
| `Areas/BACKMIN/Controllers/LoginController.cs` | 後台登入驗證    |
| `Areas/BACKMIN/Action/AccountAction.cs`        | 帳號密碼管理    |
| `DataLayers/LoginDAO.cs`                       | 登入資料存取    |

---

## 更新紀錄

| 日期       | 說明     |
| ---------- | -------- |
| 2025-11-28 | 初版建立 |
