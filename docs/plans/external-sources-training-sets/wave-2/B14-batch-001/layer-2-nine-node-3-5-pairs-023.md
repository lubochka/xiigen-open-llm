# Layer 2 — Nine-Node Flow Pairs (023)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-023 — ProbationaryRejected — ARCHITECTURE+TEST infra validation flow rejected

Request: "Architect the infrastructure topology and test the network connectivity between VPC (Virtual Private Cloud) subnets."

### Flow Attempt: InfrastructureArchitectureFlow35

**Node 1 — DomainSplitter**
  Attempted: ARCHITECTURE → 3.5 + TEST → 5.1
  Domains: vpc_topology (ARCHITECTURE), connectivity_validation (TEST)

**Node 2 — DomainTranslator**
  vpc_topology: Public/private subnet boundary; NAT gateway; inter-subnet routing
  connectivity_validation: Ping tests? TCP connectivity tests? — TEST predicate for infrastructure is probationary

**Node 3 — SchemePlanner**
  Issue identified: "Test the network connectivity" — TEST fires but applies to infrastructure validation, not unit/integration test artifact creation
  Infrastructure connectivity testing is operational validation, not a test artifact production
  Probationary flag raised: TEST at sub-phase 5.1 is for test code; infrastructure connectivity testing is operational

**Node 4 — DomainProcessor**
  Probationary rejection: The TEST predicate at 5.1 expects test code (unit tests, integration tests, contract tests)
  "Test the network connectivity" maps to: operational validation script = IMPLEMENT (if script); or operational runbook = DOCUMENT
  Not a 5.1 test flow — reject TEST routing for this request

**Node 5 — Applier**
  Corrected routing: ARCHITECTURE → 3.5 (vpc topology design); IMPLEMENT → 4.1 (connectivity validation script)
  TEST at 5.1 is rejected for this request
  Corrected CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]

**Node 6 — ResultTranslator**
  Output: vpc-topology-adr.md + connectivity-validation-script.sh
  TEST flow instance: REJECTED (not a test code artifact)

**Node 7 — ArbiterSelector**
  SelectedArbiter: ArchitectureCompletenessArbiter
  Verdict for TEST rejection: CONFIRMED REJECT — TEST at 5.1 requires test code production, not operational validation

**Node 8 — Arbiter**
  ARCHITECTURE / vpc_topology: PASS (design decision + trade-off)
  IMPLEMENT / connectivity_validation: PASS (script = code artifact)
  TEST / 5.1: REJECTED — infrastructure connectivity test is not a test code artifact
  OverallVerdict: PASS with TEST routing corrected

**Node 9 — Formatter**
  Delivery: vpc-topology-adr.md + connectivity-validation-script.sh | Status: ready | Arbiter: PASS with correction
  Teaching: "test the network connectivity" = operational validation, not a TEST (5.1) artifact. Route to IMPLEMENT (script) instead.
