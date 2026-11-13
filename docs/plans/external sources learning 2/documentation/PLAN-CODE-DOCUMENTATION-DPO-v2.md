# Code-to-Documentation DPO Capability Domain — Plan v2

Date: 2026-05-10
Supersedes: PLAN-CODE-DOCUMENTATION-CAPABILITY-v1.md
Authority documents read: XIIGEN-DPO-INTERNAL-TEACHING-PLAN-v2.md,
DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v8.md,
EXAMINATION-METHOD-ORCHESTRATION-v15.md,
XIIGEN-INTERNAL-SOURCE-REGISTRY-v11.md,
ORCHESTRATOR-TEACHING-PLAN-v5.md

Operational runbook for future models:
`C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\TRAINING-SET-BUILD-AND-TESTING-RUNBOOK-v1.md`

---

## What changed from v1

v1 was written before the internal learning guidance was read. v1 had correct
intuitions about pair shape and rejected type taxonomy, but was missing the
precise mechanics the trainer actually needs: the token overlap test, the
compiled prompt requirement, the batch composition minimums, the reward margin
gate, and the DTO field set that the runner validates.

v2 is fully aligned with the internal guidance. Every gate and field name
matches the existing training infrastructure. Claude Code can execute this
plan directly against the existing runner without creating new training
mechanics.

---

## 2026-05-11 Run-Experience Addendum

This addendum is based on the live internal training runs V244 through V247
and the audit of `documentation/learning sets.zip`.

Current baseline context:

```
V246 balanced baseline:
  rows:                 672
  weighted accuracy:    0.7068
  reward margin:        0.016892
  domains:              orchestration 257, plans 341, skills 74

V244 highest-accuracy candidate:
  rows:                 656
  weighted accuracy:    0.707317
  reward margin:        0.016734

V247 standalone external PLAN/ARCH implementation-jump ablation:
  rows:                 16
  accuracy:             0.625
  reward margin:        0.010605
```

Operational lessons:

1. Do not train large generated libraries directly. Run a strict audit first,
   then train small standalone ablations.
2. A row where `chosen == rejected` is a hard reject even when
   `overlap_ratio` is below 0.60. It carries no preference signal.
3. Mojibake in any training field (`prompt`, `chosen`, `rejected`) is a hard
   reject until the encoding is repaired.
4. `compiled_prompt_in_training: false` is allowed for CODE_TO_DOC only when
   the row explicitly uses raw-snippet mode. It is not a failure by itself.
5. Code-documentation rows must not be mixed into orchestration / plan / skill
   baselines until they pass a standalone run and then a combo run.

Audit result for `learning sets.zip`:

```
files:                       251 JSONL files
training rows:               50,000
valid JSON:                  yes
unique pair IDs:             50,000
chosen == rejected:          4,660 rows
mojibake in training fields: 4,912 rows
strict train-safe candidates: 3,921 rows
registered-domain issue:     mongodb-csharp and elasticsearch-csharp require
                              registry entries before training
```

Therefore, the correct next product of this plan is not one monolithic
train-ready file. It is three separated outputs:

```
code-doc-clean-trainable.jsonl
  Rows passing all hard gates.

code-doc-repair-needed.jsonl
  Rows with chosen==rejected, mojibake, weak annotation, missing registry
  domain, or other repairable defects.

code-doc-audit-ledger.jsonl
  Batch/run metadata only. Never fed directly to DPO training.
```

---

## Domain Declaration

```
Domain name:         code-documentation
Domain kind:         new capability domain — not a sub-phase of the existing
                     orchestration routing capability
Sub-domains:
  dotnet-csharp      — C# patterns: async/await, LINQ, DI, generics,
                       extension methods, attributes, records, pattern matching,
                       exception handling, collections
  sql-tsql           — T-SQL: queries, CTEs, window functions, stored
                       procedures, indexes, transactions, dynamic SQL,
                       error handling
  angular-typescript — Components, services, RxJS, signals, forms,
                       routing, change detection, HTTP client
Pair kind:           CODE_TO_DOC
Session label:       code-documentation-v1
```

---

## The Quality Definition — Pinned Before Everything Else

This definition drives which source text qualifies as a chosen side and which
qualifies as a rejected side. It is the standard every pair is evaluated against
before any overlap test or batch composition check.

Clever documentation answers three questions a developer actually has when
encountering unfamiliar code:

**Intent** — What does this code accomplish at the purpose level, not the
mechanical level? Not "iterates the list and filters" but "finds the first
qualifying record, returns null when nothing matches." The word to watch for
in a good chosen side: verbs of purpose — ensures, prevents, finds, returns
when, guarantees, allows callers to.

**Why** — What design decision or trade-off is embedded here that is not obvious
from reading the code? Why async here. Why a dictionary over a list. Why this
SQL index structure. Why the interface instead of the concrete type. Words to
watch for: instead of, because, to avoid, rather than, preferred when, unlike.

**Edge case** — What non-obvious precondition, failure mode, or input condition
changes behavior in a way the caller must know about? Words to watch for:
unless, when empty, if null, throws when, returns null if, note that, be aware,
cancellation.

A chosen side covering all three is curriculum tier 1.
A chosen side covering intent and why is curriculum tier 2.
A chosen side covering intent only is curriculum tier 3.
A side covering none is rejected-side material, not chosen-side material.

---

## Hard Gates — Apply Before Any Pair Is Written

These come directly from the internal training guidance and apply to
CODE_TO_DOC pairs exactly as they apply to existing pair types.

