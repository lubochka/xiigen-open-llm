# Layer 0-C — Knowledge-Edge Pairs (039)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-039-001 — Knowledge-edge: "waterfall" + "agile transformation" combined

1. "Transition from waterfall to agile transformation." → knowledge_edge | manage-delivery-001 (×2)
2. "Coordinate waterfall and agile transformation workstreams." → knowledge_edge | coordinate-001 + work-stream-001
3. "Track waterfall and agile transformation progress." → knowledge_edge | track-001 + manage-delivery-001
4. "Design waterfall and agile transformation architecture." → NOT knowledge_edge (ARCHITECTURE fires)

---

## B37-0C-039-002 — Knowledge-edge: "SAFe" + "program increment" combined

1. "Coordinate SAFe program increment ceremonies." → knowledge_edge | coordinate-001 + manage-delivery-001
2. "Run SAFe PI planning session." → knowledge_edge | manage-delivery-001
3. "Track SAFe PI objectives." → knowledge_edge | track-001 + manage-delivery-001
4. "Define SAFe PI acceptance criteria." → NOT knowledge_edge (DOCUMENT fires on "define" + "criteria")

---

## B37-0C-039-003 — Knowledge-edge: "portfolio management" + "value stream" combined

1. "Manage portfolio and map value streams." → knowledge_edge | manage-delivery-001 (×2)
2. "Coordinate portfolio management and value stream activities." → knowledge_edge | coordinate-001
3. "Track portfolio and value stream performance." → knowledge_edge | track-001
4. "Design portfolio and value stream architecture." → NOT knowledge_edge (ARCHITECTURE fires)

---

## B37-0C-039-004 — Knowledge-edge: "project closure" + "benefits realization" + "KPI"

1. "Manage project closure, track benefits realization, monitor KPI." → knowledge_edge | manage-delivery-001 (×3)
2. "Coordinate project closure and benefits realization activities." → knowledge_edge | coordinate-001
3. "Document project closure and benefits KPIs for stakeholders." → NOT knowledge_edge (DOCUMENT fires)
4. "Build project closure and KPI dashboard service." → NOT knowledge_edge (IMPLEMENT fires)

---

## B37-0C-039-005 — Complete B37 knowledge-edge boundary summary (all rounds 002-060)

**knowledge_edge triggers (zero predicates):**
Any and all B37 PM managing vocabulary from any round, used without: review, validate, define, document, write, spec, describe, criteria, stakeholder, structure, design, architecture, build, create, implement, service, module

**NOT knowledge_edge — always fires predicate:**
- "work breakdown structure" → ARCHITECTURE
- "handover document" → DOCUMENT
- Any vocabulary + stakeholder → DOCUMENT
- Any vocabulary + structure/design/architecture → ARCHITECTURE
- Any vocabulary + build/create/implement/service/module → IMPLEMENT
- Any vocabulary + review/validate → QA
- Any vocabulary + define/document/criteria/spec → DOCUMENT
