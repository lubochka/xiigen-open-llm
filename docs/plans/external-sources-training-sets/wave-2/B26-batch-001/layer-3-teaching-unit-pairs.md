# Layer 3 — Teaching Unit Pairs
# Batch: B26-batch-001
# Scenario: B26 — QA security review, sub-phase 6.3
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: teaching_unit

---

## Pair B26B1-WDA-teach-001 — SECURITY routes to 6.3 in audit context vs 5.2 in test context

A: "Audit the authentication layer" vs "Write security tests for the authentication layer."
   What sub-phase does SECURITY route to in each case?
B: "Audit the authentication layer" → SECURITY → 6.3 (Security and Compliance Review).
   "Write security tests for the authentication layer" → TEST fires (test suite writing) +
   SECURITY → 5.2 (Security and Compliance Testing). The difference: audit/review context
   → 6.3; writing-test-code context → 5.2. The co-occurring TEST predicate and the
   action verb "write tests" shift SECURITY from 6.3 to 5.2.

---

## Pair B26B1-WDA-teach-002 — "audit" as a noun vs verb: SECURITY fires only on verb

A: "Prepare for the upcoming security audit." Does SECURITY fire?
B: No. "audit" here is a noun (the upcoming audit = a future event). SECURITY fires on
"audit" as an action verb: "audit the system", "audit the access controls". "Prepare for"
is gap vocabulary — no predicate. Result: zero predicates → knowledge_edge.

---

## Pair B26B1-WDA-teach-003 — "compliance" and regulatory acronyms are gap vocabulary

A: "Ensure GDPR compliance and SOC2 readiness." Does SECURITY fire?
B: No. "GDPR" and "SOC2" are regulatory compliance acronyms — gap vocabulary with no
predicate. "compliance" is gap. "ensure" is gap. SECURITY fires on technical security
actions: audit, authentication, authorization, encryption, injection, sanitization,
vulnerability, access control, enforce. Regulatory label vocabulary is outside the
predicate lexicon. Result: zero predicates → knowledge_edge.
Gap ID: gap-security-regulatory-001.

---

## Pair B26B1-WDA-teach-004 — QA and SECURITY are independent predicates; both route independently

A: "QA the release and audit the security controls." How many routing decisions?
B: Two routing decisions. QA fires on "QA" → routes to 7.3. SECURITY fires on "audit"
+ "security controls" → routes to 6.3. Two predicates → two independent routing
decisions: [QA → 7.3, SECURITY → 6.3]. They are never collapsed. The flow factory
instantiates two separate flow instances.

---

## Pair B26B1-WDA-teach-005 — "pentest" is gap; but "authentication" still fires SECURITY

A: "Run a pentest on the authentication service." Does SECURITY fire?
B: Yes, weakly. "pentest" is gap vocabulary (penetration testing terms are not in the
SECURITY predicate lexicon). However, "authentication service" contains "authentication"
which independently fires SECURITY. CompiledPredicateIds: ["SECURITY"] via "authentication".
TrustState: probationary_accepted (weak signal — single security noun, gap action verb).
Gap warning emitted: gap-security-pentest-001.
