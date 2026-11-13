$items = @(
    "docs/plans/llm dynamic flow/",
    "docs/plans/llm-graphrag-dotnet-plan v2.md",
    "docs/plans/llm-graphrag-dotnet-plan.md",
    "docs/plans/llm_solution_e2e_parity.html",
    "docs/plans/local wiki learning/",
    "docs/plans/localwikis/",
    "docs/plans/orchestrator implementation/",
    "docs/plans/orchestrator teaching plan/",
    "docs/plans/plan the learning set/",
    "docs/plans/replanning-possible-diffs/",
    "docs/plans/searchengine-dotnet-plan.md",
    "docs/plans/session-package-extracted/",
    "docs/plans/skills loading as an ai extension/",
    "docs/plans/teaching-ledger/",
    "docs/plans/tensorcore-dotnet-plan.md",
    "docs/plans/test hypothesys.zip",
    "docs/plans/test hypothesys/",
    "docs/plans/train-ready-v2/",
    "docs/plans/uml-00-tensorcore.pdf",
    "docs/plans/uml-01-11-llm-graphrag.pdf",
    "docs/plans/uml-12-searchengine.pdf",
    "docs/plans/uml-13-codeoptimizer.pdf",
    "docs/plans/uml-14-20-ml-algorithms.pdf",
    "docs/plans/uml-21-agent-infrastructure.pdf",
    "docs/plans/uml-22-agent-algorithms.pdf",
    "docs/plans/uml-23-oss-equivalents.pdf",
    "docs/plans/uml-df-24-27-compression.pdf",
    "docs/plans/uml-df-28-30-matrix.pdf",
    "docs/plans/uml-df-28-32-semantic-nlp.pdf",
    "docs/plans/uml-full-solution.pdf",
    "docs/plans/user-local-adaptive-intake-schema-v1.md",
    "docs/plans/visual studio & visual studio code continue/",
    "docs/plans/Переломный момент мышления.txt",
    "docs/plans/новый домен/",
    "docs/plans/обучение языку/",
    "docs/projects/",
    "docs/skills/",
    "experiments-v2/",
    "experiments/",
    "learning-sets/",
    "models/",
    "qa-results/",
    "tools/",
    "training-artifacts/",
    "training-data/"
)

function Clear-GitLock {
    Stop-Process -Name git -Force -ErrorAction SilentlyContinue
    Stop-Process -Name git-lfs -Force -ErrorAction SilentlyContinue
    Get-Process | Where-Object { $_.ProcessName -match 'git' } | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 800
    if (Test-Path ".git\index.lock") {
        Remove-Item ".git\index.lock" -Force -ErrorAction SilentlyContinue
    }
}

function Invoke-GitWithTimeout {
    param (
        [string]$Arguments,
        [int]$TimeoutSeconds = 180
    )
    Clear-GitLock

    $retry = $true
    while ($retry) {
        Write-Host "Executing: git $Arguments" -ForegroundColor Cyan
        
        $pinfo = New-Object System.Diagnostics.ProcessStartInfo
        $pinfo.FileName = "git"
        $pinfo.Arguments = $Arguments
        $pinfo.WorkingDirectory = $PWD.Path
        $pinfo.UseShellExecute = $false
        $pinfo.RedirectStandardOutput = $true
        $pinfo.RedirectStandardError = $true
        $pinfo.CreateNoWindow = $true
        
        $process = New-Object System.Diagnostics.Process
        $process.StartInfo = $pinfo
        $process.Start() | Out-Null
        
        $completed = $process.WaitForExit($TimeoutSeconds * 1000)
        
        if ($completed) {
            $output = $process.StandardOutput.ReadToEnd()
            $err = $process.StandardError.ReadToEnd()
            if ($output) { Write-Host $output }
            if ($err -and $err -notmatch "nothing to commit|no changes") { 
                Write-Host $err -ForegroundColor Red 
            }
            $retry = $false
        } else {
            Write-Host "Process hanging (> $TimeoutSeconds sec). Killing..." -ForegroundColor Yellow
            $process.Kill()
            Clear-GitLock
        }
    }
}

# Старт
Clear-GitLock
Invoke-GitWithTimeout -Arguments "reset" -TimeoutSeconds 20

foreach ($item in $items) {
    $cleanItem = $item.Trim().TrimEnd('/').Trim('"')
    Write-Host "`n--- Processing: $cleanItem ---" -ForegroundColor Green

    Clear-GitLock

    # LFS для файлов >50MB
    if (Test-Path $cleanItem -PathType Container) {
        $heavyFiles = Get-ChildItem -Path $cleanItem -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object { $_.Length -gt 50MB -and $_.FullName -notmatch '\\\.git(\\|$)' }

        foreach ($file in $heavyFiles) {
            $relPath = $file.FullName.Substring($PWD.Path.Length + 1).Replace('\', '/')
            Write-Host "LFS track: $relPath ($([math]::Round($file.Length/1MB,1)) MB)" -ForegroundColor Magenta
            Invoke-GitWithTimeout -Arguments "lfs track `"$relPath`"" -TimeoutSeconds 30
        }
    }

    # Добавляем
    Invoke-GitWithTimeout -Arguments "add -- `"$cleanItem`"" -TimeoutSeconds 240

    if (Test-Path .gitattributes) {
        Invoke-GitWithTimeout -Arguments "add -- .gitattributes" -TimeoutSeconds 15
    }

    $hasStaged = (git diff --cached --name-only) -ne $null

    if ($hasStaged) {
        Invoke-GitWithTimeout -Arguments "commit -m `"add $cleanItem`" --no-verify" -TimeoutSeconds 180
        Write-Host "Pushing LFS objects..." -ForegroundColor Yellow
        Invoke-GitWithTimeout -Arguments "lfs push --all origin mainv2" -TimeoutSeconds 300
        Invoke-GitWithTimeout -Arguments "push --no-verify origin mainv2" -TimeoutSeconds 180
    } else {
        Write-Host "Skipping: no changes for '$cleanItem'" -ForegroundColor DarkGray
    }
}

Write-Host "`nDone." -ForegroundColor Green