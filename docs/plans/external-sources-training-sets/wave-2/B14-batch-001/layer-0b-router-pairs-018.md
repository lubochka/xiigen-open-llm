# Layer 0-B — Router Pairs (018)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-018-001 — ARCHITECTURE → 3.5 (DR topology)

A:
  compiled_predicate_ids: ["ARCHITECTURE"]
  compiled_text: "Design the disaster recovery topology — active-passive failover, RTO < 15 minutes, RPO < 5 minutes."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'design' + 'disaster recovery topology' + 'active-passive failover' → ARCHITECTURE → 3.5"
  TransitionType: feature_addition
  SubPhaseId: 3.5

---

## B14-router-018-002 — ARCHITECTURE+DOCUMENT → 3.5 + 6.2 (DR runbook)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "DOCUMENT"]
  compiled_text: "Architect the disaster recovery failover procedure and document the runbook as an operational guide."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'architect' + 'disaster recovery failover procedure' → ARCHITECTURE → 3.5"
    - predicate: DOCUMENT
      sub_phase: 6.2
      routing_note: "'document' + 'runbook' → DOCUMENT → 6.2"
  TransitionType: documentation
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 6.2]

---

## B14-router-018-003 — ARCHITECTURE+IMPLEMENT → 3.5 + 4.1 (DR IaC)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "IMPLEMENT"]
  compiled_text: "Design the multi-region DR architecture and implement the Terraform failover configuration."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'design' + 'multi-region DR architecture' → ARCHITECTURE → 3.5"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      routing_note: "'implement' + 'Terraform failover configuration' → IMPLEMENT → 4.1"
  TransitionType: feature_addition
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 4.1]

---

## B14-router-018-004 — TEST → 5.1 (test DR failover)

A:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Test the disaster recovery failover by promoting the replica."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      routing_note: "'test' → TEST → 5.1; 'disaster recovery' alone without design framing does not fire ARCHITECTURE"
  TransitionType: feature_addition
  SubPhaseId: 5.1
  Note: "TEST fires on 'test'. ARCHITECTURE does not fire without design/architect/define vocabulary."
