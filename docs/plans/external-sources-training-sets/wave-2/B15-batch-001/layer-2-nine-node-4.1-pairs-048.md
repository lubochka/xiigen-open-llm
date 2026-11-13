# Layer 2 — Nine-Node Flow Pairs (048)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: nine_node_flow

## B15-nine-048 — Complete nine-node 4.1 rule summary for 025–047 extension

### Key rules confirmed for 025–047

1. Node 01 A field = SelectedLogicalTransition (not raw user request) — confirmed for all 025–044 services.
2. Node 03 probationary_accepted → candidate flow executes — all 025–044 services: deterministic_generic → probationary_accepted.
3. TransitionType: refactoring → backward-compatibility required — confirmed: ExportService (032), CompensationService (037), CacheService (039).
4. IMPLEMENT+ARCHITECTURE → architecture output is N01 input for implementation — confirmed: ComplianceService (028), SagaOrchestrator (036), RateLimiterService (040), CircuitBreakerService (041), RetryService (042).
5. IMPLEMENT+API_CONTRACT → contract output feeds implementation — confirmed: EventPublisherService (035), ImportService (033), IdempotencyService (038), DeadLetterService (043).
6. QA always last — confirmed: MonitoringService (044).
7. OperationResult<T> on all public methods — confirmed across all 20 services in 025–044.
8. Fabric-first — confirmed across all 20 services.
9. No GapHandler in B15 — confirmed for 025–044.
10. DocumentationFlow21 parallel-eligible with ImplementationFlow41 — confirmed: ConsentService (029), MigrationService (034), RetryService (042), DeadLetterService (043), MonitoringService (044).
