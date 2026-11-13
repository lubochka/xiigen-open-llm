# Layer 0-A — Compiler Pairs (022)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-022-001 — "explain the architecture" → always gap

A: "Explain the microservices architecture we use for the payment domain."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "explain" = teaching vocabulary → always gap. Topic = architecture, but explain ≠ ARCHITECTURE. Requester explains, not designs.

---

## B34-compiler-022-002 — "explain and document" → DOCUMENT fires, explain is gap

A: "Explain the auth flow to the team and document it in Confluence."
Analysis: "explain" = gap; "document it in Confluence" = direct documentation action → DOCUMENT fires.
B: Predicates: ["DOCUMENT"] | TransitionType: documentation
Evidence: "document" + documentation artifact → DOCUMENT fires; "explain" = gap warning alongside DOCUMENT. Explain itself never fires DOCUMENT.

---

## B34-compiler-022-003 — "explain how to build" → gap (not IMPLEMENT)

A: "Explain how to build the retry mechanism to the junior engineers."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "explain how to build" = "teach to X" pattern → gap. The infinitive "to build" is what others learn; requester's action = explain = gap. IMPLEMENT does not fire.

---

## B34-compiler-022-004 — "explain and implement" → IMPLEMENT fires, explain is gap

A: "Explain the approach and then implement the caching layer for the product catalog."
Analysis: "explain" = gap; "implement the caching layer" = direct code action → IMPLEMENT fires.
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "implement" + "caching layer" → IMPLEMENT; "explain" = gap warning alongside IMPLEMENT.

---

## B34-compiler-022-005 — "explain security concepts" → gap (not SECURITY)

A: "Explain the security concepts behind JWT (JSON Web Token) validation to the team."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "explain" = always gap, regardless of topic. Security topic does not fire SECURITY. SECURITY fires on "secure/harden/protect [system]", not on explain-about-security.
