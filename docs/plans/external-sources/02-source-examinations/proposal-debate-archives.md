# Source Examination: Proposal and Debate Archives
## Sources: RUST-RFCS, PYTHON-PEPS, GO-PROPOSALS, KAFKA-KIPS, CLOUDEVENTS-SPEC
## Priority: 4 (families 1, 5, 8 — highest richness for reprocessing rows)
## Date examined: 2026-05-04

---

## 1. Sources Examined

| Source ID | Title | Trust tier | Why included |
|-----------|-------|-----------|-------------|
| RUST-RFCS | Rust RFCs | 2 | MIT; each RFC is a full proposal cycle: motivation → design → drawbacks → alternatives → unresolved questions → disposition |
| PYTHON-PEPS | Python PEPs | 2 | Public domain; structured lifecycle from draft to accepted/rejected/withdrawn with detailed rationale |
| GO-PROPOSALS | Go Proposals | 2 | BSD; Google-authored proposals with explicit acceptance or rejection decisions |
| KAFKA-KIPS | Apache Kafka KIPs | 1 | Apache 2.0; KIPs document distributed system design decisions with backward compatibility analysis |
| CLOUDEVENTS-SPEC | CloudEvents Specification closed issues | 1 | Apache 2.0; spec-level debates about event schema governance |

These sources are the richest for families 1, 5, and 8:
- Family 1 (prompt to commands): the "problem statement" section maps to a user intent
- Family 5 (arbitration): the "disposition" section is an arbiter verdict with full context
- Family 8 (reprocessing): the "revision history" and "rejected alternatives" sections are reprocessing decisions

---

## 2. Architecture Challenges Found

| Challenge ID | Challenge statement | Source anchor | XIIGEN domain |
|-------------|--------------------|--------------|--------------| 
| PROP-01 | A language feature needs to be added; the proposal must decide whether to implement it in the compiler, as a library, or not at all | Rust RFC 2229 "Closures Capture Disjoint Fields" — disposition: accepted after revision; Rust RFC 1737 "Minimal Multidispatch" — disposition: postponed | H_GOVERNANCE |
| PROP-02 | A pattern matching syntax proposal conflicts with an existing syntax; the arbiter must choose between backward compatibility and ergonomics | Python PEP 634 "Structural Pattern Matching" — opposed patterns section; Python PEP 642 (rejected alternative syntax) | H_GOVERNANCE |
| PROP-03 | A breaking change to a language feature is needed for correctness; the upgrade path must be designed to avoid breaking existing code | Python PEP 479 "Change StopIteration handling inside generators" — final disposition: accepted; transition period with __future__ import | H_GOVERNANCE |
| PROP-04 | A Kafka consumer group rebalance protocol is causing latency spikes; a new protocol must be designed that does not interrupt consumers during rebalance | KIP-848 "The Next Generation of the Consumer Rebalance Protocol" — motivation: current eager rebalance stops all consumers | A_TOPOLOGY |
| PROP-05 | ZooKeeper is a mandatory dependency for Kafka; the proposal must design a path to remove it while maintaining backward compatibility | KIP-500 "Replace ZooKeeper with a Self-Managed Metadata Quorum" — multi-year adoption plan | A_TOPOLOGY |
| PROP-06 | A CloudEvents attribute is added to the spec; the debate must decide whether it should be a required attribute, optional, or extension-only | CloudEvents spec issue: "Should datacontenttype be a REQUIRED or OPTIONAL attribute" (resolved: OPTIONAL) | F_REGISTRY_STORAGE |
| PROP-07 | A Go proposal is submitted to add generics; the design space is large and multiple approaches are in conflict | Go proposal #43651 "spec: add generic programming using type parameters" — alternatives section: contracts vs type sets | H_GOVERNANCE |
| PROP-08 | A Rust RFC proposes async closures; the interaction with existing trait objects creates soundness issues; the arbiter must decide whether to accept with limitations or defer | Rust RFC 3668 "async closures" — disposition: merged with trait object limitations documented | H_GOVERNANCE |

---

## 3. Patterns Found

| Pattern ID | Pattern name | When to use | When NOT to use | Source anchor |
|-----------|-------------|------------|----------------|--------------|
| PAT-PROP-01 | RFC/PEP proposal structure (motivation → design → drawbacks → alternatives → unresolved) | Use for any substantial design decision that affects multiple teams or long-term architecture; creates a complete decision record | Do not use for small local changes; the proposal overhead is not justified when the decision scope is a single module or component | Rust RFC template; PEP 0 |
| PAT-PROP-02 | Staged rollout with feature flag | Use when a breaking change must be adopted gradually; existing code continues to work under the old behavior until an opt-in period ends, then the new behavior becomes default | Do not use when backward compatibility cannot be maintained in any form — a staged rollout requires the old and new behaviors to coexist | Python PEP 479 §Transition period |
| PAT-PROP-03 | Incremental protocol migration with version negotiation | Use when a new protocol version must replace an old one; clients negotiate which version they support; old protocol remains active until all clients have migrated | Do not plan a hard cutoff date before measuring actual migration progress; protocol versions must be removable based on usage data, not schedule | KIP-500 §Migration plan; KIP-848 §Rollout |
| PAT-PROP-04 | Extension-only addition for controversial attributes | Use for spec additions where community consensus is not achievable; the attribute goes into the extension namespace, not core | Do not keep contested attributes as core forever; extension-only prevents fragmentation while keeping the core stable | CloudEvents closed issues |

