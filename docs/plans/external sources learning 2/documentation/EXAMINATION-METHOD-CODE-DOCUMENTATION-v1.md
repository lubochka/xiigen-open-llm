# Examination Method — Code Documentation DPO Domain
# Version: 1.0 — 2026-05-10
# Domain: code-documentation
# Supersedes: none (new domain)
# Authority: CODE-DOC-EXTERNAL-SOURCE-REGISTRY-v1.md
#             PLAN-CODE-DOCUMENTATION-DPO-v2.md
# Companion to: EXAMINATION-METHOD-ORCHESTRATION-v15.md
#               DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v8.md
# Grounded in: XIIGEN-DPO-INTERNAL-TEACHING-PLAN-v2.md (overlap and margin rules)
# Operational runbook: TRAINING-SET-BUILD-AND-TESTING-RUNBOOK-v1.md

---

## What This Document Is

This document governs how to examine each source document from the external
source registry, how to extract candidate pairs, how to score them, how to
assemble batches, and how to measure whether the batch will produce positive
reward margin.

It is the code-documentation equivalent of EXAMINATION-METHOD-ORCHESTRATION-v15.md.
Where the orchestration method covers ARCH, PLAN, BUG, and CODE_Q domains derived
from internal XIIGen session documents, this document covers CODE_TO_DOC pairs
derived from external open-source documentation repositories.

Read this document alongside the external source registry. The registry defines
what sources are approved and what their pair types are. This document defines
how to examine each source and construct valid pairs from it.

---

## 2026-05-11 Compatibility Addendum

This v1 method is retained for historical continuity, but train-ready work
should use `EXAMINATION-METHOD-CODE-DOCUMENTATION-v2.md` plus the 2026-05-11
run-experience gates.

The audit of `documentation/learning sets.zip` changed the minimum gates:

```
chosen == rejected          -> hard reject
mojibake in prompt/chosen/rejected -> repair before training
unregistered domain         -> repair-needed until registry entry exists
ledger rows                 -> never DPO train-ready rows
compiled_prompt_in_training:false -> allowed only for CODE_TO_DOC raw-snippet mode
```

The old v1 batch-composition rules are insufficient by themselves because they
do not catch identity failures or encoding corruption. Any v1-era batch must be
revalidated through the v2 checklist before training.

---

## The Quality Standard — Active Throughout Examination

Every examination step is governed by the clever documentation standard.
Examiners must keep it present during every file read.

A chosen side is clever documentation when it answers all three of:

```
INTENT — what does this code accomplish at the purpose level?
  Signal words: ensures, prevents, finds, returns when, guarantees,
  allows callers to, provides.
  Fail: narrates operations without stating purpose.

WHY — what design decision is embedded here that is not obvious from the code?
  Signal words: instead of, because, to avoid, rather than,
  preferred when, unlike, avoids.
  Fail: describes what the code does with no comparison or reason.

EDGE CASE — what non-obvious condition, failure mode, or input changes behavior?
  Signal words: unless, when empty, if null, throws when,
  returns null if, note that, be aware, cancellation, disposed.
  Fail: only the happy path described.
```

A chosen side covering all three → curriculum tier 1.
A chosen side covering intent and why → curriculum tier 2.
A chosen side covering intent only → curriculum tier 3.
A side covering none → discard or use as rejected material.

---

## Reward Margin Mechanics — Why This Examination Exists

From calibration run 2026-05-06: 29 of 40 batches had negative reward margin
because the rejected side shared too many tokens with the chosen side.

For CODE_TO_DOC pairs, the same risk applies. A weak rejected side that
paraphrases the chosen side in slightly different words will have overlap above
0.60 and will produce negative margin. The examination protocol at every step
is designed to prevent this.

The token overlap test is mandatory before any pair enters a batch:

```
shared = tokens in both chosen and rejected NOT in the prompt (snippet)
total  = tokens in chosen + tokens in rejected − tokens in prompt
overlap_ratio = shared / total

overlap_ratio > 0.60 → discard pair. Do not submit.
overlap_ratio 0.50–0.60 → keep only if annotation_strong is true.
overlap_ratio < 0.50 → proceed to batch assembly.
```

Expected overlap by rejected type for CODE_TO_DOC pairs:

