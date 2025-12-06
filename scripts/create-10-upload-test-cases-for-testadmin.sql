-- ============================================
-- Create 10 File Upload Test Cases for testadmin
-- For A2/C102M Upload Testing
-- ============================================

-- Purpose: Create multiple test cases that can be uploaded repeatedly
-- User: testadmin (backend hospital admin)
-- Feature: 病歷補上傳 (Medical Record Supplementary Upload)
-- URL: http://localhost:8080/A2/C102M/Index

DECLARE @hospital_code NVARCHAR(20) = '1131010011H'
DECLARE @hospital_name NVARCHAR(100) = N'醫療財團法人徐元智先生醫藥基金會亞東紀念醫院'
DECLARE @test_idno NVARCHAR(20) = 'A123456789'
DECLARE @test_name NVARCHAR(50) = N'測試病患'
DECLARE @test_birthday NVARCHAR(20) = '19900101'
DECLARE @now DATETIME = GETDATE()
DECLARE @now_str NVARCHAR(20) = FORMAT(@now, 'yyyy/MM/dd HH:mm:ss')

BEGIN TRY
    BEGIN TRANSACTION

    PRINT '=========================================='
    PRINT 'Creating 10 Upload Test Cases'
    PRINT '=========================================='

    -- Ensure test user exists
    IF NOT EXISTS (SELECT 1 FROM EEC_User WHERE user_idno = @test_idno)
    BEGIN
        INSERT INTO EEC_User (user_name, user_idno, user_birthday, user_email)
        VALUES (@test_name, @test_idno, @test_birthday, 'test@hospital.com')
        PRINT 'Created test user: ' + @test_name
    END

    -- Create 10 test cases
    DECLARE @i INT = 1
    WHILE @i <= 10
    BEGIN
        DECLARE @apply_no NVARCHAR(50) = FORMAT(@now, 'yyyyMMddHHmmss') + RIGHT('00' + CAST(@i AS NVARCHAR), 3)
        DECLARE @apply_no_sub NVARCHAR(50) = @apply_no + RIGHT(@test_idno, 6) + '001'
        
        -- Create main application
        INSERT INTO EEC_Apply (
            apply_no, 
            user_idno, 
            user_name, 
            user_birthday, 
            login_type, 
            createdatetime
        )
        VALUES (
            @apply_no, 
            @test_idno, 
            @test_name, 
            @test_birthday, 
            '3', 
            @now_str
        )

        -- Create application detail (already paid)
        INSERT INTO EEC_ApplyDetail (
            apply_no, 
            apply_no_sub, 
            user_idno, 
            hospital_code, 
            hospital_name, 
            payed, 
            payed_datetime
        )
        VALUES (
            @apply_no, 
            @apply_no_sub, 
            @test_idno, 
            @hospital_code, 
            @hospital_name, 
            'Y', 
            @now_str
        )

        -- Create price records for different medical record types
        -- Each case will have 2-3 different record types to upload
        
        -- Record Type 1: 門診病歷 (Outpatient Record)
        INSERT INTO EEC_ApplyDetailPrice (
            apply_no, 
            apply_no_sub, 
            user_idno, 
            hospital_code, 
            hospital_name, 
            his_type, 
            his_type_name, 
            price, 
            payed, 
            payed_datetime, 
            provide_status
        )
        VALUES (
            @apply_no, 
            @apply_no_sub, 
            @test_idno, 
            @hospital_code, 
            @hospital_name, 
            '01', 
            N'門診病歷', 
            130, 
            'Y', 
            @now_str, 
            '2'  -- Status 2 = Paid, waiting for upload
        )

        -- Record Type 2: 住院病歷 (Inpatient Record) - for cases 1-5
        IF @i <= 5
        BEGIN
            INSERT INTO EEC_ApplyDetailPrice (
                apply_no, 
                apply_no_sub, 
                user_idno, 
                hospital_code, 
                hospital_name, 
                his_type, 
                his_type_name, 
                price, 
                payed, 
                payed_datetime, 
                provide_status
            )
            VALUES (
                @apply_no, 
                @apply_no_sub, 
                @test_idno, 
                @hospital_code, 
                @hospital_name, 
                '02', 
                N'住院病歷', 
                200, 
                'Y', 
                @now_str, 
                '2'
            )
        END

        -- Record Type 3: 檢驗報告 (Lab Report) - for cases 6-10
        IF @i > 5
        BEGIN
            INSERT INTO EEC_ApplyDetailPrice (
                apply_no, 
                apply_no_sub, 
                user_idno, 
                hospital_code, 
                hospital_name, 
                his_type, 
                his_type_name, 
                price, 
                payed, 
                payed_datetime, 
                provide_status
            )
            VALUES (
                @apply_no, 
                @apply_no_sub, 
                @test_idno, 
                @hospital_code, 
                @hospital_name, 
                '03', 
                N'檢驗報告', 
                150, 
                'Y', 
                @now_str, 
                '2'
            )
        END

        -- Record Type 4: X光報告 (X-Ray) - for odd numbered cases
        IF @i % 2 = 1
        BEGIN
            INSERT INTO EEC_ApplyDetailPrice (
                apply_no, 
                apply_no_sub, 
                user_idno, 
                hospital_code, 
                hospital_name, 
                his_type, 
                his_type_name, 
                price, 
                payed, 
                payed_datetime, 
                provide_status
            )
            VALUES (
                @apply_no, 
                @apply_no_sub, 
                @test_idno, 
                @hospital_code, 
                @hospital_name, 
                '04', 
                N'X光報告', 
                180, 
                'Y', 
                @now_str, 
                '2'
            )
        END

        PRINT 'Created test case #' + CAST(@i AS NVARCHAR) + ': ' + @apply_no_sub

        SET @i = @i + 1
    END

    COMMIT TRANSACTION

    PRINT ''
    PRINT '=========================================='
    PRINT 'SUCCESS: 10 Test Cases Created!'
    PRINT '=========================================='
    PRINT ''
    PRINT 'Test Information:'
    PRINT '  User: testadmin / Test@1234'
    PRINT '  Patient ID: A123456789'
    PRINT '  Patient Name: 測試病患'
    PRINT '  Hospital: 亞東紀念醫院'
    PRINT ''
    PRINT 'To test file upload:'
    PRINT '  1. Login to backend: http://localhost:8080/A2/C102M/Index'
    PRINT '  2. Search for patient: A123456789'
    PRINT '  3. Click 上傳檔案 button on any record'
    PRINT '  4. Upload PDF, JPG, PNG, or other allowed files'
    PRINT ''
    PRINT 'Record Types per Case:'
    PRINT '  - Cases 1-5: 門診病歷 + 住院病歷 + X光報告(odd)'
    PRINT '  - Cases 6-10: 門診病歷 + 檢驗報告 + X光報告(odd)'
    PRINT ''
    PRINT 'Total uploadable items: ~25 records'
    PRINT ''
    PRINT 'Note: Each upload updates provide_bin to Base64,'
    PRINT '      you can re-test by clearing provide_bin column.'
    PRINT '=========================================='

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
    PRINT 'ERROR: ' + ERROR_MESSAGE()
    PRINT 'Line: ' + CAST(ERROR_LINE() AS NVARCHAR)
END CATCH
GO

-- Display created records
PRINT ''
PRINT 'Verifying created records...'
PRINT ''

SELECT 
    TOP 10
    apply_no_sub AS '申請編號',
    his_type AS '類型代碼',
    his_type_name AS '病歷類型',
    price AS '價格',
    payed AS '已付款',
    provide_status AS '狀態',
    CASE 
        WHEN provide_bin IS NULL THEN '待上傳'
        ELSE '已上傳'
    END AS '上傳狀態'
FROM EEC_ApplyDetailPrice
WHERE user_idno = 'A123456789'
    AND provide_status = '2'  -- Only show items waiting for upload
ORDER BY apply_no_sub DESC, his_type
GO