---

## 4. Anti-Patterns Found

| Anti-pattern ID | Bad design | Failure mode | Better design | Source anchor |
|----------------|-----------|-------------|--------------|--------------|
| ANTI-PROP-01 | Big-bang protocol replacement without migration plan | All clients must upgrade simultaneously; any client that lags blocks or breaks the migration; there is no rollback path | Design the new protocol to negotiate with the old; keep both active; measure migration progress; remove old protocol only when usage is near zero | KIP-500 §Migration plan |
| ANTI-PROP-02 | Accepting a proposal without resolving "unresolved questions" | The implementation proceeds with ambiguous behavior; different implementors make different choices; the language or protocol fragments | Defer acceptance until all unresolved questions are answered or explicitly designated as "resolved by implementation" | Rust RFC template §Unresolved questions |
| ANTI-PROP-03 | Introducing a new spec attribute as required without deprecation period | Existing implementations that do not include the attribute become non-conformant immediately; backward compatibility breaks at the next conformance check | Add new required attributes as optional first; measure adoption; make required in the next major version with documented migration period | CloudEvents spec governance |
| ANTI-PROP-04 | Rejecting a proposal without documenting alternatives considered | Future contributors re-open the same debate without knowing what was already tried; the same design space is explored multiple times | Every rejection documents the alternatives considered and why each was insufficient; this becomes the authoritative record for future proposals | Rust RFC disposition template |

---

## 5. Arbiter Rules Extracted

| Arbiter ID | What it checks | Positive signal | Negative signal | Source anchor |
|-----------|--------------|----------------|----------------|--------------|
| proposalCompletenessArbiter | A design proposal has all required sections before review | Motivation, design, drawbacks, alternatives, and unresolved questions are all present and non-empty | Any section is empty or marked TODO; the proposal proceeds to review without resolving its open questions | Rust RFC template; PEP 0 |
| backwardCompatibilityArbiter | A breaking change provides a migration path with transition period | A migration period is defined; old behavior is available under a feature flag or version; cutoff date is based on adoption metrics | Breaking change takes effect immediately; no migration period; old behavior is removed in the same release that introduces the change | Python PEP 479; KIP-500; KIP-848 |
| protocolMigrationArbiter | Protocol replacement proceeds incrementally with version negotiation | New and old protocols coexist; version negotiation is explicit; migration progress is measured before removing old protocol | Protocol replacement is a single cutover; no version negotiation; old protocol is removed on a fixed date regardless of migration progress | KIP-500; KIP-848 |
| specAttributePromotionArbiter | A spec attribute is promoted from optional to required only after measured adoption | Adoption of the optional attribute exceeds a threshold; a deprecation notice was issued before promotion; existing implementations have had time to adopt | Attribute promoted from optional to required without measuring adoption; implementations become non-conformant without warning | CloudEvents spec governance |

---

## 6. Candidate Training Rows

| Row ID | Dataset family | Domain | Status |
|--------|--------------|--------|-------|
| EXT-PROP-001 | 5 — arbitrateResultI_with_resultsContext | H_GOVERNANCE | draft — proposal completeness arbiter |
| EXT-PROP-002 | 5 — arbitrateResultI_with_resultsContext | H_GOVERNANCE | draft — backward compatibility arbiter |
| EXT-PROP-003 | 5 — arbitrateResultI_with_resultsContext | A_TOPOLOGY | draft — protocol migration arbiter |
| EXT-PROP-004 | 5 — arbitrateResultI_with_resultsContext | F_REGISTRY_STORAGE | draft — spec attribute promotion arbiter |
| EXT-PROP-005 | 8 — historyAndVersions_to_reprocessingDecision | H_GOVERNANCE | draft — Kafka ZooKeeper removal migration decision |
| EXT-PROP-006 | 8 — historyAndVersions_to_reprocessingDecision | H_GOVERNANCE | draft — Python PEP 479 staged rollout decision |
| EXT-PROP-007 | 1 — userPrompt_to_machineCommands | H_GOVERNANCE | draft — RFC proposal structure decomposition |
| EXT-PROP-008 | 8 — historyAndVersions_to_reprocessingDecision | A_TOPOLOGY | draft — Kafka rebalance protocol replacement decision |

---

## 7. Gaps

- Rust RFCs by comment count (top 20) are not yet examined individually — needed for family 5 decision rows with full debate context
- Python rejected PEPs (PEP 3148, PEP 526 alternatives) — needed for negative example rows
- Go generics alternatives history (type theory debates) — needed for type system arbiter rows
- KAFKA-KIPS KIP-98 (exactly-once semantics) — needed for distributed data consistency rows
