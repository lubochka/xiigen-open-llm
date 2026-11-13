# Layer 0-A — Compiler Pairs (034)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-034-001 — Complete QA reference for build cadence scenarios

This file is a complete predicate reference for nightly, weekly, monthly, and quarterly build cadence scenarios.

### QA fires from cadence release sentences
| Trigger term | Context | Notes |
|---|---|---|
| review | Any cadence release | Core QA trigger |
| approve / approval | Any cadence approval | Core QA trigger |
| verify / verification | Any cadence validation | Core QA trigger |
| validate | Any cadence check | Core QA trigger |
| sign-off / sign off | Any cadence sign-off | Rule 23 — absolute |
| quality | Quality gate context | Core QA trigger |
| compliance | Compliance check | Core QA trigger |
| inspect | Artifact inspection | Core QA trigger |
| accept / acceptance | Acceptance gate | Quality context |

### QA does NOT fire from cadence nouns
| Non-trigger term | Category |
|---|---|
| nightly | Cadence adjective |
| weekly | Cadence adjective |
| monthly | Cadence adjective |
| quarterly | Cadence adjective |
| schedule | Planning noun |
| cadence | Process noun |
| window | Time window noun |

---

## B38-0A-034-002 — IMPLEMENT triggers in cadence build sentences

| Input term | Fires IMPLEMENT? | Notes |
|---|---|---|
| service | YES — rule 2 | Any service reference, any cadence |
| build | YES — rule 6 | "nightly build" where "build" is a code-authoring verb |
| pipeline | YES | CI/CD pipeline context |
| script | NO alone | Script noun alone does not fire; needs co-occurring "build" or "service" |
| automation | NO alone | Automation noun alone does not fire |

Build as scheduling noun vs build as code-authoring verb:
- "Schedule the nightly build" → "build" is noun in scheduling context; no predicate
- "Build the nightly pipeline" → "Build" is code-authoring verb; fires IMPLEMENT

---

## B38-0A-034-003 — Gap vocabulary in cadence release sentences

| Term | Gap ID | Fires predicate? |
|---|---|---|
| rollout | gap-project-delivery-rollout-golive-001 | NO |
| cutover | gap-project-delivery-cutover-001 | NO |
| go-live | gap-project-delivery-go-live-001 | NO |
| launch | gap-project-delivery-launch-001 | NO |
| deploy / deployment (operational) | gap-project-delivery-deployment-001 | NO |
| monitor | (operational, rule 7) | NO |
| schedule | (planning noun, no predicate) | NO |

---

## B38-0A-034-004 — DecisionCount and ordering rule for cadence scenarios

| PredicateSet | DecisionCount | QA position | TransitionType |
|---|---|---|---|
| ["QA"] | 1 | 1 (last and only) | no_contribution |
| ["QA","IMPLEMENT"] | 2 | 2 (last) | feature_addition |
| ["QA","DOCUMENT"] | 2 | 2 (last) | no_contribution |
| ["QA","SECURITY"] | 2 | 2 (last) | no_contribution |
| ["QA","ARCHITECTURE"] | 2 | 2 (last) | no_contribution |
| ["QA","IMPLEMENT","DOCUMENT"] | 3 | 3 (last) | feature_addition |
| ["QA","IMPLEMENT","SECURITY"] | 3 | 3 (last) | feature_addition |
| ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | 4 | 4 (last) | feature_addition |
