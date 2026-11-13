# Сценарий 20: подготовка проверки Codex extension entrypoint readiness

Статус подготовки: `ROW20_PREP_REPAIRED_REVIEW_REQUIRED`.

Этот документ - только подготовка сценария. Он не запускает продукт, runtime,
UI, adapter entrypoint, тесты продукта, обучение, оценку, checkpoint, locator,
commit или push. Row20 не принят этим документом.

## 1. Человеческий смысл сценария

Сценарий 20 проверяет узкую готовность входной точки Codex extension:
установленный Codex plugin `codex-xiigen-smart-skills` должен показать, что
его текущая entrypoint-реализация может провести один безопасный
non-mutating smoke request до `llm_mvp_core` через тонкий adapter/core boundary.

Сценарий 20 доказывает только wiring и видимость entrypoint для одного
заданного Codex-сценария. Он не доказывает готовность продукта.

Если оба пути будут выполнены позже и пройдут независимую проверку, допустимый
вывод остается узким:

1. установленный plugin package найден;
2. manifest и skill registration соответствуют `codex-xiigen-smart-skills`;
3. текущий `src/codex-entrypoint.js` запускается из установленного plugin root;
4. entrypoint формирует `XiigenExtensionRequest`;
5. entrypoint вызывает `C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1`;
6. request, response и trace содержат один и тот же Row20 prompt и correlation id;
7. если Путь B пройдет, локальная поверхность Codex показывает тот же
   non-mutating redacted result для того же сценария.

## 2. Что этот сценарий не доказывает

Даже после успешного будущего выполнения Row20 нельзя заявлять:

- готовность продукта;
- полный joined product pass;
- token savings;
- semantic quality improvement;
- learnability или training readiness;
- eval readiness;
- Direct Preference Optimization quality;
- checkpoint mutation, checkpoint promotion или production promotion;
- locator mutation или production locator update;
- `client-adaptive-learning-loop` behavior;
- broad Codex chat UI readiness вне этого entrypoint scenario;
- hidden external AI quality;
- code mutation;
- git/stage/commit/push.

Обязательные non-claims для будущего executor packet:

- `product_readiness_claimed=false`
- `full_joined_pass=false`
- `token_savings_claimed=false`
- `semantic_quality_claimed=false`
- `learnability_claimed=false`
- `training_started=false`
- `eval_started=false`
- `checkpoint_mutation_started=false`
- `checkpoint_promotion_claimed=false`
- `locator_update_claimed=false`
- `external_ai_used=false`
- `code_mutation_claimed=false`
- `git_action_claimed=false`

## 3. Один и тот же сценарий для двух путей

Путь A и Путь B обязаны использовать один prompt и один correlation id.
Заменять их можно только после явного нового разрешения Luba.

Prompt:

```text
Row20 non-mutating Codex installed-plugin entrypoint smoke: route this harmless planning request through XIIGen Smart Skills using prompt-to-task, request only xiigen-smart-skills, keep debug output redacted, do not train, do not evaluate, do not mutate checkpoint or locator, do not call external AI, and return the selected skill/plan boundary result.
```

Correlation id:

```text
ROW20-CODEX-FULL-EXTENSION-ENTRYPOINT-NONMUTATING-SMOKE
```

Request fields:

- `method`: `prompt-to-task`
- `requestedSkills`: `["xiigen-smart-skills"]`
- `availableSkills`: `["xiigen-smart-skills"]`
- `hostFacts.surface`: `installed-codex-skill`
- `hostFacts.scenario_id`: `frdp-p01-codex-full-extension-entrypoint`
- `checkSetInputs.row20`: `codex-full-extension-entrypoint`
- `checkSetInputs.mutation`: `forbidden`
- `checkSetInputs.external_ai`: `forbidden`
- `provenance.source`: `row20-prep`
- `provenance.plugin`: `codex-xiigen-smart-skills`
- `debugEnabled`: false unless a future work order explicitly asks for
  redacted debug display. Raw private trace display is forbidden.

## 4. Путь A - automated/before-host boundary proof

Путь A отвечает на вопрос: текущая установленная JS entrypoint из Codex plugin
может создать request и вызвать `llm_mvp_core` через core API для того же Row20
prompt/correlation?

Путь A не является UI proof и не доказывает готовность продукта.

### 4.1 Точные команды для Path A

