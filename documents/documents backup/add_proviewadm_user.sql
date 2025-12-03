-- ============================================
-- Script to Add ADMIN User: proviewadm
-- Same permissions as: cccn
-- ============================================
USE eservice_new;
GO

-- STEP 1: Insert into ADMIN table (main user info)
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
    'proviewadm',           -- ACC_NO (username)
    31,                     -- UNIT_CD (same as cccn)
    1,                      -- ADMIN_SCOPE (same as cccn)
    NULL,                   -- ADMIN_LEVEL
    'Proview Admin',        -- NAME
    '',                     -- TEL
    'proviewadm@example.com', -- MAIL
    NULL,                   -- AD_OU
    NULL,                   -- SSO_KEY
    NULL,                   -- IDN
    GETDATE(),             -- LEVEL_UPD_TIME
    'N',                    -- DEL_MK (not deleted)
    NULL,                   -- DEL_TIME
    NULL,                   -- DEL_FUN_CD
    NULL,                   -- DEL_ACC
    GETDATE(),             -- UPD_TIME
    'ADM-ACC',             -- UPD_FUN_CD
    'proviewadm',          -- UPD_ACC
    GETDATE(),             -- ADD_TIME
    'ADM-ACC',             -- ADD_FUN_CD
    'proviewadm'           -- ADD_ACC
);
GO

-- STEP 2: Insert password into ADMIN_CDC
-- NOTE: The password needs to be properly encoded!
-- Placeholder - YOU MUST REPLACE THIS WITH PROPERLY ENCODED PASSWORD
INSERT INTO ADMIN_CDC (
    ACC_NO,
    ACC_PSWD
) VALUES (
    'proviewadm',
    'REPLACE_WITH_ENCODED_PASSWORD'  -- Base64-encoded password hash
);
GO

-- STEP 3: Grant menu permissions (same as cccn - 57 menu items)
-- Menu IDs: 1,2,3,4,5,6,7,8,10,111,112,113,121,122,123,131,132,133,134,141,142,143,144,145,146,147,148,151,152,153,154,161,162,163,164,165,166,167,168,169,170,171,172,173,175,176,177,178,179,180,181,182,183,184,193,1101,1102

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
GO

-- STEP 4: Verify the user was created
SELECT
    'Verification Results' AS Step,
    'ADMIN table' AS TableName,
    COUNT(*) AS RecordCount
FROM ADMIN
WHERE ACC_NO = 'proviewadm'
UNION ALL
SELECT
    'Verification Results',
    'ADMIN_CDC table',
    COUNT(*)
FROM ADMIN_CDC
WHERE ACC_NO = 'proviewadm'
UNION ALL
SELECT
    'Verification Results',
    'ADMIN_LEVEL table',
    COUNT(*)
FROM ADMIN_LEVEL
WHERE ACC_NO = 'proviewadm' AND DEL_MK = 'N';
GO

-- Query to view the created user
SELECT
    ACC_NO,
    UNIT_CD,
    ADMIN_SCOPE,
    NAME,
    MAIL,
    DEL_MK,
    ADD_TIME
FROM ADMIN
WHERE ACC_NO = 'proviewadm';
GO

PRINT 'User creation script completed!';
PRINT 'IMPORTANT: You must update the password in ADMIN_CDC with the properly encoded value.';
GO
