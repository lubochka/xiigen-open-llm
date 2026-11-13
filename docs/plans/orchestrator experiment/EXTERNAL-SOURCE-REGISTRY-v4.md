# External Source Registry
# Transition-orchestrator teaching edition
# Version: 4.0 - 2026-05-05
# Supersedes: EXTERNAL-SOURCE-REGISTRY-v3.md

---

## Purpose

This registry maps first-party and external sources to the corrected
orchestration learning chain:

```text
LogicalTransitionOrchestrator -> selected A/B/X transition -> expert split
```

---

## New Source Mapping Rule

Every source is mapped to at least one of these roles:

```text
transition intent evidence;
candidate A/B/X evidence;
candidate rejection evidence;
expert split evidence;
branch Ai/Bi/Xi evidence;
arbiter authority evidence;
human result evidence;
resource measurement evidence.
```

If a source only teaches code implementation and none of these roles, it is
deferred.

---

## Highest Priority Sources

| Source | Primary use |
|---|---|
| XIIGen user/session prompts | Transition intent evidence and prompt wording. |
| XIIGen orchestration plans | Candidate A/B/X and expected lifecycle split evidence. |
| XIIGen review corrections | Candidate rejection, arbiter, and adaptation evidence. |
| XIIGen inspector performance history | Resource measurement and performance-correction transition evidence. |
| Wave 1 security corpus | Security branch authority and release-gate arbiter evidence. |

---

## Wave 1 Reclassification

Wave 1 now has a specific role:

```text
security authority inside transition candidates
```

It can support:

```text
feature_addition candidate for O1;
external_source_integration candidate for O4;
qa_release candidate when security review blocks promotion;
repair candidate when prompt states existing security behavior is unsafe.
```

It must not force every security-related prompt into `external_source_integration`
or `repair`.

---

## License States

The v3 source states remain active:

```text
first_party;
external_license_checked;
attribution_required;
source_gap;
corruption_case;
verify_before_use.
```

`verify_before_use` still cannot enter training rows.

---

## Last Updated

2026-05-05.