Будущий executor должен выполнить эти точные команды в PowerShell из любого
рабочего каталога. Они создают новый `run-<timestamp>` внутри директории Row20.

```powershell
$scenarioRoot = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint"
$runStamp = Get-Date -Format "yyyyMMdd-HHmmss"
$runRoot = Join-Path $scenarioRoot "run-$runStamp"
New-Item -ItemType Directory -Force -Path $runRoot | Out-Null
```

Точная команда выбора plugin root. Ожидаемый selector: выбранная директория
должна содержать `.codex-plugin\plugin.json`, `skills\xiigen-smart-skills\SKILL.md`,
`package.json` с `name=codex-xiigen-smart-skills` и `main=src/codex-entrypoint.js`,
а также `src\codex-entrypoint.js`.

```powershell
$pluginRoot = "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500"
if (-not (Test-Path -LiteralPath (Join-Path $pluginRoot "src\codex-entrypoint.js"))) {
    $pluginRoot = Get-ChildItem -LiteralPath "$env:USERPROFILE\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills" -Directory |
        Where-Object {
            (Test-Path -LiteralPath (Join-Path $_.FullName ".codex-plugin\plugin.json")) -and
            (Test-Path -LiteralPath (Join-Path $_.FullName "skills\xiigen-smart-skills\SKILL.md")) -and
            (Test-Path -LiteralPath (Join-Path $_.FullName "src\codex-entrypoint.js"))
        } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1 -ExpandProperty FullName
}
if (-not $pluginRoot) { throw "Row20 plugin root not found" }
```

Точная команда contract review перед invocation:

```powershell
$package = Get-Content -LiteralPath (Join-Path $pluginRoot "package.json") -Raw | ConvertFrom-Json
$manifest = Get-Content -LiteralPath (Join-Path $pluginRoot ".codex-plugin\plugin.json") -Raw | ConvertFrom-Json
$skillPath = Join-Path $pluginRoot "skills\xiigen-smart-skills\SKILL.md"
$contract = [pscustomobject]@{
    scenario = "Сценарий 20"
    path = "Путь A"
    pluginRoot = $pluginRoot
    packageName = $package.name
    packageMain = $package.main
    manifestName = $manifest.name
    skillPathExists = Test-Path -LiteralPath $skillPath
    jsEntrypointExists = Test-Path -LiteralPath (Join-Path $pluginRoot "src\codex-entrypoint.js")
    powershellEntrypointRole = "legacy_launcher_only"
    expectedSelector = "package.name=codex-xiigen-smart-skills; package.main=src/codex-entrypoint.js"
}
$contract | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $runRoot "path-a-entrypoint-contract-review.json") -Encoding UTF8
if ($contract.packageName -ne "codex-xiigen-smart-skills") { throw "Unexpected package name" }
if ($contract.packageMain -ne "src/codex-entrypoint.js") { throw "Unexpected package main" }
if (-not $contract.skillPathExists) { throw "Skill registration file missing" }
if (-not $contract.jsEntrypointExists) { throw "JS entrypoint missing" }
```

Точная команда invocation через текущую JS entrypoint:

