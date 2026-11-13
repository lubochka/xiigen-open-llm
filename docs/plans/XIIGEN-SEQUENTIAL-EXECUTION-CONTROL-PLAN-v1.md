# XIIGen sequential execution-control plan v1

Документ создан для одной цели: убрать прыжки по фазам. Дальше проект
выполняется только по одному числовому списку фаз. Все старые имена вроде
QSG, DCM, CL, CU, CX, REVIEW, tail, repair или formatting могут существовать
только как подпункты внутри активной числовой фазы.

Дата фиксации контрольной доски: 2026-06-24.

## 1. Текущая команда Лубы и граница полномочий

Текущая команда Лубы:

> Мне нужен новый план который точно по порядку будет выполняться и в котором
> будет абсолютно ясно на этапе выполнения где мы - какая фаза, можно только
> цифры, что конйкретно это фаза решает, и сколько осталось действийдо
> завершения человеческим языком. Какие фазы остались, какие полностью
> завершены- эти прыжки по фазам это абсолютный бардак!!!

Дополнительное требование Лубы для этой версии:

> done/in-progress/remaining, - тут важно по подшагам нумерация: номер фазы.
> номер шага. статус

Что разрешено сейчас: создать этот план.

Текущее обновление Phase 14 после p14-fix-11:

> Review p14-fix-10 evidence, fix confusing wording if needed, and update the
> active Phase 14 status if the evidence qualifies as Tier B DONE_WITH_SCOPE
> for 14.04.

Текущее обновление Phase 14 после замечания Лубы о невидимом desktop-run:

> Я не вижу что бы что то происходило на компе - может нужно более детально
> обьяснить как запускать программы через владение скрином???

Текущее обновление Phase 14 после p14-fix-13 real desktop screen-control run:

> Execute Phase 14.13 real desktop screen-control proof for XIIGen extension
> hosts on this computer ... It must try real visible applications for Claude,
> Cursor, and Codex; if a host cannot be controlled, record exact blocker and
> do not mark that host full-ready.

Исторический результат p14-fix-13, superseded by p14-fix-14, записан в
`artifacts/extension-runtime-token-quality-learnability/phase-14-fixes/real-program-ui-proof/p14-fix-13-real-desktop-screen-control-20260624T041937Z/`.
Cursor получил fresh real app marker screenshot, Claude получил fresh real app
marker screenshot in input without prompt submission, но оба host остаются
extension-blocked for full host runtime. Codex получил real Codex app/plugin
surface screenshots and local installed-plugin core-boundary trace, но accepted
Codex visible marker не был получен в этом run. Это только historical/superseded
status p14-fix-13; он не является активным текущим указателем после p14-fix-14.

Текущее обновление Phase 14 после p14-fix-14 Codex-only visible marker run:

Результат p14-fix-14 записан в
`artifacts/extension-runtime-token-quality-learnability/phase-14-fixes/real-program-ui-proof/p14-fix-14-codex-visible-marker-20260624T044217Z/`.
Codex активирован как реальное окно
`C:\Program Files\WindowsApps\OpenAI.Codex_26.616.6631.0_x64__2p2nqsd0c76g0\app\Codex.exe`.
Commit/review-safe screenshot `review-safe/codex-command-palette-visible-marker-review-safe.png`
is the preferred pushed evidence copy; raw screenshot
`raw/codex/codex-command-palette-visible-marker-lowlevel-paste.png`
remains local/raw source evidence.
Raw source screenshot `raw/codex/codex-command-palette-visible-marker-lowlevel-paste.png`
показывает Codex desktop chrome, Codex command/search palette и fresh marker
`P14-REAL-CODEX-20260624T044217Z`. Same-run installed-plugin/core trace
`traces/codex/trace.json` использует тот же marker как `correlation_id`.
Codex status for 14.13: `DONE_REAL_APP_SCREENSHOT_AND_EXTENSION_TRACE`.
Scope limitation: это доказывает Codex app/plugin visible marker plus
installed-plugin/core boundary only; это не доказывает full Codex chat UI
readiness, token savings, semantic quality improvement, learnability, training
или checkpoint mutation.

Активный статус 14.13 после p14-fix-14:
`DONE_WITH_SCOPE_REAL_APP_SCREENSHOTS_COLLECTED`.

Активный счетчик плана после p14-fix-14: 115/115 substeps
DONE-with-scope; 0 IN_PROGRESS; 0 NEEDS_FIX; 0 REMAINING. Это не product
readiness, не full extension readiness и не full chat UI readiness.

Для текущего status-consistency repair разрешено только исправить wording/counts
в этом плане. Не разрешено запускать новый UI test, заявлять product readiness,
заявлять packaged Claude Desktop/chat-tab readiness, заявлять full Codex chat UI
readiness, редактировать unrelated files или выполнять git stage/commit/push.

Что не разрешено сейчас:

- не редактировать код;
- не продолжать commit/push фазы 03;
- не начинать фазу 04;
- не запускать `git add`, `git commit`, `git push`;
- не заявлять готовность проекта;
- не использовать внешний AI, внешние модели, браузерные/хостовые прогоны или
  неродственные инструменты без отдельного явного разрешения Лубы.

Роль parent-сессии после принятия плана: только оркестрация, review и commit
принятых изменений. Исполнение и review выполняют bounded sub-agents.

## 2. Честное состояние сейчас

Старый Phase 14.12 final boundary остается принят только как scoped boundary по
уже записанным evidence. После p14-fix-13 и p14-fix-14 отдельный шаг 14.13
теперь закрыт в честном scoped статусе:
`DONE_WITH_SCOPE_REAL_APP_SCREENSHOTS_COLLECTED`.

Счетчик текущего плана после p14-fix-14: 115 подшагов; 115 DONE-with-scope;
0 IN_PROGRESS; 0 NEEDS_FIX; 0 REMAINING. Внутри 14.13 больше нет remaining
work item; остаются только ограничения/non-claims для будущей product work.

Host evidence для 14.13:

- Claude: `DONE_REAL_APP_SCREENSHOT_ONLY_EXTENSION_BLOCKED`. Есть реальный
  Claude app marker screenshot from p14-fix-13; external prompt was not
  submitted; XIIGen extension/runtime trace from Claude UI не доказан.
  Commit/review-safe screenshot:
  `artifacts/extension-runtime-token-quality-learnability/phase-14-fixes/real-program-ui-proof/p14-fix-13-real-desktop-screen-control-20260624T041937Z/review-safe/claude-window-real-marker-review-safe.png`;
  raw screenshot remains local/raw evidence.
- Cursor: `DONE_REAL_APP_SCREENSHOT_ONLY_EXTENSION_BLOCKED`. Есть реальный
  Cursor app marker screenshot from p14-fix-13; XIIGen extension/runtime
  invocation from Cursor UI в p14-fix-13 не доказан.
  Commit/review-safe screenshot:
  `artifacts/extension-runtime-token-quality-learnability/phase-14-fixes/real-program-ui-proof/p14-fix-13-real-desktop-screen-control-20260624T041937Z/review-safe/cursor-window-real-marker-review-safe.png`;
  raw screenshot remains local/raw evidence.
- Codex: `DONE_REAL_APP_SCREENSHOT_AND_EXTENSION_TRACE`. Есть реальный Codex
  Desktop/plugin surface screenshot with marker `P14-REAL-CODEX-20260624T044217Z`
  from p14-fix-14 plus same-marker installed-plugin/core trace; trace has
  `host_runtime_claimed=false`.
  Commit/review-safe screenshot:
  `artifacts/extension-runtime-token-quality-learnability/phase-14-fixes/real-program-ui-proof/p14-fix-14-codex-visible-marker-20260624T044217Z/review-safe/codex-command-palette-visible-marker-review-safe.png`;
  raw screenshot remains local/raw evidence.

Это не абсолютная product-readiness и не marketing-ready claim. Не доказаны:
packaged Claude Desktop/chat-tab readiness, Cursor extension/runtime invocation
from the p14-fix-13 UI, full Codex chat UI readiness, full extension readiness,
token savings, semantic quality improvement, learnability/training,
checkpoint mutation/promotion и locator update сверх записанного evidence.

Phase 04 `PromptToTask universal task contract` считается DONE:
PromptToTask universal contract slice passed review, parent commit/push boundary
завершен, изменения pushed through commit `0f1279047`, branch synced. Эта
контрольная доска фиксирует фазу 04 как закрытую и переносит status-pointer на
фазу 05 без старта implementation.

Phase 05 `OrchestrationReasoning full trained planner` считается DONE:
OrchestrationReasoning trained planner slice passed review, parent commit/push
boundary завершен, изменения pushed through commit `e768fc254`, branch synced.
Эта контрольная доска фиксировала фазу 05 как закрытую; последующая фаза 06
закрыта отдельным boundary ниже.

Phase 06 `SkillSelection full trained selector` считается DONE:
SkillSelection trained selector slice passed review, parent commit/push boundary
завершен, изменения pushed through commit `5e9dbaaa9`, branch synced. Эта
контрольная доска фиксирует фазу 06 как закрытую и переносит status-pointer на
фазу 07 без старта implementation.

Phase 07 `ResultGap, missing-pattern and correction DPO` считается DONE:
ResultGap/missing-pattern/correction DPO slice passed review, writer-level tests
PASS, parent commit/push boundary завершен, изменения pushed through commit
`5adc48c50`, branch synced. Result gaps и user corrections теперь создают
safe/quarantined DPO candidates; generic bad outputs quarantined и не считаются
train-ready. Эта контрольная доска фиксирует фазу 07 как закрытую и переносит
status-pointer на фазу 08 без старта implementation.

Phase 08 `Common/user two-leg learning lifecycle` считается DONE:
Common/global и personal/adaptive legs разделены; representative SkillSelection
user lifecycle доказывает backup/export/import/restore/share approval,
continue-training и rollback без absolute-path leakage. Evidence: commit
`56146a268`, 3 reviews PASS, Phase08 tests PASS. Non-claim: executable
lifecycle proof является SkillSelection-only; PromptToTask,
OrchestrationReasoning и ResultGap/correction остаются required, но не были
executed в Phase 08. Эта контрольная доска фиксирует фазу 08 как закрытую и
переносит status-pointer на фазу 09 без старта implementation.

