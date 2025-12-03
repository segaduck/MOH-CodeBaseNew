# Admin User Creation - Complete Guide

## Summary

Created SQL scripts to add a new admin user `proviewadm` with the same permissions as the existing `cccn` user.

## Schema Analysis Results

### Database Tables Involved

| Table | Purpose | Records to Insert |
|-------|---------|-------------------|
| **ADMIN** | Main admin user information | 1 record |
| **ADMIN_CDC** | Admin credentials (password storage) | 1 record |
| **ADMIN_LEVEL** | Menu permission mappings | 57 records |
| **ADMIN_EPNO** | Employee info (optional) | 0 records (not needed) |

### User Details

**Reference User (cccn):**
- ACC_NO: cccn
- UNIT_CD: 31
- ADMIN_SCOPE: 1
- Menu Permissions: 57 items (MN_IDs: 1-184, 193, 1101-1102)
- Authentication: AD/SSO (no password in ADMIN_CDC)

**New User (proviewadm):**
- ACC_NO: proviewadm
- UNIT_CD: 31 (same as cccn)
- ADMIN_SCOPE: 1 (same as cccn)
- Password: Proview@12977341 (SHA256 hashed)
- Menu Permissions: 57 items (identical to cccn)
- Authentication: Database (password stored in ADMIN_CDC)

## Password Encoding Discovery

### Analysis
- Database uses **SHA256 + Base64** encoding
- All passwords in ADMIN_CDC are **44 characters** long
- Example: `hM/HWhtEeYhQ++1OceG3wkglKQDGMAwo0klDw1b2MzI=`

### Generated Hash
```
Original Password: Proview@12977341
Encoding Method: SHA256 → Base64
Generated Hash: 05VY7kAOW7WyghFr8yduwzDntk6IN8id+tkceynj4MI=
Hash Length: 44 characters ✓
```

## Files Created

### 1. `add_proviewadm_user_FINAL.sql` ⭐ **USE THIS**
**Complete ready-to-execute script** with:
- Pre-filled password hash
- All 3 table inserts (ADMIN, ADMIN_CDC, ADMIN_LEVEL)
- Verification checks
- Progress messages

### 2. `generate_password_hash.ps1`
PowerShell script to generate password hashes for different algorithms.

### 3. `password_encoding_guide.md`
Detailed guide on password encoding methods used in the system.

### 4. `add_proviewadm_user.sql`
Template version (with placeholder for password).

## How to Execute

### Option A: Using SSMS (Recommended)

1. **Open SQL Server Management Studio (SSMS)**
2. **Connect to your Docker SQL Server**
   - Server: `localhost,1433`
   - Authentication: SQL Server Authentication
   - Login: `sa`
   - Password: `YourStrong!Passw0rd`
3. **Open the script**
   - File → Open → File
   - Select: `add_proviewadm_user_FINAL.sql`
4. **Execute the script**
   - Press F5 or click "Execute"
5. **Review the output** - should show success messages

### Option B: Using sqlcmd (Command Line)

```bash
# From your Windows machine
sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" -d eservice_new -i "F:\AITest\MOH\add_proviewadm_user_FINAL.sql"
```

### Option C: Using Docker exec

```bash
# Copy file to container first
docker cp add_proviewadm_user_FINAL.sql moh-sqlserver:/tmp/

# Execute in container
docker exec moh-sqlserver bash -c "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -C -d eservice_new -i /tmp/add_proviewadm_user_FINAL.sql"
```

## Verification Queries

After execution, verify the user was created:

```sql
-- Check ADMIN table
SELECT * FROM ADMIN WHERE ACC_NO = 'proviewadm';

-- Check password exists
SELECT ACC_NO, LEN(ACC_PSWD) AS PwdLength
FROM ADMIN_CDC
WHERE ACC_NO = 'proviewadm';

-- Check menu permissions count
SELECT COUNT(*) AS PermissionCount
FROM ADMIN_LEVEL
WHERE ACC_NO = 'proviewadm' AND DEL_MK = 'N';
-- Expected: 57
```

## Login Credentials

Once created, the new admin can login with:

```
Username: proviewadm
Password: Proview@12977341
```

## Menu Permissions Granted (57 items)

The user `proviewadm` will have access to the following menu IDs:

**Main Menus (1-8, 10):**
- 1, 2, 3, 4, 5, 6, 7, 8, 10

**100-Series (11 items):**
- 111, 112, 113, 121, 122, 123, 131, 132, 133, 134

**140-Series (15 items):**
- 141, 142, 143, 144, 145, 146, 147, 148, 151, 152, 153, 154, 161, 162, 163

**160-170 Series (20 items):**
- 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 175, 176, 177, 178, 179, 180

**180-Series (6 items):**
- 181, 182, 183, 184, 193

**1100-Series (2 items):**
- 1101, 1102

## Troubleshooting

### Issue: User already exists
```sql
-- Delete existing user
DELETE FROM ADMIN_LEVEL WHERE ACC_NO = 'proviewadm';
DELETE FROM ADMIN_CDC WHERE ACC_NO = 'proviewadm';
DELETE FROM ADMIN WHERE ACC_NO = 'proviewadm';

-- Then re-run the creation script
```

### Issue: Password doesn't work
The password encoding is **SHA256 + Base64**. If the generated hash doesn't work:
1. Check application source code for password hashing logic
2. Contact the development team
3. Use the application's "Reset Password" feature

### Issue: Login fails with "Invalid username or password"
- Verify the user exists: `SELECT * FROM ADMIN WHERE ACC_NO = 'proviewadm'`
- Verify password exists: `SELECT * FROM ADMIN_CDC WHERE ACC_NO = 'proviewadm'`
- Check if application uses different authentication (AD/SSO)

## Security Notes

1. **Password Storage:** The system uses SHA256 hashing without salt (legacy approach)
2. **Test Environment:** This is appropriate for a testing environment
3. **Production Use:** For production, consider implementing:
   - Salted password hashing
   - Modern algorithms (bcrypt, Argon2)
   - Multi-factor authentication

## Next Steps

1. ✓ Execute `add_proviewadm_user_FINAL.sql`
2. ✓ Verify the user was created (run verification queries)
3. ✓ Test login with the application
4. ✓ Adjust permissions if needed

## Support Files Reference

- **Execution Script:** `add_proviewadm_user_FINAL.sql`
- **Password Generator:** `generate_password_hash.ps1`
- **Password Guide:** `password_encoding_guide.md`
- **Template:** `add_proviewadm_user.sql`
- **SQL Server Setup:** `SQL-SERVER-SETUP.md`
- **Docker Compose:** `docker-compose.yml`