| Rejected type | Expected overlap | Expected margin |
|---|---|---|
| Type 2 (name echo) | 0.05–0.20 | Reliably positive |
| Type 3 (wrong level) | 0.15–0.35 | Reliably positive |
| Type 1 (mechanical paraphrase) | 0.30–0.45 | Positive with annotation |
| Type 4 (missing edge case) | 0.40–0.55 | Positive only with strong annotation |
| Generic negation template | 0.65–0.80 | Reliably negative — forbidden |

---

## Processing Cycle — 6 Steps per Source File

```
Step 1. File scan       → identify candidate code-explanation pairs
Step 2. Eligibility     → apply size and quality filters
Step 3. Quality scoring → score explanation for intent / why / edge case
Step 4. Pair construction → chosen + rejected + annotation
Step 5. Batch assembly  → compose batch meeting minimum requirements
Step 6. Measurement gate → verify overlap ratios before committing
```

Each step has a gate. A step that fails its gate stops processing that file
until the failure is resolved.

---

## Step 1 — File Scan

### What to record per file

```
file_id              — registry_id + relative path + commit SHA
source_registry_id   — which registry entry this file belongs to
language_tag         — the code fence tag found (csharp / sql / typescript)
candidate_count      — code blocks passing the language tag filter
filtered_count       — code blocks excluded by size or quality filter
surviving_count      — candidate pairs reaching Step 3
has_multi_block      — boolean: does any example show multiple consecutive
                       code blocks before one explanation?
explanation_position — before-code | after-code | mixed (per file)
```

### Extraction heuristic — apply per source

Each source in the registry has a provisional extraction heuristic. The
heuristic is refined after Phase 2a examination of three representative files
per source. The general form is:

A candidate pair is a code fence (triple-backtick block with a language tag
matching the source domain) that is preceded or followed within the
examination window by a prose paragraph of 40 or more words that does not
itself contain a code fence.

**Examination window by source:**

| Registry ID | Window | Position | Reason |
|---|---|---|---|
| dotnet-docs-mit | 600 characters | after-code | C# docs often insert a blank line between code and explanation |
| sql-docs-mit | 500 characters | after-code | SQL docs follow code directly with explanation |
| angular-adev-mit | 500 characters | after-code | Angular guide structure: code then explanation |
| All EXT-2 sources | 400 characters | after-code | Project READMEs tend to be more compact |

### Gate 1

Minimum to continue processing a file: `surviving_count >= 1` after language
tag filter. A file with zero matching code blocks is logged as not applicable
and skipped. Do not attempt pair construction.

---

## Step 2 — Eligibility Filter

Apply these filters to every candidate pair. A candidate failing any filter
gets `pair_status: filtered-out` with a `filter_reason` recorded.

### Size filters

```
Minimum snippet lines:    5
  Reason: fewer than 5 lines has no separable intent-level meaning distinct
  from a mechanical description.
  filter_reason when failing: snippet-too-short

Maximum snippet lines:   80
  Reason: a snippet larger than 80 lines cannot be documented at the snippet
  level — it covers too many concerns for one documentation unit.
  filter_reason when failing: snippet-too-long

Minimum explanation words: 40
  Reason: fewer than 40 words cannot meaningfully address even one quality
  dimension (intent / why / edge case).
  filter_reason when failing: explanation-too-short

Maximum explanation words: 600
  Reason: explanations above 600 words are usually section-level prose
  describing the surrounding context rather than the specific snippet.
  filter_reason when failing: explanation-too-long
```

### Content filters

```
Language tag must match domain:
  dotnet-csharp:      language_tag in [csharp, cs, c#]
  sql-tsql:           language_tag in [sql, tsql, sql-server, t-sql]
  angular-typescript: language_tag in [typescript, ts]
  filter_reason when failing: wrong-language-tag

Explanation must not be a section heading:
  Reject if explanation_raw starts with ## or ###
  filter_reason when failing: explanation-is-heading

Explanation must not be structural prose only:
  Structural prose patterns to detect and exclude:
    "In the following example..."
    "The following example shows..."
    "The following code..."
    "As shown in the example..."
  If explanation_raw consists entirely of one of these phrases followed
  by fewer than 20 additional words: exclude.
  filter_reason when failing: explanation-is-structural-only

Snippet must not be an import or namespace block:
  Reject if 80% or more of non-blank lines start with:
  using, import, #using, namespace, @import, from (followed by a path)
  filter_reason when failing: snippet-is-import-block

Snippet must not be a syntax demonstration without logic:
  Reject if the snippet contains no executable statement (no assignment,
  no method call, no return, no conditional, no loop).
  This catches code blocks that show only type declarations or attribute
  syntax with no method body.
  filter_reason when failing: snippet-is-syntax-only
```

