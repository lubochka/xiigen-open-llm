# Held-Out and Corruption Pairs (024)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-024-001 — HO: complete gap vocabulary + boundaries — mixed held-out

**Request:** "Track the risk log, coordinate change requests, follow up on lessons-learned action items, escalate blockers to the programme board, and maintain the decision log."

**Expected output:**
- All tokens gap vocabulary → no predicates
- "programme board" ≠ stakeholder → DOCUMENT does NOT fire
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: raid-log-001, manage-delivery-001 (×3), track-001, coordinate-001, follow-up-001, action-items-001, escalation-001, blockers-001

---

## B37-HO-024-002 — HO: replace "programme board" with "stakeholders" → DOCUMENT fires

**Request:** "Track the risk log, coordinate change requests, follow up on action items, and escalate blockers to stakeholders."

**Expected output:**
- Same as HO-024-001 except "stakeholders" → DOCUMENT fires
- Predicates: ["DOCUMENT"]
- DomainId: "software-development"
- Route: DocumentationFlow (2.1)
- SCL: NOT emitted

**Contrast with HO-024-001:** One token change ("programme board" → "stakeholders") changes DomainId from "project-management" to "software-development" and routing from knowledge_edge to 2.1.

---

## B37-HO-024-003 — HO: extended vocabulary certification test (full)

**Request:** "Maintain risk log, process change requests, circulate project charter, update decision log, run lessons-learned, coordinate accountability, and map team dependencies."

**Expected output:**
- All tokens gap vocabulary
- "map" → not in predicate lexicon
- "team dependencies" → no API co-occurrence → gap
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted (once)
- Gap IDs: raid-log-001, manage-delivery-001 (×4), coordinate-001, accountability-001, dependencies-team-001, work-stream-001

---

## B37-HO-024-004 — COR: comprehensive v6 corruption summary for extended vocabulary

**v6-01 examples for extended vocabulary:**
- "Process change request" → Predicates: ["CHANGE_MANAGEMENT"] — invented predicate
- "Run lessons-learned" → Predicates: ["RETROSPECTIVE"] — invented predicate
- "Coordinate accountability" → Predicates: ["COORDINATION"] — invented predicate

All three: v6-01. Correct output: Predicates: [] for all three.

**v6-02 examples:**
- "Update stakeholders on risk log" → DOCUMENT suppressed — v6-02
- "Notify stakeholders of change request" → DOCUMENT suppressed — v6-02

**v6-03 examples:**
- "Define accountability structure" → DomainId "project-management" (ARCHITECTURE fired) — v6-03
- "Notify stakeholders" → DomainId "project-management" (DOCUMENT fired) — v6-03

---

## B37-HO-024-005 — HO: extended B37 final certification summary (all 12 new vocabulary families)

| Vocabulary | Zero-predicate request | Expected predicates | Expected DomainId |
|-----------|----------------------|-------------------|-----------------|
| risk log alone | "Maintain the risk log" | [] | project-management |
| change request alone | "Process the change request" | [] | project-management |
| project charter alone | "Circulate the project charter" | [] | project-management |
| decision log alone | "Update the decision log" | [] | project-management |
| lessons learned alone | "Run lessons-learned session" | [] | project-management |
| accountability alone | "Establish accountability" | [] | project-management |
| team dependencies alone | "Map team dependencies" | [] | project-management |
| + stakeholder | any above + "stakeholders" | [DOCUMENT] | software-development |
| + structure | any above + "structure" | [ARCHITECTURE] | software-development |
| + review | any above + "review" | [QA] | software-development |
| + API + dependencies | "API dependencies" | [API_CONTRACT] | software-development |
| + build/service/module | any above + code token | [IMPLEMENT] | software-development |

All 12 rows must produce correct output. Zero v6 corruptions permitted. Gate B for extended vocabulary.
