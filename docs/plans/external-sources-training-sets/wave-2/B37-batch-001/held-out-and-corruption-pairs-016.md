# Held-Out and Corruption Pairs (016)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-016-001 — HO: decision log update → knowledge_edge

**Request:** "Update the decision log after each steering committee meeting."

**Expected output:**
- "Update" → not in DOCUMENT lexicon
- "decision log" → gap
- "steering committee" → not in any predicate lexicon
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-accountability-001

---

## B37-HO-016-002 — COR: "log" fires DOCUMENT (wrong)

**Request:** "Log all key project decisions and track follow-up actions."

**v6 corrupt output:** Predicates: ["DOCUMENT"] — "log" fires DOCUMENT

**Why this is wrong:** "log" is NOT in the DOCUMENT lexicon. DOCUMENT fires on: document, spec, define, describe, requirement, story, criteria, stakeholder, functional, behavior, use-case. "log" is not on this list.

**Correct output:** [] (zero predicates). DomainId "project-management". Note: "track" also fires nothing (Rule 2).

---

## B37-HO-016-003 — HO: document architectural decisions → DOCUMENT + ARCHITECTURE

**Request:** "Document all architectural decisions in the decision log with their rationale."

**Expected output:**
- "Document" → DOCUMENT fires
- "architectural" → ARCHITECTURE fires (adjective form of architecture)
- "decisions" → gap
- "decision log" → gap
- Predicates: ["DOCUMENT", "ARCHITECTURE"]
- DomainId: "software-development"
- Routes: DocumentationFlow + ArchitectureFlow31

---

## B37-HO-016-004 — COR: DomainId "project-management" for ARCHITECTURE sentence (v6-03)

**Request:** "Map the decision-making structure for the programme board."

**v6-03 corrupt output:** Predicates: ["ARCHITECTURE"], DomainId: "project-management"

**Why this is wrong:** ARCHITECTURE fired (on "structure"). Any predicate → DomainId "software-development" (Rule 14).

**Correct output:** Predicates: ["ARCHITECTURE"], DomainId: "software-development"

---

## B37-HO-016-005 — HO: decision log held-out certification summary

| Case | Request | Expected predicates | Expected DomainId |
|------|---------|-------------------|-----------------|
| HO-016-001 | "Update decision log" | [] | project-management |
| HO-016-002 | "Log key decisions" | [] | project-management |
| HO-016-003 | "Track open decisions" | [] | project-management |
| HO-016-004 | "Document decisions" | [DOCUMENT] | software-development |
| HO-016-005 | "Document architectural decisions" | [DOCUMENT, ARCHITECTURE] | software-development |

All five must pass. Zero v6 corruptions permitted.
