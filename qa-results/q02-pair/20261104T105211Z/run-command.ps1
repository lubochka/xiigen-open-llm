param([string]$ReceiptDirectory,[string]$Name,[string]$ArgumentsJson)
$ErrorActionPreference = 'Stop'
$qaArgs = ConvertFrom-Json $ArgumentsJson
$qaInfo = [System.Diagnostics.ProcessStartInfo]::new('dotnet')
$qaInfo.WorkingDirectory = 'C:/Projects/llm_mvp_core'
$qaInfo.UseShellExecute = $false
$qaInfo.CreateNoWindow = $true
$qaInfo.RedirectStandardOutput = $true
$qaInfo.RedirectStandardError = $true
foreach ($qaArg in $qaArgs) { $qaInfo.ArgumentList.Add([string]$qaArg) }
$qaProcess = [System.Diagnostics.Process]::new()
$qaProcess.StartInfo = $qaInfo
$qaStart = [DateTimeOffset]::UtcNow
$qaWatch = [System.Diagnostics.Stopwatch]::StartNew()
[void]$qaProcess.Start()
$qaPid = $qaProcess.Id
$qaStdout = $qaProcess.StandardOutput.ReadToEndAsync()
$qaStderr = $qaProcess.StandardError.ReadToEndAsync()
$qaProcess.WaitForExit()
$qaWatch.Stop()
$qaOut = $qaStdout.GetAwaiter().GetResult()
$qaErr = $qaStderr.GetAwaiter().GetResult()
$qaUtf8 = [System.Text.UTF8Encoding]::new($false)
[IO.File]::WriteAllText((Join-Path $ReceiptDirectory ($Name+'.stdout.log')), $qaOut, $qaUtf8)
[IO.File]::WriteAllText((Join-Path $ReceiptDirectory ($Name+'.stderr.log')), $qaErr, $qaUtf8)
$qaReceipt = [ordered]@{ executable='dotnet'; arguments=$qaArgs; cwd=$qaInfo.WorkingDirectory; pid=$qaPid; start_utc=$qaStart.ToString('o'); end_utc=[DateTimeOffset]::UtcNow.ToString('o'); elapsed_ms=$qaWatch.Elapsed.TotalMilliseconds; exit_code=$qaProcess.ExitCode; process_cpu_ms=$qaProcess.TotalProcessorTime.TotalMilliseconds; peak_working_set_bytes=$qaProcess.PeakWorkingSet64; stdout=($Name+'.stdout.log'); stderr=($Name+'.stderr.log') }
[IO.File]::WriteAllText((Join-Path $ReceiptDirectory ($Name+'.receipt.json')), (($qaReceipt | ConvertTo-Json -Depth 8)+[char]10), $qaUtf8)
$qaReceipt | ConvertTo-Json -Depth 8
exit $qaProcess.ExitCode
