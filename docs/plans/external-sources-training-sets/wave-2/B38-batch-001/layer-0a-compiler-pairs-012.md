# Layer 0-A — Compiler Pairs (012)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: compiler

---

## B38-0A-012-001 — Complete B38 predicate rule table

This file is a reference summary. All pairs in Layer 0-A apply these rules.

### QA Lexicon (fires QA predicate)
| Term | Notes |
|---|---|
| review | Always fires QA regardless of context |
| approve / approved / approval | Always fires QA |
| verify / verification | Always fires QA (rules 20, 21) |
| validate | Always fires QA |
| accept / acceptance | QA when in quality/gate context |
| quality | Fires QA |
| audit | QA in release context; QA+SECURITY in security context (rule 19) |
| sign-off / sign off | Always fires QA (rule 23) |
| compliance | Fires QA (also SECURITY support in security context) |
| inspect | Fires QA |

### IMPLEMENT Lexicon (fires IMPLEMENT predicate)
| Term | Notes |
|---|---|
| service | IMPLEMENT ALWAYS — even in release review sentences (rule 2) |
| build | IMPLEMENT always — even in "build artifacts", "build provenance" (rule 6) |
| pipeline | IMPLEMENT as code artifact |
| implementation | IMPLEMENT context support |
| code | IMPLEMENT context support |
| module | IMPLEMENT context support |
| component | IMPLEMENT when code-authoring context; not in pure process sentences |

### DOCUMENT Lexicon (fires DOCUMENT predicate)
| Term | Notes |
|---|---|
| criteria | DOCUMENT ALWAYS (rule 3) — acceptance, release, exit, completion, readiness |
| document / documentation | Fires DOCUMENT |

### SECURITY Lexicon (fires SECURITY predicate)
| Term | Notes |
|---|---|
| security | Fires SECURITY |
| vulnerability | SECURITY domain |
| audit (in security context) | Fires SECURITY per rule 19 |

### B38 Gap Vocabulary (produces CapabilityGapWarning, no predicate)
| Term | Gap ID |
|---|---|
| go-live | gap-project-delivery-go-live-001 |
| checklist | gap-project-delivery-checklist-001 |
| rollout | gap-project-delivery-rollout-golive-001 |
| cutover | gap-project-delivery-cutover-001 |
| runbook | gap-project-delivery-runbook-001 |
| launch | gap-project-delivery-launch-001 |
| on-call | (operational vocabulary) |
| monitor | not in any predicate (rule 7) |
| execute (execution context) | not IMPLEMENT (rule 8) |
| deployment (operational context) | GAP (rule 4) |

---

## B38-0A-012-002 — B38 predicate boundary tests (quick reference)

The following inputs and their correct predicate sets:

"Execute the rollout" → []  (execute=gap, rollout=gap → knowledge_edge)
"Review the service" → ["QA","IMPLEMENT"]  (review=QA, service=IMPLEMENT)
"Build the service" → ["IMPLEMENT"]  (no QA terms; build+service both IMPLEMENT)
"Sign off on criteria" → ["QA","DOCUMENT"]  (sign-off=QA, criteria=DOCUMENT)
"Security audit before release" → ["QA","SECURITY"]  (audit+security together; no IMPLEMENT/DOCUMENT)
"Monitor deployment" → []  (monitor=gap, deployment=gap → knowledge_edge)
"Approve the runbook checklist" → ["QA"]  (approve=QA; runbook+checklist=gap warnings)
"Review service criteria with security audit" → ["QA","IMPLEMENT","DOCUMENT","SECURITY"]

---

## B38-0A-012-003 — Phase routing table for B38

| Predicate | Phase | Flow |
|---|---|---|
| QA | 7.1 | ReleaseGateFlow71 |
| IMPLEMENT | 4.1 | BackendServiceFlow41 |
| DOCUMENT | 2.1 | DocumentationFlow |
| SECURITY | 3.4 | SecurityAuditFlow |

QA ordering rule (absolute): QA is always last in multi-predicate pipeline.
Order for four predicates: IMPLEMENT → DOCUMENT → SECURITY → QA

---

## B38-0A-012-004 — TransitionType determination for B38

| Predicate set | TransitionType |
|---|---|
| ["QA"] only | no_contribution |
| ["QA","DOCUMENT"] | no_contribution (DOCUMENT = documentation phase, not feature_addition) |
| ["QA","SECURITY"] | no_contribution (SECURITY = security phase, not feature_addition) |
| ["QA","IMPLEMENT"] | feature_addition |
| ["QA","IMPLEMENT","DOCUMENT"] | feature_addition |
| ["QA","IMPLEMENT","SECURITY"] | feature_addition |
| ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | feature_addition |
| [] | N/A (knowledge_edge — no TransitionType assigned) |

Rule: feature_addition requires IMPLEMENT to be in the predicate set. Any combination with IMPLEMENT = feature_addition. Without IMPLEMENT, result is no_contribution (or no_contribution with documentation for DOCUMENT sets).
