# External Source Registry — Code Documentation DPO Domain
# Version: 1.0 — 2026-05-10
# Domain: code-documentation
# Sub-domains: dotnet-csharp | sql-tsql | angular-typescript
# Observed generated domains pending registry entry: mongodb-csharp | elasticsearch-csharp
# Companion to: XIIGEN-INTERNAL-SOURCE-REGISTRY-v11.md
# Used by: EXAMINATION-METHOD-CODE-DOCUMENTATION-v1.md
#           PLAN-CODE-DOCUMENTATION-DPO-v2.md
# Operational runbook: TRAINING-SET-BUILD-AND-TESTING-RUNBOOK-v1.md

---

## Purpose

This registry defines every external source approved for DPO pair construction
in the code-documentation capability domain. It governs canonicality, licence
status, extraction rules, pair types permitted, and per-source examination
protocol references.

This registry does NOT govern internal XIIGen session documents or correction
events. Those are governed by XIIGEN-INTERNAL-SOURCE-REGISTRY-v11.md.

The code-documentation domain teaches the model to produce clever documentation
for .NET C#, T-SQL, and Angular TypeScript code. Every source in this registry
provides code-explanation pairs where the code snippet is the A side and the
explanation is the B side (chosen or rejected).

---

## 2026-05-11 Registry Addendum from Learning-Set Audit

The audit of `documentation/learning sets.zip` found a valid generated library,
but not a train-ready library.

```
training rows audited:        50,000
JSON validity:                pass
pair ID uniqueness:           pass
chosen == rejected:           4,660 rows
mojibake in train fields:     4,912 rows
strict safe candidates:       3,921 rows
unregistered domains:         mongodb-csharp, elasticsearch-csharp
```

Registry consequence:

1. `mongodb-csharp` and `elasticsearch-csharp` are not train-ready domains until
   this registry receives explicit source entries for their documentation
   authorities, license state, paths, and pair construction rules.
2. A generated pair is not accepted merely because its `source_boundary` is
   `external` and its JSON is valid. It must pass the training-field gates in
   this registry and the examination method.
3. Ledger rows are not DPO pairs. They are evidence records and must stay out
   of train-ready JSONL.
4. CODE_TO_DOC uses raw code snippets as the A-side by design. Therefore
   `compiled_prompt_in_training: false` is valid only when the row also carries
   raw-snippet authority from this registry.

---

## Canonicality Framework

Every source in this registry is classified before any pair is constructed.
The classification governs whether a given copy of a document is a valid
training source at all.

```
canonical
  The authoritative version at the primary repository location.
  Pair construction is allowed subject to type rules and licence gate.
  All Tier 1 sources in this registry are canonical by definition.

extracted_copy
  A file extracted from a repository clone or ZIP for local processing.
  Valid for pair construction only when:
    — the source commit SHA is recorded at extraction time
    — the canonical repository URL is recorded alongside it
    — the extraction date is recorded
  All local working copies of cloned repositories are extracted_copy.

stale
  A version of a documentation page or file where a newer version has been
  published to the same path. Do not use as chosen side authority.
  May be used as rejected side (superseded explanation is a ready-made
  wrong answer — Type E from the DPO pair authoring guidance).
  Detect by: checking the git log for the source file, or by comparing
  the last_harvested_commit in this registry against the current HEAD.

external
  All sources in this registry are classified external.
  They must never mix silently into internal XIIGen teaching batches.
  They must always be tracked in a separate batch file with the field
  source_boundary: external in every pair row.

archive_container
  A ZIP or tarball of a documentation site export.
  Not a training source itself. Must be unpacked and each extracted file
  classified separately against this registry.

secure_operational
  Not applicable to documentation repositories. If a repository clone
  contains .env, terraform.tfstate, or credentials files, those files
  are classified secure_operational and excluded before any harvest runs.
```

**How to record canonicality in every pair row:**

