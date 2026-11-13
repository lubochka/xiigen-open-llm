# Row 12 workload spec: `frdp-p01-cursor-token-savings-baseline`

Status: `SPEC_READY`

CURRENT TASK: создать workload/spec packet для строки 12 `frdp-p01-cursor-token-savings-baseline`.
active_task_sentence_source: текущий bounded work order от Luba: "Create the row 12 workload spec packet for `frdp-p01-cursor-token-savings-baseline`."

NOT THE TASK: запуск token tests, build, измерение before/after, claim token savings, claim readiness, изменение scenario board, stage, commit, push.

EVIDENCE-ONLY: `product-readiness-scenario-board.md`, `product-readiness-scenario-board.json`, `product-readiness-action-plan.md`, `product-readiness-action-plan.json`, `phase-03-command-output/03-dotnet-token-trace.txt`, `Tests/ExtensionTokenTraceAccountingTests.cs`, `43.SmartSkillsDomain/TokenTraceAccounting.cs`.

## 1. Назначение строки 12

Строка 12 проверяет не сам факт "экономия токенов уже есть", а возможность честно сравнить baseline и candidate/adaptive режим Cursor на одном и том же workload. До измерения нужно зафиксировать prompt, marker, hash, tokenizer/accounting source, pairing fields и quality guard. Без этого token delta может быть случайным, несравнимым или полученным ценой потери качества.

Человеческий смысл: будущий worker должен сравнить одну и ту же no-send Cursor workload цепочку в baseline и adaptive mode, не отправляя запрос во внешний AI, и должен доказать, что ответ не стал хуже по минимальным структурным требованиям.

## 2. Что найдено в существующих evidence/tests

- Scenario board row 12 говорит: `Token savings baseline for Cursor: same workload/tokenizer before-after accounting plus host-visible candidate behavior`.
- Текущий статус board для row 12: automated/core/API evidence accepted только как guard-only; local Cursor token scenario не run; joined pass отсутствует.
- Следующее действие board для row 12: запустить fixed before-after token workload with quality guard, затем Cursor host-visible candidate behavior для того же marker.
- `03-dotnet-token-trace.txt` показывает прошлый test command `dotnet test ... ExtensionTokenTraceAccountingTests` с `Total tests: 5`, `Passed: 5`, `EXIT_CODE: 0`. Я не запускал этот test в этом work order; это только прочитанный evidence log.
- `ExtensionTokenTraceAccountingTests.cs` проверяет, что trace содержит stable fields и hashes, heuristic tokenizer fields, input/output/total token counts, redaction flags и forbidden claims.
- `TokenTraceAccounting.cs` задает schema/tokenizer constants: schema `xiigen.extension_token_trace.v1`, tokenizer name `xiigen-heuristic-word-punctuation`, tokenizer version `v1`, accounting type `heuristic`, baseline mode `baseline`, adaptive mode `adaptive`.
- Pair validation требует совпадения `scenario_id`, `pair_id`, `host_name`, `method`, `debug_enabled`, `prompt_hash`, `tokenizer_name`, `tokenizer_version`, `tokenizer_accounting_type`.
- Token delta alone не проходит quality gate: без approved quality score/regression rubric trace не может доказать качество или savings.

## 3. Canonical workload prompt

Текущие evidence/tests не содержат точного Cursor row 12 prompt. Тестовый prompt `"Turn this user prompt into a universal task."` существует только как unit-test fixture для accounting, с host `Claude`; он не является row 12 Cursor workload.

Поэтому этот spec определяет новый deterministic no-send prompt. Он разрешен для Cursor host, потому что:

- prompt синтетический, не содержит private/user data;
- prompt требует no-send local extension/runtime behavior и прямо запрещает external AI;
- prompt не мутирует модель, checkpoint, locator или training state;
- prompt заставляет ответ сохранять цепочку `prompt_to_task -> task_to_plan -> task_to_skills`, поэтому token comparison не может пройти ценой generic/empty answer.

Canonical line endings: `LF`.

Exact workload prompt:

