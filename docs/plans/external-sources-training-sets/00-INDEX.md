# 00-INDEX — External Sources Training Sets

## What This Folder Is

This folder contains per-node training pairs derived from external open-source sources.
Each subfolder (`clean-batch-001`, etc.) corresponds to one promotion batch.
A promotion batch is a set of source scenarios that have passed all five pre-promotion checks
and have been converted into the Pair [ID] / A / B / Reason / Phrasing B format
required by the dynamic-flow pipeline.

The raw candidate material (structured JSON rows in markdown batch files) is stored
separately at: `external-sources\batches\`

Those files are useful source material but are not in the final training shape.
This folder contains only properly formatted node pairs.

---

## Batch Registry

| Batch | Source count | Pair count | Nodes covered | Promotion date | Status |
|---|---|---|---|---|---|
| clean-batch-001 | 12 scenarios | 32 pairs | 1, 3, 8, 9 | 2026-05-04 | promoted |
| clean-batch-002 | 8 scenarios | 20 pairs | 2, 4, 7 | 2026-05-04 | promoted |
| clean-batch-003 | 1 scenario | 2 pairs | 3 | 2026-05-04 | promoted |
| CB4 (wave-1/CB4-stage-3-pairs) | 4 sources, 60 base pairs | 120 instances | 1, 2, 3, 4, 5, 6, 7, 8, 9 | 2026-05-05 | stage-4-pending |

---

## License Classes Present

| License | Source IDs | Use class |
|---|---|---|
| Apache 2.0 | KUBERNETES-KEPS, OPENTELEMETRY-SPEC, DAPPER, BIGCODEBENCH | TRAINING OK |
| MIT | SWE-BENCH, EFCORE-SOURCE, DOTNET-MICROSERVICES, NESTJS-ADVANCED, VUE-DOCS | TRAINING OK |
| CC-BY 4.0 | REACT-ADVANCED, ANGULAR-DOCS, ASPNETCORE-DOCS-SAMPLES, GOOGLE-AIP, MICROSOFT-LEARN-ARCH | ATTRIBUTION REQUIRED |
| PSF 2.0 | PYTHON-PEPS | TRAINING OK |
| CC BY-SA 4.0 | OWASP-ASVS (CB4) | ATTRIBUTION + SHARE-ALIKE REQUIRED |
| Public domain (NIST) | NIST-SP-800-218-SSDF (CB4) | TRAINING OK |
| Public domain (MITRE) | CWE-TOP25 (CB4) | TRAINING OK |
| IETF Trust (pre-5378) | IETF-RFCS (CB4) — RFC 7519, 6749, 7636, 8446 | TRAINING OK |

---

## What Is Not Here Yet

Nodes 5 and 6 have no pairs yet. These require:
- Node 5 (Applier): pairs where A is sub-task + transformational prompt + processed result and B is the concrete action and observable outcome.
- Node 6 (Result Translator): pairs where A is one formal result and B is natural language the user can read and act on.

Node 2 covers labels: C-frontend-rendering-performance, D-database-query-efficiency, A-api-topology-selection, A-node-provisioning-strategy. Still missing: E-orm-usage-pattern, B-backend-response-latency.
Node 3 Django N+1 scheme plan pair added in clean-batch-003 (CB3-03-001). Chain Node 1 → Node 2 (D label) → Node 3 → Node 4 is now fully trained for the Django N+1 scenario.
Node 4 covers sub-tasks: select_related queryset, consistent-hash routing, nullable column add, Redis CacheModule registration.
Node 7 covers: Django N+1 multi-arbiter fire case, Dapper connection reuse pass-through case.

These will be addressed in clean-batch-004 and beyond.

---

## Examination Domain Pairs — Action 2 Gap Closure

Produced by gap closure pass across five experiments from EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md (internal).
Folder: `examination-domain-pairs/`
These pairs teach the five examination domains introduced by the experiment translation work — not general node behavior.
Each domain corresponds to one experiment's target pathology.

| File | Pairs | Nodes taught | Examination domain | Action | Date |
|---|---|---|---|---|---|
| node-02-examination-domain-set.md | 10 | 2 | all five domains | Action 2 gap closure | 2026-05-04 |
| node-03-examination-domain-set.md | 10 | 3 | all five domains | Action 2 gap closure | 2026-05-04 |
| node-07-examination-domain-set.md | 20 | 7 | all five domains | Action 2 gap closure | 2026-05-04 |
| node-08-examination-domain-set.md | 20 | 8 | all five domains | Action 2 gap closure | 2026-05-04 |

Total examination domain pairs: 60 (each file contains both fire cases and pass cases per domain).

Examination domains covered:
- scheme-planner-examination — parallel cross-reference, applier crossover, hollow prerequisite
- domain-translator-examination — implementation syntax in required outcome, solution name in formal description, untraced target
- domain-processor-examination — silent failure signal, unself-contained wait state, undistinguished failure states
- arbiter-selector-examination — active domain label with implicit result disposition, inline review without result references
- cascade-plan-examination — prerequisite check without exhaustive forms, missing recovery branch, target without provenance

---

## Action Log

### Action 1 — Correction Pass (2026-05-04)

Corrections applied to five existing pair files based on CONCERN findings from EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md.
Each correction removes the pathology named in the experiment while preserving the training signal.

| File | Pair | Correction |
|---|---|---|
| clean-batch-001/node-03-scheme-planner-set.md | CB1-03-001 Step 3 (original + PB) | Replaced forward reference to Step 4 output with final-state description; parallel note explicit |
| clean-batch-001/node-03-scheme-planner-set.md | CB1-03-002 Phase 3 (original + PB) | Replaced exact SQL with structural batch description — no SQL syntax, no numeric parameters |
| clean-batch-001/node-03-scheme-planner-set.md | CB1-03-004 Step 1 (original + PB) | Replaced pre-resolved Redis reachability check with DI injection point verification |
| clean-batch-002/node-02-domain-translator-set.md | CB2-02-002 B (original + PB) | Replaced SQL SELECT clause in required outcome with structural join description |
| clean-batch-002/node-02-domain-translator-set.md | CB2-02-003 B (original + PB) | Replaced "BFF" solution name with topology description (three independent API layers) |
| clean-batch-002/node-02-domain-translator-set.md | CB2-02-004 Reason (original + PB) | Added explicit traceability note for the 30–60 second synthesized target |
| clean-batch-002/node-04-domain-processor-set.md | CB2-04-002 Failure state 2 (original + PB) | Added observable signal statement naming silence at origin and downstream Collector metric |
| clean-batch-002/node-04-domain-processor-set.md | CB2-04-002 Wait state (original + PB) | Added specific resolution check with span count, service count, and pass/fail criteria |
| clean-batch-002/node-04-domain-processor-set.md | CB2-04-003 Reason (original) | Added note distinguishing Failure states 1 and 3 by phase, observable signal, and remediation |
| clean-batch-002/node-07-arbiter-selector-set.md | CB2-07-001 B (original + PB) | Added explicit B-backend-response-latency entry with [R4] disposition |
| clean-batch-002/node-07-arbiter-selector-set.md | CB2-07-002 B (original + PB) | Replaced general connection-management NO FIRE verdict with [R1]–[R4] result-referenced inline review |
| clean-batch-003/node-03-scheme-planner-set.md | CB3-03-001 Step 1 (original + PB) | Added file location, exhaustive passing forms, exhaustive failing forms with corrective paths |
| clean-batch-003/node-03-scheme-planner-set.md | CB3-03-001 Recovery path (original + PB) | Added second branch for production-path mismatch scenario |
| clean-batch-003/node-03-scheme-planner-set.md | CB3-03-001 Reason (original + PB) | Added provenance note for 0.5-second target (sourced from CB2-04-001, not A input) |

### Action 2 — Gap Closure Pass (2026-05-04)

New pairs written for `examination-domain-pairs/` folder. These pairs teach the examination domain vocabulary
introduced by the five experiment designs. See the Examination Domain Pairs section above for file details.

Action 3 (coverage extension — pairs for proxied nodes 4, 5, 6) is deferred until Action 2 pairs are validated
by running pass gates per HOW-TO-STRUCTURE-A-LEARNING-SET-FROM-AN-EXPERIMENT-v1.md.

---

## Coverage Ledger

See `COVERAGE-LEDGER.jsonl` at `external-sources\COVERAGE-LEDGER.jsonl`.
As of 2026-05-04: 1510 candidate rows examined (EXT-001 through EXT-1510).
Promotion status of those rows: `examined` (candidate material, not yet in node files).
Rows promoted in clean-batch-001: 12 source scenarios → 32 node pairs.