```json
{
  "source_registry_id": "dotnet-docs-mit",
  "source_boundary": "external",
  "source_canonicality": "extracted_copy",
  "source_file_path": "docs/csharp/asynchronous-programming/async-in-depth.md",
  "source_commit": "a3f7c12...",
  "source_url": "https://github.com/dotnet/docs/blob/a3f7c12.../docs/csharp/...",
  "canonical_confirmed_via": "CODE-DOC-SOURCE-REGISTRY-v1 — dotnet-docs-mit — verified 2026-05-10"
}
```

A pair row where `source_canonicality` is missing, or is set to a value other
than `canonical` or `extracted_copy`, is rejected at the pre-training validation
gate regardless of pair quality.

---

## Licence Framework

Only two licence classes are permitted in this registry.

```
MIT
  Permission is granted to use, copy, modify, distribute the software and
  documentation for any purpose. Attribution: preserve the copyright notice.
  Action required: record the copyright line from the repository LICENSE file
  in this registry's notes field for each entry. Include in pair row provenance.

Apache-2.0
  Same permissions as MIT with two additional requirements:
  1. Preserve the NOTICE file if one exists in the repository.
  2. State changes made to any covered file.
  Action required: check for a NOTICE file in the repository root.
  If present: record NOTICE file path in this registry. Include NOTICE
  content in pair row provenance.
```

Licences not permitted: Creative Commons ShareAlike (share-alike clause creates
downstream obligations that are not resolved for this use), GPL family (viral),
proprietary, or unlicensed repositories.

Stack Overflow data (Creative Commons Attribution-ShareAlike 4.0) is deferred
to a separate legal review before any pairs are extracted. It does not appear
in this registry version.

---

## Source Type Taxonomy

Each source in this registry belongs to one of these types. The type determines
which pair constructions are permitted and which are excluded.

```
EXT-1 — Official documentation repository
  Definition: The primary documentation repository maintained by the vendor
  or core maintainers of the technology. Code examples are written specifically
  to teach the technology, accompanied by prose explanations.
  Permitted pairs: CODE_TO_DOC chosen from explanation prose. Rejected from the
  four typed weak documentation patterns.
  Excluded: Do not use section headings as explanations. Do not use code blocks
  that are syntax demonstrations without logic (import-only, namespace-only).

EXT-2 — Project documentation repository
  Definition: A README, wiki, or docs directory maintained by a high-quality
  open-source project. Code examples document the project's own API with
  explanations written for users of the project.
  Permitted pairs: CODE_TO_DOC chosen from explanation prose. Lower volume
  than EXT-1 but often higher specificity on design decisions (why coverage).
  Excluded: Do not use issue threads, pull request discussions, or changelog
  entries — these are not written as documentation.

EXT-3 — Tutorial and guide platform (deferred)
  Definition: External learning platforms with MIT or Apache-2.0 licensed
  content. Not yet included in this registry — requires per-platform licence
  verification before any source is added.

EXT-X — Excluded
  A source that has been evaluated and rejected. Recorded here to prevent
  re-evaluation. Reason is documented in the notes field.
```

---

## Tier 1 Sources — Official Documentation Repositories

These three repositories are the primary harvest targets. All are official
documentation repositories. All use MIT licence covering both code and
documentation content. All are classified `canonical` at the repository level
and `extracted_copy` for local working copies.

---

### Source: dotnet-docs-mit

```
Registry ID:         dotnet-docs-mit
Type:                EXT-1
Repository URL:      https://github.com/dotnet/docs
Licence:             MIT
Licence file path:   LICENSE
Copyright notice:    Copyright (c) .NET Foundation and Contributors
NOTICE file:         none
Domain:              dotnet-csharp
Harvest status:      pending
Last harvested commit: —
Estimated pair volume: 8,000–15,000 candidates before filtering
Primary content paths:
  docs/csharp/
  docs/standard/
  docs/core/
Excluded paths:
  docs/architecture/     (high-level architecture prose — not code-specific)
  docs/whats-new/        (release notes — not explanation pairs)
  docs/machine-learning/ (different domain)
  api/                   (API reference — minimal prose explanation)
Verified date:       —
Verified by:         —
Notes:
  The csharp/ directory contains the richest code-explanation pairs.
  The standard/ directory covers collections, LINQ, async — all high priority
  sub-domains. The api/ directory is excluded — it contains method signatures
  and parameter lists but minimal prose explanation of intent.
  Copyright notice must be preserved in source_provenance of every pair row.
```

