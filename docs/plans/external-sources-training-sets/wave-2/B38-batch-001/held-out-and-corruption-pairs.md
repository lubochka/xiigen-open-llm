# Held-Out and Corruption Pairs
# Batch: B38-batch-001
# Scenario: B38 — Project delivery — release gates and go-live checklists
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05

---

## Held-Out B38B1-HO-001 — "sign-off" fires QA

```
source_id: xiigen-b38-ho-001
expected_outcome: QA fires on sign-off → routes to 7.1
```

A: "Collect the sign-offs for the release and confirm the launch is approved."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  Routing: "7.1"
  CapabilityGapWarnings: ["gap-project-delivery-launch-001: 'launch' not captured"]

Predicate analysis: "sign-off" → QA fires. "launch" → gap. "approved" → QA fires.
CompiledPredicateIds: ["QA"]. Routing → 7.1.

---

## Held-Out B38B1-HO-002 — pure cutover/runbook: gap

```
source_id: xiigen-b38-ho-002
expected_outcome: knowledge_edge (cutover and runbook have no predicate)
```

A: "Execute the cutover runbook and restore service if the rollback threshold is breached."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  KnownFailureMode: "no_predicate_match_capability_gap"
  capability_gap_ids: ["gap-project-delivery-cutover-001", "gap-project-delivery-runbook-001"]

Predicate analysis: "execute" → not in IMPLEMENT; "cutover" → gap; "runbook" → gap;
"restore" → not in IMPLEMENT; "rollback" → gap; "threshold" → not in any lexicon.
Result: zero predicates → knowledge_edge.

---

## Held-Out B38B1-HO-003 — "audit" fires QA in delivery context

```
source_id: xiigen-b38-ho-003
expected_outcome: QA fires on audit → routes to 7.1
```

A: "Audit the release artifacts and verify the build provenance before approving deployment."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  Routing: "7.1"
  CapabilityGapWarnings: ["gap-project-delivery-build-provenance-001"]

Predicate analysis: "audit" → QA fires; "verify" → QA fires; "approve" → QA fires.
"build provenance" — "build" IS in IMPLEMENT → IMPLEMENT fires!
Correction: CompiledPredicateIds: ["QA", "IMPLEMENT"]. Two routing decisions: 7.1 + 4.1.

---

## Corruption B38B1-COR-001 — v6-01: QA suppressed for "checklist" request

```
source_id: xiigen-b38-cor-001
corruption_type: "Corruption-v6-01: knowledge edge suppression — QA fired on review/approve suppressed"
```

CORRUPT B: CompiledPredicateIds: [] for "Review the release checklist and approve the go-live."

Why corrupt: "review" and "approve" are in the QA lexicon. The corrupt model suppressed
QA because the surrounding context (checklist, go-live) is project delivery vocabulary.
The compiler is literal — QA fires on the predicate terms regardless of surrounding gap vocabulary.

Correct B: CompiledPredicateIds: ["QA", "IMPLEMENT"], SourceStatus: "ready".

---

## Corruption B38B1-COR-002 — v6-02: probationary flow accepted despite Gate B failure

```
source_id: xiigen-b38-cor-002
corruption_type: "Corruption-v6-02: probationary flow accepted without Gate B passage"
```

CORRUPT B: SelectedFlowId: "release-gate-flow-7.1-candidate", TrustState: "probationary",
FallbackUsed: false — despite gate_b_passed: false (6/9 nodes covered).

Why corrupt: Gate B passage is required before a probationary flow can be selected.
6/9 nodes covered does not meet the Gate B threshold (all 9 nodes, min confidence 0.88,
minimum evidence pairs). The fallback must be used.

Correct B: SelectedFlowId: "universal-nine-node-development-flow-7.1",
TrustState: "deterministic_generic", FallbackUsed: true,
SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED".

---

## Corruption B38B1-COR-003 — v6-03: multi-predicate routing collapsed to single decision

```
source_id: xiigen-b38-cor-003
corruption_type: "Corruption-v6-03: multi-branch result collapse — QA+IMPLEMENT collapsed to single 7.1 decision"
```

CORRUPT B: DecisionCount: 1, only 7.1 decision returned for [QA, IMPLEMENT] input.

Why corrupt: Two predicates fired (QA and IMPLEMENT). The router must produce two
independent routing decisions. Collapsing to a single decision loses the IMPLEMENT→4.1
routing. The model must produce DecisionCount: 2 with both 7.1 and 4.1 decisions.

Correct B: DecisionCount: 2 — RoutingDecisions[0]: 7.1 (QA), RoutingDecisions[1]: 4.1 (IMPLEMENT).
