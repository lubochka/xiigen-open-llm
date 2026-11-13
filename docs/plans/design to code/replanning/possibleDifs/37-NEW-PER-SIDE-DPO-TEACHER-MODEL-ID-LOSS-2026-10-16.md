# N37-03 — Новый конфликт контракта: admission DPO знает класс автора, но не identity chosen.model/rejected.model

**16.10.2026 · SOURCE-CONFIRMED DATA-CONTRACT LIMITATION; CONTRACT APPLICABILITY OPEN; TRAINING QUALITY IMPACT HYPOTHESIS; NATIVE_NOT_RUN.** Core источник: `codex/dpo-implementation@793d7b755b23aac90445da3ac6661b1bff7e6672`. Не изменять `llm_mvp_core/docs/plans/replanning-possible-diffs`.

## 1. Короткий вывод

В предоставленном проектном документе `WHAT-TO-DO-WHAT-NOT-TO-DO-XIIGEN-v2.md`, раздел `DPO And Learning`, есть явное требование: **`chosen.model must differ from rejected.model`**. В действительном тренерском admission-contract доступны только **типы авторов** `OpenTeacher/LocalModel/DeterministicGenerator/...`, общий `IssuerName` и fingerprint текста. Там нет конкретной identity/версии **модели для каждой стороны** пары. Поэтому этот именно контракт не способен проверить «chosen.model != rejected.model», даже если upstream знает эти два имени.

Одновременно [RP01 роль учителя](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/LEARNING-EXAMINATION.md) и [N35 synthesis](35-MASTER-RP01-DIVERGENCE-HISTORICAL-SUCCESSES-AND-CAUSAL-ORDER-2026-10-16.md) формулируют обучение через **допущенную внешнюю открытую китайскую модель** как автора окончательных chosen И rejected. Прямого доказательства, что это буквально один и тот же `model-id`, а не два разных допущенных идентификатора, здесь нет. Нужно решить **область применения политики** и прочитать actual provenance каждого будущего pair, а не автоматически делать вывод о нарушении.

Это отличается от давно известного `simulation` vs `external teacher`: речь не о том, что оба автора исторического RP01 помечены `DeterministicGenerator`, а о том, какие **два model ID способны пережить admission boundary**, если в будущем внешние пары будут настоящими.

## 2. Исходники и точный механизм потери информации

