# Training Set Build and Testing Runbook v1

Date: 2026-05-11

Scope: XIIGen internal and external DPO learning sets, with special focus on
orchestration, skills, plans, and code-documentation external sources.

This is not a presentation document. It is an operational handoff for the next
model or engineer that has to continue building learning sets without already
knowing the system.

The core lesson from the current run is simple: this corpus cannot be prepared
well by deterministic file rules alone. Scripts can inventory, validate, split,
hash, and train. They cannot decide whether a document is a root prompt, a
canonical input, a derived version, a stale output, a user-intent authority, or a
model-generated implementation answer. Those decisions must be model-led and
must use the registry/examination guides.

## Required Reading Order

Read these first, in this order:

1. `C:\Users\lubao\Documents\xiigen\llm mvp\intelrnal learn\XIIGEN-DPO-REGISTRY-GUIDE-v1.md`
2. `C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\PLAN-CODE-DOCUMENTATION-DPO-v2.md`
3. `C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\EXAMINATION-METHOD-CODE-DOCUMENTATION-v2.md`
4. `C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\CODE-DOC-EXTERNAL-SOURCE-REGISTRY-v1.md`
5. `C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\learning-sets-review-summary.json`
6. `C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\learning-sets-review-deeper-summary.json`
7. `C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\model-led-run-20260508-v1\RUN-STATUS.json`

Use v1 examination material only for historical compatibility. Train-ready
decisions should follow v2 plus the 2026-05-11 addenda.

## Current Baseline To Protect

The current accepted balanced baseline is in:

`C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\model-led-run-20260508-v1\RUN-STATUS.json`

Current accepted run:

```text
RUN-416 / V246
rows trained: 672 / 672
domain counts:
  orchestration: 257
  plans:         341
  skills:        74
weighted accuracy:      0.7068
final accuracy:         0.6875
weighted reward margin: 0.016892
final reward margin:    0.023389
elapsed ms:             388540.42
checkpoint bytes total: 2382931
```

Why this matters:

- The user explicitly wants orchestration, skills, and plans to be most accurate.
- Code is project-local and should not dominate the core learning signal.
- External code-documentation rows must not be mixed into this baseline until
  they pass standalone and combo tests.

## What Went Wrong Today

These are not theoretical concerns. They were observed in the current run.

### 1. Deterministic classification mishandled user-intent input documents

Some raw user request / genesis prompt documents were treated as uncertain and
moved toward review-required logic. That is wrong for XIIGen.

In this corpus, many user request documents are part of the original prompt
surface. They were the input to early sessions and the system was built from
them. They are not automatically lower-authority because they are user-facing or
because they mix request text with AI-synthesized design response.

Correct approach:

- Treat user-intent documents as potential root prompt authority.
- Use the registry guide to decide whether they are:
  - root user intent / genesis prompt,
  - session startup prompt,
  - flow initiation description,
  - product spec,
  - design response,
  - versioned plan,
  - stale or superseded variant.
- If the type is not obvious, do model-led adjudication. Do not let a script
  reject or demote them by filename alone.

Reference examples from prior examination:

- B162: flow initiation description documents.
- `basic_prompt.txt` family: run instruction input.
- `SESSION-START-PROMPT.md`: project-specific startup prompt.
- `FLOW-NN-PLANNING-SIMULATION-R0.md`: simulation-generated training input.

### 2. Chosen and rejected were identical in 4,660 rows

Observed in `learning sets.zip` audit:

```text
training rows:             50,000
chosen_equals_rejected:     4,660
affected rejected type:     missing-edge-case
```

This is a hard failure. If chosen and rejected are identical, the row teaches no
preference direction. It can directly damage DPO signal even when JSON is valid.

Correct approach:

- Hard reject from train-ready.
- Put in repair-needed.
- Rebuild the rejected side with an actual wrong behavior:
  - omitted edge case,
  - wrong level of abstraction,
  - name echo,
  - mechanical paraphrase,
  - wrong dependency order,
  - implementation jump where planning/orchestration was needed.

### 3. Mojibake appeared in training fields

Observed in `learning sets.zip` audit:

```text
mojibake rows total:          26,909
mojibake in prompt:            1,000
mojibake in chosen:            4,432
mojibake in rejected:          1,032
mojibake in annotation:       24,500
mojibake in train fields:      4,912
```

