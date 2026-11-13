param(
    [switch]$Json
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).Path
$artifactDir = Join-Path $repoRoot "artifacts/extension-runtime-token-quality-learnability/phase-13-qa-docs-no-overclaim"
$script:checks = @()

function Decode-Utf8Text {
    param([string]$Base64Text)

    return [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($Base64Text))
}

$ru = @{
    Phase = Decode-Utf8Text "0KTQsNC30LAgMTMvMTQ6IFFBIGF1dG9tYXRpb24sIGRvY3VtZW50YXRpb24sIGFuZCBuby1vdmVyY2xhaW0gZXZpZGVuY2Uu"
    HumanSummary = Decode-Utf8Text "0KTQsNC30LAgMTMg0LTQtdC70LDQtdGCIGRhc2hib2FyZC9BUEkvc21va2UgZXZpZGVuY2Ug0LDQstGC0L7QvNCw0YLQuNC30LjRgNC+0LLQsNC90L3Ri9C8INC4INC/0L7QvdGP0YLQvdGL0Lwg0LTQu9GPINCb0YPQsdGLLCDQvdC+INC90LUg0LTQvtC60LDQt9GL0LLQsNC10YIg0YTQuNC90LDQu9GM0L3Rg9GOINCz0L7RgtC+0LLQvdC+0YHRgtGMINC/0YDQvtC00YPQutGC0LAu"
    DocRequiredSummary = Decode-Utf8Text "0JIgUGhhc2UgMTMg0LTQvtC60YPQvNC10L3RgtCw0YbQuNC4INC10YHRgtGMINC+0LHRj9C30LDRgtC10LvRjNC90YvQuSDRh9C10LvQvtCy0LXQutC+0YfQuNGC0LDQtdC80YvQuSDRgtC10LrRgdGCLg=="
    DocMissing = Decode-Utf8Text "0J7QsdGP0LfQsNGC0LXQu9GM0L3Ri9C5IGRvY3VtZW50YXRpb24gZmlsZSDQvtGC0YHRg9GC0YHRgtCy0YPQtdGCLg=="
    DashboardQaPassSummary = Decode-Utf8Text "0KHRg9GJ0LXRgdGC0LLRg9GO0YnQsNGPIGRhc2hib2FyZC9BUEkgUUEgYXV0b21hdGlvbiDQv9GA0L7RhdC+0LTQuNGCINC4INC+0YHRgtCw0LXRgtGB0Y8g0LIgZGFzaGJvYXJkLXNjb3BlZCDQs9GA0LDQvdC40YbQtS4="
    DashboardQaMissingSummary = Decode-Utf8Text "RXhpc3RpbmcgQVBJIGRhc2hib2FyZCBRQSBzY3JpcHQg0L7RgtGB0YPRgtGB0YLQstGD0LXRgi4="
    DashboardQaParseFailSummary = Decode-Utf8Text "RXhpc3RpbmcgZGFzaGJvYXJkIFFBIG91dHB1dCDQvdC1INGA0LDQt9C+0LHRgNCw0L0g0LrQsNC6IEpTT04u"
    HostScenarioSummary = Decode-Utf8Text "SG9zdCBzY2VuYXJpbyBwYWNrZXQg0LzQvtC20L3QviDQv9C10YDQtdC/0YDQvtCy0LXRgNC40YLRjCDQsdC10Lcg0L/QvtCy0YLQvtGA0L3QvtCz0L4g0LfQsNC/0YPRgdC60LAgaG9zdDsgcnVudGltZSBwcm9vZiDQvtGB0YLQsNC10YLRgdGPINC+0YLQtNC10LvRjNC90L4g0L7RgiBkYXNoYm9hcmQvQVBJIGV2aWRlbmNlLg=="
    HostScenarioMissingSummary = Decode-Utf8Text "SG9zdCBydW50aW1lIHByb29mIHBhY2tldCDQvtGC0YHRg9GC0YHRgtCy0YPQtdGCLg=="
    StatusBoardSummary = Decode-Utf8Text "0JrQvtC90YLRgNC+0LvRjNC90LDRjyDQtNC+0YHQutCwINC/0L7QutCw0LfRi9Cy0LDQtdGCIDcg0LLRi9C/0L7Qu9C90LXQvdC90YvRhSDQv9C+0LTRiNCw0LPQvtCyIFBoYXNlIDEzINC4INC+0YHRgtCw0LLQu9GP0LXRgiBwYXJlbnQgYm91bmRhcnkg0L7RgtC60YDRi9GC0YvQvC4="
    NoOverclaimSummary = Decode-Utf8Text "Tm8tb3ZlcmNsYWltIHNjYW4g0L/QviDQuNC30LzQtdC90LXQvdC90L7QvNGDIFBoYXNlIDEzINGC0LXQutGB0YLRgyDQvdC1INC90LDRiNC10Lsg0L/QvtC70L7QttC40YLQtdC70YzQvdGL0YUgZm9yYmlkZGVuIHJlYWRpbmVzcyBjbGFpbXMu"
    SelfCheckSummary = Decode-Utf8Text "0KHQsNC80L7Qv9GA0L7QstC10YDQutCwIG5vLW92ZXJjbGFpbSBzY2FuINC+0YLQutC70L7QvdGP0LXRgiDQv9C+0LvQvtC20LjRgtC10LvRjNC90YPRjiBmb3JiaWRkZW4g0YHRgtGA0L7QutGDINC4INGA0LDQt9GA0LXRiNCw0LXRgiDRj9Cy0L3Rg9GOINC+0YLRgNC40YbQsNGC0LXQu9GM0L3Rg9GOINGE0L7RgNC80YPQu9C40YDQvtCy0LrRgy4="
    EvidenceScope = Decode-Utf8Text "RGFzaGJvYXJkL0FQSSBRQSwg0YHRgtCw0YLQuNGH0LXRgdC60LDRjyDQv9C10YDQtdC/0YDQvtCy0LXRgNC60LAgaG9zdCBwYWNrZXRzLCDQtNC+0LrRg9C80LXQvdGC0LDRhtC40Y8g0Lggbm8tb3ZlcmNsYWltIHdvcmRpbmcgb25seS4g0K3RgtC+INC90LUgZmluYWwgcHJvZHVjdCByZWFkaW5lc3MsINC90LUgZnVsbCBob3N0IFVJIHJlYWRpbmVzcywg0L3QtSBmdWxsIHNlbWFudGljIHF1YWxpdHksINC90LUgZnVsbCBsZWFybmFiaWxpdHkvdHJhaW5pbmcgc3VjY2Vzcywg0L3QtSBjaGVja3BvaW50IHByb21vdGlvbiDQuCDQvdC1IGxvY2F0b3IgdXBkYXRlLg=="
    Step1301 = Decode-Utf8Text "0KLQtdC60YPRidC40LkgYm91bmRlZCB3b3JrIG9yZGVyINC+0YIg0JvRg9Cx0Ys6IENvbnRpbnVlIHBsZWFzZS4="
    Step1302 = Decode-Utf8Text "dG9vbHMvcGhhc2UxMy1xYS1kb2NzLW5vLW92ZXJjbGFpbS5wczEg0L/RgNC+0LLQtdGA0Y/QtdGCIGhvc3Qgc2NlbmFyaW8gcGFja2V0czsgdG9vbHMvYXBpLWRhc2hib2FyZC1xYS5wczEg0LTQsNC10YIgZGFzaGJvYXJkL0FQSSBRQSBvdXRwdXQu"
    Step1303 = Decode-Utf8Text "0JTQvtC60YPQvNC10L3RgtGLINC4IHNjcmlwdCDRgNCw0LfQtNC10LvRj9GO0YIgZGFzaGJvYXJkL0FQSSBldmlkZW5jZSDQuCBsaXZlIGhvc3QgcmVhZGluZXNzLg=="
    Step1304 = Decode-Utf8Text "RGFzaGJvYXJkL0FQSS9kZWJ1Zy9ldmlkZW5jZS1zY29wZSBkb2NzINGB0L7QtNC10YDQttCw0YIgUGhhc2UgMTMgdXNhZ2Ug0LggYm91bmRhcmllcy4="
    Step1305 = Decode-Utf8Text "RXZpZGVuY2Utc2NvcGUgZG9jcyDQvtC/0LjRgdGL0LLQsNGO0YIgbG9jYWwgRFBPLCBjb250aW51ZS10cmFpbmluZywgZnJlc2gtbG9hZCwgaGVsZC1vdXQvYWJsYXRpb24g0Lggcm9sbGJhY2sgbGltaXRzLg=="
    Step1306 = Decode-Utf8Text "Tm8tb3ZlcmNsYWltIHNjYW4g0L7RgtC60LvQvtC90Y/QtdGCIGZvcmJpZGRlbiByZWFkaW5lc3Mgd29yZGluZy4="
    Step1307 = Decode-Utf8Text "U2VsZi1yZXZpZXcg0L/RgNC+0LLQtdGA0Y/QtdGCIHNjcmlwdCBxdWFsaXR5LCBkb2NzIHF1YWxpdHksINC+0YLRgdGD0YLRgdGC0LLQuNC1IGhpZGRlbiBkZXRlcm1pbmlzdGljIHNob3J0Y3V0cyDQuCBuby1vdmVyY2xhaW0u"
    Step1308 = Decode-Utf8Text "U3RhdHVzIGJvYXJkINC+0LHQvdC+0LLQu9C10L07IHBhcmVudCByZXZpZXcvY29tbWl0L3N0YXR1cyBib3VuZGFyeSDQvtGB0YLQsNC10YLRgdGPINCy0L3QtSBuby1naXQgc3ViLWFnZW50IHNsaWNlLg=="
    ReportTitle = Decode-Utf8Text "0J7RgtGH0LXRgiBQaGFzZSAxMyBRQS9kb2NzL25vLW92ZXJjbGFpbQ=="
    ReportIntro = Decode-Utf8Text "0JrRgNCw0YLQutC+INC00LvRjyDQm9GD0LHRizog0Y3RgtCwINGE0LDQt9CwINC00LXQu9Cw0LXRgiBkYXNoYm9hcmQvQVBJL3Ntb2tlIGV2aWRlbmNlINCw0LLRgtC+0LzQsNGC0LjQt9C40YDQvtCy0LDQvdC90YvQvCwg0L/QvtC90Y/RgtC90YvQvCDQuCDRh9C10YHRgtC90L4g0L7Qs9GA0LDQvdC40YfQtdC90L3Ri9C8LiDQrdGC0L7RgiBwYWNrZXQg0L3QtSDQtNC+0LrQsNC30YvQstCw0LXRgiDRhNC40L3QsNC70YzQvdGD0Y4g0LPQvtGC0L7QstC90L7RgdGC0Ywg0L/RgNC+0LTRg9C60YLQsC4="
    SliceStatus = Decode-Utf8Text "0KHRgtCw0YLRg9GBIHNsaWNl"
    StepStatusHeading = Decode-Utf8Text "0KHRgtCw0YLRg9GBINGI0LDQs9C+0LI="
    CountersHeading = Decode-Utf8Text "0KHRh9C10YLRh9C40LrQuA=="
    ProvesHeading = Decode-Utf8Text "0KfRgtC+INGN0YLQviDQtNC+0LrQsNC30YvQstCw0LXRgg=="
    NotProvesHeading = Decode-Utf8Text "0KfRgtC+INGN0YLQviDQvdC1INC00L7QutCw0LfRi9Cy0LDQtdGC"
    ChecksHeading = Decode-Utf8Text "0J/RgNC+0LLQtdGA0LrQuA=="
    GitBoundaryHeading = Decode-Utf8Text "R2l0IGJvdW5kYXJ5"
    TableStep = Decode-Utf8Text "0KjQsNCz"
    TableStatus = Decode-Utf8Text "0KHRgtCw0YLRg9GB"
    TableCheck = Decode-Utf8Text "0J/RgNC+0LLQtdGA0LrQsA=="
    TableSummary = Decode-Utf8Text "0JrRgNCw0YLQutC+"
    CounterPhaseDone = Decode-Utf8Text "UGhhc2UgMTMg0YHQtNC10LvQsNC90L46IDcvOC4="
    CounterPhaseProgress = Decode-Utf8Text "UGhhc2UgMTMg0LIg0YDQsNCx0L7RgtC1OiAxMy4wOC8xMy4wOC4="
    CounterPhaseRemaining = Decode-Utf8Text "UGhhc2UgMTMg0LXRidC1INC90LUg0L3QsNGH0LDRgtC+OiAwLzgu"
    CounterPlanDone = Decode-Utf8Text "0JLQtdGB0Ywg0L/Qu9Cw0L0g0YHQtNC10LvQsNC90L46IDEwMS8xMTQu"
    CounterPlanRemaining = Decode-Utf8Text "0JLQtdGB0Ywg0L/Qu9Cw0L0g0L3QtSDQt9Cw0LrRgNGL0YLQvjogMTMvMTE0Lg=="
    Proves1 = Decode-Utf8Text "0KHRg9GJ0LXRgdGC0LLRg9GO0YnQsNGPIGRhc2hib2FyZC9BUEkgUUEg0L/QvtCy0YLQvtGA0L3QviDQt9Cw0L/Rg9GB0LrQsNC10YLRgdGPINGH0LXRgNC10LcgdG9vbHMvYXBpLWRhc2hib2FyZC1xYS5wczEu"
    Proves2 = Decode-Utf8Text "SG9zdCBzY2VuYXJpbyBwYWNrZXRzINC00LvRjyBDbGF1ZGUsIEN1cnNvciDQuCBDb2RleCDRgdGC0LDRgtC40YfQtdGB0LrQuCDQv9C10YDQtdC/0YDQvtCy0LXRgNGP0Y7RgtGB0Y8g0L3QsCBjb3JyZWxhdGlvbi9kZWJ1Zy9uZWdhdGl2ZS1wYXRoL25vLW92ZXJjbGFpbSBmaWVsZHMu"
    Proves3 = Decode-Utf8Text "SHVtYW4gZG9jcyDQvtCx0YrRj9GB0L3Rj9GO0YIgZGFzaGJvYXJkL0FQSS9kZWJ1Zy9ldmlkZW5jZS1zY29wZS9sb2NhbCBsaWZlY3ljbGUgYm91bmRhcmllcy4="
    Proves4 = Decode-Utf8Text "0JjQt9C80LXQvdC10L3QvdGL0LkgUGhhc2UgMTMg0YLQtdC60YHRgiDQvdC1INGB0L7QtNC10YDQttC40YIg0L/QvtC70L7QttC40YLQtdC70YzQvdGL0YUgZm9yYmlkZGVuIHJlYWRpbmVzcyBjbGFpbXMu"
    NotProves1 = Decode-Utf8Text "0K3RgtC+INC90LUg0LTQvtC60LDQt9GL0LLQsNC10YIg0YTQuNC90LDQu9GM0L3Rg9GOINCz0L7RgtC+0LLQvdC+0YHRgtGMINC/0YDQvtC00YPQutGC0LAu"
    NotProves2 = Decode-Utf8Text "0K3RgtC+INC90LUg0LTQvtC60LDQt9GL0LLQsNC10YIgZnVsbCBob3N0IFVJIHJlYWRpbmVzcy4="
    NotProves3 = Decode-Utf8Text "0K3RgtC+INC90LUg0LTQvtC60LDQt9GL0LLQsNC10YIgZnVsbCBzZW1hbnRpYyBxdWFsaXR5Lg=="
    NotProves4 = Decode-Utf8Text "0K3RgtC+INC90LUg0LTQvtC60LDQt9GL0LLQsNC10YIgZnVsbCBsZWFybmFiaWxpdHkvdHJhaW5pbmcgc3VjY2Vzcy4="
    NotProves5 = Decode-Utf8Text "0K3RgtC+INC90LUg0L/RgNC+0LTQstC40LPQsNC10YIgY2hlY2twb2ludHMg0Lgg0L3QtSDQvtCx0L3QvtCy0LvRj9C10YIgbG9jYXRvcnMu"
    NotProves6 = Decode-Utf8Text "0K3RgtC+INC90LUg0LfQsNC80LXQvdGP0LXRgiBQaGFzZSAxNCBmaW5hbCByZXZpZXdzLg=="
    GitBoundary = Decode-Utf8Text "R2l0IGNvbW1hbmRzINC90LUg0YLRgNC10LHRg9GO0YLRgdGPINC4INC90LUg0YDQsNC30YDQtdGI0LXQvdGLINC00LvRjyDRjdGC0L7Qs9C+IHN1Yi1hZ2VudCBzbGljZS4="
    ConsoleDone = Decode-Utf8Text "UGhhc2UgMTMgUUEvZG9jcy9uby1vdmVyY2xhaW0gcmVwb3J0"
}

