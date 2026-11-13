# Layer 0-A — Compiler Pairs
# Batch: B26-batch-001
# Scenario: B26 — QA security review — security audit, vulnerability assessment, sub-phase 6.3
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: compiler

---

## Pair B26B1-WDA-compiler-001 — SECURITY + QA: security audit request

```
source_id: xiigen-b26-compiler-001
transition_type: feature_addition
phase: 6
sub_phase: 6.3
coverage_status: ready
pair_kind: compiler
```

A:
  user_request: "Audit the authentication layer for security vulnerabilities and QA the access control policies."

B:
  DocumentId: "B26-compiler-001-prompt"
  DocumentTitle: "Security audit of authentication layer and access control QA"
  CompiledText: "Audit the authentication layer for security vulnerabilities and QA the access control policies."
  CompiledPredicateIds: ["SECURITY", "QA"]
  SourceStatus: "ready"

  Predicate evidence:
    "audit" — SECURITY fires on "audit" in security context
    "authentication layer" — SECURITY fires on "authentication"
    "security vulnerabilities" — SECURITY fires on "security" + "vulnerabilities"
    "QA" — QA predicate fires on "QA" (literal abbreviation)
    "access control" — SECURITY fires on "access control"
    "policies" — not independently a predicate term
    Result: ["SECURITY", "QA"] — two predicates

  Note: "audit" fires SECURITY even without "security" being explicit, but here
  "security vulnerabilities" + "authentication" + "access control" provide dense SECURITY
  signal. QA fires independently via the literal "QA" abbreviation.

---

## Pair B26B1-WDA-compiler-002 — SECURITY only: vulnerability scan

```
source_id: xiigen-b26-compiler-002
transition_type: feature_addition
phase: 6
sub_phase: 6.3
coverage_status: ready
pair_kind: compiler
```

A:
  user_request: "Review the API endpoints for SQL injection vulnerabilities and enforce input sanitization."

B:
  DocumentId: "B26-compiler-002-prompt"
  DocumentTitle: "API security review for SQL injection and input sanitization"
  CompiledText: "Review the API endpoints for SQL injection vulnerabilities and enforce input sanitization."
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT"]
  SourceStatus: "ready"

  Predicate evidence:
    "SQL injection" — SECURITY fires on "injection" (injection attack category)
    "vulnerabilities" — SECURITY fires on "vulnerabilities"
    "enforce" — SECURITY fires on "enforce" in security context (enforce policies/rules)
    "input sanitization" — SECURITY fires on "sanitization"
    "API endpoints" — API_CONTRACT fires on "API" + "endpoints"
    "review" — AUTHORIZATION? No: "review" in security context fires SECURITY via
      co-occurrence with "vulnerabilities"
    Result: ["SECURITY", "API_CONTRACT"]

---

## Pair B26B1-WDA-compiler-003 — QA + DOCUMENT: QA report documentation

```
source_id: xiigen-b26-compiler-003
transition_type: feature_addition
phase: 6
sub_phase: 6.3
coverage_status: ready
pair_kind: compiler
```

A:
  user_request: "QA the release candidate and document all findings in the security review report."

B:
  DocumentId: "B26-compiler-003-prompt"
  DocumentTitle: "QA release candidate and document security review findings"
  CompiledText: "QA the release candidate and document all findings in the security review report."
  CompiledPredicateIds: ["QA", "DOCUMENT", "SECURITY"]
  SourceStatus: "ready"

  Predicate evidence:
    "QA" — QA predicate fires on "QA"
    "document" — DOCUMENT fires on "document" (action verb)
    "security review" — SECURITY fires on "security" + "review" (in security audit context)
    "release candidate" — not in any predicate (product management term)
    "findings" — not in any predicate
    "report" — DOCUMENT? "report" alone does not fire DOCUMENT; "document... report" → DOCUMENT via "document"
    Result: ["QA", "DOCUMENT", "SECURITY"]
