# Row21 prep: Codex UI route recovery / exact blocker scenario

## Человеческий смысл

Сценарий 21 проверяет не новый backend-смоук и не повтор Row20, а отдельный пользовательский маршрут: можно ли из локального Codex Desktop/UI отправить маркерный prompt в установленный Codex plugin `codex-xiigen-smart-skills` и увидеть ответ в самом Codex UI.

После Row20 этот сценарий нужен потому, что Row20 доказал только установленный Codex plugin JS entrypoint до adapter/core boundary. Row20 не доказал Codex Desktop UI route, не доказал отправку prompt через UI и не доказал видимый ответ в Codex UI. Поэтому Row21 должен либо восстановить этот UI route, либо дать точный blocker с доказательствами, почему UI route сейчас нельзя проверить без слепого ввода или без захвата секретов.

Успех Row21 означает: тот же маркерный сценарий имеет два связанных доказательства. Путь A подтверждает установленный entrypoint/adaptor boundary как поддерживающее доказательство. Путь B подтверждает, что локальный Codex Desktop/UI принял prompt именно через подтвержденный UI input и показал видимый ответ/маркер, либо фиксирует точный blocker, который не дает безопасно выполнить UI route. Успех не доказывает готовность продукта, не доказывает token savings, не доказывает semantic quality, не доказывает training/learnability, не доказывает checkpoint promotion и не обновляет locator, если locator отдельно не доказан.

## Общий marker и граница claims

- Scenario id: `frdp-p01-codex-codex-chat-ui-marker`.
- Маркерный prompt для обоих путей:

```text
Row21 Codex UI route marker. Use installed XIIGen Smart Skills Codex plugin if available. Return a short visible marker: ROW21_CODEX_UI_ROUTE_MARKER.
```

- Ожидаемый видимый marker: `ROW21_CODEX_UI_ROUTE_MARKER` или видимый redacted debug marker `xiigen prompt-to-task :` плюс связанный response artifact из Path A.
- Путь A не заменяет Путь B. Путь A нужен только как поддерживающее доказательство установленного plugin entrypoint.
- Путь B является решающим для UI route pass/blocker.
- Запрещено: не использовать слепой ввод; не вводить prompt в неподтвержденное окно; не вставлять prompt через clipboard, пока не подтвержден целевой input; не записывать секреты, токены, email/password, private chat content.

## Путь A: automated/supporting proof

Цель Path A: подтвердить, что установленный Codex plugin bundle и его entrypoint доступны и пишут request/response/trace для того же Row21 marker. Это supporting proof, не UI proof.

Целевая установленная папка:

```text
C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500
```

Команды, которые должен выполнить следующий executor, если ему отдельно разрешат выполнение сценария:

```powershell
$ErrorActionPreference = "Stop"
$scenarioDir = "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-codex-chat-ui-marker"
$pluginRoot = "C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500"
$entrypoint = Join-Path $pluginRoot "src\codex-entrypoint.ps1"
$pathADir = Join-Path $scenarioDir "path-a-entrypoint"
New-Item -ItemType Directory -Force -Path $pathADir | Out-Null

Get-Item -LiteralPath (Join-Path $pluginRoot "package.json") |
  Select-Object FullName,Length,LastWriteTime |
  ConvertTo-Json -Depth 4 |
  Set-Content -LiteralPath (Join-Path $pathADir "row21-path-a-plugin-entrypoint-file.json") -Encoding UTF8

Get-Item -LiteralPath (Join-Path $pluginRoot "src\codex-entrypoint.js"), $entrypoint |
  Select-Object FullName,Length,LastWriteTime |
  ConvertTo-Json -Depth 4 |
  Set-Content -LiteralPath (Join-Path $pathADir "row21-path-a-entrypoint-files.json") -Encoding UTF8

& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $entrypoint `
  -Method "prompt-to-task" `
  -UserPromptOrTaskText "Row21 Codex UI route marker. Use installed XIIGen Smart Skills Codex plugin if available. Return a short visible marker: ROW21_CODEX_UI_ROUTE_MARKER." `
  -HostFactsJson '{"row":"21","scenario":"frdp-p01-codex-codex-chat-ui-marker","path":"A","supporting_only":"true"}' `
  -RequestedSkillsJson '["codex-xiigen-smart-skills:xiigen-smart-skills"]' `
  -AvailableSkillsJson '["codex-xiigen-smart-skills:xiigen-smart-skills"]' `
  -CheckSetInputsJson '{"row21_marker":"ROW21_CODEX_UI_ROUTE_MARKER","ui_route_required":"true"}' `
  -ProvenanceJson '{"scenario":"Row21","path":"A","claim_boundary":"supporting_entrypoint_only"}' `
  -CorrelationId "row21-path-a-codex-entrypoint" `
  -RequestPath (Join-Path $pathADir "row21-path-a-request.json") `
  -ResponsePath (Join-Path $pathADir "row21-path-a-response.json") `
  -TracePath (Join-Path $pathADir "row21-path-a-trace.json") `
  -DebugEnabled *>&1 |
  Tee-Object -FilePath (Join-Path $pathADir "row21-path-a-entrypoint-console.log")

Get-Content -LiteralPath (Join-Path $pathADir "row21-path-a-response.json") -Raw |
  ConvertFrom-Json |
  ConvertTo-Json -Depth 20 |
  Set-Content -LiteralPath (Join-Path $pathADir "row21-path-a-response-normalized.json") -Encoding UTF8
```

