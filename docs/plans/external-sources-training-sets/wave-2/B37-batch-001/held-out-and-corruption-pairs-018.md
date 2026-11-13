# Held-Out and Corruption Pairs (018)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-018-001 — HO: accountability coordination → knowledge_edge

**Request:** "Assign RACI and establish accountability for all delivery workstream leads."

**Expected output:**
- "Assign" → not in any predicate lexicon
- "RACI" → not in any predicate lexicon
- "establish" → not in any predicate lexicon
- "accountability" → not in any predicate lexicon (gap)
- "delivery workstream leads" → gap
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: gap-project-managing-accountability-001, gap-project-managing-work-stream-001, gap-project-managing-manage-delivery-001

---

## B37-HO-018-002 — COR: "accountability structure" → DomainId "project-management" (v6-03)

**Request:** "Map the accountability structure for the programme board."

**v6-03 corrupt output:** Predicates: ["ARCHITECTURE"], DomainId: "project-management"

**Why this is wrong:** ARCHITECTURE fired (on "structure"). Any predicate → DomainId "software-development" (Rule 14). v6-03 is the error.

**Correct output:** Predicates: ["ARCHITECTURE"], DomainId: "software-development"

---

## B37-HO-018-003 — HO: "define accountability structure" → DOCUMENT + ARCHITECTURE

**Request:** "Define the accountability structure and RACI for the programme delivery."

**Expected output:**
- "Define" → DOCUMENT fires
- "accountability" → gap
- "structure" → ARCHITECTURE fires
- "RACI" → gap
- Predicates: ["DOCUMENT", "ARCHITECTURE"]
- DomainId: "software-development"
- Routes: DocumentationFlow + ArchitectureFlow31
- Gap IDs: gap-project-managing-accountability-001

---

## B37-HO-018-004 — COR: ACCOUNTABILITY predicate invented (v6-01)

**Request:** "Coordinate accountability and escalation across all workstreams."

**v6-01 corrupt output:** Predicates: ["ACCOUNTABILITY"] — new predicate invented

**Why this is wrong:** No ACCOUNTABILITY predicate exists in any predicate lexicon. The correct output is [] (zero predicates). v6-01 is the error of inventing a predicate.

**Correct output:** Predicates: [], DomainId: "project-management", knowledge_edge, SCL emitted.

---

## B37-HO-018-005 — HO: accountability held-out certification summary

| Case | Request | Expected predicates | Expected DomainId |
|------|---------|-------------------|-----------------|
| HO-018-001 | "Establish accountability" | [] | project-management |
| HO-018-002 | "Coordinate RACI" | [] | project-management |
| HO-018-003 | "Map accountability structure" | [ARCHITECTURE] | software-development |
| HO-018-004 | "Define accountability structure" | [DOCUMENT, ARCHITECTURE] | software-development |
| HO-018-005 | "Build RACI matrix" | [IMPLEMENT] | software-development |

All five must pass. Zero v6 corruptions permitted.
