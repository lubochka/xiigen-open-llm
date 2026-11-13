# P19 evidence card

P19 проверяет, можно ли P17E SkillSelection production-export candidate продвигать в runtime/common model path.

## Решение

Direct promotion заблокирован. Candidate остаётся artifact-only.

## Evidence

- Existing runtime/common path существует и загружается через `XiigenExtensionApiService`.
- P17E candidate manifest говорит `accuracyAtExport=0` и `semanticQualityClaimed=false`.
- P17E locator говорит `runtime_wired=false`, `common_model_path_updated=false`, `final_model_path_updated=false`.
- Existing common checkpoint больше: 1309772 bytes против 17711 bytes у candidate.
- Existing common state содержит 900 trained pair ids; candidate содержит 19.

## Что P19 не делал

- Не копировал candidate в `models/skill-selection/skill-selection-v1`.
- Не менял runtime behavior.
- Не запускал training.
- Не заявлял semantic quality или final readiness.