### SQL-specific filter

```
Multi-statement blocks:
  If a SQL code block contains more than one distinct SQL statement
  (identified by semicolons or GO separators), check whether the
  explanation refers to all statements or only the final query.
  If the explanation refers only to the final query: truncate the
  snippet to the final query only. Record truncation in filter_reason.
  If the explanation refers to the setup queries as well: keep the full
  block but note that pair quality scoring must cover all statements.
```

### Angular-specific filter

```
Template-only blocks:
  If a typescript code block contains only a @Component decorator with
  template HTML and no class body, exclude it.
  filter_reason when failing: snippet-is-decorator-only

Split component blocks:
  When two consecutive code blocks appear (one html, one typescript),
  use the typescript block as the candidate snippet and the explanation
  following both blocks as the candidate explanation.
  Record in extraction_heuristic: "split-component-pair"
```

### Gate 2

Minimum to continue: at least 1 candidate survives all filters in the file.
If zero survive, log the file as fully filtered and move to the next file.

---

## Step 3 — Quality Scoring

Score each surviving candidate explanation against the three quality dimensions.
Record the three boolean flags in the source row.

### Scoring method

Read `explanation_raw` and answer three yes/no questions:

```
intent_covered:
  Does the explanation state what the code accomplishes at the level of
  purpose or outcome — not just what operations it performs?

  YES: Uses purpose verbs: ensures, prevents, finds, returns when, guarantees,
  allows callers to, provides. Framing is about the result, not the mechanics.

  NO: Narrates operations in order (first X, then Y, returns Z) without
  stating what the overall purpose is. A caller reading this would know
  what the code does but not why it exists.

why_covered:
  Does the explanation mention a design decision, trade-off, or reason for
  the approach used?

  YES: Uses comparison or reason language: instead of, because, to avoid,
  rather than, preferred when, unlike, avoids, this choice. A reader
  would understand why this approach was taken over alternatives.

  NO: Pure description with no comparison or reason. The explanation tells
  the reader what the code does but not why it does it this way.

edge_case_covered:
  Does the explanation mention a non-obvious condition, failure mode, or
  input that changes the code's behavior?

  YES: Uses caveat language: unless, when empty, if null, throws when,
  returns null if, note that, be aware, cancellation, disposed, be careful,
  important, warning. A caller reading this learns something that is not
  immediately obvious from the code itself.

  NO: Only the happy path described. A caller could be surprised by
  behavior under edge conditions.
```

### Scoring outcomes and curriculum tier assignment

```
intent + why + edge case → curriculum_tier: 1 — passes all three gates
intent + why             → curriculum_tier: 2 — edge case missing
intent only              → curriculum_tier: 3 — minimum acceptable chosen side
None YES                 → discard from chosen pool

A row where intent_covered is false is not usable as a chosen side.
It may be usable as rejected Type 1 (mechanical paraphrase) material.
Record it with pair_status: filtered-out, filter_reason: no-intent-coverage,
and a note: candidate-for-rejected-type-1.
```

### Automated scoring approach

For large-scale scoring, use an LLM call with the following prompt pattern:

```
Given this code snippet:
---
{snippet_raw}
---
And this explanation:
---
{explanation_raw}
---

Answer three yes/no questions about the explanation:
1. intent_covered: Does the explanation state what the code accomplishes
   at the purpose level, not just what operations it performs?
2. why_covered: Does the explanation mention a design reason or trade-off
   for the approach used?
3. edge_case_covered: Does the explanation mention a non-obvious condition,
   failure mode, or input that changes behavior?

Return only a JSON object with three boolean fields:
{"intent_covered": true/false, "why_covered": true/false, "edge_case_covered": true/false}
```

Review a sample of automated scores manually (10% of each batch) before
trusting the scorer at scale. Calibrate the scorer against the quality
standard if it consistently over-scores `why_covered` or under-scores
`edge_case_covered`.

### Gate 3

Minimum to continue: at least 1 candidate has `intent_covered: true` in
the file. If zero, log the file as producing no valid chosen sides.

---

## Step 4 — Pair Construction

### Choosing the rejected type

For each valid chosen candidate, select one rejected type. The selection
criteria:

