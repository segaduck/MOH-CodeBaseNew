-- =============================================
-- 部署日期: 2025-12-04
-- 修改項目: 更新系統操作服務諮詢電話
-- 舊電話: (02)8590-6349
-- 新電話: (02)7730-7378
-- =============================================

USE eservice_new;
GO

-- 更新系統操作服務諮詢電話
UPDATE SETUP 
SET SETUP_VAL = '(02)7730-7378',
    UPD_TIME = GETDATE(),
    UPD_FUN_CD = 'DEPLOY-20251204',
    UPD_ACC = 'SYSTEM'
WHERE SETUP_CD = 'SERVICETEL';
GO

-- 顯示更新後的結果
SELECT SETUP_CD, SETUP_DESC, SETUP_VAL 
FROM SETUP 
WHERE SETUP_CD = 'SERVICETEL';
GO

PRINT '系統操作服務諮詢電話已更新為: (02)7730-7378';
GO

