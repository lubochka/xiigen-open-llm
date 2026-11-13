# Layer 0-C — Knowledge Edge Pairs (048) — QA+IMPLEMENT combo knowledge edge reference
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E048-001 — IMPLEMENT trigger vocabulary (complete list for Round 3)

build, implement, create service, develop service, add service, construct service, write service, make service + any "service" noun trigger
  Teaching: Any of these fires IMPLEMENT. All deduplicate to one predicate.

---

## E048-002 — QA trigger vocabulary (complete list, all positions)

QA as verb ("QA this", "QA the build"), QA as noun modifier ("QA sign-off", "QA gate", "QA approve", "QA review"), QA as subject ("QA blocks release"), QA as parenthetical ("requires QA") → all fire QA
  Teaching: Any occurrence of the "QA" abbreviation in any syntactic position fires QA.

---

## E048-003 — IMPLEMENT+QA canonical output

["IMPLEMENT", "QA"] → feature_addition | canonical flow order: ImplementationFlow41 then ReleaseGateFlow71
  Teaching: IMPLEMENT+QA always outputs feature_addition. Canonical predicate and flow order is fixed.

---

## E048-004 — QA alone vs IMPLEMENT+QA TransitionType contrast

QA alone: no TransitionType | IMPLEMENT+QA: feature_addition
  Teaching: Only IMPLEMENT triggers feature_addition. QA is always gate-only.

---

## E048-005 — "quality assurance" spelled out does NOT fire QA

"quality assurance review" → gap | "ensure quality" → gap | "quality check" → gap
  Teaching: Spelled-out quality vocabulary is gap. Only the "QA" abbreviation fires the QA predicate.
