# PowerShell script to generate password hashes
# Password: Proview@12977341

$password = 'Proview@12977341'

# SHA256 Hash
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$bytes = [System.Text.Encoding]::UTF8.GetBytes($password)
$hash_sha256 = $sha256.ComputeHash($bytes)
$base64_sha256 = [Convert]::ToBase64String($hash_sha256)

# MD5 Hash
$md5 = [System.Security.Cryptography.MD5]::Create()
$hash_md5 = $md5.ComputeHash($bytes)
$base64_md5 = [Convert]::ToBase64String($hash_md5)

# SHA1 Hash (sometimes used)
$sha1 = [System.Security.Cryptography.SHA1]::Create()
$hash_sha1 = $sha1.ComputeHash($bytes)
$base64_sha1 = [Convert]::ToBase64String($hash_sha1)

Write-Host "`nPassword Hash Generation Results" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host "`nOriginal Password: $password"
Write-Host "`n1. SHA256 + Base64:"
Write-Host "   Hash: $base64_sha256"
Write-Host "   Length: $($base64_sha256.Length) characters"
Write-Host "`n2. MD5 + Base64:"
Write-Host "   Hash: $base64_md5"
Write-Host "   Length: $($base64_md5.Length) characters"
Write-Host "`n3. SHA1 + Base64:"
Write-Host "   Hash: $base64_sha1"
Write-Host "   Length: $($base64_sha1.Length) characters"
Write-Host "`nNote: The database passwords are 44 characters long."
Write-Host "SHA256 generates 44 characters, which matches!" -ForegroundColor Yellow
Write-Host "`nRecommended hash to use: SHA256"
Write-Host "SQL Update Command:" -ForegroundColor Cyan
Write-Host "UPDATE ADMIN_CDC SET ACC_PSWD = '$base64_sha256' WHERE ACC_NO = 'proviewadm';" -ForegroundColor White
