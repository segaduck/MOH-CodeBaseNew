# Password Encoding Guide for ADMIN_CDC

## Current Situation

The `ADMIN_CDC` table stores passwords in base64-encoded format (44 characters).

**Example from database:**
```
Password: hM/HWhtEeYhQ++1OceG3wkglKQDGMAwo0klDw1b2MzI=
Length: 44 characters
Format: Base64 (ends with =)
```

## Your Password to Encode

**Plain Text:** `Proview@12977341`

## Password Encoding Methods to Try

The application likely uses one of these methods:

### Method 1: SHA256 + Base64 (Most Common)
```sql
-- SQL Server built-in function
SELECT CONVERT(VARCHAR(100),
    HASHBYTES('SHA2_256', 'Proview@12977341'), 2) AS HexHash;

-- Then Base64 encode the result
```

### Method 2: Using SQL Server to Generate Base64
```sql
-- Direct Base64 encoding of SHA256 hash
SELECT CAST('' AS XML).value('xs:base64Binary(sql:column("bin"))', 'VARCHAR(MAX)') AS Base64Hash
FROM (
    SELECT HASHBYTES('SHA2_256', 'Proview@12977341') AS bin
) AS tab;
```

### Method 3: MD5 + Base64 (Legacy)
```sql
SELECT CAST('' AS XML).value('xs:base64Binary(sql:column("bin"))', 'VARCHAR(MAX)') AS Base64Hash
FROM (
    SELECT HASHBYTES('MD5', 'Proview@12977341') AS bin
) AS tab;
```

## Finding the Correct Method

### Option A: Check Application Source Code
Look for password hashing logic in your application code:
- Search for: "ADMIN_CDC", "ACC_PSWD", "password", "hash"
- Common locations:
  - Login/Authentication modules
  - User management/admin creation functions
  - Password reset functions

### Option B: Analyze Existing Passwords
If you have access to a test account where you know both the plain password and the hash in the database, you can reverse-engineer the method.

### Option C: Contact Development Team
The safest option is to ask the original development team or check documentation.

## SQL Commands to Generate Encoded Password

### Try SHA256 + Base64 (Recommended First)

```sql
USE eservice_new;
GO

-- Generate SHA256 hash in Base64
DECLARE @password VARCHAR(100) = 'Proview@12977341';
DECLARE @hash VARBINARY(MAX);
DECLARE @base64 VARCHAR(100);

SET @hash = HASHBYTES('SHA2_256', @password);
SET @base64 = CAST('' AS XML).value('xs:base64Binary(sql:variable("@hash"))', 'VARCHAR(MAX)');

SELECT @base64 AS EncodedPassword, LEN(@base64) AS Length;
GO
```

### Try MD5 + Base64

```sql
DECLARE @password VARCHAR(100) = 'Proview@12977341';
DECLARE @hash VARBINARY(MAX);
DECLARE @base64 VARCHAR(100);

SET @hash = HASHBYTES('MD5', @password);
SET @base64 = CAST('' AS XML).value('xs:base64Binary(sql:variable("@hash"))', 'VARCHAR(MAX)');

SELECT @base64 AS EncodedPassword, LEN(@base64) AS Length;
GO
```

## Steps to Complete Setup

1. **Generate the encoded password** using one of the methods above
2. **Update the SQL script** `add_proviewadm_user.sql`
   - Replace `'REPLACE_WITH_ENCODED_PASSWORD'` with your generated hash
3. **Execute the script** in SSMS or via sqlcmd
4. **Test the login** to verify it works

## Alternative: Update After Creation

If you're unsure about the encoding, you can:

1. Create the user WITHOUT the ADMIN_CDC record first
2. Use the application's "Reset Password" or "Create User" function
3. Let the application generate the correct password hash

## Verification Query

After setting the password, verify it was added:

```sql
SELECT
    ACC_NO,
    ACC_PSWD,
    LEN(ACC_PSWD) AS PasswordLength
FROM ADMIN_CDC
WHERE ACC_NO = 'proviewadm';
```

Expected result: Password should be ~44 characters in base64 format.

## Security Note

The password hashes in the database appear to use a **simple hash without salt**, which is not ideal for security but common in legacy systems. If you're managing this system long-term, consider implementing:
- Salted password hashing
- Modern algorithms (bcrypt, Argon2)
- Password complexity requirements
