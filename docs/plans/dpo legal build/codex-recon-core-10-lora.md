# Codex Recon — core/10.LoRA (LoRA-адаптеры/дообучение, роль в обучении юнитов)

Рекон для плана ILearningDynamicGraph. Только чтение + запись этого файла. Никакого кода/тестов/раннеров/git.

## Что это и где (точные ключевые пути)

Проект существует и НЕ пустой. Исходники (4 файла):
- `C:/Projects/llm_mvp_core/10.LoRA/LoRA.csproj` — ссылки на `00.TensorCore`, `03.TransformerModel`, `04.TrainingLoop`.
- `C:/Projects/llm_mvp_core/10.LoRA/LoraConfig.cs` — record `LoraConfig` (Rank=8, Alpha=16, Dropout=0.05, TargetModules=["q_proj","v_proj"], Scaling => Alpha/Rank).
- `C:/Projects/llm_mvp_core/10.LoRA/LoraLinear.cs` — класс `LoraLinear : Module` (адаптер поверх замороженного `Linear`).
- `C:/Projects/llm_mvp_core/10.LoRA/LoraApplicator.cs` — статический `LoraApplicator.Apply(...)` + reflection-хелпер `GetNamedLinears`.

Документация: `C:/Projects/llm_mvp_core/docs/projects/10-lora.md` (подробная, честная: явно пишет "не ILearningMachine", "не доказательство качества").

Реальные потребители в коде (не только доки) — найдены через Grep:
- `C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:308` — класс `LogicalDpoTrainingUnit : ILogicalUnit<...>`; строка `:339` вызывает `LoraApplicator.Apply(request.Input.PolicyModel, request.Input.LoraConfig ?? new LoraConfig())` ПЕРЕД DPO-шагами (`DpoTrainer.StepVector`). Ingredient-список включает `"10.LoRA:LoraApplicator"`.
- `C:/Projects/llm_mvp_core/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs:858` — фабрика `LoraConfig{Rank=2,Alpha=4,...}`.
- `C:/Projects/llm_mvp_core/18.CapabilityTraining/TeachingRegistryPersistence.cs:303` — `LoraApplicator.Apply(...)`.
- `C:/Projects/llm_mvp_core/16.AutonomousLearning/Training/AutonomousTrainer.cs:8,17,41` — комментарии: Step 7 IncrementalTrainer → LoRA rank=8/alpha=16; реальные LoRA-градиенты делаются НЕ здесь (делегируются `IIncrementalTrainer`).
- `C:/Projects/llm_mvp_core/03.TransformerModel/TransformerBlock.cs:19` — поля attention открыты "для LoraApplicator".
- `C:/Projects/llm_mvp_core/00.TensorCore/Init/Initializers.cs:24,45` — Kaiming для A, Zeros для B "используется в LoRA".
- Тесты-потребители: `Tests/LoraTests.cs`, `Tests/CapabilityResultTranslatorAndDeeperModelTests.cs:126`, `Tests/IntegrationTests2.cs:172`, `Tests/AutonomousLearningTests.cs`.

## Как устроено/работает (кратко)

LoRA здесь — это классический parameter-efficient fine-tuning механизм поверх собственного TensorCore (Rule 10: без внешних ML-фреймворков).
- `LoraLinear.Forward`: `output = base(x) + scaling * ((dropout(x) @ A^T) @ B^T)`. A — Kaiming `[Rank, inDim]`, B — нули `[outDim, Rank]`, поэтому начальная поправка = 0. Base `Weight`/`Bias` замораживаются (`RequiresGrad=false`), `lora_a`/`lora_b` регистрируются как trainable.
- `LoraApplicator.Apply`: (1) замораживает ВСЕ `model.Parameters()`; (2) по `model.Blocks` берёт `block.Attention`, через reflection находит поля типа `Linear`, нормализует имена (убирает `_`, lowercase), сопоставляет с `TargetModules` как substring; (3) создаёт `LoraLinear` для совпавших и возвращает `IReadOnlyList<LoraLinear>`.
- ВАЖНОЕ ограничение (подтверждено кодом и доком 10-lora.md §54-63, §242, §314): `Apply` НЕ делает `field.SetValue`, НЕ встраивает адаптеры в `LlmModel.Forward`, НЕ сохраняет их в модель. Адаптеры возвращаются "сбоку"; их обязан подцепить вызывающий training-loop. Нет собственного checkpoint/export/import LoRA-ассета, нет inference-ветки dropout (всегда `training:true`), нет валидации `Rank<=0`.

## Релевантность к ILearningDynamicGraph (DPO/обучение/граф/оркестрация/измерение/teacher-provenance)

