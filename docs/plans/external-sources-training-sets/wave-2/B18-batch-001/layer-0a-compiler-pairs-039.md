# Layer 0-A — Compiler Pairs (039)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-039-001 — ReplicationSetup: predicate firing

**Request:** "Write a migration to configure streaming replication between primary and replica databases."

**Token scan:**
- "write a migration" → IMPLEMENT fires (Rule 2)
- "streaming replication" → 4.4 vocabulary (database replication setup)
- "primary and replica databases" → 4.4 infrastructure vocabulary
- No "structure", "architecture", "pattern" → ARCHITECTURE does not fire
- No security tokens → SECURITY does not fire

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-039-002 — ReplicationSetup with ARCHITECTURE

**Request:** "Design the replication architecture for a primary/replica cluster and write the setup migration."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "replication architecture" → "architecture" → ARCHITECTURE fires (Rule 7, absolute)
- "write the setup migration" → IMPLEMENT fires (Rule 2)
- "primary/replica cluster" → 4.4 vocabulary
- No security or TEST tokens

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-039-003 — ReadReplicaPromotion: predicate firing

**Request:** "Write a migration script to promote the read replica to primary during failover."

**Token scan:**
- "write a migration script" → IMPLEMENT fires (Rule 2)
- "promote the read replica" → 4.4 vocabulary (database replication promotion)
- "failover" → 4.4 infrastructure vocabulary
- No ARCHITECTURE, SECURITY, or TEST tokens

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-039-004 — ReadReplicaPromotion with TEST

**Request:** "Write the replica promotion migration and write tests to verify the new primary handles all write operations correctly."

**Token scan:**
- "write the replica promotion migration" → IMPLEMENT fires (Rule 2)
- "write tests" → TEST fires (Rule 12: verb form "write tests")
- "replica promotion" → 4.4 vocabulary
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "TEST"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 5.x
- DomainId: software-development

---

## B18-0A-039-005 — ReplicationSetup with SECURITY

**Request:** "Set up encrypted replication between database nodes and write the migration to configure TLS for replication traffic."

**Token scan:**
- "write the migration" → IMPLEMENT fires (Rule 2)
- "encrypted replication" → "encrypted" → SECURITY fires (Rule 14, absolute)
- "TLS" (Transport Layer Security) → SECURITY fires (Rule 14: encryption protocol)
- "replication between database nodes" → 4.4 vocabulary
- No ARCHITECTURE tokens

**Output:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- sub_phase: 4.4 + security
- DomainId: software-development
