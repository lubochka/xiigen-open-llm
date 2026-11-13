# P3 Failure Discriminator — Document 1
# How BLOCK differs from PASS_WITH_GAPS at the predicate level
# Date: 2026-05-04

## The Core Distinction

The transition gate produces three verdict types:

- PASS: all upstream constraints, decisions, knowledge edges, and rejected
  items have semantic neighbors in the downstream packet set. No rejected
  upstream pattern was reinstated.
- PASS_WITH_GAPS: at least one upstream item is present as a concern (named
  gap) or boundary (named scope limit). The item is visible in the downstream
  domain packet set even if enforcement is not proven. No upstream item is
  entirely absent.
- BLOCK: at least one upstream high-confidence constraint is absent from the
  downstream domain packet set. The item has no representation — not even as
  a concern or boundary. The transition cannot proceed.

## What T34 Produced

T34 produced PASS_WITH_GAPS on the BaseService constraint because:

- The upstream PhaseContextPacket listed BaseService in ConstraintsInherited.
- The downstream domain packet set contained td06_base_service_constraint with
  CoverageStatus = concern.
- The constraint was visible. The enforcement was absent. The gap was named.

The transition gate counted td06 as a semantic neighbor of the upstream
BaseService constraint. LostConstraintCount = 0.

## What P3-C Produces

P3-C corrupts the downstream domain packet set by removing td06 entirely.

- The upstream PhaseContextPacket still lists BaseService in ConstraintsInherited.
- The downstream domain packet set contains 9 packets. No packet carries the
  BaseService predicate in any field.
- The transition gate finds no semantic neighbor for the BaseService constraint.
  LostConstraintCount ≥ 1.

The transition gate must produce BLOCK. The BLOCK is attributed to the
BaseService predicate because that is the specific constraint with no neighbor.

## Predicate Evidence for the Distinction

The BLOCK/PASS_WITH_GAPS discriminator operates at the predicate level.

Predicate evidence for PASS_WITH_GAPS (T34 case):

```
ConstraintsInherited: ["BaseService inheritance is a production service pattern..."]
DomainPackets: [{PacketId: "td06_base_service_constraint", CoverageStatus: "concern", ...}]
Mentions(packet, "BaseService") = true for td06
LostConstraintCount = 0
Verdict = PASS_WITH_GAPS
```

Predicate evidence for BLOCK (P3-C corrupted case):

```
ConstraintsInherited: ["BaseService inheritance is a production service pattern..."]
DomainPackets: [9 packets, none referencing BaseService]
Mentions(packet, "BaseService") = false for all 9 packets
LostConstraintCount = 1
Verdict = BLOCK
```

## Discriminator Definition

The learned discriminator produced by P3-L encodes:

```
discriminator_id: base_service_dropped_predicate
pattern: LostConstraintCount > 0 AND dropped_constraint_name = "BaseService"
verdict_when_pattern_matches: BLOCK
verdict_when_pattern_absent_but_concern_present: PASS_WITH_GAPS
scope: BaseService predicate only
generalization: none — this discriminator applies to the BaseService
  predicate. It does not apply to other dropped predicates unless separately
  learned.
```

## What Must Not Happen

- The discriminator must not convert T34-2 (cross-phase context packet concern)
  from PASS_WITH_GAPS to BLOCK. The cross-phase context packet was present as
  a concern in T34. The discriminator is BaseService-specific.
- The discriminator must not apply retroactively to the T34-3 scenario
  (build-time sequence classification). That scenario has nothing to do with
  BaseService.
- The P3-V verification run must confirm the discriminator is inactive when
  BaseService is present as a concern, not when it is entirely absent.

## Severity Boundary

The severity boundary between BLOCK and PASS_WITH_GAPS is:

| Condition | Verdict |
|---|---|
| Upstream constraint absent from all downstream packets | BLOCK |
| Upstream constraint present as concern packet | PASS_WITH_GAPS |
| Upstream constraint present as boundary packet | PASS_WITH_GAPS |
| Upstream constraint present as covered packet | PASS |

A constraint is absent when Mentions() returns false for every packet in the
downstream domain packet set. A constraint is not absent if it appears in any
packet, regardless of CoverageStatus.
