# Layer 0-C — Knowledge Edge Pairs (046)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-046-001 — complete "integrate" exception map for B34

All forms of "integrate" and their B34 treatment:
| Form | Treatment |
|------|-----------|
| integrate [X] (requester direct) | API_CONTRACT fires |
| integrating [X] (participial in guidance frame) | API_CONTRACT fires + gap warning |
| show how to integrate | gap (infinitive exception) |
| demonstrate how to integrate | gap (infinitive exception) |
| teach to integrate | gap (teach-to exception) |
| train to integrate | gap (teach-to parallel) |
| explain how to integrate | gap (explain exception) |
| consult/seek advice on integrating | gap (advisory exception) |
| discuss integrating (planning) | gap (deliberation frame) |
| facilitate integration of [X] | API_CONTRACT fires (facilitate+direct integration noun) |
| help [person] integrate [X] | API_CONTRACT fires (help + direct action) |
| help [person] learn to integrate | gap (help learn = teaching) |

---

## KE-046-002 — "second opinion" scope boundary

"Second opinion" triggers gap in ALL topic contexts:
- second opinion on architecture → gap (not ARCHITECTURE)
- second opinion on security approach → gap (not SECURITY)
- second opinion on test strategy → gap (not TEST)
- second opinion on API design → gap (not API_CONTRACT)
- second opinion on implementation → gap (not IMPLEMENT)
Only when paired with direct code action does the code predicate fire.

---

## KE-046-003 — "rubber duck" scope boundary

"Rubber duck" triggers gap in ALL topic contexts:
- rubber duck architecture decisions → gap
- rubber duck security design → gap
- rubber duck test approach → gap
- rubber duck integration design → gap (not API_CONTRACT; even with integrate topic)
- rubber duck implementation plan → gap (not IMPLEMENT)
Only when "rubber duck and [code verb]" = code predicate fires alongside gap warning.

---

## KE-046-004 — "sounding board" scope boundary

"Sounding board" triggers gap in ALL topic contexts:
- sounding board for architecture → gap (not ARCHITECTURE)
- sounding board for security model → gap (not SECURITY)
- sounding board for test strategy → gap (not TEST)
- sounding board for integration design → gap (not API_CONTRACT)
Only when "sounding board + then [code verb]" = code predicate fires alongside gap warning.

---

## KE-046-005 — "sanity check" scope boundary

"Sanity check" triggers gap in ALL topic contexts:
- sanity check the architecture → gap
- sanity check the security posture → gap (not SECURITY)
- sanity check the tests → gap (not TEST)
- sanity check the API contract → gap (not API_CONTRACT)
- sanity check the implementation → gap (not IMPLEMENT)
Only when "sanity check + direct code action" = code predicate fires alongside gap warning.
