-- Generate password hash for: Proview@12977341
USE eservice_new;
GO

SET QUOTED_IDENTIFIER ON;
GO

DECLARE @password VARCHAR(100) = 'Proview@12977341';
DECLARE @hash_sha256 VARBINARY(MAX);
DECLARE @hash_md5 VARBINARY(MAX);
DECLARE @base64_sha256 VARCHAR(100);
DECLARE @base64_md5 VARCHAR(100);

-- Generate SHA256 hash
SET @hash_sha256 = HASHBYTES('SHA2_256', @password);

-- Generate MD5 hash
SET @hash_md5 = HASHBYTES('MD5', @password);

-- Convert to Base64 using XML
SET @base64_sha256 = CAST('' AS XML).value('xs:base64Binary(sql:variable("@hash_sha256"))', 'VARCHAR(MAX)');
SET @base64_md5 = CAST('' AS XML).value('xs:base64Binary(sql:variable("@hash_md5"))', 'VARCHAR(MAX)');

-- Display results
SELECT
    'SHA256' AS Method,
    @base64_sha256 AS EncodedPassword,
    LEN(@base64_sha256) AS Length
UNION ALL
SELECT
    'MD5' AS Method,
    @base64_md5 AS EncodedPassword,
    LEN(@base64_md5) AS Length;
GO
