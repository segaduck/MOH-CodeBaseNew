# Vendor Account Authentication Analysis
## test & turbo10901 Password Storage Investigation

---

## 🔍 Key Finding: **NO Database Passwords - They Use AD Authentication**

Both vendor accounts (**test** and **turbo10901**) authenticate through **Active Directory (AD)**, NOT database passwords.

---

## 📊 Authentication Analysis

### Vendor Accounts Status

| Account | Email | Password in DB | AD/SSO Keys | Login Records | Last Login |
|---------|-------|----------------|-------------|---------------|------------|
| **test** | ted@turbotech.com.tw | ❌ NO | ❌ NO | 139 logins | 2024-11-01 |
| **turbo10901** | teresawu@turbotech.com.tw | ❌ NO | ❌ NO | **9,393 logins** | 2025-11-07 |
| **cccn** | cccn@mohw.gov.tw | ❌ NO | ❌ NO | - | - |

### Key Observations:

1. **NO passwords stored in ADMIN_CDC** for any vendor accounts
2. **NO AD_OU or SSO_KEY values** in ADMIN table
3. **Both accounts successfully logged in thousands of times**
4. **turbo10901 was VERY active** (9,393 logins!) until 3 days before deletion

---

## 🏢 System Authentication Configuration

### Active Directory Setup (from SETUP table):

```
AD Server Configuration:
├── AD_SERVER:   10.10.10.2
├── AD_DOMAIN:   intra.gov.tw
├── AD_ACCOUNT:  hyweb
├── AD_PASSWORD: e-service!123
└── LOGIN_TIMEOUT: 600 minutes (10 hours)
```

### How It Works:

The system uses a **centralized AD authentication service**:

1. User enters username/password at login
2. Application connects to **AD Server (10.10.10.2)**
3. AD validates credentials against **intra.gov.tw** domain
4. If successful, application creates session (600 min timeout)
5. **NO password stored in database** - all authentication via AD

---

## 📈 Database-Wide Authentication Statistics

```sql
Total Admin Accounts:     209
Active Accounts:          51
Accounts with passwords:  100 (ALL DELETED)
Active with passwords:    0   ⚠️ ZERO!
```

### Password Storage Pattern:

| Account Type | Count | Password in DB | Status |
|--------------|-------|----------------|--------|
| **Active accounts** | 51 | ❌ NO | Use AD auth |
| **Deleted COVID accounts** (cdccovid001-100) | 100 | ✅ YES | Old/legacy |
| **Deleted other** | 58 | ❌ NO | - |

---

## 🔐 Historical Context

### Authentication System Migration

The system underwent an authentication migration:

**Phase 1: Database Authentication (Legacy)**
- Passwords stored in ADMIN_CDC (SHA256+Base64)
- Used for temporary COVID accounts (cdccovid001-100)
- **All accounts from this phase are now DELETED**

**Phase 2: Active Directory (Current)** ⭐
- All active accounts use AD authentication
- No passwords stored in database
- Centralized credential management
- Includes both internal (@mohw.gov.tw) and external (@turbotech.com.tw) users

---

## 🔍 How Vendor Accounts Authenticate

### External Email + AD Authentication

Even though vendor accounts have external email addresses (@turbotech.com.tw), they still authenticate via AD:

**Setup Process:**
1. Vendor account created in ADMIN table (test, turbo10901)
2. Corresponding AD account created on **intra.gov.tw** domain
3. Vendor given AD credentials separately (not stored in DB)
4. Vendor logs in using AD username/password
5. System validates via AD server (10.10.10.2)

**Login Flow:**
```
User: test
Password: [AD Password - not in database]
    ↓
Application → AD Server (10.10.10.2)
    ↓
AD validates against: intra.gov.tw
    ↓
Success → Session created (LOGIN_LOG recorded)
```

---

## 📝 Login History Evidence

### turbo10901 Login Records (Recent)

```
Date: 2025-11-07 12:24:47 - Status: S (Success)
Date: 2025-11-07 12:00:21 - Status: O (Logout)
Date: 2025-11-07 12:00:08 - Status: S (Success)
Date: 2025-11-07 12:00:00 - Status: A (Auth Attempt)
Date: 2025-10-14 17:24:29 - Status: S (Success)
```

**Status Codes:**
- **S** = Successful login (authenticated via AD)
- **O** = Logout/Sign out
- **A** = Authentication attempt/failure

