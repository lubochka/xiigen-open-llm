# Opus Pass P05 — Реальный код DPO / обучения / жизненного цикла

Дата: 2026-07-08. Срез: РЕАЛЬНЫЙ код DPO/тренинга/lifecycle в `C:\Projects\llm_mvp_core`
(11.DPO, 42.ProjectWikiDomain training runners/pipelines, checkpoints, IModelRepository,
IRemoteUnitLocator, manifests). Оценка ведётся от кода/артефактов, а не от самоотчёта пакета.
Границы зрелости: COPIED_DOC_ONLY != BUILT_NOT_PROVEN != PROVEN.

## 1. Что реально проверено (bounded reads)

1. `11.DPO/DpoTrainer.cs` (прочитан целиком, 333 строки) — **НАСТОЯЩИЙ** from-scratch DPO
   тренер: policy/reference `LlmModel`, `AdamW`, ручной backward через logits, DPO-loss
   `-log σ(β·margin)`, метрики `dpo_step_NNNNNN.json` через atomic write (`.tmp`→`Move`).
   Это не стаб. Beta=0.1, lr=1e-5 по умолчанию. Есть `Step/StepMany/EvaluateMany/StepVector`.

2. `42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs` (прочитан целиком, 499 строк) —
   **НАСТОЯЩИЙ** тренировочный runner поверх `DpoTrainer`: строит `LlmModel`, гоняет
   epochs×batch, пишет `dpo-pairs.jsonl`, `dpo-pair-diagnostics.jsonl`, `policy_final.bin`,
   `reference_base.bin`, `project-wiki-dpo-metric-summary.json`; меряет CPU/RAM/elapsed;
   валидирует 50 train + 50 held-out и требует `[WRONG:` в rejected. НО:
   - токенайзер **фейковый**: `StableHash(token) % (vocabSize-1)` (`:352`, `:357`) — хеш-
     коллизии превращают семантику в произвольные id;
   - модель игрушечная: ctor defaults `vocabSize=2048, contextLen=192, hiddenDim=64,
     numLayers=1, numHeads=4, ffnHidden=128` (`:39-44`).
   Значит loss/accuracy/reward-margin меряют предсказание хеш-токенов на 1-слойном
   трансформере, а не семантику ответа.

3. `25.ModelServer/Contracts/IModelRepository.cs` + единственная реализация
   `Local/LocalModelRepository.cs` (прочитаны целиком). `UploadAsync` **ничего не
   передаёт**: проверяет наличие `PolicyCheckpointPath`, считает SHA256 и возвращает
   `RemoteUnitManifest` В ПАМЯТИ (`:50-77`). HTTP/HttpListener/WebApplication/MapPost в
   `25.ModelServer/*.cs` — grep 0. Реального model-server-процесса нет.

4. `25.ModelServer/Contracts/IRemoteUnitLocator.cs` + единственная реализация
   `Remote/InMemoryRemoteUnitLocator.cs` (прочитаны целиком). Локатор — `Dictionary<string,
   RemoteUnitManifest>` в памяти процесса (`:8`); `RegisterAsync` кладёт в словарь, который
   теряется при выходе. Второй `: IRemoteUnitLocator` — только тест-дубль в
   `Tests/TrainableRouterOrchestrationTests.cs`. Персистентного реестра/маркетплейса нет.

5. Manifest-паблишеры 42-домена (`CodeTraceBuilderManifestPublisher.cs` и ~9 аналогов) —
   реальные классы, вызывают `_repository.UploadAsync` (`:105`) + `_locator.RegisterAsync`
   (`:109`), и **подключены** к пайплайнам (`*TrainingPipeline.cs` создают паблишер по
   умолчанию, напр. `CodeTraceBuilderTrainingPipeline.cs:37`). Т.е. код publish/register
   собран, но опирается на no-op local + in-memory транспорты из пунктов 3-4.

