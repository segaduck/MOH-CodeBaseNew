-- ============================================
-- Script to Add ADMIN User: proviewadm
-- Password: Proview@12977341
-- Same permissions as: cccn
-- ============================================
USE eservice_new;
GO

PRINT '========================================';
PRINT 'Creating Admin User: proviewadm';
PRINT '========================================';
PRINT '';

-- Check if user already exists
IF EXISTS (SELECT 1 FROM ADMIN WHERE ACC_NO = 'proviewadm')
BEGIN
    PRINT 'WARNING: User proviewadm already exists in ADMIN table!';
    PRINT 'Please delete existing user first or use UPDATE statements instead.';
    PRINT '';
END
GO

-- STEP 1: Insert into ADMIN table (main user info)
PRINT 'Step 1: Inserting into ADMIN table...';
INSERT INTO ADMIN (
    ACC_NO,
    UNIT_CD,
    ADMIN_SCOPE,
    ADMIN_LEVEL,
    NAME,
    TEL,
    MAIL,
    AD_OU,
    SSO_KEY,
    IDN,
    LEVEL_UPD_TIME,
    DEL_MK,
    DEL_TIME,
    DEL_FUN_CD,
    DEL_ACC,
    UPD_TIME,
    UPD_FUN_CD,
    UPD_ACC,
    ADD_TIME,
    ADD_FUN_CD,
    ADD_ACC
) VALUES (
    'proviewadm',                    -- ACC_NO (username)
    31,                              -- UNIT_CD (same as cccn)
    1,                               -- ADMIN_SCOPE (same as cccn)
    NULL,                            -- ADMIN_LEVEL
    'Proview Admin',                 -- NAME
    '',                              -- TEL
    'proviewadm@proview.com',        -- MAIL
    NULL,                            -- AD_OU
    NULL,                            -- SSO_KEY
    NULL,                            -- IDN
    GETDATE(),                       -- LEVEL_UPD_TIME
    'N',                             -- DEL_MK (not deleted)
    NULL,                            -- DEL_TIME
    NULL,                            -- DEL_FUN_CD
    NULL,                            -- DEL_ACC
    GETDATE(),                       -- UPD_TIME
    'ADM-ACC',                       -- UPD_FUN_CD
    'proviewadm',                    -- UPD_ACC
    GETDATE(),                       -- ADD_TIME
    'ADM-ACC',                       -- ADD_FUN_CD
    'proviewadm'                     -- ADD_ACC
);
PRINT 'ADMIN table: 1 record inserted';
PRINT '';
GO

-- STEP 2: Insert password into ADMIN_CDC
PRINT 'Step 2: Inserting password into ADMIN_CDC...';
INSERT INTO ADMIN_CDC (
    ACC_NO,
    ACC_PSWD
) VALUES (
    'proviewadm',
    '05VY7kAOW7WyghFr8yduwzDntk6IN8id+tkceynj4MI='  -- SHA256 hash of: Proview@12977341
);
PRINT 'ADMIN_CDC table: Password set (SHA256 hash)';
PRINT '';
GO

-- STEP 3: Grant menu permissions (same as cccn - 57 menu items)
PRINT 'Step 3: Granting menu permissions (57 items)...';

-- Batch insert for better performance
DECLARE @MenuIDs TABLE (MN_ID INT);
INSERT INTO @MenuIDs VALUES
(1),(2),(3),(4),(5),(6),(7),(8),(10),
(111),(112),(113),(121),(122),(123),
(131),(132),(133),(134),
(141),(142),(143),(144),(145),(146),(147),(148),
(151),(152),(153),(154),
(161),(162),(163),(164),(165),(166),(167),(168),(169),(170),(171),(172),(173),(175),(176),(177),(178),(179),(180),
(181),(182),(183),(184),(193),
(1101),(1102);

INSERT INTO ADMIN_LEVEL (
    ACC_NO,
    MN_ID,
    DEL_MK,
    DEL_TIME,
    DEL_FUN_CD,
    DEL_ACC,
    UPD_TIME,
    UPD_FUN_CD,
    UPD_ACC,
    ADD_TIME,
    ADD_FUN_CD,
    ADD_ACC
)
SELECT
    'proviewadm',          -- ACC_NO
    MN_ID,                 -- MN_ID from temp table
    'N',                   -- DEL_MK
    NULL,                  -- DEL_TIME
    NULL,                  -- DEL_FUN_CD
    NULL,                  -- DEL_ACC
    GETDATE(),            -- UPD_TIME
    'ADM-ACC',            -- UPD_FUN_CD
    'proviewadm',         -- UPD_ACC
    GETDATE(),            -- ADD_TIME
    'ADM-ACC',            -- ADD_FUN_CD
    'proviewadm'          -- ADD_ACC
FROM @MenuIDs;

PRINT 'ADMIN_LEVEL table: 57 menu permissions granted';
PRINT '';
GO

-- STEP 4: Verification
PRINT '========================================';
PRINT 'Verification Results:';
PRINT '========================================';

DECLARE @AdminCount INT, @CdcCount INT, @LevelCount INT;

SELECT @AdminCount = COUNT(*) FROM ADMIN WHERE ACC_NO = 'proviewadm';
SELECT @CdcCount = COUNT(*) FROM ADMIN_CDC WHERE ACC_NO = 'proviewadm';
SELECT @LevelCount = COUNT(*) FROM ADMIN_LEVEL WHERE ACC_NO = 'proviewadm' AND DEL_MK = 'N';

PRINT 'Records in ADMIN table: ' + CAST(@AdminCount AS VARCHAR(10));
PRINT 'Records in ADMIN_CDC table: ' + CAST(@CdcCount AS VARCHAR(10));
PRINT 'Records in ADMIN_LEVEL table: ' + CAST(@LevelCount AS VARCHAR(10));
PRINT '';

IF @AdminCount = 1 AND @CdcCount = 1 AND @LevelCount = 57
BEGIN
    PRINT '✓ SUCCESS: User proviewadm created successfully!';
    PRINT '';
    PRINT 'Login Credentials:';
    PRINT '  Username: proviewadm';
    PRINT '  Password: Proview@12977341';
END
ELSE
BEGIN
    PRINT '✗ WARNING: Some records may not have been created correctly.';
    PRINT 'Please review the output above.';
END
PRINT '';
GO

-- Display user details
PRINT 'User Details:';
PRINT '----------------------------------------';
SELECT
    ACC_NO AS [Username],
    UNIT_CD AS [Unit Code],
    ADMIN_SCOPE AS [Admin Scope],
    NAME AS [Display Name],
    MAIL AS [Email],
    DEL_MK AS [Deleted],
    ADD_TIME AS [Created Date]
FROM ADMIN
WHERE ACC_NO = 'proviewadm';
GO

PRINT '';
PRINT '========================================';
PRINT 'Script completed successfully!';
PRINT '========================================';
GO
