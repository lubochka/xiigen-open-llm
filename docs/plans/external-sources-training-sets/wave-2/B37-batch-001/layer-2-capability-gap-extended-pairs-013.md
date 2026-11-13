# Layer 2 — Capability-Gap-Extended Pairs (013)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-013-001 — "maintain the risk log" gap

A: "Maintain the risk log and track all identified project risks."

Predicates: []
GapId: gap-project-managing-raid-log-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-track-001

---

## B37-L2-013-002 — "risk log with escalation" gap

A: "Monitor the risk log and escalate all critical risks to the programme director."

Predicates: []
GapId: gap-project-managing-raid-log-001
DomainId: project-managing

Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Additional gap IDs: gap-project-managing-escalation-001

---

## B37-L2-013-003 — "review risk log" — QA fires, gap warning raised

A: "Review the risk log before the steering committee meeting."

Predicates: [QA]
GapId: gap-project-managing-raid-log-001
DomainId: software-development

Route: ReviewFlow (QA sub-phase)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: gap-project-managing-raid-log-001 (risk management intent not addressed by QA flow)

---

## B37-L2-013-004 — "document risk log" — DOCUMENT fires, gap warning raised

A: "Document all project risks in the risk log with probability and impact assessments."

Predicates: [DOCUMENT]
GapId: gap-project-managing-raid-log-001
DomainId: software-development

Route: DocumentationFlow (2.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: gap-project-managing-raid-log-001

---

## B37-L2-013-005 — "build risk log dashboard" — IMPLEMENT fires, gap warning raised

A: "Build a risk log dashboard to visualise all open risks and their severity ratings."

Predicates: [IMPLEMENT]
GapId: gap-project-managing-raid-log-001
DomainId: software-development

Route: BackendServiceFlow41 (4.1)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: gap-project-managing-raid-log-001
