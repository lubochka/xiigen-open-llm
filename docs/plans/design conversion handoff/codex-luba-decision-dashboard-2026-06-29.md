# LUBA_DECISION_DASHBOARD - 2026-06-29

## LUBA_DECISIONS_2026_06_29

Простыми словами для Luba: эти решения уже приняты 2026-06-29 и заменяют старую формулировку, где эти пункты выглядели как нерешенные блокеры. Это не product completion и не доказательство runtime/browser/merge/visual readiness. Это только запись решений, которые разблокируют следующие bounded execution tasks.

1. PR не нужен. `PR not needed; verify mergeability only`. Новые PR не создавать. PR #6 и PR #7 не трогать. Нужно только проверить, что `DesignToCode` может влиться в самую свежую read-only source branch без конфликтов. `Work happens on DesignToCode in a separate folder or worktree`. `Only check mergeability into the latest updated source branch; Luba performs the merge`.
2. Разрешения на runtime/browser проверки даны. `all runtime/browser permissions granted`. Это теперь execution task, а не permission blocker. Следующий worker должен выполнять только утвержденный runtime/browser сценарий и записать evidence; это не разрешение на PR/git/cleanup/external AI.
3. Визуальный judge для этого plan path - сам Codex agent. `Codex agent is the visual judge`. Не требовать Claude/OpenAI/Gemini provider, secret, account, external endpoint или external AI call, если Luba позже отдельно не изменит это решение.
4. Старую/original папку не трогать. `do not touch old/original folder`. Никакого cleanup/revert/stash/restore/delete/move/archive там в текущем продолжении.
5. Человеческая формулировка для package/trace/corpora/fixture: `packages/traces/corpora/fixtures stay untouched unless needed for a concrete proof task`. Сейчас не трогать packages, trace files, training corpora/DPO data или test fixtures, если конкретная runtime/browser/mergeability proof task не требует их явно.

Boundaries:

- No PR mutation.
- No cleanup or old-folder mutation.
- No external AI provider/secret requirement for this plan path.
- No package/trace/corpora/fixture mutation unless a concrete proof task later needs it and names the exact scope.

## Коротко для Luba

Сейчас это не product completion, не PR readiness, не merge readiness, не runtime proof и не visual fidelity proof. Это один короткий dashboard решений: что осталось выбрать, какой безопасный default, что будет после "да", что останется после "нет/пока нет", и какого bounded worker запускать дальше.

Главное правило: `Proof readiness items are tasks, not stop blockers`. То есть эти пункты не надо называть финальными блокерами, если по ним можно выдать следующий ограниченный worker. Но без решения Luba нельзя запускать runtime/browser, внешний visual AI, cleanup, PR mutation, package/trace/corpora/fixture mutation или future push/PR/mergeability proof.

Общие границы:

- `Human-language blocker reports before machine labels`
- `Work happens on DesignToCode in a separate folder or worktree`
- `Luba performs the merge`
- `no external AI call`
- `no secrets captured`

## 1. PR: PR #6, ошибочный draft PR #7, и будущий PR из DesignToCode

Что это значит по-русски:
PR #6 уже открыт из старой ветки `DesignToCode-clean`. Draft PR #7 открыт из `codex/chat-gpt-5.5`, но эта ветка должна быть source authority/read-only, а не рабочей PR-веткой. Для текущего chain правильная рабочая ветка - `DesignToCode`. По прочитанному decision-prep, отдельный PR с head `DesignToCode` на момент той проверки не был найден.

Рекомендуемый safe default:
Пока ничего не менять в GitHub: оставить PR #6 как старый draft, оставить PR #7 как исторический ошибочный draft record, не создавать новый PR до явного approval Luba.

Если Luba говорит "да":
Сначала запускается read-only recheck PR/branch metadata. Если Luba выбирает новый PR, worker создает draft PR from `DesignToCode` to `main` с заранее утвержденным title/body. Если Luba выбирает action по PR #7, worker меняет только PR #7 и только выбранным способом: close, retitle или body note. Merge не выполняется.

Если Luba говорит "нет/пока нет":
PR #6 остается старым draft из `DesignToCode-clean`; PR #7 остается draft из source branch; PR из `DesignToCode` пока не появляется. Это сохраняет путаницу в GitHub, но не мутирует состояние.

Exact bounded worker после решения:
`bounded-pr-designtocode-action-after-luba-decision`