**Sub-domain coverage map:**

| Sub-domain | Primary paths in repo |
|---|---|
| async-await | docs/csharp/asynchronous-programming/ |
| linq | docs/csharp/linq/ |
| dependency-injection | docs/core/extensions/dependency-injection.md |
| generics | docs/csharp/programming-guide/generics/ |
| extension-methods | docs/csharp/programming-guide/classes-and-structs/extension-methods.md |
| attributes-reflection | docs/csharp/advanced-topics/reflection-and-attributes/ |
| exception-handling | docs/csharp/fundamentals/exceptions/ |
| collections | docs/standard/collections/ |
| records-structs | docs/csharp/language-reference/builtin-types/record.md |
| pattern-matching | docs/csharp/fundamentals/functional/pattern-matching.md |

**Extraction heuristic (provisional — refine from examination log):**

A candidate pair is a triple-backtick code block tagged with `csharp` or `cs`
that is followed within 600 characters by a prose paragraph of 40 or more words
that does not itself contain a code block.

The 600-character window is wider than default because the dotnet/docs repository
often inserts a blank line or a short lead-in sentence between the code block and
the explanation paragraph. Refine this value after running Phase 2a examination.

**Pair kinds permitted:** `CODE_TO_DOC`

**Rejected types expected:**
- Type 1 (mechanical paraphrase): high volume — many dotnet/docs explanations are
  thorough, making the mechanical version easy to generate
- Type 2 (name echo): applicable for well-named API members
- Type 3 (wrong level): applicable where internal implementation is visible
- Type 4 (missing edge case): highest value — async cancellation, null returns,
  empty collection behaviour are frequent edge cases in this corpus

---

### Source: sql-docs-mit

```
Registry ID:         sql-docs-mit
Type:                EXT-1
Repository URL:      https://github.com/MicrosoftDocs/sql-docs
Licence:             MIT
Licence file path:   LICENSE
Copyright notice:    Copyright (c) Microsoft Corporation
NOTICE file:         none
Domain:              sql-tsql
Harvest status:      pending
Last harvested commit: —
Estimated pair volume: 3,000–6,000 candidates before filtering
Primary content paths:
  docs/t-sql/
  docs/relational-databases/
Excluded paths:
  docs/sql-server/install/     (installation guides — no code pairs)
  docs/ssms/                   (SSMS UI — not T-SQL focused)
  docs/reporting-services/     (different domain)
  docs/analysis-services/      (different domain)
Verified date:       —
Verified by:         —
Notes:
  The t-sql/ directory is the primary target. It covers statements, functions,
  operators, and data types with worked examples and explanation prose.
  The relational-databases/ directory covers indexes, transactions, and query
  processing — rich source for edge-case documentation pairs (e.g. what happens
  at different isolation levels, when an index is not used).
  Code blocks in this repo use the sql language tag. Some blocks use
  sql-server or tsql tags — include all three in the language tag filter.
  Copyright notice must be preserved in source_provenance of every pair row.
```

**Sub-domain coverage map:**

| Sub-domain | Primary paths in repo |
|---|---|
| select-queries | docs/t-sql/queries/ |
| window-functions | docs/t-sql/functions/ranking-functions-transact-sql.md and sibling files |
| ctes | docs/t-sql/queries/with-common-table-expression-transact-sql.md |
| stored-procedures | docs/t-sql/statements/create-procedure-transact-sql.md |
| indexes | docs/relational-databases/indexes/ |
| transactions | docs/t-sql/language-elements/begin-transaction-transact-sql.md and siblings |
| dynamic-sql | docs/relational-databases/system-stored-procedures/sp-executesql-transact-sql.md |
| error-handling | docs/t-sql/language-elements/try-catch-transact-sql.md |

**Extraction heuristic (provisional — refine from examination log):**

