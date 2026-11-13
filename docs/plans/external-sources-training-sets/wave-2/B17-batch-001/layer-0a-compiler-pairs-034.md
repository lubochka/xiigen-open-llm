# Layer 0-A — Compiler Pairs (034)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Salesforce CRM connector

---

## B17-0A-034-001 — Salesforce: build connector

**Input:** "Build the Salesforce CRM connector with REST API endpoint and webhook integration."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| endpoint | API_CONTRACT (reinforces; fires once) |
| webhook | API_CONTRACT (reinforces; fires once) |
| integration | API_CONTRACT (reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Integration vocabulary: connector, endpoint, webhook → IMPLEMENT routes to 4.3
"CRM" acronym: no predicate.

---

## B17-0A-034-002 — Salesforce: integrate

**Input:** "Integrate Salesforce into the lead management workflow."

Triggers: "Integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation

---

## B17-0A-034-003 — Salesforce: build with OAuth token

**Input:** "Build Salesforce connector with OAuth 2.0 connected app token flow."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
DecisionCount: 4 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)

---

## B17-0A-034-004 — Salesforce: implement (no integration vocab)

**Input:** "Implement Salesforce lead scoring algorithm."

Triggers: "Implement" → IMPLEMENT
No integration vocabulary → IMPLEMENT routes to 4.1
CompiledPredicateIds: ["IMPLEMENT"]
"Salesforce" proper noun: no predicate. "lead scoring algorithm": no predicate.
DecisionCount: 1 | UniqueSubPhases: 1 (4.1)

---

## B17-0A-034-005 — Salesforce: create tests

**Input:** "Build Salesforce connector and create tests for the opportunity sync endpoint."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| endpoint | API_CONTRACT (reinforces; fires once) |
| create tests | TEST |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST"]
DecisionCount: 3 | UniqueSubPhases: 3 (4.3, 3.3, 5.1)
