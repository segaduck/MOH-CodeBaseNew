-- ============================================
-- Create Test User with Comprehensive Cases
-- For EECOnline Electronic Medical Record System
-- ============================================

-- Test User Info:
-- ID Number: A123456789
-- Name: 測試用戶
-- Birthday: 1990/01/01 (format: 19900101)
-- Email: test@example.com

DECLARE @test_idno NVARCHAR(20) = 'A123456789'
DECLARE @test_name NVARCHAR(50) = N'測試用戶'
DECLARE @test_birthday NVARCHAR(20) = '19900101'
DECLARE @test_email NVARCHAR(100) = 'test@example.com'
DECLARE @hospital_code NVARCHAR(20) = '1131010011H'
DECLARE @hospital_name NVARCHAR(100) = N'醫療財團法人徐元智先生醫藥基金會亞東紀念醫院'
DECLARE @now DATETIME = GETDATE()
-- Format datetime as string for NVARCHAR columns (createdatetime expects 'yyyy/MM/dd HH:mm:ss' format)
DECLARE @now_str NVARCHAR(20) = FORMAT(@now, 'yyyy/MM/dd HH:mm:ss')

-- Clean up existing test data (optional - uncomment if needed)
-- DELETE FROM EEC_ApplyDetailPrice WHERE user_idno = @test_idno
-- DELETE FROM EEC_ApplyDetail WHERE user_idno = @test_idno
-- DELETE FROM EEC_Apply WHERE user_idno = @test_idno
-- DELETE FROM EEC_User WHERE user_idno = @test_idno

