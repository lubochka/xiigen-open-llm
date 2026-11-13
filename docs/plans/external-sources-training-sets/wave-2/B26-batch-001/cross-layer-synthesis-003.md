# Cross-Layer Synthesis (003)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## S003 — End-to-end trace: Gap path — "Make the application more secure overall"

### Layer 0-A
Input: "Make the application more secure overall."
  "make" → gap | "more secure" → gap ("secure" as adjective without enforcement verb) | "overall" → gap
  CompiledPredicateIds: [] → gap

### Layer 0-B
  No predicates compiled. Router emits: GapHandler path.
  SCHEDULE_CAPABILITY_LEARNING emitted to subproject 16 (AutonomousLearningPipeline (16)).
  DomainId: "software-development"

### Layer 0-D
  FlowFactory.CreateGap() → GapFlow
  No SecurityAuditFlow63. No ImplementationFlow.

### Layer 1
  TransitionType: not assigned (gap path has no flow)

### Layer 2
  GapHandler response: "The request does not specify a concrete security action. Please specify: audit a service for a vulnerability class, implement a security control, or enforce a security policy."

### Layer 3
  Teaching: Adjective-only security requests ("more secure", "better secured", "security improvements") are gaps. SECURITY requires an action verb: audit, enforce, implement, sanitize, encrypt, scan.

### Cross-layer invariant
  Gap path emits SCHEDULE_CAPABILITY_LEARNING. No flow is instantiated. GapHandler returns a clarification prompt.

---

## S003-B — Contrast: "Audit the application for common OWASP Top 10 vulnerabilities"

### Layer 0-A
  "audit" → SECURITY | "OWASP" → SECURITY | "Top 10 vulnerabilities" → SECURITY
  CompiledPredicateIds: ["SECURITY"]

### Layer 0-B
  SECURITY → SubPhase 6.3

### Layer 3
  Teaching: Adding "audit" + "OWASP" transforms the gap request into a routable SECURITY request at 6.3. Contrast with the gap form which has no action verb and no security vocabulary co-occurrence.

---

## S003-C — Second contrast: "Is the application secure?"

### Layer 0-A
  "is" → gap (interrogative, no action verb) | "secure" → gap (adjective)
  CompiledPredicateIds: [] → gap

### Layer 3
  Teaching: Interrogative forms ("is X secure?", "are we secure?") are also gaps. SECURITY requires imperative action: audit, enforce, scan, implement controls.

