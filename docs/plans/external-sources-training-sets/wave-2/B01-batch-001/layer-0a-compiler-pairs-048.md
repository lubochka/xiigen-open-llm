# Layer 0-A — Compiler Pairs (048)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-048-001 — UI+SECURITY combo: "build a login form with XSS protection" fires UI+IMPLEMENT+SECURITY

```
source_id: xiigen-b01-0a-048-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a login form component with input validation, XSS protection, and CSRF token integration."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "SECURITY"]
  Confidence: 0.94

Predicate evidence:
  UI fires on: "form", "input", "component"
  IMPLEMENT fires on: "build"
  SECURITY fires on: "XSS", "CSRF", "token", "validation" (security context)
  Three predicates → three independent routing decisions.
  TransitionType: feature_addition (IMPLEMENT present).

---

## B01-0A-048-002 — UI+SECURITY: "design secure file upload widget" fires UI+IMPLEMENT+SECURITY

```
source_id: xiigen-b01-0a-048-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create a file upload widget with client-side type validation, file size limits, and malware scan integration."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "SECURITY"]
  Confidence: 0.93

Predicate evidence:
  UI fires on: "widget" (component family)
  IMPLEMENT fires on: "create"
  SECURITY fires on: "validation", "scan" (security context), "limits" (security constraint)
  TransitionType: feature_addition.

---

## B01-0A-048-003 — UI+SECURITY: "document security requirements for form components" fires UI+DOCUMENT+SECURITY

```
source_id: xiigen-b01-0a-048-003
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Document the security requirements for all form components including input sanitization and authentication token handling."
  CompiledPredicateIds: ["UI", "DOCUMENT", "SECURITY"]
  Confidence: 0.93

Predicate evidence:
  UI fires on: "form", "component", "input"
  DOCUMENT fires on: "document", "requirements"
  SECURITY fires on: "security", "sanitization", "authentication", "token"
  TransitionType: documentation (IMPLEMENT absent).

---

## B01-0A-048-004 — UI+SECURITY: full sweep — build secure component library

```
source_id: xiigen-b01-0a-048-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a secure UI component library, define the authentication API schema, document security requirements, and write penetration tests."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "ARCHITECTURE", "DOCUMENT", "SECURITY", "TEST"]
  Confidence: 0.95

Predicate evidence:
  UI fires on: "ui", "component"
  IMPLEMENT fires on: "build", "write"
  ARCHITECTURE fires on: "API", "schema", "authentication" (infrastructure)
  DOCUMENT fires on: "document", "requirements"
  SECURITY fires on: "secure", "security", "penetration"
  TEST fires on: "tests" (via "penetration tests")
  Six predicates → six independent routing decisions.
