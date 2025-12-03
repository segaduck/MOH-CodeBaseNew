# Admin User Comparison Report
## Comparing: cccn vs test vs turbo10901

---

## Executive Summary

| Attribute | cccn | test | turbo10901 |
|-----------|------|------|------------|
| **Status** | ✅ Active | ✅ Active | ❌ **DELETED** |
| **Unit Code** | 31 | 8 | 31 |
| **Admin Scope** | 1 | 1 | 1 |
| **Menu Permissions** | 57 active | 9 active | 54 deleted |
| **Password in DB** | ❌ No (AD/SSO) | ❌ No (AD/SSO) | ❌ No |
| **Created Date** | 2024-10-28 | 2023-11-22 | 2020-05-25 |
| **Created By** | turbo10901 | turbo10901 | ccksd |
| **Deleted Date** | N/A | N/A | 2025-11-10 |
| **Deleted By** | N/A | N/A | cccn |

---

## Detailed Comparison

### 1. User: **cccn** (許景能)

**Status:** ✅ Active Super Admin

**Basic Info:**
- Display Name: 許景能
- Email: cccn@mohw.gov.tw
- Phone: 6349
- Unit Code: 31 (likely main admin unit)
- Admin Scope: 1 (full scope)

**Permissions:**
- **Total**: 57 active menus
- **Access Level**: FULL ACCESS (Super Administrator)

**Menu IDs:**
```
1, 2, 3, 4, 5, 6, 7, 8, 10,
111, 112, 113, 121, 122, 123,
131, 132, 133, 134,
141, 142, 143, 144, 145, 146, 147, 148,
151, 152, 153, 154,
161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173,
175, 176, 177, 178, 179, 180,
181, 182, 183, 184,
193,
1101, 1102
```

**History:**
- Created: 2024-10-28 09:33:19 by turbo10901
- Last Updated: 2025-10-09 09:57:41 by cccn (self-update)

**Authentication:** AD/SSO (no password in ADMIN_CDC)

---

### 2. User: **test** (測試帳號)

**Status:** ✅ Active Limited User

**Basic Info:**
- Display Name: 測試帳號 (Test Account)
- Email: ted@turbotech.com.tw
- Phone: 27769993
- Unit Code: **8** (different from cccn!)
- Admin Scope: 1

**Permissions:**
- **Total**: 9 active menus
- **Access Level**: LIMITED ACCESS (Testing/Development account)

**Menu IDs:**
```
4, 5, 6, 141, 143, 144, 151, 152, 169
```

**Permission Analysis:**
- Has **84% FEWER** permissions than cccn (9 vs 57)
- Missing critical menus: 1, 2, 3, 7, 8, 10 (likely core admin functions)
- Access appears limited to specific functional areas

**History:**
- Created: 2023-11-22 14:42:21 by turbo10901
- Last Updated: 2024-11-01 16:25:18 by turbo10901

**Authentication:** AD/SSO (no password in ADMIN_CDC)

---

### 3. User: **turbo10901** (客服專用)

**Status:** ❌ **DELETED** (DEL_MK = 'Y')

**Basic Info:**
- Display Name: 客服專用 (Customer Service Account)
- Email: teresawu@turbotech.com.tw
- Phone: NULL
- Unit Code: 31 (same as cccn)
- Admin Scope: 1

**Permissions:**
- **Total**: 54 permissions (ALL DELETED)
- **Access Level**: Previously HIGH ACCESS, now NONE

**Menu IDs (Before Deletion):**
```
1, 2, 3, 4, 5, 6, 7, 8, 10,
111, 112, 113, 121, 122, 123,
131, 132, 133, 134,
141, 142, 143, 144, 145, 146, 147, 148,
151, 152, 153, 154,
161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173,
175, 176, 177,
181, 182, 183, 184,
193,
1101, 1102
```

**Comparison with cccn:**
- turbo10901 had **54** permissions
- cccn has **57** permissions
- **Missing from turbo10901:** 178, 179, 180 (3 menus)

**History:**
- Created: 2020-05-25 10:27:19 by ccksd (earliest user!)
- Last Updated: 2025-11-10 09:06:28 by cccn
- **DELETED**: 2025-11-10 09:06:28 by cccn ⚠️

**Authentication:** AD/SSO (no password in ADMIN_CDC)

**Note:** This user was deleted very recently (November 10, 2025), possibly because it's a contractor/vendor account (turbotech.com.tw domain) that is no longer needed.

---

## Key Differences Summary