```
Type 2 (name echo) — use when:
  The code snippet has a well-named method, class, or property whose
  name, when split at PascalCase or camelCase boundaries, produces a
  grammatical sentence. This is the easiest type to produce and the most
  reliable margin. Include at least 20% of pairs per batch as Type 2.

Type 3 (wrong level) — use when:
  The code snippet has visible internal mechanics (a loop variable, a
  temp variable, an internal helper call) that a caller would not need
  to understand. The wrong-level rejected describes these mechanics while
  saying nothing about the caller-visible contract.
  Works well for: stored procedures, Angular services, complex LINQ chains.

Type 1 (mechanical paraphrase) — use when:
  The code snippet has a clear sequence of operations that can be narrated
  in order. The mechanical paraphrase sounds like documentation but adds
  nothing beyond what a reader can already see in the code.
  Works well for: straightforward C# methods, SELECT queries, component
  lifecycle hooks.

Type 4 (missing edge case) — use when:
  The chosen side covers an edge case (intent_covered and edge_case_covered
  are both true). The missing-edge-case rejected is identical to the chosen
  side minus the edge case information.
  Only use Type 4 when the chosen side has edge_case_covered: true.
  Do not manufacture an edge case for the rejected if the chosen did not
  identify one. That would make the rejected more informative than intended.
```

### Authoring the rejected side

**Type 1 — Mechanical paraphrase**

Narrate each operation in the snippet in order using plain English. Use no
purpose verbs. Include no reason for the approach. Include no edge cases.

Structure: "[verb]-s [object]" for each statement in order.
For a LINQ chain: "Filters the [collection] where [condition], orders by
[field] [direction], and returns the [first/all] result[s]."
For a C# method: "Calls [method], assigns the result to [variable],
and returns [value]."
For a SQL query: "Selects [columns] from [table], joins [table2] on
[condition], and filters where [condition]."

Strip out: purpose language, design reasoning, caller guidance, edge cases.

**Type 2 — Name echo**

Split the primary identifier (method name, class name, property name,
stored procedure name) at PascalCase or camelCase word boundaries.
Rearrange into a sentence ending with a period.

Examples:
  `GetActiveUsersByDepartment` → "Gets the active users by department."
  `SaveOrderConfirmationAsync` → "Saves the order confirmation asynchronously."
  `usp_GetPaginatedOrdersByTenant` → "Gets paginated orders by tenant."
  `notificationService.sendWelcomeEmail` → "Sends the welcome email."

Do not add any additional information. The name echo must be exactly the
identifier rephrased. The shorter it is, the lower the overlap ratio.

**Type 3 — Wrong level**

Identify the internal mechanics visible in the snippet. Write documentation
that describes these mechanics in detail while saying nothing about:
- what inputs the code requires
- what the code returns
- what the code guarantees to the caller
- what happens under edge conditions

For a stored procedure: describe the temp table structure, the intermediate
aggregation steps, the cursor logic (if any). Say nothing about parameters
or result set shape.

For an Angular service: describe the BehaviorSubject initialization, the
asObservable() call, the internal state variable. Say nothing about what
subscribers receive or when they receive it.

For a LINQ chain: describe the deferred execution, the iterator protocol,
the expression tree construction. Say nothing about what the caller gets
back or what happens with an empty collection.

**Type 4 — Missing edge case**

Copy the chosen side. Remove the edge case information.

Locate the sentences or phrases in the chosen side that contain:
unless, when empty, if null, throws when, returns null if, note that,
be aware, cancellation, disposed, be careful, warning.

Remove those sentences. Ensure the remaining text is grammatically complete.
Do not add any new content. The missing-edge-case rejected must be a strict
subset of the chosen side — everything it says is correct, the weakness is
only what it omits.

After removing the edge case content, compute the overlap ratio. If
overlap_ratio > 0.55, the pair needs a stronger annotation. If
overlap_ratio > 0.60, discard and choose a different rejected type for
this candidate.

### Writing the annotation

Every rejected side ends with a `[WRONG: ...]` block. The annotation names
the specific weakness using the standard from the DPO pair authoring guidance.

**Annotation structure for CODE_TO_DOC pairs:**

```
[WRONG: "{rejected type label}" — {what the rejected side fails to tell the caller}.
{What a caller relying on the rejected side would not know}.
{What the chosen side adds that the rejected omits}.]
```

**Annotation by rejected type:**

