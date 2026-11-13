# WDA-01 Executor Contract
# Document to sub-phase classification
# Round A - Implementation instructions for Round B
# Date: 2026-05-04

---

## Round B Scope

Implement a deterministic WDA-01 classification proof.

One thing is under test: the system can classify one source document or source
packet into each of the 31 canonical sub-phases with a measurable confidence
and wrong-domain guard.

Do not run within-phase orchestration in this round. Do not write broad corpus
pairs in this round.

---

## Required Source Preparation

For each probe in `WDA-01-SOURCE-DOCUMENT-SELECTION-LEDGER.md`:

1. Try to locate a ready local source document.
2. If found, record its path and mark `source_status = ready`.
3. If not found, create a hand-authored source packet and mark
   `source_status = knowledge_edge`.
4. Confirm three verifiable claims.
5. Confirm one known failure mode.
6. Confirm expected top-three predicates.

The hand-authored packet is allowed only for deterministic routing-shape proof.
It cannot count toward corpus quota.

---

## Required Deterministic Test

Recommended name:

```text
WDA01_DocumentClassification_ClassifiesAllCanonicalSubPhases
```

The test should:

1. Load or construct the 31 probe inputs.
2. Route each probe through the classification surface available in the current
   experiment harness.
3. Assert phase id and sub-phase id against
   `WDA-01-EXPECTED-CLASSIFICATION-TRACE.md`.
4. Assert ready-source confidence is at least 0.70.
5. Assert nearest wrong sub-phase score is below 0.50.
6. Assert missing source is represented as a knowledge edge.
7. Produce a human-readable classification report.

If the real `sub-phase-router` unit does not exist yet, use deterministic
fixture outputs but write the failure classification as:

```text
runtime_gap: sub-phase-router unit not yet implemented
```

This keeps WDA-01 Round B honest: it can prove the oracle and source ledger,
but it cannot claim trained router replacement until the unit exists.

---

## Required Evidence

Round B must produce:

- JSON evidence
- markdown human result
- classification confusion table
- round manifest
- round zip

The JSON evidence must contain:

```text
classification_count
correct_phase_count
correct_sub_phase_count
min_confidence
max_wrong_domain_score
wrong_domain_high_confidence_count
source_knowledge_edge_count
e2e_classification_success
```

---

## Forbidden Moves

- Do not count hand-authored packets toward broad corpus quota.
- Do not reduce the probe count from 31 to 27.
- Do not hide missing source documents.
- Do not patch the oracle to match a wrong classifier output.
- Do not proceed to WDA-02 until WDA-01 has a reviewed Round B result.
- Do not generate broad training pairs before failure classification.

---

## Learning Output Rule

If WDA-01 fails, write only the minimum pairs needed for the failing unit:

- wrong phase: `sub-phase-router` contrast pair
- wrong sub-phase: `sub-phase-router` local-neighbor pair
- low confidence: one original plus one phrasing B for the same sub-phase
- high wrong-domain score: wrong-domain guard pair
- missing source: no model pair; source acquisition task

No downstream node pairs are allowed until classification is stable.

---

## Next Action After Passing

Create WDA-02 Phase 3 Round A artifacts:

1. source packet for technical design within-phase orchestration
2. expected trace covering sub-phases 3.1 through 3.5
3. human result checklist
4. executor contract

Phase 3 is first because it has the strongest existing source coverage and is
the center of the development lifecycle transition chain.