## 2. Runtime/browser execution permission

Что это значит по-русски:
Документы нашли только candidate commands. Ничего не запускалось. Для DesignConversion наиболее конкретный browser path выглядит как .NET Chromium adapter, который генерирует временный `file:///.../index.html` и screenshot, а не как hosted localhost app. Также есть runtime-flow tests, WPF dashboard app/tests/script, и ModelServer candidate, но без найденного URL/port/health endpoint.

Рекомендуемый safe default:
Не запускать runtime/browser/tests/builds/server. Сначала Luba выбирает один lane и разрешает exact command, evidence path и screenshot policy.

Если Luba говорит "да":
Worker запускает только выбранный lane. Практичный первый lane: DesignConversion real screenshot test, если Luba разрешает `dotnet test` и browser screenshot capture. Если browser executable не найден, worker пишет environment blocker вроде `BROWSER_RUNTIME_UNAVAILABLE`, не claims browser proof.

Если Luba говорит "нет/пока нет":
Остаются только candidate commands. Нет runtime proof, browser proof, WPF proof, hosted URL proof, product readiness или visual fidelity proof.

Exact bounded worker после решения:
`bounded-runtime-browser-execution-worker`

## 3. External visual AI judge

Что это значит по-русски:
Внешний visual judge может сравнивать approved screenshots/rendered artifacts/design references по rubric, но сейчас нет выбранного provider/account/model/endpoint/secret env var name, data-sharing approval, artifact set, rubric, thresholds, cost/retry/retention policy. Поэтому safe state: `no external AI call`.

Рекомендуемый safe default:
Не отправлять ничего внешнему провайдеру. Не читать и не печатать secret values. Собирать только owner decisions.

Если Luba говорит "да":
Сначала нужен preflight worker: он проверит, что все решения заполнены, что artifact paths exact, что data-sharing разрешен, и если Luba отдельно разрешила, проверит только наличие named env var без печати значения. Только после preflight PASS и отдельного execution approval можно вызывать provider.

Если Luba говорит "нет/пока нет":
External visual judge остается не настроен. Можно выбрать waiver или local/offline downgraded path, но это не external visual judge proof и не visual fidelity proof.

Exact bounded worker после решения:
`bounded-external-visual-judge-preflight-worker`

После preflight PASS и отдельного approval:
`bounded-external-visual-judge-execution-worker`

## 4. Cleanup/residue: original folder vs clean DesignToCode worktree

Что это значит по-русски:
Правильная рабочая папка для текущего workflow - `C:\Projects\llm_mvp_core_DesignToCode_worktree`; по прочитанному classifier она была clean by read-only status. Оригинальная папка `C:\Projects\llm_mvp_core` содержит большой local residue/evidence хвост и не должна использоваться как доказательство, что `DesignToCode` worktree грязный.

Рекомендуемый safe default:
Ничего не чистить, не restore, не stash, не delete. Считать original folder отдельным evidence/residue вопросом и сначала сделать selector.

Если Luba говорит "да":
Worker классифицирует original-folder residue по exact categories: keep evidence, archive outside repo, commit candidate, quarantine, ignore for current workflow, cleanup candidate. Actual cleanup запускается только отдельным later worker после exact paths, backup/rollback policy и verification commands.

Если Luba говорит "нет/пока нет":
Clean claim остается narrow only for `DesignToCode worktree`; original folder residue остается как есть. Нельзя claim commit readiness, cleanup readiness или trace sensitivity safety.

Exact bounded worker после решения:
`bounded-original-folder-residue-selector-worker`

Для будущей cleanup execution только после selector:
`bounded-cleanup-execution-worker-after-selector`

## 5. Package / trace / corpora / fixture selectors

Что это значит по-русски:
Остались четыре artifact lanes, где нельзя автоматически build/replay/import/export/cleanup. Нужно выбрать exact scope и действие.

Рекомендуемый safe default:
Defer and preserve. Не build packages, не replay trace, не import/export corpora, не cleanup fixtures, не inspect sensitive contents без policy.

Если Luba говорит "да":
Luba выбирает конкретный lane, exact paths, действие и policy. Worker делает только выбранный doc/preflight/provenance/sensitivity step. Mutation/build/replay/training/cleanup/publish требуют отдельного явного разрешения.

Если Luba говорит "нет/пока нет":
Artifacts остаются unresolved owner decisions. Нет package provenance proof, trace safety proof, trace replay readiness, corpora disposition, training/DPO completion, fixture correctness или cleanup readiness.

