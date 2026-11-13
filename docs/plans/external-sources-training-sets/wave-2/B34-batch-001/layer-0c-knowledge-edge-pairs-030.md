# Layer 0-C — Knowledge Edge Pairs (030)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-030-001 — "brief on" + gap ID assignment detail

"brief [team]" = general status communication → gap-skills-guidance-mentor-001.
"brief [new hire]" = onboarding context → gap-skills-guidance-onboard-001.
"brief on [any topic]" = information-sharing regardless of topic → gap.

Priority: if new-hire context detected → onboard-001. Otherwise → mentor-001. "Brief" alone never → train-001.

---

## KE-030-002 — "demonstrate" + gap ID assignment

"demonstrate to [team]" → gap-skills-guidance-train-001 (demonstration = training delivery).
"demonstrate [methodology/pattern]" → gap-skills-guidance-train-001.
"demonstrate [process] to [new hire]" → gap-skills-guidance-train-001 (demonstrate takes train-001 even in onboarding context; onboard-001 reserved for familiarize/introduce/ramp-up verbs).

---

## KE-030-003 — "run through" + gap ID assignment

"run through with [general team]" → gap-skills-guidance-mentor-001.
"run through for [new hire]" → gap-skills-guidance-onboard-001 (new hire context).
"run through [methodology]" → gap-skills-guidance-train-001 if training context. Default: mentor-001.

---

## KE-030-004 — "show how to" + gap ID assignment

"show how to [verb]" = always gap-skills-guidance-train-001 (teaching/demonstration = train category).
No context overrides this: "show new hire how to" → still train-001 (show = training verb, not onboarding verb).

---

## KE-030-005 — complete Round 2 gap ID assignment table

| Guidance verb | Default gap ID | Override conditions |
|--------------|----------------|---------------------|
| show how to | train-001 | none |
| show [existing artifact] | train-001 | none |
| demonstrate | train-001 | none |
| introduce [person] to | train-001 | new hire context → onboard-001 |
| brief [team] on | mentor-001 | new hire context → onboard-001 |
| familiarize with | onboard-001 | none (always onboard-001) |
| run through | mentor-001 | new hire/training context → train-001 |
| review with | mentor-001 | none |
| present to | train-001 | none |
| discuss | mentor-001 | none |
