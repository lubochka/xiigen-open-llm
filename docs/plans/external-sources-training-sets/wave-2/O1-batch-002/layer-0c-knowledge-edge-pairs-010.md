# Layer 0-C — Knowledge Edge Pairs (010)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1b-0C-010-001 — Unroutable transition: pure infrastructure complaint

A: "the database is slow today"

B: Result: knowledge_edge | Predicates: []

---

## O1b-0C-010-002 — Unroutable transition: UI cosmetic complaint

A: "the button colors are wrong on the dashboard"

B: Result: knowledge_edge | Predicates: []

---

## O1b-0C-010-003 — Unroutable transition: vague feature request

A: "make the app better for enterprise customers"

B: Result: knowledge_edge | Predicates: []

---

## O1b-0C-010-004 — Unroutable transition: HR/process language

A: "the team needs better tooling for the security review process"

B: Result: knowledge_edge | Predicates: []
   Reasoning: "security review" could seem like SECURITY, but "security review process" in an HR/process context (not a technical implementation request) does not fire a predicate in the O1b scenario. However, if it were "implement the security review endpoint", "implement" and "security" would fire.

---

## O1b-0C-010-005 — Unroutable transition: business strategy statement

A: "we need to move to a zero-trust architecture next quarter"

B: Result: VALID ROUTING | Predicates: ["ARCHITECTURE", "SECURITY"]
   Reasoning: TEACHING POINT — "architecture" fires ARCHITECTURE; "zero-trust" contains "trust" (near-match to security model). In this case, the technical terms present make it routable. "move to a zero-trust architecture" is a technical architectural decision, not pure strategy. Predicates fire despite the business framing.
