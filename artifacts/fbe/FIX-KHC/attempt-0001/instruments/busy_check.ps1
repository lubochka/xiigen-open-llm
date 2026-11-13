(Get-CimInstance Win32_Process | Where-Object {
  ($_.Name -eq 'dotnet.exe' -or $_.Name -eq 'vstest.console.exe' -or $_.Name -eq 'testhost.exe') -and
  $_.CommandLine -like '*LlmFromScratch.Tests*' -and
  $_.CommandLine -notlike '*--artifacts-path*' -and
  $_.ProcessId -ne $PID
} | Measure-Object).Count
