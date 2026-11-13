# Phase 14.04 Claude/Codex visible-marker proof, p14-fix-10

Дата пакета: 2026-06-24T03:38:27.7291165Z.
Папка пакета: p14-fix-10-claude-codex-visible-marker-20260624-063010.

## Короткий вывод

14.04 можно считать PASS-кандидатом только в узком scope test-only visible marker hook, если Луба принимает такой hook как Tier B evidence для текущего задания. Claude и Codex оба получили свежий visible marker/result screenshot и same-run trace с тем же correlation id. Cursor остается accepted evidence из p14-fix-09.

Это не заявляет packaged Claude Desktop/chat-tab readiness, не заявляет full Codex chat UI readiness, не заявляет final readiness 14.12, token savings, semantic quality improvement, learnability, training или checkpoint mutation.

Важно про исполнение: этот пакет был произведен bounded sub-agent; nested sub-agent не использовался. Git stage/commit/push не выполнялись. Эта формулировка исправляет прежнюю отрицательную confusing wording, которая могла звучать как отрицание bounded sub-agent packet.

## Что было сделано

- Прочитан active Phase 14 plan и p14-fix-09 report.
- Выполнен Get-StartApps для Claude/Codex/Cursor/VS Code: raw/discovery/start-apps.txt.
- Выполнен Get-Process по Claude/Codex/Cursor/Code windows: raw/discovery/process-windows.txt.
- Проверены shell commands: claude не найден в PATH; codex, cursor, code найдены: raw/discovery/commands.txt.
- Найден local Claude Code executable и безопасно проверен только --version: 2.1.181 (Claude Code), без prompt/external chat: raw/discovery/claude-code-version.txt.
- Проверены entrypoints: Codex installed plugin entrypoint src/codex-entrypoint.js, core route tools/xiigen-extension-core-api.ps1, repo helpers phase14-claude-ui-proof-diagnostic.ps1 and phase14-codex-ui-proof.ps1.
- Для Codex сделана попытка real Codex app screenshot: raw/codex/window-codex-visible-marker.png и raw/codex/window-codex-after-ctrl-end.png; свежий marker там не виден, поэтому эти screenshots не закрывают Codex.
- Для Claude/Codex использован минимальный test-only visible marker hook: raw/harness/show-visible-marker-window.ps1 и contract raw/harness/visible-marker-hook-contract.json.
- Захват screenshots сделан через user32 foreground + System.Drawing.CopyFromScreen.

## Host statuses

| Host | Status | Visible screenshot | Same-run trace | Ограничение |
|---|---|---|---|---|
| Claude | DONE_WITH_SCOPE_TEST_ONLY_VISIBLE_HOOK_CANDIDATE | raw/claude/review-safe-window-claude-test-only-visible-marker-hook.png показывает CLAUDE_VISIBLE_RESULT_DONE_WITH_SCOPE_CANDIDATE и P14-FIX-10-CLAUDE-20260624T033241Z | raw/claude/trace.json имеет тот же id, response_success=true, external_ai_used=false, training_started=false | Не доказан packaged Claude Desktop/chat-tab path; exact native Claude adapter source в repo не найден |
| Codex | DONE_WITH_SCOPE_TEST_ONLY_VISIBLE_HOOK_CANDIDATE | raw/codex/review-safe-window-codex-test-only-visible-marker-hook.png показывает CODEX_VISIBLE_RESULT_DONE_WITH_SCOPE_CANDIDATE и P14-FIX-10-CODEX-20260624T033241Z | raw/codex/trace.json имеет тот же id, response_success=true, route идет через installed src/codex-entrypoint.js | Native Codex app screenshot still did not show marker; full Codex chat UI readiness не доказана |
| Cursor | DONE_WITH_SCOPE_FROM_P14_FIX_09 | p14-fix-09 raw/cursor/window-cursor-visible-marker.png | p14-fix-09 same-run trace | Не менялся в p14-fix-10 |

## Gate result

- 14.04: PASS_CANDIDATE_WITH_TEST_ONLY_VISIBLE_HOOK_SCOPE, если accepted hook scope засчитывается как Tier B для текущего задания.
- 14.12: NOT_CLOSED до отдельного accepted review/status boundary.
- final_readiness_claimed: false.

## Remaining blockers / non-claims

- Sub-agent scope: этот пакет был произведен bounded sub-agent; nested sub-agent не использовался. Git stage/commit/push не выполнялись.
- Claude native packaged UI path остается gap: нет безопасного Claude Desktop/chat-tab marker proof, и exact native Claude adapter file в repo не найден.
- Codex native app visible marker остается gap: реальное окно Codex было захвачено, но показывало старый thread участок без свежего marker; test-only hook закрыл только visible-hook scope.
- Adapter/core или hook evidence не доказывает token savings, semantic quality, learnability, DPO/training, checkpoint mutation, full product readiness.

## Evidence index

Commit-safe proof evidence:
- raw/claude/request.json
- raw/claude/response.json
- raw/claude/trace.json
- raw/claude/review-safe-window-claude-test-only-visible-marker-hook.png
- raw/codex/request.json
- raw/codex/response.json
- raw/codex/trace.json
- raw/codex/review-safe-window-codex-test-only-visible-marker-hook.png
- raw/harness/visible-marker-hook-contract.json
- raw/harness/proof-structural-validation.json

Local diagnostic artifacts, not required for committed proof:
- raw/claude/window-claude-test-only-visible-marker-hook.png
- raw/claude/screenshot-manifest.json
- raw/codex/window-codex-test-only-visible-marker-hook.png
- raw/codex/test-hook-screenshot-manifest.json
- raw/codex/window-codex-visible-marker.png
- raw/codex/window-codex-after-ctrl-end.png
- raw/codex/screenshot-manifest.json
- raw/codex/screenshot-after-ctrl-end-manifest.json
- raw/discovery/start-apps.txt
- raw/discovery/commands.txt
- raw/discovery/source-entrypoint-summary.txt
- raw/discovery/process-windows.txt
- raw/discovery/claude-code-version.txt
- raw/discovery/repo-entrypoint-candidates.txt
- raw/discovery/codex-installed-plugin-files.txt
- raw/harness/show-visible-marker-window.ps1

## Commit hygiene

- Stage the report files, JSON summaries, same-run request/response/trace files, harness contract/validation, and the two review-safe hook screenshots.
- Leave native Codex screenshots without the fresh marker local-only; they document a limitation and are not accepted proof.
- Leave raw/private screenshots local-only unless they are explicitly redacted or review-safe.
- Leave large source discovery dumps local-only; `raw/discovery/source-entrypoint-summary.txt`, `raw/discovery/commands.txt`, and `raw/discovery/start-apps.txt` are enough if discovery context is staged.

## Format gate

PASS_EXIT_0_NO_OUTPUT: git diff --check -- artifacts/extension-runtime-token-quality-learnability/phase-14-fixes/real-program-ui-proof/p14-fix-10-claude-codex-visible-marker-20260624-063010. Limitation: files are new/untracked, so Git did not inspect untracked file content without staging; no staging was performed.
