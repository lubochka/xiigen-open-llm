# Layer 0-C — Knowledge-Edge Pairs (028)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-028-001 — MonthlyRelease context vs predicate boundary

### Monthly release terms that are pure context
| Term | Category | Notes |
|---|---|---|
| monthly release | Cadence noun | No predicate |
| release train | Delivery concept | No predicate |
| release schedule | Planning noun | No predicate |
| release window | Time window | No predicate |
| production environment | Infrastructure | No predicate |
| staging environment | Infrastructure | No predicate |
| deployment slot | Infrastructure | No predicate |
| release plan | Planning document | No predicate alone |

### Monthly release terms that fire predicates
| Term | Predicate | Notes |
|---|---|---|
| review | QA | Fires regardless of "monthly" qualifier |
| approve | QA | Fires regardless of "monthly" qualifier |
| sign-off | QA | Rule 23 — fires in all cadence contexts |
| criteria | DOCUMENT | Rule 3 — "monthly release criteria" fires DOCUMENT |
| service | IMPLEMENT | Rule 2 — "monthly service review" fires IMPLEMENT |
| build | IMPLEMENT | Rule 6 — "build monthly artifacts" fires IMPLEMENT |
| security | SECURITY | Fires in "monthly security audit" |

---

## B38-0C-028-002 — QuarterlyRelease knowledge edge boundary

### Inputs that produce knowledge_edge
| Input | Analysis |
|---|---|
| "Plan the quarterly release timeline." | "Plan" not in lexicon; "timeline" is context → [] |
| "Assign the Q3 release to the sprint." | "Assign" not in lexicon → [] |
| "Set the quarterly release cutover date." | "Set" not in lexicon; "cutover" → GAP → [] + warning |
| "Manage the quarterly release calendar." | "Manage" not in lexicon → [] |

All: knowledge_edge when no QA/IMPLEMENT/DOCUMENT/SECURITY/ARCHITECTURE trigger present.

---

## B38-0C-028-003 — deployment in monthly/quarterly sentences (rule 4)

Rule 4 (B38): "deployment" in operational context does NOT fire IMPLEMENT.
"deployment" fires CapabilityGapWarning: gap-project-delivery-deployment-001

| Sentence fragment | IMPLEMENT fires? | Gap fires? |
|---|---|---|
| "Approve the monthly deployment" | NO | YES (deployment) |
| "Review the deployment readiness" | NO | YES (deployment) |
| "Validate the deployment artifacts" | NO | YES (deployment) |
| "Build the deployment pipeline" | YES (build fires IMPLEMENT) | YES (deployment) |

Note: "Build the deployment pipeline" fires IMPLEMENT from "build" (rule 6), and "deployment" ALSO fires the gap warning. Both are present simultaneously.