| Gate | Rule | Reason |
|---|---|---|
| Token overlap test | `overlap_ratio > 0.60` — do not submit the pair | Calibration run showed overlap above 0.60 produces negative reward margin 100% of the time |
| Chosen/rejected identity | `chosen.trim() == rejected.trim()` — hard reject | V247 audit found 4,660 rows with no preference signal despite valid JSON |
| Encoding sanity | Any mojibake such as `Ã`, `Â`, `â€`, `Ã¢` in `prompt`, `chosen`, or `rejected` — repair before training | Corrupted tokens teach the model broken documentation strings |
| No generic rejected strings | Rejected must not open with "Do not generate", "Should not produce", or any negation template | Negation templates have overlap ~0.70 with chosen — guaranteed negative margin |
| Annotation required | Every rejected ends with `[WRONG: ...]` naming the specific documentation weakness | Annotation tokens widen distributional gap without inventing content |
| One prompt per batch | Same code snippet not used as chosen in one pair and context in another | Batch cross-contamination degrades accuracy |
| Prompt mode explicit | `compiled_prompt_in_training: false` is allowed only with raw-snippet CODE_TO_DOC mode; otherwise compiler output must be present | Prevents silently treating an uncompiled orchestration prompt like a code snippet |
| Registered domain | Every `domain` must exist in the registry before training | `mongodb-csharp` and `elasticsearch-csharp` appeared in generated sets before registry coverage |
| Standalone first | New generated sets run as 16-64 row standalone ablations before any combo run | V235 showed large continuation batches can damage accuracy and margin |

---

## The Token Overlap Test — Applies Before Every Pair Submission

```
shared = tokens in both chosen and rejected that are NOT also in the prompt
total  = tokens in chosen + tokens in rejected − tokens in prompt
overlap_ratio = shared / total
```

`overlap_ratio > 0.60` — do not submit. Revise rejected or discard the pair.

Expected ratios by rejected type for CODE_TO_DOC pairs:

| Rejected type | Expected overlap | Expected margin |
|---|---|---|
| Type 1 — mechanical paraphrase | 0.30–0.45 | Positive with strong annotation |
| Type 2 — name echo | 0.05–0.20 | Reliably positive |
| Type 3 — wrong level | 0.15–0.35 | Reliably positive |
| Type 4 — missing edge case | 0.40–0.55 | Positive only with strong annotation |
| Generic negation template | 0.65–0.80 | Reliably negative — forbidden |

Type 4 is the most valuable for training and the hardest to keep below 0.60.
When a Type 4 pair exceeds 0.55, the annotation must introduce at least five
non-overlapping tokens that name why the missing edge case matters functionally.

---

## Plan Structure

Four phases. Each phase has sub-phases. Each phase has an exit gate. No phase
begins until the previous phase's exit gate is met.

```
Phase 1 — Source Registry and Licence Verification
  1a — Registry schema
  1b — Tier 1 repository catalogue and licence check
  1c — Tier 2 repository catalogue

Phase 2 — Repository Examination and Source Row Harvesting
  2a — Per-repository examination protocol (manual, before any code)
  2b — Harvester implementation
  2c — Source row extraction and filtering
  2d — Sub-domain tagging

Phase 3 — Pair Builder
  3a — Chosen side scoring (three quality flags)
  3b — Rejected side authoring (four types with worked examples)
  3c — Overlap ratio check and annotation quality gate
  3d — Pair DTO assembly (full field set)

Phase 4 — Validation Gate and Runner Integration
  4a — Pre-training validation gates (ten gates)
  4b — Batch composition requirements
  4c — Measurement gate (reward margin and loss)
  4d — Session ledger
```

---

## Phase 1 — Source Registry and Licence Verification

### What this phase produces

A populated source registry JSON file. A licence verification record for each
Tier 1 repository. Nothing is harvested in Phase 1.

### 1a — Registry Schema

File location:
```
C:\Users\lubao\Documents\xiigen\llm mvp\code-documentation\
  source-registry\CODE-DOC-SOURCE-REGISTRY-v1.json
```

Each entry:

```json
{
  "registry_id": "string — e.g. dotnet-docs-mit",
  "repository_url": "string — full GitHub URL",
  "license": "MIT | Apache-2.0",
  "license_file_path": "string — path to LICENSE within repo",
  "notice_file_path": "string or null — Apache-2.0 repos only",
  "license_verified_date": "string — ISO date",
  "domain": "dotnet-csharp | sql-tsql | angular-typescript",
  "tier": "1 | 2",
  "content_type": "official-docs | project-docs",
  "estimated_pair_volume": "integer",
  "harvest_status": "pending | in-progress | complete | paused",
  "last_harvested_commit": "string — commit SHA",
  "source_canonicality": "canonical",
  "notes": "string"
}
```

`source_canonicality` is set to `canonical` for all Tier 1 and Tier 2 sources.
These are primary repositories, not mirrors or copies. If a repository turns out
to be a fork rather than the authoritative source, update to `extracted_copy`
before harvesting begins.

### 1b — Tier 1 Repository Catalogue and Licence Check

These three repositories are the primary harvest targets. All are official
documentation repositories maintained by Microsoft or the Angular team. All use
MIT licence for both code and documentation content.

| Registry ID | Repository | Licence | Domain |
|---|---|---|---|
| `dotnet-docs-mit` | `github.com/dotnet/docs` | MIT | dotnet-csharp |
| `sql-docs-mit` | `github.com/MicrosoftDocs/sql-docs` | MIT | sql-tsql |
| `angular-adev-mit` | `github.com/angular/angular` path `adev/src/content` | MIT | angular-typescript |

Licence verification checklist — run once per repository before any harvest:

```
□ Locate the LICENSE file at the repository root
□ Confirm the licence text matches MIT verbatim
□ Check whether the documentation directory has its own separate licence
  — If yes, record it; if not MIT or Apache-2.0, exclude that directory
□ Confirm there is no NOTICE file requiring special attribution handling
□ Record verified_date as today and commit SHA at time of verification
□ Set harvest_status to pending — not in-progress until Phase 2 begins
```

Attribution requirement: MIT licence requires preserving the copyright notice.
Record the copyright line from the LICENSE file in the registry entry `notes`
field. This ensures every source row can demonstrate attribution if audited.

### 1c — Tier 2 Repository Catalogue

Tier 2 repositories are harvested after Tier 1 is complete and the pipeline
is validated. These are high-quality open source projects with permissive
licences and well-documented code examples.

