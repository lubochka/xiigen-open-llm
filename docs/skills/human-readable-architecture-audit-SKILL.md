# Human-Readable Architecture Audit Skill

## Обязательный словарь человеческой коммуникации

При загрузке этого навыка читать [Словарь человеческой коммуникации](human-communication-GLOSSARY.md). Для архитектурных объяснений различать предметную тему обучения (домен), конкретную задачу и направление, единицу/поток — получателя обучения, механизм и измерения. Unit/Flow — архитектурные сущности; оркестрация может быть темой обучения. Не выводить количество доменов из числа классов, механизмов, направлений или профилей проверок и не предполагать соответствие «один домен — одна единица».

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## When to load

Load this skill for architect, planning, plan review, code review,
documentation, and audit artifacts that explain architecture, trainable units,
learned behavior, code risks, DPO, checkpoints, manifests, KNN, dictionaries,
counters, scorers, selectors, routers, or private decision methods.

Also load it when Luba asks for iterative audit readability improvement,
Russian architecture explanation, UML, no-code algorithms, code quotes, or a
human-readable trainable-architecture audit.

Also load it when an audit claims sufficiency/completion/closure, when "v1
scope completed" needs bounded wording, or when a later broader audit in the
same architecture area finds BLOCKING/HIGH issues.

## Luba Plan-Quality Criteria / Human Architecture Priority

When comparing plans, the plan that is clearer to Luba, more readable as
architecture, more reliable architecturally, more honest about evidence, more
faithful to requirements, better controlled for execution, and lower-risk for
overclaim is the better plan. Newer status bookkeeping is not enough.

Use newer deltas from another plan to patch the human-readable stronger plan
when appropriate. Do not replace architecture quality with ledger freshness.
Do not proceed as an executor-only session until SK-INTENT, SK-PLAN,
SK-REVIEW, and SK-ARCH when applicable are loaded.

## Subagent-Only Firewall Compatibility

Human-readable audit and architecture repairs must respect the Parent Architect
Subagent-Only Firewall:

```text
Parent Architect Subagent-Only Firewall
parent local sidecar work ban
read-only verification after a sub-agent packet returns
sub-agent waiting is not a stop
no local non-overlapping work escape
```

When the current instruction is sub-agent-only, architect-only, governance-only,
or forbids the parent/current session from becoming executor, the parent must
not directly author governed audit, plan, protocol, skill, report, data, or code
repairs. Parent review may inspect and integrate only after the sub-agent packet
returns. Waiting for the packet is not a stop gate while a safe bounded next
action exists.

## Governance Repair Report Clarity Gate

When an audit, review, or repair touches governance, skills, guides, agent files,
or start prompts after NEEDS_FIX/user challenge, the Luba-facing report must
answer these questions in simple Russian before any internal packet keys:

```text
Какой слой был сломан: правила, навык, инструкция для агента, план, код продукта, тесты, документы, доказательства или публикация.
Почему нужно было изменить действующий файл, а не написать ещё один отчёт.
Что именно изменено в действующем файле и как это не даёт повторить ошибку.
Что эта правка правил не доказывает: готовность продукта, работу в реальном приложении, обучение, тесты или завершённый цикл проверки.
Что возобновляется дальше и какая ближайшая конкретная работа остаётся.
```

Internal packets may still carry machine fields such as
`target_layer_classification_before_fix`, `review_then_fix_then_fresh_read_review`,
or `unfinished_work_includes_project_governance_tests_docs_evidence`, but those
keys are secondary evidence, not Luba-facing headings.
It must not present governance repair as product readiness, host readiness,
training readiness, or a completed review/fix cycle unless that exact evidence
exists.

Allowed trainable architecture levels are exactly `ILearningMachine`,
`ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit`, and
`ILearningMachinePair`. Interface generics are code signatures, not additional
levels.

## Human-Readable Architecture Artifact Gate

The artifact must be readable by Luba as a human architect, in Russian unless
Luba explicitly asks for another language. Machine labels may exist, but they
are secondary anchors. They cannot be the main explanation.

