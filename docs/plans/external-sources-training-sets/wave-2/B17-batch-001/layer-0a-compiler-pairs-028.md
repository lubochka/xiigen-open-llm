# Layer 0-A — Compiler Pairs (028)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: DocuSign e-signature connector

---

## B17-0A-028-001 — DocuSign: build connector

**Input:** "Build the DocuSign e-signature connector with Connect webhook event handling."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| webhook | API_CONTRACT | 5 (reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Integration vocabulary: connector, webhook → IMPLEMENT routes to 4.3

---

## B17-0A-028-002 — DocuSign: integrate

**Input:** "Integrate DocuSign envelope creation into the document workflow."

Triggers: "Integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
"document workflow" and "envelope" are domain nouns — no predicate.

---

## B17-0A-028-003 — DocuSign: build with OAuth and token

**Input:** "Build DocuSign connector with OAuth authorization code flow and access token refresh."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY (Rule 6; unconditional) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
DecisionCount: 4 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)
"access token refresh" — "token" fires SECURITY unconditionally.

---

## B17-0A-028-004 — DocuSign: refactor

**Input:** "Refactor the DocuSign connector to use the v2.1 eSignature REST API endpoint."

| Trigger | Predicate |
|---|---|
| Refactor | IMPLEMENT (Rule 14; TransitionType=refactoring) |
| connector | API_CONTRACT |
| endpoint | API_CONTRACT (reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: refactoring
DecisionCount: 2 | UniqueSubPhases: 2 (4.3, 3.3)