function Join-RepoPath {
    param([string]$RelativePath)

    return Join-Path $repoRoot $RelativePath
}

function Read-RepoText {
    param([string]$RelativePath)

    return [System.IO.File]::ReadAllText((Join-RepoPath $RelativePath), [System.Text.Encoding]::UTF8)
}

function Write-TextFile {
    param(
        [string]$Path,
        [string]$Text
    )

    $normalized = (($Text -split "\r?\n") | ForEach-Object { $_ -replace "[ `t]+$", "" }) -join "`n"
    [System.IO.File]::WriteAllText($Path, $normalized, [System.Text.UTF8Encoding]::new($false))
}

function Add-Check {
    param(
        [string]$Name,
        [bool]$Passed,
        [string]$Summary,
        [string]$Evidence
    )

    $script:checks += [pscustomobject]@{
        name = $Name
        passed = $Passed
        summary = $Summary
        evidence = $Evidence
    }
}

function Add-TextContainsCheck {
    param(
        [string]$Name,
        [string]$RelativePath,
        [string[]]$RequiredText
    )

    if (-not (Test-Path -LiteralPath (Join-RepoPath $RelativePath))) {
        Add-Check $Name $false $ru.DocMissing $RelativePath
        return
    }

    $text = Read-RepoText $RelativePath
    $missing = @($RequiredText | Where-Object { -not $text.Contains($_) })
    Add-Check $Name ($missing.Count -eq 0) $ru.DocRequiredSummary ("file: $RelativePath; missing: $($missing -join ' | ')")
}