Mojibake in annotations is bad but may be repairable after triage. Mojibake in
prompt, chosen, or rejected is not train-ready.

Correct approach:

- Detect markers such as `Ã`, `Â`, `â€`, `Ã¢`.
- Move affected rows to repair-needed.
- Repair source encoding before rewriting pairs.
- Do not train corrupted strings.

### 4. Compact external rows hurt accuracy when added too broadly

The run history showed that some external PLAN/ARCH compact rows were useful
only after careful filtering. Larger continuation batches hurt the baseline.

Current accepted/rejected evidence in `RUN-STATUS.json`:

```text
Accepted:
  V236: 64 rows, accuracy 0.8906, margin 0.038659
  V242_tail16: 16 rows, accuracy 0.75, margin 0.024962
  V245: 16 skill rows, accuracy 0.6875, margin 0.023389

Rejected:
  V234: 16 rows, accuracy 0.375, margin -0.003348
  V235: 128 rows, accuracy 0.3438, margin -0.010048
  V238: 16 rows, accuracy 0.5, margin 0.00105
  V241: 16 rows, accuracy 0.4375, margin -0.00668
```

Correct approach:

- Never push a broad external batch directly into the current balanced baseline.
- Run standalone ablation first with 16-64 rows.
- Promote only if reward margin is positive and accuracy is not damaging.
- Run a combo test after standalone success.
- Keep rejected batches documented.

### 5. External code-documentation domains were generated before registry coverage

`learning sets.zip` includes five 10k-row domains:

```text
angular-typescript:      10,000
dotnet-csharp:           10,000
elasticsearch-csharp:    10,000
mongodb-csharp:          10,000
sql-tsql:                10,000
```

But the code-documentation source registry originally covered only part of that
surface. `mongodb-csharp` and `elasticsearch-csharp` must get registry entries
before they can be train-ready.

Correct approach:

- JSON validity is not source authority.
- Generated rows are not train-ready just because they are structured.
- Add source registry entries, license state, canonical docs, and structure
  rules first.

### 6. `compiled_prompt_in_training=false` is not always a failure

For orchestration, planning, skill, and internal registry pairs, training text
should usually be compiled and explicit.

For code-documentation `CODE_TO_DOC` rows, raw snippet mode can be valid:

```text
compiled_prompt_in_training: false
prompt_mode: raw_snippet
pair_kind: CODE_TO_DOC
```

Correct approach:

- Do not use one global compiled-prompt rule across all domains.
- Validate prompt mode against pair kind and source registry.
- Raw snippet is not valid for orchestration/planning/skill governance pairs.

### 7. Weak negatives can lower weighted accuracy

Rows with vague rejected answers, generic implementation todos, or
near-paraphrases often fail to teach the intended boundary.

Correct approach:

- Rejected must be wrong for a specific reason.
- Always include `[WRONG: ...]` with the defect.
- For orchestration/plans/skills, prefer hard negatives:
  - wrong node order,
  - wrong authority source,
  - implementation instead of planning,
  - stale version instead of canonical version,
  - skill used in the wrong session phase,
  - missing trust-state or FlowFactory rule,
  - missing human/user-intent input authority.

## Build Pipeline

The build pipeline has two different kinds of work:

- model-led semantic work,
- deterministic mechanical work.

Do not confuse them.

### Stage 1: Inventory every source surface

Include all files, all subdirectories, and all ZIP contents.

Source roots requested by the user:

```text
C:\Projects\xiigen mvp
C:\Projects\llm_mvp_core
C:\Users\lubao\Documents\xiigen
```

External source root:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2
```

Internal output root:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2
```

Mechanical scripts may enumerate files and ZIP members. Model must decide
meaning, authority, type, version, and training use.

### Stage 2: Model-led type and authority adjudication

For each document, decide:

- What document family is it?
- Is it root input, generated output, registry, prompt, plan, skill, state,
  evidence, test result, source code, or archive container?
- Is it canonical, stale, extracted_copy, patch, limited_authority, or
  review-required?
- Is it a new type or a new version of an existing type?
- What DPO behavior can it teach?
- What is the strongest wrong answer?

This stage must use the registry guide. Do not hard-code the answer from
filename patterns alone.

Important rule:

Raw user requests and flow initiation documents are not suspicious by default.
They often are the root prompt authority.

### Stage 3: Build candidate rows by domain priority

Priority order for the current internal baseline:

1. orchestration,
2. plans,
3. skills,
4. registry/type adjudication,
5. external code-documentation only after isolated testing.

For orchestration/plans/skills, the row should preserve the actual reasoning
surface:

- A-side: realistic request or question.
- Chosen: correct behavior with the right authority, order, and scope.
- Rejected: plausible but wrong behavior with `[WRONG: ...]`.

Do not replace system reasoning with generic implementation advice.

### Stage 4: Validate before train-ready

Hard gates:

```text
JSONL parses
pair id unique
prompt/a_side exists
chosen exists
rejected exists
chosen != rejected
rejected contains [WRONG:]
no mojibake in prompt/chosen/rejected
overlap_ratio <= 0.60 unless explicitly justified
domain registered or intentionally internal
source path exists or source authority is documented
canonical/stale/version state is explicit
prompt_mode valid for pair kind
ledger/status rows are not used as DPO rows
```

Rows that fail gates go to repair-needed, not train-ready.

### Stage 5: Train small first

Training order:

1. Build 16-64 row standalone ablation.
2. Train/test standalone.
3. Accept only if reward margin is positive and accuracy is acceptable.
4. Run combo test against current accepted baseline.
5. Accept only if combo does not damage the protected baseline.
6. Record accepted/rejected in `RUN-STATUS.json`.

Why:

Large batches can hide bad row families. Small runs show whether a source family
has actual signal.

## Where The Logic Lives In Code

The scripts are not the source of truth for type meaning. They are reference
implementations for mechanical build/test behavior.

### Current run state and metrics

```text
C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\model-led-run-20260508-v1\RUN-STATUS.json
```

Use this for:

- current accepted baseline,
- rows trained,
- domain counts,
- accuracy,
- loss,
- reward margin,
- elapsed time,
- checkpoint bytes,
- accepted additions,
- rejected runs.

### Run script archive

Fresh root-level scripts were moved here:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\learning runs internal sets
```

Older model-led run scripts are here:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\model-led-run-20260508-v1
```

### Scripts that explain the current build logic

Use these as code references:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\learning runs internal sets\run366-build-orchestration-skill-plan-supplement-v199.ps1
```

What it shows:

- domain-priority rows for orchestration, skills, and plans,
- explicit `domain_focus`,
- strong target behaviors,
- rejected answers with `[WRONG:]`,
- why these domains are tier-0.

```text
C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\learning runs internal sets\run373-build-external-orch-plan-supplement-v203.ps1
```

What it shows:

- importing external PLAN/ARCH compact and structured pairs,
- preserving external license checked state,
- converting external rows into orchestration/planning curriculum candidates,
- why external rows must still be tested.

```text
C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\learning runs internal sets\run375-build-v204-guide-root-domain-curriculum.ps1
```

What it shows:

- guide-root curriculum building,
- use of external source guides,
- domain inference for skills/orchestration/plans,
- overlap calculation,
- adding model-led guide pairs.

```text
C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\learning runs internal sets\run377-build-v205-structured-external-only.ps1
```

What it shows:

- removing compact external PLAN/ARCH rows after weak training behavior,
- keeping structured external rows,
- stratifying by normalized family,
- updating current train-ready pointer.

```text
C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\learning runs internal sets\run381-build-v211-rewrite-repaired-domain-rows.ps1
```

What it shows:

- rewriting repaired domain rows,
- strengthening weak candidates before re-entry.

```text
C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\learning runs internal sets\run388-build-broad-accepted-rewritten-chunk.ps1
```

What it shows:

- re-entering broad accepted rows only after adding stronger domain-specific
  contrast,
- parameterized chunking by skip/take/version,
- protecting the current pointer and quality file.

### Training-ready outputs

Current training-ready material is under:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\model-led-run-20260508-v1\train-ready-v2
```

Use the pointer file there to know which dataset is current. Do not infer from
the newest filename alone.

### Evidence and quality files

Evidence lives under:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\model-led-run-20260508-v1\evidence
```

Quality JSON files live under:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\model-led-run-20260508-v1\train-ready-v2\quality
```

Read quality files before deciding that a generated set is good.

## External Code-Documentation Sets

The external code-doc sets are useful, but they are not automatically safe.

Audited archive:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\learning sets.zip
```

Audit summaries:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\learning-sets-review-summary.json
C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\learning-sets-review-deeper-summary.json
```

Observed result:

