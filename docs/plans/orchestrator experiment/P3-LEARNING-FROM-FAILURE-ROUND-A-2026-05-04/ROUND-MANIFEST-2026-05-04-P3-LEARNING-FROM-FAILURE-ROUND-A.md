# Round Manifest — P3 Learning From Failure — Round A
# Date: 2026-05-04

## Round Identity

- Experiment: P3 — Learning From Transition Failure
- Round: A (oracle documents)
- Date: 2026-05-04
- Status: complete

## Purpose

Round A produces the five oracle documents required before Round B can
implement the deterministic P3 learning-from-failure proof.

P3 extends P2 (T34 Phase 3 to Phase 4 transition experiment). It proves
that a failed transition produces a specific learned discriminator that
does not over-generalize.

## Oracle Documents Produced

| File | Purpose |
|---|---|
| P3-SOURCE-BINDING-v1.md | Defines the three sub-experiments and their source bindings |
| P3-FAILURE-DISCRIMINATOR-v1.md | Document 1 — how BLOCK differs from PASS_WITH_GAPS at the predicate level |
| P3-LEARNING-CYCLE-TAXONOMY-v1.md | Document 2 — what the learning cycle captures from a BLOCK finding |
| P3-EXPECTED-TRACE-v1.md | Document 3 — oracle for all three sub-experiment expected traces |
| P3-HUMAN-RESULT-STANDARDS-v1.md | Document 4 — required human-facing output for each sub-experiment |
| P3-EXECUTOR-CONTRACT-v1.md | Document 5 — Round B pass gates, metrics, required tests, and constraints |

## Three Sub-Experiments

### P3-C — Corrupted Run

Input: Phase 3 upstream context packet (same as T34) plus corrupted downstream
domain packet set (9 packets, td06 BaseService absent).

Expected output: BLOCK verdict, LostConstraintCount = 1, attributed to
BaseService predicate.

### P3-L — Learning Cycle

Input: P3-C BLOCK finding.

Expected output: FeedbackRecord P3-L-FB-001 (named BaseService dropped
predicate) and LearnedDiscriminator P3-L-DISC-001 (BaseService-specific,
no generalization).

### P3-V — Verification Run

Input: Phase 3 upstream context packet plus original T34 uncorrupted domain
packet set (10 packets, td06 present as concern).

Expected output: PASS_WITH_GAPS (same as T34), DiscriminatorActivated false,
ConcernCount 2, BlockCount 0.

## Overall P3 Pass Gates

| Gate | Required result |
|---|---|
| GATE-P3-1 | P3-C OverallVerdict is BLOCK |
| GATE-P3-2 | P3-C LostConstraintCount ≥ 1, attributed to BaseService predicate |
| GATE-P3-3 | P3-C cross-phase context concern is not converted to block |
| GATE-P3-4 | P3-L FeedbackRecordsAdded = 1, named BaseService dropped predicate |
| GATE-P3-5 | P3-L DiscriminatorsAdded = 1, specificity BaseService only, no generalization |
| GATE-P3-6 | P3-V OverallVerdict is PASS_WITH_GAPS |
| GATE-P3-7 | P3-V ConcernCount = 2, BlockCount = 0 |
| GATE-P3-8 | P3-V DiscriminatorActivated is false |

## Dependency on T34

P3 shares the upstream PhaseContextPacket and the uncorrupted domain packet
set with T34. Round B must import T34's BuildPhaseContextPacket and
BuildDomainPackets methods or reproduce their exact output.

The P3-C corrupted set is derived by removing td06_base_service_constraint
from the T34 domain packet set. No other packets change.

## Round B Prerequisites

Before Round B begins:

1. All five Round A documents must be accepted.
2. T34 (P2) must be complete with all tests green.
3. The T34 BuildPhaseContextPacket and BuildDomainPackets methods are
   available as the uncorrupted baseline.

## What Round A Does Not Produce

- No learning-set pairs are produced in Round A.
- No C# code is produced in Round A.
- No test files are produced in Round A.

Round B is responsible for all implementation.