function Test-AllowedOverclaimContext {
    param([string]$Text)

    $lower = $Text.ToLowerInvariant()
    $claimPattern = "(?:final product readiness|full product readiness|project ready|full extension readiness achieved|full host ui readiness|full semantic quality|semantic[- ]quality|full learnability|learnability|checkpoint promotion|locator update|host runtime|claude|cursor|codex|full extension|readiness|learning)"
    $allowedPatterns = @(
        "\b(?:does not|do not|cannot|must not|never|not|no)\b.{0,120}\b$claimPattern\b",
        "\b$claimPattern\b.{0,120}\b(?:not claimed|unproven|remains unproven|cannot be claimed|must not be claimed|is not proof|does not prove|not proof)\b",
        "\b(?:prevent|prevents|preventing)\b.{0,120}\b$claimPattern\b.{0,80}\bclaim\b",
        "\b(?:reject|rejected|forbidden|negative|violation|search pattern|bad example)\b.{0,120}\b$claimPattern\b",
        "\b$claimPattern\b.{0,120}\b(?:reject|rejected|forbidden|negative|violation|bad example)\b",
        "\u043d\u0435.{0,120}\b$claimPattern\b",
        "\b$claimPattern\b.{0,120}\u043d\u0435.{0,60}(?:\u0434\u043e\u043a\u0430\u0437|\u0433\u043e\u0442\u043e\u0432|\u044f\u0432\u043b)"
    )

    return @($allowedPatterns | Where-Object { $lower -match $_ }).Count -gt 0
}

