# Held-Out and Corruption Pairs (024)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-024-A — Held-out: "teach to QA" → gap

A: "Teach the engineering teams to QA their own features before merging."
B: Predicates: [] | GapHandler → gap-skills-guidance-train-001
Reason: "teach to QA" = "teach to X" pattern → gap. Infinitive "to QA" = learning target. Requester's action = teach = gap.

---

## HC-024-B — Held-out: "QA the release portal" → QA fires

A: "QA the new developer portal before the v2 release this Friday."
B: Predicates: ["QA"] | QAGatingFlow73 | TransitionType: no contribution
Reason: "QA" abbreviation as direct action verb → QA fires. Requester performs QA directly.

---

## HC-024-C — Held-out: guidance purpose clause + code actions → code predicates fire

A: "To help the team grow, implement the reference architecture and document the design patterns."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | ImplementationFlow41 ‖ DocumentationFlow
TransitionType: feature_addition
Reason: "to help the team grow" = purpose clause → gap warning; "implement" → IMPLEMENT; "document" → DOCUMENT. Code predicates fire; guidance purpose = gap warning.

---

## HC-024-D — Complete B34 corruption pattern summary

| Corruption type | Example corrupt output | Correct output |
|----------------|----------------------|----------------|
| v6-01 (invented predicates) | mentor → ["MENTORING"] | [] → gap |
| v6-02 (gap suppression) | explain → ["DOCUMENT"] | [] → gap |
| v6-02 (gap suppression) | shadow while she builds → ["IMPLEMENT"] | [] → gap |
| v6-02 (gap suppression) | walk through architecture → ["ARCHITECTURE"] | [] → gap |
| v6-02 (gap suppression) | teach to integrate → ["API_CONTRACT"] | [] → gap |
| v6-03 (predicate loss) | walk through integrating → [] | ["API_CONTRACT"] |
| v6-03 (predicate loss) | coach through building → [] | ["IMPLEMENT"] |
| v6-03 (predicate loss) | guide through writing tests → [] | ["TEST"] |