Ожидаемые Path A outputs:

- `path-a-entrypoint/row21-path-a-plugin-entrypoint-file.json`
- `path-a-entrypoint/row21-path-a-entrypoint-files.json`
- `path-a-entrypoint/row21-path-a-request.json`
- `path-a-entrypoint/row21-path-a-response.json`
- `path-a-entrypoint/row21-path-a-trace.json`
- `path-a-entrypoint/row21-path-a-entrypoint-console.log`
- `path-a-entrypoint/row21-path-a-response-normalized.json`

Path A pass evidence:

- request содержит `CorrelationId = row21-path-a-codex-entrypoint`.
- request содержит `UserPromptOrTaskText` с `ROW21_CODEX_UI_ROUTE_MARKER`.
- response/trace существуют и парсятся как JSON.
- response или trace показывает adapter/core boundary для Codex host.

Path A rejection:

- entrypoint file отсутствует;
- response/trace не созданы;
- JSON не парсится;
- output пытается заявить UI pass. Это rejection, потому что Path A не является UI proof.

## Путь B: local Codex Desktop/UI route attempt

Цель Path B: проверить тот же scenario marker через локальное приложение Codex Desktop/UI. Это единственный путь, который может дать pass по UI route. Если безопасный UI route нельзя подтвердить, Path B должен вернуть точный blocker.

Целевое окно/приложение:

- Локальное приложение Codex Desktop, окно с заголовком/видимым chrome Codex.
- Целевой workspace/thread: `C:\Projects\llm_mvp_core`, если он уже открыт в текущем Codex Desktop session.
- Нельзя вводить текст в браузер, терминал, редактор, shell, другой chat или неподтвержденное окно.

Session/login preflight:

1. Сначала сделать screenshot полного рабочего стола или bounds целевого окна без ввода: `path-b-ui/row21-path-b-preflight-desktop-or-window.png`.
2. Проверить, что visible app действительно Codex Desktop: заголовок окна, sidebar/thread chrome, composer/chat area или другой устойчивый Codex UI marker.
3. Проверить, что session не находится на anonymous/login screen. Если открыт login/auth screen, не вводить email/password/token; зафиксировать blocker `UI_AUTH_REQUIRED_NO_SECRETS_CAPTURED`.
4. Если экран содержит секреты, токены, recovery codes, email/password или private account panels, не снимать детальный screenshot; использовать redacted screenshot или UIA metadata без секретов и зафиксировать `SECRET_RISK_ABORTED`.

UI strategy 1: UIA-first, если clean UIA controls доступны.

