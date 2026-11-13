# Layer 0-C — Knowledge Edge Pairs (005)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-005-001 — "explain security" vs "secure" predicate distinction

"Explain secure coding practices" → gap (explaining security = teaching, not implementing)
"Teach the team about SQL injection" → gap (teaching about vulnerability ≠ SECURITY)
"Secure the API against SQL injection" → SECURITY (securing = implementing security control)
"Harden the authentication layer" → SECURITY (hardening = implementing security control)
"Add input sanitization middleware" → IMPLEMENT+SECURITY (add + middleware → IMPLEMENT; sanitization → SECURITY)

Rule: SECURITY fires on security-IMPLEMENTING actions (secure, harden, protect, guard), not on security-EXPLAINING actions (explain, teach, train on, discuss).

---

## KE-005-002 — One firing predicate vs GapHandler mutual exclusion

"Mentor the team and document the coding standards" → ["DOCUMENT"] → GapHandler NOT invoked
"Mentor the team on coding standards" → [] → GapHandler invoked
The difference: "and document" = explicit DOCUMENT action from requester. "on coding standards" = guidance object.

Rule: GapHandler is invoked ONLY when predicate set = []. Any firing predicate (even one) suppresses GapHandler.

---

## KE-005-003 — "code review" disambiguation

"Review code" (mentoring) → gap (mentoring activity)
"Document the code review process" → DOCUMENT (document + process spec)
"Automate code review checks" → gap ("automate" = tooling vocabulary without code verb)
"Build the automated code review bot" → IMPLEMENT (build + code artifact)
"Write tests that enforce code review standards" → TEST (write tests)

Rule: "code review" in mentoring context → gap. With code-action verb → that predicate fires.
