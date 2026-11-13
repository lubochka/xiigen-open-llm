# Held-Out and Corruption Pairs
# Batch: B26-batch-001
# Scenario: B26 — QA security review, sub-phase 6.3
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05

---

## HO-001 — "enforce" + "authorization": SECURITY fires

A: "Enforce role-based authorization rules across all API endpoints."
Expected: SECURITY fires on "enforce" (security-enforcement verb) + "authorization" (SECURITY term).
API_CONTRACT fires on "API endpoints". CompiledPredicateIds: ["SECURITY", "API_CONTRACT"].
Two routing decisions: SECURITY → 6.3, API_CONTRACT → 3.3.
AUTHORIZATION predicate: "authorization" also fires AUTHORIZATION. Three predicates:
["SECURITY", "AUTHORIZATION", "API_CONTRACT"].

---

## HO-002 — pure QA: "QA the build" → QA only

A: "QA the new build before it goes to staging."
Expected: QA fires on "QA". No other predicates. "build", "staging" are gap.
CompiledPredicateIds: ["QA"]. Routing: 7.3. TransitionType: feature_addition.

---

## HO-003 — SECURITY + DOCUMENT: audit + write up

A: "Audit the encryption implementation and write up the compliance findings."
Expected: SECURITY fires on "audit" + "encryption". DOCUMENT fires on "write up".
CompiledPredicateIds: ["SECURITY", "DOCUMENT"]. Two routing decisions: SECURITY → 6.3, DOCUMENT → 6.2.

---

## COR-001 — v6-01: SECURITY suppressed for "audit" + regulatory vocabulary

CORRUPT: "Audit GDPR compliance and review data protection policies." → CompiledPredicateIds: [].
Why corrupt: "audit" fires SECURITY as an action verb. The corrupt model saw "GDPR" (gap)
and "compliance" (gap) and suppressed SECURITY — but "audit" fires independently of the
object being audited. Correct: CompiledPredicateIds: ["SECURITY"] via "audit".

---

## COR-002 — v6-03: QA and TEST collapsed to single sub-phase 5.1

CORRUPT: "QA and test the authentication service." → single routing decision to 5.1.
Why corrupt: "QA" fires QA predicate → 7.3. "test" fires TEST predicate → 5.1.
"authentication" fires SECURITY → 6.3. Three predicates, three routing decisions.
Collapsing all to 5.1 discards QA (7.3) and SECURITY (6.3). V6-03 multi-branch collapse.
Correct: [QA → 7.3, TEST → 5.1, SECURITY → 6.3] — three independent decisions.

---

## COR-003 — v6-03: SECURITY routing to 5.2 when review/audit context calls for 6.3

CORRUPT: "Audit the access control layer for vulnerabilities." → SECURITY → 5.2.
Why corrupt: 5.2 (Security and Compliance Testing) applies when the request is about
writing security tests for a test suite. "Audit" in a review context routes to 6.3
(Security and Compliance Review). The corrupt model defaulted SECURITY to 5.2 regardless
of context. Correct: SECURITY → 6.3 for audit/review requests.
