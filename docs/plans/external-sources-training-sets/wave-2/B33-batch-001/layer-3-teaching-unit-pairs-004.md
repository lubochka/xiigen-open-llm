# Layer 3 — Teaching Unit Pairs (004)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-004 — "prompt injection" is the only AI vocabulary SECURITY exception

**Teaching goal**: Understand why "prompt injection" fires SECURITY while all other "prompt" uses are gaps.

**Rule**: "prompt injection" → SECURITY. No other "prompt" phrase fires any predicate.

**Reasoning**: Prompt injection is a security vulnerability class — categorically identical to:
- SQL injection → SECURITY
- XSS (Cross-Site Scripting) → SECURITY
- Command injection → SECURITY
The word "injection" signals attack vector, not AI concept.

**Examples**:

| Request | Predicate | Reason |
|---------|-----------|--------|
| "Prevent prompt injection attacks" | SECURITY | injection = attack vector |
| "Add prompt injection guards" | SECURITY | injection = attack vector |
| "Rewrite the system prompt" | [] gap | prompt as AI config |
| "Optimize the prompt template" | [] gap | prompt as AI input |
| "Engineer better prompts" | [] gap | prompt engineering = AI vocabulary |
| "Build a prompt manager" | IMPLEMENT | build = code verb overrides |

**Note**: "Build prompt injection detection" → IMPLEMENT+SECURITY (code verb fires IMPLEMENT; injection fires SECURITY).
