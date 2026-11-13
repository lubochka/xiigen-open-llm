# Row10-S5a launch/storage diagnostic prep

claim_scope:
Row10-S5a prep-only diagnostic plan in Russian; no Cursor launch, no UI automation, no live process manipulation, no product/runtime execution.
evidence_scope:
Read the two source salvage packet files named in the work order and created this Markdown/JSON prep packet.
count_claimed:
2 source salvage files read; 1 prep folder created; 2 prep packet files created.
independent_evidence_count:
source file read outputs, folder creation output, file creation diff, and verification command outputs after creation.
subagent_id_or_NONE:
bounded-subagent Row10-S5a
downgrade_needed:
Yes for any broader claim: this packet is prep only and does not prove Cursor readiness, XIIGen panel readiness, or a product no-send UI pass.

## Verdict

`PASS_PREP`

Этот пакет готовит S5a как короткий диагностический маршрут перед будущим запуском Cursor. Он не выполняет запуск, не трогает UI, не закрывает процессы и не проверяет live desktop. Доказательная граница узкая: пакет только превращает слишком крупный S5 в безопасные micro-steps с явными PASS/BLOCKED критериями.

## Source Salvage

Прочитанные источники:

- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s5-cursor-launch-recovery-no-send-20260627-045817\row10-s5-timeout-salvage.md`
- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s5-cursor-launch-recovery-no-send-20260627-045817\row10-s5-timeout-salvage.json`

Узко поддержанные факты из salvage:

- Cursor executable path был указан как `C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe`.
- Route/command был указан как `xiigenSmartSkills.panel`.
- В S5 были сохранены evidence записи о `Storage Error` и `SQLITE_CANTOPEN: unable to open database file`.
- В S5 был зафиксирован Windows lock/PIN overlay, поэтому будущий UI маршрут должен начинаться с desktop preflight.
- Salvage не доказывает полный S5 no-send UI pass и не доказывает интерактивную готовность XIIGen panel.

## Safety Constraints

- Не запускать Cursor в S5a prep.
- Не выполнять UI automation в S5a prep.
- Не управлять live процессами и не закрывать Cursor в S5a prep.
- Не вводить prompt и не нажимать send в будущих шагах без отдельного work order.
- Не делать screenshots с секретами, PIN, account details, tokens, keys или приватным текстом.
- Не делать product readiness claims по этому prep-пакету.
- Не запускать tests/builds/product runtime для этого prep-пакета.
- Не делать code/board/status edits, commit или push.

## BLOCKED_MISSING_LAUNCH_DIAGNOSTIC_DETAILS

Маркер: `BLOCKED_MISSING_LAUNCH_DIAGNOSTIC_DETAILS`

Этот маркер не ломает prep-пакет, но блокирует немедленный будущий launch/focus execution, пока поля ниже не будут заполнены или подтверждены:

- Полная предыдущая argument array для Cursor запуска; salvage packet подтверждает executable path и `--command xiigenSmartSkills.panel`, но не является полным readback всех launch args.
- Точный `--extensionDevelopmentPath`, если будущий запуск должен быть Extension Development Host.
- Решение для stale Cursor handling: только изоляция через clean `--user-data-dir`/`--extensions-dir` или отдельный разрешенный close-stale-process step.
- Конкретная future-run evidence folder для launch execution; этот prep-пакет не создает future-run storage.
- Desktop session состояние непосредственно перед будущим UI шагом: unlocked, no lock/PIN overlay, no secret surface visible.

## Micro-Step S5a-01: no-UI command/path validation

Цель: проверить путь к Cursor и route string без запуска Cursor.

Будущие команды, если будет отдельное разрешение на no-UI diagnostic execution:

```powershell
$cursorExe = 'C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe'
$routeCommand = 'xiigenSmartSkills.panel'

$pathResult = [ordered]@{
  cursor_exe = $cursorExe
  cursor_exe_exists = Test-Path -LiteralPath $cursorExe -PathType Leaf
  route_command = $routeCommand
  route_command_present = [bool]$routeCommand
}

if ($pathResult.cursor_exe_exists) {
  $item = Get-Item -LiteralPath $cursorExe
  $pathResult.cursor_exe_full_name = $item.FullName
  $pathResult.cursor_exe_length = $item.Length
  $pathResult.cursor_exe_last_write_time = $item.LastWriteTime.ToString('o')
  $pathResult.cursor_exe_sha256 = (Get-FileHash -LiteralPath $cursorExe -Algorithm SHA256).Hash
}

$pathResult | ConvertTo-Json -Depth 5
```

Expected evidence files for future execution:

- `s5a-01-cursor-path-validation.json`
- `s5a-01-command-path-validation-transcript.txt`

PASS criteria:

- `cursor_exe_exists = true`.
- File metadata and SHA256 are captured.
- `route_command = xiigenSmartSkills.panel`.
- No Cursor process is launched.

BLOCKED criteria:

- Cursor exe path is missing or not a file.
- Route command string is empty or changed without a plan update.
- Any command unexpectedly launches Cursor or opens UI.

## Micro-Step S5a-02: storage path/user-data/temp dir validation

Цель: заранее убрать риск `SQLITE_CANTOPEN` через clean isolated storage и write/delete probes before launch.

Будущие команды, если будет отдельное разрешение на no-UI storage diagnostic execution:

