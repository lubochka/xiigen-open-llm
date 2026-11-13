# TOKEN-03: baseline common/no-adaptive traces

Run id: token-03-20260618-040021
Папка evidence: artifacts/extension-runtime-token-quality-learnability/token-03-baseline-traces/token-03-20260618-040021

## Что сделано

TOKEN-03 собрал свежий baseline common/no-adaptive packet для всех 54 строк TOKEN-01. Каждая строка является handler-level/core-boundary evidence: прямой вызов XiigenExtensionApiService.ProcessHostExtensionRequest, учет токенов через xiigen-heuristic-word-punctuation/v1/heuristic-estimate.

Покрытие строк:
- TOKEN-01 rows represented: 54.
- Fresh TOKEN-03 baseline rows captured: 54.
- Live host baseline token rows captured in this phase: 0.
- Adaptive/user-local rows captured in this phase: 0.
- QUALITY-03 non-regression rows passed in this phase: 0.

По host:
- Claude: 18 handler-level baseline rows; live host token baseline остается deferred.
- Codex: 18 handler-level baseline rows; live host token baseline остается deferred.
- Cursor: 18 handler-level baseline rows; live host token baseline остается deferred.

По method:
- orchestration-plan: 18 rows.
- prompt-to-task: 18 rows.
- skill-selection: 18 rows.

Машиночитаемые артефакты:
- Baseline trace JSONL: artifacts/extension-runtime-token-quality-learnability/token-03-baseline-traces/token-03-20260618-040021/baseline-token-traces.jsonl
- Baseline trace JSON: artifacts/extension-runtime-token-quality-learnability/token-03-baseline-traces/token-03-20260618-040021/baseline-token-traces.json
- Accounting rows JSONL: artifacts/extension-runtime-token-quality-learnability/token-03-baseline-traces/token-03-20260618-040021/token-03-accounting-rows.jsonl
- Accounting summary JSON: artifacts/extension-runtime-token-quality-learnability/token-03-baseline-traces/token-03-20260618-040021/token-03-accounting-report.json
- Missing/deferred JSONL: artifacts/extension-runtime-token-quality-learnability/token-03-baseline-traces/token-03-20260618-040021/token-03-missing-deferred-rows.jsonl

## Текущий смысл evidence

Этот packet доказывает только handler-level common/no-adaptive baseline accounting для захваченных строк TOKEN-01. Он не доказывает live Claude/Cursor/Codex UI/chat/send-message token savings, потому что в TOKEN-03 не было live host baseline token rows.

Evidence scope по host/method:
- Claude rows captured as handler_core_boundary_baseline with HostName=Claude. Это не заменяет Claude Code/plugin/session-hook или Claude Desktop/chat UI live token evidence.
- Cursor rows captured as handler_core_boundary_baseline with HostName=Cursor. Это не заменяет Cursor Extension Host/chat participant live-pair evidence.
- Codex rows captured as handler_core_boundary_baseline with HostName=Codex. Это не заменяет Codex app/chat/send-message evidence.
- Missing live host rows записаны как explicit deferred rows и не подменяются handler-level traces.

Все captured rows сохраняют:
- run_kind=baseline.
- claim_allowed=false.
- no_external_ai_task_solving=true.
- no_training_started=true.
- no_model_mutation=true.

## Что остается открытым

TOKEN-04 остается open: нужно собрать adaptive/user-local traces для тех же pair_id values с точными user-local checkpoint или overlay refs.

QUALITY-03 остается open: нужно сравнить baseline/adaptive outputs для тех же pairs и доказать non-regression до любого claim о token reduction.

Live host savings остаются open: нужны baseline и adaptive traces из одного и того же live host scope, а не handler-only substitution.

## Claims kept closed

TOKEN-03 не заявляет token savings, live host savings, quality improvement, semantic quality, learnability, production locator/checkpoint mutation, target promotion, rollback safety, final readiness или full Claude/Cursor/Codex runtime readiness.

## Следующий independent track

После parent review acceptance и parent commit/push следующий bounded phase: TOKEN-04 adaptive/user-local trace capture для тех же TOKEN-01 pair_id values, затем QUALITY-03 same-pair non-regression до любого savings claim.