BEGIN TRY
    BEGIN TRANSACTION

    -- 1. Create EEC_User record (front-end user)
    IF NOT EXISTS (SELECT 1 FROM EEC_User WHERE user_idno = @test_idno)
    BEGIN
        INSERT INTO EEC_User (user_name, user_idno, user_birthday, user_email)
        VALUES (@test_name, @test_idno, @test_birthday, @test_email)
        PRINT 'Created EEC_User record'
    END

    -- 2. Create multiple applications with different statuses
    DECLARE @apply_no1 NVARCHAR(50) = FORMAT(@now, 'yyyyMMddHHmmss') + '001'
    DECLARE @apply_no2 NVARCHAR(50) = FORMAT(DATEADD(DAY, -7, @now), 'yyyyMMddHHmmss') + '002'
    DECLARE @apply_no3 NVARCHAR(50) = FORMAT(DATEADD(DAY, -14, @now), 'yyyyMMddHHmmss') + '003'
    DECLARE @apply_no4 NVARCHAR(50) = FORMAT(DATEADD(DAY, -30, @now), 'yyyyMMddHHmmss') + '004'
    DECLARE @apply_no5 NVARCHAR(50) = FORMAT(DATEADD(DAY, -60, @now), 'yyyyMMddHHmmss') + '005'

    -- Application 1: Today - Pending payment
    -- Note: createdatetime is NVARCHAR, must use FORMAT() to ensure 'yyyy/MM/dd HH:mm:ss' format
    INSERT INTO EEC_Apply (apply_no, user_idno, user_name, user_birthday, login_type, createdatetime)
    VALUES (@apply_no1, @test_idno, @test_name, @test_birthday, '3', @now_str)

    -- Application 2: 7 days ago - Paid, waiting for upload
    INSERT INTO EEC_Apply (apply_no, user_idno, user_name, user_birthday, login_type, createdatetime)
    VALUES (@apply_no2, @test_idno, @test_name, @test_birthday, '3', FORMAT(DATEADD(DAY, -7, @now), 'yyyy/MM/dd HH:mm:ss'))

    -- Application 3: 14 days ago - Paid, uploaded, downloadable
    INSERT INTO EEC_Apply (apply_no, user_idno, user_name, user_birthday, login_type, createdatetime)
    VALUES (@apply_no3, @test_idno, @test_name, @test_birthday, '3', FORMAT(DATEADD(DAY, -14, @now), 'yyyy/MM/dd HH:mm:ss'))

    -- Application 4: 30 days ago - Completed with downloads
    INSERT INTO EEC_Apply (apply_no, user_idno, user_name, user_birthday, login_type, createdatetime)
    VALUES (@apply_no4, @test_idno, @test_name, @test_birthday, '3', FORMAT(DATEADD(DAY, -30, @now), 'yyyy/MM/dd HH:mm:ss'))

    -- Application 5: 60 days ago - Expired/cancelled
    INSERT INTO EEC_Apply (apply_no, user_idno, user_name, user_birthday, login_type, createdatetime)
    VALUES (@apply_no5, @test_idno, @test_name, @test_birthday, '3', FORMAT(DATEADD(DAY, -60, @now), 'yyyy/MM/dd HH:mm:ss'))

    PRINT 'Created 5 EEC_Apply records'

    -- 3. Create EEC_ApplyDetail records
    DECLARE @sub1 NVARCHAR(50) = @apply_no1 + RIGHT(@test_idno, 6) + '001'
    DECLARE @sub2 NVARCHAR(50) = @apply_no2 + RIGHT(@test_idno, 6) + '001'
    DECLARE @sub3 NVARCHAR(50) = @apply_no3 + RIGHT(@test_idno, 6) + '001'
    DECLARE @sub4 NVARCHAR(50) = @apply_no4 + RIGHT(@test_idno, 6) + '001'
    DECLARE @sub5 NVARCHAR(50) = @apply_no5 + RIGHT(@test_idno, 6) + '001'

    -- Detail 1: Pending payment
    INSERT INTO EEC_ApplyDetail (apply_no, apply_no_sub, user_idno, hospital_code, hospital_name, payed)
    VALUES (@apply_no1, @sub1, @test_idno, @hospital_code, @hospital_name, 'N')

    -- Detail 2: Paid
    INSERT INTO EEC_ApplyDetail (apply_no, apply_no_sub, user_idno, hospital_code, hospital_name, payed, payed_datetime)
    VALUES (@apply_no2, @sub2, @test_idno, @hospital_code, @hospital_name, 'Y', FORMAT(DATEADD(DAY, -6, @now), 'yyyy/MM/dd HH:mm:ss'))

    -- Detail 3: Paid
    INSERT INTO EEC_ApplyDetail (apply_no, apply_no_sub, user_idno, hospital_code, hospital_name, payed, payed_datetime)
    VALUES (@apply_no3, @sub3, @test_idno, @hospital_code, @hospital_name, 'Y', FORMAT(DATEADD(DAY, -13, @now), 'yyyy/MM/dd HH:mm:ss'))

    -- Detail 4: Paid
    INSERT INTO EEC_ApplyDetail (apply_no, apply_no_sub, user_idno, hospital_code, hospital_name, payed, payed_datetime)
    VALUES (@apply_no4, @sub4, @test_idno, @hospital_code, @hospital_name, 'Y', FORMAT(DATEADD(DAY, -29, @now), 'yyyy/MM/dd HH:mm:ss'))

    -- Detail 5: Not paid (expired)
    INSERT INTO EEC_ApplyDetail (apply_no, apply_no_sub, user_idno, hospital_code, hospital_name, payed)
    VALUES (@apply_no5, @sub5, @test_idno, @hospital_code, @hospital_name, 'N')

    PRINT 'Created 5 EEC_ApplyDetail records'

    -- 4. Create EEC_ApplyDetailPrice records with various medical record types
    -- Price 1-1: Pending payment - 門診病歷
    INSERT INTO EEC_ApplyDetailPrice (apply_no, apply_no_sub, user_idno, hospital_code, hospital_name, his_type, his_type_name, price, payed, provide_status)
    VALUES (@apply_no1, @sub1, @test_idno, @hospital_code, @hospital_name, '01', N'門診病歷', 130, 'N', NULL)

    -- Price 1-2: Pending payment - 住院病歷
    INSERT INTO EEC_ApplyDetailPrice (apply_no, apply_no_sub, user_idno, hospital_code, hospital_name, his_type, his_type_name, price, payed, provide_status)
    VALUES (@apply_no1, @sub1, @test_idno, @hospital_code, @hospital_name, '02', N'住院病歷', 200, 'N', NULL)

    -- Price 2-1: Paid, waiting upload - 門診病歷
    INSERT INTO EEC_ApplyDetailPrice (apply_no, apply_no_sub, user_idno, hospital_code, hospital_name, his_type, his_type_name, price, payed, payed_datetime, provide_status)
    VALUES (@apply_no2, @sub2, @test_idno, @hospital_code, @hospital_name, '01', N'門診病歷', 130, 'Y', FORMAT(DATEADD(DAY, -6, @now), 'yyyy/MM/dd HH:mm:ss'), '2')

    -- Price 2-2: Paid, waiting upload - 檢驗報告
    INSERT INTO EEC_ApplyDetailPrice (apply_no, apply_no_sub, user_idno, hospital_code, hospital_name, his_type, his_type_name, price, payed, payed_datetime, provide_status)
    VALUES (@apply_no2, @sub2, @test_idno, @hospital_code, @hospital_name, '03', N'檢驗報告', 150, 'Y', FORMAT(DATEADD(DAY, -6, @now), 'yyyy/MM/dd HH:mm:ss'), '2')

    -- Price 3-1: Uploaded, ready for download - 門診病歷
    INSERT INTO EEC_ApplyDetailPrice (apply_no, apply_no_sub, user_idno, hospital_code, hospital_name, his_type, his_type_name, price, payed, payed_datetime, provide_status, provide_datetime, download_count)
    VALUES (@apply_no3, @sub3, @test_idno, @hospital_code, @hospital_name, '01', N'門診病歷', 130, 'Y', FORMAT(DATEADD(DAY, -13, @now), 'yyyy/MM/dd HH:mm:ss'), '3', FORMAT(DATEADD(DAY, -12, @now), 'yyyy/MM/dd HH:mm:ss'), 0)

    -- Price 3-2: Uploaded, ready for download - X光報告
    INSERT INTO EEC_ApplyDetailPrice (apply_no, apply_no_sub, user_idno, hospital_code, hospital_name, his_type, his_type_name, price, payed, payed_datetime, provide_status, provide_datetime, download_count)
    VALUES (@apply_no3, @sub3, @test_idno, @hospital_code, @hospital_name, '04', N'X光報告', 180, 'Y', FORMAT(DATEADD(DAY, -13, @now), 'yyyy/MM/dd HH:mm:ss'), '3', FORMAT(DATEADD(DAY, -12, @now), 'yyyy/MM/dd HH:mm:ss'), 0)

    -- Price 4-1: Downloaded - 門診病歷
    INSERT INTO EEC_ApplyDetailPrice (apply_no, apply_no_sub, user_idno, hospital_code, hospital_name, his_type, his_type_name, price, payed, payed_datetime, provide_status, provide_datetime, download_count)
    VALUES (@apply_no4, @sub4, @test_idno, @hospital_code, @hospital_name, '01', N'門診病歷', 130, 'Y', FORMAT(DATEADD(DAY, -29, @now), 'yyyy/MM/dd HH:mm:ss'), '3', FORMAT(DATEADD(DAY, -28, @now), 'yyyy/MM/dd HH:mm:ss'), 3)

    -- Price 4-2: Downloaded - 住院病歷
    INSERT INTO EEC_ApplyDetailPrice (apply_no, apply_no_sub, user_idno, hospital_code, hospital_name, his_type, his_type_name, price, payed, payed_datetime, provide_status, provide_datetime, download_count)
    VALUES (@apply_no4, @sub4, @test_idno, @hospital_code, @hospital_name, '02', N'住院病歷', 200, 'Y', FORMAT(DATEADD(DAY, -29, @now), 'yyyy/MM/dd HH:mm:ss'), '3', FORMAT(DATEADD(DAY, -28, @now), 'yyyy/MM/dd HH:mm:ss'), 2)

    -- Price 4-3: Downloaded - 腦波報告
    INSERT INTO EEC_ApplyDetailPrice (apply_no, apply_no_sub, user_idno, hospital_code, hospital_name, his_type, his_type_name, price, payed, payed_datetime, provide_status, provide_datetime, download_count)
    VALUES (@apply_no4, @sub4, @test_idno, @hospital_code, @hospital_name, '05', N'腦波報告', 130, 'Y', FORMAT(DATEADD(DAY, -29, @now), 'yyyy/MM/dd HH:mm:ss'), '3', FORMAT(DATEADD(DAY, -28, @now), 'yyyy/MM/dd HH:mm:ss'), 1)

    -- Price 5-1: Expired - 門診病歷
    INSERT INTO EEC_ApplyDetailPrice (apply_no, apply_no_sub, user_idno, hospital_code, hospital_name, his_type, his_type_name, price, payed, provide_status)
    VALUES (@apply_no5, @sub5, @test_idno, @hospital_code, @hospital_name, '01', N'門診病歷', 130, 'N', NULL)

    PRINT 'Created 11 EEC_ApplyDetailPrice records'

    COMMIT TRANSACTION
    PRINT '=========================================='
    PRINT 'Test user created successfully!'
    PRINT '=========================================='
    PRINT 'User ID: A123456789'
    PRINT 'Name: 測試用戶'
    PRINT 'Birthday: 1990/01/01'
    PRINT ''
    PRINT 'Cases created:'
    PRINT '  - 5 Applications (EEC_Apply)'
    PRINT '  - 5 Application Details (EEC_ApplyDetail)'
    PRINT '  - 11 Price/Record Items (EEC_ApplyDetailPrice)'
    PRINT ''
    PRINT 'Statuses covered:'
    PRINT '  - Pending payment (N)'
    PRINT '  - Paid, waiting upload (Y, status=2)'
    PRINT '  - Uploaded, ready for download (Y, status=3)'
    PRINT '  - Downloaded multiple times'
    PRINT '  - Expired/Not paid'
    PRINT ''
    PRINT 'Medical record types:'
    PRINT '  - 門診病歷 (Outpatient)'
    PRINT '  - 住院病歷 (Inpatient)'
    PRINT '  - 檢驗報告 (Lab Report)'
    PRINT '  - X光報告 (X-Ray)'
    PRINT '  - 腦波報告 (EEG Report)'

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
    PRINT 'Error occurred: ' + ERROR_MESSAGE()
END CATCH