| Registry ID | Repository | Licence | Domain | Why included |
|---|---|---|---|---|
| `automapper-mit` | `github.com/AutoMapper/AutoMapper` | MIT | dotnet-csharp | Object mapping patterns, well-documented API examples |
| `mediatr-mit` | `github.com/jbogard/MediatR` | MIT | dotnet-csharp | CQRS patterns with intent explanations |
| `fluentvalidation-apache` | `github.com/FluentValidation/FluentValidation` | Apache-2.0 | dotnet-csharp | Rule chaining, extension method documentation |
| `dapper-apache` | `github.com/DapperLib/Dapper` | Apache-2.0 | sql-tsql | SQL-centric, parameterized query documentation |
| `ngrx-mit` | `github.com/ngrx/platform` | MIT | angular-typescript | State management, effects, selectors |
| `angular-material-mit` | `github.com/angular/components` | MIT | angular-typescript | Component API docs with usage examples |

Apache-2.0 note: Apache-2.0 repositories may have a NOTICE file. If present,
the NOTICE file path must be recorded and its content preserved in the
`notes` field of the registry entry. Attribution is required.

### Phase 1 Exit Gate

- Registry schema file exists at the canonical path
- All three Tier 1 repositories have registry entries with `license_verified_date` populated
- All six Tier 2 repositories have registry entries with `harvest_status: pending`
- No repository moves to `harvest_status: in-progress` without a verified licence entry

---

## Phase 2 — Repository Examination and Source Row Harvesting

### What this phase produces

A source row file per domain. Source rows are candidates — not yet pairs.
Each row is a code snippet plus explanation prose, tagged and filtered.

### 2a — Per-Repository Examination Protocol

Before writing any harvester code, examine each Tier 1 repository manually.
This determines what the code-explanation pair structure actually looks like
in each source, so the harvester is built for the real structure rather than
an assumed one.

Run this protocol once per repository. Record findings in the examination log.

**Examination steps:**

```
1. Clone or browse the repository on GitHub
2. Navigate to the primary documentation directory:
   — dotnet/docs: docs/csharp/
   — sql-docs: docs/t-sql/ and docs/relational-databases/
   — angular/adev/src/content: guide/ and tutorials/
3. Open three representative markdown files in different topic areas
4. For each file, answer and record:
   a. Does it contain triple-backtick code blocks?
   b. Is each code block followed by prose explaining it?
      Or is the explanation before the code block?
      Or is it mixed?
   c. What is the typical code block length in lines? (count three examples)
   d. What is the typical explanation paragraph length in words? (count three)
   e. How many code-explanation pairs appear in one file? (count one full file)
   f. Are there code blocks that have no explanation? What percentage?
   g. Are there code blocks that are syntax-only (no executable logic)?
5. Derive the extraction heuristic for this repository from the answers
6. Record findings in the examination log file
```

**Examination log schema:**

```json
{
  "registry_id": "string",
  "examined_date": "string — ISO date",
  "examined_by": "string",
  "sample_files": ["path1", "path2", "path3"],
  "code_block_position": "before-explanation | after-explanation | mixed",
  "typical_snippet_lines_min": "integer",
  "typical_snippet_lines_max": "integer",
  "typical_explanation_words_min": "integer",
  "typical_explanation_words_max": "integer",
  "pairs_per_file_min": "integer",
  "pairs_per_file_max": "integer",
  "code_blocks_without_explanation_pct": "float 0-1",
  "syntax_only_blocks_pct": "float 0-1",
  "extraction_heuristic": "string — the pattern derived from the above",
  "exclusion_directories": ["list of paths to skip"],
  "notes": "string"
}
```

Examination log location:
```
C:\Users\lubao\Documents\xiigen\llm mvp\code-documentation\
  source-registry\CODE-DOC-EXAMINATION-LOG-v1.json
```

### 2b — Harvester Implementation

The harvester is a Python script. Python is used here for speed of iteration
on the data pipeline — the rest of the project is C# but this is a one-time
extraction tool, not production infrastructure.

**Harvester inputs:**
- The source registry file from Phase 1
- The examination log from 2a
- A root directory containing cloned Tier 1 repositories

**Harvester outputs:**
- One source row JSONL file per domain
- A harvest summary JSON file

**Extraction heuristic (general, refined per-repository from examination log):**

A candidate pair is a code fence (triple-backtick block with a language tag)
that is followed within 500 characters by a prose paragraph of 40 or more words
that does not itself contain a code fence.

The 500-character window and 40-word minimum are starting values. Adjust them
per-repository based on the examination log findings before running the
harvester on the full corpus.

**Harvester implementation location:**
```
C:\Users\lubao\Documents\xiigen\llm mvp\code-documentation\
  harvester\harvester.py
```

### 2c — Source Row Extraction and Filtering

Each extracted candidate becomes a source row with this shape:

```json
{
  "source_row_id": "string — uuid",
  "registry_id": "string",
  "source_file_path": "string — path within repository",
  "source_commit": "string — commit SHA",
  "source_url": "string — direct GitHub URL to file and line",
  "domain": "dotnet-csharp | sql-tsql | angular-typescript",
  "sub_domain": "string — see sub-domain list",
  "language_tag": "string — the code fence tag, e.g. csharp | sql | typescript",
  "snippet_raw": "string — code content inside the fence",
  "snippet_lines": "integer",
  "explanation_raw": "string — prose paragraph following the code",
  "explanation_words": "integer",
  "extraction_heuristic": "string — which heuristic rule matched",
  "harvest_timestamp": "string — ISO datetime",
  "source_canonicality": "canonical",
  "canonical_confirmed_via": "string — registry_id + verified date",
  "pair_status": "candidate | filtered-out | promoted-to-pair",
  "filter_reason": "string — populated when pair_status is filtered-out"
}
```

**Size filters — a row failing any filter gets `pair_status: filtered-out`:**

```
Minimum snippet lines:   5   (fewer than 5 lines has no separable intent)
Maximum snippet lines:   80  (larger than 80 needs section-level documentation)
Minimum explanation words: 40  (fewer cannot cover even one quality dimension)
Maximum explanation words: 600 (more is usually section context, not code-specific)
```

