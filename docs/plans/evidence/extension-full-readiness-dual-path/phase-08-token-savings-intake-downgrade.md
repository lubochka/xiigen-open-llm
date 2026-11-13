# Phase 08.01 - Token savings before/after intake/downgrade

CURRENT TASK: Retry Phase 08.01 Token savings before/after intake/downgrade. Build the requested artifacts directly in the allowed write scope from existing evidence only.

active_task_sentence_source: Luba current unquoted instruction: "CURRENT TASK: Retry Phase 08.01 Token savings before/after intake/downgrade. You are already the spawned bounded sub-agent executor. Build the requested artifacts directly in the allowed write scope."

NOT THE TASK: do not run new probes/tests/builds/evals, do not launch GUI, do not send prompts/messages, do not mutate model/training/checkpoint/locator, do not edit code, do not claim token savings unless existing evidence proves same-scenario baseline vs adapted run, do not claim product/full extension/Codex UI/semantic quality/learnability readiness, and do not stage/commit/push/switch branches.

EVIDENCE-ONLY: active plan Phase 08, Phase 03 token trace command output and packets, Phase 05 product matrix, Phase 06 full-extension matrix, Phase 07 Codex chat UI downgrade, Phase 02 schema samples, execution state/log, and token-related evidence already present under this evidence root.

## Фаза и шаг

Фаза 9 из 17: Token savings before/after. Смысл фазы: отделить наличие token trace/accounting guard от реального доказательства экономии токенов, чтобы token savings не был заявлен без одинакового baseline/adapted сравнения, качества без регрессии и host-specific evidence.

Шаг 08.01: intake/downgrade. Смысл шага: прочитать уже существующие evidence artifacts, записать, что именно есть по token trace, чего не хватает для before/after сравнения, и оставить token savings claim закрытым.

Статус: DONE_AS_MATRIX_INTAKE_ONLY. Это intake/downgrade artifact, а не token savings proof.

## Короткий вывод

Token trace существует: да. Существующий evidence file `phase-03-command-output/03-dotnet-token-trace.txt` содержит старый command output для `ExtensionTokenTraceAccountingTests`, где 5 accounting/guard tests passed. Phase 03 packets для Claude, Cursor и Codex также ссылаются на этот artifact как accepted Automated boundary evidence.

Но существование trace не доказывает token savings. Этот trace показывает, что система умеет фиксировать/проверять accounting guard conditions и что token delta alone cannot pass quality or claim savings. В Phase 03 token payloads при этом `baseline_tokens`, `candidate_tokens` и `repeat_count` равны `null`, а `savings_calculation` записан как `NOT_CALCULATED_IN_PHASE_03; trace/accounting guard only`. Это означает: форма и guard есть, но численного same-scenario before/after результата нет.

Before/after comparison потребовал бы отдельный accepted evidence packet: фиксированный prompt/workload hash, тот же host, тот же tokenizer/version/method/debug mode, baseline run без local model/adaptation, adapted/local-model run, извлеченные token counts, repeat count, формулу delta, quality non-regression tied to the same delta, и host-specific joined evidence для Claude/Cursor/Codex там, где host behavior заявляется. Ничего из этого в существующем evidence root не найдено как completed savings proof.

## Top counts and conclusion

| Metric | Count |
|---|---:|
| total matrix rows | 9 |
| token trace files found | 1 |
| token-savings Phase 03 host rows found | 3 |
| `PROVEN_NARROW` rows | 1 |
| `SCHEMA_OR_GUARD_ONLY` rows | 2 |
| `MISSING_EVIDENCE` rows | 5 |
| `CLAIM_CLOSED` rows | 1 |
| accepted same-scenario baseline/adapted pairs | 0 |
| baseline runs with numeric token counts | 0 |
| adapted/local-model runs with numeric token counts | 0 |
| before/after delta calculations | 0 |
| quality non-regression records tied to token delta | 0 |
| host-specific Claude/Cursor/Codex token comparisons accepted | 0 |
| token savings claim opened | 0 |

Conclusion: `token_savings_claim_opened = false`. Evidence supports only narrow trace/accounting guard availability and schema requirements, not token savings.

## Evidence base