Type 1 template:
```
[WRONG: "mechanical paraphrase" narrates the operations the code performs
without stating {what the code is for / what it guarantees / why this approach
was chosen}. A caller reading only this would know what the code does mechanically
but not {the purpose it serves / when to use it / what to check in the result}.]
```

Type 2 template:
```
[WRONG: "name echo" restates the {method/procedure/class} name as a sentence.
The caller learns nothing about {what inputs are required / what the code returns /
what happens on failure}. Real documentation answers the question a caller has
when encountering an unfamiliar {method/procedure/component} — this does not.]
```

Type 3 template:
```
[WRONG: "wrong level" describes the internal {BehaviorSubject / temp table /
loop variable / local helper} that callers never interact with.
The caller needs to know {what to pass in / what comes back / what guarantees
the code makes}. Internal implementation details answer none of these questions.]
```

Type 4 template:
```
[WRONG: "missing edge case" correctly describes {what the code does at intent level}
but omits {the specific edge case}. A caller following only this documentation
would not know that {consequence of the missing edge case}.]
```

**Annotation quality gate:**

Strong annotation (acceptable): names the specific weakness type, states what
the caller cannot learn from the rejected side, names the consequence.

Weak annotation (not acceptable): says only "this is not helpful" or "missing
information" without naming what is missing and why it matters.

A pair with `overlap_ratio > 0.45` and a weak annotation is invalid.
The annotation must carry enough distributional signal to compensate for
the high overlap.

### Gate 4 — Overlap check per pair

Before recording any pair:

```
□ Compute overlap_ratio
□ overlap_ratio > 0.60 → discard. Record as filtered-out: overlap-exceeded.
□ overlap_ratio 0.50–0.60 → check annotation_strong. If false: discard.
□ For Type 4 pairs with overlap_ratio > 0.50:
  count the tokens in the annotation NOT present in the rejected text.
  If count < 5 → annotation is insufficient. Revise annotation or discard.
□ [WRONG: ...] annotation is specific — names weakness type and consequence.
□ Rejected is a coherent response, not a negation phrase.
```

---

## Step 5 — Batch Assembly

### Composition minimums

Each batch must meet these minimums before it is committed.

| Pair type | Minimum fraction | Notes |
|---|---|---|
| Type 2 (name echo) | 20% | Most reliable margin. Include in every batch. |
| Type 3 (wrong level) | 25% | Low to moderate overlap. Strong margin. |
| Type 1 (mechanical paraphrase) | 30% | Moderate overlap. Annotation weight. |
| Type 4 (missing edge case) | 15% | Highest overlap. Strong annotation required. |
| Curriculum tier 1 pairs | 20% minimum | At least one-fifth cover all three quality flags. |
| Sub-domain variety | 3+ different sub-domains | Prevents over-fitting to one topic. |

No code snippet (prompt) may appear as the basis for more than one pair in
the same batch. When duplicates are found after assembly:
- Keep the pair with the lowest overlap_ratio.
- Discard the others.
- Record discarded pair IDs in the batch assembly log.

### Domain distribution target per batch

| Domain | Target fraction |
|---|---|
| dotnet-csharp | 40% of pairs |
| sql-tsql | 30% of pairs |
| angular-typescript | 30% of pairs |

Adjust target fractions based on available source volume. The constraint is
that no batch consists entirely of one domain.

### Batch size

Minimum pairs per batch: 8
Maximum pairs per batch: 50

Batches below 8 pairs do not produce statistically reliable reward margin
measurements. Batches above 50 pairs take too long to diagnose when margin
is negative.

### Gate 5 — Pre-batch composition check

Before proceeding to measurement:

```
□ Rejected type distribution meets minimums
□ Curriculum tier 1 pairs >= 20%
□ At least 3 different sub-domains represented
□ No prompt appears more than once
□ All pairs have overlap_ratio < 0.60
□ All pairs with overlap_ratio > 0.45 have annotation_strong: true
□ source_boundary: external on every row
□ compiled_prompt_in_training: false on every row only for CODE_TO_DOC raw-snippet mode (confirm with runner config)
□ chosen.trim() != rejected.trim()
□ prompt/chosen/rejected contain no mojibake markers: Ã, Â, â€, Ã¢
□ domain is registered in CODE-DOC-EXTERNAL-SOURCE-REGISTRY-v1.md
```

---

## Step 6 — Measurement Gate

### Metric 1 — Reward margin

```
reward_margin_mean = mean(chosen_reward − rejected_reward) across all pairs
```

