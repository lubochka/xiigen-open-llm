# Universal common vs adaptive/quarantine ledger

Run: `quality-controls-source-license-partition-20260618-073408`

Итог: `0/12` scenarios разрешены для common training. Все строки — evidence-only/user-local/quarantine до license/source split approval.

## Rows

- `TQ-001-prompt-task-documentation`: class=`universal_task_semantics`, common=`COMMON_REJECTED_LICENSE_UNKNOWN`, adaptive/evidence=`USER_LOCAL_ONLY_OR_EVIDENCE_ONLY_LICENSE_UNKNOWN`, failure=`FAIL_PRIVATE_OR_USER_PREF_AS_COMMON; FAIL_COMMON_LICENSE_NOT_PERMISSIVE`.
- `TQ-002-prompt-task-interrupt`: class=`universal_task_semantics`, common=`COMMON_REJECTED_LICENSE_UNKNOWN`, adaptive/evidence=`USER_LOCAL_ONLY_OR_EVIDENCE_ONLY_LICENSE_UNKNOWN`, failure=`FAIL_PRIVATE_OR_USER_PREF_AS_COMMON; FAIL_COMMON_LICENSE_NOT_PERMISSIVE`.
- `TQ-003-prompt-task-redaction`: class=`universal_task_semantics`, common=`COMMON_REJECTED_LICENSE_UNKNOWN`, adaptive/evidence=`USER_LOCAL_ONLY_OR_EVIDENCE_ONLY_LICENSE_UNKNOWN`, failure=`FAIL_PRIVATE_OR_USER_PREF_AS_COMMON; FAIL_COMMON_LICENSE_NOT_PERMISSIVE`.
- `TQ-004-orchestration-runtime-proof`: class=`universal_orchestration_semantics`, common=`COMMON_REJECTED_LICENSE_UNKNOWN`, adaptive/evidence=`EVIDENCE_ONLY_LICENSE_UNKNOWN`, failure=`FAIL_COMMON_LICENSE_NOT_PERMISSIVE`.
- `TQ-005-orchestration-token-pairing`: class=`universal_orchestration_semantics`, common=`COMMON_REJECTED_LICENSE_UNKNOWN`, adaptive/evidence=`EVIDENCE_ONLY_LICENSE_UNKNOWN`, failure=`FAIL_COMMON_LICENSE_NOT_PERMISSIVE`.
- `TQ-006-orchestration-learning-lifecycle`: class=`universal_orchestration_semantics`, common=`COMMON_REJECTED_LICENSE_UNKNOWN`, adaptive/evidence=`USER_LOCAL_ONLY_OR_EVIDENCE_ONLY_LICENSE_UNKNOWN`, failure=`FAIL_PRIVATE_OR_USER_PREF_AS_COMMON; FAIL_COMMON_LICENSE_NOT_PERMISSIVE`.
- `TQ-007-skill-architecture-audit`: class=`universal_skill_selection_semantics`, common=`COMMON_REJECTED_LICENSE_UNKNOWN`, adaptive/evidence=`USER_LOCAL_ONLY_OR_EVIDENCE_ONLY_LICENSE_UNKNOWN`, failure=`FAIL_PRIVATE_OR_USER_PREF_AS_COMMON; FAIL_COMMON_LICENSE_NOT_PERMISSIVE`.
- `TQ-008-skill-debug-runtime`: class=`universal_skill_selection_semantics`, common=`COMMON_REJECTED_LICENSE_UNKNOWN`, adaptive/evidence=`USER_LOCAL_ONLY_OR_EVIDENCE_ONLY_LICENSE_UNKNOWN`, failure=`FAIL_PRIVATE_OR_USER_PREF_AS_COMMON; FAIL_COMMON_LICENSE_NOT_PERMISSIVE`.
- `TQ-009-skill-learning-safety`: class=`universal_skill_selection_semantics`, common=`COMMON_REJECTED_LICENSE_UNKNOWN`, adaptive/evidence=`USER_LOCAL_ONLY_OR_EVIDENCE_ONLY_LICENSE_UNKNOWN`, failure=`FAIL_PRIVATE_OR_USER_PREF_AS_COMMON; FAIL_COMMON_LICENSE_NOT_PERMISSIVE`.
- `TQ-010-mixed-no-overclaim`: class=`universal_mixed_task_orchestration_skill_semantics`, common=`COMMON_REJECTED_LICENSE_UNKNOWN`, adaptive/evidence=`EVIDENCE_ONLY_LICENSE_UNKNOWN`, failure=`FAIL_COMMON_LICENSE_NOT_PERMISSIVE`.
- `TQ-011-mixed-shortcut-audit`: class=`universal_mixed_task_orchestration_skill_semantics`, common=`COMMON_REJECTED_LICENSE_UNKNOWN`, adaptive/evidence=`EVIDENCE_ONLY_LICENSE_UNKNOWN`, failure=`FAIL_COMMON_LICENSE_NOT_PERMISSIVE`.
- `TQ-012-mixed-final-readiness`: class=`universal_mixed_task_orchestration_skill_semantics`, common=`COMMON_REJECTED_LICENSE_UNKNOWN`, adaptive/evidence=`EVIDENCE_ONLY_LICENSE_UNKNOWN`, failure=`FAIL_COMMON_LICENSE_NOT_PERMISSIVE`.