**Quality filters:**

```
Language tag must match domain:
  dotnet-csharp: [csharp, cs, c#]
  sql-tsql: [sql, tsql, sql-server, t-sql]
  angular-typescript: [typescript, ts, angular]

Explanation must not be a section heading:
  Reject if explanation_raw starts with ## or ###

Explanation must not be another code block:
  Reject if first 40 words contain more than 20 characters of
  code-like content (backtick sequences, lines of semicolons or braces)

Snippet must not be an import block:
  Reject if 80% or more of lines start with using, import, #include, or are blank
```

**Sub-domain list — used in 2d tagging:**

For `dotnet-csharp`:
- `async-await` — async, await, Task, ValueTask, CancellationToken
- `dependency-injection` — constructor injection, IServiceCollection, lifetimes
- `linq` — IEnumerable, Select, Where, FirstOrDefault, aggregation
- `generics` — generic types, type constraints, covariance
- `extension-methods` — static extension classes, fluent APIs
- `attributes-reflection` — custom attributes, Type, GetCustomAttribute
- `exception-handling` — try/catch, custom exceptions, finally
- `collections` — Dictionary, List, Span, Memory, IReadOnlyCollection
- `records-structs` — record, init, with-expression, value semantics
- `pattern-matching` — switch expression, is pattern, positional pattern

For `sql-tsql`:
- `select-queries` — SELECT, JOIN, subqueries, aliases
- `window-functions` — ROW_NUMBER, RANK, LAG, LEAD, OVER, PARTITION BY
- `ctes` — WITH, recursive CTE, multi-CTE
- `stored-procedures` — CREATE PROCEDURE, parameters, OUTPUT, RETURN
- `indexes` — clustered, non-clustered, covering, INCLUDE columns
- `transactions` — BEGIN TRAN, COMMIT, ROLLBACK, isolation levels
- `dynamic-sql` — EXEC sp_executesql, parameterized dynamic queries
- `error-handling` — TRY/CATCH, THROW, RAISERROR, XACT_ABORT

For `angular-typescript`:
- `components` — @Input, @Output, lifecycle hooks, ViewChild
- `services-di` — providedIn, InjectionToken, hierarchical injectors
- `rxjs-observables` — Observable, Subject, pipe, operators, takeUntil
- `signals` — signal(), computed(), effect(), toSignal()
- `forms` — FormGroup, FormControl, Validators, async validators
- `routing` — ActivatedRoute, Router, guards, resolvers, lazy loading
- `change-detection` — ChangeDetectionStrategy.OnPush, markForCheck, async pipe
- `http-client` — HttpClient, interceptors, catchError, retry

### 2d — Sub-Domain Tagging

After filtering, tag each surviving row with its most specific sub-domain using
keyword matching on the snippet content.

```
For dotnet-csharp rows:
  Contains await or async keyword → async-await
  Contains IEnumerable, .Select(, .Where( → linq
  Contains try { or catch ( → exception-handling
  Contains Dictionary< or new Dictionary → collections
  Contains record or init; → records-structs
  Contains switch ( and => → pattern-matching
  None of the above → general-dotnet-csharp (flag for manual review)

For sql-tsql rows:
  Contains ROW_NUMBER( or RANK( or LAG( or LEAD( → window-functions
  Contains WITH and AS ( on a line by itself → ctes
  Contains CREATE PROCEDURE or ALTER PROCEDURE → stored-procedures
  Contains BEGIN TRAN or BEGIN TRANSACTION → transactions
  Contains EXEC sp_executesql → dynamic-sql
  Contains TRY and CATCH → error-handling
  None of the above → select-queries if SELECT present, else general-sql

For angular-typescript rows:
  Contains Observable or Subject → rxjs-observables
  Contains signal( or computed( or effect( → signals
  Contains FormGroup or FormControl → forms
  Contains ActivatedRoute or Router → routing
  Contains ChangeDetectionStrategy → change-detection
  Contains HttpClient or HttpHeaders → http-client
  None of the above → general-angular
```

For rows matching multiple sub-domains, assign the most specific one.
For rows with no match, assign `general-{domain}` and flag for manual review.

### Phase 2 Exit Gate

- Source row JSONL file exists for each Tier 1 domain
- Harvest summary exists showing: total files examined, candidate pairs found,
  filtered-out count with reason breakdown, surviving rows per domain
- Every surviving row has a sub-domain tag
- No row with `pair_status: filtered-out` appears in the surviving set

---

## Phase 3 — Pair Builder

### What this phase produces

Validated pair DTO records. Each pair has an explicit chosen side, explicit
rejected side, rejected type, quality flags, overlap ratio, and the full
field set the runner validates. These are the actual training records.

### 3a — Chosen Side Scoring

Not every source row produces a good chosen side. The `explanation_raw` field
from the source row is the chosen candidate. Score it against the three quality
dimensions before accepting it.

**Scoring — answer three yes/no questions:**

```
intent_covered:
  Does the explanation state what the code accomplishes at the level of
  purpose or outcome, not just what operations it performs?

  YES signals: "ensures", "finds", "prevents", "returns when",
  "allows callers to", "guarantees", "provides" — framing purpose.

  NO signals: line-by-line description without purpose framing.

why_covered:
  Does the explanation mention a design reason or trade-off?

  YES signals: "instead of", "because", "to avoid", "rather than",
  "preferred when", "unlike" — framing a choice.

  NO signals: pure description with no comparison or reason given.

edge_case_covered:
  Does the explanation mention a non-obvious condition, failure mode,
  or behavior under specific inputs?

  YES signals: "unless", "when empty", "if null", "throws when",
  "returns null if", "note that", "be aware", "cancellation" — caveats.

  NO signals: only the happy path described.
```

**Scoring outcomes:**

```
intent + why + edge case → curriculum_tier: 1 — full quality bar met
intent + why             → curriculum_tier: 2 — edge case missing
intent only              → curriculum_tier: 3 — minimum acceptable chosen
None YES                 → discard from chosen pool
                         → explanation may be usable as rejected Type 1
```

