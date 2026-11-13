# Layer 0-A — Compiler Pairs (057)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: GCS (Google Cloud Storage) connector

---

## B17-0A-057-001 — GCS: predicate firing

**Input:** "Build the GCS (Google Cloud Storage) connector for blob upload and signed URL generation."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Note: GCS (Google Cloud Storage) and "blob" are domain nouns — no predicates.
Integration vocabulary present: connector → IMPLEMENT routes to 4.3

---

## B17-0A-057-002 — GCS: "integrate" alone

**Input:** "Integrate the Google Cloud Storage JSON API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent.

---

## B17-0A-057-003 — GCS: service account token fires SECURITY

**Input:** "Implement the GCS connector with service account access token for bucket operations."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-057-004 — GCS: OAuth + token

**Input:** "Build the GCS connector with OAuth 2.0 service account token exchange."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
DecisionCount: 4 | UniqueSubPhases: 3

---

## B17-0A-057-005 — GCS: "signed URL" alone does not fire SECURITY

**Input:** "Build the GCS connector to generate signed URLs for temporary object access."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |

Note: "signed URL" alone does NOT fire SECURITY (Rule 6 requires "token"; Rule 8 requires HMAC validation phrase). Signed URLs are not token compounds.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
DecisionCount: 2 | UniqueSubPhases: 2
