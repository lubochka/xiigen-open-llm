# Layer 0-C — Knowledge-Edge Pairs (026)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-026-001 — PreReleaseTag knowledge edge boundary

### PreReleaseTag terms: context vs predicate
| Term | Category | Notes |
|---|---|---|
| pre-release tag | Release label | Context — no predicate |
| release candidate (RC) | Release artifact | Context — no predicate |
| alpha / beta | Pre-release stages | Context — no predicate |
| tag | Labeling noun/verb | Context — no predicate alone |
| registry | Artifact storage | Context — no predicate |
| publish | Distribution verb | Context — no predicate |
| verify | QA trigger | YES (QA) |
| sign-off / sign off | QA trigger | YES (QA) — rule 23 |
| security | SECURITY trigger | YES (SECURITY) |
| build | IMPLEMENT trigger | YES (IMPLEMENT) — rule 6 |
| pipeline | IMPLEMENT trigger | YES (IMPLEMENT) — code artifact context |

---

## B38-0C-026-002 — RCCandidate knowledge edge boundary

### RCCandidate terms that do NOT fire predicates
| Term | Context category | Notes |
|---|---|---|
| release candidate | Release artifact | No predicate |
| RC | Abbreviation of release candidate | No predicate |
| candidate build | Release artifact phrase | "candidate" = context; "build" = IMPLEMENT if code-authoring verb |
| nomination | Process noun | No predicate |
| promote | Promotion action | Context; no predicate |
| freeze | Code freeze action | Context; no predicate |

### RCCandidate terms that DO fire predicates
| Term | Predicate | Rule |
|---|---|---|
| review | QA | Core |
| approve | QA | Core |
| sign-off | QA | Rule 23 |
| criteria | DOCUMENT | Rule 3 — absolute |
| build (verb) | IMPLEMENT | Rule 6 — absolute |
| service | IMPLEMENT | Rule 2 — absolute |
| security | SECURITY | Core |
| audit | QA + SECURITY (if security co-occurs) | Rule 19 |

---

## B38-0C-026-003 — Three outcomes for RC/pre-release inputs

1. FLOW_EXECUTING: Input contains review/approve/sign-off/verify (fires QA); possibly also build/service/criteria
   - Example: "Review the RC build and sign off" → PredicateSet: ["QA","IMPLEMENT"]
   
2. KNOWLEDGE_EDGE: Input contains only RC labels and planning verbs
   - Example: "Promote the RC to production" → PredicateSet: []
   
3. GAP_WITH_PREDICATES: Input contains QA trigger AND gap vocabulary
   - Example: "Approve the RC rollout" → PredicateSet: ["QA"], CapabilityGapWarnings: ["rollout"]
