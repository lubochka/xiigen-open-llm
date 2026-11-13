# Layer 2 — Capability Gap Extended Pairs (006)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B33-gap-006 — SecurityAuditFlow nine-node: Prompt injection prevention

Request: "Add prompt injection prevention to the agent input layer."

### Flow: SecurityAuditFlow (SECURITY predicate; "prompt injection" exception)

**Node 1**: Parse request. "prompt injection" → SECURITY exception (treated as security vulnerability class).
**Node 2**: Predicate set = ["SECURITY"]. (IMPLEMENT also possible if "add" = code implementation — context-dependent.)
**Node 3**: Security scope: prompt injection attack vectors in agent input layer. Input sanitization, pattern detection, context boundary enforcement.
**Node 4**: Threat model: injection via user input field, system prompt override attempts, indirect injection via retrieved content.
**Node 5**: Arbiter: SecurityArbiter
**Node 6**: Controls identified: input length limits, forbidden token patterns, context isolation, output filtering for prompt leakage.
**Node 7**: SecurityAuditFlow GREEN — controls specified; implementation required.
**Node 8**: Report: prompt-injection-security-spec.md | Threats: 4 attack vectors | Controls: 4 mitigations
**Node 9**: Verdict: SECURITY_APPROVED — prompt injection prevention specification complete.

---

**Key rule**: "prompt injection" is the only AI vocabulary that fires a predicate (SECURITY). All other "prompt" uses → gap. Rationale: prompt injection is a security vulnerability class analogous to SQL injection or XSS.