Phase 09 `Claude full extension live proof` считается DONE:
Claude package/entrypoint wiring доказан; handler-level request path до core
работает; visible success/error path записан; debug off/on traces проверены;
same-run correlation id подтверждает core handling в той же runtime-сессии.
Evidence: Claude commits `cc953ef`, `e4eaac7`; core evidence commit
`97bf9348e`; review 1/2 after fix PASS, review 2/2 PASS, targeted format review
PASS. Non-claim: это handler-level Claude proof, не полный Claude Desktop UI и
не Cursor/Codex/all-host readiness. Эта контрольная доска фиксирует фазу 09 как
закрытую и переносит status-pointer на фазу 10 без старта implementation.

Phase 10 `Cursor full extension live proof` считается DONE:
Cursor package/entrypoint command wiring доказан; bounded Extension Host command
path до core работает; visible success/error path записан; debug off/on traces
проверены; same-run correlation id подтверждает core handling в той же
runtime-сессии. Evidence: Cursor commit `c92cf73`; core evidence commit
`5de1f3e69`; review 1/2 PASS, review 2/2 PASS, targeted format review PASS.
Non-claim: это bounded Cursor Extension Host command proof, не full manual
Cursor UI screenshot и не all-host readiness. Эта контрольная доска фиксирует
фазу 10 как закрытую и переносит status-pointer на фазу 11 без старта
implementation.

Phase 11 `Codex full extension live proof` считается DONE:
installed Codex plugin skill surface loaded; `src/codex-entrypoint.js` ran
through node; debug off/on reached `llm_mvp_core`; controlled
`invalid_request` error path visible; same-run correlation id recorded; two
reviews PASS. Evidence: `87e125e1d phase 11: add codex plugin proof evidence`;
`2d631cb5b governance: require fail-fast commit prechecks`. Non-claim: Phase 11
не доказал token savings, quality improvement, learnability, training,
checkpoint mutation, full Codex chat UI или all-host readiness. Эта контрольная
доска фиксирует фазу 11 как закрытую и переносит status-pointer на фазу 12 без
старта implementation.

## 3. Мы сейчас здесь

Активная фаза ровно одна:

Фаза 12/14: DONE / готово. Cross-host token and quality comparison.

Что решила фаза 12: проверила cross-host token and quality comparison через
adapter/core traces и evidence по model checkpoint, не превращая token delta в
заявление о полной готовности продукта.

Шаги 12.01-12.10: DONE / готово.

Что доказано Phase 12: 9/9 adaptive responses имеют
`orchestration_reasoning_status=orchestration_plan_produced`; 9/9 internal
learned status; 0 fallback matches; 9/9 structured plan evidence; 9/9 adaptive
rubric pass; 9/9 token reduction accepted; targeted tests PASS, 35/35 for
`OrchestrationReasoningStateLoadingTests` and
`XiigenExtensionApiContractsTests`.

Evidence commits Phase 12: `f1697513b phase 12: record blocked token quality
evidence`; `1b9eee8a7 phase 12: produce universal orchestration evidence`.

Фаза 13/14: DONE / готово. QA automation, documentation, and no-overclaim
evidence.

Шаги 13.01-13.08: DONE / готово.

Что решила фаза 13: сделала dashboard/API/smoke evidence автоматизированным,
человеко-читаемым и безопасно ограниченным, чтобы его нельзя было выдать за
final readiness.

Evidence commit Phase 13: `31471d277 phase 13: add qa docs no-overclaim
evidence`.

Parent verification Phase 13 PASS: `tools/phase13-qa-docs-no-overclaim.ps1
-Json`; `dotnet test Tests/LlmFromScratch.Tests.csproj --filter
"ApiDashboard|XiigenExtensionApi" --no-restore --nologo` PASS, 83/83; `dotnet
test Tests.ApiDashboardWpf/Tests.ApiDashboardWpf.csproj --no-restore --nologo`
PASS, 9/9; no-overclaim self-check rejected the positive forbidden line and
allowed the negative wording; generated artifacts were fixed to UTF-8 no BOM,
LF-only, no trailing whitespace.

Фаза 14/14: DONE_WITH_SCOPE_REAL_APP_SCREENSHOTS_COLLECTED. Десять финальных
review, 14.12 scoped boundary и 14.13 real desktop screen-control evidence
закрыты в записанном scope. Это не полная product readiness и не full extension
readiness.

Шаг 14.13/14.13: DONE_WITH_SCOPE_REAL_APP_SCREENSHOTS_COLLECTED / real desktop
screen-control proof collected with explicit host limitations.
Старый шаг 14.12/14.12 остается DONE только как scoped final boundary. Boundary
packet:
`artifacts/extension-runtime-token-quality-learnability/phase-14-final-boundary/p14-final-boundary-20260624-070127/phase14-final-boundary-report.md`
и machine-readable sibling
`artifacts/extension-runtime-token-quality-learnability/phase-14-final-boundary/p14-final-boundary-20260624-070127/phase14-final-boundary-report.json`.

Что решила фаза 14 сейчас: собрала финальный readiness-пакет, выполнила 12
final review/status rows, закрыла lifecycle/DPO строки 14.08 и 14.09 через
P14-FIX-03, приняла 14.02 как DONE with scope, приняла 14.04 только как
`DONE_WITH_SCOPE_TEST_ONLY_VISIBLE_HOOK`, а затем 14.12 подтвердил, что все
строки 14.01-14.12 поддержаны записанным evidence и что ограничения остаются
видимыми. После замечания Лубы добавлен 14.13: p14-fix-13 собрал реальные
видимые app screenshots для Cursor и Claude, а p14-fix-14 закрыл Codex real app
marker screenshot plus installed-plugin/core trace. JSON-only report или
adapter/core smoke сами по себе 14.13 не закрывают.

Общие счетчики плана: всего подшагов 115; после p14-fix-14 готово 115/115
DONE-with-scope. In progress 0/115; blocked by review / needs fix 0/115;
осталось 0/115.

Host rows после p14-fix-14:

- Claude: `DONE_REAL_APP_SCREENSHOT_ONLY_EXTENSION_BLOCKED`. Есть screenshot
  реального Claude app window/input with marker from p14-fix-13; external prompt
  не отправлялся; XIIGen extension/runtime trace from Claude UI не доказан.
  Preferred commit/review-safe screenshot:
  `review-safe/claude-window-real-marker-review-safe.png`; raw screenshot remains
  local/raw evidence.
- Cursor: `DONE_REAL_APP_SCREENSHOT_ONLY_EXTENSION_BLOCKED`. Есть screenshot
  реального Cursor window with marker from p14-fix-13; XIIGen extension/runtime
  invocation from Cursor UI в p14-fix-13 не доказан.
  Preferred commit/review-safe screenshot:
  `review-safe/cursor-window-real-marker-review-safe.png`; raw screenshot remains
  local/raw evidence.
- Codex: `DONE_REAL_APP_SCREENSHOT_AND_EXTENSION_TRACE` after p14-fix-14. Есть
  screenshot реального Codex desktop/app/plugin surface. Preferred
  commit/review-safe screenshot:
  `review-safe/codex-command-palette-visible-marker-review-safe.png`; raw
  `raw/codex/codex-command-palette-visible-marker-lowlevel-paste.png` remains
  local/raw evidence. Fresh
  marker `P14-REAL-CODEX-20260624T044217Z` plus same-run installed-plugin/core
  trace `traces/codex/trace.json`. Scope limitation: не full Codex chat UI
  readiness and not product readiness; trace says `host_runtime_claimed=false`.

Текущий шаг 14.04 закрыт только в этом accepted scope. Он доказывает
test-only visible hook surface plus same-run trace, а не packaged Claude
Desktop/chat-tab readiness, не full Codex chat UI readiness и не final product
readiness. Старый product-hook packet `fb01d6d87` и adapter/core traces остаются
supporting only: adapter/component/core smoke сам по себе не доказывает видимую
UI/session поверхность.

Phase 14 принимает такие evidence tiers для extension UI proof:

- Tier A: isolated real program UI session with screenshot/render capture plus
  same-run trace correlation. Это самый сильный UI proof.
- Tier B: authorized current visible real program UI session, visible screen
  control run, or sub-agent visible runtime surface with screenshot/render
  capture plus same-run trace correlation. Это принимается как DONE_WITH_SCOPE,
  когда privacy/redaction, exact visible-control attempt и limitations записаны
  явно.
- Tier C: adapter/component/core smoke only. Это полезное supporting evidence,
  но оно никогда не закрывает full UI proof само по себе. Adapter/core-only
  smoke remains Tier C supporting evidence and is never full UI proof by itself.

Для 14.04 закрывающий proof больше не может быть тем же adapter/core smoke или
redacted app-frame screenshot без visible fresh marker. Достаточно
real-program screenshot proof, если он показывает видимый Claude/Codex runtime
result или visible marker/result surface, same-run `llm_mvp_core`
request/response/trace correlation, отсутствие утечки чужих приватных данных и
честные no-overclaim ограничения.

#### Phase 14 screen-control runbook для real desktop app proof

Каждый future real UI test sub-agent обязан выполнить эти шаги по каждому host:

1. `14.13.Claude`: запустить или активировать реальное установленное приложение
   Claude Desktop/app на текущем Windows desktop. Если установлен только другой
   Claude surface, например Claude Code UI, это записывается как scoped surface
   и не закрывает packaged Claude Desktop/app readiness.
2. `14.13.Cursor`: запустить или активировать реальное установленное приложение
   Cursor, открыть настоящий Cursor window или Extension Host window, где виден
   результат extension/runtime run.
3. `14.13.Codex`: запустить или активировать реальное установленное Codex
   desktop/app окно или Codex plugin surface в Codex desktop UI. Node adapter,
   CLI-only smoke или plugin package load без видимого Codex app window не
   закрывает этот host.
4. Использовать видимое окно в текущем desktop session. Hidden, background,
   minimized, headless, off-screen, test-only helper window или screenshot из
   synthetic harness не закрывает real UI readiness.