- [Phase 03 Automated boundary report](phase-03-automated-boundary-report.md): records `token_trace`, 5 guard tests passed, and "token savings remain unclaimed".
- [Phase 03 token trace command output](phase-03-command-output/03-dotnet-token-trace.txt): old command output for `ExtensionTokenTraceAccountingTests`; no new test was run in this step.
- [Phase 03 quality scoring command output](phase-03-command-output/04-dotnet-quality-scoring.txt): old guard output showing token reduction alone cannot pass quality when quality regresses or quality evidence is incomplete.
- [Phase 03 Automated boundary packets JSON](phase-03-automated-boundary-packets.json): Claude/Cursor/Codex token rows have `baseline_tokens: null`, `candidate_tokens: null`, `repeat_count: null`, and `savings_calculation: NOT_CALCULATED_IN_PHASE_03; trace/accounting guard only`.
- [Phase 02 dual-path evidence contract](phase-02-dual-path-evidence-contract.md): future `typed_payloads.token` requires workload identity, tokenizer/version, baseline tokens, candidate tokens, repeat counts, savings calculation, quality guard, evidence, remaining, and downgraded claim.
- [Phase 02 sample joined evidence](phase-02-sample-joined-evidence.json): schema sample only; `join_status: NOT_RUN`, `joined_claim_proven: false`, and token counts are null.
- [Phase 05 product readiness matrix](phase-05-product-readiness-matrix.md): token savings before/after is downgraded and missing accepted before/after savings report.
- [Phase 07 Codex chat UI downgrade](phase-07-codex-chat-ui-readiness-downgrade.md): token/quality guards are core-only and not Codex UI before/after proof.

## Matrix rows

| row label | status | evidence links | missing evidence | next bounded step | overclaim ban |
|---|---|---|---|---|---|
| Boundary token trace availability. | `PROVEN_NARROW` | [Phase 03 report](phase-03-automated-boundary-report.md); [token trace output](phase-03-command-output/03-dotnet-token-trace.txt); [Phase 03 packets](phase-03-automated-boundary-packets.json) | Numeric before/after counts, actual savings calculation, host-visible joined evidence, and quality non-regression tied to a real delta are absent. | Future bounded Phase 08 measurement packet may reuse the trace method only after it records actual baseline/adapted runs. | Нельзя заявлять token savings из факта, что trace/accounting guard существует. |
| Baseline run without local model/adaptation. | `MISSING_EVIDENCE` | [Phase 03 packets](phase-03-automated-boundary-packets.json); [Phase 02 contract](phase-02-dual-path-evidence-contract.md) | No accepted baseline run without local model/adaptation; `baseline_tokens` is null in existing token payloads. | Future bounded baseline run on fixed workload, with local model/adaptation disabled and evidence saved. | Нельзя заявлять savings без baseline count for the same scenario. |
| Adapted/local-model run. | `MISSING_EVIDENCE` | [Phase 03 packets](phase-03-automated-boundary-packets.json); [Phase 05 matrix](phase-05-product-readiness-matrix.md) | No accepted adapted/local-model run; `candidate_tokens` is null and no local/adapted behavior is joined. | Future bounded adapted/local-model run on the same workload and same host/method/tokenizer. | Нельзя заявлять savings без candidate/adapted count for the same scenario. |
| Same prompt set and same host comparability. | `MISSING_EVIDENCE` | [token trace output](phase-03-command-output/03-dotnet-token-trace.txt); [Phase 03 packets](phase-03-automated-boundary-packets.json) | Guard test names require same prompt/host/method/tokenizer/debug, but no completed same-prompt same-host evidence pair exists. | Future bounded pair packet with prompt/workload hashes, host, method, tokenizer, tokenizer version, and debug mode matched. | Нельзя сравнивать token counts across changed prompt set, host, method, tokenizer, or debug mode. |
| Token count extraction method. | `SCHEMA_OR_GUARD_ONLY` | [token trace output](phase-03-command-output/03-dotnet-token-trace.txt); [Phase 02 contract](phase-02-dual-path-evidence-contract.md) | Method/guard existence is present, but no accepted production extraction output for baseline and adapted runs exists. | Future bounded extractor output must record baseline_tokens, candidate_tokens, tokenizer/version, and repeat_count. | Нельзя превращать extractor/trace method readiness into a savings result. |
| Before/after delta calculation. | `MISSING_EVIDENCE` | [Phase 03 packets](phase-03-automated-boundary-packets.json); [Phase 02 sample joined evidence](phase-02-sample-joined-evidence.json) | `savings_calculation` is `NOT_CALCULATED_IN_PHASE_03`; sample joined evidence is `NOT_RUN`. No delta formula was applied to actual counts. | Future bounded calculation: `(baseline_tokens - candidate_tokens) / baseline_tokens`, with raw counts, repeat count, and scenario id preserved. | Нельзя заявлять percentage or absolute token savings while delta is not calculated from actual matched counts. |
| Quality non-regression tied to token delta. | `SCHEMA_OR_GUARD_ONLY` | [quality scoring output](phase-03-command-output/04-dotnet-quality-scoring.txt); [token trace output](phase-03-command-output/03-dotnet-token-trace.txt) | Quality guards exist, but no quality non-regression record is tied to an actual baseline/adapted token delta. | Future bounded Phase 08/09 packet must attach quality score/rubric/non-regression verdict to the same workload delta. | Нельзя заявлять savings when quality regresses or when quality is not tied to the same before/after pair. |
| Host-specific token comparison Claude/Cursor/Codex. | `MISSING_EVIDENCE` | [Phase 03 packets](phase-03-automated-boundary-packets.json); [Phase 06 matrix](phase-06-full-extension-readiness-matrix.md); [Phase 07 matrix](phase-07-codex-chat-ui-readiness-downgrade.md) | Claude/Cursor/Codex token rows are Automated boundary only; Local desktop is not accepted for token savings and no host-specific comparison is joined. | Future host-specific bounded evidence packet per host, only after safe host route and same-scenario pair exists. | Нельзя переносить token result from one host to Claude/Cursor/Codex readiness or host-specific savings. |
| Token savings conclusion. | `CLAIM_CLOSED` | This Phase 08.01 artifact; [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 05 matrix](phase-05-product-readiness-matrix.md); [Phase 07 matrix](phase-07-codex-chat-ui-readiness-downgrade.md) | Accepted same-scenario baseline/adapted pair, numeric counts, delta calculation, quality non-regression, and host-specific joined evidence are all missing. | Keep claim closed; next numbered plan phase after parent review is Phase 09 semantic quality improvement eval unless Luba authorizes a future bounded Phase 08 measurement run. | Нельзя заявлять token savings. The evidence supports only: token trace/accounting guard exists; savings are not proven. |