For automated scoring at scale: use a lightweight LLM call that returns the
three boolean answers as a JSON object. Prompt the model to answer each of the
three questions strictly yes or no based on the explanation text. Any row where
the automated scorer returns `intent_covered: false` is discarded. Rows where
the scorer is uncertain (returns neither clear yes nor no) go to a manual
review queue.

### 3b — Rejected Side Authoring — Four Types with Worked Examples

**Type 1 — Mechanical Paraphrase**

Describes what the code does line by line. Sounds like documentation. Teaches
nothing about intent, why, or edge cases.

How to produce: narrate each operation in the code in order. Use no purpose
verbs. Include no reason for the approach. Include no edge cases.

Example — C# LINQ method finding the cheapest in-stock product:

```
Chosen (tier 1 — all three quality flags):
Returns the lowest-priced product currently in stock for the given category,
or null when no inventory exists. Using FirstOrDefault rather than First
avoids an exception on empty results — callers must check for null before
displaying a price. The OrderBy runs before FirstOrDefault so only one
pass through the sorted results is needed.

Rejected (Type 1 — mechanical):
Filters the products list where InStock is true, orders the results by
Price ascending, and returns the first element or null.

[WRONG: "mechanical paraphrase" lists the operations the code performs
without explaining what the code is for, why this ordering was chosen,
or what callers must do with a null result.]
```

Expected overlap ratio: 0.30–0.45. Annotation adds tokens about caller
behavior and design intent not present in the rejected.

**Type 2 — Name Echo**

Restates the method, property, or class name rephrased as a sentence. Zero
added value. Entirely realistic — many codebases contain exactly this.

How to produce: split the identifier on PascalCase or camelCase boundaries,
rearrange into a sentence.

Example — SQL stored procedure `GetPaginatedOrdersByTenant`:

```
Chosen (tier 2 — intent and why):
Returns one page of orders for the specified tenant, ordered by creation
date descending. The page_size and page_number parameters control which
slice is returned — the caller is responsible for passing valid values
because the procedure does not validate that page_number is within the
total available range.

Rejected (Type 2 — name echo):
Gets paginated orders by tenant.

[WRONG: "name echo" repeats the procedure name rephrased as a sentence.
The caller learns nothing about what inputs are required, what the result
set columns are, or what happens when page_number exceeds the available pages.]
```

Expected overlap ratio: 0.05–0.20. Name echo is short and uses different
token distribution. Very strong margin without heavy annotation.

**Type 3 — Wrong Level**

Documents internal implementation details that callers do not need, while
missing the contract the caller actually needs to know.

How to produce: describe the internal mechanics — the temp table, the loop
variable, the local helper — while saying nothing about input requirements,
output shape, or behavioral guarantees.

Example — Angular service managing authentication state:

```
Chosen (tier 1 — all three flags):
Provides the current user's authentication state as an Observable that
emits immediately on subscription and updates whenever the state changes.
Components should use the async pipe rather than subscribing directly —
the service manages the subscription lifecycle internally. Returns null
when no user is authenticated; never completes or errors.

Rejected (Type 3 — wrong level):
Maintains an internal BehaviorSubject initialized with null. The getUser$
getter exposes it as a read-only Observable via asObservable(). When
updateUser is called, it calls next() on the subject with the new value.

[WRONG: "wrong level" describes internal BehaviorSubject mechanics that
callers never need to know about. The caller needs to know what to subscribe
to, when it emits, what null means, and whether to manage the subscription
manually. Internal implementation details answer none of these questions.]
```

Expected overlap ratio: 0.15–0.35. Internal mechanics vocabulary differs
significantly from contract vocabulary.

**Type 4 — Missing Edge Case**

The documentation is mostly correct at the intent level but omits the one
non-obvious failure mode or precondition that changes behavior. The hardest
type to produce correctly. The most valuable for training.

How to produce: write a correct intent-level explanation, then deliberately
omit the edge case that the chosen side covers. Everything else should be
accurate. The weakness is the gap, not an error.

Example — C# async method with CancellationToken:

```
Chosen (tier 1 — all three flags):
Sends the order confirmation email asynchronously using the configured
provider. Pass a CancellationToken linked to the request lifetime so the
operation cancels cleanly when the caller times out or disconnects — without
a token, the email send continues even after the HTTP response has been
sent, consuming resources with no way to cancel.

Rejected (Type 4 — missing edge case):
Sends the order confirmation email asynchronously using the configured
provider. Returns a Task that completes when the send operation finishes.

[WRONG: "missing edge case" correctly describes what the method does
and its async nature but omits what happens without a CancellationToken.
Without the token, a timed-out or disconnected request leaves the email
send operation running with no way to cancel it — a resource leak the
caller must understand to avoid.]
```

Expected overlap ratio: 0.40–0.55. Most text is the same — the gap is the
missing information. Annotation carries significant signal weight here. When
overlap exceeds 0.50, the annotation must name the specific failure consequence
(resource leak, incorrect behaviour, silent error) using at least five tokens
not present in the rejected text.

**Annotation quality standard for all four types:**

Weak (does not improve margin):
`[WRONG: this is not a good documentation approach.]`

Strong (names the specific failure):
`[WRONG: "name echo" restates the method name as a sentence. The caller
learns nothing about the required inputs, the output shape, or what the
method returns on failure. Real documentation answers the question a caller
has when they encounter an unfamiliar method — this does not.]`

The strong annotation names what the rejected side fails to answer. These
tokens appear in the chosen side's semantic neighbourhood but not in the
rejected text, widening the distributional gap.

### 3c — Overlap Ratio Check and Annotation Quality Gate

After authoring the chosen and rejected sides, apply these checks in order:

```
Step 1 — Compute overlap_ratio using the formula:
  shared = tokens in both chosen and rejected NOT in the prompt (snippet)
  total  = tokens in chosen + tokens in rejected − tokens in prompt
  overlap_ratio = shared / total

Step 2 — Check thresholds:
  overlap_ratio > 0.60 → discard pair entirely
  overlap_ratio 0.50–0.60 → annotation must introduce ≥ 5 non-overlapping
    tokens naming the functional consequence of the weakness
  overlap_ratio < 0.50 → proceed to DTO assembly

Step 3 — Annotation quality check (for all pairs):
  Annotation is acceptable if it:
    — names the specific weakness type (mechanical, name echo, wrong level,
      missing edge case)
    — states what the caller or reader cannot learn from the rejected side
    — names the consequence (null not handled, resource leak, wrong caller
      behavior, unknown failure mode)

  Annotation is rejected if it:
    — says only "this is wrong" or "not helpful"
    — names the weakness type but does not explain why it matters
    — is absent

Step 4 — For Type 4 pairs with overlap_ratio > 0.50:
  Verify the annotation introduces the functional consequence tokens.
  Count the tokens in the annotation that do not appear in the rejected text.
  If count < 5 → annotation is insufficient → revise annotation or discard.

Step 5 — Identity and encoding:
  chosen.trim() == rejected.trim() → discard or repair. No exceptions.
  prompt/chosen/rejected contains mojibake (Ã, Â, â€, Ã¢) → repair before
  training. Do not let corrupted text into train-ready.
```

### 3d — Pair DTO Assembly

The full pair DTO. These fields are validated by the pre-training gate in
Phase 4 before any training begins.

```json
{
  "pair_id": "string — uuid",
  "source_row_id": "string — links to source row",
  "registry_id": "string — which source repository",
  "source_file_path": "string",
  "source_commit": "string — commit SHA",
  "source_url": "string — direct link to file",
  "source_canonicality": "canonical",
  "canonical_confirmed_via": "string",
  "capability_domain": "code-documentation",
  "domain": "dotnet-csharp | sql-tsql | angular-typescript",
  "sub_domain": "string — e.g. dotnet-csharp/async-await",
  "pair_kind": "CODE_TO_DOC",
  "curriculum_tier": "1 | 2 | 3",
  "prompt": "string — the code snippet formatted for the model",
  "prompt_mode": "raw_snippet | compiled_prompt",
  "compiled_prompt_in_training": "boolean — false is valid only for raw_snippet CODE_TO_DOC",
  "chosen": "string — the clever documentation",
  "rejected": "string — the weak documentation",
  "rejected_type": "mechanical-paraphrase | name-echo | wrong-level | missing-edge-case",
  "intent_covered": "boolean",
  "why_covered": "boolean",
  "edge_case_covered": "boolean",
  "overlap_ratio": "float",
  "annotation_quality": "float 0-1 — see rubric below",
  "annotation_strong": "boolean — see threshold below",
  "chosen_equals_rejected": "boolean",
  "mojibake_in_prompt": "boolean",
  "mojibake_in_chosen": "boolean",
  "mojibake_in_rejected": "boolean",
  "authored_by": "harvested | semi-automatic | manual",
  "authored_date": "string — ISO date",
  "pair_status": "valid | review-needed | invalid",
  "quality_status": "train_ready | needs_review | rejected"
}
```

**Annotation quality rubric:**

```
+0.30 — intent_covered is true
+0.25 — why_covered is true
+0.25 — edge_case_covered is true
+0.10 — overlap_ratio < 0.30 (strong distributional contrast)
+0.10 — rejected_type is missing-edge-case or wrong-level
─────
Max: 1.00

annotation_strong = true when:
  annotation_quality >= 0.55 AND annotation meets the quality standard
  (names weakness, names what caller cannot learn, names consequence)

annotation_strong = false when:
  annotation_quality < 0.55 OR annotation is generic

Pairs where annotation_strong is false and overlap_ratio > 0.45 are
invalid — the distributional gap is too small for the annotation to carry
without a strong annotation present.
```

### Phase 3 Exit Gate

- Pair DTO JSONL file exists per domain
- Every pair has `pair_status` set
- No pair with `pair_status: invalid` is in the training-ready set
- Pair summary exists showing: total pairs, breakdown by rejected type,
  breakdown by curriculum tier, breakdown by annotation quality band
- At least one pair per sub-domain in each domain
- Mean overlap_ratio across all pairs is below 0.40

---

## Phase 4 — Validation Gate and Runner Integration

### What this phase produces

A pre-training validation gate that rejects malformed pairs before the trainer
sees them. Batch composition requirements. A measurement gate. A session ledger.

### 4a — Pre-Training Validation Gates

Fourteen gates. Run in order. Any hard failure stops the run with a clear
error message. Informational gates may route rows to the repair queue, but they
must not silently pass into train-ready.