- [`DpoPairSideAuthorKind` L13–33](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/11.DPO/Models/DpoTrainerPairAdmission.cs#L13-L33): конечный enum `User, LocalModel, OpenTeacher, DeterministicGenerator, PaidExternalModel`. Два разных `OpenTeacher` с разными версиями и один и тот же `OpenTeacher` здесь неразличимы.
- [`DpoTrainerPairAdmission` L42–107](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/11.DPO/Models/DpoTrainerPairAdmission.cs#L42-L107): конструктор принимает `chosenAuthor, rejectedAuthor, question, chosen, rejected, issuerName`; сохраняет `ChosenAuthor`, `RejectedAuthor`, `HasPaidExternalModelSide`, `ContentFingerprintSha256` и `IssuerName`. У модели chosen/rejected отдельной identity нет.
- [`CreateAdmitted` L213–241](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/11.DPO/Models/DpoTrainerPairAdmission.cs#L213-L241): публичный API принимает только эти поля; проверка `chosenModelId != rejectedModelId` невозможна внутри этого вызова.
- [`DpoPair` L7–13](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/11.DPO/Models/DpoPair.cs#L7-L13): сохраняет только `Prompt/Chosen/Rejected`. [`DpoDatasetRow` L8–31](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/11.DPO/Models/DpoDatasetRow.cs#L8-L31) переносит тот же pair/admission, не добавляя per-side model ID.
- [RP01 `CodeDocumentationSimulationDpoExperiment` L65–70](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L65-L70): историческая simulation честно помечает оба края `DeterministicGenerator`. Здесь действительно **не заявлено** внешнее происхождение; не превращать этот факт в ложное доказательство провала реальных teacher models.

**Что доказано:** в указанном admission schema отсутствует информация для проверки точного неравенства двух model-id. **Что не доказано:** нет внешнего/предыдущего валидатора, не существует сохранённого sidecar manifest, фактические teachers совпали, различие teacher models математически необходимо для DPO, или именно это обрушило RP01.

## 3. Почему это может повлиять на качество и где нет

Само использование одного и того же model ID с разными промптами **не делает DPO математически неправильным**. Тем не менее отсутствие identity может скрыть слабые/тривиальные rejected, взаимозависимость ошибок, не тот зафиксированный teacher version, а также ретроспективно запрещать выяснить, какой teacher произвёл полезное изменение. Для `DPO chosen/rejected` это потенциально важный источник **интерпретируемости причин улучшения**, а не автоматическая гарантия качества от двух моделей.

**P1 контракт/происхождение пары** только если проектное правило об отдельных model ID действительно применяется к данному recipient и upstream не сохраняет/не валидирует его. **Не переносить** на `KnnLogicalUnit` (там обучающие метки уже приняты) и `Translation ExecutorUnit` только потому, что те используют положительные/отрицательные истории.

## 4. Малый опровержимый опыт N37-TEACHER-ID-01

**Сначала read-only lineage, ноль вызовов учителя:** выбрать 8 существующих пар из C# и 8 из EN→RU (если есть реальные пары, не симуляции), проследить `raw source manifest → legal teacher producer → DpoTrainerPairCustoms → DpoDatasetRow → sealed saved dataset → trainer`. Для каждой стороны выписать `model id, version, provider, author kind, licence/terms decision, response SHA` и точный путь где identity теряется или сохраняется. Разделить `NOT_PRESENT_IN_ADMISSION` и `NOT_PRESENT_ANYWHERE`. Симуляционные 500 пометить `SIMULATION_NOT_ELIGIBLE_FOR_TEACHER_COMPARISON`.

**A/B metadata-only контраст:** создать два **test-only** конверта с абсолютно одинаковыми строками `Prompt/Chosen/Rejected`, одинаковым `IssuerName` и author kinds `OpenTeacher/OpenTeacher`, но в sidecar A `modelId(chosen)=Qwen-model-1, modelId(rejected)=Qwen-model-1`, B `modelId(chosen)=Qwen-model-1, modelId(rejected)=Other-approved-open-teacher`. При `CreateAdmitted` проверить, что current admission bytes/fingerprint не различают A и B. Это фальсификатор наблюдаемости provenance, а **не доказательство потери предметного качества**. Для Policy-boundary B необходимо требовать per-side ID в проверяемом manifest или при admission, но только после архитектурного решения.

**Отдельный quality contrast только при подтверждённой применимости и легальности:** 8 законных source-disjoint C# или EN→RU задач, две разрешённые teacher-production стратегии с одинаковыми темами/бюджетами, independent blind CODEX/Astra judge на сложность `rejected` и полезность расхождения. Затем Tiny8–16 learning-step с одинаковой B0/reference/LoRA, sealed 8/16 domain-law answers, primary = `law_correct_final_count`, secondary = teacher-contrast informativeness/coverage. **Не требовать повторного RP01-500**. Отдельно помечать происхождение chosen/rejected; не предполагать две учительские модели доступны без проверки лицензии/разрешения.

**Фальсификаторы:** валидированный upstream per-side model identity уже существует и сохраняется до durable evidence; правило `chosen.model != rejected.model` ограничено только Lite/Enterprise products, не этим экспериментальным `core` recipient; нормативно разрешён same model but different samples; independent judge не находит разницы при matched pairs; Student quality не меняется.

## 5. Дедуп и проверка планирования

Известное [N03 DPO gradient](03-DPO-PAIR-AND-GRADIENT.md) уже говорит о simulation и законном авторстве; [N35 master](35-MASTER-RP01-DIVERGENCE-HISTORICAL-SUCCESSES-AND-CAUSAL-ORDER-2026-10-16.md) — про роли CODEX/Chinese teacher. **Новая узкая проверка:** в самом входе тренера `author category` не эквивалентен `model identity per side`, и это не доказано прежней матрицей фактов. Нельзя делать вывод «все пары нарушают политику» без флагов/manifest и решения области применения.

**Review acceptance:** видимый source->field loss, две версии конверта A/B, выбор применимости правила, трейс реального сохранённого model-id, falsifier, подтверждение что код/weights и чужой `core/docs/plans/replanning-possible-diffs` не менялись. Статус сегодня — `SOURCE_CONTRACT_GAP / POSSIBLE_POLICY_CONFLICT / QUALITY_NOT_MEASURED`.
