# Layer 0-B — Router Pairs (007)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: router

---

## B14-router-007-001 — ARCHITECTURE+DOCUMENT: TransitionType documentation

A: ["ARCHITECTURE", "DOCUMENT"] | verbs: "design" + "document/diagram"
B: → 3.5 + 6.2 | TransitionType: documentation
  Teaching: When IMPLEMENT is absent, even multi-predicate requests with ARCHITECTURE receive TransitionType: documentation.

---

## B14-router-007-002 — ARCHITECTURE+IMPLEMENT: TransitionType feature_addition

A: ["ARCHITECTURE", "IMPLEMENT"] | verbs: "architect" + "implement"
B: → 3.5 + 4.1 | TransitionType: feature_addition
  Teaching: IMPLEMENT presence drives TransitionType to feature_addition regardless of ARCHITECTURE.

---

## B14-router-007-003 — ARCHITECTURE+IMPLEMENT+DOCUMENT: TransitionType feature_addition

A: ["ARCHITECTURE", "IMPLEMENT", "DOCUMENT"] | verbs: "design" + "implement" + "document"
B: → 3.5 + 4.1 + 6.2 | TransitionType: feature_addition
  Teaching: When IMPLEMENT is present, TransitionType = feature_addition even with DOCUMENT co-occurring.

---

## B14-router-007-004 — ARCHITECTURE+QA+DOCUMENT → 3 flows

A: ["ARCHITECTURE", "QA", "DOCUMENT"]
B: → 3.5 + 7.3 + 6.2 | TransitionType: documentation (no IMPLEMENT)
  Teaching: Three predicates, three flows. No IMPLEMENT → TransitionType: documentation.

---

## B14-router-007-005 — ARCHITECTURE+IMPLEMENT+TEST+DOCUMENT → 4 flows

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT"]
B: → 3.5 + 4.1 + 5.1 + 6.2 | TransitionType: feature_addition
  Teaching: Full IaC cycle: design (3.5), implement (4.1), test (5.1), document (6.2). Four concurrent flows.