Rule: `reward_margin_mean < 0` → reject batch. Do not commit.

### Metric 2 — Loss versus random-guess floor

```
loss_mean
```

Rule: `loss_mean ≈ 0.693` (natural log of 2) → no learning occurring.
Overlap scan required before resubmission.

### Overlap scan when either metric fails

```
For each pair in the batch:
  if overlap_ratio > 0.50: flag as suspect
  if overlap_ratio > 0.60: mark as discard — this pair failed Gate 4 and
    should not have reached this step
Report: flagged pairs with overlap_ratios, rejected type, curriculum tier.
```

### Diagnosis table

| Symptom | Likely cause | Fix |
|---|---|---|
| All pairs negative margin | Generic negation strings in rejected | Replace all rejected with typed approaches |
| Type 4 pairs all negative | High overlap with weak annotation | Strengthen annotation or convert to Type 3 |
| Type 1 pairs negative | Mechanical paraphrase too similar to chosen | Check sub-domain — some topics have less vocabulary divergence. Add more Type 2 |
| Loss stays at 0.693 | compiled_prompt_in_training misconfigured | Verify runner accepts false for this domain |
| All pairs near-zero margin | Chosen and rejected same vocabulary | Check that rejected type taxonomy is applied — name echo and wrong level should produce strong distributional difference |
| Tier 1 pairs all negative | Full three-flag chosen too close to rejected | Verify overlap for tier 1 pairs specifically. Tier 1 chosen is longer — check it is not closer to Type 1 rejected than expected |
| Angular pairs all negative | TypeScript snippets share vocabulary with any documentation style | Apply more Type 2 pairs for Angular (short identifier names produce very low overlap) |

---

## Source File Examination Protocol — Per Repository

This section contains the protocol for Phase 2a manual examination of three
representative files per Tier 1 repository. Run this before writing the
harvester. Record findings in the examination log.

### dotnet-docs-mit — Three files to examine

```
File 1: docs/csharp/asynchronous-programming/async-in-depth.md
  Questions to answer:
  — How many code-explanation pairs appear in this file?
  — Are explanations consistently after the code block?
  — What is the typical gap (characters) between code block and explanation?
  — Does the explanation begin with structural prose ("The following example...")?
  — Do the explanations cover intent, why, or edge cases?
  — Are there code blocks with no following explanation?

File 2: docs/csharp/linq/index.md or a LINQ sub-page
  Same questions. Note: LINQ documentation tends toward mechanical
  description — calibrate quality scorer expectations accordingly.

File 3: docs/csharp/fundamentals/exceptions/exception-handling.md
  Same questions. Note: exception handling documentation typically covers
  edge cases — calibrate edge_case_covered expectations accordingly.
```

Record findings in:
```
CODE-DOC-EXAMINATION-LOG-v1.json → entry for registry_id: dotnet-docs-mit
```

### sql-docs-mit — Three files to examine

```
File 1: docs/t-sql/language-elements/try-catch-transact-sql.md
  Questions to answer:
  — Do code blocks contain multiple SQL statements? If yes, are they
    setup queries followed by the main query, or all part of one example?
  — Does the explanation refer to all statements or only the main query?
  — What language tags appear? (sql, tsql, sql-server, other?)
  — Does explanation begin with structural prose ("The following example...")?
  — Word count of typical explanation paragraphs?

File 2: docs/t-sql/functions/ranking-functions-transact-sql.md
  Same questions. Window function documentation — check edge case coverage
  (PARTITION BY with empty partitions, NULLs in ORDER BY).

File 3: docs/relational-databases/indexes/clustered-and-nonclustered-indexes-described.md
  Same questions. Index documentation — check why coverage (why clustered
  vs non-clustered, when to add INCLUDE columns).
```

Record findings in:
```
CODE-DOC-EXAMINATION-LOG-v1.json → entry for registry_id: sql-docs-mit
```

### angular-adev-mit — Three files to examine

```
File 1: adev/src/content/guide/signals.md
  Questions to answer:
  — Do code blocks alternate between typescript and html? If yes, does the
    split-component rule apply or are typescript blocks standalone?
  — Are explanations self-contained for the typescript block alone?
  — Does the documentation cover signal reactivity edge cases (circular
    computation, effect cleanup)?
  — Word count of typical explanation paragraphs?

File 2: adev/src/content/guide/di/dependency-injection.md
  Same questions. DI documentation — check why coverage (why providers are
  scoped, why injection tokens are used instead of concrete classes).

File 3: adev/src/content/guide/http/making-requests.md
  Same questions. HTTP client documentation — check edge case coverage
  (error handling, request cancellation, interceptor ordering).
```

