# Corpus Expansion Plan
# Continuous whole-development corpus plan
# Version: 5.0 - 2026-05-05
# Supersedes: CORPUS-EXPANSION-PLAN-v4.md

---

## Core Change From v4

Version 4 treated corpus work as a sequence of stop/report gates. That was the
wrong operating model for the current goal.

Version 5 changes the model:

```text
No user-facing middle gates.
No pause after each batch.
No report until the current corpus execution path is finished.
Validation checks still run, but they are internal repair loops.
If a check fails, the batch fixes itself or records a source gap and keeps the corpus path moving.
```

The corpus plan is now a continuous execution protocol.

---

## Non-Negotiable Rules

These rules replace the old stop-gate language.

1. Work does not stop for a report between batches.
2. Every batch still writes a ledger, pair files, conformance notes, manifest,
   zip, and next-machine action.
3. A failed check becomes a correction task inside the same execution path.
4. Missing source authority becomes a named source knowledge edge, not an
   invented training fact.
5. The corpus is never declared production-mature until production-scale quotas
   are met.
6. The corpus can be declared build-ready before production maturity if every
   required unit and transition has full-path minimum coverage.

---

## Completion Levels

The plan now has two completion levels. This prevents the oversized production
quota from blocking the first useful learning-set build.

### Level 1 - Build-Ready Orchestration Corpus

This is the completion target for the current work.

The corpus is build-ready when all conditions below are true:

```text
all 31 sub-phases have at least one source-backed or source-gap document packet;
all 31 sub-phases have node-01 router pairs;
all 31 sub-phases have node-02 domain-translator pairs;
all 31 sub-phases are represented in node-03 scheme-planner pairs;
all 31 sub-phases are represented through node-04, node-05, and node-06 chain pairs;
all 31 sub-phases are represented through node-07, node-08, and node-09 review/formatter pairs;
all six adjacent phase transitions have at least one transition-carry-forward case;
held-out checks exist for each phase family;
every missing source area is explicitly named as a knowledge edge;
all batch artifacts are indexed and zipped.
```

Build-ready means the local machine has enough structured examples to begin
training and replacement experiments across the whole development area.

Build-ready does not mean production-mature.

### Level 2 - Production-Mature Corpus

This remains the larger quota target:

```text
31 sub-phases * 100 training document packets = 3,100 training document packets
31 sub-phases * 20 held-out document packets = 620 held-out document packets
60 transition held-out document packets
```

Production maturity is a scale target, not the blocker for the first functional
learning-set build.

---

## Source Authority Model

Each corpus row must be one of four source states:

| Source state | Meaning | May train? | May count toward build-ready? |
|---|---|---|---|
| `source_backed` | A local or license-cleared source supports the row | yes | yes |
| `external_license_checked` | User-accepted external source supports the row | yes | yes |
| `source_gap` | The required source is missing and the row teaches the machine to ask for it | yes, as knowledge-edge behavior | yes |
| `synthetic_reviewed` | A reviewed synthetic scenario fills temporary breadth | limited | yes, but must be replaced for production maturity |

The important change is that missing source does not block progress. It becomes
training material for knowledge-edge behavior.

The corpus must not pretend a `source_gap` row is source-backed.

---

## Canonical Development Area

The canonical lifecycle remains seven phases and thirty-one sub-phases.

| Phase | Sub-phases |
|---|---|
| 1 UI/UX definition | 1.1 design system and tokens; 1.2 screen and layout structure; 1.3 interaction model and state transitions; 1.4 accessibility and compliance; 1.5 motion and animation |
| 2 Functional specification | 2.1 user stories and acceptance criteria; 2.2 business rules; 2.3 integration point specification; 2.4 data model boundaries |
| 3 Technical design | 3.1 architecture and component topology; 3.2 data schema and migration design; 3.3 API contract definition; 3.4 security and authorization model; 3.5 infrastructure and deployment topology |
| 4 Implementation | 4.1 backend services; 4.2 frontend and client rendering; 4.3 integration and provider connectors; 4.4 data migration execution; 4.5 documentation |
| 5 Testing | 5.1 unit and contract tests; 5.2 integration tests; 5.3 end-to-end scenario tests; 5.4 performance and load tests |
| 6 QA | 6.1 functional review; 6.2 regression coverage verification; 6.3 security and compliance review; 6.4 accessibility audit |
| 7 QA automation | 7.1 test scenario automation; 7.2 regression suite maintenance; 7.3 CI/CD gate configuration; 7.4 coverage and reporting |