```powershell
$runRoot = 'C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s5a-future-run-YYYYMMDD-HHMMSS'
$userDataDir = Join-Path $runRoot 'user-data-clean'
$extensionsDir = Join-Path $runRoot 'extensions-clean'
$tempDir = Join-Path $runRoot 'temp'

New-Item -ItemType Directory -Force -Path $userDataDir, $extensionsDir, $tempDir | Out-Null

$probeResults = foreach ($dir in @($userDataDir, $extensionsDir, $tempDir)) {
  $probeFile = Join-Path $dir 'write-delete-probe.txt'
  's5a-storage-probe' | Set-Content -LiteralPath $probeFile -Encoding UTF8 -NoNewline
  $existsAfterWrite = Test-Path -LiteralPath $probeFile -PathType Leaf
  Remove-Item -LiteralPath $probeFile -Force
  [ordered]@{
    dir = $dir
    dir_exists = Test-Path -LiteralPath $dir -PathType Container
    probe_write_ok = $existsAfterWrite
    probe_delete_ok = -not (Test-Path -LiteralPath $probeFile)
    full_path_length = $dir.Length
  }
}

[ordered]@{
  run_root = $runRoot
  user_data_dir = $userDataDir
  extensions_dir = $extensionsDir
  temp_dir = $tempDir
  process_temp = [System.IO.Path]::GetTempPath()
  env_temp = $env:TEMP
  env_tmp = $env:TMP
  probes = $probeResults
} | ConvertTo-Json -Depth 8
```

Expected evidence files for future execution:

- `s5a-02-storage-path-validation.json`
- `s5a-02-storage-path-validation-transcript.txt`

PASS criteria:

- Clean `user-data-clean`, `extensions-clean`, and `temp` dirs are created inside the future-run evidence folder.
- Write/delete probe succeeds in each directory.
- Paths are local, readable, writable, and not reused from the failed S5 `user-data-quoted-retry`.
- Future launch args will use the clean dirs, not the prior failed storage path.

BLOCKED criteria:

- Any directory cannot be created.
- Any write/delete probe fails.
- Future run root is not chosen.
- Path policy requires closing stale Cursor but no close-stale-process work order exists.

## Micro-Step S5a-03: desktop foreground/lock overlay preflight

Цель: проверить до UI route discovery, что Windows session не находится на lock/PIN overlay и что evidence capture не захватит секреты.

Future preflight procedure:

1. Confirm the desktop is unlocked before any Cursor launch/focus attempt.
2. Confirm no Windows lock/PIN/account prompt is visible.
3. Confirm no token, password, private account detail, or secret-bearing window is visible in the capture area.
4. If screenshot evidence is required by a future work order, capture only a redacted/safe screenshot and save redaction metadata.
5. Abort before any prompt typing or send action if lock/PIN overlay appears.

Expected evidence files for future execution:

- `s5a-03-desktop-preflight.json`
- `s5a-03-redaction-note.json` only if a screenshot is captured by a future UI-scoped work order.

PASS criteria:

- Desktop is confirmed unlocked immediately before launch/focus.
- No lock/PIN overlay is visible.
- Screenshot policy says no secrets captured and no credentials entered.
- No prompt typed and no prompt sent.

BLOCKED criteria:

- Lock/PIN overlay is visible.
- Any secret-bearing surface would be captured.
- Operator cannot confirm desktop foreground state.
- Future work order does not allow UI/screenshot evidence.

## Micro-Step S5a-04: future launch/focus no-send route discovery

Цель: only after S5a-01 through S5a-03 pass, perform a future minimal launch/focus route discovery without typing or sending a prompt.

Status now: blocked for execution by `BLOCKED_MISSING_LAUNCH_DIAGNOSTIC_DETAILS`.

Future command skeleton, not to run from this prep packet:

```powershell
$cursorExe = 'C:\Users\lubao\AppData\Local\Programs\cursor\Cursor.exe'
$userDataDir = '<future-run-root>\user-data-clean'
$extensionsDir = '<future-run-root>\extensions-clean'
$extensionDevelopmentPath = '<BLOCKED_MISSING_EXTENSION_DEVELOPMENT_PATH>'

$argList = @(
  '--user-data-dir', $userDataDir,
  '--extensions-dir', $extensionsDir,
  '--extensionDevelopmentPath', $extensionDevelopmentPath,
  '--command', 'xiigenSmartSkills.panel'
)

Start-Process -FilePath $cursorExe -ArgumentList $argList -PassThru
```

Expected evidence files for future execution:

- `s5a-04-launch-command.json`
- `s5a-04-start-process-result.json`
- `s5a-04-window-inventory.json`
- `s5a-04-storage-log-search.txt`
- `s5a-04-no-send-route-discovery-redacted.png` only if future UI evidence is explicitly authorized and safe.

PASS criteria:

- S5a-01, S5a-02, and S5a-03 are already PASS in the same future run packet.
- Full argument array is recorded before launch.
- Launch uses clean `--user-data-dir` and `--extensions-dir`.
- No `Storage Error` dialog and no `SQLITE_CANTOPEN` are observed in saved logs.
- Route discovery reaches `xiigenSmartSkills.panel` without prompt typed and without prompt sent.
- Claim remains route discovery only, not product readiness.

BLOCKED criteria:

- Any previous micro-step is not PASS.
- `--extensionDevelopmentPath` remains unknown when Extension Development Host is required.
- Lock/PIN overlay appears.
- `Storage Error` or `SQLITE_CANTOPEN` reappears.
- Any prompt would need to be typed or sent.

## Markdown/JSON Agreement Keys

The JSON packet must agree with this Markdown on:

- `verdict = PASS_PREP`
- `blocked_marker = BLOCKED_MISSING_LAUNCH_DIAGNOSTIC_DETAILS`
- micro-step ids `S5a-01`, `S5a-02`, `S5a-03`, `S5a-04`
- safety constraints: no Cursor launch, no UI automation, no prompt typed/sent, no screenshots with secrets, no product readiness claims
- current execution status: prep-only; S5a-04 execution blocked until missing launch diagnostic details are filled