Technical inventories, field schemas, code-reference tables, and exact packet
fields are internal or reference material. They do not become the primary
Russian architecture merely because they are complete. The primary layer must
first explain what exists, why it matters, what happens in order, and what is
expected. Diagrams, fenced examples, and tables are part of that visible layer
when they contain comments, notes, legends, branch labels, expected results, or
quoted "good" examples; those explanations must be readable in the operator's
language. Exact identifiers may remain as secondary code references.

For product/code/training-adjacent acceptance, this skill delegates the concise
reporting contract to `output-readability-gate-SKILL.md` Product-Level Human
Readability Acceptance Gate. Architecture artifacts still own UML, simulations,
and no-code algorithm clarity; the product report must also show behavior,
example input/output, expected-vs-actual tests, storage/validation-vs-learning
boundary, remaining work before first training run, and published-vs-pending
state. For a post-challenge or large rewrite, also use the semantic probe in
`output-readability-gate-SKILL.md`: one compact live fragment is checked for
meaning before the rest of the document is rewritten.

Encoding readability is part of the gate: `human-readable Russian text must not contain mojibake`.
Required literal: `rendered/readback Russian must be human-readable, not only marker-clean`.
Required literal: `mojibake scan must include common UTF-8/Windows-1252 corruption markers and human readback of Russian sections`.
A Russian human summary fails until marker-name scans and direct readback prove
the changed section is readable Cyrillic.

For extension/runtime scenario coverage artifacts, the human explanation must
include the Dual-Path Host Verification Rule / Двухконтурная проверка
host-сценариев. Do not present the scenario as one fragile `user in host ->
extension -> core -> answer in host UI` chain. Report `phase.step`, Automated
boundary status, Local desktop status, evidence, remaining work, and downgraded
claims. Automated boundary means adapter/API/core request-response, model
logic, DPO/training, token measurement, quality scoring, backup/export/import/
share/continue-training/rollback, checkpoint/locator gates. Local desktop means
agent on the local computer in the real installed host with visible marker,
screenshot/video/log evidence, and exact blocker if UI control is impossible.
JSON-only, adapter/core-only, test-only hook, process/window-list, or synthetic
helper UI cannot replace Local desktop proof; screenshot-only cannot replace
Automated boundary proof; a blocked Local desktop path is not a stop gate and
must downgrade the claim. Human-facing reports must state total scenarios,
covered by Path A, covered by Path B, blocked Path B with reason, and remaining
steps. Deprecated legacy terms `product readiness` and `product-ready count`
must not be used as the primary completion term.

Hard fail:
- table-only, shortcode-heavy, English-only, schema-first, or schema-only text;
- prompt/spec text that tells a future executor what to invent;
- code dumps without no-code explanation;
- generic rows such as "wrap in ILogicalUnit", "add KNN", "add tests",
  "create schema", "chosen/rejected/quarantine", or "make typed state".

## Required Human Format

Every architecture, trainable, code-review, documentation, or audit artifact
that touches architecture or learned behavior must include:

1. Clear Russian summary in human terms.
2. A solved architecture block for every major architecture area: problem,
   selected design, rejected alternatives, UML, data flow, current code refs,
   future files/classes/methods/signatures/dependencies,
   deterministic-vs-learned boundary or explicit N/A, gates/tests, failure
   modes, and docs.
3. UML importable to draw.io: PlantUML or Mermaid `classDiagram`, plus concrete
   data-flow simulations and expected branch outcomes.
4. No-code algorithm explanation: what happens, where, in what order, with
   class/function references, but not a code dump.
5. Code quote section. For each finding, give exact file + line + short quote,
   then:
   - сейчас;
   - почему плохо/ок;
   - должно стать;
   - какие файлы/classes/records/methods/data rows;
   - как обучается/работает;
   - какие sets/checkpoints/manifests;
   - какие tests/e2e/qa;
   - lifecycle.
6. A concrete solution immediately under each problem quote. A later appendix is
   not enough.

The code-reference table and blueprint are not the architecture explanation.
They support it after the Russian meaning is clear. A table row, code fence, or
Mermaid/PlantUML note fails when its human explanation is English-only,
machine-label-only, or understandable only by reading the source first.

