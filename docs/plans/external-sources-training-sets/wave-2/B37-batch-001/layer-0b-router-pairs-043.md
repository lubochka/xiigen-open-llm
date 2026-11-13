# Layer 0-B — Router Pairs (043)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-043-001 — gap_interruption for Round 3+4 vocabulary

**Scenario:** User in active DocumentationFlow (2.1), issues PM request:
"Coordinate the change control board and track scope creep."

**Transition orchestrator:**
- TransitionType: gap_interruption
- ContextCarryForward: true
- Next state: knowledge_edge
- DomainId: "project-management"
- SCL: emitted
- After gap: DocumentationFlow (2.1) resumable

---

## B37-0B-043-002 — gap_interruption for Round 4 vocabulary in flow context

**Scenario:** User in BackendServiceFlow41 (4.1), issues PM request:
"Manage the agile transformation and coordinate SAFe adoption."

**Transition orchestrator:**
- TransitionType: gap_interruption
- ContextCarryForward: true
- Next state: knowledge_edge
- DomainId: "project-management"
- SCL: emitted
- After gap: BackendServiceFlow41 (4.1) context resumable

---

## B37-0B-043-003 — Combined Round 3+4 routing confirmation

**Long zero-predicate sentence spanning both rounds:**
"Manage escalation, coordinate change control board, track waterfall delivery, manage agile transformation, run SAFe ceremonies, and oversee program increment planning."

All gap vocabulary:
- Route: knowledge_edge
- DomainId: "project-management"
- SCL: emitted (once)
- GapIds: escalation-001, manage-delivery-001 (×3), coordinate-001, track-001 forwarded

---

## B37-0B-043-004 — v6 routing corruptions for Round 3+4

**v6-01 (invented factory):**
- "Manage scope creep" → ScopeCreepFlow selected — v6-01
- "Run post-mortem" → PostMortemFlow selected — v6-01
- "Coordinate SAFe" → SAFeFlow selected — v6-01
Correct: knowledge_edge for all three.

**v6-02 (DOCUMENT suppressed):**
- "Send communication plan to stakeholders" → routes to knowledge_edge (DOCUMENT suppressed) — v6-02
Correct: DocumentationFlow (2.1) — DOCUMENT fires on "stakeholders".

**v6-03 (wrong DomainId):**
- "Design waterfall structure" → DomainId "project-management" — v6-03
Correct: DomainId "software-development" — ARCHITECTURE fired.

---

## B37-0B-043-005 — Complete Round 3+4 routing behaviour summary

1. All Round 3a+4 new vocabulary → zero predicates when used alone → knowledge_edge
2. False positives unchanged: stakeholder → DOCUMENT; structure → ARCHITECTURE; review → QA; build/service → IMPLEMENT
3. "work breakdown structure" → ARCHITECTURE always (even without additional tokens)
4. "handover document" → DOCUMENT (on "document" token)
5. DomainId unchanged: zero predicates → "project-management"; any predicate → "software-development"
6. SCL rule unchanged: zero predicates only
7. No new factories added in Round 3 or Round 4
