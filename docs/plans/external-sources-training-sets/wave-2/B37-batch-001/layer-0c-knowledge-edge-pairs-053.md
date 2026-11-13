# Layer 0-C — Knowledge-Edge Pairs (053)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-053-001 — Knowledge-edge: "quality gate" + "sign-off" + "benefits realization"

1. "Enforce quality gates, manage sign-off, track benefits realization." → knowledge_edge
2. "Coordinate quality gate, sign-off, and benefits activities." → knowledge_edge | coordinate-001
3. "Review quality gate and sign-off for benefits realization." → NOT knowledge_edge (QA fires)
4. "Define quality gate, sign-off, and benefits criteria." → NOT knowledge_edge (DOCUMENT fires)

---

## B37-0C-053-002 — Knowledge-edge: "risk mitigation" + "change control board" + "scope creep"

1. "Manage risk mitigation, coordinate CCB, monitor scope creep." → knowledge_edge
2. "Escalate risk mitigation and scope creep to CCB." → knowledge_edge | escalation-001
3. "Build risk mitigation, CCB, and scope control service." → NOT knowledge_edge (IMPLEMENT fires)
4. "Define risk mitigation and scope creep criteria for CCB." → NOT knowledge_edge (DOCUMENT fires)

---

## B37-0C-053-003 — Knowledge-edge: "communication plan" + "steering committee" + "programme board"

1. "Prepare communication plan and present to steering committee and programme board." → knowledge_edge
2. "Circulate communication plan to steering committee." → knowledge_edge
3. "Circulate communication plan to steering committee and stakeholders." → NOT knowledge_edge (stakeholders fires DOCUMENT)
4. "Document communication plan for steering committee and stakeholders." → NOT knowledge_edge

---

## B37-0C-053-004 — Knowledge-edge: "waterfall" + "project closure" + "post-mortem"

1. "Manage waterfall project closure and run post-mortem." → knowledge_edge
2. "Coordinate waterfall closure and post-mortem activities." → knowledge_edge | coordinate-001
3. "Document waterfall closure and post-mortem for stakeholders." → NOT knowledge_edge
4. "Review waterfall closure and post-mortem findings." → NOT knowledge_edge (QA fires)

---

## B37-0C-053-005 — Knowledge-edge: "agile transformation" + "SAFe" + "PI" + "portfolio" + "value stream"

1. "Manage agile transformation, SAFe adoption, PI planning, portfolio, and value streams." → knowledge_edge
2. "Coordinate all agile methodology activities." → knowledge_edge | coordinate-001
3. "Design all agile methodology architecture." → NOT knowledge_edge (ARCHITECTURE fires)
4. "Implement all agile methodology tooling." → NOT knowledge_edge (IMPLEMENT fires)