6. Живые promoted-чекпоинты СУЩЕСТВУЮТ на диске (`git ls-files models/*` = 15 файлов):
   `models/{prompt-to-task,orchestration-reasoning,orchestration,skill-selection,
   translation}/*-v1/{manifest.json, *-checkpoint.json, runner-report.json}` + `models/index.json`.
   Runtime грузит их: `43.SmartSkillsDomain/XiigenExtensionApiService.cs` метод
   `LoadCommonAssets` (`:1389-1469`) читает `models/prompt-to-task/...`,
   `models/orchestration-reasoning/...`, `models/skill-selection/...`.

7. Содержимое promoted-чекпоинтов (что именно «обучено»):
   - `models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json` (18 796 байт):
     `learnedState.learnedPatterns[]` = списки **ключевых слов** (`learnedSignals`: "approval",
     "plan", "planning", "scope_creep" …). Это обученный СЛОВАРЬ сигналов, не нейромодель.
   - `models/prompt-to-task/.../manifest.json`: `semanticQualityClaimed:false`,
     `learnedPatternCount:12`, `dpoPairCount:8`, `heldOutRowCount:4`. Обучено на 8 DPO-парах.
   - `models/skill-selection/.../runner-report.json`: `command:"regenerate-skill-selection-
     model-from-training-data"`, `trained_pair_count:900`, `unit_profile_counts{...}` — это
     профильные счётчики по 11 суб-юнитам (dictionary/counter state), не DPO поверх LlmModel.

8. Связь тренера с пайплайном расширений: `git grep -c "DpoTrainer|LlmModel"` в
   `43.SmartSkillsDomain/*.cs` = **0**. Реальный трансформерный DPO из 11.DPO НЕ используется
   тремя машинами расширений (PromptToTask/OrchestrationReasoning/SkillSelection). Вызыватели
   `DpoTrainer`: 11.DPO, 18.CapabilityTraining, 42.ProjectWikiDpoMetricRunner, experiments-v2.

9. `.xiigen`-writer СУЩЕСТВУЕТ (опровергает P02 «no writer/persister for .xiigen»):
   `artifacts/adaptive-learning/AD-02/AD-04/AD-07/AD-09A/.xiigen/smart-skills/*/`
   `adaptive-checkpoint.json`, `adaptive-training-report.json`, `continue-probe-report.json`,
   `restore-import-report.json`. Контракты в `43.SmartSkillsDomain/{SkillSelection,PromptToTask,
   OrchestrationReasoning}Contracts.cs` + `Phase08CommonUserTwoLegLifecycle.cs`.

## 2. Итог P05

Реальный DPO-движок (11.DPO) — настоящий и с персистентными артефактами (4263 `.bin`,
`dpo_step_*.json`), НО он обслуживает 42.ProjectWiki bootstrap-юниты на игрушечной модели с
хеш-токенайзером, а НЕ три расширения. «Обученные модели» расширений в `models/` — это
обученные словари ключевых слов (prompt-to-task) и профильные счётчики (skill-selection) с
явным `semanticQualityClaimed=false`. Lifecycle-транспорт (upload/locate) собран как код, но
реализован no-op local + in-memory (нет model-server, нет персистентного локатора). Промоушен
candidate→`models/` физически произошёл (файлы есть), но продвинутые чекпоинты не доказывают
семантику; continue-training/adaptive uplift — только bounded AD-* пробы без числового прироста.

## 3. Новые дыры (детально) — см. таблицу в мастер-файле P05

- P05-1 Extension trained-models = словари/счётчики, не трансформерный DPO; `semanticQualityClaimed=false`.
- P05-2 Реальный 11.DPO тренер отсоединён от ответного пути расширений.
- P05-3 ProjectWikiDpoMetricRunner: хеш-токенайзер + игрушечная модель → метрики не про семантику.
- P05-4 IModelRepository.UploadAsync — no-op, нет реального model-server/HTTP.
- P05-5 IRemoteUnitLocator — только in-memory Dictionary, нет персистентного реестра.
- P05-6 `.xiigen` writer существует (P02 «no writer» устарел), но adaptive/continue-training числовой прирост не доказан.
