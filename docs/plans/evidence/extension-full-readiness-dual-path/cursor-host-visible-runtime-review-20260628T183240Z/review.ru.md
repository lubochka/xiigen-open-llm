# CURSOR-HOST-VISIBLE-RUNTIME-REVIEW-08

claim_scope: Review of Cursor row12 host-visible/runtime-proof slice: four scoped Cursor files, current git status/diffs for those files, existing row12 Path A evidence packet, syntax/parser checks, and overclaim/core-boundary checks.
evidence_scope: Commands run in `C:\Projects\cursor-xiigen-smart-skills` plus existing evidence under `C:\Projects\llm_mvp_core_codex_sync_20260628\docs\plans\evidence\extension-full-readiness-dual-path\reference-preservation-row12-cursor-fix-20260628T153936Z`.
count_claimed: 1 bounded review packet, 0 source fixes, 0 Cursor reruns in this review.
independent_evidence_count: 14 command outputs inspected in this review, including status/diff, source reads/searches, evidence artifact discovery, syntax/parser checks, structured JSON validation, and diff hygiene checks.
subagent_id_or_NONE: CURSOR-HOST-VISIBLE-RUNTIME-REVIEW-08
downgrade_needed: Yes for product readiness, full Cursor extension readiness, full local desktop UI pass, token savings, semantic quality, training, checkpoint promotion, and Path B UI proof. No for the narrow Path A host command/core-boundary evidence reviewed here.

## Что проверено

Проверены ровно ожидаемые Cursor-файлы:

- `C:\Projects\cursor-xiigen-smart-skills\src\extension.ts`
- `C:\Projects\cursor-xiigen-smart-skills\dist\extension.js`
- `C:\Projects\cursor-xiigen-smart-skills\scripts\run-frdp-row12-host-visible.ps1`
- `C:\Projects\cursor-xiigen-smart-skills\test\host-qa\FRDP-row12-hostVisibleRuntimeProofQA.js`

`git status --short` в Cursor repo показал только посторонние dirty/untracked файлы: `README.md`, `docs/PROJECT-DOC.md`, `package.json`, `scripts/local-runtime-helper.js`, `docs/CURSOR-DOCS-FINAL-COVERAGE-TOKEN-QUALITY-NONHOST.md`, `reports/RUNTIME-03-cursor-live-runtime/runtime-03-20260618-012559/runtime-03-cursor-user-data/`, `scripts/verify-docs-final-coverage-nonhost.js`. Эти файлы не трогались и не staged.

`git diff -- src/extension.ts dist/extension.js scripts/run-frdp-row12-host-visible.ps1 test/host-qa/FRDP-row12-hostVisibleRuntimeProofQA.js` вернул пустой вывод. Поэтому я не могу подтвердить наличие текущих uncommitted изменений в этих четырех файлах; доказательство поддерживает только review текущего состояния этих файлов и существующего evidence packet.

## Что доказывает сценарий

Существующий latest evidence packet:

`C:\Projects\llm_mvp_core_codex_sync_20260628\docs\plans\evidence\extension-full-readiness-dual-path\reference-preservation-row12-cursor-fix-20260628T153936Z\row12-host-visible-runner-report.json`

поддерживает узкое утверждение:

Cursor Extension Host command/API Path A вызвал `xiigenSmartSkills.runtimeProof.invoke`, дошел до `llm_mvp_core` через core API, сохранил structured references (`marker`, `scenario_id`, `pair_id`), создал request/response/core-trace/host-trace/entrypoint/redacted/latest refs, и не заявил readiness/token savings/semantic quality/Path B UI proof.

Структурная проверка JSON показала:

- `report.passed=True`
- `report.path_b=False`
- `report.readiness=False`
- `report.semantic=False`
- `report.token=False`
- `host.entrypoint=True`
- `host.callsSameHandlerCoreBoundary=True`
- `host.fullUi=False`
- `host.semantic=False`
- `host.externalAi=False`
- `entry.sameRunCorrelationMatched=True`
- `entry.fullUi=False`
- `entry.semantic=False`
- `red.command=True`
- `red.tokenSavingsClaimed=False`
- `red.semanticQualityClaimed=False`
- `red.modelReadinessClaimed=False`
- `validation.promptLeakSafe=True`

## Что не доказывает

Это не доказывает full local desktop pass, product readiness, full Cursor extension readiness, token savings, semantic quality, training, checkpoint promotion, locator update, или Path B local real-app UI evidence.

Контрольные границы сохранены:

- marker-only != full local desktop pass
- boundary/API evidence != host readiness
- trace != savings
- schema != training
- checkpoint candidate != promotion

## Implementation boundary

`src/extension.ts` и `dist/extension.js` остаются thin host adapter: они читают конфиг `xiigenSmartSkills`, строят request/trace refs, вызывают `llm_mvp_core` через `powershell.exe -File <coreApiScript>`, а host trace явно пишет `fullCursorExtensionHostUiProof: false`, `semanticQualityClaimed: false`, `externalAiOrUnrelatedToolUsed: false` и `noOverclaimReview` с ограничением scope.

Core logic остается за `llm_mvp_core`; Cursor-side code не делает host-side semantic decision.

## Команды

- `git status --short`
- `git diff -- src/extension.ts dist/extension.js scripts/run-frdp-row12-host-visible.ps1 test/host-qa/FRDP-row12-hostVisibleRuntimeProofQA.js`
- `Get-ChildItem` searches for row12 report/blocker evidence in Cursor repo and core evidence root
- `Get-Content -Raw` for the row12 PowerShell launcher and host QA runner
- `rg` searches over scoped Cursor source and evidence roots
- `node --check .\dist\extension.js` — PASS, exit 0
- `node --check .\test\host-qa\FRDP-row12-hostVisibleRuntimeProofQA.js` — PASS, exit 0
- PowerShell parser for `scripts\run-frdp-row12-host-visible.ps1` — PASS
- Structured parse of `row12-host-visible-runner-report.json` plus referenced JSON files — PASS for the narrow Path A claims above
- `git diff --check -- src/extension.ts dist/extension.js scripts/run-frdp-row12-host-visible.ps1 test/host-qa/FRDP-row12-hostVisibleRuntimeProofQA.js` — PASS, no output
- `git diff --cached -- src/extension.ts dist/extension.js scripts/run-frdp-row12-host-visible.ps1 test/host-qa/FRDP-row12-hostVisibleRuntimeProofQA.js` — empty

I did not rerun Cursor itself in this review. Evidence was present and was independently parsed/reviewed. I did not use external AI/web. I did not capture secrets. I did not commit or push.

## Findings

PASS for this narrow review slice:

- The four expected Cursor paths are currently clean in git.
- The latest located row12 evidence packet reports PASS for Path A only.
- All report refs parsed as existing during this review.
- Source and evidence wording keep overclaim boundaries closed.
- JS syntax and PS1 parser checks passed.
- No small fix was needed inside the allowed four-file write scope.

Residual limitation:

- The reviewed evidence is Path A Extension Host command/API/core-boundary evidence. It is not Path B human-visible local desktop UI proof.

## PASS/NEEDS_FIX

PASS_NARROW_PATH_A_REVIEW.

No source fix was made.

## Следующее bounded action

Smallest next action, if the active readiness chain still requires dual-path proof: delegate a separate Path B local real Cursor UI scenario review/run that records exact visible app/window target, clicks/typing/expected visible state, screenshots, no secrets captured, and pairs that Path B result with the already reviewed Path A packet.
