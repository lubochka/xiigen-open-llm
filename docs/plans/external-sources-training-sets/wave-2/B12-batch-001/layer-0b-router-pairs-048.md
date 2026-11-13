# Layer 0-B — Router Pairs (048)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router

---

## B12-0B-048-001 — API_CONTRACT+SECURITY combo: [API_CONTRACT, DOCUMENT, SECURITY] → 3.3 + 2.1 + 3.4

```
source_id: xiigen-b12-0b-048-001
pair_kind: router
```

A:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  UserRequest: "Define the API contract for the payment service including API key authentication and OAuth2 token validation."

B:
  Routes: ["3.3", "2.1", "3.4"]
  PrimarySubPhase: "3.3"
  DecisionCount: 3
  TransitionType: "documentation"
  FlowOrder:
    1. APIContractFlow33
    2. parallel(DocumentationFlow21, SecurityAuditFlow34)
    3. QAFlow
  v6 check: "api" + "contract" → 3.3 ✓ | "define" → 2.1 ✓ | "authentication" + "oauth2" → ONE SECURITY → 3.4 ✓
  SECURITY multiplicity: authentication + oauth2 = 1 SECURITY predicate → 1 routing decision to 3.4

---

## B12-0B-048-002 — API_CONTRACT+SECURITY combo: [IMPLEMENT, API_CONTRACT, SECURITY] → 4.3 + 3.3 + 3.4

```
source_id: xiigen-b12-0b-048-002
pair_kind: router
```

A:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
  UserRequest: "Implement the secure REST API endpoint with OAuth2 bearer token authentication for the admin panel."

B:
  Routes: ["4.3", "3.3", "3.4"]
  PrimarySubPhase: "4.3"
  DecisionCount: 3
  TransitionType: "feature_addition"
  FlowOrder:
    parallel(ImplementationFlow43, APIContractFlow33, SecurityAuditFlow34) → QAFlow
  v6 check: "implement" → 4.3 ✓ | "rest" + "api" + "endpoint" → 3.3 ✓ | "oauth2" + "authentication" → ONE SECURITY → 3.4 ✓

---

## B12-0B-048-003 — Round 3 routing summary: files 037–048

```
source_id: xiigen-b12-0b-048-003
pair_kind: router_reference
```

Round 3 routing patterns (files 037–048):

| Topic | Predicates | Routes | TransitionType |
|-------|-----------|--------|----------------|
| RateLimitingAPI (define) | API_CONTRACT+DOCUMENT | 3.3+2.1 | documentation |
| RateLimitingAPI (implement) | IMPLEMENT+API_CONTRACT | 4.3+3.3 | feature_addition |
| RateLimitingAPI (with auth) | API_CONTRACT+DOCUMENT+SECURITY | 3.3+2.1+3.4 | documentation |
| CachingAPI (design) | ARCHITECTURE+API_CONTRACT+DOCUMENT | 3.1+3.3+2.1 | documentation |
| IdempotencyAPI (expose) | API_CONTRACT | 3.3 | documentation |
| VersionedEndpoint (design) | ARCHITECTURE+API_CONTRACT | 3.1+3.3 | documentation |
| DeprecatedEndpoint (update) | API_CONTRACT+DOCUMENT | 3.3+2.1 | documentation |
| APIKeyManagement (implement+auth) | IMPLEMENT+API_CONTRACT+SECURITY | 4.3+3.3+3.4 | feature_addition |
| OAuthScopeAPI (implement) | IMPLEMENT+API_CONTRACT+SECURITY | 4.3+3.3+3.4 | feature_addition |
| TenantScopedAPI (expose) | API_CONTRACT | 3.3 | documentation |
| MultiVersionAPI (build+doc) | IMPLEMENT+API_CONTRACT+DOCUMENT | 4.3+3.3+2.1 | feature_addition |
| BackwardCompatAPI | API_CONTRACT+DOCUMENT | 3.3+2.1 | documentation |
| APIDocGenerator (build) | IMPLEMENT+API_CONTRACT+DOCUMENT | 4.3+3.3+2.1 | feature_addition |
| API_CONTRACT+SECURITY combo | API_CONTRACT+DOCUMENT+SECURITY | 3.3+2.1+3.4 | documentation |

---

## B12-0B-048-004 — SECURITY multiplicity routing rule reference

```
source_id: xiigen-b12-0b-048-004
pair_kind: router_reference
```

SECURITY multiplicity rule (B12-R27): multiple SECURITY triggers in one request produce ONE
routing decision to 3.4, not multiple decisions.

Correct:
  "authentication" + "OAuth2" → CompiledPredicateIds includes "SECURITY" once
  DecisionCount for [API_CONTRACT, DOCUMENT, SECURITY] = 3 (not 4)
  Routes = ["3.3", "2.1", "3.4"] (not ["3.3", "2.1", "3.4", "3.4"])

Corrupt (v6-03 variant):
  DecisionCount = 4 (treating each SECURITY trigger as a separate decision)
  Routes = ["3.3", "2.1", "3.4", "3.4"] (wrong — SECURITY routes once)