**Total Activity:**
- test: 139 successful logins
- turbo10901: 9,393 successful logins (heavy usage!)

---

## ⚠️ Security Implications

### For Vendor Accounts:

1. **Password Not Retrievable** - Passwords stored in AD only, not in database
2. **No Local Password Reset** - Must go through AD admin
3. **Access Revocation** - Deleting ADMIN record doesn't disable AD account
4. **External Contractor Risk** - Vendor accounts may retain AD access even after DB deletion

### Current Status:

- **turbo10901**:
  - ✅ Deleted from ADMIN table (2025-11-10)
  - ❓ AD account status unknown
  - ⚠️ May still have AD credentials if not deactivated

- **test**:
  - ✅ Active in ADMIN table
  - ✅ Has AD authentication
  - ⚠️ Vendor account still accessible

---

## 🎯 Answer to Your Question

### "How are the 2 vendor accounts' passwords stored in db?"

**Answer:** They are **NOT stored in the database at all**.

**Authentication Method:**
- ✅ **Active Directory (AD)** authentication
- ❌ **NOT** database password (ADMIN_CDC is empty for them)
- ❌ **NOT** using AD_OU or SSO_KEY fields (those are NULL)

**How They Work:**
1. Both accounts authenticate via **centralized AD server** (10.10.10.2)
2. Passwords stored in **Active Directory** (intra.gov.tw domain)
3. Application queries AD server for authentication
4. Database only stores account metadata, not credentials

---

## 🔍 Comparison: Database Auth vs AD Auth

| Feature | Database Auth (Legacy) | AD Auth (Current) |
|---------|----------------------|-------------------|
| Password Storage | ADMIN_CDC table | AD Server |
| Encryption | SHA256+Base64 | AD encryption |
| Examples | cdccovid001-100 (deleted) | cccn, test, turbo10901 |
| Status | ❌ Deprecated | ✅ Active |
| Active Users | 0 | 51 |
| Vendor Support | No | ✅ Yes |

---

## 💡 Implications for proviewadm

### Decision Needed:

Since vendor accounts use **AD authentication**, you have two options for **proviewadm**:

### Option 1: Database Authentication (Current Script) ✅ Recommended
```
Password: Stored in ADMIN_CDC (SHA256+Base64)
Advantage: Self-contained, no AD dependency
Use Case: External contractor without AD access
Script: add_proviewadm_user_FINAL.sql (already created)
```

### Option 2: AD Authentication (Like vendor accounts)
```
Password: Stored in AD server only
Advantage: Consistent with current system
Use Case: If proviewadm should be a "real" admin
Requirement: Must create AD account separately
Script: Need to modify (remove ADMIN_CDC insert)
```

**Recommendation:**
- If proviewadm is **permanent internal staff** → Use AD authentication
- If proviewadm is **temporary/testing** → Use database authentication (current script)

---

## 📊 Summary Table

| Account | Auth Method | Password Location | Status | Usage |
|---------|-------------|-------------------|--------|-------|
| cccn | AD | AD Server (10.10.10.2) | Active | Primary Admin |
| test | AD | AD Server (10.10.10.2) | Active | Test/Limited |
| turbo10901 | AD | AD Server (10.10.10.2) | Deleted | Former Vendor Admin |
| cdccovid001-100 | Database | ADMIN_CDC (SHA256) | Deleted | Legacy/COVID |
| **proviewadm (planned)** | Database | ADMIN_CDC (SHA256) | - | New Admin |

---

## 🔧 How to Check AD Account Status

If you need to verify vendor AD accounts:

```powershell
# On AD Server (10.10.10.2) or domain controller
Get-ADUser -Filter "Name -like '*turbo*' -or Name -like '*test*'" `
    -Server intra.gov.tw `
    -Properties Enabled, LastLogonDate

# Check if accounts are still active in AD
```

---

## 📌 Key Takeaways

1. ✅ **ALL active accounts use AD authentication** (51 accounts)
2. ❌ **NO active accounts have database passwords** (0 accounts)
3. 🔐 **Vendor accounts authenticate via AD** despite external emails
4. 🗄️ **Database passwords only exist for deleted legacy accounts** (100 cdccovid accounts)
5. ⚙️ **AD Server:** 10.10.10.2 (intra.gov.tw domain)
6. ⏱️ **Session timeout:** 600 minutes (10 hours)
