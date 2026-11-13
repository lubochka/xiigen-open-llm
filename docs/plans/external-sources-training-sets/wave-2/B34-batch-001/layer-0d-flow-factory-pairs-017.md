# Layer 0-D — Flow Factory Pairs (017)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-017-001 — "Advise" vocabulary: factory map

| Request | Predicates | Factory | TransitionType |
|---------|-----------|---------|----------------|
| "Advise on best practices" | [] | GapHandler | none |
| "Advise on security" | [] | GapHandler | none |
| "Advise and design the architecture" | [ARCHITECTURE] | ArchitectureFlow31 | documentation |
| "Advise and harden the gateway" | [SECURITY] | SecurityAuditFlow | documentation |
| "Advise and implement the solution" | [IMPLEMENT] | ImplementationFlow41 | feature_addition |
| "Advise and document the guidelines" | [DOCUMENT] | DocumentationFlow | documentation |
| "Advise and integrate the new system" | [API_CONTRACT] | APIContractFlow33 | documentation |

---

## FF-017-002 — Advisory + code factory chaining examples

"Advise on the data model and design the database schema" → [ARCHITECTURE] → ArchitectureFlow31
"Advise on the security posture and harden the endpoints and write security tests" → [SECURITY, TEST] → SecurityAuditFlow + UnitTestingFlow51

TransitionType always follows the highest-priority code predicate present:
- IMPLEMENT present → feature_addition
- Only ARCHITECTURE/DOCUMENT/TEST/API_CONTRACT/SECURITY → documentation

---

## FF-017-003 — "Advise to [action]" vs "advise and [action]"

"Advise the team to implement X" → GapHandler (advise + recommendation to others = gap; requester does not implement)
"Advise on approach and implement X" → ImplementationFlow41 (requester implements)
"Recommend the team build the feature" → GapHandler (recommend + third-party action = gap)
"Recommend and then build the feature" → ImplementationFlow41 (requester builds)