## Counts and downgrade interpretation

| Status | Rows | Meaning |
|---|---:|---|
| `PROVEN_NARROW` | 1 | Token trace/accounting guard availability exists as prior Phase 03 command evidence. |
| `SCHEMA_OR_GUARD_ONLY` | 2 | Required method/quality gates are described or tested as guards only; no actual before/after result exists. |
| `MISSING_EVIDENCE` | 5 | Required baseline/adapted/comparability/delta/host evidence is absent. |
| `CLAIM_CLOSED` | 1 | The conclusion row explicitly keeps token savings closed. |

## Non-claims

- I did not run new probes/tests/builds/evals in this step.
- I did not launch GUI or send prompts/messages.
- I did not mutate model/training/checkpoint/locator.
- I did not edit code.
- I did not stage, commit, push, switch branches, or use any git write/ref action.
- I did not prove product readiness, full extension readiness, full Codex chat UI readiness, semantic quality improvement, learnability/training, checkpoint promotion, locator update, or host runtime pass.
- I did not prove token savings. The evidence supports only: token trace/accounting guard exists; same-scenario baseline/adapted savings proof is missing.

## Verification plan for this artifact

- JSON parse `phase-08-token-savings-intake-downgrade.json`.
- Exact search Markdown/JSON for all 9 required row labels.
- Search unsupported token savings claims; only non-claim, missing-evidence, closed-claim, guard-only, or overclaim-ban contexts are allowed.
- Run `git diff --check` over changed files; if files are untracked, run no-index whitespace checks for those files.

## Changed files intended by this step

- `docs/plans/evidence/extension-full-readiness-dual-path/phase-08-token-savings-intake-downgrade.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/phase-08-token-savings-intake-downgrade.json`
- `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-STATE.md`
- `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-LOG.md`
