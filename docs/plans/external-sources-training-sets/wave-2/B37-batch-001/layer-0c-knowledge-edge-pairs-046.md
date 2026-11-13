# Layer 0-C — Knowledge-Edge Pairs (046)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-046-001 — Knowledge-edge: v6 corruptions specific to knowledge_edge

**v6-01 (invented predicate, missed knowledge_edge):**
- "Run escalation process." → produces ESCALATION predicate → v6-01 (correct: Predicates: [] → knowledge_edge)
- "Manage scope creep." → produces SCOPE_MANAGEMENT → v6-01 (correct: Predicates: [] → knowledge_edge)

**v6-02 (DOCUMENT suppressed, went to knowledge_edge incorrectly):**
- "Send escalation update to stakeholders." → routes to knowledge_edge (DOCUMENT suppressed) → v6-02 (correct: DOCUMENT fires → DocumentationFlow)

**v6-03 (DomainId error — knowledge_edge path):**
- "Design governance structure." → routes to knowledge_edge with DomainId "project-management" → v6-03 (correct: ARCHITECTURE fires → ArchitectureFlow31 + DomainId "software-development")

---

## B37-0C-046-002 — Knowledge-edge: "post-mortem" + "change control board" + "steering committee"

**Input:** "Run post-mortem, coordinate change control board, and present findings to steering committee."

All gap vocabulary → knowledge_edge.
DomainId: "project-management".
SCL: emitted.
GapIds: manage-delivery-001 (×3), coordinate-001.

**Contrast:** "Present findings to steering committee and stakeholders." → DOCUMENT fires → NOT knowledge_edge.

---

## B37-0C-046-003 — Knowledge-edge: "portfolio management" + "program increment" + "SAFe"

**Input:** "Manage portfolio management, coordinate SAFe, and run PI planning."

All gap vocabulary → knowledge_edge.
DomainId: "project-management".
SCL: emitted.

**Contrast:** "Manage portfolio, coordinate SAFe, and build PI tracking service." → IMPLEMENT fires → NOT knowledge_edge.

---

## B37-0C-046-004 — Knowledge-edge: "project closure" + "waterfall" + "benefits realization"

**Input:** "Manage waterfall project closure and track benefits realization."

All gap vocabulary → knowledge_edge.
DomainId: "project-management".
SCL: emitted.

**Contrast:** "Manage waterfall project closure, track benefits realization, and build the KPI dashboard." → IMPLEMENT fires.

---

## B37-0C-046-005 — Knowledge-edge: final gap ID reference for all rounds

**All gap IDs emitted by knowledge_edge for B37 vocabulary:**

Primary: track-001, coordinate-001, blockers-001, escalation-001, accountability-001, follow-up-001, action-items-001, track-status-001, rag-status-001, raid-log-001, progress-001, status-report-001, manage-delivery-001, work-stream-001, dependencies-team-001, stakeholder-update-001
Compound: coordinate-blockers-001, escalation-accountability-001, track-progress-001

All gap IDs prefixed: gap-project-managing-{ID}
