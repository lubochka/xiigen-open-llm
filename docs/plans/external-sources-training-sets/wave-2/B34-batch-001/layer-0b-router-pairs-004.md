# Layer 0-B — Router Pairs (004)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: router

---

## B34-router-004-001 — Gap route: "coach on security"

A: "Coach the team on secure coding practices."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Key rule: "explain/coach/mentor on security" ≠ SECURITY predicate. SECURITY requires implementing security controls.

---

## B34-router-004-002 — SECURITY route (implementing, not explaining)

A: "Add input validation to harden the API against injection attacks."
Router decision: Predicates = ["SECURITY"] → SecurityAuditFlow
Evidence: "harden" + "injection attacks" → SECURITY (security-implementing verb + attack vocabulary).

---

## B34-router-004-003 — IMPLEMENT+TEST+DOCUMENT three-flow route

A: "Build the mentorship matching system, write unit tests, and document the matching algorithm."
Router decision: Predicates = ["IMPLEMENT", "TEST", "DOCUMENT"] → ImplementationFlow41 + UnitTestingFlow51 + DocumentationFlow
TransitionType: feature_addition. Gap for "mentorship" context = silently skipped (predicates fire).

---

## B34-router-004-004 — Gap route: "share best practices"

A: "Share best practices on API design with the new engineering hires."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001

---

## B34-router-004-005 — v6 corruption: "teach to implement" collapses to IMPLEMENT only

v6 corrupt: "Teach the team to implement microservices" → ["IMPLEMENT"]
Correct: Predicates = [] → GapHandler
Error type: v6-02. "teach to implement" = teaching vocabulary → gap. The embedded verb "implement" after "teach to" is infinitive/tutorial framing, not a direct action verb from the requester.
Note: If the requester is asking to implement (not teach to implement), IMPLEMENT fires. Context of who performs the action matters.
