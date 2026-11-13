# Wave 1 Held-Out Split v1
# Date: 2026-05-05

---

## Purpose

Define the held-out evaluation split required before any Wave 1 sub-phase can
be promoted into the v4 corpus.

This document defines the split. It does not claim held-out documents have been
collected.

---

## Per-Sub-Phase Held-Out Shape

Each sub-phase needs 20 held-out documents:

| Category | Count | Required behavior |
|---|---:|---|
| correct flow selection | 4 | correct phase and sub-phase, confidence >= 0.70 |
| knowledge edge detection | 4 | named gap, no invented solution |
| cross-sub-phase constraint | 4 | boundary detected, arbiter fires when needed |
| conflict detection | 4 | conflict blocked or reprocessed, zero formatter leakage |
| edge cases | 4 | sub-phase-specific edge behavior |

Passing threshold:

```text
17/20 held-out documents pass
0/4 conflict documents leak conflict to formatter
```

---

## Held-Out Id Convention

```text
WDA-HO-{sub_phase_id}-{category}-{number}
```

Example:

```text
WDA-HO-3.4-security-and-authorization-model-conflict-01
```

---

## Conflict Leakage Rule

Conflict leakage has zero tolerance.

If a conflict document reaches the formatter as an accepted result, the
sub-phase fails even if the score is otherwise 19/20.

---

## Transition Held-Out Shape

The v4 transition set requires 60 held-out transition documents:

| Transition | Count |
|---|---:|
| phase-1-ui-ux-definition -> phase-2-functional-specification | 10 |
| phase-2-functional-specification -> phase-3-technical-design | 10 |
| phase-3-technical-design -> phase-4-implementation | 10 |
| phase-4-implementation -> phase-5-testing | 10 |
| phase-5-testing -> phase-6-qa | 10 |
| phase-6-qa -> phase-7-qa-automation | 10 |

Each transition pack must include:

- 2 clean pass cases;
- 2 pass-with-gaps cases;
- 2 hard block cases;
- 2 knowledge-edge continuity cases;
- 2 rejected-item preservation cases.

---

## Current Status

| Held-out set | Required | Collected | Promoted |
|---|---:|---:|---:|
| sub-phase held-out | 620 | 0 | 0 |
| transition held-out | 60 | 0 | 0 |
| total | 680 | 0 | 0 |

---

## Next Action

The first held-out batch should target the five center-phase sub-phases named
in the v4 Wave 2 plan:

```text
3.1 Architecture and component topology
3.3 API contract definition
3.4 Security and authorization model
4.1 Backend services
4.3 Integration and provider connectors
```

That batch requires:

```text
5 sub-phases * 20 held-out documents = 100 held-out documents
```
