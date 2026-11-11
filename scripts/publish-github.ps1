param(
    [string]$Owner = 'FoveLier',
    [string]$Repo = 'roomtune'
)
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.Encoding]::UTF8
$token = $env:GH_TOKEN
if (-not $token) { throw 'GH_TOKEN is required' }
$root = 'F:\VisualStudio2022\program\RoomTune'
$headers = @{
    Authorization = "token $token"
    Accept = 'application/vnd.github+json'
    'User-Agent' = 'roomtune-lab'
}

function Post-Json([string]$path, $payload) {
    $json = $payload | ConvertTo-Json -Depth 8 -Compress
    $bytes = [Text.Encoding]::UTF8.GetBytes($json)
    return Invoke-RestMethod -Method Post -Uri ('https://api.github.com' + $path) -Headers $headers -Body $bytes -ContentType 'application/json; charset=utf-8'
}

$repoUrl = "https://api.github.com/repos/$Owner/$Repo"
$repository = $null
try {
    $repository = Invoke-RestMethod -Uri $repoUrl -Headers $headers
} catch {
    if ($_.Exception.Response.StatusCode.value__ -ne 404) { throw }
    $repository = Post-Json '/user/repos' @{
        name = $Repo
        private = $false
        description = 'RoomTune acoustic correction PC and Android'
        auto_init = $true
    }
    Start-Sleep -Seconds 2
}

$defaultBranch = $repository.default_branch
$ref = Invoke-RestMethod -Uri "$repoUrl/git/ref/heads/$defaultBranch" -Headers $headers
$parent = $ref.object.sha

$files = @(git -C $root ls-files)
$tree = @()
$index = 0
foreach ($file in $files) {
    $index += 1
    $absolute = Join-Path $root $file
    if (-not (Test-Path $absolute)) { continue }
    $base64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($absolute))
    $blob = Post-Json "/repos/$Owner/$Repo/git/blobs" @{ content = $base64; encoding = 'base64' }
    $tree += @{ path = ($file -replace '\\', '/'); mode = '100644'; type = 'blob'; sha = $blob.sha }
    Write-Host "[$index/$($files.Count)] $file"
}
$treeObject = Post-Json "/repos/$Owner/$Repo/git/trees" @{ tree = $tree }
$commit = Post-Json "/repos/$Owner/$Repo/git/commits" @{
    message = 'RoomTune v1.0.0 双端交付'
    tree = $treeObject.sha
    parents = @($parent)
}
$updateJson = @{ sha = $commit.sha; force = $true } | ConvertTo-Json -Compress
Invoke-RestMethod -Method Patch -Uri "$repoUrl/git/refs/heads/$defaultBranch" -Headers $headers -Body ([Text.Encoding]::UTF8.GetBytes($updateJson)) -ContentType 'application/json; charset=utf-8' | Out-Null
Write-Host "Repo: https://github.com/$Owner/$Repo"
Write-Host "Commit: $($commit.sha)"

$pcZip = Join-Path $root 'artifacts\RoomTune-PC.zip'
$codeZip = Join-Path $root 'artifacts\RoomTune-code.zip'
$apk = Get-ChildItem (Join-Path $root 'artifacts') -Recurse -Filter '*.apk' -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
$release = Post-Json "/repos/$Owner/$Repo/releases" @{
    tag_name = 'v1.0.0'
    name = 'RoomTune v1.0.0 · PC + Android'
    body = 'RoomTune 声居 v1.0.0：PC + Android 双端数字房间声学校正系统。包含 PC WPF 桌面端、Android APK、完整 docs/审查/截图。PC 需 .NET Framework 4.8；手机端为 HBuilderX uni-app Android 构建；AI 解释为可选，核心声学计算在本地确定性完成。'
    draft = $false
    prerelease = $false
}
$assets = @($pcZip, $codeZip)
if ($apk) { $assets += $apk.FullName }
foreach ($asset in $assets) {
    if (-not (Test-Path $asset)) { continue }
    $name = [IO.Path]::GetFileName($asset)
    $uploadUrl = "https://uploads.github.com/repos/$Owner/$Repo/releases/$($release.id)/assets?name=$name"
    Invoke-RestMethod -Method Post -Uri $uploadUrl -Headers $headers -InFile $asset -ContentType 'application/octet-stream' | Out-Null
    Write-Host "Uploaded $name"
}
Write-Host "Release: https://github.com/$Owner/$Repo/releases/tag/v1.0.0"