Record findings in:
```
CODE-DOC-EXAMINATION-LOG-v1.json → entry for registry_id: angular-adev-mit
```

---

## Failure Modes — Code Documentation Domain

These failure modes are specific to CODE_TO_DOC pair construction. They
supplement the failure modes in EXAMINATION-METHOD-ORCHESTRATION-v15.md.

```
CD-01: Structural prose used as explanation
  Definition: The explanation_raw consists of "The following example shows..."
  or a similar structural sentence with no actual explanation content.
  Detection: explanation_raw begins with one of the structural prose patterns
  listed in Step 2.
  Fix: Extend the extraction window to capture the paragraph after the
  structural sentence. If no paragraph follows, discard the candidate.

CD-02: Chosen side is mechanical at all three tiers
  Definition: The explanation_raw describes operations in order with no
  purpose-level framing. All three quality flags score false.
  Detection: automated scorer returns intent_covered: false.
  Fix: Discard from chosen pool. Mark as candidate-for-rejected-type-1.
  Do not attempt to improve the explanation — it came from the source
  as-is and must not be rewritten.

CD-03: Type 4 rejected identical to chosen minus one sentence
  Definition: The missing-edge-case rejected was produced by deleting
  one sentence, leaving the remaining text structurally identical to
  the chosen side. Overlap ratio exceeds 0.55.
  Detection: overlap_ratio > 0.55 for a Type 4 pair.
  Fix option 1: Strengthen the annotation to introduce 5+ non-overlapping
  consequence tokens.
  Fix option 2: Switch to Type 3 rejected for this candidate — the wrong-level
  approach will produce lower overlap for the same snippet.
  Fix option 3: Discard and find a different candidate from the same sub-domain.

CD-04: Name echo too long
  Definition: The identifier being echoed is long enough that splitting it
  produces a sentence of 15+ words, causing overlap_ratio > 0.40.
  Detection: Type 2 pair with overlap_ratio > 0.40.
  Fix: This identifier is too long for a clean name echo. Shorten to the
  primary noun component of the name.
  Example: SaveOrderConfirmationAsyncWithRetryOnTransientFailure →
  shorten rejected to "Saves order confirmation." (not the full split).

CD-05: SQL multi-statement block incorrectly split
  Definition: The SQL code block was split at a semicolon but the explanation
  refers to the full multi-statement example including the setup queries.
  Detection: explanation_raw refers to a table or variable only defined in
  the setup portion of the snippet, which was excluded by the filter.
  Fix: Revert to the full multi-statement block as the snippet. Do not
  split if the explanation references setup content.

CD-06: Angular template block paired without TypeScript class
  Definition: A TypeScript code block was extracted that contains only a
  @Component decorator with a templateUrl reference and no class body.
  The class body is in a separate file not captured by the extractor.
  Detection: snippet_raw contains @Component but no class keyword or
  method definitions.
  Fix: Apply the decorator-only filter to exclude. This snippet has no
  documentable logic outside the framework metadata.

CD-07: Explanation covers a different snippet than the one extracted
  Definition: The extraction heuristic captured the explanation paragraph
  from a neighbouring code block, not the intended one. The explanation
  refers to concepts not visible in the extracted snippet.
  Detection: Manual review — explanation references identifiers not present
  in snippet_raw.
  Fix: Extend or narrow the extraction window. If the correct explanation
  cannot be isolated, discard the candidate.

CD-08: External pair in internal batch
  Definition: A CODE_TO_DOC pair from an external source registry entry has
  been added to an internal teaching batch without source_boundary: external.
  Detection: pair row missing source_boundary field or field value is not
  "external".
  Fix: Separate into an external-only batch file. Never mix.
```

---

## Required Coverage Labels

Every pair row produced by this examination method must carry these fields.
A row missing any required field is rejected at the pre-training validation gate.