Exact bounded workers после решения:

- Packages: `bounded-package-artifact-policy-selector-worker`
- Trace JSON: `bounded-trace-json-sensitivity-worker`
- Wiki/corpora/DPO: `bounded-wiki-domain-corpora-decision-packet-worker`
- Figma nested worktree/fixtures: `bounded-figma-fixture-owner-decision-prep-worker` or `bounded-figma-nested-worktree-owner-intent-decision-prep-worker`

## 6. Source branch policy recheck before future push/PR/mergeability proof

Что это значит по-русски:
Source branch is read-only authority. Ее выбирают как latest updated remote branch ahead of main на момент проверки, но это не рабочая ветка Codex. Рабочая ветка остается `DesignToCode`. Перед любым future push/PR/mergeability proof нужно заново проверить, не изменилась ли latest updated source branch.

Рекомендуемый safe default:
Перед будущим push/PR/mergeability proof всегда запускать read-only source recheck. Не мутировать source branch. Не считать старый dry check финальным merge readiness.

Если Luba говорит "да":
Worker делает read-only source selection, исключает `DesignToCode` как source authority, записывает source branch name/SHA, и если source changed, запускает только dry mergeability check against `DesignToCode`. Никакого merge.

Если Luba говорит "нет/пока нет":
Future push/PR/mergeability proof не должен продолжаться с stale source authority. Остается только policy packet, не current proof.

Exact bounded worker после решения:
`bounded-source-branch-recheck-before-push-pr-mergeability-proof`

## Самый маленький набор решений, чтобы продолжить

1. PR: выбрать safe default no PR mutation, либо разрешить exact PR action для PR #7 / new draft PR from `DesignToCode`.
2. Runtime/browser: выбрать один lane и разрешить exact command, evidence path, screenshot policy.
3. External visual judge: выбрать waiver/local/offline/external provider path. Для external path заполнить provider/account/model/endpoint/secret env var name/data-sharing/rubric/cost.
4. Cleanup/residue: выбрать, делать ли original-folder residue selector сейчас; default preserve.
5. Artifacts: выбрать первый lane для selector: packages, trace JSON, corpora/DPO, or Figma fixtures.
6. Source branch: подтвердить, что перед любым future push/PR/mergeability proof нужен fresh read-only source recheck.

## Маленькое техническое приложение

Технические статусы из исходных packets, без расширения claim:

- PR packet: decision-prep only; no PR mutation.
- Runtime/browser packet: command discovery only; no runtime/browser execution.
- External judge packet: owner-decision collector only; `no external AI call`; `no secrets captured`.
- Cleanup packet: `DesignToCode worktree` clean by read-only status in source report; original folder has local residue; no cleanup/revert/stash/restore.
- Package/trace/corpora/fixture packet: selector only; no package build, no trace replay, no corpora import/export, no fixture cleanup.
- Source policy packet: policy only; no source branch mutation; `Luba performs the merge`.

Preserved boundaries:

- This dashboard is not product completion.
- This dashboard is not PR readiness.
- This dashboard is not merge readiness.
- This dashboard is not runtime proof.
- This dashboard is not visual fidelity proof.
- This dashboard is not external visual judge proof.

## Evidence ledger for this dashboard

claim_scope:
Created one consolidated Russian decision dashboard for Luba from the named decision/prep/audit reports.

evidence_scope:
Read the seven named reports in `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\` with UTF-8 readback, plus a target-path existence check before creation.

count_claimed:
1 dashboard file created; 7 named source reports read; 0 tests/builds/browser/runtime/external AI/secrets/cleanup/git/PR actions run.

independent_evidence_count:
8 evidence records: seven source report reads plus one target-path existence check.

subagent_id_or_NONE:
delegated-bounded-sub-agent-executor-current-turn; no nested agent used.

downgrade_needed:
Yes for any claim beyond document consolidation. The evidence supports only this dashboard and the narrow facts quoted from the source reports.

files_changed:
`C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-luba-decision-dashboard-2026-06-29.md`

verification:
Post-write verification should be literal/readback only. No product, PR, merge, runtime, browser, visual, external AI, cleanup, package, trace, corpora, fixture, or source-branch execution proof is implied.

next_safe_followups:
Run the smallest selected bounded worker after Luba chooses from the six decision groups above.
