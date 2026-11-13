# Layer 0-C — Knowledge-Edge Pairs (051)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-051-001 — Knowledge-edge: "escalation process" + "risk mitigation" combined

1. "Run escalation process and manage risk mitigation." → knowledge_edge | escalation-001 + raid-log-001
2. "Coordinate escalation and risk mitigation activities." → knowledge_edge | coordinate-001
3. "Implement escalation and risk mitigation services." → NOT knowledge_edge (IMPLEMENT fires)
4. "Document escalation and risk mitigation for stakeholders." → NOT knowledge_edge (DOCUMENT fires)

---

## B37-0C-051-002 — Knowledge-edge: "communication plan" + "post-mortem" + "lessons learned"

1. "Prepare communication plan, run post-mortem, coordinate lessons learned." → knowledge_edge
2. "Circulate communication plan and share post-mortem findings." → knowledge_edge
3. "Write communication plan and document post-mortem for stakeholders." → NOT knowledge_edge (DOCUMENT fires)
4. "Build communication and post-mortem tracking platform." → NOT knowledge_edge (IMPLEMENT fires)

---

## B37-0C-051-003 — Knowledge-edge: "steering committee" + "change control board" + "programme board"

1. "Coordinate steering committee, CCB, and programme board." → knowledge_edge
2. "Escalate to steering committee, CCB, and programme board." → knowledge_edge | escalation-001
3. "Escalate to steering committee, CCB, programme board, and stakeholders." → NOT knowledge_edge (stakeholders fires DOCUMENT)
4. "Document steering committee, CCB, and programme board decisions for stakeholders." → NOT knowledge_edge

---

## B37-0C-051-004 — Knowledge-edge: "project kickoff" + "sign-off process" + "project closure"

1. "Facilitate project kickoff, manage sign-off process, and run project closure." → knowledge_edge
2. "Coordinate project kickoff and sign-off activities." → knowledge_edge | coordinate-001
3. "Document project kickoff, sign-off, and closure for stakeholders." → NOT knowledge_edge (DOCUMENT fires)
4. "Review project kickoff and sign-off effectiveness." → NOT knowledge_edge (QA fires)

---

## B37-0C-051-005 — Knowledge-edge: "scope creep" + "change control board" + "risk mitigation"

1. "Monitor scope creep, coordinate CCB, manage risk mitigation." → knowledge_edge
2. "Escalate scope creep to CCB and manage risk mitigation." → knowledge_edge | escalation-001
3. "Define scope boundaries, CCB criteria, and risk mitigation plan." → NOT knowledge_edge (DOCUMENT fires)
4. "Build scope, CCB, and risk mitigation tracking service." → NOT knowledge_edge (IMPLEMENT fires)