A candidate pair is a triple-backtick code block tagged with `sql`, `tsql`, or
`sql-server` that is followed within 500 characters by a prose paragraph of
40 or more words that does not contain a code block.

SQL documentation in this repository tends to place explanation after the code
example. The explanation often begins with "In the following example" or "The
following example shows" — these lead-in phrases should be stripped from the
explanation before scoring quality dimensions, as they are structural prose, not
intent-level explanation.

**Special handling for SQL examples:**

Many SQL examples in this repository show multiple queries in one code block —
a setup query followed by the main query. When the code block contains more than
one distinct SQL statement, segment the block at the statement boundary. Pair the
main query (the one the explanation describes) with the explanation. Discard the
setup queries from the pair — they add context the model does not need to produce
documentation for the main query.

**Pair kinds permitted:** `CODE_TO_DOC`

**Rejected types expected:**
- Type 3 (wrong level): highest applicability — SQL documentation often explains
  the execution plan internally when callers need to know the result set shape
- Type 4 (missing edge case): transaction isolation levels, NULL handling in
  aggregates, what happens when a CTE is empty — all rich edge case territory

---

### Source: angular-adev-mit

```
Registry ID:         angular-adev-mit
Type:                EXT-1
Repository URL:      https://github.com/angular/angular
Licence:             MIT
Licence file path:   LICENSE
Copyright notice:    Copyright Google LLC
NOTICE file:         none
Domain:              angular-typescript
Harvest status:      pending
Last harvested commit: —
Estimated pair volume: 2,000–4,000 candidates before filtering
Primary content paths:
  adev/src/content/guide/
  adev/src/content/tutorials/
Excluded paths:
  adev/src/content/reference/        (API reference — minimal prose)
  adev/src/content/roadmap.md        (planning document — no code pairs)
  packages/                           (framework source — not documentation)
  integration/                        (integration test fixtures)
Verified date:       —
Verified by:         —
Notes:
  The adev/ directory is the Angular documentation source introduced in Angular 17.
  Earlier documentation lived in angular.io/ — that content has been migrated to adev/.
  Do not harvest from the older angular.io/ path even if present in a clone.
  The guide/ directory is the primary target: component lifecycle, dependency
  injection, RxJS patterns, signals, forms, routing.
  The tutorials/ directory provides step-by-step examples with explanation —
  richer chosen side candidates because the prose explains design intent.
  Code blocks use typescript as the language tag.
  Copyright notice must be preserved in source_provenance of every pair row.
```

**Sub-domain coverage map:**

| Sub-domain | Primary paths in repo |
|---|---|
| components | adev/src/content/guide/components/ |
| services-di | adev/src/content/guide/di/ |
| rxjs-observables | adev/src/content/guide/pipes.md and rxjs references in guide/ |
| signals | adev/src/content/guide/signals.md |
| forms | adev/src/content/guide/forms/ |
| routing | adev/src/content/guide/routing/ |
| change-detection | adev/src/content/guide/change-detection.md |
| http-client | adev/src/content/guide/http/ |

**Extraction heuristic (provisional — refine from examination log):**

A candidate pair is a triple-backtick code block tagged with `typescript` or `ts`
that is followed within 500 characters by a prose paragraph of 40 or more words
that does not contain a code block.

Angular documentation frequently uses annotations in code blocks (decorator syntax
with `@`, template expressions). Snippets that consist only of a template HTML
block with no TypeScript logic are excluded — the language tag is often `html` in
these cases, which the domain filter will catch. Snippets that show only
`@Component` decorator metadata with no class body are excluded by the minimum
5-line rule.

**Special handling for Angular examples:**

Angular tutorials often split a component into template and class segments shown
in separate code blocks. When two consecutive code blocks appear — one `html` and
one `typescript` — treat the `typescript` block as the candidate snippet and use
the explanation that follows both blocks as the candidate explanation. Do not pair
the `html` block alone with the explanation.

**Pair kinds permitted:** `CODE_TO_DOC`

