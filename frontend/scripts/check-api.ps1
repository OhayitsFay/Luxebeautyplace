param([string]$ApiUrl = 'http://127.0.0.1:5117/api')

$ErrorActionPreference = 'Stop'
$config = @{}
Get-Content (Join-Path $PSScriptRoot '../../backend/.env') | ForEach-Object {
    if ($_ -match '^([^#=]+)=(.*)$') { $config[$matches[1].Trim()] = $matches[2].Trim().Trim('"') }
}

function ExpectStatus($Method, $Path, $Body, $Status, $Headers = @{}) {
    try {
        $options = @{ Uri = "$ApiUrl$Path"; Method = $Method; Headers = $Headers; UseBasicParsing = $true }
        if ($null -ne $Body) {
            $options.ContentType = 'application/json'
            $options.Body = ConvertTo-Json $Body -Depth 8
        }
        $response = Invoke-WebRequest @options
        $actual = [int]$response.StatusCode
    } catch {
        if (!$_.Exception.Response) { throw }
        $actual = [int]$_.Exception.Response.StatusCode
    }
    if ($actual -ne $Status) { throw "$Method $Path expected $Status; received $actual" }
    Write-Host "PASS: $Method $Path -> $actual"
}

$health = Invoke-RestMethod "$ApiUrl/health"
if ($health.status -ne 'Healthy') { throw 'Database is not healthy.' }
$login = @{
    email = $config['Admin__DefaultEmail']
    password = $config['Admin__DefaultPassword']
} | ConvertTo-Json
$session = Invoke-RestMethod "$ApiUrl/admin/login" -Method Post -ContentType application/json -Body $login
if (!$session.token -or !$session.expiresAt.EndsWith('Z')) { throw 'Missing token or unambiguous UTC expiry.' }
$headers = @{ Authorization = "Bearer $($session.token)" }
Write-Host 'PASS: configured admin sign-in and UTC session expiry'

foreach ($path in '/services', '/products', '/gallery', '/testimonials', '/admin/setup-status') {
    ExpectStatus Get $path $null 200
}
foreach ($path in '/appointments', '/admin/accounts', '/admin/testimonials') {
    ExpectStatus Get $path $null 401
    ExpectStatus Get $path $null 200 $headers
}
ExpectStatus Post '/admin/login' @{email='';password=''} 400
ExpectStatus Post '/admin/login' @{email=$config['Admin__DefaultEmail'];password='not-the-password'} 401
ExpectStatus Post '/admin/accounts' @{fullName='';email='invalid';phone='';password='short'} 400 $headers
ExpectStatus Post '/admin/forgot-password' @{email=$config['Admin__DefaultEmail'];setupCode='incorrect'} 401
ExpectStatus Post '/services' @{serviceName='Invalid';price=-1;duration=0} 400 $headers
ExpectStatus Post '/appointments' @{serviceId=0} 400
ExpectStatus Post '/testimonials' @{name='';review='';rating=6} 400
ExpectStatus Post '/products' @{productName='Invalid';price=-1;stockQuantity=-1} 400 $headers
ExpectStatus Post '/gallery' @{title='';category='';imageUrl=''} 400 $headers
ExpectStatus Patch '/appointments/0/status/999' $null 400 $headers
ExpectStatus Patch '/testimonials/0/status/999' $null 400 $headers

$services = Invoke-RestMethod "$ApiUrl/services"
foreach ($service in $services) {
    if ($service.imageUrl.StartsWith('/')) {
        $imagePath = Join-Path $PSScriptRoot ("../public/" + $service.imageUrl.TrimStart('/'))
        if (!(Test-Path -LiteralPath $imagePath)) { throw "Missing database service image: $($service.imageUrl)" }
    }
}
Write-Host "PASS: $($services.Count) database service image paths; no records modified."