For graph-style or DynamicGraph systems, the human-readable architecture block
must also explain the graph in plain language before tables: roles and
hierarchy, nodes, edges, input/output contracts, many-to-many outputs,
fan-out/fan-in, multi-output edge contracts, async execution, and which edge is
deterministic, trainable, or only helper/eval authority. Each edge needs a
hidden deterministic shortcut check; otherwise the graph explanation is not
complete enough for Luba review.

The solved architecture block is required even when the artifact is "only an
audit" or "only a review". Findings without adjacent solved design are not
enough to authorize downstream planning or implementation.

## Concrete Repair Blueprint Gate

For every suspicious Dictionary/counter/KNN/scorer/private selector/private
decision-method quote, write one concrete per-field/per-method blueprint:

```text
current field/method:
current runtime calculation:
allowed architecture level:
replacement class/unit/flow:
input/output records:
typed trained-state record and exact fields:
old private method replaced by:
new runtime call chain:
training update algorithm:
KNN/nearest method signatures and role:
sources and license gate:
positive DPO example row:
rejected DPO example row:
quarantine example row:
affected sets/checkpoints/manifests:
set rebuild or quarantine rule:
tests/e2e/qa:
storage route from current contract:
backup/export/import/share:
continue-training from checkpoint:
```

If the row contains only labels or a prompt/spec for later work, it не является
решением. The review must fail until the blueprint names concrete classes,
records, methods, tests, data rows, assets, and lifecycle paths. The blueprint
is the technical ledger behind the explanation, not a replacement for the
operator-language architecture narrative.

## Audit Sufficiency Boundary Gate

A readable audit may still be too narrow. Do not claim "достаточно", "готово",
"закрыто", "architecture audit sufficient", or "architecture audit complete"
unless the audit scope was broad enough and the breadth challenge pass is green.

Allowed bounded wording:
- "v1 scope completed" only when the exact checked scope is named;
- "focused audit found X" only when broader architecture sufficiency is not
  implied.

Forbidden completion wording:
- "Architecture audit sufficient/complete" without a green breadth challenge;
- "готово/закрыто" when the audit only checked selected quotes, one file, one
  mechanism family, or one report section;
- any schema-only or prompt/spec completion claim that hides unexamined adjacent
  runtime/trainable authority zones.

Minimum breadth challenge for architecture/trainable audit:

After focused findings, run a second pass over adjacent trainable/runtime zones:
- confidence claims and confidence floors;
- export accuracy claims;
- target-label leakage;
- runtime routing confidence floors;
- source/path/provenance leakage;
- fallback-to-first-entry and default-candidate shortcuts.

Parent review must ask and answer:

```text
What would a broader audit likely find that this artifact missed?
```

The answer must list likely missed zones, what was checked now, what remains
explicitly out-of-scope, and whether the final wording is bounded.

## Delta-Severity Feedback Gate

If the next audit in the same architecture area finds BLOCKING/HIGH issues that
should have been discoverable, treat the previous "sufficient/ready/closed"
review as a retroactive review-skill failure.

Record the repair:

```text
previous audit/review claim:
later BLOCKING/HIGH finding:
why it should have been discoverable:
why the previous review missed it:
skill/guide/index/agent rule patched:
new breadth challenge row:
completion wording corrected:
```

Return to governance repair before polishing the artifact when this gate fails.

## Review/Fix Cycle Evidence Gate

When Luba asks for iterative audit readability improvement with "minimum N",
"at least N", "preferably 10", or equivalent wording, a cycle is a real
review/fix action, not a ledger row.

Each cycle must have an inspectable `cycle_evidence` packet:

```text
cycle_number:
requested_cycle_floor:
reviewer_or_subagent_id:
artifact_before_ref:
artifact_after_ref:
loaded_skills:
loaded_gates:
findings:
fix_refs_or_no_fix_justification:
recheck_evidence:
readability_delta:
parent_review_disposition:
```

The cycle ledger may summarize and link these packets, but it cannot replace
them. `real_cycle_count` counts only valid packets created in order after prior
fixes. `ledger_row_count` is never enough by itself.

Anti-pattern: writing N ledger rows after one review, copying the same findings
across rows, or claiming "10 cycles completed" without N artifact before/after
refs and rechecks. This fails the audit and returns the artifact to
`IN_PROGRESS_NEXT_STEP_KNOWN`.