**Rejected types expected:**
- Type 3 (wrong level): strong applicability — Angular documentation sometimes
  explains internal lifecycle hook timing when callers need to know what to do
  in the hook, not how Angular schedules it
- Type 4 (missing edge case): signal computation dependencies, RxJS unsubscribe
  patterns, change detection edge cases with OnPush — all rich edge case territory

---

## Tier 2 Sources — Project Documentation Repositories

These six repositories are harvested after Tier 1 is complete and the pipeline
is validated end-to-end. All use MIT or Apache-2.0 licences. Volume is lower
than Tier 1 but specificity on design decisions (why coverage) is often higher.

---

### Source: automapper-mit

```
Registry ID:         automapper-mit
Type:                EXT-2
Repository URL:      https://github.com/AutoMapper/AutoMapper
Licence:             MIT
Licence file path:   LICENSE.txt
Copyright notice:    Copyright (c) 2010 Jimmy Bogard
NOTICE file:         none
Domain:              dotnet-csharp
Harvest status:      pending
Last harvested commit: —
Estimated pair volume: 200–500 candidates
Primary content paths:
  docs/
  README.md
Excluded paths:
  src/                (framework source — not documentation)
  tests/              (test fixtures — not documentation)
Verified date:       —
Verified by:         —
Notes:
  AutoMapper documentation explains object-to-object mapping, profile
  configuration, and custom value resolvers. Rich source for why coverage —
  the documentation frequently explains why mapping is configured a particular
  way (e.g. why ForMember is used instead of convention mapping for a
  specific property). Strong source for dependency-injection sub-domain.
```

---

### Source: mediatr-mit

```
Registry ID:         mediatr-mit
Type:                EXT-2
Repository URL:      https://github.com/jbogard/MediatR
Licence:             MIT
Licence file path:   LICENSE
Copyright notice:    Copyright (c) Jimmy Bogard
NOTICE file:         none
Domain:              dotnet-csharp
Harvest status:      pending
Last harvested commit: —
Estimated pair volume: 100–300 candidates
Primary content paths:
  README.md
  docs/ (if present)
Excluded paths:
  src/
  samples/   (examine separately — may contain explanation prose)
Verified date:       —
Verified by:         —
Notes:
  MediatR documentation covers CQRS request/handler patterns, pipeline
  behaviours, and notification publishing. The README contains high-quality
  code examples with explanation of the design intent. The samples/ directory
  contains runnable examples — examine separately in Phase 2a to determine
  whether explanation prose accompanies the samples.
  Strong source for the intent dimension: MediatR explanations routinely
  state why the mediator pattern is used, not just what it does.
```

---

### Source: fluentvalidation-apache

```
Registry ID:         fluentvalidation-apache
Type:                EXT-2
Repository URL:      https://github.com/FluentValidation/FluentValidation
Licence:             Apache-2.0
Licence file path:   LICENSE
Copyright notice:    Copyright (c) .NET Foundation and Contributors
NOTICE file:         check at verification time — record path if present
Domain:              dotnet-csharp
Harvest status:      pending
Last harvested commit: —
Estimated pair volume: 300–600 candidates
Primary content paths:
  docs/
Excluded paths:
  src/
  tests/
Verified date:       —
Verified by:         —
Notes:
  Apache-2.0 source. NOTICE file must be checked at verification.
  FluentValidation documentation explains rule chaining, custom validators,
  and async validation with clear intent-level prose. The docs/ directory
  is structured with worked examples. Strong source for extension-methods
  sub-domain. Explanation prose frequently covers edge cases (what happens
  when a rule chain fails, when async validation is required).
```

---

### Source: dapper-apache

