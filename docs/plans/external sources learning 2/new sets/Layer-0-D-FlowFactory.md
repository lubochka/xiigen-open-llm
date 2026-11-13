# Layer 0-D — Flow Factory Selection Pairs

```
Contract:
  ILogicalFlowFactory<TInput>.Create(LogicalFlowFactoryRequest)
      -> ILogicalFlow<TInput>
```

## Common coverage labels (apply to every pair in this file)

```
source_license_state: external_license_checked
contract_signature:   ILogicalFlowFactory
node_role:            layer_0_d
pair_kind:            flow_factory_selection
coverage_status:      covered
attribution_required: false
source_id:            rfc-7519, rfc-6749     (3.4 ES-S5 scenario for all four trust states)
source_title:         RFC 7519 + RFC 6749 (factory-selection trust states)
transition_type:      training_cycle
phase:                3
sub_phase:            3.4
task_type:            flow-factory-selection
```

Per-pair labels carry: `trust_state`, `rejected_type`, `overlap_ratio`.

Token-overlap gate: every rejected satisfies overlap_ratio < 0.60.

Per the v8 acceptance checklist and v7 pipeline plan §2.5d, every batch
must include all four trust-state cases:

```
1. UseLearnedUnitWhenAvailable: true  + Trusted learned unit       → learned selected
2. UseLearnedUnitWhenAvailable: false + Trusted learned unit       → deterministic selected
3. UseLearnedUnitWhenAvailable: true  + only Rejected learned unit → deterministic fallback
4. UseLearnedUnitWhenAvailable: true  + Probationary unit (>= min) → Probationary selected
```

All four pairs in this file use the same scenario context (ES-S5 / 3.4) so
the trust state is the only varying input. ES-S5 is chosen because it is
the registered scenario in this batch — the only one with learned units in
play.

---

## Pair[ID] ES-2026-05-08-3.4-L0D-01 — Trusted + flag true → learned selected

A:
```
LogicalFlowFactoryRequest:
  ScenarioId:                    "ES-S5"
  TargetSubPhaseId:              "3.4"
  UseLearnedUnitWhenAvailable:   true
  MinimumTrustLevel:             "Trusted"
  AvailableLearnedUnits:
    - UnitId:      "learned-3.4-jwt-validator-v3"
      TrustLevel:  "Trusted"
      Coverage:    0.94
  DeterministicUnits:
    - UnitId: "deterministic-3.4-jwt-validator"
```

B:
```
SelectedUnit:
  UnitId:     "learned-3.4-jwt-validator-v3"
  Source:     "learned"
  TrustLevel: "Trusted"
SelectionReason: "UseLearnedUnitWhenAvailable=true and a Trusted learned unit is available. Learned selected."
```

Reason:
```
[WRONG: when the flag is true and a Trusted learned unit exists, the
factory must select the learned unit. The rejected selects the deterministic
fallback even though no fallback condition has fired. This is the inverse
of the rule — the deterministic unit is the fallback path, not the
default path when learned is available and the flag permits it.]
```

Phrasing-B (Type A — wrong fallback selection):
```
SelectedUnit:
  UnitId:     "deterministic-3.4-jwt-validator"
  Source:     "deterministic"
  TrustLevel: "n/a"
SelectionReason: "Selected deterministic unit despite trusted learned unit being available."
```

Coverage labels:
```
trust_state:    trusted_flag_true
rejected_type:  A
overlap_ratio:  0.31   (Type A: chosen learned UnitId vs rejected deterministic UnitId; same prefix, suffix differs)
```

---

## Pair[ID] ES-2026-05-08-3.4-L0D-02 — Trusted + flag false → deterministic selected

A:
```
LogicalFlowFactoryRequest:
  ScenarioId:                    "ES-S5"
  TargetSubPhaseId:              "3.4"
  UseLearnedUnitWhenAvailable:   false
  MinimumTrustLevel:             "Trusted"
  AvailableLearnedUnits:
    - UnitId:      "learned-3.4-jwt-validator-v3"
      TrustLevel:  "Trusted"
      Coverage:    0.94
  DeterministicUnits:
    - UnitId: "deterministic-3.4-jwt-validator"
```

B:
```
SelectedUnit:
  UnitId:     "deterministic-3.4-jwt-validator"
  Source:     "deterministic"
  TrustLevel: "n/a"
SelectionReason: "UseLearnedUnitWhenAvailable=false. Deterministic selected even though a Trusted learned unit is available."
```

Reason:
```
[WRONG: when the flag is false, the deterministic unit must be selected
regardless of learned unit availability. The flag is an explicit operator
override — its purpose is to force deterministic execution. The rejected
selects the learned unit anyway, which makes the flag a no-op. The flag
must be load-bearing or the operator cannot ever audit deterministic
behavior.]
```

