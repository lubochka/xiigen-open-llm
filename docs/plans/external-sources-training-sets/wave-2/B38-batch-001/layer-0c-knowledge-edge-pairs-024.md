# Layer 0-C — Knowledge-Edge Pairs (024)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-024-001 — Complete gap vs predicate boundary reference for B38

This file is a complete gap/predicate boundary reference for B38 sub-phase 7.1.

### Terms that fire predicates (never gap)
| Term | Predicate | Rule |
|---|---|---|
| review | QA | Core lexicon |
| approve / approval / approved | QA | Core lexicon |
| verify / verification | QA | Rule 20/21 |
| validate / validation | QA | Core lexicon |
| accept / acceptance | QA | Quality context |
| quality | QA | Core lexicon |
| audit | QA (+ SECURITY if "security" present) | Rule 19 |
| sign-off / sign off / sign-offs | QA | Rule 23 |
| compliance | QA | Core lexicon |
| inspect | QA | Core lexicon |
| service | IMPLEMENT | Rule 2 — ABSOLUTE |
| build | IMPLEMENT | Rule 6 — ABSOLUTE |
| pipeline | IMPLEMENT | Code artifact context |
| criteria | DOCUMENT | Rule 3 — ABSOLUTE |
| document / documentation | DOCUMENT | Core lexicon |
| security | SECURITY | Core lexicon |
| vulnerability | SECURITY | Core lexicon |
| architecture | ARCHITECTURE | Design context |

### Terms that are always gap (never predicate)
| Term | Gap ID | Rule |
|---|---|---|
| go-live | gap-project-delivery-go-live-001 | Rule 5 |
| checklist | gap-project-delivery-checklist-001 | Rule 5 |
| rollout | gap-project-delivery-rollout-golive-001 | Rule 5 |
| cutover | gap-project-delivery-cutover-001 | Rule 5 |
| runbook | gap-project-delivery-runbook-001 | Rule 5 |
| launch | gap-project-delivery-launch-001 | Rule 5 |
| on-call | gap-operational | Operational |
| monitor | (operational) | Rule 7 |
| execute / execution | (operational) | Rule 8 |
| deployment (operational) | gap-project-delivery-deployment-001 | Rule 4 |

### Terms that are context (no predicate, no gap warning)
| Term | Notes |
|---|---|
| release | Context word; no predicate alone |
| production | Infrastructure context |
| staging | Infrastructure context |
| environment | Infrastructure context |
| hotfix | Delivery noun; no predicate |
| handoff | Operational transfer; no predicate |
| remediation | Process noun; no predicate |
| Q2 / sprint / version | Time/version context |
| stakeholders / teams / leads | People context |

---

## B38-0C-024-002 — Three outcomes for B38 inputs

1. FLOW_EXECUTING: PredicateSet is non-empty; routing decisions generated; N09 produces result.
2. KNOWLEDGE_EDGE: PredicateSet is empty; all vocabulary is gap/context; no routing.
3. GAP_WITH_PREDICATES: PredicateSet is non-empty but gap vocabulary is also present; routing proceeds + gap warnings attached.

Zero predicates ALWAYS produces knowledge_edge.
knowledge_edge is NOT GapHandler.
knowledge_edge is NOT SCHEDULE_CAPABILITY_LEARNING.
knowledge_edge simply records that the input cannot be routed.