5. Вывести реальное приложение на передний план. Разрешенные способы:
   Windows Start menu/app launcher, `Start-Process` только для visible launch,
   existing process activation, UIAutomation/WinAppDriver/manual screen-control
   tool, или PowerShell foreground activation через OS window handle. Если
   automation tool недоступен, agent обязан записать exact blocker.
6. Ввести или отобразить уникальный run marker в самом окне приложения или в
   human-readable result surface внутри этого окна:
   `P14-REAL-<HOST>-<UTC_TIMESTAMP>`. Marker должен быть видим на screenshot
   вместе с понятным output/result text.
7. Выполнить минимальный same-run input/output path через host surface, когда
   это доступно: prompt/command/panel action -> visible result -> trace. Сохранить
   input, output, token/quality trace и correlation id, если host/runtime их
   отдает. Trace полезен, но trace alone is insufficient.
8. Сделать screenshot actual application window или whole desktop containing
   the real app window. Screenshot path должен быть записан рядом с коротким
   human-readable observation: какое приложение видно, какой marker/output
   виден, какой host status это закрывает или не закрывает.
9. Если host не получается контролировать через UI automation, записать exact
   blocker, screenshot текущего blocker/surface если безопасно, и продолжить
   остальные hosts. Нельзя закрывать full readiness для заблокированного host.

#### Phase 14 rejection criteria для real UI proof

- JSON-only evidence rejected: JSON/trace/report без screenshot настоящего окна
  приложения не закрывает real UI readiness.
- Adapter/core smoke only rejected for full UI readiness: adapter smoke,
  component smoke, node entrypoint, package load или core trace остаются только
  supporting evidence.
- Test-only hook accepted only as scoped fallback, not full app readiness:
  test-only visible hook может остаться `DONE_WITH_SCOPE_TEST_ONLY_VISIBLE_HOOK`,
  но не становится `DONE_REAL_APP_SCREENSHOT`.
- Screenshot of unrelated helper window rejected: helper, terminal, harness,
  browser/devtools или redacted blank frame не заменяют окно Claude/Cursor/Codex.
- Screenshot without visible marker/output rejected: на screenshot должен быть
  fresh marker и человекочитаемый result/output.
- Process/window-list without screenshot rejected: список процессов, window
  handles или foreground logs без screenshot не доказывают видимое UI-событие.

#### Phase 14 human-readable status model

| phase.step | what it solves | status | evidence needed | evidence present | remaining work |
|---|---|---|---|---|---|
| 14.02.Claude | Проверяет Claude host/runtime surface | DONE_REAL_APP_SCREENSHOT_ONLY_EXTENSION_BLOCKED for 14.13; older 14.02 host/runtime remains DONE_WITH_SCOPE_TEST_ONLY_VISIBLE_HOOK | Claude Desktop/app window foreground screenshot with `P14-REAL-CLAUDE-<UTC_TIMESTAMP>`, visible output if safely available, screenshot path, observation, same-run trace if available | p14-fix-13 real Claude app/input marker screenshot; no external prompt submitted | No remaining 14.13 screenshot work; no XIIGen extension/runtime trace from Claude UI proven |
| 14.03.Cursor | Проверяет Cursor host/runtime surface | DONE_REAL_APP_SCREENSHOT_ONLY_EXTENSION_BLOCKED for 14.13; older Cursor extension evidence remains DONE | Real Cursor window screenshot with marker/output plus same-run trace when available | p14-fix-13 real Cursor app marker screenshot; older p14-fix-09 Cursor visible marker evidence remains recorded | No remaining 14.13 screenshot work; no XIIGen extension/runtime invocation from Cursor UI proven in p14-fix-13 |
| 14.04.ScopedUI | Хранит старое принятое scoped evidence без overclaim | DONE_WITH_SCOPE_TEST_ONLY_VISIBLE_HOOK | Marker/result screenshot plus same-run trace, explicitly scoped as fallback | Claude/Codex test-only hook screenshots plus traces; Cursor real screenshot | Не использовать этот row как full desktop readiness |
| 14.13.RealAppUI | Закрывает просьбу Лубы: видно ли реальное приложение на компьютере | DONE_WITH_SCOPE_REAL_APP_SCREENSHOTS_COLLECTED | Real foreground app screenshots for Claude, Cursor and Codex, each with fresh marker/output or honest scoped limitation, screenshot path, observation, same-run trace when available | p14-fix-13 evidence folder: `artifacts/extension-runtime-token-quality-learnability/phase-14-fixes/real-program-ui-proof/p14-fix-13-real-desktop-screen-control-20260624T041937Z/`; commit/review-safe Cursor screenshot `review-safe/cursor-window-real-marker-review-safe.png` (raw `raw/cursor/window-cursor-real-marker.png` remains local/raw evidence); commit/review-safe Claude screenshot `review-safe/claude-window-real-marker-review-safe.png` (raw `raw/claude/window-claude-real-marker-direct-keys.png` remains local/raw evidence); p14-fix-14 Codex evidence folder: `artifacts/extension-runtime-token-quality-learnability/phase-14-fixes/real-program-ui-proof/p14-fix-14-codex-visible-marker-20260624T044217Z/`; commit/review-safe Codex screenshot `review-safe/codex-command-palette-visible-marker-review-safe.png` shows marker `P14-REAL-CODEX-20260624T044217Z` (raw `raw/codex/codex-command-palette-visible-marker-lowlevel-paste.png` remains local/raw evidence); Codex trace `traces/codex/trace.json` uses the same marker and says `host_runtime_claimed=false` | 0 remaining inside 14.13; limitations remain non-claims: Claude/Cursor screenshots only, Codex not full chat UI readiness, no product readiness |

Evidence index для p14-fix-09:

- Report: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-09-real-program-visible-result-20260624-060843\phase14-real-program-visible-result-proof.md`.
- Machine report: `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\real-program-ui-proof\p14-fix-09-real-program-visible-result-20260624-060843\phase14-real-program-visible-result-proof.json`.
- Cursor accepted screenshot: `raw/cursor/window-cursor-visible-marker.png`.
- Cursor desktop screenshot: `raw/cursor/desktop-after-cursor-foreground.png`.
- Claude no-marker screenshots: `raw/claude/private-window-claude-no-marker-attempt.png`,
  `raw/claude/review-safe-window-claude-redacted.png`.
- Codex no-marker screenshots: `raw/codex/private-window-codex-visible-marker-attempt.png`,
  `raw/codex/private-window-codex-after-ctrl-end.png`,
  `raw/codex/private-window-codex-after-click-wheel.png`,
  `raw/codex/review-safe-window-codex-redacted.png`,
  `raw/codex/review-safe-window-codex-after-ctrl-end-redacted.png`,
  `raw/codex/review-safe-window-codex-after-click-wheel-redacted.png`.

Статус после final boundary:

- 14.12: отдельный final review/status boundary packet создан и подтверждает,
  что 14.04 принят only with scope и все final rows 14.01-14.12 закрыты.
- После 14.12 все еще нельзя заявлять packaged Claude Desktop/chat-tab readiness,
  full Codex chat UI readiness, token savings, semantic quality improvement,
  learnability/training success, checkpoint promotion или locator update сверх
  уже записанного evidence.

Что не заявляем после Phase 14 review: это не финальная готовность продукта, не
packaged Claude extension host/chat-tab readiness, не полная Codex chat/UI
readiness, не полная semantic quality, не полный learnability/training success,
не checkpoint promotion и не locator update. P14-FIX-03 доказывает staged
full-machine lifecycle для PromptToTask/SkillSelection и закрывает 14.08/14.09;
Claude UI proof закрывает только visible Claude Code UI -> same-run core
correlation boundary; эти proof создают только scoped final boundary, а не
absolute product-readiness claim.

Никакое исправление tail, formatting, DCM, governance wording или state wording
не создает новую фазу. Оно является частью подшага `03.13` внутри фазы 03.

## 4. Главный числовой список фаз

Все числа ниже должны вычисляться из таблиц подшагов в разделе 5. Phase-level
counts не являются самостоятельным источником правды.

| Фаза | Человеческое имя | Статус фазы | Подшагов всего | DONE | Не закрыто |
|---|---|---:|---:|---:|---:|
| 01 | Контрольная доска и запрет прыжков | DONE | 3 | 3 | 0 |
| 02 | Честная фиксация текущего evidence | DONE | 4 | 4 | 0 |
| 03 | OrchestrationReasoning generalization repair | DONE | 15 | 15 | 0 |
| 04 | PromptToTask universal task contract | DONE | 8 | 8 | 0 |
| 05 | OrchestrationReasoning full trained planner | DONE | 9 | 9 | 0 |
| 06 | SkillSelection full trained selector | DONE | 8 | 8 | 0 |
| 07 | ResultGap, missing-pattern and correction DPO | DONE | 7 | 7 | 0 |
| 08 | Common/user two-leg learning lifecycle | DONE | 9 | 9 | 0 |
| 09 | Claude full extension live proof | DONE | 7 | 7 | 0 |
| 10 | Cursor full extension live proof | DONE | 7 | 7 | 0 |
| 11 | Codex full extension live proof | DONE | 7 | 7 | 0 |
| 12 | Cross-host token and quality comparison | DONE | 10 | 10 | 0 |
| 13 | QA automation, docs and no-overclaim | DONE | 8 | 8 | 0 |
| 14 | Десять финальных review и real desktop UI proof | DONE_WITH_SCOPE_REAL_APP_SCREENSHOTS_COLLECTED | 13 | 13 | 0 |

Итоговый счетчик сейчас:

- всего фаз: 14;
- DONE-with-scope: 14;
- REMAINING: 0;
- BLOCKED_BY_REVIEW: 0;
- NOT_STARTED: 0;
- IN_PROGRESS: 0;
- всего подшагов: 115;
- подшаги DONE-with-scope: 115;
- подшаги BLOCKED_BY_REVIEW: 0;
- подшаги IN_PROGRESS: 0;
- подшаги REMAINING: 0;
- всего не закрыто: 0;
- текущая позиция: фаза 14/14
  DONE_WITH_SCOPE_REAL_APP_SCREENSHOTS_COLLECTED.

## 5. Детали фаз и подшаги

Правило статуса подшага: каждый подшаг имеет только один из четырех статусов:
`DONE`, `IN_PROGRESS`, `REMAINING`, `BLOCKED_BY_REVIEW`.

### Фаза 01 - Контрольная доска и запрет прыжков

- Что решает: делает единый человеческий порядок выполнения, чтобы phase names
  не прыгали между старыми label-системами.
- Статус фазы: DONE.
- Подшагов всего: 3.
- Подшагов DONE: 3.
- Подшагов не закрыто: 0.
- Sub-agent role required: plan-control reviewer после создания документа.
- Review count required before commit: 1 review, если этот файл потом будет
  коммититься.
- Commit/push boundary: parent-only, только после принятия Лубой и review.

| Подшаг | Статус | Человеческое действие | Что доказывает/решает | Evidence для DONE |
|---|---|---|---|---|
| 01.01 | DONE | Создать единый control-plan artifact | Есть один читаемый файл, а не разбросанные label-фазы | Этот файл существует в `docs/plans/` |
| 01.02 | DONE | Зафиксировать master list 01-14 | Есть один порядок фаз от старта до финала | Раздел 4 содержит 14 фаз |
| 01.03 | DONE | Ввести no-jump/status rules | Future updates обязаны показывать фазу, подшаг и статус | Разделы 6-7 содержат no-jump rule и шаблон |

### Фаза 02 - Честная фиксация текущего evidence

- Что решает: не дает старым state/report словам `READY`, `PASS`,
  `COMMIT_READY` или `FIXED_COMMIT_READY` стать проектной готовностью.
- Статус фазы: DONE.
- Подшагов всего: 4.
- Подшагов DONE: 4.
- Подшагов не закрыто: 0.
- Sub-agent role required: evidence-reader или state-review sub-agent для
  будущей проверки, без write/git authority.
- Review count required before commit: 1 review.
- Commit/push boundary: parent-only, если меняется только plan/state wording.

| Подшаг | Статус | Человеческое действие | Что доказывает/решает | Evidence для DONE |
|---|---|---|---|---|
| 02.01 | DONE | Записать последний known evidence leaf | Видно, откуда взята current-position evidence | Раздел 2 содержит путь к evidence leaf |
| 02.02 | DONE | Записать честное состояние проекта | Не возникает ложного readiness claim | Раздел 2 говорит текущий scoped status и явные non-claims |
| 02.03 | DONE | Записать, что phase 03 не committed | `FIXED_COMMIT_READY` не равен DONE | Раздел 2 отделяет sub-agent slice от boundary |
| 02.04 | DONE | Пометить dirty/DCM tail как evidence, не completion | Tail не становится новой фазой и не закрывает фазу | Разделы 2-3 называют dirty/DCM current evidence |

### Фаза 03 - OrchestrationReasoning generalization repair

- Что решает: закрывает текущую незакоммиченную repair-фазу по
  `OrchestrationReasoning` generalization, не позволяя перепрыгнуть в фазу 04.
- Статус фазы: DONE.
- Подшагов всего: 15.
- Подшагов DONE: 15.
- Подшагов BLOCKED_BY_REVIEW: 0.
- Подшагов REMAINING: 0.
- Подшагов не закрыто: 0.
- Sub-agent role required: review sub-agent для проверки packet/evidence и,
  если нужно, repair sub-agent только для хвоста внутри фазы 03.
- Review count required before commit: 2 reviews минимум: bounded review
  sub-agent + parent acceptance review.
- Commit/push boundary: только parent после review. Sub-agent git authority:
  forbidden.

| Подшаг | Статус | Человеческое действие | Что доказывает/решает | Evidence для DONE |
|---|---|---|---|---|
| 03.01 | DONE | Зафиксировать work-order boundary repair 03 | Repair относится к фазе 03, не к фазе 04 | `state-entry.json` в phase 03 evidence leaf |
| 03.02 | DONE | Создать manifest phase 03 evidence | Есть список artifact files repair slice | `manifest.json` |
| 03.03 | DONE | Записать expanded-v1 generalization run | Generalization repair slice дал runtime/eval output | `expanded-v1-generalization.stdout.log` |
| 03.04 | DONE | Записать eval summary/results | Результаты repair slice можно проверить | `expanded-v1-generalization-eval-summary.json` и `.jsonl` |
| 03.05 | DONE | Записать leakage/ablation evidence | Repair не закрывает качество без ablation evidence | `leakage-ablation-report.json` |
| 03.06 | DONE | Записать model lifecycle evidence | Lifecycle evidence существует, но не равен финальной readiness | `model-lifecycle-evidence.json` |
| 03.07 | DONE | Записать dashboard QA addition evidence | Dashboard QA не подменяет host readiness | `dashboard-qa-addition-result.json` |
| 03.08 | DONE | Записать test command summary | Видно, какие проверки были выполнены в slice | `test-command-summary.md/json` |
| 03.09 | DONE | Записать code review notes | Есть review notes для slice evidence | `code-review-notes.md/json` |
| 03.10 | DONE | Записать completion report | Slice завершен только до commit-ready boundary | `completion-report.json` и `completion-report-ru.md` |
| 03.11 | DONE | Записать leaf-local state entry | Central state не менялся и git не использовался | `state-entry.json`, `git_used=false` |
| 03.12 | DONE | Записать selected next work order | Следующий шаг - parent review, не phase 04 | `selected-next-work-order.md/json`, `phase_04_started=false` |
| 03.13 | DONE | Parent review dirty files, DCM tail, reports and evidence | Решает, принят ли phase 03 slice и нет ли незакрытого хвоста | Integration review accepted dirty set and commit order |
| 03.14 | DONE | Parent-only phase 03 commit/push boundary | Коммитит только принятый phase 03 scope; sub-agent git forbidden | Parent commit/push boundary completed; pushed through commit `54d87202e`; branch synced |
| 03.15 | DONE | Human status board update after boundary | Фаза 03 становится DONE only after accepted boundary | This status board update records phase 03 as DONE |

### Фаза 04 - PromptToTask universal task contract

- Что решает: пользовательский prompt переводится в универсальную
  структурированную задачу, а не в ad hoc текст или привязку к конкретному
  старому плану.
- Статус фазы: DONE.
- Подшагов всего: 8.
- Подшагов DONE: 8.
- Подшагов IN_PROGRESS: 0.
- Подшагов REMAINING: 0.
- Подшагов не закрыто: 0.
- Sub-agent role required: implementation sub-agent, test sub-agent, review
  sub-agent.
- Review count required before commit: 2 reviews минимум.
- Commit/push boundary: parent-only после PASS тестов и review; completed in
  commit `0f1279047`, branch synced.

| Подшаг | Статус | Человеческое действие | Что доказывает/решает | Evidence для DONE |
|---|---|---|---|---|
| 04.01 | DONE | Выдать bounded work order фазы 04 | Есть точный scope без старых transcript-authority; implementation not started | Work order with phase `04` and no git authority |
| 04.02 | DONE | Собрать prompt/held-out scenario matrix | PromptToTask проверяется на source-disjoint inputs | Prompt matrix and held-out split artifact |
| 04.03 | DONE | Реализовать или repair task contract path | Prompt превращается в structured task | Code/evidence packet for PromptToTask contract |
| 04.04 | DONE | Проверить negative/authority cases | Старые планы/attachments не становятся task authority | Negative test report |
| 04.05 | DONE | Запустить focused runner/tests | Поведение воспроизводимо локально | Test summary with zero phase-induced failures |
| 04.06 | DONE | Провести source-disjoint evaluation | Нет leakage по prompt/task data | Eval report |
| 04.07 | DONE | Провести bounded review | Review принимает PromptToTask behavior and evidence | Review packet PASS |
| 04.08 | DONE | Parent commit/status boundary | Фаза 04 закрыта только после accepted commit/status | Parent commit/push evidence completed in commit `0f1279047`, branch synced |

### Фаза 05 - OrchestrationReasoning full trained planner

- Что решает: task -> orchestration plan должен идти через обучаемое
  reasoning-состояние, а не через filename/BM25/static lookup.
- Статус фазы: DONE; phase 05 review и parent commit/push boundary закрыты в
  commit `e768fc254`, branch synced.
- Подшагов всего: 9.
- Подшагов DONE: 9.
- Подшагов REMAINING: 0.
- Подшагов не закрыто: 0.
- Sub-agent role required: model/data implementation sub-agent, eval sub-agent,
  no-hidden-determinism review sub-agent.
- Review count required before commit: 3 reviews минимум.
- Commit/push boundary: parent-only после model/eval/review acceptance;
  completed in commit `e768fc254`, branch synced.

| Подшаг | Статус | Человеческое действие | Что доказывает/решает | Evidence для DONE |
|---|---|---|---|---|
| 05.01 | DONE | Выдать bounded work order фазы 05 | Planner work не стартует из phase jump | Work order with phase `05` |
| 05.02 | DONE | Подготовить guide-state data/provenance | Training material имеет source/provenance boundary | Dataset/provenance artifact |
| 05.03 | DONE | Реализовать trained planner path | Task -> plan идет через trained state | Implementation/evidence packet |
| 05.04 | DONE | Создать checkpoint/manifest | Planner state сохраняется как asset | Checkpoint and manifest |
| 05.05 | DONE | Проверить fresh-load | State загружается без retrain | Fresh-load report |
| 05.06 | DONE | Проверить continue-training | Continuation идет от existing checkpoint | Continue-training report |
| 05.07 | DONE | Провести held-out scenario eval | Planner обобщает за пределы train data | Held-out eval report |
| 05.08 | DONE | Провести ablation/no-hidden-determinism review | Lookup/BM25/static routing не подменяет reasoning | Review packet PASS |
| 05.09 | DONE | Parent commit/status boundary | Фаза 05 закрыта после accepted review | Parent commit/push completed in commit `e768fc254`, branch synced |

### Фаза 06 - SkillSelection full trained selector

- Что решает: task + orchestration plan -> skills должен выбирать нужные
  skills из trained state, а не из позиции кандидата, ключевого слова или
  статической таблицы.
- Статус фазы: DONE.
- Подшагов всего: 8.
- Подшагов DONE: 8.
- Подшагов REMAINING: 0.
- Подшагов не закрыто: 0.
- Sub-agent role required: skill-selection implementation sub-agent, eval
  sub-agent, architecture review sub-agent.
- Review count required before commit: 3 reviews минимум.
- Commit/push boundary: parent-only после accepted review packet.

| Подшаг | Статус | Человеческое действие | Что доказывает/решает | Evidence для DONE |
|---|---|---|---|---|
| 06.01 | DONE | Выдать bounded work order фазы 06 | SkillSelection стартует только после предыдущих DONE | Accepted phase 06 evidence in commit `5e9dbaaa9` |
| 06.02 | DONE | Зафиксировать semantic unit coverage | Все units известны до implementation | Accepted phase 06 evidence in commit `5e9dbaaa9` |
| 06.03 | DONE | Реализовать trained selector path | Skills выбираются обучаемым selector | Accepted phase 06 evidence in commit `5e9dbaaa9` |
| 06.04 | DONE | Создать checkpoint/manifest | Selector state сохраняется как asset | Accepted phase 06 evidence in commit `5e9dbaaa9` |
| 06.05 | DONE | Провести held-out skill-selection eval | Selector обобщает на новые tasks | Accepted phase 06 evidence in commit `5e9dbaaa9` |
| 06.06 | DONE | Проверить import/export/fresh-load | Selector asset переносим и загружаем | Accepted phase 06 evidence in commit `5e9dbaaa9` |
| 06.07 | DONE | Провести architecture/no-hidden review | Нет hidden deterministic final authority | Accepted phase 06 evidence in commit `5e9dbaaa9` |
| 06.08 | DONE | Parent commit/status boundary | Фаза 06 закрыта после accepted review | Parent commit/push completed in commit `5e9dbaaa9`, branch synced; status board records phase 06 DONE |

### Фаза 07 - ResultGap, missing-pattern and correction DPO

- Что решает: если нужный pattern/skill отсутствует или Луба исправляет
  результат, система создает безопасный DPO/local adaptive signal, а не просто
  хранит chat text.
- Статус фазы: DONE.
- Подшагов всего: 7.
- Подшагов DONE: 7.
- Подшагов REMAINING: 0.
- Подшагов не закрыто: 0.
- Evidence for DONE: commit `5adc48c50`, review PASS, writer-level tests PASS.
- Что закрыто: result gaps и user corrections теперь создают safe/quarantined
  DPO candidates; generic bad outputs quarantined и не считаются train-ready.
- Sub-agent role required: DPO authoring sub-agent, privacy/source review
  sub-agent, parent review.
- Review count required before commit: 2 reviews минимум.
- Commit/push boundary: parent-only; external AI output нельзя использовать
  как training row без отдельного разрешения.

| Подшаг | Статус | Человеческое действие | Что доказывает/решает | Evidence для DONE |
|---|---|---|---|---|
| 07.01 | DONE | Выдать bounded work order фазы 07 | DPO work имеет exact allowed scope | Accepted phase 07 evidence in commit `5adc48c50`; review PASS; writer-level tests PASS |
| 07.02 | DONE | Реализовать missing-pattern DPO writer | Missing pattern создает trainable candidate, not chat-only | Accepted phase 07 evidence in commit `5adc48c50`; review PASS; writer-level tests PASS |
| 07.03 | DONE | Реализовать user-correction DPO writer | Correction становится structured adaptive signal | Accepted phase 07 evidence in commit `5adc48c50`; review PASS; writer-level tests PASS |
| 07.04 | DONE | Проверить provenance/privacy/source route | Raw private/vendor output не становится target | Accepted phase 07 evidence in commit `5adc48c50`; review PASS; writer-level tests PASS |
| 07.05 | DONE | Проверить near-miss taxonomy | Rejected rows учат plausible failure, не generic bad output | Accepted phase 07 evidence in commit `5adc48c50`; review PASS; writer-level tests PASS |
| 07.06 | DONE | Провести DPO review | DPO rows accepted or quarantined | Accepted phase 07 evidence in commit `5adc48c50`; review PASS; writer-level tests PASS |
| 07.07 | DONE | Parent commit/status boundary | Фаза 07 закрыта после accepted review | Parent commit/push completed in commit `5adc48c50`, branch synced; status board records phase 07 DONE |

### Фаза 08 - Common/user two-leg learning lifecycle

- Что решает: у каждой обучаемой сферы есть common/global leg в
  `llm_mvp_core` и personal/adaptive leg у пользователя; user leg можно
  backup/export/import/restore/share и continue-train без silent common
  mutation.
- Статус фазы: DONE.
- Подшагов всего: 9.
- Подшагов DONE: 9.
- Подшагов REMAINING: 0.
- Подшагов не закрыто: 0.
- Evidence for DONE: commit `56146a268`, 3 reviews PASS, Phase08 tests PASS.
- Non-claim: executable lifecycle proof является SkillSelection-only;
  PromptToTask, OrchestrationReasoning и ResultGap/correction остаются required,
  но не были executed в Phase 08.
- Sub-agent role required: lifecycle implementation sub-agent, portability eval
  sub-agent, privacy review sub-agent.
- Review count required before commit: 3 reviews минимум.
- Commit/push boundary: parent-only после accepted lifecycle evidence.

| Подшаг | Статус | Человеческое действие | Что доказывает/решает | Evidence для DONE |
|---|---|---|---|---|
| 08.01 | DONE | Выдать bounded work order фазы 08 | Lifecycle work starts with exact scope | Accepted phase 08 evidence in commit `56146a268`; 3 reviews PASS; Phase08 tests PASS |
| 08.02 | DONE | Составить common/user two-leg matrix | Каждая обучаемая сфера имеет две ноги | Accepted phase 08 evidence in commit `56146a268`; SkillSelection representative proof only |
| 08.03 | DONE | Зафиксировать user-local store manifest | Personal leg хранится у пользователя | Accepted phase 08 evidence in commit `56146a268`; no absolute-path leakage |
| 08.04 | DONE | Проверить backup/export package | User model можно backup/export | Accepted phase 08 evidence in commit `56146a268`; Phase08 tests PASS |
| 08.05 | DONE | Проверить restore/import fresh profile | User model можно восстановить без retrain | Accepted phase 08 evidence in commit `56146a268`; Phase08 tests PASS |
| 08.06 | DONE | Проверить approved share/copy | Share safe, anonymized, not silent sync | Accepted phase 08 evidence in commit `56146a268`; share requires approval |
| 08.07 | DONE | Проверить continue-training from checkpoint | Training continues from existing/imported state | Accepted phase 08 evidence in commit `56146a268`; Phase08 tests PASS |
| 08.08 | DONE | Проверить rollback/hash isolation | Common upgrade не перетирает personal leg | Accepted phase 08 evidence in commit `56146a268`; rollback proof without absolute-path leakage |
| 08.09 | DONE | Parent commit/status boundary | Фаза 08 закрыта после accepted lifecycle packet | Parent commit/push completed in commit `56146a268`; 3 reviews PASS; status board records phase 08 DONE |

### Фаза 09 - Claude full extension live proof

- Что решает: Claude extension реально работает как thin host adapter через
  live host path и core boundary: package/entrypoint wiring, handler-level
  request path до core, visible success/error, debug off/on и same-run
  correlation id.
- Статус фазы: DONE.
- Подшагов всего: 7.
- Подшагов DONE: 7.
- Подшагов REMAINING: 0.
- Подшагов не закрыто: 0.
- Sub-agent role required: Claude host implementation/probe sub-agent, runtime
  evidence review sub-agent.
- Review count required before commit: 2 reviews минимум; review 1/2 after fix
  PASS, review 2/2 PASS, targeted format review PASS.
- Commit/push boundary: parent-only in host repo/core repo as applicable;
  Claude commits `cc953ef`, `e4eaac7`, core evidence commit `97bf9348e`.
- Non-claim: handler-level Claude proof only, not full Claude Desktop UI and not
  Cursor/Codex/all-host readiness.

| Подшаг | Статус | Человеческое действие | Что доказывает/решает | Evidence для DONE |
|---|---|---|---|---|
| 09.01 | DONE | Выдать bounded work order фазы 09 | Claude work does not borrow Cursor/Codex evidence | Accepted Claude-scoped phase 09 work; Claude commits `cc953ef`, `e4eaac7`; core evidence commit `97bf9348e` |
| 09.02 | DONE | Проверить package/entrypoint wiring | Adapter thin and callable | Package/entrypoint wiring accepted in Claude commits `cc953ef`, `e4eaac7`; source/evidence review PASS |
| 09.03 | DONE | Провести live host request path | Claude request reaches core boundary | Handler-level request path to core accepted; core evidence commit `97bf9348e` |
| 09.04 | DONE | Проверить visible answer/error path | User sees result and error behavior | Visible success/error evidence accepted in phase 09 proof packet |
| 09.05 | DONE | Проверить debug off/on traces | Debug has exact `xiigen <method>` input/output shape | Debug off/on evidence accepted in phase 09 proof packet |
| 09.06 | DONE | Review core boundary/correlation | Fresh correlation proves same-run core handling | Same-run correlation id accepted; review 1/2 after fix PASS, review 2/2 PASS |
| 09.07 | DONE | Parent commit/status boundary | Фаза 09 закрыта после accepted Claude proof | Claude commits `cc953ef`, `e4eaac7`; core commit `97bf9348e`; targeted format review PASS; status board records Phase 09 DONE; non-claim: not full Claude Desktop UI |

### Фаза 10 - Cursor full extension live proof

- Что решает: Cursor extension реально работает как thin host adapter через
  Extension Host/UI/command path и core boundary.
- Статус фазы: DONE.
- Подшагов всего: 7.
- Подшагов DONE: 7.
- Подшагов REMAINING: 0.
- Подшагов не закрыто: 0.
- Sub-agent role required: Cursor host implementation/probe sub-agent, runtime
  evidence review sub-agent.
- Review count required before commit: 2 reviews минимум; review 1/2 PASS,
  review 2/2 PASS, targeted format review PASS.
- Commit/push boundary: parent-only in host repo/core repo as applicable; Cursor
  commit `c92cf73`, core evidence commit `5de1f3e69`.
- Non-claim: bounded Cursor Extension Host command proof only, not full manual
  Cursor UI screenshot and not all-host readiness.

| Подшаг | Статус | Человеческое действие | Что доказывает/решает | Evidence для DONE |
|---|---|---|---|---|
| 10.01 | DONE | Выдать bounded work order фазы 10 | Cursor work does not borrow Claude/Codex evidence | Accepted Cursor-scoped phase 10 work; Cursor commit `c92cf73`; core evidence commit `5de1f3e69` |
| 10.02 | DONE | Проверить package/entrypoint command wiring | Adapter thin and callable | Cursor package/entrypoint command wiring accepted in commit `c92cf73`; source/evidence review PASS |
| 10.03 | DONE | Провести bounded Extension Host command path | Cursor command reaches core boundary | Bounded Extension Host command path accepted; core evidence commit `5de1f3e69` |
| 10.04 | DONE | Проверить visible answer/error path | User sees result and error behavior | Visible success/error evidence accepted in phase 10 proof packet |
| 10.05 | DONE | Проверить debug off/on traces | Debug has exact `xiigen <method>` input/output shape | Debug off/on evidence accepted in phase 10 proof packet |
| 10.06 | DONE | Review core boundary/correlation | Fresh correlation proves same-run core handling | Same-run correlation id accepted; review 1/2 PASS, review 2/2 PASS |
| 10.07 | DONE | Parent commit/status boundary | Фаза 10 закрыта после accepted Cursor proof | Cursor commit `c92cf73`; core commit `5de1f3e69`; targeted format review PASS; status board records Phase 10 DONE; non-claim: not full manual Cursor UI screenshot or all-host readiness |

### Фаза 11 - Codex full extension live proof

- Что решает: Codex extension/plugin/chat/app path реально работает как thin
  adapter через core boundary, а не только как package/static/component proof.
- Статус фазы: DONE.
- Подшагов всего: 7.
- Подшагов DONE: 7.
- Подшагов REMAINING: 0.
- Подшагов не закрыто: 0.
- Sub-agent role required: Codex host implementation/probe sub-agent, runtime
  evidence review sub-agent.
- Review count required before commit: 2 reviews минимум; review 1/2 PASS,
  review 2/2 PASS.
- Commit/push boundary: parent-only in host repo/core repo as applicable;
  evidence commit `87e125e1d`, fail-fast governance repair commit `2d631cb5b`.
- Non-claim: installed Codex plugin skill surface/node entrypoint proof only,
  not token savings, quality improvement, learnability, training, checkpoint
  mutation, full Codex chat UI, or all-host readiness.

| Подшаг | Статус | Человеческое действие | Что доказывает/решает | Evidence для DONE |
|---|---|---|---|---|
| 11.01 | DONE | Выдать bounded work order фазы 11 | Codex work does not borrow Claude/Cursor evidence | Accepted Codex-scoped phase 11 work; evidence commit `87e125e1d`; fail-fast governance repair commit `2d631cb5b` |
| 11.02 | DONE | Проверить package/entrypoint wiring | Adapter thin and callable | Installed Codex plugin skill surface loaded; source/evidence review PASS |
| 11.03 | DONE | Провести live Codex/plugin/chat path | Codex request reaches core boundary | `src/codex-entrypoint.js` ran through node and reached `llm_mvp_core` |
| 11.04 | DONE | Проверить visible answer/error path | User sees result and error behavior | Controlled `invalid_request` error path visible |
| 11.05 | DONE | Проверить debug off/on traces | Debug has exact `xiigen <method>` input/output shape | Debug off/on reached `llm_mvp_core` |
| 11.06 | DONE | Review core boundary/correlation | Fresh correlation proves same-run core handling | Same-run correlation id accepted; review 1/2 PASS, review 2/2 PASS |
| 11.07 | DONE | Parent commit/status boundary | Фаза 11 закрыта после accepted Codex proof | Evidence commit `87e125e1d phase 11: add codex plugin proof evidence`; governance commit `2d631cb5b governance: require fail-fast commit prechecks`; status board records Phase 11 DONE |

### Фаза 12 - Cross-host token and quality comparison

- Что решает: сравнить before/after tokens and quality on same scenarios after
  host proofs, без подмены quality простым token delta.
- Статус фазы: DONE.
- Подшагов всего: 10.
- Подшагов DONE: 10.
- Подшагов REMAINING: 0.
- Подшагов не закрыто: 0.
- Sub-agent role required: token eval sub-agent, quality eval sub-agent,
  independent review sub-agent.
- Review count required before commit: accepted Phase 12 evidence after
  ORCH-12 boundary.
- Commit/push boundary: closed by evidence commits `f1697513b` and
  `1b9eee8a7`.
- Что доказано: 9/9 adaptive responses have
  `orchestration_reasoning_status=orchestration_plan_produced`; 9/9 internal
  learned status; 0 fallback matches; 9/9 structured plan evidence; 9/9
  adaptive rubric pass; 9/9 token reduction accepted; targeted tests PASS,
  35/35 for `OrchestrationReasoningStateLoadingTests` and
  `XiigenExtensionApiContractsTests`.
- Non-claim: Phase 12 не доказывает final product readiness, full host UI
  readiness, full semantic quality, full learnability/training success,
  checkpoint promotion или locator update.

| Подшаг | Статус | Человеческое действие | Что доказывает/решает | Evidence для DONE |
|---|---|---|---|---|
| 12.01 | DONE | Выдать bounded work order фазы 12 | Token/quality work starts only after host/eval prerequisites | Phase 12 evidence accepted |
| 12.02 | DONE | Собрать cross-host scenario pair matrix | Baseline/adaptive сравниваются на same pairs | `f1697513b`; `1b9eee8a7` |
| 12.03 | DONE | Собрать baseline token traces | Есть до-состояние | Phase 12 token quality evidence recorded |
| 12.04 | DONE | Собрать adaptive token traces | Есть после-состояние | 9/9 adaptive responses have orchestration plan produced |
| 12.05 | DONE | Проверить accounting/variance | Token delta математически сопоставим | 9/9 token reduction accepted |
| 12.06 | DONE | Зафиксировать quality rubric | Quality не подменяется token delta | 9/9 adaptive rubric pass |
| 12.07 | DONE | Scoring baseline/adaptive quality | Есть before/after quality comparison | 9/9 structured plan evidence; 9/9 internal learned status |
| 12.08 | DONE | Held-out/ablation/no-leakage checks | Улучшение не является memorization/leakage | 0 fallback matches |
| 12.09 | DONE | Independent token+quality review | Token success blocked if quality regresses | Targeted tests PASS, 35/35 |
| 12.10 | DONE | Parent commit/status boundary | Фаза 12 закрыта после accepted comparison | Evidence commits `f1697513b phase 12: record blocked token quality evidence`; `1b9eee8a7 phase 12: produce universal orchestration evidence`; status `Фаза 12 DONE` |

### Фаза 13 - QA automation, docs and no-overclaim

- Что решает: автоматизирует проверки и документацию, но не дает dashboard/API
  или smoke evidence притвориться full extension readiness.
- Статус фазы: DONE.
- Подшагов всего: 8.
- Подшагов DONE: 8.
- Подшагов IN_PROGRESS: 0.
- Подшагов REMAINING: 0.
- Подшагов не закрыто: 0.
- Sub-agent role required: QA automation sub-agent, documentation sub-agent,
  no-overclaim review sub-agent.
- Review count required before commit: 3 reviews минимум.
- Commit/push boundary: parent-only после docs/QA acceptance.

| Подшаг | Статус | Человеческое действие | Что доказывает/решает | Evidence для DONE |
|---|---|---|---|---|
| 13.01 | DONE | Выдать bounded work order фазы 13 | QA/docs work has exact scope | Текущий bounded work order от Лубы: `Continue please`; этот work order является evidence для 13.01 |
| 13.02 | DONE | Добавить host scenario QA automation | Host paths can be rechecked | `tools/phase13-qa-docs-no-overclaim.ps1`; `phase13-qa-docs-no-overclaim-report.*` |
| 13.03 | DONE | Scope dashboard/API evidence | Dashboard/API не подменяет live host readiness | Phase 13 checks в script/report/docs разделяют dashboard/API evidence и live host readiness |
| 13.04 | DONE | Написать extension/API/dashboard/debug docs | Human docs explain usage and verification | `docs/projects/43-smart-skills-domain-dashboard.md`; `docs/API-DASHBOARD-QA-HOWTO-ru.md`; `docs/XIIGEN-DEBUG-TRACE-HOWTO-ru.md` |
| 13.05 | DONE | Написать local lifecycle docs | Backup/import/share/continue-training понятны человеку | `docs/EVIDENCE-SCOPES-HOWTO-ru.md` объясняет lifecycle scope entries и Phase 13 no-overclaim gate |
| 13.06 | DONE | Заполнить no-overclaim matrix | Reports say what they do and do not prove | `phase13-qa-docs-no-overclaim-report.*` содержит no-overclaim scan |
| 13.07 | DONE | Провести docs/QA review | Docs and QA accepted without readiness overclaim | Phase 13 self-review checks записаны в `phase13-qa-docs-no-overclaim-report.*` |
| 13.08 | DONE | Parent commit/status boundary | Фаза 13 закрыта после accepted docs/QA | Parent commit/push boundary closed in `31471d277 phase 13: add qa docs no-overclaim evidence`; parent verification PASS: QA/docs script PASS, API/dashboard tests PASS 83/83, WPF tests PASS 9/9, no-overclaim self-check PASS, generated artifacts UTF-8 no BOM/LF-only/no trailing whitespace |

### Фаза 14 - Десять финальных review и real desktop UI proof

- Что решает: финально проверяет, можно ли закрыть execution plan по записанному
  evidence scope, и отдельно требует доказать, что Claude, Cursor и Codex
  реально запускались/активировались как видимые desktop-приложения на этом
  компьютере.
- Статус фазы: DONE_WITH_SCOPE_REAL_APP_SCREENSHOTS_COLLECTED
  (14.01-14.12 закрыты only-with-scope; 14.13 closed with scoped real app
  screenshot evidence; full product/extension/chat UI readiness не заявляется).
- Подшагов всего: 13.
- Подшагов DONE-with-scope: 13.
- Подшагов BLOCKED_BY_REVIEW / NEEDS_FIX: 0.
- Подшагов IN_PROGRESS: 0.
- Подшагов REMAINING: 0.
- Подшагов не закрыто: 0.
- Host status после p14-fix-14: Claude
  DONE_REAL_APP_SCREENSHOT_ONLY_EXTENSION_BLOCKED; Cursor
  DONE_REAL_APP_SCREENSHOT_ONLY_EXTENSION_BLOCKED; Codex
  DONE_REAL_APP_SCREENSHOT_AND_EXTENSION_TRACE.
- Remaining inside 14.13: none. Limitations remain as non-claims/future product
  work: Claude/Cursor screenshots do not prove XIIGen extension/runtime traces
  from those UIs; Codex trace is installed-plugin/core only with
  `host_runtime_claimed=false`; no full product readiness.
- p14-fix-09 evidence index: `artifacts/extension-runtime-token-quality-learnability/phase-14-fixes/real-program-ui-proof/p14-fix-09-real-program-visible-result-20260624-060843/phase14-real-program-visible-result-proof.md`;
  key screenshot names: `raw/cursor/window-cursor-visible-marker.png`,
  `raw/claude/review-safe-window-claude-redacted.png`,
  `raw/codex/review-safe-window-codex-redacted.png`,
  `raw/codex/review-safe-window-codex-after-ctrl-end-redacted.png`,
  `raw/codex/review-safe-window-codex-after-click-wheel-redacted.png`.
- Требование к review-исполнителям: 10 финальных review sub-agents или 10
  изолированных финальных review passes, плюс финальное принятие parent.
- Требование к числу review перед commit: 10 финальных review.
- Граница commit/push: вне текущего 14.12 boundary. В этом шаге git
  stage/commit/push не разрешены.

| Подшаг | Статус | Человеческое действие | Что доказывает/решает | Доказательство для DONE |
|---|---|---|---|---|
| 14.01 | DONE | Собрать финальный readiness-пакет | Финальные review смотрят один полный пакет | `artifacts/extension-runtime-token-quality-learnability/phase-14-final-reviews/final-readiness-packet.json`; `phase14-final-review-report.json` |
| 14.02 | DONE | Финальный review 1: Claude | Claude readiness действительно закрыта в принятом scope | DONE_WITH_SCOPE after UI-evidence-02: `phase14-claude-ui-live-proof.json` доказывает visible Claude Code UI prompt submission -> same-run `llm_mvp_core` request/response/trace correlation; packaged Claude extension host/chat-tab readiness не доказана и не заявлена |
| 14.03 | DONE | Финальный review 2: Cursor | Cursor readiness действительно закрыта | PASS: FR-02 Cursor Extension Host плюс panel/runtime proof прошли без fallback |
| 14.04 | DONE | Финальный review 3: real-program visible UI proof | Проверяет старое Claude/Cursor/Codex visible host/runtime proof без overclaim | DONE_WITH_SCOPE_TEST_ONLY_VISIBLE_HOOK after p14-fix-11 re-review of p14-fix-10: Claude screenshot `raw/claude/review-safe-window-claude-test-only-visible-marker-hook.png` shows `P14-FIX-10-CLAUDE-20260624T033241Z` plus visible result marker and same-run `raw/claude/trace.json`; Cursor remains DONE_WITH_SCOPE_FROM_P14_FIX_09 from `raw/cursor/window-cursor-visible-marker.png` plus same-run trace; Codex screenshot `raw/codex/review-safe-window-codex-test-only-visible-marker-hook.png` shows `P14-FIX-10-CODEX-20260624T033241Z` plus visible result marker and same-run `raw/codex/trace.json`. Historical p14-fix-10 Codex marker gap is superseded for 14.13 by p14-fix-14; full Codex chat UI readiness and product readiness are not claimed. |
| 14.05 | DONE | Финальный review 4: cross-host behavior | Prompt -> task -> plan -> skills работает across hosts | PASS: FR-04, FR-05, FR-06 |
| 14.06 | DONE | Финальный review 5: token | Token before/after доказан честно | PASS: FR-05 Phase 12 token evidence |
| 14.07 | DONE | Финальный review 6: quality | Quality before/after не регрессирует | PASS: FR-05 Phase 12 quality no-regression evidence |
| 14.08 | DONE | Финальный review 7: learnability/DPO | DPO/local learning реально доказаны | PASS after P14-FIX-03: `phase14-lifecycle-proof.json` доказывает staged full-machine lifecycle для PromptToTask и SkillSelection; FR-08 DPO construction остается PASS |
| 14.09 | DONE | Финальный review 8: lifecycle/rollback | Backup/export/import/share/continue/rollback закрыты | PASS after P14-FIX-03: PromptToTask и SkillSelection прошли checkpoint+manifest load, backup/export/import/share, continue-training, fresh-load, held-out/ablation/no-leakage, rollback и no common locator mutation |
| 14.10 | DONE | Финальный review 9: core boundary/no-hidden | Logic в core, adapters thin, no hidden deterministic authority | PASS: FR-04, FR-05, FR-06, FR-11 |
| 14.11 | DONE | Финальный review 10: docs/QA/no-overclaim | Documentation и claims честные | PASS: FR-09, FR-10, FR-12 |
| 14.12 | DONE | Parent final report/status boundary | Закрывает старый boundary только scoped to recorded evidence and limitations | `artifacts/extension-runtime-token-quality-learnability/phase-14-final-boundary/p14-final-boundary-20260624-070127/phase14-final-boundary-report.md`; JSON sibling; final verdict `DONE_WITH_SCOPE / FINAL_BOUNDARY_COMPLETE` only for recorded scoped evidence; не доказывает packaged Claude Desktop/chat-tab readiness или full Codex chat UI readiness |
| 14.13 | DONE_WITH_SCOPE_REAL_APP_SCREENSHOTS_COLLECTED | Real desktop screen-control proof for Claude, Cursor and Codex | Проверяет, что человек может увидеть настоящие приложения на текущем компьютере | p14-fix-13 collected Cursor real app marker screenshot and Claude real app marker screenshot; preferred commit/review-safe copies are `review-safe/cursor-window-real-marker-review-safe.png` and `review-safe/claude-window-real-marker-review-safe.png`, while raw screenshots remain local/raw evidence; both hosts remain `DONE_REAL_APP_SCREENSHOT_ONLY_EXTENSION_BLOCKED`; p14-fix-14 collected accepted Codex Desktop command/search palette screenshot, with preferred commit/review-safe copy `review-safe/codex-command-palette-visible-marker-review-safe.png` and raw `raw/codex/codex-command-palette-visible-marker-lowlevel-paste.png` retained as local/raw evidence; marker `P14-REAL-CODEX-20260624T044217Z` plus same-run installed-plugin/core trace `traces/codex/trace.json`; Codex is `DONE_REAL_APP_SCREENSHOT_AND_EXTENSION_TRACE`; this does not claim full Codex chat UI readiness or product readiness |

## 6. No-jump rule

1. В каждый момент есть ровно одна активная фаза.
2. Следующая фаза не начинается, пока текущая не имеет status DONE.
3. `COMMIT_READY`, `FIXED_COMMIT_READY`, passed test, smoke, JSON report или
   sub-agent packet не равны DONE, если review/commit boundary еще открыт.
4. Фаза не может называться DONE, если хотя бы один ее подшаг не DONE.
5. Фаза не может называться DONE, если required commit/push boundary есть, но
   commit/push evidence еще не существует.
6. Tail, DCM tail, formatting, state wording, governance wording, report
   wording и cleanup не становятся новой фазой. Они входят в текущую числовую
   фазу как numbered substeps или repair substeps.
7. Старые label-системы QSG/DCM/CL/CU/CX/REVIEW можно писать только в скобках
   после номера фазы: например, `Фаза 03, substep DCM tail`.
8. Parent не делает content/code/governance implementation напрямую, если
   выполнение требует sub-agents. Parent orchestrates, reviews and commits
   accepted changes.
9. Любой future status update обязан начинаться с человеческого счетчика:
   `Фаза N/14, шаг NN.MM/NN.Total, статус ...`.
   Строка `Фаза N/14, шаг NN.MM/NN.Total, статус X` без человеческого
   объяснения фазы, шага, находки, исправления и остатка запрещена для
   сообщений Лубе.

## 7. Шаблон человеческого статуса для будущих сообщений

Этот шаблон обязателен для каждого progress update, status update,
review/status packet, near-final и final сообщения Лубе. Машинный указатель
`phase.step.status` сохраняется для no-jump контроля, но он не заменяет
человеческое объяснение.

```text
Фаза N/14: <человеческое имя фазы>
Что решает фаза: <одно простое русское предложение>
Шаг NN.MM/NN.Total: <человеческое имя шага>
Что решает шаг: <одно простое русское предложение>
Сейчас: <человеческий перевод статуса; технический статус можно дать в скобках>
Что найдено: <что проверка увидела, или "пока не проверялось">
Что исправлено: <что изменилось, или "пока ничего не менялось">
Что осталось: <оставшаяся работа и boundary простыми словами>
Счетчики фазы: сделано <X>/<Y>, осталось <Y-X>/<Y>
Счетчики всего плана: сделано <A>/<B>, осталось <B-A>/<B>
```

Обязательный перевод статусов:

```text
DONE / done = готово: результат принят и boundary закрыт.
IN_PROGRESS / in-progress = в работе: сейчас выполняется этот шаг.
NEEDS_FIX / review-block = нужна правка: проверка нашла конкретный разрыв.
REMAINING / remaining = еще не начато: шаг стоит в очереди.
BLOCKED / blocked = заблокировано: нужен внешний ответ или недоступный ресурс.
```

Технические labels вроде `generic_bad_output`, `writer-level path`, `QSG-28`,
`STATE goal_reached=true`, `PASS`, `DONE` или `IN_PROGRESS_NEXT_STEP_KNOWN`
можно показывать только вместе с переводом в той же строке или соседнем
предложении: что это значит по-человечески, что review нашел, что fix сделал и
что осталось. Сообщение, которое говорит только `Фаза N/14, шаг NN.MM/NN.Total,
статус X`, не проходит human-readable gate.

Пример для текущей позиции после p14-fix-14:

```text
Фаза 13/14: DONE / готово. QA automation, documentation, and no-overclaim evidence.
Что решила фаза 13: сделала dashboard/API/smoke evidence автоматизированным, человеко-читаемым и безопасно ограниченным.
Шаги 13.01-13.08: DONE / готово.
Сейчас: Phase 14 final-review gate выполнил 12 изолированных review/status rows; P14-FIX-03 закрыл lifecycle rows 14.08/14.09, UI-evidence-02 закрыл 14.02 Claude with scope, p14-fix-09 закрыл Cursor visible marker with scope, p14-fix-11 принял p14-fix-10 как DONE_WITH_SCOPE_TEST_ONLY_VISIBLE_HOOK для 14.04, 14.12 final boundary записал старую scoped matrix, p14-fix-13 собрал реальные Cursor/Claude app screenshots, а p14-fix-14 собрал Codex app/plugin screenshot plus installed-plugin/core trace.
Фаза 14/14: DONE_WITH_SCOPE_REAL_APP_SCREENSHOTS_COLLECTED.
Шаг 14.13/14.13: DONE_WITH_SCOPE_REAL_APP_SCREENSHOTS_COLLECTED / real desktop screen-control proof collected with host limitations.
Что решила фаза 14: старый readiness-пакет собран в scoped evidence; всего 115 подшагов, 115/115 DONE-with-scope, 0/115 in progress, 0/115 NEEDS_FIX, 0/115 REMAINING.
Что закрывает 14.13: Claude real app marker screenshot only, Cursor real app marker screenshot only, Codex real app/plugin marker screenshot plus same-marker installed-plugin/core trace with `host_runtime_claimed=false`.
Что НЕ доказано Phase 14: product readiness, full extension readiness, full Codex chat/UI readiness, Claude/Cursor XIIGen runtime trace from those UIs, semantic quality improvement, token savings, learnability/training, checkpoint promotion и locator update.
Счётчики всего плана: готово 115/115 DONE-with-scope, blocked by review / needs fix 0/115, осталось 0/115.
```

## 8. Карта 16 требований Лубы к фазам

| Требование | Человеческая формулировка | Какие фазы закрывают |
|---|---|---|
| R01 | Claude full extension работает в реальном host/runtime path | 09, 13, 14 |
| R02 | Cursor full extension работает в реальном host/runtime path | 10, 13, 14 |
| R03 | Codex full extension работает в реальном host/runtime path | 11, 13, 14 |
| R04 | Universal prompt -> task работает как обучаемый task contract | 04, 12, 14 |
| R05 | Task -> orchestration plan работает через OrchestrationReasoning | 03, 05, 12, 14 |
| R06 | Task -> skills работает через SkillSelection | 06, 12, 14 |
| R07 | Missing pattern/result gap создает DPO/adaptive signal | 07, 08, 14 |
| R08 | User correction создает безопасный DPO/adaptive signal | 07, 08, 14 |
| R09 | Common/global leg живет в `llm_mvp_core` | 08, 12, 14 |
| R10 | User/personal adaptive leg живет у пользователя и не перетирается common update | 08, 14 |
| R11 | Backup/export/import/restore/share user model доказаны | 08, 13, 14 |
| R12 | Continue-training from existing/imported checkpoint доказан | 08, 12, 14 |
| R13 | Token before/after сравнение доказано и связано с quality gate | 12, 14 |
| R14 | Quality before/after доказано на rubric, held-out, ablation/no-leakage | 12, 14 |
| R15 | QA automation, docs, no-overclaim и no-hidden-determinism закрыты | 05, 06, 12, 13, 14 |
| R16 | Финал подтвержден 10 review passes и human-readable final readiness matrix | 14 |

## 9. Определение финального DONE_WITH_SCOPE проекта

Execution plan считается `DONE_WITH_SCOPE / FINAL_BOUNDARY_COMPLETE` для
recorded scoped evidence только когда все ниже доказано packet-backed evidence,
10 финальных review passes дают PASS, а final boundary явно оставляет scope
limitations видимыми. После p14-fix-14 дополнительный 14.13 screen-control
target считается закрытым как
`DONE_WITH_SCOPE_REAL_APP_SCREENSHOTS_COLLECTED`, потому что по Claude, Cursor и
Codex есть real app screenshot evidence, а limitations записаны явно:

1. Есть три extension/host slices: Cursor, Claude, Codex, причем Claude/Codex
   visible proof может быть принят только в явно записанном scope.
2. Каждая extension является thin adapter, а логика остается в `llm_mvp_core`.
3. Universal prompt -> task работает.
4. Task -> orchestration plan работает.
5. Task -> skills работает.
6. Common/global leg существует для обучаемых сфер.
7. User/personal adaptive leg существует и изолирован.
8. DPO/local learning работает для missing patterns и user corrections.
9. Backup/export/import/restore/share доказаны.
10. Continue-training from existing/imported checkpoint доказан.
11. Token before/after comparison доказан.
12. Quality before/after comparison доказан без regression.
13. QA automation покрывает реальные host/scenario paths.
14. No-hidden-determinism и no-overclaim review закрыты.
15. Final readiness matrix явно показывает PASS или accepted scoped PASS по
    каждой строке.
16. Выполнены 10 final reviews, и ни один review не оставил FAIL или
    IN_PROGRESS row.
17. Для 14.13 real-app screenshot collection есть реальные foreground
    screenshots Claude Desktop/app, Cursor и Codex desktop/app/plugin surface на
    текущем компьютере. Claude и Cursor закрыты только как screenshots without
    XIIGen runtime traces from those UIs. Codex закрыт как screenshot plus
    same-marker installed-plugin/core trace with `host_runtime_claimed=false`.

Запрещенные абсолютные product-readiness слова даже после scoped closure:

- READY;
- project ready;
- full extension readiness achieved.

Разрешенная финальная формулировка после 14.12 boundary и p14-fix-14:

`DONE_WITH_SCOPE / FINAL_BOUNDARY_COMPLETE + DONE_WITH_SCOPE_REAL_APP_SCREENSHOTS_COLLECTED`

Она означает только: execution plan закрыт по recorded evidence and limitations.
Она не означает packaged Claude Desktop/chat-tab readiness, Cursor UI runtime
invocation, full Codex chat UI readiness, full extension readiness, full semantic
quality, token-savings marketing, training success, checkpoint promotion или
locator update.

Текущая формулировка статуса для 14.13:

`DONE_WITH_SCOPE_REAL_APP_SCREENSHOTS_COLLECTED`

## 10. Правило sub-agent execution

Каждая implementation/review фаза после этого плана требует bounded sub-agent
work order. В work order обязательно должны быть:

- номер фазы;
- номер подшага в формате `NN.MM`;
- allowed write scope;
- artifact type;
- exact Luba authority quote;
- allowed verification;
- forbidden broad verification;
- no git authority for artifact/code/content sub-agent;
- `parent_will_perform_commit_push_after_review=true`;
- max active time;
- direct Luba questions answered first;
- old transcripts/state/packets are evidence only.

Для будущего real-app/UI work order, если Луба отдельно попросит расширять
доказательства за пределы закрытого 14.13 status scope, work order обязан
содержать Phase 14 screen-control runbook из этого файла: какие реальные
приложения запускать или активировать, как вывести Claude Desktop/app, Cursor и
Codex desktop/app/plugin surface на передний план, какой fresh marker показать,
каким способом сделать screenshot actual application window или whole desktop,
куда сохранить screenshot path, какие input/output/tokens/quality traces
сохранить when available, и как записать exact blocker. Real UI test agent must
be given desktop/screen-control steps and must not silently replace them with
test-only hooks, JSON-only evidence, adapter/core smoke, process/window-list или
unrelated helper window screenshots.

Sub-agent не stage, не commit, не push. Commit/push делает parent только после
accepted review boundary.

## 11. Правило внешних AI и инструментов

Внешний AI, внешние модели, external host calls, browser automation, paid API,
image generation, web search, unrelated tools и любые tool families outside the
approved work order запрещены без явного current-turn разрешения Лубы.

Локальное чтение repo files для plan evidence не является разрешением на
runtime/test/training/git/tool expansion.

## 12. Как этот план предотвращает прыжки

Прыжок считается ошибкой, если агент:

- говорит о фазе 04, пока фаза 03 не DONE;
- выносит tail/DCM/formatting в отдельную top-level phase;
- называет `FIXED_COMMIT_READY` завершением фазы;
- пропускает substep counter;
- пропускает substep id `NN.MM`;
- скрывает review/commit boundary;
- говорит `готово`, когда остались exact remaining substeps;
- называет фазу DONE, когда не все ее подшаги DONE;
- называет фазу DONE, когда commit/push boundary required, но evidence нет;
- использует старую state file строку как текущую команду Лубы.

Антидот: в каждом статусе агент обязан назвать:

1. `Фаза N/14`;
2. `шаг NN.MM/NN.Total`;
3. `что решает эта фаза`;
4. `сколько подшагов осталось в этой фазе и их статусы`;
5. `какие фазы еще NOT_STARTED`;
6. `какой boundary закрывает DONE`.

## 13. Статус после p14-fix-14

После 14.12 final boundary и p14-fix-14 status-pointer не является full product
readiness. Подшаги 14.01-14.12 остаются DONE only-with-scope, включая 14.04
`DONE_WITH_SCOPE_TEST_ONLY_VISIBLE_HOOK` и 14.12 `FINAL_BOUNDARY_COMPLETE`.
Подшаг 14.13 закрыт как
`DONE_WITH_SCOPE_REAL_APP_SCREENSHOTS_COLLECTED`.

Текущий concrete status path:

1. Внутри этого плана осталось 0 execution substeps. Общий счетчик: 115/115
   DONE-with-scope, 0 NEEDS_FIX, 0 REMAINING.
2. 14.13 закрыт только в evidence scope: Claude/Cursor screenshots only,
   Codex screenshot plus installed-plugin/core trace. Это не full product,
   extension или chat UI readiness.
3. Этот plan repair сам по себе не запускает UI tests, code edits, training,
   checkpoint promotion или locator update.
4. Git stage/commit/push остаются вне текущего разрешенного шага, пока Луба не
   даст отдельное явное разрешение.