```
Registry ID:         dapper-apache
Type:                EXT-2
Repository URL:      https://github.com/DapperLib/Dapper
Licence:             Apache-2.0
Licence file path:   License.txt
Copyright notice:    Copyright (c) 2011 Marc Gravell, contributors
NOTICE file:         check at verification time — record path if present
Domain:              sql-tsql
Harvest status:      pending
Last harvested commit: —
Estimated pair volume: 200–400 candidates
Primary content paths:
  Readme.md
  docs/ (if present)
Excluded paths:
  Dapper/        (library source — not documentation)
  DapperTests/   (test fixtures)
Verified date:       —
Verified by:         —
Notes:
  Apache-2.0 source. NOTICE file must be checked at verification.
  Dapper documentation explains parameterized query patterns, result mapping,
  multi-result queries, and stored procedure calls. SQL-centric with C# on the
  execution side. Strong source for stored-procedures sub-domain and for the
  why dimension (why parameterized queries prevent SQL injection, why strongly
  typed mapping is used). Edge case coverage often present (null mapping
  behaviour, multi-result disposal).
```

---

### Source: ngrx-mit

```
Registry ID:         ngrx-mit
Type:                EXT-2
Repository URL:      https://github.com/ngrx/platform
Licence:             MIT
Licence file path:   LICENSE
Copyright notice:    Copyright (c) 2017 Brandon Roberts, Mike Ryan, Rob Wormald, Victor Savkin
NOTICE file:         none
Domain:              angular-typescript
Harvest status:      pending
Last harvested commit: —
Estimated pair volume: 400–800 candidates
Primary content paths:
  projects/ngrx.io/content/guide/
Excluded paths:
  projects/ngrx.io/content/api/   (API reference — minimal prose)
  modules/                         (framework source)
Verified date:       —
Verified by:         —
Notes:
  NgRx documentation explains state management patterns: store, actions,
  reducers, effects, selectors. Strong source for rxjs-observables sub-domain
  and for the why dimension — NgRx documentation consistently explains the
  design rationale for unidirectional data flow. The content/guide/ directory
  mirrors the structure of the Angular adev documentation. Effects and selector
  documentation contains rich edge case coverage (what happens when an effect
  errors, memoization behaviour of selectors).
```

---

### Source: angular-material-mit

```
Registry ID:         angular-material-mit
Type:                EXT-2
Repository URL:      https://github.com/angular/components
Licence:             MIT
Licence file path:   LICENSE
Copyright notice:    Copyright Google LLC
NOTICE file:         none
Domain:              angular-typescript
Harvest status:      pending
Last harvested commit: —
Estimated pair volume: 300–600 candidates
Primary content paths:
  src/material/        (component docs alongside source)
  src/cdk/             (CDK docs alongside source)
Excluded paths:
  src/dev-app/         (demo app — not documentation)
  src/e2e-app/         (test app)
Verified date:       —
Verified by:         —
Notes:
  Angular Material documentation is co-located with component source in the
  src/material/ directory. Each component directory contains a *.md file with
  usage examples and explanation. The CDK (Component Dev Kit) documentation in
  src/cdk/ is higher signal for the components sub-domain because it explains
  abstract patterns rather than specific Material Design components.
  Explanation prose tends toward the intent dimension (what the component
  provides) with some why coverage (why CDK is used instead of direct DOM
  manipulation). Edge case coverage is variable — examine during Phase 2a.
```

---

## Excluded Sources

These sources were evaluated and rejected. Recorded here to prevent re-evaluation.

---

### Excluded: stackoverflow-ccsa

```
Registry ID:         stackoverflow-ccsa
Type:                EXT-X (excluded)
Repository URL:      https://archive.org/details/stackexchange
Licence:             Creative Commons Attribution-ShareAlike 4.0
Reason for exclusion:
  ShareAlike clause requires derivative works to use the same licence.
  Legal implications for training data use are not resolved for this project.
  Deferred to a separate legal review session.
  Do not harvest from this source until explicit approval is recorded here.
Status:              deferred — do not use
```

---

### Excluded: microsoft-learn-scraped

```
Registry ID:         microsoft-learn-scraped
Type:                EXT-X (excluded)
Reason for exclusion:
  Scraped web content from learn.microsoft.com is not a licensed source.
  The underlying content IS available under MIT via github.com/dotnet/docs
  and github.com/MicrosoftDocs/sql-docs (both registered above).
  Use the GitHub repositories — not scraped web content.
Status:              excluded — use dotnet-docs-mit or sql-docs-mit instead
```