---

## Continuous Batch Contract

Every batch still produces the same artifacts. The difference is that the
executor does not stop after producing them.

Required artifacts per batch:

```text
source ledger delta
node-01 router pairs
node-02 translator pairs
node-03 planner pairs
node-04/05/06 processor-applier-translator chain pairs
node-07/08/09 arbiter-formatter pairs
held-out or source-gap conformance notes
coverage ledger update
batch index
round manifest
round zip
next-machine action
```

The next-machine action is for continuity, not for stopping.

---

## Internal Validation Checks

The old gates become internal checks.

For each batch:

```text
source rows have authority or source_gap state;
A and B are actual logical-unit boundary objects;
Phrasing B companions exist;
node-03 plans include dependencies;
node-04/05/06 chain is grounded in prior node output;
node-07 selections match visible status classes;
node-08 verdicts preserve pass, concern, reprocess, and rejected distinctions;
node-09 formatter output does not hide concerns or rejections;
zip exists;
index lists the batch.
```

If a check fails, fix the batch. Do not report and wait.

---

## Batch Shape For Build-Ready Completion

The build-ready corpus is completed in eight continuous batches.

| Batch | Scope | Purpose |
|---|---|---|
| Batch 001 | center phase group | establish full-path package shape |
| Batch 002 | UI/UX definition | cover Phase 1 |
| Batch 003 | functional specification | cover Phase 2 |
| Batch 004 | technical design remaining | cover 3.2 and 3.5 plus reinforce 3.1/3.3/3.4 |
| Batch 005 | implementation remaining | cover 4.2, 4.4, 4.5 plus reinforce 4.1/4.3 |
| Batch 006 | testing and QA | cover Phases 5 and 6 |
| Batch 007 | QA automation | cover Phase 7 |
| Batch 008 | transitions | cover six adjacent phase transitions and carry-forward behavior |

Batch 001 is already complete as a full-path pair package.

---

## Node Pair Rules

Pair rules remain unchanged:

```text
A = exact logical-unit input
B = exact LogicalUnitResult output
Reason = why B is correct for A
Phrasing B = same logical case, different surface wording
```

B must be the actual output, not a description of the output.

---

## Runtime Conformance Model

Runtime conformance is layered.

Layer 1:

```text
node-01 router conformance on held-out/source-gap rows
```

Layer 2:

```text
node-02 through node-09 learned-unit replacement checks for representative batches
```

Layer 3:

```text
full logical-flow execution through ILogicalOrchestrator-visible traces
```

Build-ready requires Layer 1 across all phase families and pair coverage across
all nine nodes. Production maturity requires Layers 2 and 3 at scale.

---

## Reporting Rule

The executor reports only when one of these is true:

```text
build-ready corpus is complete;
production-mature corpus is complete;
the run is blocked by missing user-provided source authority that cannot be represented as source_gap;
the user explicitly interrupts.
```

No middle reports.

---

## Current State After v5 Adoption

Already complete:

```text
Batch 001 full-path pair package
Batch 001 node-01 executable held-out conformance
WDA suite passing
security-verification-3.4 executable proof slice
```

Remaining for build-ready completion:

```text
Batch 002 UI/UX definition
Batch 003 functional specification
Batch 004 remaining technical design
Batch 005 remaining implementation
Batch 006 testing and QA
Batch 007 QA automation
Batch 008 transitions
```

The next machine action is Batch 002.