```powershell
$prompt = "Row20 non-mutating Codex installed-plugin entrypoint smoke: route this harmless planning request through XIIGen Smart Skills using prompt-to-task, request only xiigen-smart-skills, keep debug output redacted, do not train, do not evaluate, do not mutate checkpoint or locator, do not call external AI, and return the selected skill/plan boundary result."
$correlation = "ROW20-CODEX-FULL-EXTENSION-ENTRYPOINT-NONMUTATING-SMOKE"
$hostFactsJson = '{"surface":"installed-codex-skill","scenario_id":"frdp-p01-codex-full-extension-entrypoint"}'
$requestedSkillsJson = '["xiigen-smart-skills"]'
$availableSkillsJson = '["xiigen-smart-skills"]'
$checkSetInputsJson = '{"row20":"codex-full-extension-entrypoint","mutation":"forbidden","external_ai":"forbidden"}'
$provenanceJson = '{"source":"row20-prep","plugin":"codex-xiigen-smart-skills"}'
$stdoutPath = Join-Path $runRoot "path-a-stdout.txt"
$stderrPath = Join-Path $runRoot "path-a-stderr.txt"
$requestPath = Join-Path $runRoot "path-a-request.json"
$responsePath = Join-Path $runRoot "path-a-response.json"
$tracePath = Join-Path $runRoot "path-a-trace.json"
$nodeArgs = @(
    (Join-Path $pluginRoot "src\codex-entrypoint.js"),
    "--method", "prompt-to-task",
    "--user-prompt-or-task-text", $prompt,
    "--host-facts-json", $hostFactsJson,
    "--requested-skills-json", $requestedSkillsJson,
    "--available-skills-json", $availableSkillsJson,
    "--check-set-inputs-json", $checkSetInputsJson,
    "--provenance-json", $provenanceJson,
    "--correlation-id", $correlation,
    "--core-root", "C:\Projects\llm_mvp_core",
    "--extension-root", $pluginRoot,
    "--request-path", $requestPath,
    "--response-path", $responsePath,
    "--trace-path", $tracePath
)
Push-Location $pluginRoot
try {
    & node.exe @nodeArgs 1> $stdoutPath 2> $stderrPath
    $exitCode = $LASTEXITCODE
}
finally {
    Pop-Location
}
[pscustomobject]@{
    scenario = "Сценарий 20"
    path = "Путь A"
    command = "node.exe src/codex-entrypoint.js --method prompt-to-task --user-prompt-or-task-text <Row20 prompt> --correlation-id ROW20-CODEX-FULL-EXTENSION-ENTRYPOINT-NONMUTATING-SMOKE --core-root C:\Projects\llm_mvp_core --request-path path-a-request.json --response-path path-a-response.json --trace-path path-a-trace.json"
    pluginRoot = $pluginRoot
    exitCode = $exitCode
    stdoutPath = $stdoutPath
    stderrPath = $stderrPath
    requestPath = $requestPath
    responsePath = $responsePath
    tracePath = $tracePath
    noProductReadinessClaim = $true
} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $runRoot "path-a-command-result.json") -Encoding UTF8
```

### 4.2 Expected outputs for Path A

Путь A должен создать эти evidence files:

- `path-a-entrypoint-contract-review.json`
- `path-a-request.json`
- `path-a-response.json`
- `path-a-trace.json`
- `path-a-command-result.json`
- `path-a-stdout.txt`
- `path-a-stderr.txt`

Минимальные ожидаемые признаки PASS для Path A:

- `path-a-command-result.json.exitCode` равен `0`;
- `path-a-request.json.CorrelationId` равен Row20 correlation id;
- `path-a-request.json.UserPromptOrTaskText` равен Row20 prompt;
- `path-a-request.json.Method` равен `prompt-to-task`;
- `path-a-request.json.RequestedSkills` содержит только `xiigen-smart-skills`;
- `path-a-response.json` существует и является JSON;
- `path-a-trace.json` существует и является JSON;
- evidence явно показывает `externalAiUsed=false` или эквивалентный
  no-external-AI признак;
- evidence явно показывает отсутствие training/eval/checkpoint/locator mutation.

Если response, trace или exit code отсутствуют, это FAIL/BLOCKER, а не readiness.

## 5. Путь B - local Codex desktop/UI proof

Путь B отвечает на вопрос: локальный Codex desktop UI показывает тот же
non-mutating Row20 сценарий для той же correlation, без скрытого утверждения
о product readiness?

Путь B не заменяет Путь A. Если полный Codex chat UI нельзя надежно
контролировать в этом сценарии, executor должен записать, какая локальная
видимая evidence собрана, и что остается не proven:
`CODEX_ROUTE_BLOCKED_CLEAN_SURFACE_NOT_PROVEN`.

### 5.1 Preconditions before clicks

Перед UI/runtime execution scenario prep must PASS by independent prep review.
До этого Путь B не запускать.

Локальный оператор/sub-agent должен:

1. Открыть реальное приложение Codex Desktop на этом компьютере.
2. Убедиться, что видна рабочая область `C:\Projects\llm_mvp_core`.
3. Убедиться, что сессия не anonymous: видна учетная запись, активная
   авторизация или обычная рабочая поверхность Codex.
4. Если Codex показывает login screen, не вводить и не сохранять секреты.
   Попросить Luba пройти login вручную, затем продолжить только после
   появления обычной рабочей поверхности.
5. Перед screenshot закрыть или скрыть окна с секретами, токенами, private
   chat content и personal data. No secrets captured.

### 5.2 Точные клики и ввод для Path B

Точные клики для будущего оператора:

