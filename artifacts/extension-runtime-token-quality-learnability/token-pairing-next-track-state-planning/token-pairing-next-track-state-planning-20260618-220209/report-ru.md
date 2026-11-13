# TOKEN-PAIRING-NEXT-TRACK-STATE-PLANNING-01 - setup packet

Generated: 2026-06-18T22:02:09+03:00
Branch: codex/chat-gpt-5.5
Run root: artifacts/extension-runtime-token-quality-learnability/token-pairing-next-track-state-planning/token-pairing-next-track-state-planning-20260618-220209

## Доска: всего / сделано / сейчас / осталось

| Поле | Статус |
|---|---|
| Всего | Нужно доказать token before/after for Claude/Cursor/Codex только через валидные baseline/adaptive пары и quality non-regression для того же pair_id. |
| Сделано | Создан setup packet: матрица пар, список узких существующих evidence refs, условия capture и следующие bounded work orders. Runtime/eval/training не запускались. |
| Сейчас | Token claim закрыт. Следующий безопасный шаг: TOKEN-01-SCENARIO-PAIRING-PREP-01, подготовка scenario/pair ledger без runtime capture. |
| Осталось | Нужны same-host/same-prompt/same-method/same-evidence-class baseline/adaptive traces, adaptive target refs, tokenizer/accounting version, redaction, variance and quality non-regression before any token savings claim. |

## Что уже есть, но слишком узко

- Claude: есть narrow live host surface proof через direct hook -> llm_mvp_core, debug-on/off и negative missing-locator. Это не полный Claude Desktop/chat UI journey и не token pair.
- Cursor: safe-launch/defer packet фиксирует, что чистый no-backend/no-model-metadata relaunch пока не доказан; Cursor token live capture должен ждать safe-launch contract или быть явно scoped non-live.
- Codex: installed skill/plugin-cache adapter reached llm_mvp_core core boundary, но full Codex desktop chat UI/app visible conversation не доказан.
- Старые token/schema/trace/scoring artifacts полезны как context, но текущий token claim не открывают: нужны свежие same-pair refs, accounting, variance and quality non-regression.

## Какие пары нужны

| Host | Baseline common/no-adaptive | Adaptive user-local/target | Что обязательно совпадает | Почему claim закрыт сейчас |
|---|---|---|---|---|
| Claude | Same Claude host surface, same prompt/scenario/method, model_mode=common_baseline, adaptive_enabled=false, no user checkpoint. | Same Claude surface and evidence class, adaptive_enabled=true, exact user_local_checkpoint_id or target_checkpoint_id. | pair_id, prompt hash, method, debug, evidence_classification, target_locator_mode except adaptive variable, tokenizer/accounting version, redaction policy. | Нет complete same-pair accounting + adaptive ref + quality non-regression; full Claude Desktop/chat UI not proven. |
| Cursor | Same Cursor/VS Code Extension Host/UI or approved CLI path, common baseline, adaptive disabled. | Same Cursor path, adaptive enabled with exact user-local target ref. | Same host runtime/surface/evidence class, prompt, method, tokenizer/accounting, redaction, Cursor live refs for live claim. | Safe-launch/no-backend contract missing; Cursor full UI proof deferred/blocked. |
| Codex | Same Codex plugin/app/chat/app-server or explicitly scoped installed adapter surface, common baseline. | Same Codex surface, adaptive enabled with exact user-local target ref. | Same evidence class and tokenizer/accounting; Codex live refs required for live savings. | Current proof is installed skill/plugin-cache adapter only, not full Codex desktop chat UI/app. |

## Что можно готовить без runtime/eval/training

- Canonical scenario ids, prompt hashes and pair_id ledger.
- Tokenizer/accounting version and counted-span contract.
- Raw-private/redacted trace manifest templates and redaction policy.
- Per-host capture readiness checklist with Cursor/Codex/Claude deferred prerequisites.
- Bounded work orders for later capture and quality gates.

## Что должно ждать later proof

- Actual runtime capture for any host whose same-evidence-class pair is not ready.
- Cursor runtime capture until safe no-backend/no-model-metadata launch is proven or the row is explicitly scoped non-live.
- Full live Codex savings until full Codex app/chat pair exists.
- Token delta claim until same-pair quality non-regression passes for the exact pair_id.
- Any quality, semantic, learnability, target promotion, locator update, final readiness or product readiness claim.

## Следующие bounded work orders

1. TOKEN-01-SCENARIO-PAIRING-PREP-01 - prepare scenario/pair ledger only, no runtime capture.
2. TOKEN-02-TOKENIZER-ACCOUNTING-FREEZE-01 - freeze tokenizer/accounting/counting spans/redaction.
3. TOKEN-03-BASELINE-TRACE-CAPTURE-PREREQ-CHECK-01 - decide which hosts are capture-ready; defer exact blocked rows only.
4. TOKEN-04-PAIRED-BASELINE-ADAPTIVE-CAPTURE-01 - later actual capture only after separate runtime authorization and prereq PASS.
5. TOKEN-05-QUALITY-NON-REGRESSION-GATE-01 - later quality gate for same pair_id before any token claim.
6. TOKEN-06-TOKEN-DELTA-REPORT-01 - later scoped token delta report only after valid pairing and quality PASS.

## What this proves

- This proves the token-pairing setup matrix and next bounded work order queue only.

## What this does not prove

- It does not prove token savings, quality improvement, semantic quality, learnability, live host readiness, final readiness or product readiness.

## Next independent track

- TOKEN-01-SCENARIO-PAIRING-PREP-01: prepare scenario/pair ledger and capture prerequisites without runtime/eval/training.
