# Layer 0-B — Router Pairs (056)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-056-001 — full round vocabulary routing table (complete reference)

| Vocabulary | Round | Gap ID | GapHandler? |
|-----------|-------|--------|-------------|
| mentor, coach, guide, advise | 1 | mentor-001 | Yes |
| teach, train [human], explain, onboard, shadow, upskill, facilitate | 1 | train-001 or mentor-001 | Yes |
| pair-with, walk-through, workshop | 1/2 | mentor-001 or train-001 | Yes |
| show how to, demonstrate | 2/3 | train-001 | Yes |
| introduce [person] | 2/3 | onboard-001 or train-001 | Yes |
| brief [person] on | 2/3 | mentor-001 | Yes |
| familiarize with | 2/3 | onboard-001 | Yes |
| run through [X] with | 2/3 | mentor-001 | Yes |
| review [X] with | 2/3 | mentor-001 | Yes |
| present [X] to | 2/3 | train-001 | Yes |
| discuss [X] with | 2/3 | mentor-001 | Yes |
| consult on/with | 3 | mentor-001 | Yes |
| seek advice on | 3 | mentor-001 | Yes |
| get feedback on | 3 | mentor-001 | Yes |
| sanity check [X] with | 3 | mentor-001 | Yes |
| second opinion | 3 | mentor-001 | Yes |
| rubber duck | 3 | mentor-001 | Yes |
| sounding board | 3 | mentor-001 | Yes |

---

## B34-router-056-002 — introduce [feature] vs [person]: routing summary

introduce [person/team/role] to [topic] → GapHandler (gap)
introduce [feature/module/capability] into [system] → ImplementationFlow41 (IMPLEMENT)
introduce [concept] to [person] → GapHandler (gap)
introduce [noun] (ambiguous) → evaluate subject: person → gap; software artifact → IMPLEMENT

---

## B34-router-056-003 — "show [output]" vs "show how to" routing boundary

show [noun/output] to [person] → neutral (no gap, no predicate unless other vocabulary)
show how to [verb] → GapHandler
show the [result/dashboard/report] → neutral
show the [person] how to → GapHandler

---

## B34-router-056-004 — "brief" noun vs verb routing boundary

brief [person] on [topic] (verb) → GapHandler
a brief [document/spec] (adjective) → no gap
write a brief (noun) → DOCUMENT
the technical brief (noun) → DOCUMENT if write action present

---

## B34-router-056-005 — "run [X]" vs "run through [X] with" routing boundary

run the tests → TEST (if direct action)
run the pipeline → context-dependent (not gap)
run through [X] with [person] → GapHandler (explanation vocabulary)
run through the checklist (without person target) → borderline: likely gap if review/explanation intent