function Get-OverclaimViolations {
    param(
        [string]$Source,
        [string]$Text
    )

    $patterns = @(
        @{ name = "final_product_readiness"; regex = "\b(final product readiness|full product readiness|project ready|full extension readiness achieved)\b" },
        @{ name = "full_host_ui_readiness"; regex = "\bfull host UI readiness\b" },
        @{ name = "full_semantic_quality"; regex = "\b(full semantic quality|semantic[- ]quality\b.{0,80}\b(proven|proved|ready|complete|completed|closed|passes|passed))\b" },
        @{ name = "full_learnability"; regex = "\b(full learnability|learnability\b.{0,80}\b(proven|proved|ready|complete|completed|closed|passes|passed))\b" },
        @{ name = "checkpoint_promotion"; regex = "\bcheckpoint promotion\b.{0,80}\b(proven|proved|ready|complete|completed|closed|passes|passed|done)\b" },
        @{ name = "locator_update"; regex = "\blocator update\b.{0,80}\b(proven|proved|ready|complete|completed|closed|passes|passed|done)\b" },
        @{ name = "dashboard_proves_runtime"; regex = "\bdashboard\b.{0,80}\b(proves?|proved|proven)\b.{0,80}\b(host runtime|claude|cursor|codex)\b" },
        @{ name = "smoke_proves_extension"; regex = "\bsmoke\b.{0,80}\b(proves?|proved|proven)\b.{0,80}\b(full extension|readiness)\b" },
        @{ name = "dpo_proves_learning"; regex = "\bDPO row\b.{0,80}\b(proves?|proved|proven)\b.{0,80}\b(learnability|learning)\b" }
    )

    $violations = New-Object System.Collections.Generic.List[string]
    $lines = $Text -split "\r?\n"
    for ($index = 0; $index -lt $lines.Count; $index++) {
        $line = $lines[$index]
        if (Test-AllowedOverclaimContext $line) {
            continue
        }

        foreach ($pattern in $patterns) {
            if ($line -match $pattern.regex) {
                $violations.Add("$($Source):$($index + 1):$($pattern.name):$($line.Trim())")
            }
        }
    }

    return @($violations)
}

