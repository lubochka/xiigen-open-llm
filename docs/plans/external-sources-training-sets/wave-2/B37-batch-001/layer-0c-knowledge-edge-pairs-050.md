# Layer 0-C — Knowledge-Edge Pairs (050)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-050-001 — Knowledge-edge: complete boundary table for Round 4b (049-060)

| Vocabulary | KE? | Exception trigger |
|-----------|-----|-----------------|
| waterfall alone | Yes | structure/design → No |
| agile transformation alone | Yes | define/implement → No |
| SAFe alone | Yes | implement/design → No |
| program increment alone | Yes | define/criteria → No |
| portfolio management alone | Yes | structure/design → No |
| value stream alone | Yes | architecture/design → No |
| flow state alone | Yes | build/service → No |
| work breakdown structure | No | "structure" always fires |
| WBS abbreviation alone | Yes | (only abbreviation) |
| project closure alone | Yes | document/review → No |
| benefits realization alone | Yes | build/define → No |
| KPI monitoring alone | Yes | build/design → No |

---

## B37-0C-050-002 — Knowledge-edge: "WBS abbreviation" is the sole abbreviation boundary in B37

All other B37 abbreviations behave identically to the spelled-out form:
- "SAFe" = gap (same as "Scaled Agile Framework")
- "PI" = gap (same as "program increment")
- "CCB" = gap (same as "change control board")
- "PM" = gap (same as "project managing" or "project management")
- "RACI" = gap (same as accountability framework)
- "RAG" = gap (same as RAG-status)

ONLY "WBS" differs: "WBS" = gap; "work breakdown structure" = ARCHITECTURE (via "structure").

---

## B37-0C-050-003 — Knowledge-edge: "SAFe" + "PI" all abbreviations zero-predicate

1. "Coordinate SAFe PI ceremonies." → knowledge_edge (both abbreviations → gap)
2. "Manage SAFe PI delivery." → knowledge_edge
3. "Track SAFe PI metrics." → knowledge_edge
4. "Design SAFe PI governance structure." → NOT knowledge_edge (ARCHITECTURE fires on "structure")

---

## B37-0C-050-004 — Knowledge-edge: "portfolio" alone vs "portfolio structure"

1. "Manage portfolio management." → knowledge_edge
2. "Design portfolio structure." → NOT knowledge_edge (ARCHITECTURE fires on "structure")
3. "Manage portfolio structure." → NOT knowledge_edge (ARCHITECTURE fires on "structure")

Rule: "structure" fires ARCHITECTURE regardless of which PM noun precedes it.

---

## B37-0C-050-005 — Knowledge-edge: confirmation of Layer 0-C complete scope

Layer 0-C knowledge-edge files (002-060) teach:
1. Which PM vocabulary is gap (zero predicate → knowledge_edge)
2. Boundary conditions for each vocabulary family
3. Verb-level boundaries ("write" vs "prepare"; "review" vs "monitor")
4. Gap ID assignment for each vocabulary family
5. SCL emission rule (one per request)
6. DomainId correlation (zero predicates ↔ "project-management")
7. Committee name ≠ stakeholder token rule
8. "WBS abbreviation" gap vs "work breakdown structure" ARCHITECTURE exception
