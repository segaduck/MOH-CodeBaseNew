/*
=====================================================
  ZIPCODE6 資料表建立腳本
  版本: 1.1
  日期: 2024-12-04
  說明: 建立6碼郵遞區號資料表 (含完整地址資料)

  使用方式:
    DEV:    在 Docker SQL Server 執行
    STAGING: 模擬正式環境測試
    PROD:   正式環境部署前需完整備份

  變更記錄:
    v1.1 - 改為儲存完整地址資料 (非唯一郵遞區號)
         - 新增 ID 主鍵, ZIP_CO 改為索引
         - 新增 SCOOP (範圍) 欄位
=====================================================
*/

USE eservice_new;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT '========================================';
PRINT 'ZIPCODE6 Table Creation Script v1.1';
PRINT 'Started: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';

BEGIN TRY
    BEGIN TRANSACTION;

    -- ============================================
    -- Step 1: 建立 Migration Log 表 (如不存在)
    -- ============================================
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DB_MIGRATION_LOG')
    BEGIN
        CREATE TABLE DB_MIGRATION_LOG (
            ID INT IDENTITY(1,1) PRIMARY KEY,
            MIGRATION_NAME NVARCHAR(100) NOT NULL,
            APPLIED_DT DATETIME NOT NULL DEFAULT GETDATE(),
            DESCRIPTION NVARCHAR(500) NULL,
            EXECUTED_BY NVARCHAR(100) NULL DEFAULT SYSTEM_USER
        );
        PRINT '✓ Created DB_MIGRATION_LOG table';
    END

    -- ============================================
    -- Step 2: 檢查是否已執行過此 Migration
    -- ============================================
    IF EXISTS (SELECT 1 FROM DB_MIGRATION_LOG WHERE MIGRATION_NAME = 'ZIPCODE6_CREATE_V1.1')
    BEGIN
        PRINT '⚠ Migration ZIPCODE6_CREATE_V1.1 already applied - skipping';
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- ============================================
    -- Step 3: 如果舊版表存在，先備份並刪除
    -- ============================================
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ZIPCODE6')
    BEGIN
        PRINT '⚠ Existing ZIPCODE6 table found - backing up and dropping...';

        DECLARE @backupTable NVARCHAR(100);
        SET @backupTable = 'ZIPCODE6_OLD_' + FORMAT(GETDATE(), 'yyyyMMdd_HHmmss');

        DECLARE @sql NVARCHAR(MAX);
        SET @sql = 'SELECT * INTO ' + @backupTable + ' FROM ZIPCODE6';
        EXEC sp_executesql @sql;

        DROP TABLE ZIPCODE6;
        PRINT '    ✓ Backed up to ' + @backupTable + ' and dropped old table';
    END

    -- ============================================
    -- Step 4: 建立 ZIPCODE6 資料表 (完整地址版本)
    -- ============================================
    PRINT 'Creating ZIPCODE6 table (full address version)...';

    CREATE TABLE ZIPCODE6 (
        -- 自動遞增主鍵
        ID          INT IDENTITY(1,1) NOT NULL,

        -- 6碼郵遞區號
        ZIP_CO      NVARCHAR(6)   NOT NULL,

        -- 3碼郵遞區號 (對應舊制)
        ZIP3        NVARCHAR(3)   NOT NULL,

        -- 縣市名稱
        CITYNM      NVARCHAR(20)  NOT NULL,

        -- 鄉鎮市區名稱
        TOWNNM      NVARCHAR(20)  NOT NULL,

        -- 路段名稱
        ROADNM      NVARCHAR(100) NULL,

        -- 範圍說明 (如: 全, 單, 雙, 1號至100號)
        SCOOP       NVARCHAR(100) NULL,

        -- 建立時間
        CREATE_DT   DATETIME      NOT NULL DEFAULT GETDATE(),

        -- 更新時間
        UPDATE_DT   DATETIME      NULL,

        -- 主鍵約束
        CONSTRAINT PK_ZIPCODE6 PRIMARY KEY CLUSTERED (ID)
    );

    PRINT '✓ ZIPCODE6 table created';

    -- ============================================
    -- Step 5: 建立索引
    -- ============================================
    -- 主要查詢索引: 依郵遞區號查詢
    CREATE NONCLUSTERED INDEX IX_ZIPCODE6_ZIP_CO
    ON ZIPCODE6(ZIP_CO);
    PRINT '✓ Index IX_ZIPCODE6_ZIP_CO created';

    -- 3碼郵遞區號索引
    CREATE NONCLUSTERED INDEX IX_ZIPCODE6_ZIP3
    ON ZIPCODE6(ZIP3);
    PRINT '✓ Index IX_ZIPCODE6_ZIP3 created';

    -- 縣市索引
    CREATE NONCLUSTERED INDEX IX_ZIPCODE6_CITYNM
    ON ZIPCODE6(CITYNM);
    PRINT '✓ Index IX_ZIPCODE6_CITYNM created';

    -- 縣市+鄉鎮複合索引
    CREATE NONCLUSTERED INDEX IX_ZIPCODE6_CITY_TOWN
    ON ZIPCODE6(CITYNM, TOWNNM);
    PRINT '✓ Index IX_ZIPCODE6_CITY_TOWN created';

    -- 驗證用覆蓋索引 (包含常用查詢欄位)
    CREATE NONCLUSTERED INDEX IX_ZIPCODE6_VALIDATE
    ON ZIPCODE6(ZIP_CO)
    INCLUDE (CITYNM, TOWNNM);
    PRINT '✓ Index IX_ZIPCODE6_VALIDATE created';

    -- ============================================
    -- Step 6: 記錄 Migration
    -- ============================================
    INSERT INTO DB_MIGRATION_LOG (MIGRATION_NAME, DESCRIPTION)
    VALUES ('ZIPCODE6_CREATE_V1.1', N'建立6碼郵遞區號資料表 ZIPCODE6 (含完整地址資料)');

    COMMIT TRANSACTION;

    PRINT '========================================';
    PRINT '✓ Migration completed successfully';
    PRINT 'Finished: ' + CONVERT(VARCHAR, GETDATE(), 120);
    PRINT '========================================';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT '========================================';
    PRINT '✗ Migration FAILED!';
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR);
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR);
    PRINT '========================================';

    THROW;
END CATCH
GO