function Add-NoOverclaimSelfCheck {
    $positiveViolations = @(Get-OverclaimViolations "self-positive" "Dashboard only proves final product readiness")
    $negativeViolations = @(Get-OverclaimViolations "self-negative" "Dashboard does not prove final product readiness")
    $passed = $positiveViolations.Count -gt 0 -and $negativeViolations.Count -eq 0
    $evidence = "positive_rejected: $($positiveViolations.Count -gt 0); negative_allowed: $($negativeViolations.Count -eq 0); positive_line: Dashboard only proves final product readiness; negative_line: Dashboard does not prove final product readiness"
    Add-Check "phase13_no_overclaim_self_check_strict_positive_negative" $passed $ru.SelfCheckSummary $evidence
}

function Invoke-ApiDashboardQa {
    $scriptPath = Join-RepoPath "tools/api-dashboard-qa.ps1"
    if (-not (Test-Path -LiteralPath $scriptPath)) {
        Add-Check "phase13_existing_api_dashboard_qa_script_exists" $false $ru.DashboardQaMissingSummary "tools/api-dashboard-qa.ps1"
        return $null
    }

    $output = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Json 2>&1
    $exitCode = $LASTEXITCODE
    $outputText = (($output | Out-String).Trim())
    Write-TextFile (Join-Path $artifactDir "api-dashboard-qa-output.json") $outputText

    try {
        $report = $outputText | ConvertFrom-Json -ErrorAction Stop
        $passed = $exitCode -eq 0 -and $report.passed -eq $true
        $checkCount = @($report.checks).Count
        Add-Check "phase13_existing_api_dashboard_qa_passes" $passed $ru.DashboardQaPassSummary "exit_code: $exitCode; checks: $checkCount; scope: $($report.evidence_scope)"
        return $report
    }
    catch {
        Add-Check "phase13_existing_api_dashboard_qa_passes" $false $ru.DashboardQaParseFailSummary "exit_code: $exitCode; parse_error: $($_.Exception.Message)"
        return $null
    }
}

