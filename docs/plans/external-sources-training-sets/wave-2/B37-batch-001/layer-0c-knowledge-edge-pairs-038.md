# Layer 0-C — Knowledge-Edge Pairs (038)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-038-001 — Knowledge-edge: "communication plan" + "project kickoff" combined

1. "Prepare communication plan for project kickoff." → knowledge_edge | manage-delivery-001
2. "Coordinate communication plan and kickoff activities." → knowledge_edge | coordinate-001 + manage-delivery-001
3. "Run project kickoff and circulate communication plan." → knowledge_edge | manage-delivery-001 (×2)
4. "Document communication plan and kickoff agenda for stakeholders." → NOT knowledge_edge (DOCUMENT fires)

---

## B37-0C-038-002 — Knowledge-edge: "scope creep" + "change control board" combined

1. "Monitor scope creep and coordinate CCB." → knowledge_edge | manage-delivery-001 + coordinate-001
2. "Escalate scope creep to CCB." → knowledge_edge | escalation-001 + manage-delivery-001
3. "Track scope creep and CCB decisions." → knowledge_edge | track-001 + manage-delivery-001
4. "Define scope and CCB criteria." → NOT knowledge_edge (DOCUMENT fires on "define")

---

## B37-0C-038-003 — Knowledge-edge: "risk mitigation" + "quality gate" combined

1. "Run risk mitigation and enforce quality gates." → knowledge_edge | raid-log-001 + manage-delivery-001
2. "Coordinate risk mitigation and quality gate activities." → knowledge_edge | coordinate-001
3. "Track risk mitigation and quality gate status." → knowledge_edge | track-001
4. "Review risk mitigation and quality gate effectiveness." → NOT knowledge_edge (QA fires)

---

## B37-0C-038-004 — Knowledge-edge: "sign-off process" + "post-mortem" combined

1. "Manage sign-off and run post-mortem." → knowledge_edge | manage-delivery-001 (×2)
2. "Coordinate sign-off and post-mortem activities." → knowledge_edge | coordinate-001
3. "Track sign-off and post-mortem action items." → knowledge_edge | track-001 + action-items-001
4. "Document sign-off and post-mortem for stakeholders." → NOT knowledge_edge (DOCUMENT fires)

---

## B37-0C-038-005 — Knowledge-edge: boundary when "programme board" added

**"programme board" ≠ stakeholders:**
1. "Escalate to programme board." → knowledge_edge
2. "Escalate to stakeholders." → NOT knowledge_edge (DOCUMENT fires)
3. "Escalate to programme board and steering committee." → knowledge_edge (both = committee names)
4. "Escalate to programme board and stakeholders." → NOT knowledge_edge (stakeholders fires DOCUMENT)

Key: committee names are gap vocabulary; "stakeholders" always fires DOCUMENT.