---

## Pair Types Permitted by Source Type

| Source type | CODE_TO_DOC chosen | TYPE_E stale-version pairs | Other pair kinds |
|---|---|---|---|
| EXT-1 | Yes — primary use | Yes — when older doc versions exist | No |
| EXT-2 | Yes — primary use | No — version history not tracked at this stage | No |
| EXT-X | No | No | No |

All pairs from all sources carry `source_boundary: external` and must be
batched in external-only batch files. They must never silently mix with
internal XIIGen teaching pairs.

---

## Licence Verification Checklist

Run once per source before any harvest begins. Record results in the
verification fields of this registry.

```
□ Navigate to the repository root on GitHub
□ Locate the LICENSE or LICENSE.txt file
□ Read the full licence text — confirm it matches MIT or Apache-2.0 verbatim
□ Record the copyright line (e.g. "Copyright (c) .NET Foundation and Contributors")
  in the notes field of this registry entry
□ Check whether a NOTICE file exists at the repository root
  — If yes: record the NOTICE file path in the notes field
  — If yes: read the NOTICE file and record its content in the notes field
□ Check whether the documentation directory has its own separate licence file
  — If yes: confirm it is also MIT or Apache-2.0
  — If it is a different licence: exclude that directory from harvest
    and record the exclusion in the excluded_paths list
□ Record verified_date as today's date in ISO format
□ Record verified_by as the person or session that ran the check
□ Set harvest_status to pending (ready to begin Phase 2)
  — Do not set in-progress until Phase 2a examination log is complete
```

---

## Source Row Schema

Every source row extracted from any source in this registry has this shape.
This is the intermediate record between raw extraction and pair construction.

```json
{
  "source_row_id": "uuid",
  "registry_id": "string — e.g. dotnet-docs-mit",
  "source_boundary": "external",
  "source_canonicality": "extracted_copy",
  "canonical_confirmed_via": "CODE-DOC-SOURCE-REGISTRY-v1 — {registry_id} — verified {date}",
  "source_file_path": "string — path within the repository",
  "source_commit": "string — commit SHA at time of extraction",
  "source_url": "string — full GitHub URL with commit and line anchor",
  "domain": "dotnet-csharp | sql-tsql | angular-typescript",
  "sub_domain": "string — from the sub-domain taxonomy",
  "language_tag": "string — the code fence language tag",
  "snippet_raw": "string — the raw code content",
  "snippet_lines": "integer",
  "explanation_raw": "string — the prose paragraph",
  "explanation_words": "integer",
  "extraction_heuristic": "string — which heuristic rule produced this row",
  "harvest_timestamp": "string — ISO datetime",
  "pair_status": "candidate | filtered-out | promoted-to-pair",
  "filter_reason": "string — reason when pair_status is filtered-out"
}
```

---

## Pair Row Schema

Every pair produced from any source in this registry has these fields in
addition to the source row fields above. These are the fields validated by the
pre-training gate.

```json
{
  "pair_id": "uuid",
  "source_row_id": "string — links to source row",
  "source_boundary": "external",
  "source_canonicality": "extracted_copy",
  "source_type": "EXT-1 | EXT-2",
  "capability_domain": "code-documentation",
  "domain": "dotnet-csharp | sql-tsql | angular-typescript",
  "sub_domain": "string",
  "pair_kind": "CODE_TO_DOC",
  "curriculum_tier": "1 | 2 | 3",
  "prompt": "string — the code snippet",
  "prompt_mode": "raw_snippet",
  "compiled_prompt_in_training": false,
  "chosen": "string — the clever documentation",
  "rejected": "string — the weak documentation",
  "rejected_type": "mechanical-paraphrase | name-echo | wrong-level | missing-edge-case",
  "intent_covered": "boolean",
  "why_covered": "boolean",
  "edge_case_covered": "boolean",
  "overlap_ratio": "float",
  "annotation_quality": "float 0-1",
  "annotation_strong": "boolean",
  "chosen_equals_rejected": "boolean - must be false for train_ready",
  "mojibake_in_prompt": "boolean - must be false for train_ready",
  "mojibake_in_chosen": "boolean - must be false for train_ready",
  "mojibake_in_rejected": "boolean - must be false for train_ready",
  "authored_by": "harvested | semi-automatic | manual",
  "authored_date": "string — ISO date",
  "pair_status": "valid | review-needed | invalid",
  "quality_status": "train_ready | needs_review | rejected"
}
```