### Unit Code (UNIT_CD)
- **cccn & turbo10901**: Unit 31 (main admin unit)
- **test**: Unit 8 (different department/unit)

### Permission Levels
```
cccn (57)         ████████████████████████████████████████████████████████ SUPER ADMIN
turbo10901 (54)   ██████████████████████████████████████████████████████   FORMER ADMIN (DELETED)
test (9)          █████████                                                LIMITED USER
```

### Missing Permissions Analysis

**test is missing (compared to cccn):**
- Core admin menus: 1, 2, 3, 7, 8, 10
- 100-series: 111-134 (14 menus)
- 140-series: 142, 145, 146, 147, 148
- 150-series: 153, 154
- 160-series: All except 169 (20+ menus)
- 180-series: All (178-184, 193)
- 1100-series: 1101, 1102

**turbo10901 was missing (compared to cccn):**
- Only 3 menus: 178, 179, 180
- These appear to be recently added features that turbo10901 didn't have access to

### Access Profiles

**cccn - Super Administrator:**
- Full system access
- Can manage all modules
- Active and frequently updated

**test - Limited Test Account:**
- Restricted to specific modules
- Different unit assignment (Unit 8)
- Used for testing purposes
- Maintained by turbo10901

**turbo10901 - Former Admin (DELETED):**
- Was a high-privilege vendor/contractor account
- Had nearly full access (54/57 menus)
- Belonged to TurboTech (external vendor)
- Deleted by cccn on 2025-11-10
- Created other admin accounts (cccn, test)

---

## Authentication Methods

**All three users use AD/SSO authentication:**
- No passwords stored in ADMIN_CDC table
- Authentication likely handled by:
  - Active Directory (AD_OU field exists)
  - Single Sign-On (SSO_KEY field exists)
  - Government email domain (@mohw.gov.tw for cccn)
  - External domain (@turbotech.com.tw for test/turbo10901)

---

## Historical Timeline

```
2020-05-25: turbo10901 created by ccksd (TurboTech contractor account)
2023-11-22: test created by turbo10901
2024-10-28: cccn created by turbo10901
2024-11-01: test updated by turbo10901
2025-10-09: cccn self-updated (gained 3 new permissions: 178, 179, 180)
2025-11-10: turbo10901 DELETED by cccn (end of contractor relationship?)
```

---

## Recommendations for proviewadm

Based on this analysis, when creating **proviewadm**, you should consider:

### Option 1: Full Access (Like cccn)
```
Unit Code: 31
Admin Scope: 1
Permissions: 57 menus (including 178, 179, 180)
```
✅ **Recommended if proviewadm needs full administrative access**

### Option 2: Limited Access (Like test)
```
Unit Code: 8
Admin Scope: 1
Permissions: 9 menus (4, 5, 6, 141, 143, 144, 151, 152, 169)
```
✅ **Recommended if proviewadm is for testing/limited operations**

### Option 3: Former Admin Level (Like turbo10901)
```
Unit Code: 31
Admin Scope: 1
Permissions: 54 menus (excluding 178, 179, 180)
```
⚠️ **Not recommended - this was a contractor account**

---

## Security Notes

1. **turbo10901 was a vendor account** (TurboTech) with high privileges
2. **Recently deleted** (2025-11-10) - likely contract ended
3. **test account uses vendor email** (ted@turbotech.com.tw) - should be monitored
4. **cccn is the current primary admin** with full access

### Suggested Actions:
- ✅ **proviewadm should follow cccn's model** (57 permissions)
- ⚠️ Review if **test account still needed** (uses external vendor email)
- ✅ **turbo10901 properly deactivated** (all permissions revoked)

---

## SQL Queries Used for Analysis

```sql
-- Basic info comparison
SELECT ACC_NO, UNIT_CD, ADMIN_SCOPE, NAME, DEL_MK
FROM ADMIN
WHERE ACC_NO IN ('cccn', 'test', 'turbo10901');

-- Permission counts
SELECT ACC_NO,
       COUNT(*) AS TotalPerms,
       SUM(CASE WHEN DEL_MK='N' THEN 1 ELSE 0 END) AS ActivePerms
FROM ADMIN_LEVEL
WHERE ACC_NO IN ('cccn', 'test', 'turbo10901')
GROUP BY ACC_NO;

-- History
SELECT ACC_NO, ADD_TIME, ADD_ACC, DEL_TIME, DEL_ACC
FROM ADMIN
WHERE ACC_NO IN ('cccn', 'test', 'turbo10901');
```