function Add-HostScenarioCheck {
    param(
        [string]$HostName,
        [string]$RelativePath,
        [string[]]$RequiredMarkers
    )

    if (-not (Test-Path -LiteralPath (Join-RepoPath $RelativePath))) {
        Add-Check "phase13_host_scenario_qa_$HostName" $false $ru.HostScenarioMissingSummary $RelativePath
        return
    }

    $text = Read-RepoText $RelativePath
    $missingMarkers = @($RequiredMarkers | Where-Object { -not $text.Contains($_) })
    $hasNoOverclaimFlags = $text -match '"semantic_quality_claimed"\s*:\s*(false|"false")' `
        -and $text -match '"training_started"\s*:\s*false' `
        -and $text -match '"eval_started"\s*:\s*false' `
        -and $text -match '"checkpoint_mutation_started"\s*:\s*false'
    $hasNarrowScope = $text -match '(only|unproven|does not prove|does not claim|not claimed|IN_PROGRESS_NEXT_STEP_KNOWN)'
    $passed = $missingMarkers.Count -eq 0 -and $hasNoOverclaimFlags -and $hasNarrowScope

    Add-Check "phase13_host_scenario_qa_$HostName" $passed $ru.HostScenarioSummary ("file: $RelativePath; missing markers: $($missingMarkers -join ' | '); no_overclaim_flags: $hasNoOverclaimFlags; narrow_scope: $hasNarrowScope")
}

New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null

$apiDashboardReport = Invoke-ApiDashboardQa

Add-HostScenarioCheck "Claude" "artifacts/extension-runtime-token-quality-learnability/claude-runtime-proof.json" @(
    "Claude",
    "correlation_id",
    "redacted_input",
    "redacted_output",
    "negative_error_path",
    "XiigenExtensionApiService"
)

Add-HostScenarioCheck "Cursor" "artifacts/extension-runtime-token-quality-learnability/cursor-runtime-proof-rerun.json" @(
    "Cursor",
    "correlation_id",
    "redacted_input",
    "redacted_output",
    "controlled_negative_error_path",
    "XiigenExtensionApiService"
)

Add-HostScenarioCheck "Codex" "artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-04g.json" @(
    "Codex",
    "app-server",
    "request_ref",
    "response_ref",
    "controlled_negative_path",
    "XiigenExtensionApiService"
)

Add-TextContainsCheck "phase13_dashboard_doc_is_human_readable_and_scoped" "docs/projects/43-smart-skills-domain-dashboard.md" @(
    (Decode-Utf8Text "0JrQvtC80LDQvdC00LAgUGhhc2UgMTMgUUEvbm8tb3ZlcmNsYWlt"),
    "dashboard-to-core invocation only; not extension host runtime proof",
    (Decode-Utf8Text "0L3QtSDQtNC+0LrQsNC30YvQstCw0LXRgiBsaXZlIENsYXVkZS9DdXJzb3IvQ29kZXggZXh0ZW5zaW9uIHJ1bnRpbWU="),
    "QA automation, documentation, and no-overclaim evidence."
)

Add-TextContainsCheck "phase13_api_dashboard_howto_lists_command_and_limits" "docs/API-DASHBOARD-QA-HOWTO-ru.md" @(
    (Decode-Utf8Text "0JrQvtC80LDQvdC00LAgUGhhc2UgMTM="),
    ".\tools\phase13-qa-docs-no-overclaim.ps1",
    (Decode-Utf8Text "RGFzaGJvYXJkL0FQSSBwYXNzINC90LUg0LTQvtC60LDQt9GL0LLQsNC10YIgbGl2ZSBDbGF1ZGUvQ3Vyc29yL0NvZGV4IHJ1bnRpbWU="),
    (Decode-Utf8Text "0KfRgtC+INGN0YLQviDQtNC+0LrQsNC30YvQstCw0LXRgg==")
)

Add-TextContainsCheck "phase13_evidence_scope_howto_lists_forbidden_upgrades" "docs/EVIDENCE-SCOPES-HOWTO-ru.md" @(
    "Phase 13 no-overclaim gate",
    "Documentation QA report",
    "Continue-training report",
    "Rollback report",
    (Decode-Utf8Text "RGFzaGJvYXJkL0FQSSAtPiBsaXZlIGhvc3QgcnVudGltZTog0LfQsNC/0YDQtdGJ0LXQvdC+Lg==")
)

Add-TextContainsCheck "phase13_debug_howto_keeps_trace_narrow" "docs/XIIGEN-DEBUG-TRACE-HOWTO-ru.md" @(
    (Decode-Utf8Text "0JPRgNCw0L3QuNGG0LAgZGVidWcgZXZpZGVuY2Ug0LTQu9GPIFBoYXNlIDEz"),
    (Decode-Utf8Text "RGVidWcgdHJhY2Ug0YHQsNC8INC/0L4g0YHQtdCx0LUg0L3QtSDQtNC+0LrQsNC30YvQstCw0LXRgiBydW50aW1lIHJlYWRpbmVzcw=="),
    "correlation id",
    "redacted input"
)

$planText = Read-RepoText "docs/plans/XIIGEN-SEQUENTIAL-EXECUTION-CONTROL-PLAN-v1.md"
$statusBoardOk = $planText.Contains("| 13 | QA automation, docs and no-overclaim | IN_PROGRESS | 8 | 7 | 1 |") `
    -and $planText.Contains("- substep DONE: 101;") `
    -and $planText.Contains("- substep IN_PROGRESS: 1;") `
    -and $planText.Contains("- substep REMAINING: 12;") `
    -and $planText.Contains("13.08/13.08") `
    -and $planText.Contains("| 13.08 | IN_PROGRESS | Parent commit/status boundary |") `
    -and $planText.Contains((Decode-Utf8Text "0Y3RgtC+0YIgc3ViLWFnZW50INC90LUg0LfQsNC/0YPRgdC60LDQuyBnaXQ="))
Add-Check "phase13_status_board_counters_are_consistent" $statusBoardOk $ru.StatusBoardSummary "expected global counters: done 101/114, in progress 13.08/13.08, remaining 13/114"
Add-NoOverclaimSelfCheck

$scanTargets = @(
    "docs/projects/43-smart-skills-domain-dashboard.md",
    "docs/API-DASHBOARD-QA-HOWTO-ru.md",
    "docs/EVIDENCE-SCOPES-HOWTO-ru.md",
    "docs/XIIGEN-DEBUG-TRACE-HOWTO-ru.md"
)
$overclaimViolations = @($scanTargets |
    Where-Object { Test-Path -LiteralPath (Join-RepoPath $_) } |
    ForEach-Object { Get-OverclaimViolations $_ (Read-RepoText $_) })
Add-Check "phase13_no_overclaim_scan_changed_text" ($overclaimViolations.Count -eq 0) $ru.NoOverclaimSummary ($overclaimViolations -join " | ")

$passed = -not ($script:checks | Where-Object { -not $_.passed })
$report = [pscustomobject]@{
    report = "phase13-qa-docs-no-overclaim"
    phase = $ru.Phase
    human_summary_ru = $ru.HumanSummary
    status = $(if ($passed) { "PASS_PHASE13_SUBAGENT_SLICE_PARENT_BOUNDARY_REMAINS" } else { "FAIL_PHASE13_SUBAGENT_SLICE" })
    phase13_steps = @(
        [pscustomobject]@{ step = "13.01"; status = "DONE"; evidence = $ru.Step1301 },
        [pscustomobject]@{ step = "13.02"; status = "DONE"; evidence = $ru.Step1302 },
        [pscustomobject]@{ step = "13.03"; status = "DONE"; evidence = $ru.Step1303 },
        [pscustomobject]@{ step = "13.04"; status = "DONE"; evidence = $ru.Step1304 },
        [pscustomobject]@{ step = "13.05"; status = "DONE"; evidence = $ru.Step1305 },
        [pscustomobject]@{ step = "13.06"; status = "DONE"; evidence = $ru.Step1306 },
        [pscustomobject]@{ step = "13.07"; status = $(if ($passed) { "DONE" } else { "IN_PROGRESS" }); evidence = $ru.Step1307 },
        [pscustomobject]@{ step = "13.08"; status = "IN_PROGRESS"; evidence = $ru.Step1308 }
    )
    counters = [pscustomobject]@{
        phase13_done = 7
        phase13_in_progress = 1
        phase13_remaining = 0
        plan_done = 101
        plan_in_progress = "13.08/13.08"
        plan_remaining = 13
    }
    evidence_scope = $ru.EvidenceScope
    checks = $script:checks
}

$jsonReportPath = Join-Path $artifactDir "phase13-qa-docs-no-overclaim-report.json"
$mdReportPath = Join-Path $artifactDir "phase13-qa-docs-no-overclaim-report.md"
$reportJson = $report | ConvertTo-Json -Depth 8
Write-TextFile $jsonReportPath $reportJson

$checksMarkdown = ($script:checks | ForEach-Object {
    $status = if ($_.passed) { "PASS" } else { "FAIL" }
    "| $($_.name) | $status | $($_.summary.Replace('|', '/')) | $($_.evidence.Replace('|', '/')) |"
}) -join [Environment]::NewLine

$stepsMarkdown = ($report.phase13_steps | ForEach-Object {
    "| $($_.step) | $($_.status) | $($_.evidence.Replace('|', '/')) |"
}) -join [Environment]::NewLine

$reportMd = @"
# $($ru.ReportTitle)

$($ru.Phase)

$($ru.ReportIntro)

$($ru.SliceStatus): $($report.status)

## $($ru.StepStatusHeading)

| $($ru.TableStep) | $($ru.TableStatus) | Evidence |
|---|---|---|
$stepsMarkdown

## $($ru.CountersHeading)

- $($ru.CounterPhaseDone)
- $($ru.CounterPhaseProgress)
- $($ru.CounterPhaseRemaining)
- $($ru.CounterPlanDone)
- $($ru.CounterPlanRemaining)

## $($ru.ProvesHeading)

- $($ru.Proves1)
- $($ru.Proves2)
- $($ru.Proves3)
- $($ru.Proves4)

## $($ru.NotProvesHeading)

- $($ru.NotProves1)
- $($ru.NotProves2)
- $($ru.NotProves3)
- $($ru.NotProves4)
- $($ru.NotProves5)
- $($ru.NotProves6)

## $($ru.ChecksHeading)

| $($ru.TableCheck) | $($ru.TableStatus) | $($ru.TableSummary) | Evidence |
|---|---|---|---|
$checksMarkdown

## $($ru.GitBoundaryHeading)

$($ru.GitBoundary)
"@
Write-TextFile $mdReportPath $reportMd

if ($Json) {
    $reportJson
}
else {
    "$($ru.ConsoleDone): $($report.status)"
    "JSON: $jsonReportPath"
    "Markdown: $mdReportPath"
}

if (-not $passed) {
    exit 1
}

exit 0