1. Получить список top-level windows и выбрать только окно Codex Desktop по title/process/bounds.
2. Получить UIA tree для выбранного окна и найти chat composer control типа `Edit`, `TextBox`, `Document` или явный input с доступным bounding rectangle.
3. Сохранить metadata без содержимого private chat: `path-b-ui/row21-path-b-uia-window-tree-redacted.json`.
4. Перед вводом сделать screenshot с выделением/описанием target bounds: `path-b-ui/row21-path-b-input-target-confirmed.png`.
5. Только если target input подтвержден, сфокусировать его click/keyboard focus по UIA pattern или по подтвержденному rectangle внутри окна Codex.
6. Вставить marker prompt через clipboard-safe paste: сохранить предыдущее содержимое clipboard metadata только как length/type, установить prompt, нажать `Ctrl+V`, затем немедленно очистить или восстановить clipboard после отправки.
7. Нажать `Enter` или подтвержденную кнопку Send только после screenshot/focus evidence.
8. Снять `path-b-ui/row21-path-b-after-submit.png`.
9. Дождаться видимого response/marker без дополнительных prompts и снять `path-b-ui/row21-path-b-visible-response.png`.

UI strategy 2: screenshot/focus-confirmed fallback, если clean UIA controls не exposed.

1. Сделать screenshot целевого Codex Desktop окна и записать bounds выбранного окна: `path-b-ui/row21-path-b-window-focus-candidate.json`.
2. Подтвердить, что окно активно: title/process/bounds совпадают, target window frontmost, screenshot показывает Codex chrome и composer area.
3. Если composer area видима, но UIA input недоступен, допускается один manual-assisted click только по подтвержденному composer rectangle внутри Codex window. До click должны быть сохранены screenshot-before и target rectangle evidence.
4. После click проверить focus indicator/caret или UI state change screenshot: `path-b-ui/row21-path-b-focus-after-click.png`.
5. Только после подтверждения focus вставить prompt через clipboard-safe paste и отправить. Если focus/caret не подтвержден, abort: не использовать слепой ввод.
6. Если окно перекрыто, на другом мониторе, свернуто, имеет несколько похожих Codex windows или target rectangle спорный, не вводить prompt; классифицировать blocker.

Two-monitor/overlap fallback:

- Если Codex Desktop не на primary monitor, сначала записать monitor layout metadata: `path-b-ui/row21-path-b-monitor-layout.json`.
- Если возможно безопасно, переместить или развернуть окно Codex Desktop на primary monitor без ввода текста и сделать новый screenshot.
- Если окно перекрыто другим окном или screenshot не показывает composer, закрывать/перетаскивать чужие окна нельзя. Зафиксировать `UI_TARGET_OCCLUDED_OR_AMBIGUOUS`.
- Если обнаружено несколько Codex windows и нельзя доказать нужный workspace/thread, зафиксировать `MULTIPLE_CODEX_WINDOWS_AMBIGUOUS`.

Path B pass evidence:

- `path-b-ui/row21-path-b-preflight-desktop-or-window.png`
- `path-b-ui/row21-path-b-uia-window-tree-redacted.json` или `path-b-ui/row21-path-b-window-focus-candidate.json`
- `path-b-ui/row21-path-b-input-target-confirmed.png`
- `path-b-ui/row21-path-b-after-submit.png`
- `path-b-ui/row21-path-b-visible-response.png`
- `path-b-ui/row21-path-b-ui-route-result.json`
- `path-b-ui/row21-path-b-screen-recording.mp4` если разрешена запись и она не захватывает секреты.

Pass требует все условия:

- целевое окно Codex Desktop подтверждено до ввода;
- prompt введен только в подтвержденный Codex UI input;
- видимый response/marker появился в Codex UI;
- evidence не содержит секреты;
- Path A artifacts существуют для same-scenario comparison.

Path B rejection:

- prompt был отправлен без подтвержденного target input;
- использован blind input, coordinate-only click без screenshot/target/focus evidence или paste в неподтвержденное окно;
- screenshot/video содержит секреты без redaction;
- response виден только в terminal/log/Path A, но не в Codex UI;
- UI marker не связан с Row21 prompt;
- claim говорит "готовность продукта" или "token savings", хотя сценарий этого не доказывает.

## Точный blocker