```
Gate 1 — File format
  Every record has both `chosen` and `rejected` fields.
  Error: "Input is not a pair DTO file — missing chosen or rejected fields."

Gate 2 — Pair completeness
  Every record has: pair_id, source_row_id, domain, sub_domain, pair_kind,
  prompt, chosen, rejected, rejected_type, curriculum_tier, intent_covered,
  overlap_ratio, annotation_quality, annotation_strong, pair_status,
  source_canonicality, source_commit.
  Error: "Pair {pair_id} is missing required fields: {field_list}."

Gate 3 — Pair kind
  pair_kind must be CODE_TO_DOC for all records in this file.
  Error: "Mixed pair kinds — separate CODE_TO_DOC pairs before running."

Gate 4 — Rejected type validity
  rejected_type must be one of the four defined types.
  Error: "Pair {pair_id} has unrecognised rejected_type: {value}."

Gate 5 — No generic negation strings
  The rejected field must not open with "Do not generate", "Should not
  produce", "Not applicable", or any negation template phrase.
  Error: "Pair {pair_id} uses a forbidden generic negation string."

Gate 6 — Overlap ratio
  overlap_ratio must be <= 0.60 for every pair.
  Pairs with overlap_ratio 0.50–0.60 must have annotation_strong: true.
  Error: "Pair {pair_id} has overlap_ratio {value} — discard or revise."

Gate 7 — Intent coverage
  intent_covered must be true for every pair in the training-ready set.
  Error: "Pair {pair_id} has intent_covered: false — invalid as chosen side."

Gate 8 — Annotation strength for high-overlap pairs
  If overlap_ratio > 0.45 and annotation_strong is false → invalid.
  Error: "Pair {pair_id}: high overlap requires strong annotation."

Gate 9 — Source provenance
  Every pair must have source_file_path, source_commit, source_url, and
  source_canonicality populated.
  Error: "Pair {pair_id} is missing source provenance."

Gate 10 — Chosen/rejected identity
  chosen.trim() must not equal rejected.trim().
  Error: "Pair {pair_id} has identical chosen and rejected text — no DPO signal."

Gate 11 — Encoding sanity
  prompt, chosen, and rejected must not contain mojibake markers:
  `Ã`, `Â`, `â€`, `Ã¢`.
  Error: "Pair {pair_id} contains mojibake in {field}; repair encoding first."

Gate 12 — Registered domain
  domain must be declared in CODE-DOC-EXTERNAL-SOURCE-REGISTRY-v1.md before
  the pair enters train-ready.
  Error: "Pair {pair_id} uses unregistered domain {domain}."

Gate 13 — Prompt mode
  compiled_prompt_in_training may be false only when pair_kind is CODE_TO_DOC
  and prompt_mode is raw_snippet or the registry explicitly states that raw
  code snippets are the A-side for this domain.
  Error: "Pair {pair_id} has uncompiled prompt without raw-snippet authority."

Gate 14 — Status filter (informational — does not stop the run)
  Only pairs with pair_status: valid enter training.
  Pairs with pair_status: review-needed go to the review queue.
  Pairs with pair_status: invalid are logged and skipped.
  Log: "Excluded {n} pairs: {n_invalid} invalid, {n_review} review-needed."
```

### 4b — Batch Composition Requirements

Each training batch must meet these composition minimums. These follow the
same structure as the existing batch composition rules in the DPO pair authoring
guidance.

| Pair type | Minimum fraction | Notes |
|---|---|---|
| Rejected type 2 (name echo) | 20% | Lowest overlap, most reliable margin. Include in every batch. |
| Rejected type 3 (wrong level) | 25% | Low to moderate overlap. Strong signal. |
| Rejected type 1 (mechanical paraphrase) | 30% | Moderate overlap. Annotation carries weight. |
| Rejected type 4 (missing edge case) | 15% | Highest overlap. Annotation is critical. |
| Curriculum tier 1 pairs | 20% minimum | At least one-fifth of pairs meet all three quality flags |
| Multi-sub-domain variety | At least 3 different sub-domains per batch | Prevents over-fitting to one topic area |

No prompt (code snippet) may appear as the basis for more than one pair in the
same batch. If duplicates exist after assembly, keep the pair with the lowest
overlap_ratio and discard the others.

### 4c — Measurement Gate

Before committing any batch to the ledger, verify the metrics below. The
measurement unit is the candidate chunk, not the entire generated library.

Run order:

```
1. Standalone ablation: 16-64 rows from one behavior family.
2. If standalone reward_margin_mean > 0 and accuracy >= 0.60, run a combo
   with the current baseline.
3. Accept into baseline only if the combo improves reward margin or improves
   the target domain without damaging the overall baseline.
4. If combo hurts the baseline, keep the chunk as candidate or repair-needed.
```

**Metric 1 — Reward margin:**

```
reward_margin_mean = mean(chosen_reward − rejected_reward) across all pairs in the batch
```

Rule: `reward_margin_mean < 0` — reject batch. Do not commit.

Run the diagnosis table before resubmitting.

For code-documentation, a positive but weak margin is not enough for broad
inclusion. Treat `0 < reward_margin_mean < 0.010` as candidate-only unless the
batch fixes a known held-out failure.

**Metric 2 — Loss versus random-guess floor:**

```
loss_mean across batch
```

Rule: `loss_mean ≈ 0.693` (the natural log of 2, which is the random-guess
level) — no learning is occurring. Even if reward margin is technically
positive, a batch at this loss level indicates pairs are too weakly separated.
Run the overlap scan on all pairs before resubmitting.

**Overlap scan (run when either metric fails):**

```
For each pair in the batch:
  if overlap_ratio > 0.50: flag as suspect
  if overlap_ratio > 0.60: mark as discard
Report flagged pairs with their overlap ratios.
```

**Diagnosis table for CODE_TO_DOC batches:**

| Symptom | Likely cause | Fix |
|---|---|---|
| All pairs negative margin | Generic negation strings in rejected | Replace all rejected with one of the four typed approaches |
| Some pairs have exactly zero signal | chosen and rejected are identical | Hard reject; rebuild rejected side from a different failure mode |
| Unexpected tokens or unreadable punctuation | Mojibake in prompt/chosen/rejected | Repair encoding and re-run validation before training |
| Type 4 pairs all negative | Missing edge case pairs have high overlap with weak annotation | Extend annotation to name functional consequence, or convert to Type 3 |
| Type 1 pairs borderline | Mechanical paraphrase shares too much vocabulary with chosen | Increase annotation strength or mix in more Type 2 and 3 |
| Loss stays at 0.693 | Prompt mode mismatch or weak contrast | Verify raw-snippet authority for CODE_TO_DOC, then inspect chosen/rejected contrast |
| All pairs near-zero margin | Chosen and rejected too similar in vocabulary | Check sub-domain — some topics have less vocabulary divergence; add more Type 2 pairs |
| Curriculum tier 1 pairs all negative | Full three-flag chosen is too close to rejected in some sub-domains | Verify overlap_ratio for tier 1 pairs specifically; may need stronger Type 4 annotation |

### 4d — Session Ledger

Each accepted batch gets one ledger entry. This follows the same format as
the existing internal DPO teaching ledger.

File location:
```
C:\Users\lubao\Documents\xiigen\llm mvp\code-documentation\
  ledger\code-doc-teaching-ledger-{date}.jsonl
```

Required fields per entry:

```json
{
  "batch_id": "string — e.g. code-doc-001-dotnet-async",
  "session_label": "code-documentation-v1",
  "registry_ids_used": ["list of registry_ids in this batch"],
  "domain": "code-documentation",
  "sub_domains_covered": ["list of sub_domains in this batch"],
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
  "overlap_scan": {
    "max_overlap_ratio": "float",
    "pairs_above_0.50": "integer",
    "pairs_above_0.60": "integer"
  },
  "reward_margin_mean": "float",
  "loss_mean": "float",
  "accuracy": "float",
  "verdict": "ACCEPT_BATCH | REJECT_BATCH",
  "compiled_prompt_in_training": "boolean",
  "prompt_mode": "raw_snippet | compiled_prompt",
  "encoding_gate": {
    "mojibake_in_prompt": "boolean",
    "mojibake_in_chosen": "boolean",
    "mojibake_in_rejected": "boolean"
  },
  "identity_gate": {
    "chosen_equals_rejected": "boolean"
  },
  "annotation_quality_mean": "float",
  "notes": "string"
}
```

`compiled_prompt_in_training: false` is valid for CODE_TO_DOC only when
`prompt_mode: raw_snippet`. It is not valid for orchestration, planning, skill,
or any compiler-routed prompt family unless a separate registry entry says so.
Entries with `verdict: REJECT_BATCH` are recorded but not counted toward the
capability progress metrics.

### Phase 4 Exit Gate

- Validation gate logic exists in code and has been tested against at least one
  real pair DTO file — all ten gates producing correct pass or error messages
- At least one accepted batch with `verdict: ACCEPT_BATCH` in the ledger
- Pair quality registry exists with at least one run entry
- Reward margin mean for the accepted batch is >= 0

---

## File Locations — Full Map

```
C:\Users\lubao\Documents\xiigen\llm mvp\code-documentation\
  source-registry\
    CODE-DOC-SOURCE-REGISTRY-v1.json
    CODE-DOC-EXAMINATION-LOG-v1.json
  source-rows\
    dotnet-csharp\rows.jsonl
    sql-tsql\rows.jsonl
    angular-typescript\rows.jsonl
    harvest-summary.json
  pairs\
    dotnet-csharp\pairs-v1.jsonl
    sql-tsql\pairs-v1.jsonl
    angular-typescript\pairs-v1.jsonl
    review-queue\review-needed.jsonl
    pair-summary.json
  ledger\
    code-doc-teaching-ledger-{date}.jsonl
  harvester\
    harvester.py
    scorer.py
    pair-builder.py
    validator.py
```

---

## Success Metrics

| Metric | Minimum | Target |
|---|---|---|
| Batches with reward_margin_mean >= 0 | 60% of all batches | 80% |
| Mean loss below 0.693 after 5 batches | Loss < 0.670 | Loss < 0.640 |
| Sub-domains with at least one accepted batch | 8 of 26 total | 20 of 26 |
| Mean overlap_ratio across all pairs | < 0.45 | < 0.35 |
| Pairs with annotation_strong: true | 80% | 95% |
| prompt_mode valid for domain | 100% | 100% |
| chosen_equals_rejected | 0 | 0 |
| mojibake in prompt/chosen/rejected | 0 | 0 |
| Pairs with curriculum_tier 1 | 20% | 30% |

---

## Execution Order

Phase 1 first — registry and licence before any cloning or harvesting.

Phase 2 starts with 2a manual examination — the harvester is not written
until the examination log exists for all three Tier 1 repositories.

Phase 3 starts only after Phase 2 exit gate is met and at least 100 source
rows per domain are in the surviving set.

Phase 4 validation gate is written and tested against intentionally malformed
pairs (all ten gates should fire on fabricated failures) before any real pair
batch runs through it.

---

## What Claude Code Executes

The executor builds and runs these artifacts, in this order:

```
1. Phase 1: Write CODE-DOC-SOURCE-REGISTRY-v1.json with Tier 1 entries.
   Run licence verification checklist for each Tier 1 repository.
   Record findings. No harvesting in this step.

2. Phase 2a: Examine three representative markdown files per Tier 1 repository.
   Record findings in CODE-DOC-EXAMINATION-LOG-v1.json.
   Do not write harvester.py until this step is complete.

3. Phase 2b: Write harvester.py using the examination log to configure
   the extraction heuristic per repository. Test against 10 files before
   running on the full corpus.

4. Phase 2c-2d: Run harvester.py against Tier 1 repositories.
   Apply size and quality filters. Tag sub-domains.
   Produce source row JSONL files and harvest-summary.json.

5. Phase 3a: Write scorer.py that scores explanation_raw for the three
   quality flags. Run against all surviving source rows.
   Discard rows where intent_covered is false.

6. Phase 3b-3c: Write pair-builder.py that:
   — Authors rejected sides from the four typed approaches
   — Computes overlap_ratio
   — Checks annotation quality
   — Assembles pair DTOs

7. Phase 4a: Write validator.py implementing all ten gates.
   Test against fabricated malformed pairs — all ten gates must fire
   correctly before the validator is used on real pairs.

8. Phase 4b-4d: Run validator.py against pair DTO files.
   Assemble batches meeting composition requirements.
   Run measurement gate. Record accepted batches in the ledger.
```

---

## What This Plan Does Not Cover

**Model architecture changes.** Whether the existing DPO runner handles
CODE_TO_DOC pairs transparently or needs a mode flag is not determined here.
Verify with the runner configuration before Phase 4 runs.

**Held-out evaluation.** How to measure whether the model actually produces
better documentation after training requires a separate evaluation set and
evaluation protocol. That is a follow-on plan.

**Tier 2 harvesting.** Tier 2 repositories are catalogued in Phase 1 but not
harvested until Tier 1 is complete and the pipeline is validated end-to-end.

**Automated rejected side generation at scale.** Phase 3b describes manual
and semi-automatic authoring. Full automation is possible — generate a weak
version using a prompted LLM call, then quality-gate it through the overlap
check and annotation strength check. That adds a generation step and a
secondary quality gate. Defer to a plan revision after Phase 3 is validated
with manual authoring first.