For the 10 Review Cycle Gate on an execution plan, every cycle must also record
reviewer/sub-agent id, exact plan section reviewed, findings, fixes or no-fix
rationale, recheck evidence, `requested_model=gpt-5.5`,
`requested_reasoning_effort=xhigh`, visible runtime/request metadata or
`mode_verified_from_work_order=true` when runtime metadata is unavailable and no
concrete contradiction is visible, and whether the reviewed plan proves three
full extensions for Claude, Cursor, and Codex. Generic system/developer identity
text such as "GPT-5" is not downgrade evidence by itself; a stop based only on
that text is a false downgrade stop and premature stop loophole. The ledger is
only an index; the evidence is the ordered packet set.

## Human-Readable Plan/Execution Log Gate

Execution-stage artifacts must be readable to Luba as the primary status. Every
phase, review, execution, or completion packet must include:

```text
stage:
current_plan_step:
what_changed:
what_remains:
claims_opened:
claims_closed:
why_this_status_is_true:
```

Machine-only logs, JSON matrices, status codes, or shortcode-heavy tables are
secondary evidence only. If runtime execution will continue after governance
repair, the Execution Plan Visibility Gate requires the parent to present the
plan and 10-cycle outcome in human-readable Russian in chat before continuing.

## Iterative Readability Review

When Luba explicitly asks for iterative audit readability improvement, the
review floor is minimum 6 cycles, preferably 10.

Each cycle must record:
- which skill/gate was applied;
- what failed;
- what was fixed;
- why the artifact became more human-readable.

If a cycle finds a missing skill/gate, stop polishing the artifact and return to
governance repair first. After the skill/gate is repaired, restart or continue
the readability cycle ledger with the new gate applied.

## Review Verdict

Reject the artifact when:
- Russian human summary is missing;
- Russian human summary contains mojibake or broken encoding artifacts;
- UML has no concrete simulation examples;
- algorithm text is code-first rather than no-code;
- changed architecture/data flow lacks a human-labeled UML/diagram link or
  branch simulation, when such a change is being accepted;
- expected behavior is not separated from actually observed positive/negative
  tests or probes;
- code quotes lack adjacent concrete solutions;
- solved architecture blocks are missing for architecture/trainable areas;
- deterministic-vs-learned boundary is missing or not explicitly N/A when no
  learned component is relevant;
- a solution is generic, schema-only, table-only, or prompt/spec;
- trainable repair omits typed state, training update algorithm, KNN role,
  DPO examples, sources/license, affected assets/tests, or lifecycle.
- audit completion wording overclaims sufficiency without a green breadth
  challenge;
- a later broader audit found discoverable BLOCKING/HIGH issues and the
  previous review did not record Delta-Severity Feedback Gate repair.

## Integration

Cross-reference with:
- `AGENTS.md` OP-60 Human-Readable Architecture Artifact Gate;
- `AGENTS.md` OP-61 Audit Sufficiency Boundary Gate;
- `CLAUDE.md` Human-readable architecture audit pattern;
- `plan-review-SKILL.md` FC-31 and FC-32;
- `audit-protocol-SKILL.md`;
- `trainable-architecture-decision-SKILL.md`;
- `implementation-doc-uml-SKILL.md`;
- `implementation-doc-algorithms-SKILL.md`;
- `implementation-doc-code-refs-SKILL.md`;
- `output-readability-gate-SKILL.md`.

## Plan-Family Readability Admission Addendum

For active plan artifacts that are meant for Luba or for architect review,
readability is a readiness precondition, not polish.

Reject the artifact when current human-readable sections contain mojibake or
double-encoded text markers such as these escaped marker forms:

```text
\u00d0
\u00d1
\u00c2
\u00e2\u0080\u0099
\u00e2\u0080\u009c
\u00e2\u0080\u009d
\ufffd
```

Required check:

```text
mojibake_marker_count_current_sections:
clean_cyrillic_readback_sample:
affected_files:
content_changed: false for mechanical encoding repair
```

If a later Fable/Claude/benchmark audit reports unreadable active plan Russian
while the Claude baseline is readable, the active plan must be repaired or
readiness downgraded. A clean top overlay does not make unreadable Part A blocks
ready for implementation or Luba review.