```
source_registry_id         — registry entry from CODE-DOC-SOURCE-REGISTRY-v1
source_boundary            — always "external" for this domain
source_canonicality        — "extracted_copy" for locally cloned repositories
canonical_confirmed_via    — registry entry + verified date
source_file_path           — path within the repository
source_commit              — commit SHA at extraction time
source_url                 — full GitHub URL with commit and line
domain                     — "code-documentation"
sub_domain                 — from the sub-domain taxonomy
pair_kind                  — "CODE_TO_DOC"
rejected_type              — mechanical-paraphrase | name-echo | wrong-level
                             | missing-edge-case
curriculum_tier            — 1 | 2 | 3
intent_covered             — boolean
why_covered                — boolean
edge_case_covered          — boolean
overlap_ratio              — float
annotation_quality         — float 0-1
annotation_strong          — boolean
compiled_prompt_in_training — false (all CODE_TO_DOC pairs in this domain)
pair_status                — valid | review-needed | invalid
quality_status             — train_ready | needs_review | rejected
```

`pair_kind` for this domain is always `CODE_TO_DOC`. This is a new pair kind
not present in the allowed list in EXAMINATION-METHOD-ORCHESTRATION-v15.md.
It must be added to the runner's allowed pair_kind list before Phase 4 begins.

---

## Acceptance Checklist

Run before adding any pair to a batch.

```
□ CANONICALITY GATE — stop if failed. Do not proceed to any other check.
  source_canonicality is extracted_copy (for locally cloned repositories).
  canonical_confirmed_via names the registry entry and verification date.
  source_boundary is "external".
  Failure here means the source is invalid regardless of pair quality.

□ source_commit is recorded — not null, not "HEAD"

□ Language tag matches domain:
  dotnet-csharp: [csharp, cs, c#]
  sql-tsql: [sql, tsql, sql-server, t-sql]
  angular-typescript: [typescript, ts]

□ snippet_lines is between 5 and 80 inclusive

□ explanation_words is between 40 and 600 inclusive

□ intent_covered is true

□ Rejected side is a complete, coherent response — not a negation phrase
  ("Do not...", "Skip...", "Not applicable..." all fail this check)

□ rejected_type is one of the four permitted types

□ [WRONG: ...] annotation is present and specific
  Annotation names: the weakness type, what the caller cannot learn,
  the consequence.
  Fails if: says only "this is wrong" or "not helpful" without naming
  what is missing.

□ overlap_ratio <= 0.60

□ If overlap_ratio > 0.45: annotation_strong is true

□ For Type 4 pairs with overlap_ratio > 0.50:
  at least 5 non-overlapping consequence tokens in annotation

□ compiled_prompt_in_training: false
  (runner configuration must accept false for this domain)

□ source_boundary: external
  (this pair is never mixed into an internal teaching batch)

□ pair_status: valid (not review-needed, not invalid)
```

---

## Session Ledger Format

Each accepted batch gets one entry in the session ledger.

File location:
```
C:\Users\lubao\Documents\xiigen\llm mvp\code-documentation\
  ledger\code-doc-teaching-ledger-{date}.jsonl
```

```json
{
  "batch_id": "string — e.g. code-doc-001-dotnet-async",
  "session_label": "code-documentation-v1",
  "registry_ids_used": ["dotnet-docs-mit"],
  "source_boundary": "external",
  "domain": "code-documentation",
  "sub_domains_covered": ["dotnet-csharp/async-await", "dotnet-csharp/linq"],
  "pair_count": "integer",
  "pairs_by_rejected_type": {
    "mechanical-paraphrase": "integer",
    "name-echo": "integer",
    "wrong-level": "integer",
    "missing-edge-case": "integer"
  },
  "pairs_by_curriculum_tier": {
    "1": "integer",
    "2": "integer",
    "3": "integer"
  },
  "pairs_by_domain": {
    "dotnet-csharp": "integer",
    "sql-tsql": "integer",
    "angular-typescript": "integer"
  },
  "overlap_scan": {
    "max_overlap_ratio": "float",
    "mean_overlap_ratio": "float",
    "pairs_above_0.50": "integer",
    "pairs_above_0.60": "integer"
  },
  "reward_margin_mean": "float",
  "loss_mean": "float",
  "accuracy": "float",
  "verdict": "ACCEPT_BATCH | REJECT_BATCH",
  "compiled_prompt_in_training": false,
  "annotation_strong_fraction": "float — fraction of pairs with annotation_strong: true",
  "notes": "string"
}
```

`compiled_prompt_in_training: false` must be recorded and must not trigger
a batch rejection in the runner. If the runner rejects batches with this value:
this is a configuration gap that must be resolved before any batches are run.
Record the resolution in the notes field of the first accepted batch entry.

---

## Last Updated

2026-05-10. Version 1.0.
