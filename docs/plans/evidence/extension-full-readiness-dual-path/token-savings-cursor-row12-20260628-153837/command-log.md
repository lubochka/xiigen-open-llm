# Command log

Exit 1:
    rg --files | rg "(^|/)(row12-workload-spec\.json|tools/frdp-row12-cursor-token-trace-harness\.ps1)$"
    Strict path regex did not match Windows-style paths; no missing-file claim was made from this.

Exit 0:
    rg --files | rg "row12-workload-spec\.json|frdp-row12-cursor-token-trace-harness\.ps1"
    Located harness and workload spec.

Exit 0:
    Get-ChildItem -LiteralPath .\tools -Force | Where-Object { $_.Name -like '*row12*' -or $_.Name -like '*token*trace*' -or $_.Name -like '*frdp*' } | Select-Object -ExpandProperty FullName
    Confirmed harness path under tools.

Exit 0:
    Get-Content -LiteralPath .\tools\frdp-row12-cursor-token-trace-harness.ps1 ...
    Inspected harness parameters/setup/pair validation path.

Exit 0:
    Get-Content -LiteralPath .\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\row12-workload-spec.json ...
    Inspected workload spec fields.

Exit 0:
    rg -n "dotnet|Invoke|Start-Process|http|OpenAI|api|Send|Run|Build|Remove-Item" .\tools\xiigen-extension-core-api.ps1
    Checked local helper invocation line and no external-AI path markers in the helper text inspected.

Exit 0:

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Projects\llm_mvp_core\tools\frdp-row12-cursor-token-trace-harness.ps1" -CoreRoot "C:\Projects\llm_mvp_core" -SpecPath "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\row12-workload-spec.json" -MeasurementRoot "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\token-savings-cursor-row12-20260628-153837"
 > stdout.log 2> stderr.log
    Ran the bounded row12 harness. Stdout and stderr are saved in this folder.

Exit 0:
    Parsed execution-report.json, pair-validation.json, baseline-trace.json, and adaptive-trace.json with PowerShell ConvertFrom-Json.
    Result: report_status=MEASUREMENT_PACKET_READY, pair_valid=true, pair_errors=0, before_tokens=6209, after_tokens=6209, delta_tokens=0.
