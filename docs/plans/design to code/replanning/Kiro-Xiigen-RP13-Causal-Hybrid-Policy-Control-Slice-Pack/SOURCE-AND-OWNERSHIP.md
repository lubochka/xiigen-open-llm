# RP13 — источники, глубина проверки и ownership

## Настоящие корни

- P = `lubochka/xiigen-planning@llm_plan3`; D=`P/llm mvp/design to code session`; C=`P/llm mvp/design to code session/replanning`.
- K = `lubochka/llm_mvp_core@codex/dpo-implementation`; G=`P/llm mvp/dpo legal build`; F=`P/llm mvp/llm dynamic flow`; V=`P/llm mvp/visual studio & visual studio code continue`. По необходимости — оригинальные реестры соседних P directories (language translation, orchestrator extension, core).
- Перед каждым live run получи новый SHA каждого требуемого repo, path/blob/line текущего primary, сохранение dirty worktree и разрешённый extent. Предыдущие commit IDs являются указателями, не гарантией актуальности.

## Первичный обязательный поиск (релевантные главы, не весь корпус в одно окно)

`D/original task.md`, `D/task prompt.md`, `D/Алгоритм создания ДПО.md`, `D/09.09.26WorkLeft-RESEARCH-01-training-quality-speed-accuracy-program.md`, `D/09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md`, `D/09.09.26WorkLeft-MECH-01-mechanism-validation.md`, `D/09.09.26WorkLeft-02-teaching-chinese-model.md`, `D/09.09.26WorkLeft-PLAN-HUMAN-01.md`, `D/09.09.26WorkLeft-RAG-00-usage-guide.md`, `D/09.09.26WorkLeft-RAG-MASTER-INDEX.md`, `D/Kiro-Xiigen-Original-Slice-Queue.md`. Проверить общие промпты/дополнительные задания вне Rxx. Каждый RAG relevant hit пройти **через MD или NDJSON до настоящего первичного документа в G/F/V**, отметить `INDEX_ONLY` vs `PRIMARY` vs `CODE` vs `CALL` vs `ACTUAL_RUN`.

Переиспользовать `C/previous rounds thinking results.md` (как исторический указатель, не окончательный verdict), `C/PARALLEL-R01-R03-JOIN.md`, `C/R01-R25-TO-CODE-SLICES-STRATEGY.md`, `C/RECOVERED-DESIGN-CONTRACT.md`, `C/R22-CAUSAL-EXPERIMENT-CATALOG.md`, `C/R23-PC-STATE-FEEDBACK-LIFECYCLE.md`, `C/R24-DTC-IDE-ORDINARY-ACCEPTANCE.md`, `C/R25-ASTRA-REVIEW-PACKET.md` и нужные `C/results/Rxx/{FINDINGS.md,SOURCES.csv,DECISIONS.md}`. R01/R03 primary traversal ещё был неполным, R07/R13 требовали repair и R25 Astra review не был проведён в прочитанной публикации: refresh actual, НЕ наследовать статус.

## Адреса core для проверки, НЕ подтверждённая сегодня функциональность

| Вопрос | Уже известный locator; обязательно заново смотреть full method/callers | Идентифицировать |
|---|---|---|
| Domain/vector/kNN | `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs`, `06.VectorSearch`, `17.DynamicDomains` | semantic representation, actual multi-hit consumption, k weighting/policy, right domain |
| Lossless pair / trainer | `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`, `11.DPO/DpoTrainer.cs`, `00.Core/LearningMachineContracts.cs` | Compact-before-Encode seam, chosen/rejected objective, exact mutable state |
| Typed route | `00.Core/LogicalContracts.cs` и live ILogicalUnit/Flow/Orchestrator implementations | OutputRefs/typed payload, source→next consumer, many→many semantics |
| Dynamic graph | `00.Core/DynamicGraph*Contracts.cs` и actual nonstub implementations | higher-order motif decisions, accepted edge authority and state |
| Ordinary product | `43.SmartSkillsDomain/XiigenExtensionApiService.cs`, `25.ModelServer/DesignConversion/DesignConversionModelServerLocalCallerAdapter.cs` | working ordinary calls vs partial/local adapter, current selected checkpoint |

Одна историческая трасса сообщает использование `hit[0]` у KnnLogicalUnit, другая — semantic loss при Compact; это **кандидаты** для гипотез/проверки, не исправления, не подтверждённый эффект. Слова `ILearningUnit`/`ILearningFlow` из требования не доказывают существование компилируемых одноимённых generic APIs: сначала найти настоящие contracts.

## Разграничение задач

| RP owner | Что оставляем ему | Что исследует/меняет только RP13 |
|---|---|---|
| RP01/RP05 | Student exact DPO, LoRA, full checkpoint, at-most-once feedback | адресация причины к Student либо не-Student; consumer receipt |
| RP02 | domain/IR/tokenization/vector and kNN implementation/state | выбрать допустимый route/когда использовать output; направить диагностический feedback владельцу |
| RP03 | Unit many→many & Flow learnable corrections | causal dependency/provenance, не менять Unit trainer |
| RP04 | async Orchestrator / higher-order DynamicGraph | сравнить пути и найти ответственную authority, без нового scheduler |
| RP07 | Builder candidate/teaching policy | handoff weakness с доказательством, не новый Builder |
| RP08 | cross-domain hybrid integration | **специфическая** per-decision causal blame + right-recipient policy, а не end-to-end port |
| RP09 | independent blind examiner/run accounting | использовать его реальные manifests/runner, никаких вторых оценщиков |
| RP06/S08/S09 | DTC/IDE host/release/native | cold inference proof через их *настоящий* consumer |

Для каждого seam: `primary→requirement_id→algorithm state owner→method/caller→consumer→falsifier→owner/action`. Грубый historical source-map без actual caller = не `CODE_TRACED`.
