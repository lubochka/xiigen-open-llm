# Layer 0-C — Knowledge-Edge Pairs (037)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-037-001 — DataAnonymization edges

**Active edges for DataAnonymization scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "write migration" + "anonymize" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "anonymize" (verb) | SECURITY (PII handling vocabulary) | Rule 14 |
| "PII" | SECURITY (absolute) | Rule 14 |
| "GDPR" | SECURITY (absolute) | Rule 14 |
| "sensitive data" | SECURITY (absolute) | Rule 14 |
| "anonymization pattern" | ARCHITECTURE (via "pattern") | Rule 7 |
| "anonymization architecture" | ARCHITECTURE (via "architecture") | Rule 7 |
| "write tests" | TEST | Rule 12 |

---

## B18-0C-037-002 — PIIMasking edges

**Active edges for PIIMasking scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "write migration" + "mask" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "mask" (verb) | SECURITY (PII masking = PII handling) | Rule 14 |
| "PII" | SECURITY (absolute) | Rule 14 |
| "credit card numbers" / "SSN" | SECURITY reinforcement (financial PII) | Rule 14 |
| "masking pattern" | ARCHITECTURE (via "pattern") | Rule 7 |
| "masking structure" | ARCHITECTURE (via "structure") | Rule 3 |

---

## B18-0C-037-003 — DataArchival edges

**Active edges for DataArchival scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "archive" (verb) / "write migration" (archive) | IMPLEMENT; feature_addition; 4.4 | Rule 2/5 |
| "archive" alone | NOT SECURITY (data operation without PII token) | Context-dependent |
| "PII" + "archive" | SECURITY (absolute) | Rule 14 |
| "GDPR" + "archive" | SECURITY (absolute) | Rule 14 |
| "encrypted at rest" + "archive" | SECURITY (absolute) | Rule 14 |
| "archival structure" | ARCHITECTURE (via "structure") | Rule 3 |
| "archival pattern" | ARCHITECTURE (via "pattern") | Rule 7 |

**Key edge:** "archive" alone ≠ SECURITY. "archive" + PII/GDPR/encrypted → SECURITY fires.

---

## B18-0C-037-004 — ShardingMigration edges

**Active edges for ShardingMigration scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "write migration" + "shard" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "restructure for sharding" | IMPLEMENT; refactoring; 4.4 | Rule 1 |
| "sharded architecture" | ARCHITECTURE (via "architecture") | Rule 7 |
| "shard key pattern" | ARCHITECTURE (via "pattern") | Rule 7 |
| "shard structure" | ARCHITECTURE (via "structure") | Rule 3 |
| "horizontal sharding" | 4.4 vocabulary (not SECURITY) | Context |

---

## B18-0C-037-005 — ReplicationSetup + ReadReplicaPromotion edges

**Active edges for replication scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "write migration" + "replication" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "promote the read replica" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "encrypted replication" | SECURITY (via "encrypted") | Rule 14 |
| "TLS" | SECURITY (encryption protocol) | Rule 14 |
| "replication architecture" | ARCHITECTURE (via "architecture") | Rule 7 |
| "replica architecture" | ARCHITECTURE (via "architecture") | Rule 7 |
| "replication setup" alone | NOT ARCHITECTURE | No absolute token |
