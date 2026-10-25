$ErrorActionPreference = "Stop"

Set-Location $PSScriptRoot

dotnet run --project src/Haven.Web/Haven.Web.csproj --launch-profile http

if ($LASTEXITCODE -ne 0) {
    Write-Error "HAVEN không thể khởi động. Vui lòng kiểm tra lỗi ở trên."
}