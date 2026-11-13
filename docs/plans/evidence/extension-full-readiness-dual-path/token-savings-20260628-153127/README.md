# Token savings before/after packet

## Что проверялось

Проверялся уже joined Claude сценарий:

```text
C:/Projects/llm_mvp_core/docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/claude-same-scenario-dual-path-join-20260628-145508/
```

Связанные папки:

```text
Path B: C:/Projects/llm_mvp_core/docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/claude-path-b-prompt-submission-20260628-142748/
Path A: C:/Projects/llm_mvp_core/docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/claude-same-scenario-path-a-20260628-144612/
Join:   C:/Projects/llm_mvp_core/docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/claude-same-scenario-dual-path-join-20260628-145508/
```

Узкая цель была только токенная: можно ли честно получить before/after token accounting из уже существующего dual-path evidence и локальных adapter/core traces.

## Результат

Статус: `BLOCKED_NO_COMPARABLE_BEFORE_AFTER_TRACE`.

Token savings не измерялись и не claimed. В Claude evidence есть same-prompt join, Path A adapter/API/core request/response/trace и Path B UI screenshot/action-log packet. Но там нет пары baseline/adaptive или before/after token traces с одинаковым сценарием и сопоставимым accounting.

## Почему нельзя честно посчитать

Path A содержит core request/response/trace для prompt-to-task, но inspected `trace.json` не содержит token-count fields: `token_count_input`, `token_count_output`, `token_count_total` или их baseline/adaptive equivalents.

Path B содержит UI screenshots and action log, not a token-accounting trace. Screenshot/action evidence cannot be used as comparable token input/output accounting.

Join packet links the same prompt and statuses only. Its own `joined_evidence_does_not_prove` already excludes token savings.

Local token/accounting code exists, including `43.SmartSkillsDomain/TokenTraceAccounting.cs`, `Tests/ExtensionTokenTraceAccountingTests.cs`, `tools/token-quality-runner.ps1`, and `tools/frdp-row12-cursor-token-trace-harness.ps1`. Those prove that an accounting shape exists, not that the Claude joined scenario already has comparable before/after counts.

## Что это не доказывает

This packet does not prove token savings, quality improvement, semantic quality, learnability, DPO/training, checkpoint mutation, checkpoint promotion, locator update, product readiness, all-host readiness, or host UI readiness.

## Следующее bounded действие

Use the existing Cursor row12 path as the next exact token-measurement route, or create the same kind of parameterized Claude no-send measurement harness:

```text
Spec:    C:/Projects/llm_mvp_core/docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-token-savings-baseline/row12-workload-spec.json
Harness: C:/Projects/llm_mvp_core/tools/frdp-row12-cursor-token-trace-harness.ps1
```

If the next worker stays on Claude, first add/produce paired baseline/adaptive traces for the exact Claude prompt with the `TokenTraceAccounting` required fields, then validate the pair before any token-savings claim.

## Verification

Verification results are recorded in `result.json`. No broad tests/builds, host UI work, training, DPO, checkpoint mutation, git stage, commit, or push were performed.