`compiled_prompt_in_training` is `false` for all CODE_TO_DOC pairs in this
domain. Code snippets are not processed through the DevelopmentPromptCompiler —
they are already structured content. The training infrastructure must accept
`false` as a valid value for this domain. Confirm with the runner configuration
before Phase 4 begins.

Additional hard train-ready rules:

```
chosen_equals_rejected == true      -> rejected
mojibake_in_prompt == true          -> repair-needed
mojibake_in_chosen == true          -> repair-needed
mojibake_in_rejected == true        -> repair-needed
domain not listed in this registry  -> repair-needed until registry entry added
quality_status == needs_review      -> not train-ready
```

The most important failure is `chosen_equals_rejected`. It is worse than a weak
annotation because it provides no preference direction at all.

---

## Quick Reference Table

| Registry ID | Type | Licence | Domain | Tier | Harvest status | Estimated pairs |
|---|---|---|---|---|---|---|
| dotnet-docs-mit | EXT-1 | MIT | dotnet-csharp | 1 | pending | 8,000–15,000 |
| sql-docs-mit | EXT-1 | MIT | sql-tsql | 1 | pending | 3,000–6,000 |
| angular-adev-mit | EXT-1 | MIT | angular-typescript | 1 | pending | 2,000–4,000 |
| automapper-mit | EXT-2 | MIT | dotnet-csharp | 2 | pending | 200–500 |
| mediatr-mit | EXT-2 | MIT | dotnet-csharp | 2 | pending | 100–300 |
| fluentvalidation-apache | EXT-2 | Apache-2.0 | dotnet-csharp | 2 | pending | 300–600 |
| dapper-apache | EXT-2 | Apache-2.0 | sql-tsql | 2 | pending | 200–400 |
| ngrx-mit | EXT-2 | MIT | angular-typescript | 2 | pending | 400–800 |
| angular-material-mit | EXT-2 | MIT | angular-typescript | 2 | pending | 300–600 |
| stackoverflow-ccsa | EXT-X | CC-BY-SA | all | — | excluded | — |
| microsoft-learn-scraped | EXT-X | none | all | — | excluded | — |

---

## Gap Map by Domain

| Domain | Primary gap | Root cause | Fix |
|---|---|---|---|
| dotnet-csharp | edge_case_covered low | Async cancellation and null-return patterns require reading code carefully | Prioritise Type 4 pairs from async-await and linq sub-domains |
| sql-tsql | why_covered low | SQL documentation tends toward mechanical explanation | Seek stored-procedure and index documentation — these have design rationale |
| angular-typescript | annotation_strong low for Type 4 | RxJS unsubscribe and signal patterns have high overlap between correct and weak docs | Require 5+ non-overlapping tokens in annotation for all angular Type 4 pairs |
| mongodb-csharp | unregistered generated domain | `learning sets.zip` contains 10,000 rows but this registry has no MongoDB source authority | Add registry entry, license proof, canonical paths, and structure rules before training |
| elasticsearch-csharp | unregistered generated domain | `learning sets.zip` contains 10,000 rows but this registry has no Elasticsearch source authority | Add registry entry, license proof, canonical paths, and structure rules before training |
| all | generated-row identity bug | 4,660 audited rows had identical chosen and rejected text | Hard reject before training; rebuild rejected side or move to repair-needed |
| all | encoding corruption | 4,912 audited rows had mojibake in prompt/chosen/rejected | Repair encoding before training; do not let corrupted text reach DPO |

---

## Last Updated

2026-05-11. Version 1.0 plus run-experience addendum from V244-V247 and
`learning sets.zip` audit.
