# Held-Out and Corruption Pairs (023)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-023-001 — HO: managing + ARCHITECTURE (accountability structure)

**Request:** "Map the accountability structure and coordinate delivery accountability."

**Expected output:**
- "Map" → not in predicate lexicon
- "accountability structure" → "structure" fires ARCHITECTURE
- "accountability" → gap
- "coordinate" → not in predicate lexicon
- "delivery accountability" → gap
- Predicates: ["ARCHITECTURE"]
- DomainId: "software-development"
- Route: ArchitectureFlow31 (3.1)
- Gap IDs: gap-project-managing-accountability-001, gap-project-managing-coordinate-001, gap-project-managing-manage-delivery-001

---

## B37-HO-023-002 — HO: managing without structure → zero predicates

**Request:** "Coordinate delivery accountability across all programme workstreams."

**Expected output:**
- "Coordinate" → not in predicate lexicon
- "delivery accountability" → gap (no "structure")
- "programme workstreams" → gap
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: coordinate-001, accountability-001, work-stream-001, manage-delivery-001

---

## B37-HO-023-003 — COR: mixed managing + IMPLEMENT → MANAGEMENT predicate added (v6-01)

**Request:** "Build the escalation tracking service and maintain the risk log."

**v6-01 corrupt output:** Predicates: ["IMPLEMENT", "MANAGEMENT"] — MANAGEMENT predicate invented

**Why this is wrong:** No MANAGEMENT predicate exists. Only IMPLEMENT fires (on "build" + "service"). Managing vocabulary (escalation tracking, risk log) → gap warnings only.

**Correct output:** Predicates: ["IMPLEMENT"], DomainId: "software-development", Gap IDs: escalation-001, track-001, raid-log-001

---

## B37-HO-023-004 — HO: three predicates from one managing sentence

**Request:** "Define the accountability structure, build the tracking service, and document the decision log."

**Expected output:**
- "Define" → DOCUMENT fires
- "accountability structure" → "structure" → ARCHITECTURE fires
- "build" → IMPLEMENT fires
- "tracking service" → "service" → IMPLEMENT fires (combined: one IMPLEMENT)
- "document" → DOCUMENT fires (combined: one DOCUMENT)
- "decision log" → gap
- Predicates: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]
- DomainId: "software-development"
- Routes: 2.1 + 3.1 + 4.1

---

## B37-HO-023-005 — HO: mixed managing + code certification summary

| Case | Request | Expected predicates | Expected DomainId |
|------|---------|-------------------|-----------------|
| HO-023-001 | "Map accountability structure" | [ARCHITECTURE] | software-development |
| HO-023-002 | "Coordinate accountability" | [] | project-management |
| HO-023-003 | "Build tracking service + maintain risk log" | [IMPLEMENT] | software-development |
| HO-023-004 | "Define accountability structure + build service" | [DOCUMENT, ARCHITECTURE, IMPLEMENT] | software-development |
| HO-023-005 | "Review accountability structure" | [QA, ARCHITECTURE] | software-development |

All five must pass. Zero v6 corruptions permitted.