1. В окне Codex Desktop нажать кнопку создания нового thread/chat.
2. Если Codex просит workspace, выбрать `C:\Projects\llm_mvp_core`.
3. В поле сообщения вставить ровно этот текст:

```text
Use the installed Codex plugin skill codex-xiigen-smart-skills:xiigen-smart-skills for Сценарий 20. Correlation id: ROW20-CODEX-FULL-EXTENSION-ENTRYPOINT-NONMUTATING-SMOKE. Row20 non-mutating Codex installed-plugin entrypoint smoke: route this harmless planning request through XIIGen Smart Skills using prompt-to-task, request only xiigen-smart-skills, keep debug output redacted, do not train, do not evaluate, do not mutate checkpoint or locator, do not call external AI, and return the selected skill/plan boundary result.
```

4. Нажать Send/Enter один раз.
5. Дождаться видимого ответа Codex.
6. Не запускать дополнительные prompts для исправления результата в том же
   evidence packet. Если ответ не коррелирует с Row20, записать rejection.

Ожидаемые visible states:

- Codex показывает новый или чистый thread для этой проверки;
- в видимом prompt есть Row20 correlation id;
- ответ содержит redacted result или понятный blocker;
- ответ не показывает raw private trace;
- ответ не утверждает product readiness;
- если plugin/skill не виден или не вызывается, это фиксируется как
  `CODEX_ROUTE_BLOCKED_CLEAN_SURFACE_NOT_PROVEN`.

### 5.3 Evidence capture for Path B

Точные команды для фиксации локального окна и screenshot metadata:

```powershell
$scenarioRoot = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint"
$runRoot = Get-ChildItem -LiteralPath $scenarioRoot -Directory -Filter "run-*" | Sort-Object LastWriteTime -Descending | Select-Object -First 1 -ExpandProperty FullName
if (-not $runRoot) { throw "Run root not found; run Path A setup first" }
$codexWindows = Get-Process | Where-Object { $_.MainWindowTitle -match "Codex" } |
    Select-Object ProcessName, Id, MainWindowTitle, MainWindowHandle
$codexWindows | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $runRoot "path-b-codex-window-process.json") -Encoding UTF8
```

Точная команда full-screen screenshot после отправки prompt и получения ответа:

```powershell
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$bounds = [System.Windows.Forms.SystemInformation]::VirtualScreen
$bitmap = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.CopyFromScreen($bounds.Left, $bounds.Top, 0, 0, $bounds.Size)
$screenshotPath = Join-Path $runRoot "path-b-codex-ui-capture.png"
$bitmap.Save($screenshotPath, [System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose()
$bitmap.Dispose()
```

Точная команда UIA readback dump, если UIAutomation доступен:

```powershell
Add-Type -AssemblyName UIAutomationClient
$codex = Get-Process | Where-Object { $_.MainWindowTitle -match "Codex" } | Sort-Object StartTime -Descending | Select-Object -First 1
if (-not $codex) { throw "Codex window not found" }
$root = [System.Windows.Automation.AutomationElement]::FromHandle($codex.MainWindowHandle)
$walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker
$items = New-Object System.Collections.Generic.List[object]
function Add-UiaNode {
    param([System.Windows.Automation.AutomationElement]$Node, [int]$Depth)
    if ($null -eq $Node -or $Depth -gt 5) { return }
    $items.Add([pscustomobject]@{
        depth = $Depth
        name = $Node.Current.Name
        automationId = $Node.Current.AutomationId
        controlType = $Node.Current.ControlType.ProgrammaticName
    })
    $child = $walker.GetFirstChild($Node)
    while ($null -ne $child) {
        Add-UiaNode -Node $child -Depth ($Depth + 1)
        $child = $walker.GetNextSibling($child)
    }
}
Add-UiaNode -Node $root -Depth 0
$items | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $runRoot "path-b-codex-ui-readback.json") -Encoding UTF8
```

Если screenshot на двух мониторах нечитаемый или окна перекрывают Codex:

1. Переместить Codex Desktop на основной монитор.
2. Закрыть или свернуть перекрывающие окна.
3. Повторить screenshot command.
4. Записать fallback в `path-b-local-ui-notes.md`, включая фразу
   `no secrets captured`.

Путь B должен создать эти evidence files:

- `path-b-codex-window-process.json`
- `path-b-codex-ui-capture.png`
- `path-b-codex-ui-readback.json` или `path-b-uia-unavailable.md`
- `path-b-local-ui-notes.md`
- `path-b-codex-installed-plugin-visibility.md`

