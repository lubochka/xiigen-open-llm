# Layer 0-A — Compiler Pairs
# Batch: B17-batch-001
# Scenario: B17 — Development — integration — third-party payment connector
# Sub-phase: 4.3 (Integration and External Services)
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: compiler

---

## Pair B17B1-WDA-compiler-001 — integration build: IMPLEMENT + API_CONTRACT

```
source_id: xiigen-b17-compiler-001
transition_type: feature_addition
phase: 4
sub_phase: 4.3
pair_kind: compiler
```

A:
  user_request: "Build the Stripe payment integration connector — implement the charge, refund, and webhook handlers."

B:
  DocumentId: "B17-compiler-001-prompt"
  DocumentTitle: "Stripe payment integration connector with charge, refund, and webhook handling"
  CompiledText: "Build the Stripe payment integration connector — implement the charge, refund, and webhook handlers."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  SourceStatus: "ready"

Predicate evidence:
  "build" → IMPLEMENT; "implement" → IMPLEMENT; "handlers" → IMPLEMENT (handler ≈ function/class)
  "integration" → API_CONTRACT (with no explicit "api" co-occurrence — but "connector" is in API_CONTRACT)
  "webhook" → API_CONTRACT fires on "webhook"
  "connector" → API_CONTRACT fires on "connector" (external service connector = API contract term)
  Result: ["IMPLEMENT", "API_CONTRACT"]

---

## Pair B17B1-WDA-compiler-002 — integration with spec: IMPLEMENT + API_CONTRACT + DOCUMENT

```
source_id: xiigen-b17-compiler-002
transition_type: feature_addition
phase: 4
sub_phase: 4.3
pair_kind: compiler
```

A:
  user_request: "Integrate the SendGrid email API and document the request interface for sending transactional emails."

B:
  DocumentId: "B17-compiler-002-prompt"
  DocumentTitle: "SendGrid email API integration and request interface documentation"
  CompiledText: "Integrate the SendGrid email API and document the request interface for sending transactional emails."
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "DOCUMENT"]
  SourceStatus: "ready"

Predicate evidence:
  "integrate" → API_CONTRACT (with "api" co-occurrence)
  "API" → API_CONTRACT
  "document" → DOCUMENT
  "interface" → API_CONTRACT (with API co-occurrence)
  "sending" — not directly in any predicate; but "integrate" + "api" covers IMPLEMENT? No — "integrate" fires API_CONTRACT, not IMPLEMENT
  Need IMPLEMENT: Does "integrate" fire IMPLEMENT? No. IMPLEMENT fires on: implement, build, code, write, develop, create, function, class, method, logic, module, service. "integrate" is API_CONTRACT only.
  But: does any IMPLEMENT term fire here? "sending" — no. So IMPLEMENT does NOT fire here.

  Correction: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  "integrate" → API_CONTRACT; "API" → API_CONTRACT; "interface" → API_CONTRACT; "document" → DOCUMENT; "sending" → nothing.
  IMPLEMENT does not fire — "integrate" is an API_CONTRACT term, not an IMPLEMENT term.

Corrected B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  CapabilityGapWarnings: []

Teaching point: "integrate" fires API_CONTRACT, NOT IMPLEMENT. The developer intent
may be to write integration code, but the compiler fires on the term "integrate" which
belongs to the API_CONTRACT lexicon. To additionally fire IMPLEMENT, the request must
contain "build", "implement", "write", "code", "create", "function", "class", "service", etc.

---

## Pair B17B1-WDA-compiler-003 — connector with security: IMPLEMENT + API_CONTRACT + SECURITY

```
source_id: xiigen-b17-compiler-003
transition_type: feature_addition
phase: 4
sub_phase: 4.3
pair_kind: compiler
```

A:
  user_request: "Build the OAuth 2.0 connector for the third-party identity provider and implement the token exchange flow."

B:
  DocumentId: "B17-compiler-003-prompt"
  DocumentTitle: "OAuth 2.0 third-party identity provider connector with token exchange"
  CompiledText: "Build the OAuth 2.0 connector for the third-party identity provider and implement the token exchange flow."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
  SourceStatus: "ready"

Predicate evidence:
  "build" → IMPLEMENT
  "connector" → API_CONTRACT
  "implement" → IMPLEMENT
  "token" → SECURITY fires on "token"
  "OAuth" → AUTHORIZATION fires on "oauth"
  "identity provider" — "identity" not directly in AUTHORIZATION; but "oauth" is
  "exchange" — not in any predicate
  Result: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
