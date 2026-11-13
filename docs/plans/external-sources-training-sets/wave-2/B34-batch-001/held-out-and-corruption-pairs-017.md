# Held-Out and Corruption Pairs (017)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-017-A — Held-out: "advise on security" → gap (not SECURITY)

A: "Advise the engineering team on secure coding practices and common vulnerability patterns."
B: Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Reason: "advise on security" = advisory/teaching vocabulary → gap. Teaching about security ≠ SECURITY predicate.

---

## HC-017-B — Held-out: "advise and harden" → SECURITY

A: "Advise on the attack vectors and harden the API authentication layer against token theft."
B: Predicates: ["SECURITY"] | SecurityAuditFlow | TransitionType: documentation
Reason: "advise" = gap warning; "harden" = direct action → SECURITY fires.

---

## HC-017-C — Corruption v6-02: "advise on security" fires SECURITY

v6 corrupt: "Advise the team on writing secure API endpoints" → Predicates: ["SECURITY"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02 (gap suppression). "advise on writing secure" = advisory vocabulary → gap. Teaching about security ≠ SECURITY. SECURITY fires on harden/secure/protect direct actions by requester.

---

## HC-017-D — Corruption v6-02: "advise on quality assurance" fires QA

v6 corrupt: "Advise the team on quality assurance processes" → Predicates: ["QA"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02. "advise on quality assurance" = double gap: advise = guidance vocab; quality assurance spelled out = gap. QA fires only on "QA" abbreviation. Spelled-out "quality assurance" → gap.