`path-b-codex-installed-plugin-visibility.md` должен человеческим текстом
сказать, был ли plugin/skill реально виден или invoked в Codex UI. Если видна
только обычная chat response без доказательства route, это blocker, не PASS.

## 6. Joined comparison

После Path A и Path B будущий executor должен создать:

- `joined-row20-path-comparison.json`
- `row20-execution-report.md`
- `row20-status.json`

`joined-row20-path-comparison.json` должен сравнить:

- одинаковый Row20 prompt;
- одинаковый correlation id;
- Path A exit code и status;
- наличие Path A request/response/trace;
- Path B screenshot/readback;
- видимость или blocker installed Codex plugin route;
- non-claims;
- итоговый narrow claim.

`row20-status.json` должен сохранить `row20_accepted=false`, пока независимый
review не подтвердит допустимый evidence boundary. Этот prep repair сам Row20
accepted не ставит.

## 7. Success criteria

Prep считается готовым к независимому prep review, если:

- документ написан по-русски и понятен человеку;
- есть clear phase/step meaning для Сценарий 20;
- есть same-scenario Путь A и Путь B;
- есть точные команды для automated path;
- есть точные клики, visible states, session/login preflight, screenshot/UIA
  capture и two-monitor fallback для local UI path;
- перечислены exact evidence files для Path A, Path B и joined comparison;
- явно написано, что Row20 не доказывает готовность продукта;
- JSON содержит `next_work_order_text_ru` на русском с теми же командами и
  шагами;
- Row20 не отмечен accepted.

## 8. Rejection criteria

Независимый prep review должен вернуть NEEDS_FIX, если:

- текст снова начинается с machine/status language вместо человеческого смысла;
- Путь A или Путь B описан только на уровне намерений без точных команд/кликов;
- нет session/login preflight для Codex Desktop;
- нет fallback для двух мониторов или перекрытых окон;
- разрешено capturing secrets;
- evidence files названы неexactly или не разделены на Path A, Path B, joined;
- есть claim о product readiness, full Codex chat UI readiness, token savings,
  semantic quality, learnability/training, checkpoint/locator mutation или git;
- Row20 принят этим prep packet.

## 9. Next work order text in Russian

Bounded executor work order после PASS независимого prep review:

```text
Выполни только Сценарий 20 `frdp-p01-codex-full-extension-entrypoint` в директории `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-full-extension-entrypoint`. Сначала прочитай этот prep packet и skill `codex-xiigen-smart-skills:xiigen-smart-skills`. Не редактируй code. Не используй git. Не запускай training, eval, checkpoint mutation/promotion, locator update, external AI или product readiness checks. Путь A: выполни точные команды из раздела 4.1, выбери installed plugin root по указанному selector, запусти `node.exe src\codex-entrypoint.js` с Row20 prompt/correlation и сохрани `path-a-entrypoint-contract-review.json`, `path-a-request.json`, `path-a-response.json`, `path-a-trace.json`, `path-a-command-result.json`, `path-a-stdout.txt`, `path-a-stderr.txt`. Путь B: только после PASS prep review открой локальный Codex Desktop, проверь login/session без capture секретов, создай clean thread, выполни точные клики из раздела 5.2, отправь тот же prompt/correlation, сохрани `path-b-codex-window-process.json`, `path-b-codex-ui-capture.png`, `path-b-codex-ui-readback.json` или `path-b-uia-unavailable.md`, `path-b-local-ui-notes.md`, `path-b-codex-installed-plugin-visibility.md`. Если Codex UI route нельзя чисто доказать, запиши `CODEX_ROUTE_BLOCKED_CLEAN_SURFACE_NOT_PROVEN` и не называй это PASS. Затем создай `joined-row20-path-comparison.json`, `row20-execution-report.md`, `row20-status.json` с `row20_accepted=false` до независимого review. В отчете явно напиши: Row20 не доказывает готовность продукта, full Codex chat UI readiness, token savings, semantic quality, learnability/training, checkpoint mutation/promotion или locator update.
```

## 10. Узкий claim этого prep repair

Этот repair packet делает только одно: подготавливает человекочитаемый русский
сценарий Row20 с точными командами, точными кликами, evidence files,
success/rejection criteria и non-claims. Он не выполняет Row20, не принимает
Row20 и не заявляет readiness.