```text
250 training JSONL files
50,000 training rows
0 invalid JSON
50,000 unique pair ids
4,660 chosen == rejected
4,912 rows with mojibake in prompt/chosen/rejected
42,486 rows with annotation_strong=false
0 rows with overlap_ratio > 0.60
```

Correct use:

- Treat as a large candidate library.
- Split into clean-trainable, repair-needed, and audit-ledger.
- Register missing domains before training.
- Repair identity and encoding failures.
- Run standalone ablation before mixing with internal baseline.

Do not use this set as direct input to the orchestration/plans/skills baseline.

## How To Build A Good Pair

A good pair has one learning target.

Pattern:

```json
{
  "id": "stable unique id",
  "domain_focus": "orchestration | plans | skills",
  "source": "real source path",
  "source_span": "line/section/file member",
  "a_side": "question/request that a real session would ask",
  "chosen": "correct answer using the right authority and order",
  "rejected": "plausible wrong answer. [WRONG: specific defect]",
  "pair_kind": "specific behavior family",
  "target_behavior": "what this row should teach"
}
```

Good rejected examples:

- Uses stale version instead of canonical version.
- Treats root user intent as low-authority derived text.
- Sends planning work directly to implementation.
- Skips Layer 0 routing and jumps to FormatterUnit.
- Uses confidence without trust-state.
- Applies a skill outside its allowed session phase.
- Produces code when the request asks for architecture or plan adjudication.

Bad rejected examples:

- Same text as chosen.
- Chosen with one negated sentence.
- Generic "do less detail" answer.
- Mechanical paraphrase.
- Wrong answer that is too obviously unrelated.

## Model-Led Typing Rule

When a type is unclear, do this:

1. Read the source document.
2. Read its surrounding directory and ZIP context.
3. Identify producer and consumer:
   - Was it human input?
   - Was it AI-generated response?
   - Was it a session output?
   - Was it a state/checkpoint?
   - Was it a registry or guide?
   - Was it a patch/appendix?
4. Identify lifecycle:
   - first version,
   - revised version,
   - canonical current version,
   - stale superseded version,
   - extracted copy,
   - limited-authority external synthesis.
5. Map to existing registry type if possible.
6. If not possible, create an undefined-type candidate with evidence.
7. Do not demote the row just because code cannot classify it.

This is the rule that protects user request documents and early prompt surfaces.

## Testing Checklist

Before training:

```text
[ ] All intended files, subdirectories, and ZIP contents were inventoried.
[ ] Each candidate row has source path and source span.
[ ] Type/canonicality was model-adjudicated when ambiguous.
[ ] User-intent documents were checked as possible root prompt authority.
[ ] chosen != rejected.
[ ] rejected contains [WRONG: ...].
[ ] No mojibake in prompt/a_side/chosen/rejected.
[ ] Domain is registered or internal.
[ ] Pair kind matches prompt mode.
[ ] Ledger/status rows are excluded from DPO training rows.
[ ] Domain counts are visible.
[ ] Quality JSON generated.
[ ] Current train-ready pointer updated only after validation.
```

After training:

```text
[ ] rows_trained == rows_total.
[ ] weighted accuracy did not regress materially.
[ ] reward margin is positive.
[ ] final reward margin is not collapsing.
[ ] low-performing batch is recorded as rejected.
[ ] accepted batch is recorded with rows, accuracy, margin, and domain counts.
[ ] RUN-STATUS.json is updated.
```

## Acceptance Policy

A new set may be accepted into the balanced baseline only when:

- it protects orchestration/plans/skills signal,
- it improves or preserves reward margin,
- it does not damage weighted accuracy,
- it has no identity or encoding failures,
- its negative examples are meaningful,
- its source authority is traceable,
- its canonicality/version state is explicit.

If a set is useful but weak, keep it as candidate or repair-needed. Do not force
it into train-ready because it is large.

## What The Next Model Should Do First

1. Read this runbook.
2. Read the registry guide and the external source guides.
3. Read `RUN-STATUS.json`.
4. Inspect `CURRENT-TRAIN-READY-POINTER.json` in `train-ready-v2`.
5. Review rejected runs before adding new external rows.
6. Continue with small, model-adjudicated, domain-priority batches.

The goal is not to produce the biggest set. The goal is to produce rows that
teach the system how to reason about XIIGen orchestration, plans, and skills
without losing the root prompt and document-authority structure that built the
system in the first place.
