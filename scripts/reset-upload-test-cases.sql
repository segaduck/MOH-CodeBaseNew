-- ============================================
-- Reset Upload Test Cases
-- For Repeated Testing
-- ============================================

-- This script clears uploaded files so you can re-test uploads
-- Patient ID: A123456789

DECLARE @test_idno NVARCHAR(20) = 'A123456789'
DECLARE @affected_rows INT = 0

BEGIN TRY
    BEGIN TRANSACTION

    -- Clear uploaded files from EEC_ApplyDetailPrice
    UPDATE EEC_ApplyDetailPrice
    SET 
        provide_bin = NULL,         -- Clear Base64 file content
        provide_ext = NULL,         -- Clear file extension
        provide_datetime = NULL,    -- Clear upload timestamp
        provide_user_no = NULL,     -- Clear uploader info
        provide_user_name = NULL,
        download_count = 0,         -- Reset download count
        provide_status = '2'        -- Set back to "waiting for upload"
    WHERE user_idno = @test_idno
        AND payed = 'Y'             -- Only paid records
        AND provide_bin IS NOT NULL -- Only already uploaded records

    SET @affected_rows = @@ROWCOUNT

    -- Also clear the upload log if exists
    DELETE FROM EEC_ApplyDetailUploadLog
    WHERE user_idno = @test_idno

    COMMIT TRANSACTION

    PRINT '=========================================='
    PRINT 'Upload Test Cases Reset Complete'
    PRINT '=========================================='
    PRINT 'Patient ID: ' + @test_idno
    PRINT 'Records reset: ' + CAST(@affected_rows AS NVARCHAR)
    PRINT ''
    PRINT 'All uploaded files have been cleared.'
    PRINT 'Status reset to: 待上傳 (Waiting for Upload)'
    PRINT ''
    PRINT 'You can now test uploads again!'
    PRINT '=========================================='

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
    PRINT 'ERROR: ' + ERROR_MESSAGE()
END CATCH
GO

-- Verify reset
SELECT 
    COUNT(*) AS '可上傳記錄數',
    SUM(CASE WHEN provide_bin IS NULL THEN 1 ELSE 0 END) AS '待上傳',
    SUM(CASE WHEN provide_bin IS NOT NULL THEN 1 ELSE 0 END) AS '已上傳'
FROM EEC_ApplyDetailPrice
WHERE user_idno = 'A123456789'
    AND payed = 'Y'
GO
