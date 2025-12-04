/*
=====================================================
  ZIPCODE6 驗證腳本
  版本: 1.0
  日期: 2024-12-04
  說明: 驗證 ZIPCODE6 資料表建立與資料匯入是否成功

  使用方式: 在 SQL Server 執行此腳本
=====================================================
*/

USE eservice_new;
GO

SET NOCOUNT ON;

PRINT '========================================';
PRINT 'ZIPCODE6 Verification Report';
PRINT 'Time: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- ============================================
-- 1. 檢查表是否存在
-- ============================================
PRINT '[1] Table Existence Check';
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ZIPCODE6')
    PRINT '    ✓ Table ZIPCODE6 exists'
ELSE
BEGIN
    PRINT '    ✗ Table ZIPCODE6 NOT FOUND!';
    RETURN;
END

-- ============================================
-- 2. 檢查表結構
-- ============================================
PRINT '';
PRINT '[2] Table Structure';
SELECT
    COLUMN_NAME AS [Column],
    DATA_TYPE AS [Type],
    ISNULL(CAST(CHARACTER_MAXIMUM_LENGTH AS VARCHAR), '-') AS [Length],
    IS_NULLABLE AS [Nullable]
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ZIPCODE6'
ORDER BY ORDINAL_POSITION;

-- ============================================
-- 3. 檢查索引
-- ============================================
PRINT '';
PRINT '[3] Index Check';
SELECT
    i.name AS [Index Name],
    CASE WHEN i.is_primary_key = 1 THEN 'PK' ELSE 'IX' END AS [Type],
    STRING_AGG(c.name, ', ') AS [Columns]
FROM sys.indexes i
JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('ZIPCODE6')
GROUP BY i.name, i.is_primary_key
ORDER BY i.is_primary_key DESC, i.name;

-- ============================================
-- 4. 資料統計
-- ============================================
PRINT '';
PRINT '[4] Data Statistics';

DECLARE @totalCount INT, @distinctZip3 INT, @distinctCity INT;

SELECT @totalCount = COUNT(*) FROM ZIPCODE6;
SELECT @distinctZip3 = COUNT(DISTINCT ZIP3) FROM ZIPCODE6;
SELECT @distinctCity = COUNT(DISTINCT CITYNM) FROM ZIPCODE6;

PRINT '    Total Records:    ' + FORMAT(@totalCount, 'N0');
PRINT '    Distinct ZIP3:    ' + FORMAT(@distinctZip3, 'N0');
PRINT '    Distinct Cities:  ' + FORMAT(@distinctCity, 'N0');

-- 各縣市分布
PRINT '';
PRINT '    Records by City:';
SELECT
    CITYNM AS [City],
    COUNT(*) AS [Count],
    CAST(COUNT(*) * 100.0 / @totalCount AS DECIMAL(5,2)) AS [Percentage]
FROM ZIPCODE6
GROUP BY CITYNM
ORDER BY COUNT(*) DESC;

-- ============================================
-- 5. 抽樣資料檢查
-- ============================================
PRINT '';
PRINT '[5] Sample Data (First 10 records)';
SELECT TOP 10
    ZIP_CO AS [6碼],
    ZIP3 AS [3碼],
    CITYNM AS [縣市],
    TOWNNM AS [鄉鎮市區],
    LEFT(ISNULL(ROADNM, ''), 30) AS [路名範例]
FROM ZIPCODE6
ORDER BY ZIP_CO;

-- ============================================
-- 6. 關鍵郵遞區號驗證
-- ============================================
PRINT '';
PRINT '[6] Key Postal Code Verification';

-- 衛福部地址 115204
IF EXISTS (SELECT 1 FROM ZIPCODE6 WHERE ZIP_CO = '115204')
BEGIN
    SELECT '115204 (衛福部)' AS [Test], ZIP_CO, CITYNM + TOWNNM AS [地址]
    FROM ZIPCODE6 WHERE ZIP_CO = '115204';
    PRINT '    ✓ 115204 exists';
END
ELSE
    PRINT '    ✗ 115204 NOT FOUND!';

-- 台北市中正區
IF EXISTS (SELECT 1 FROM ZIPCODE6 WHERE ZIP_CO = '100001')
BEGIN
    SELECT '100001 (中正區)' AS [Test], ZIP_CO, CITYNM + TOWNNM AS [地址]
    FROM ZIPCODE6 WHERE ZIP_CO = '100001';
    PRINT '    ✓ 100001 exists';
END
ELSE
    PRINT '    ✗ 100001 NOT FOUND!';

-- ============================================
-- 7. 與 ZIPCODE (5碼) 表比對
-- ============================================
PRINT '';
PRINT '[7] Comparison with ZIPCODE (5-digit) table';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ZIPCODE')
BEGIN
    DECLARE @zip5Count INT;
    SELECT @zip5Count = COUNT(*) FROM ZIPCODE;
    PRINT '    ZIPCODE (5-digit) records: ' + FORMAT(@zip5Count, 'N0');
    PRINT '    ZIPCODE6 (6-digit) records: ' + FORMAT(@totalCount, 'N0');
END
ELSE
    PRINT '    ZIPCODE table not found (skipped)';

-- ============================================
-- 8. Migration Log
-- ============================================
PRINT '';
PRINT '[8] Migration History';
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'DB_MIGRATION_LOG')
BEGIN
    SELECT
        MIGRATION_NAME AS [Migration],
        CONVERT(VARCHAR, APPLIED_DT, 120) AS [Applied],
        DESCRIPTION AS [Description]
    FROM DB_MIGRATION_LOG
    WHERE MIGRATION_NAME LIKE 'ZIPCODE6%'
    ORDER BY APPLIED_DT;
END
ELSE
    PRINT '    DB_MIGRATION_LOG table not found';

PRINT '';
PRINT '========================================';
PRINT 'Verification Complete';
PRINT '========================================';
GO
