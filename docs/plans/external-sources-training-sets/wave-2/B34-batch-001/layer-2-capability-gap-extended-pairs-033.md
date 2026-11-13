# Layer 2 — Capability Gap Extended Pairs (033)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-033 — guidance + SECURITY topic disambiguation: extended execution

A: "Brief the team on the OWASP (Open Web Application Security Project) API Security Top 10."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
GapHandler: INVOKED | SCHEDULE_CAPABILITY_LEARNING: EMITTED | Verdict: GAP_LOGGED
Rule demonstrated: brief on OWASP = guidance vocabulary → gap. Security topic = learning content.

A: "Consult with the security team on hardening the authentication layer."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
GapHandler: INVOKED | Verdict: GAP_LOGGED
Rule demonstrated: consult on security topic = advisory → gap (not SECURITY predicate).

A: "Brief the team on SQL injection and harden the ORM (Object-Relational Mapping) query layer."
B: Predicates: ["SECURITY"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
GapHandler: SUPPRESSED | Verdict: SECURITY_APPROVED + GAP_WARNING
SecurityAuditFlow: ORM query parameterization enforced, prepared statements validated. APPROVED.

A: "Seek advice on the mTLS (mutual Transport Layer Security) setup and implement the mutual certificate authentication."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Verdict: IMPLEMENT_APPROVED + GAP_WARNING
