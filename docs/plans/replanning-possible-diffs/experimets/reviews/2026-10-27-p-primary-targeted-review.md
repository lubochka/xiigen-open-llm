# Адресный review новых PRIMARY: 27 октября 2026

Worker: `/root/transfer_math_audit`. Фаза READ_ONLY_RESEARCH + Markdown. Это отдельный review **родительских** P42, P64, P92, P94, P111, P118 и общих 34/35. Прочитаны новые верхние паспорта, назначенные методы и ограниченные fixture/evidence; 470 исторических подслучаев не перечитывались. Собственные 50 P-паспортов и TTQ02 — авторская работа с self-readback, описанная в [promotion evidence](2026-10-27-p-card-promotion-evidence.md), а не независимый review.

## Найденные несоответствия и readback исправлений

| Поверхность | Доказанная проблема первоначального паспорта | Минимальное исправление и прочитанное состояние |
|---|---|---|
| [P42](../cards/P42-checker-arbiter.md) | A «request+answer» не соответствует current source. `QualityOrchestrator.Check` проверяет длину/result return; `InternalResultExaminer.Examine` ищет predicates только в response. EN `not` при простом concat не становится RU `не`. | Parent назначил явную experimental ingress-абляцию: native Examine, predicates=[не], A response=`"не "+candidate`, B=response. Actual и synthetic candidates различены; только ошибочный actual порождает corrective lesson. Прочитано исправленное описание. |
| [P64](../cards/P64-tier-postfilter.md) | Native writer автоматически кладёт каждый target ещё в PrefixSignature/StructureSignature. При совпадении full/prefix правильный третий target может попасть в full bucket и уничтожить контраст. Условного переименования только третьего ключа недостаточно. | Обязательные controlled `P64_FULL`/`P64_PREFIX` у writer и component reader, key-collision guard; native дополнительные записи сохранены. Owner исправлен на FindNearestCascade. Ordinary Flow отделён от доказательства cascade. Readback подтвердил. |
| [P92](../cards/P92-dpo-memory-flow-bridge.md) | Projector и Project internal; отсутствие Unit возвращает UniversalNineNodeDevelopmentFlow, а не no-hit. Factory default domain=subPhaseId=plan не совпадает с обучаемым DomainTag=orchestration. | Используются публичные TransformerKnnTrainingExample напрямую, с явной границей «не production admission/DPO». A исполняет настоящий fallback; B — настоящий learned Flow. Role/trust, UseLearnedUnitWhenAvailable=true и unitDomainTag=orchestration назначены явно. Readback всех трёх исправлений подтверждён. |
| [P94](../cards/P94-typed-many-results.md), [P111](../cards/P111-reward-credit.md) | Native KnnLogicalUnit не допускает Train(empty), private ctor не предоставляет пустой instance. Cold нельзя считать native retrieval. P111 restoration через дополнительный Train превысил бы budget. | Cold теперь NO_TRAINED_UNIT до регистрации. P94: 5 cold + 10 retrieval. P111: 2 cold + 14 retrieval, 6 lesson presentations; S0 — неизменные исходные instances, восстановление ссылок без нового fit. Содержательный readback подтверждён. |
| [34](../34-P-experiment-execution-contract.md) | Приоритет индивидуального deadline был дан только neural, хотя native паспорта тоже имеют явные пределы. | Явный budget/deadline любого PRIMARY теперь приоритетен. Добавлено общее различение cold observation/native query/updates. Readback подтверждён. |
| [35](../35-P-fixture-packs.md) | U-OR cold panel мог трактоваться как невозможный пустой native KNN; заголовок FORMULA говорил о двух формулах при четырёх строках. | 12 NO_TRAINED_UNIT для пустого KNN, baseline Flow только при прямом назначении; четыре формулы. Readback подтверждён. |

Первичные источники для выводов:

- [QualityOrchestrator.Check](C:/Projects/llm_mvp_core/15.OrchestratorModel/QualityOrchestrator.cs:35), [InternalResultExaminer.Examine](C:/Projects/llm_mvp_core/22.AgentAlgorithms/Pipeline/InternalFirstAiSupport.cs:89).
- [ExecutorUnit.TrainOnExample](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:405): записи signature/prefix/structure на строках 417–422; [FindNearestCascade](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationUnitContracts.cs:246); [full/prefix construction](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationSemanticTokenizer.cs:124).
- [internal projector](C:/Projects/llm_mvp_core/18.CapabilityTraining/GeneratedCapabilityExecutableManifestProjector.cs:321), [публичный training DTO](C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:30), [native memory Train](C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:99), [factory ctor/Create](C:/Projects/llm_mvp_core/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs:7292).
- [KnnLogicalUnit.Train(empty) refusal](C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:120).

## Проверенные счета и отсутствие дополнительных выводов

| Паспорт/набор | Конечный счёт по исправленному документу |
|---|---|
| P42 | 8 setup lessons, до 4 corrections; 14 Flow, 8 checker calls. Отсутствие ошибочного actual даёт NO_CONTRAST учебной части. |
| P64 | 4 lessons; 6 ordinary Flow + 4 component + 2 component после reload = 12 readouts. |
| P92 | 1 memory fit на 8 строках; 24 Flow; cap 20 encoder Embed/Forward = 8 fit + 12 B; 0 gradient updates. Это новая retrieval memory на frozen encoder, не обучение encoder. |
| P94 | 4 lessons; 5 cold observations + 10 retrieval; cap 16 node calls. |
| P111 | 6 lesson presentations; 2 cold + 14 native queries. Отказ missing owner проверяется до восстановления/repair-fit. |
| P118 | 4 независимых machines × 2 lessons = 8; 2 cold + 8 trained GeneratePlanAsync = 10. Mode не форсируется; actual route определяет границу вывода. |
| 35 | U-OR: 8 TRAIN/4 EXAM; M-CODE: 8 file-local pairs; SEQ-CODE: 12 pairs и 200×12=2400 updates на arm; T-ENRU: 8/8; FORMULA: 4/4; DOC-FACT: 4/4. Это счёт назначенных данных, не результат их усвоения. |

Для [P118](../cards/P118-projectwiki-content.md) подтверждён hash назначенного `results/2026-10-20/IDENTITY-M1-01/A/states.json`: `e74255093298c06374dc1a1e7d52aee57bf06da3d9eabf3ceb9915cd94397144`. [TrainOrchestrationBatchAsync](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/OrchestrationPlanMachine.cs:172) действительно разделяет Mode1/Mode2 writers, а [GeneratePlanAsync](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/OrchestrationPlanMachine.cs:60) выбирает фактический learned/fallback route. Дополнительного substantive defect этого выбранного паспорта в заданном scope не установлено; это не подтверждение будущего effect.

Конфигурация P92 сохраняет одну фактическую initialization в frozen checkpoint с SHA. Не назначен несуществующий seed-параметр LlmModel. После snapshot повторное случайное создание encoder не считается restore. Успешный Build, регистрация, output shape и точные runtime counters остаются будущей квалификацией, не доказанной этим review.

## Границы evidence

Паспорта исходно закреплены за `6e58d068db4de36b82afff49fd8a8f98eba8eb9c`. Последний read-only `git rev-parse HEAD` во время завершения дал `dc8eb29bd3a8fa0429d0263cb056799b3ae25f25`: shared checkout двигался параллельно. Review опирается на указанные фактически прочитанные source и live Markdown; полнота межкоммитного audit не заявляется. Перед будущим выполнением 34 требует сохранить реальные source identities и явный A/B patch.

Правки родительских карточек выполнял parent; worker сообщил конкретные замечания и прочитал исправленные участки. В текущей работе не создавались runtime hosts, datasets, model state или product deltas, не выполнялись build/test/train/inference. Никакое число строк, карточек или исправленных замечаний здесь не обозначает число экспериментальных циклов.

