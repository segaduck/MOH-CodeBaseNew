/*
=====================================================
  ZIPCODE6 Rollback 腳本
  版本: 1.0
  日期: 2024-12-04

  ⚠ 警告: 此腳本會刪除 ZIPCODE6 資料表及其所有資料!

  使用時機:
  - Migration 失敗需要回滾
  - 需要重新執行 Migration
  - 功能需要完全移除

  執行前請確認:
  1. 已備份必要資料
  2. 已確認不會影響正式環境運作
  3. 已獲得主管授權 (正式環境)
=====================================================
*/

USE eservice_new;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT '========================================';
PRINT '⚠ ZIPCODE6 ROLLBACK SCRIPT';
PRINT 'Time: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- 確認提示
PRINT '⚠ WARNING: This script will DELETE the ZIPCODE6 table!';
PRINT '';

-- 在正式環境中，建議加入確認機制
-- DECLARE @confirm VARCHAR(10);
-- SET @confirm = 'NO';  -- 改為 'YES' 才會執行
-- IF @confirm <> 'YES'
-- BEGIN
--     PRINT 'Rollback cancelled. Set @confirm = ''YES'' to proceed.';
--     RETURN;
-- END

BEGIN TRY
    BEGIN TRANSACTION;

    -- ============================================
    -- Step 1: 備份資料 (可選)
    -- ============================================
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ZIPCODE6')
    BEGIN
        DECLARE @backupTable NVARCHAR(100);
        SET @backupTable = 'ZIPCODE6_BACKUP_' + FORMAT(GETDATE(), 'yyyyMMdd_HHmmss');

        PRINT '[Step 1] Creating backup table: ' + @backupTable;

        DECLARE @sql NVARCHAR(MAX);
        SET @sql = 'SELECT * INTO ' + @backupTable + ' FROM ZIPCODE6';
        EXEC sp_executesql @sql;

        DECLARE @backupCount INT;
        SET @sql = 'SELECT @cnt = COUNT(*) FROM ' + @backupTable;
        EXEC sp_executesql @sql, N'@cnt INT OUTPUT', @cnt = @backupCount OUTPUT;

        PRINT '    ✓ Backed up ' + CAST(@backupCount AS VARCHAR) + ' records to ' + @backupTable;
    END

    -- ============================================
    -- Step 2: 刪除索引
    -- ============================================
    PRINT '';
    PRINT '[Step 2] Dropping indexes...';

    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ZIPCODE6_ZIP3' AND object_id = OBJECT_ID('ZIPCODE6'))
    BEGIN
        DROP INDEX IX_ZIPCODE6_ZIP3 ON ZIPCODE6;
        PRINT '    ✓ Dropped IX_ZIPCODE6_ZIP3';
    END

    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ZIPCODE6_CITYNM' AND object_id = OBJECT_ID('ZIPCODE6'))
    BEGIN
        DROP INDEX IX_ZIPCODE6_CITYNM ON ZIPCODE6;
        PRINT '    ✓ Dropped IX_ZIPCODE6_CITYNM';
    END

    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ZIPCODE6_CITY_TOWN' AND object_id = OBJECT_ID('ZIPCODE6'))
    BEGIN
        DROP INDEX IX_ZIPCODE6_CITY_TOWN ON ZIPCODE6;
        PRINT '    ✓ Dropped IX_ZIPCODE6_CITY_TOWN';
    END

    -- ============================================
    -- Step 3: 刪除資料表
    -- ============================================
    PRINT '';
    PRINT '[Step 3] Dropping ZIPCODE6 table...';

    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ZIPCODE6')
    BEGIN
        DROP TABLE ZIPCODE6;
        PRINT '    ✓ ZIPCODE6 table dropped';
    END
    ELSE
    BEGIN
        PRINT '    ⚠ ZIPCODE6 table does not exist - nothing to drop';
    END

    -- ============================================
    -- Step 4: 記錄 Rollback
    -- ============================================
    PRINT '';
    PRINT '[Step 4] Recording rollback...';

    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'DB_MIGRATION_LOG')
    BEGIN
        INSERT INTO DB_MIGRATION_LOG (MIGRATION_NAME, DESCRIPTION)
        VALUES ('ZIPCODE6_ROLLBACK', N'Rolled back ZIPCODE6 table - Backup: ' + @backupTable);
        PRINT '    ✓ Rollback recorded in DB_MIGRATION_LOG';
    END

    COMMIT TRANSACTION;

    PRINT '';
    PRINT '========================================';
    PRINT '✓ Rollback completed successfully';
    PRINT '';
    PRINT 'Notes:';
    PRINT '- Backup table created: ' + @backupTable;
    PRINT '- To restore, run: SELECT * INTO ZIPCODE6 FROM ' + @backupTable;
    PRINT '- To clean up backup: DROP TABLE ' + @backupTable;
    PRINT '========================================';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT '';
    PRINT '========================================';
    PRINT '✗ Rollback FAILED!';
    PRINT 'Error: ' + ERROR_MESSAGE();
    PRINT '========================================';

    THROW;
END CATCH
GO