```text
[FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1]
Host: Cursor no-send local extension runtime.
Scenario: frdp-p01-cursor-token-savings-baseline.
User request: Convert this synthetic request into a bounded Smart Skills execution plan without calling external AI.
Requirements:
1. Preserve the marker and scenario id in references.
2. Produce a prompt_to_task result with task_id, task_title, constraints, and acceptance_gates.
3. Produce a task_to_plan result with three ordered steps and one verification gate per step.
4. Produce a task_to_skills result selecting skills from this allowed set only: prompt-to-task, task-to-plan, task-to-skills, evidence-ledger.
5. State non_claims: no token savings claim, no semantic quality claim, no readiness claim, no training, no checkpoint promotion, no locator update.
6. Return deterministic structured text with sections: task, plan, skills, references, non_claims.
```

Marker: `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`

Scenario id: `frdp-p01-cursor-token-savings-baseline`

Pair id: `frdp-p01-row12-cursor-token-baseline-v1`

Primary method/core API path for token trace pairing: `prompt-to-task`

Required host path label for later worker: `Cursor no-send local extension runtime`

External AI send: forbidden.

## 4. Workload hash rule

Prompt hash rule must match `TokenTraceAccounting.HashText`:

- take the exact workload prompt text above;
- use UTF-8 bytes;
- preserve LF line endings exactly;
- do not include Markdown fences;
- compute SHA-256;
- store as lowercase hex with `sha256:` prefix.

Expected workload prompt hash:

```text
sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374
```

If a future worker gets a different hash, the run is not comparable and must stop before any before/after claim.

## 5. Tokenizer/accounting source

Known from source:

- trace schema: `xiigen.extension_token_trace.v1`
- tokenizer name: `xiigen-heuristic-word-punctuation`
- tokenizer version: `v1`
- tokenizer accounting type: `heuristic`
- tokenizer/accounting source file: `43.SmartSkillsDomain/TokenTraceAccounting.cs`
- tokenizer/accounting source file SHA-256: `d012a59912c60de27fabf822ea032937f1a03407cede1d856bda6d554f78168f`
- accounting test source file: `Tests/ExtensionTokenTraceAccountingTests.cs`
- accounting test source file SHA-256: `95b3ea183e641a639021bb0ff659ff158e6e7e6292c187e09cfe4d2aacfdebdc`

Unknown/not present in trace schema:

- compiled assembly hash;
- git commit hash of the measurement run;
- host extension bundle hash;
- Cursor extension version/build id for the local host run.

Future measurement worker must record these unknowns before making any accepted measurement packet:

- repo commit or explicit dirty-worktree status;
- compiled `43.SmartSkillsDomain` assembly path and SHA-256 if an assembly is used;
- Cursor extension build/version/hash or exact blocker if unavailable;
- `TokenTraceAccounting.cs` source hash read back at run time;
- exact schema/tokenizer constants read back from produced traces.

## 6. Before/after pairing fields

Baseline and adaptive/candidate traces are comparable only when all fields below match, except `mode` and token/answer counts:

| Field | Required value/rule |
|---|---|
| `scenario_id` | `frdp-p01-cursor-token-savings-baseline` |
| `pair_id` | `frdp-p01-row12-cursor-token-baseline-v1` |
| `host_name` | `Cursor` |
| `host_path_label` | `Cursor no-send local extension runtime` |
| `method` | `prompt-to-task` |
| `core_api_path` | same `prompt-to-task` API path for both runs; any extra task/plan/skills artifacts must be generated from the same workload and same host route |
| `debug_enabled` | same value in both traces; recommended value for row 12 is `true` so the later worker can inspect deterministic trace fields |
| `prompt_hash` | `sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374` |
| `tokenizer_name` | `xiigen-heuristic-word-punctuation` |
| `tokenizer_version` | `v1` |
| `tokenizer_accounting_type` | `heuristic` |
| `isolation_mode` | same fresh isolated runtime label for both traces |
| `external_ai_send` | `false` for both runs |
| `contains_raw_private_content` | `false` for both runs |
| `prompt_redacted` / `answer_redacted` | `true` in both traces where schema supports it |
| baseline `mode` | `baseline` |
| adaptive/candidate `mode` | `adaptive` |
| forbidden claims in trace | `token_savings_claimed=false`, `semantic_quality_claimed=false`, `learnability_claimed=false` |

