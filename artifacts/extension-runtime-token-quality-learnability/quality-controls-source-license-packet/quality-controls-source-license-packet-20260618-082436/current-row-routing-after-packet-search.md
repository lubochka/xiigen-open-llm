# Current Row Routing After Packet Search

## Counts

- COMMON_ALLOWED: 0/12 scenarios, 0/54 pair rows.
- USER_ADAPTIVE_ONLY: 7/12 scenarios, 21/54 pair rows.
- EVIDENCE_ONLY: 5/12 scenarios, 33/54 pair rows.
- QUARANTINE: 0/12 scenarios, 0/54 pair rows.

Все 12/54 rows остаются blocked для common route. `USER_ADAPTIVE_ONLY` здесь означает только future user-local/adaptive route после отдельного approval и manifest; это не training approval.

## Scenario Routes

| Scenario | Route | Pair rows | Reason |
|---|---:|---:|---|
| `TQ-001-prompt-task-documentation` | `USER_ADAPTIVE_ONLY` | 3 | Источник помечен как user-local/private; common training запрещен без permissive license packet. Позже row может идти только в user-local/adaptive контур после отдельного approval/store manifest/DPO quality gate, без переноса в common. |
| `TQ-002-prompt-task-interrupt` | `USER_ADAPTIVE_ONLY` | 3 | Источник помечен как user-local/private; common training запрещен без permissive license packet. Позже row может идти только в user-local/adaptive контур после отдельного approval/store manifest/DPO quality gate, без переноса в common. |
| `TQ-003-prompt-task-redaction` | `USER_ADAPTIVE_ONLY` | 3 | Источник помечен как user-local/private; common training запрещен без permissive license packet. Позже row может идти только в user-local/adaptive контур после отдельного approval/store manifest/DPO quality gate, без переноса в common. |
| `TQ-004-orchestration-runtime-proof` | `EVIDENCE_ONLY` | 3 | Источник является project-local/control evidence с неизвестной лицензией; он может объяснять review/gate status, но не становится common training и не становится user-adaptive training row без отдельного derived/user-local packet. |
| `TQ-005-orchestration-token-pairing` | `EVIDENCE_ONLY` | 3 | Источник является project-local/control evidence с неизвестной лицензией; он может объяснять review/gate status, но не становится common training и не становится user-adaptive training row без отдельного derived/user-local packet. |
| `TQ-006-orchestration-learning-lifecycle` | `USER_ADAPTIVE_ONLY` | 3 | Источник помечен как user-local/private; common training запрещен без permissive license packet. Позже row может идти только в user-local/adaptive контур после отдельного approval/store manifest/DPO quality gate, без переноса в common. |
| `TQ-007-skill-architecture-audit` | `USER_ADAPTIVE_ONLY` | 3 | Источник помечен как user-local/private; common training запрещен без permissive license packet. Позже row может идти только в user-local/adaptive контур после отдельного approval/store manifest/DPO quality gate, без переноса в common. |
| `TQ-008-skill-debug-runtime` | `USER_ADAPTIVE_ONLY` | 3 | Источник помечен как user-local/private; common training запрещен без permissive license packet. Позже row может идти только в user-local/adaptive контур после отдельного approval/store manifest/DPO quality gate, без переноса в common. |
| `TQ-009-skill-learning-safety` | `USER_ADAPTIVE_ONLY` | 3 | Источник помечен как user-local/private; common training запрещен без permissive license packet. Позже row может идти только в user-local/adaptive контур после отдельного approval/store manifest/DPO quality gate, без переноса в common. |
| `TQ-010-mixed-no-overclaim` | `EVIDENCE_ONLY` | 9 | Источник является project-local/control evidence с неизвестной лицензией; он может объяснять review/gate status, но не становится common training и не становится user-adaptive training row без отдельного derived/user-local packet. |
| `TQ-011-mixed-shortcut-audit` | `EVIDENCE_ONLY` | 9 | Источник является project-local/control evidence с неизвестной лицензией; он может объяснять review/gate status, но не становится common training и не становится user-adaptive training row без отдельного derived/user-local packet. |
| `TQ-012-mixed-final-readiness` | `EVIDENCE_ONLY` | 9 | Источник является project-local/control evidence с неизвестной лицензией; он может объяснять review/gate status, но не становится common training и не становится user-adaptive training row без отдельного derived/user-local packet. |
