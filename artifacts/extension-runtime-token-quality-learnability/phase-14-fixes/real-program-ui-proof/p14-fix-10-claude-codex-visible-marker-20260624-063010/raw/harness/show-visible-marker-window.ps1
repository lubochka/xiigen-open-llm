param(
    [Parameter(Mandatory = $true)][string]$HostName,
    [Parameter(Mandatory = $true)][string]$CorrelationId,
    [Parameter(Mandatory = $true)][string]$ResultMarker,
    [Parameter(Mandatory = $true)][string]$ResponsePath,
    [Parameter(Mandatory = $true)][string]$TracePath,
    [int]$HoldSeconds = 90
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$response = Get-Content -Raw -LiteralPath $ResponsePath | ConvertFrom-Json
$trace = Get-Content -Raw -LiteralPath $TracePath | ConvertFrom-Json
$responseSuccess = [bool]$response.Success
$traceCorrelation = [string]$trace.correlation_id
$traceHost = [string]$trace.host_name
$traceExternalAi = [string]$trace.external_ai_used
$traceTraining = [string]$trace.training_started
$status = [string]$response.Status
$sameCorrelation = ($traceCorrelation -eq $CorrelationId)

$form = New-Object System.Windows.Forms.Form
$form.Text = "XIIGen $HostName visible marker hook - $CorrelationId"
$form.Width = 1120
$form.Height = 620
$form.StartPosition = 'CenterScreen'
$form.BackColor = [System.Drawing.Color]::White
$form.TopMost = $true
$form.Font = New-Object System.Drawing.Font('Segoe UI', 12)

$title = New-Object System.Windows.Forms.Label
$title.AutoSize = $true
$title.Font = New-Object System.Drawing.Font('Segoe UI', 22, [System.Drawing.FontStyle]::Bold)
$title.Location = New-Object System.Drawing.Point(24, 22)
$title.Text = "XIIGen $HostName test-only visible marker hook"
$form.Controls.Add($title)

$box = New-Object System.Windows.Forms.TextBox
$box.Multiline = $true
$box.ReadOnly = $true
$box.ScrollBars = 'Vertical'
$box.Font = New-Object System.Drawing.Font('Consolas', 15)
$box.Location = New-Object System.Drawing.Point(28, 82)
$box.Width = 1040
$box.Height = 440
$box.Text = @"
$ResultMarker
correlation_id=$CorrelationId
response_success=$responseSuccess
response_status=$status
same_run_trace=$TracePath
trace_correlation_id=$traceCorrelation
trace_host_name=$traceHost
same_correlation=$sameCorrelation
external_ai_used=$traceExternalAi
training_started=$traceTraining
response_path=$ResponsePath

Scope: test-only visible host marker hook. The window is visible proof only when paired with the same-run request/response/trace above. It is not a packaged host/chat readiness claim.
"@
$form.Controls.Add($box)

$footer = New-Object System.Windows.Forms.Label
$footer.AutoSize = $true
$footer.Location = New-Object System.Drawing.Point(28, 535)
$footer.Text = "Hold window for screenshot; closes automatically after $HoldSeconds seconds."
$form.Controls.Add($footer)

$timer = New-Object System.Windows.Forms.Timer
$timer.Interval = [Math]::Max(1, $HoldSeconds) * 1000
$timer.Add_Tick({ $timer.Stop(); $form.Close() })
$timer.Start()
[void]$form.ShowDialog()