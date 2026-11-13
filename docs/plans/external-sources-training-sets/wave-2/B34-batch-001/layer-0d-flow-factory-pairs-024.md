# Layer 0-D — Flow Factory Pairs (024)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-024-001 — Complete B34 factory reference: guidance vocabulary scenarios

All-gap scenario (GapHandler + SCHEDULE_CAPABILITY_LEARNING):
- Any request where ALL verbs are guidance vocabulary
- No embedded code actions by the requester
- Factory: GapHandler | TransitionType: none

Gap-warning + code factory scenario:
- Request has guidance vocabulary + embedded code action by requester
- OR "integrate" present in any framing
- Factory: code predicate's factory | CapabilityGapWarning appended | GapHandler suppressed

---

## FF-024-002 — TransitionType matrix for B34

| Primary predicate(s) present | TransitionType |
|-----------------------------|----------------|
| [] (gap only) | none |
| [DOCUMENT] only | documentation |
| [TEST] only | documentation |
| [ARCHITECTURE] only | documentation |
| [API_CONTRACT] only | documentation |
| [SECURITY] only | documentation |
| [IMPLEMENT] (any combination) | feature_addition |
| [IMPLEMENT] + refactor intent | refactoring |
| [QA] last in chain | no contribution (QA appended to existing type) |

Guidance vocabulary framing never changes TransitionType.

---

## FF-024-003 — SCHEDULE_CAPABILITY_LEARNING emission rule for B34

Emitted: once, when predicate set = [] and GapHandler invoked
NOT emitted: when predicate set contains any code predicate (even with gap warnings)
NOT emitted: multiple times for multiple guidance verbs in one request
Event payload: {gap_id: "[most specific match]", domainId: "software-development", request: "[original request text]"}
