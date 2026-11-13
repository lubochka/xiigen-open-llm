# Layer 0-A — Compiler Pairs (024)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-024-001 — Complete B38 predicate reference: QA extended lexicon

This file is a complete predicate reference summary for B38 sub-phase 7.1 (Release and Deployment Automation — Quality Assurance (QA)).

### Extended QA Lexicon
| Term | Context | Notes |
|---|---|---|
| review | Any release context | Core QA trigger |
| approve / approval / approved | Any context | Core QA trigger |
| verify / verification | Any context | Core QA trigger |
| validate / validation | Any context | Core QA trigger |
| accept / acceptance | Quality/gate context | Fires QA |
| quality | Quality context | Fires QA |
| audit | Release context | QA; in security context also fires SECURITY (rule 19) |
| sign-off / sign off / sign-offs | All grammatical forms | Always fires QA (rule 23) |
| compliance | Release context | Fires QA |
| inspect | Any context | Fires QA |
| readiness | When combined with "gate" or "review" | Fires QA |
| gate (as verb: "gate the release") | Action context | Fires QA |

---

## B38-0A-024-002 — Complete B38 predicate reference: IMPLEMENT, DOCUMENT, SECURITY, GAP

### IMPLEMENT Lexicon (fires IMPLEMENT)
| Term | Rule | Notes |
|---|---|---|
| service | Rule 2 — ABSOLUTE | Even in release review sentences |
| build | Rule 6 — ABSOLUTE | Even "build artifacts", "build provenance" |
| pipeline | Code artifact context | CI/CD configuration is code |
| implementation | Support term | Code-authoring context |
| module | Code-authoring context | Not in pure process sentences |
| component | Code-authoring context | Not in pure governance sentences |

### DOCUMENT Lexicon (fires DOCUMENT)
| Term | Rule | Notes |
|---|---|---|
| criteria | Rule 3 — ABSOLUTE | All qualifiers: release, acceptance, exit, entry, readiness, completion |
| document / documentation / documenting | Any context | Fires DOCUMENT |

### SECURITY Lexicon (fires SECURITY)
| Term | Rule | Notes |
|---|---|---|
| security | Any context | Fires SECURITY |
| vulnerability | SECURITY domain | |
| audit + security co-occurrence | Rule 19 | Both QA and SECURITY fire |

### B38 Gap Vocabulary (CapabilityGapWarning only — no predicate)
| Term | Gap ID |
|---|---|
| go-live | gap-project-delivery-go-live-001 |
| checklist | gap-project-delivery-checklist-001 |
| rollout | gap-project-delivery-rollout-golive-001 |
| cutover | gap-project-delivery-cutover-001 |
| runbook | gap-project-delivery-runbook-001 |
| launch | gap-project-delivery-launch-001 |
| on-call | gap-operational-on-call |
| monitor | Rule 7 — not in predicate lexicon |
| execute / execution context | Rule 8 — not IMPLEMENT |
| deployment (operational) | Rule 4 — not IMPLEMENT without code co-occurrence |
| handoff | Operational delivery term, no predicate |

---

## B38-0A-024-003 — Complete TransitionType determination table

| PredicateSet | TransitionType | Notes |
|---|---|---|
| [] | N/A (knowledge_edge) | Zero predicates → knowledge_edge, not GapHandler |
| ["QA"] | no_contribution | QA alone = governance only |
| ["DOCUMENT"] | no_contribution | Documentation = no code change |
| ["SECURITY"] | no_contribution | Security audit = no code change |
| ["ARCHITECTURE"] | no_contribution | Architecture design = no code change |
| ["QA","DOCUMENT"] | no_contribution | No IMPLEMENT |
| ["QA","SECURITY"] | no_contribution | No IMPLEMENT |
| ["QA","ARCHITECTURE"] | no_contribution | No IMPLEMENT |
| ["QA","DOCUMENT","SECURITY"] | no_contribution | No IMPLEMENT |
| ["QA","IMPLEMENT"] | feature_addition | IMPLEMENT present |
| ["QA","IMPLEMENT","DOCUMENT"] | feature_addition | IMPLEMENT present |
| ["QA","IMPLEMENT","SECURITY"] | feature_addition | IMPLEMENT present |
| ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | feature_addition | IMPLEMENT present |
| ["QA","IMPLEMENT","DOCUMENT","SECURITY","ARCHITECTURE"] | feature_addition | IMPLEMENT present |

Rule: TransitionType = feature_addition if and only if IMPLEMENT is in the predicate set.

---

## B38-0A-024-004 — QA ordering rule and DecisionCount rules

QA ordering rule (Rule 12 — absolute):
- QA is always last in any multi-predicate pipeline
- Canonical order for all four non-QA predicates: IMPLEMENT → DOCUMENT → SECURITY → ARCHITECTURE → QA
- When only QA is present: trivially satisfied (one predicate, one decision)

DecisionCount rule:
- DecisionCount = len(PredicateSet)
- Each predicate generates exactly one independent routing decision
- Multi-predicate sets must NOT be collapsed (v6-03 corruption)

Anti-corruption rules:
- v6-01: QA must not be suppressed by co-present gap vocabulary
- v6-02: Probationary flows must not execute when Gate B fails
- v6-03: Multi-predicate sets must not be collapsed to single decision
