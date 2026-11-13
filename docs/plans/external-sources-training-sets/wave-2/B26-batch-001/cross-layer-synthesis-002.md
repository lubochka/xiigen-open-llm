# Cross-Layer Synthesis (002)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## S002 — End-to-end trace: "Implement rate limiting on the login endpoint and audit the existing brute-force exposure"

### Layer 0-A
Input: "Implement rate limiting on the login endpoint and audit the existing brute-force exposure."
  "implement" → IMPLEMENT | "rate limiting" → SECURITY (brute-force mitigation) | "audit" → SECURITY | "brute-force" → SECURITY
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]

### Layer 0-B
  IMPLEMENT → SubPhase 4.1 | SECURITY → SubPhase 5.2 (implement context) OR 6.3 (audit context)
  Both contexts present: "implement rate limiting" → 5.2; "audit existing exposure" → 6.3
  DecisionSet: [ImplementFlow41, SecurityAuditFlow63, SecurityImplementFlow52]

### Layer 0-D
  FlowFactory.CreateMulti(["IMPLEMENT","SECURITY"]) → [ImplementationFlow41, SecurityAuditFlow63]
  Note: SECURITY sub-phase resolved per clause: audit clause → 6.3; implement clause already covered by IMPLEMENT→4.1

### Layer 1
  TransitionType: feature_addition (implement verb drives TransitionType)
  Sequential: IMPLEMENT (rate limiting) → SECURITY audit (existing exposure)

### Layer 2 (SecurityAuditFlow63)
  Audit findings: No rate limiting on /auth/login (CRITICAL) | Unlimited retry attempts observed | Node 7: FAIL

### Layer 3
  Teaching: Same request can contain both implement and audit contexts. SECURITY sub-phase resolves per clause. "brute-force" fires SECURITY as attack vector noun.

### Cross-layer invariant
  A single request with both "implement" and "audit" verbs routes to both 4.1 and 6.3. TransitionType is driven by IMPLEMENT context, not SECURITY context.

---

## S002-B — Contrast: "Make the login endpoint more secure"

### Layer 0-A
  "make" → gap; "more secure" → gap ("secure" as adjective is gap); "login endpoint" → domain gap
  CompiledPredicateIds: [] → GapHandler

### Layer 3
  Teaching: "more secure" fires nothing. "secure" as adjective is a gap. Contrast with "audit the login endpoint for brute-force exposure" which fires SECURITY.