Any mismatch in required same-fields makes the packet `SPEC_NEEDS_FIX` for measurement purposes and blocks token comparison.

## 7. Quality guard rubric

The later measurement may report a token delta only if all quality guard assertions pass. The quality guard is structured because the current source does not provide an approved numeric semantic-quality scorer for this row.

Critical failure if any item below fails:

1. Pair validation has zero errors.
2. Both traces forbid token savings, semantic quality, and learnability claims.
3. Baseline and adaptive responses preserve the marker `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`.
4. Baseline and adaptive responses preserve scenario id `frdp-p01-cursor-token-savings-baseline`.
5. `prompt_to_task` output exists and contains `task_id`, `task_title`, `constraints`, and `acceptance_gates`.
6. `task_to_plan` output exists and contains exactly three ordered steps.
7. Every plan step has one verification gate.
8. `task_to_skills` output exists.
9. Skill selection includes `prompt-to-task`, `task-to-plan`, and `task-to-skills`.
10. Every selected skill is from the allowed set: `prompt-to-task`, `task-to-plan`, `task-to-skills`, `evidence-ledger`.
11. References include the marker and scenario id.
12. `non_claims` include no token savings claim, no semantic quality claim, no readiness claim, no training, no checkpoint promotion, and no locator update.
13. The response does not become generic: it must contain the exact marker, exact scenario id, all required sections, and row-specific non-claims.
14. Deterministic shortcuts are disclosed: synthetic no-send workload, heuristic tokenizer/accounting, no external AI send, and token delta alone not being quality proof.

Pass threshold:

- `pair_validation_errors_count == 0`
- `quality_assertions_total == 14`
- `quality_assertions_passed == 14`
- `critical_failures_count == 0`
- `external_ai_send == false`
- `token_savings_claimed == false`
- `semantic_quality_claimed == false`
- `readiness_claimed == false`

Token delta observation threshold for future review:

- adaptive/candidate `token_count_total < baseline token_count_total` may be recorded only as `observed_candidate_delta`.
- It is not a token savings claim until a separate review accepts the pair, quality guard, host evidence, and non-claim audit.
- If `adaptive.token_count_total >= baseline.token_count_total`, the packet may still be useful evidence, but it fails the token-reduction observation.

## 8. Required artifacts for later measurement worker

The later worker must create or update measurement artifacts separately from this spec. Minimum required artifacts:

- `baseline-request.json`
- `baseline-response.json`
- `baseline-core-trace.json`
- `baseline-command-result.json`
- `baseline-command-output.txt`
- `baseline-command-error.txt`
- `adaptive-request.json`
- `adaptive-response.json`
- `adaptive-core-trace.json`
- `adaptive-command-result.json`
- `adaptive-command-output.txt`
- `adaptive-command-error.txt`
- `token-traces.json`
- `token-traces.jsonl`
- `pair-validation.json`
- `quality-guard.json`
- `execution-report.md`
- `execution-report.json`
- Cursor host-visible candidate evidence or exact blocker for the same marker.

Every artifact must reference:

- `row12-workload-spec.md`;
- `row12-workload-spec.json`;
- scenario id;
- pair id;
- marker;
- prompt hash;
- tokenizer/accounting source hash;
- whether external AI send occurred.

## 9. Explicit non-claims

This spec does not prove token savings.

This spec does not prove product readiness.

This spec does not prove full Cursor readiness.

This spec does not prove semantic quality improvement.

This spec does not prove learnability, training, checkpoint promotion, locator update, or host-wide readiness.

This spec only closes the workload-definition gap before a later bounded worker performs a measurement.

## 10. Exact next worker task after review

After review accepts this spec, the next bounded worker should run:

Create the row 12 measurement packet for `frdp-p01-cursor-token-savings-baseline` using `row12-workload-spec.md` and `row12-workload-spec.json`. Run the fixed no-send Cursor baseline/adaptive workload with the exact prompt hash `sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374`, same host, same debug mode, same tokenizer/accounting source, same `prompt-to-task` core API path, and no external AI send. Produce baseline/adaptive traces, pair validation, quality guard, token delta observation, host-visible candidate evidence or exact blocker, and explicit non-claims. Do not claim token savings or readiness.
