# Layer 2 — Nine-Node Flow Pairs (047)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: nine_node_flow

## B15-nine-047 — v6 corruption risks for 025–044 services (nine-node level)

### v6-01: IMPLEMENT suppressed (domain noun used as trigger)

Risk: "KYC verification" suppresses IMPLEMENT → Node 01 A: SelectedLogicalTransition has empty DomainPacket.
Correct: "KYC" is a domain noun; IMPLEMENT fires from "create" verb → DomainPacket contains IMPLEMENT packet.

### v6-02: ARCHITECTURE/SECURITY not caught

Risk: "anonymization domain model" → ARCHITECTURE packet missing from DomainPacket.
Correct: "anonymization domain model" fires ARCHITECTURE; DomainPacket must contain both ARCHITECTURE and IMPLEMENT packets.

Risk: "protect raw data endpoint" → SECURITY packet missing.
Correct: "protect" → SECURITY; DomainPacket must contain SECURITY packet.

### v6-03: multi-predicate collapsed at Node 01

Risk: Node 01 emits single DomainPacket for IMPLEMENT even when SECURITY + IMPLEMENT predicates both present.
Correct: Node 01 emits one DomainPacket per unique predicate — one SECURITY packet + one IMPLEMENT packet.

### Arbiter (Node 08) failure conditions confirmed

Node 08 (ArbiterUnit) fails with ArbiterVerdict: FAIL when:
1. Any public method does not return OperationResult<T>.
2. Any injectable dependency is constructed with new X() inside the service constructor.
Both conditions trigger FAIL regardless of which service is being reviewed.