- DPO/обучение: ВЫСОКАЯ как механизм. LoRA — это adapter-tier, который реально подключается в DPO-путь обучения юнита (`LogicalDpoTrainingUnit.ExecuteAsync` → `LoraApplicator.Apply` → `DpoTrainer.StepVector`). Для авто-построения DPO под ILearningUnit/Flow/Orchestrator/DynamicGraph LoRA — кандидат на "tier дешёвого обновления весов" базовой transformer-модели, а не на узел графа.
- Граф/оркестрация: НИЗКАЯ напрямую. LoRA не маршрутизирует, не имеет edges/fan-in/fan-out, не принимает решений. В терминах Graph-Style Reconstruction Gate скила это не узел и не ребро графа, а ресурс/инструмент внутри обучающего узла.
- Измерение: ЧАСТИЧНАЯ. Сам `LoraLinear`/`Applicator` метрик не даёт; числовые метрики (loss/accuracy/time) формирует потребитель — `LogicalDpoTrainingResult` (поля `LastLoss`, `LastAccuracy`, `TrainingTimeMs`, `MetricFilePaths` из `dpo_step_*.json`). То есть LoRA вносит `LoraAdapterCount`, а не качество.
- Teacher-provenance / Legal Teacher Gate: НЕЙТРАЛЬНО/НЕ ПОКРЫТО. LoRA не задаёт источник teacher-данных и не валидирует лицензию учителя. Legal-teacher/provenance поля (teacher_access_mode, model_license, closed_proprietary_model) должны жить на уровне сборки DPO-пар (источники), а не в LoRA. Пробел для плана: LoRA сам не предотвращает запрещённый closed-proprietary teacher.

## Классификация (по правилам скилов)

Файлы — ПРОДУКЦИОННЫЙ КОД, поэтому классифицирую механизм:

- Уровень в иерархии (ILearningMachine/Orchestrator/Flow/Unit/Pair): НИ ОДИН. `LoraConfig`/`LoraLinear`/`LoraApplicator` не реализуют ни один из пяти разрешённых trainable-уровней.
- Классификация: ВНУТРЕННИЙ ДЕТЕРМИНИСТИЧЕСКИЙ/ИНФРАСТРУКТУРНЫЙ ХЕЛПЕР (math/training-infrastructure layer, "model component" tier), который ДОЛЖЕН быть обёрнут разрешённым уровнем.
- Кем владеет (enclosing allowed level): `ILogicalUnit` — а именно `18.CapabilityTraining/LlmIngredientLogicalUnits.cs:308` `LogicalDpoTrainingUnit : ILogicalUnit<LogicalDpoTrainingRequest, LogicalDpoTrainingResult>`, который вызывает `LoraApplicator.Apply` (`:339`) внутри DPO-обучения. Это легитимная "implementation detail inside an allowed level".
- Почему так (ссылка на правило): trainable-architecture-decision-SKILL.md строки 95-118 и 141 — список разрешённых уровней + "Everything else is an implementation detail unless wrapped by one of those levels"; KNN/retrieval/scoring-нота (109-117) по аналогии: компонент модели допустим внутри уровня, нарушение — выдавать standalone-компонент за всю trainable-архитектуру. generated-code-review-SKILL.md Layer 5A (158-220): scorers/adapters и т.п. — не паттерны архитектуры, проходят ревью только как internal detail внутри разрешённого уровня с честной формулировкой.
- Verdict (по словарю скила): `OK_INTERNAL_DETAIL` / `OK_MODEL_COMPONENT` — детерминированная training-infrastructure, честно помеченная (док 10-lora.md прямо запрещает выдавать её за ILearningMachine/качество/обученность). НЕ `FORBIDDEN_RUNTIME_SHORTCUT`, т.к. это не runtime-authority смыслового решения и не замаскированный lookup.
- deterministic-vs-learned boundary: A/B матрицы — learned (trainable params); reflection-сопоставление имён, заморозка, формула, scaling — детерминированы. lora_a/lora_b обучаются ВНЕШНИМ optimizer/DPO, не самим проектом 10.LoRA.

## Пробелы / что доисследовать

1. Wiring-пробел (подтверждён): `Apply` не встраивает адаптеры в `LlmModel.Forward` и не делает `SetValue`. Нужно проверить в плане, КТО реально применяет градиенты к `lora_a/lora_b` (delegate `IIncrementalTrainer` в `16.AutonomousLearning` — не прочитан построчно).
2. LoRA-asset lifecycle: нет собственного checkpoint/export/import/locator/manifest для LoRA-адаптеров. `AutonomousTrainer.cs:43` упоминает "saved LoRA adapter file", но путь/формат не подтверждён чтением — доисследовать `16.AutonomousLearning/Training/AutonomousTrainer.cs` и `IIncrementalTrainer`.
3. Числовые метрики обучения именно LoRA-tier (а не общего DPO) — не выделены; для ILearningDynamicGraph нужно решить, как граф измеряет вклад adapter-tier (ablation: с/без LoRA).
4. Teacher-provenance/legal-gate для DPO-данных, проходящих через LoRA-путь, живёт вне 10.LoRA — проверить `11.DPO`/источники DPO-пар (`23.SemanticCompilerAdapter:TrainingVectorBuilder`, `LogicalDpoTrainingUnit` входные `Preferences`).
5. Не прочитаны построчно: `DpoTrainer.StepVector` (11.DPO) и `DefaultLogicalCapabilityTrainer.ConvergenceMonitor` — релевантны для того, как граф обучающих юнитов реально сходится; вынесено как доисследование (защита от перегруза, лимит файлов соблюдён).