Phrasing-B (Type A — flag false ignored):
```
SelectedUnit:
  UnitId:     "learned-3.4-jwt-validator-v3"
  Source:     "learned"
  TrustLevel: "Trusted"
SelectionReason: "Learned unit available so selected despite UseLearnedUnitWhenAvailable=false."
```

Coverage labels:
```
trust_state:    trusted_flag_false
rejected_type:  A
overlap_ratio:  0.32   (Type A: chosen deterministic UnitId vs rejected learned UnitId; flag override semantics differ)
```

---

## Pair[ID] ES-2026-05-08-3.4-L0D-03 — Only Rejected unit + flag true → deterministic fallback

A:
```
LogicalFlowFactoryRequest:
  ScenarioId:                    "ES-S5"
  TargetSubPhaseId:              "3.4"
  UseLearnedUnitWhenAvailable:   true
  MinimumTrustLevel:             "Probationary"
  AvailableLearnedUnits:
    - UnitId:      "learned-3.4-jwt-validator-v2"
      TrustLevel:  "Rejected"
      Coverage:    0.31
  DeterministicUnits:
    - UnitId: "deterministic-3.4-jwt-validator"
```

B:
```
SelectedUnit:
  UnitId:     "deterministic-3.4-jwt-validator"
  Source:     "deterministic"
  TrustLevel: "n/a"
SelectionReason: "UseLearnedUnitWhenAvailable=true but the only learned unit is Rejected. Rejected units are never selected. Deterministic fallback engaged."
```

Reason:
```
[WRONG: Rejected units are never selected regardless of the
UseLearnedUnitWhenAvailable flag. Rejection is the strongest
disqualification — the unit failed prior gating with measured evidence
(coverage 0.31 here). The rejected selects it anyway because the flag is
true. This violates the trust hierarchy and would propagate a known-bad
unit into runtime. The deterministic fallback is the correct path.]
```

Phrasing-B (Type A — Rejected unit selected):
```
SelectedUnit:
  UnitId:     "learned-3.4-jwt-validator-v2"
  Source:     "learned"
  TrustLevel: "Rejected"
SelectionReason: "UseLearnedUnitWhenAvailable=true so any learned unit is selected."
```

Coverage labels:
```
trust_state:    rejected_only_fallback
rejected_type:  A
overlap_ratio:  0.34   (Type A: chosen deterministic-fallback vs rejected Rejected-unit-selected; verdict tokens disjoint)
```

---

## Pair[ID] ES-2026-05-08-3.4-L0D-04 — Probationary + min Probationary + flag true → Probationary selected

A:
```
LogicalFlowFactoryRequest:
  ScenarioId:                    "ES-S5"
  TargetSubPhaseId:              "3.4"
  UseLearnedUnitWhenAvailable:   true
  MinimumTrustLevel:             "Probationary"
  AvailableLearnedUnits:
    - UnitId:      "learned-3.4-jwt-validator-v4-prob"
      TrustLevel:  "Probationary"
      Coverage:    0.71
  DeterministicUnits:
    - UnitId: "deterministic-3.4-jwt-validator"
```

B:
```
SelectedUnit:
  UnitId:     "learned-3.4-jwt-validator-v4-prob"
  Source:     "learned"
  TrustLevel: "Probationary"
SelectionReason: "UseLearnedUnitWhenAvailable=true and the learned unit meets the MinimumTrustLevel=Probationary threshold. Probationary unit selected for evaluation."
```

Reason:
```
[WRONG: Probationary trust meets the Probationary minimum exactly — the
unit qualifies. Selecting it is how the system gathers the runtime data
that promotes it to Trusted or rejects it. The rejected forces the
deterministic fallback anyway, denying the Probationary unit any chance
to graduate. This breaks the learned-unit lifecycle: units stuck at
Probationary forever because the factory never picks them.]
```

Phrasing-B (Type A — Probationary blocked):
```
SelectedUnit:
  UnitId:     "deterministic-3.4-jwt-validator"
  Source:     "deterministic"
  TrustLevel: "n/a"
SelectionReason: "Probationary trust selected deterministic fallback to be safe."
```

Coverage labels:
```
trust_state:    probationary_meets_minimum
rejected_type:  A
overlap_ratio:  0.33   (Type A: chosen Probationary-selected vs rejected deterministic-blocked; selection rationale tokens disjoint)
```

---

## Layer 0-D summary

```
Pairs:                  4 (all four trust-state cases as required)
Type A rejected:        4
Trust states covered:
  Trusted + flag true                → learned selected           (pair 01)
  Trusted + flag false               → deterministic selected     (pair 02)
  only Rejected + flag true          → deterministic fallback     (pair 03)
  Probationary >= minimum + flag true → Probationary selected     (pair 04)
```

This satisfies the v8 acceptance checklist requirement that flow factory
selection includes all four trust-state cases.
