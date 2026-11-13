# Layer 0-B — Router Pairs (017)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-017-001 — Gap route: "advise" → gap

A: "Advise the team on which database technology to adopt for the new service."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001

---

## B34-router-017-002 — ARCHITECTURE route: "advise" + design embedded

A: "Advise on the trade-offs and design the data storage architecture for the recommendation engine."
Router decision: Predicates = ["ARCHITECTURE"] → ArchitectureFlow31
Evidence: "design the data storage architecture" → ARCHITECTURE; "advise on trade-offs" = gap warning.

---

## B34-router-017-003 — Gap route: "advise on security" → gap (not SECURITY)

A: "Advise the developers on writing secure authentication code."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "advise on writing secure code" = advisory vocabulary → gap. Teaching about security ≠ SECURITY predicate.

---

## B34-router-017-004 — SECURITY route: "advise" + harden embedded

A: "Advise on the vulnerabilities and harden the API gateway against SQL injection attacks."
Router decision: Predicates = ["SECURITY"] → SecurityAuditFlow
Evidence: "harden the API gateway" → SECURITY; "advise on vulnerabilities" = gap warning alongside SECURITY.

---

## B34-router-017-005 — DOCUMENT route: "advise" + document embedded

A: "Advise on the content and document the API design guidelines for the engineering handbook."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow
Evidence: "document the API design guidelines" → DOCUMENT; "advise on content" = gap warning alongside DOCUMENT.