Точный blocker означает, что executor безопасно пытался подготовить UI route по правилам Path B, но не смог дойти до ввода/отправки/видимого ответа из-за конкретной, доказанной причины. Blocker отличается от pass тем, что pass показывает confirmed UI input + отправку prompt + visible response в Codex UI, а blocker показывает, почему этот route нельзя выполнить безопасно сейчас.

Допустимые blocker classes:

- `CODEX_DESKTOP_NOT_RUNNING`: нет видимого/доступного окна Codex Desktop.
- `UI_AUTH_REQUIRED_NO_SECRETS_CAPTURED`: Codex открыт на login/auth screen; секреты не вводились и не снимались.
- `UI_INPUT_CONTROL_NOT_FOUND`: target window подтверждено, но composer/input не найден ни UIA, ни screenshot-safe способом.
- `UI_TARGET_OCCLUDED_OR_AMBIGUOUS`: окно перекрыто, не видно composer, monitor/window layout не позволяет безопасный ввод.
- `MULTIPLE_CODEX_WINDOWS_AMBIGUOUS`: несколько Codex windows, нужный workspace/thread нельзя доказать.
- `FOCUS_NOT_CONFIRMED_ABORTED`: target input возможно найден, но focus/caret/active control не подтвержден; не использовать слепой ввод.
- `CLIPBOARD_OR_SECRET_RISK_ABORTED`: безопасная clipboard вставка или redacted evidence невозможны без риска секретов.
- `NO_VISIBLE_RESPONSE_AFTER_CONFIRMED_SUBMIT`: prompt подтвержденно отправлен в Codex UI, но response/marker не появился в допустимый timeout.

Blocker evidence must include:

- `path-b-ui/row21-path-b-blocker.json` with blocker class, exact failed step, attempted strategy, abort reason, and no-secrets statement.
- screenshot/UIA metadata proving the blocker without secrets.
- `path-b-ui/row21-path-b-no-blind-input-attestation.txt` stating no prompt was typed/pasted into unconfirmed target.

## Joined comparison

Joined comparison должен соединить Path A и Path B без расширения claims:

- `row21-joined-comparison.json`
- `row21-joined-comparison.md`

Comparison fields:

- `scenario_id`
- `path_a_status`
- `path_b_status`
- `same_marker_used`
- `ui_route_result = pass | blocker | reject`
- `blocker_class_or_null`
- `claim_boundary`
- `non_claims`

Joined pass возможен только при Path A support pass и Path B UI pass. Joined blocker возможен при Path A support pass или fail, если Path B содержит точный blocker evidence. Joined comparison не доказывает готовность продукта.

## Success criteria

- Сценарий 21 выполнен как same-scenario dual path.
- Путь A artifacts созданы и показывают только supporting entrypoint/adaptor boundary.
- Путь B либо показывает confirmed Codex Desktop UI route с visible marker, либо дает точный blocker.
- Все screenshots/video/UIA metadata не содержат секретов.
- `row21-joined-comparison.json` ясно отделяет pass от blocker.
- Non-claims сохранены: no product readiness, no token savings, no semantic quality, no training/learnability, no checkpoint promotion, no locator update unless separately proven.

## Rejection criteria

- Нет Path B evidence.
- Path A выдается за UI proof.
- Использован или не исключен blind input.
- Нет точной blocker classification при UI failure.
- Evidence захватывает секреты.
- Нет joined comparison.
- Есть claim, что Row21 доказывает product readiness, token savings, semantic quality, training/learnability, checkpoint promotion или locator update.

## Next work order text RU

Исполнитель Row21 должен выполнить этот сценарий только после отдельного разрешения на product/UI execution. Сначала выполнить Path A команды из этого prep и сохранить listed evidence files. Затем выполнить Path B в локальном Codex Desktop: подтвердить окно, session/login state, target input, focus и no-secrets boundary; отправлять marker prompt только в подтвержденный input; если target/focus/input/session нельзя подтвердить, немедленно остановить ввод и записать точный blocker. После этого создать joined comparison и вернуть только narrow claim: UI route pass или exact blocker, без claim о готовности продукта.
