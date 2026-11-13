# Layer 0-A — Compiler Pairs (056)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: AWS S3 connector

---

## B17-0A-056-001 — AWS S3: predicate firing

**Input:** "Build the AWS S3 (Simple Storage Service) object storage connector for file upload and retrieval."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Note: S3 (Simple Storage Service) is a product acronym — fires no predicate.
Integration vocabulary present: connector → IMPLEMENT routes to 4.3

---

## B17-0A-056-002 — AWS S3: "integrate" alone

**Input:** "Integrate the AWS S3 REST API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent — storage domain has no exception.

---

## B17-0A-056-003 — AWS S3: presigned URL token fires SECURITY

**Input:** "Implement the AWS S3 connector with presigned URL access token for secure object downloads."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6) |

Note: "presigned URL" alone does not fire SECURITY; "token" in "access token" does.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-056-004 — AWS S3: "upload" is not IMPLEMENT

**Input:** "Upload files to the S3 bucket and trigger a Lambda function on object creation."

Triggers: none ("upload" and "trigger" are not IMPLEMENT triggers)
CompiledPredicateIds: []
TransitionType: none

---

## B17-0A-056-005 — AWS S3: STS token + OAuth

**Input:** "Build the AWS S3 connector with STS (Security Token Service) temporary access token and OAuth role assumption."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| token (in STS temporary access token) | SECURITY (Rule 6) |
| OAuth | AUTHORIZATION |

Note: STS (Security Token Service) — expansion contains "Token" → SECURITY fires. "OAuth" → AUTHORIZATION.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
DecisionCount: 4 | UniqueSubPhases: 3
