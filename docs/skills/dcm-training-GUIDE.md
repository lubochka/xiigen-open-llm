# Гайд по обучающему раннеру DesignConversionDomain (DCM)

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.

**Версия:** 1.0
**Статус:** рабочий гайд (companion к `dcm-dpo-construction-GUIDE.md`)
**Язык:** русский — основной; код и идентификаторы на английском
**Аудитория:** архитектор, исполнитель, ревьюер в рамках llm_mvp_core

Данный гайд описывает **runner-протокол, жизненный цикл чекпоинта, метрики и продвижение весов**
для DesignConversionDomain. Вопросы формирования DPO-пар, таксономии дефектов, Render-QA и
AI-судьи описаны в `dcm-dpo-construction-GUIDE.md`.

---

## §0 Как используется этот skill

Используй этот гайд, когда нужно спроектировать или проверить обучающий раннер DCM: порядок `LoadCheckpointAsync`,
`TrainBatchAsync`, `SaveCheckpointAsync`, `ExportAsync`, backup, fresh-load и continue-training.
Он не описывает реализацию юнитов и не разрешает product-code phase; он задаёт runner contract для архитектурной
проверки, work order и reviewer packet.

Перед любым claim вроде "trained", "готово к продвижению", "улучшает HTML+CSS" или "заменяет Bootstrap" runner report
должен закрыть source/license gate из DPO-гайда, метрики из measurement-гайда и lifecycle ниже. Если evidence нет,
правильный статус — `OPEN`, `PROMOTION_BLOCKED` или честный scaffold, а не обученная функция.

---

## §1 Назначение и контекст

### Что делает этот гайд

`dcm-dpo-construction-GUIDE.md` отвечает на вопрос «**как составить обучающую пару**» —
структуру `DesignConversionTrainReadyPair`, правила chosen/rejected, карантин, лицензию.

Этот гайд отвечает на вопрос «**как обучить и сохранить модель**»:

- в какой последовательности вызывать методы машины;
- чем отличается `SaveCheckpointAsync` от `ExportAsync`;
- как общее состояние связывает обучение и инференс;
- какие метрики фиксировать после каждого батча;
- как понять, что модель готова к продвижению в репозиторий.

### Связь с DPO-гайдом

Секция 9 `dcm-dpo-construction-GUIDE.md` явно указывает:

> «Детали runner-протокола, метрик и lifecycle (load → fallback → export/import →
> continue-training) описаны в отдельном документе обучающего раннера домена.»

Данный документ является именно этим компаньоном.

---

## §2 Архитектура обучаемого домена

### ILearningMachinePair — две ноги

DesignConversionDomain реализует интерфейс:

```
ILearningMachinePair<DesignConversionPersonal, DesignConversionGlobal>
```

Это означает две независимые ноги:

| Нога | Тип | Где живёт | Чему учится |
|------|-----|-----------|-------------|
| **Personal** | `ILearningMachine<DesignConversionPersonal>` | у пользователя, на его устройстве | предпочтения конкретного пользователя, его проекты |
| **Global** | `ILearningMachine<DesignConversionGlobal>` | `25.ModelServer` + endpoint в `15.OrchestratorModel` | общие паттерны конверсии дизайна из источников с разрешённой лицензией/consent |

Обе ноги — полноценные `ILearningMachine`, каждая с собственным чекпоинтом, своими
DPO-парами и своим `SaveCheckpointAsync`.

### Правило A-LM-07 для DCM

Глобальная нога публикует чекпоинт через `IModelRepository.UploadAsync` на `25.ModelServer`.
Endpoint регистрируется в `15.OrchestratorModel`. Персональная нога хранится на стороне
пользователя и **не публикуется** в общий репозиторий.

### Правило A-LM-08 для DCM

Синхронизация от Personal к Global допустима только через **анонимизированные сигналы**.
Конкретные дизайн-файлы, URL, имена проектов или любые идентификаторы пользователя
не попадают в глобальную обучающую выборку без явного согласия.

### Юниты внутри каждой ноги

Каждая нога содержит два обучаемых юнита:

```
CriticUnit   : ILogicalUnit<CriticInput, QualityVerdict>
ImproverUnit : ILogicalUnit<ImproverInput, ImprovedHtml>
```

`Train*` методы существуют **только** на `ILearningMachine` (правило A-LM-02).
Юниты (`ILogicalUnit`) — чистые инференс-контракты; добавлять `Train*` в них запрещено.

Допустимые trainable уровни для DCM-описаний: `ILearningMachinePair`, `ILearningMachine`, `ILogicalOrchestrator`,
`ILogicalFlow`, `ILogicalUnit`. Но ownership сохранения, загрузки, promotion и вызова `Train*` остаётся на Machine/Pair.
Regex/BM25 lookup, static validator, deterministic render helper или ad-hoc service не могут называться обученной единицей.
Они допустимы только как bootstrap/scaffold evidence и должны проходить anti-shortcut проверку в measurement-гайде.

---

## §3 Общее состояние — единственный мост

### Почему это важно

Между `TrainBatchAsync` (обучение) и `GetOrchestratorAsync` / инференсом нет прямого
вызова. Единственный механизм передачи знаний — **общее состояние (shared state)**,
которое читается юнитами во время инференса и обновляется во время обучения.

Аналог в TranslationDomain (комментарий в `TranslationTrainedUnits.cs`, строка 253):

> «Share the KNN store — this is the sole bridge between TrainBatchAsync and inference.»

Аналог в `TranslationUnitContracts.cs`, строка 177:

> «Shared state is the only mechanism that connects TrainBatchAsync with GetOrchestratorAsync.»

### Для DCM это означает

После каждого `TrainBatchAsync` обновлённые веса/паттерны CriticUnit и ImproverUnit
живут в общем состоянии машины. Когда потом вызывается `GetOrchestratorAsync`, оркестратор
получает юниты, которые читают именно это обновлённое состояние.

Если вызвать `GetOrchestratorAsync` **до** `TrainBatchAsync` (или до загрузки чекпоинта),
юниты работают с нулевым/инициальным состоянием — это не ошибка, но это Bootstrap-режим,
а не обученный режим.

### Практическое правило

> Никогда не передавать юниты «снаружи» объекта машины как независимые ссылки.
> Инференс-оркестратор должен получать юниты через `GetOrchestratorAsync` той же
> машины, в которой уже выполнен `TrainBatchAsync` или `LoadCheckpointAsync`.

---

## §4 SaveCheckpointAsync vs ExportAsync

Это наиболее распространённая точка путаницы. Два метода делают разные вещи.

### `SaveCheckpointAsync` — реальное сохранение состояния на диск

```
await machine.SaveCheckpointAsync(absoluteCheckpointPath, loopState, ct);
```

- Сериализует **полное обученное состояние** (веса CriticUnit, ImproverUnit,
  накопленные паттерны, метрики) в JSON-файл на диске.
- Это единственный способ сохранить модель между сессиями раннера.
- Без этого вызова обучение существует только в памяти до завершения процесса.
- Аналог в TranslationDomain: `TranslationMachine.cs`, строки 384–388.

Комментарий из аналогичного места в TranslationMachine.cs (строка 252):

> «This method does NOT write to disk. Call SaveCheckpointAsync to persist.»

(Это относится к промежуточному обновлению состояния — сохранение на диск происходит
именно в `SaveCheckpointAsync`.)

### `ExportAsync` — метаданные для манифеста, не сохранение

```
var export = await machine.ExportAsync(ct);
// export содержит: CheckpointRef + ManifestRef
```

- Возвращает **только метаданные**: ссылку на чекпоинт и манифест.
- **Не** сериализует обученное состояние.
- **Не** записывает файлы на диск.
- Используется после `SaveCheckpointAsync` — чтобы получить ссылку на уже сохранённый
  чекпоинт и передать её в `IModelRepository.UploadAsync`.
- Аналог: `TranslationBatch029Runner.cs`, строка 224.

### `LoadCheckpointAsync` (статический) — полное восстановление

```
var result = await DesignConversionMachine.LoadCheckpointAsync(absoluteCheckpointPath, ct);
```

- Читает JSON-файл чекпоинта с диска.
- Возвращает полностью восстановленное состояние машины.
- Это путь **continue-training**: загрузка → обучение → сохранение → продолжение.
- Аналог: `TranslationMachine.cs`, строки 444–447.

### `ImportAsync` — НЕ полная загрузка модели

```
await machine.ImportAsync(importData, ct);
```

- Восстанавливает только **скалярные метаданные** (версия, accuracy).
- Не загружает обученные веса юнитов.
- Не является путём для continue-training.
- Используется для синхронизации метаданных между нодами/сервисами.

### Таблица различий

| Метод | Записывает на диск | Восстанавливает веса | Назначение |
|-------|--------------------|----------------------|------------|
| `SaveCheckpointAsync` | ДА — JSON с весами | — | Реальное сохранение модели |
| `ExportAsync` | НЕТ | — | Получить метаданные для манифеста |
| `LoadCheckpointAsync` | — | ДА — полное | Continue-training / свежая загрузка |
| `ImportAsync` | НЕТ | НЕТ — только скаляры | Синхронизация метаданных |

---

## §5 Runner-протокол — последовательность вызовов

### Полная последовательность одного обучающего цикла

```
1. Загрузить чекпоинт (или создать новую машину для cold-start)
   result = await DesignConversionMachine.LoadCheckpointAsync(path, ct)

2. Для каждого батча (по 50 обучающих пар):
   a. Подготовить LearningBatch<DesignConversionDomain>
      - Examples: 50 обучающих строк (HeldOut=false)
      - HeldOutExamples: 50 held-out строк (HeldOut=true) для оценки

   b. Проверить source/license gates для каждой accepted DPO-пары
      - Personal leg: приватный source остаётся локальным, Global не получает его без consent
      - Global leg: source license/consent разрешает обучение и производные сигналы
      - failed/not_run gate => пара не попадает в TrainBatchAsync

   c. Вызвать TrainBatchAsync
      batchResult = await machine.TrainBatchAsync(batch, ct)

   d. Записать метрики батча (см. §6 и `dcm-measurement-GUIDE.md`)

   e. Проверить дедупликацию (по ExampleId+UnitId, НЕ по BatchId)

3. Перед перезаписью существующего checkpoint — создать backup или записать waiver
   backupRef = checkpointBackup.Create(...)

4. После всех батчей — сохранить состояние на диск
   await machine.SaveCheckpointAsync(absoluteCheckpointPath, loopState, ct)

5. Выполнить fresh-load verification
   reloaded = await DesignConversionMachine.LoadCheckpointAsync(absoluteCheckpointPath, ct)
   assert metrics delta <= allowed tolerance

6. Получить метаданные для манифеста
   var export = await machine.ExportAsync(ct)

7. Проверить, что ImportAsync используется только для metadata sync и не заменяет LoadCheckpointAsync

8. Если проходит ворота A-LM-03 (см. §7) — продвинуть в репозиторий
   await modelRepository.UploadAsync(export.CheckpointRef, export.ManifestRef, ct)
```

### Правило A-LM-04 — структура батча

Каждый батч содержит ровно **50 обучающих строк** и **50 held-out строк**.

- Обучающие строки (`HeldOut=false`): используются для обновления весов.
- Held-out строки (`HeldOut=true`): используются для оценки `AccuracyAfter`
  без влияния на веса.

Held-out выборка должна быть **фиксированной** (не меняться от батча к батчу),
чтобы метрика была сопоставима между батчами.

### Правило дедупликации

Пары не должны обучаться повторно на основе составного ключа `ExampleId+UnitId`.
BatchId не является ключом дедупликации — одна и та же пара может встретиться
в разных батчах с разным BatchId, но должна быть пропущена при повторном обучении.

Аналог: `TranslationMachine.cs` — `_structuredTrainingIdentities` отслеживает
`ExampleId+UnitId` composite key.

### Важно: `TrainBatchAsync` пропускает held-out строки

Метод обучает только строки с `HeldOut=false`. Строки с `HeldOut=true` используются
только для оценки (вычисления `AccuracyAfter`), но не обновляют веса.

---

## §6 Метрики батча

После каждого `TrainBatchAsync` раннер должен зафиксировать следующие поля
в результате батча (`LearningBatchResult`):

| Поле | Тип | Описание |
|------|-----|----------|
| `BatchId` | string/GUID | Идентификатор батча |
| `RowsTrained` | int | Количество обученных строк (HeldOut=false) |
| `AccuracyBefore` | double | Точность на held-out ДО обучения этого батча |
| `AccuracyAfter` | double | Точность на held-out ПОСЛЕ обучения этого батча |
| `RewardMargin` | double | Средний зазор между chosen и rejected в батче |
| `CpuMs` | long | Затраченное CPU-время в миллисекундах |
| `PeakRamMb` | double | Пиковое потребление RAM в мегабайтах |
| `ElapsedMs` | long | Реальное прошедшее время в миллисекундах |
| `PromotedUnitIds` | string[] | Идентификаторы юнитов, прошедших ворота продвижения |

### Как интерпретировать метрики

**`AccuracyAfter` > `AccuracyBefore`** — обучение принесло пользу. Если наоборот —
проверить качество DPO-пар (возможен карантинный материал).

**`RewardMargin`** — средний зазор между `AccuracyChosen` и `AccuracyRejected` по
всем парам в батче. Если зазор меньше 0.05 — пары слишком похожи; проверить
правила минимального зазора в `dcm-dpo-construction-GUIDE.md`.

**`PromotedUnitIds`** — непустой список означает, что один или несколько юнитов
прошли ворота A-LM-03 и готовы к продвижению через `IModelRepository.UploadAsync`.

### Репорт раннера — обязательные поля

После завершения обучающего цикла раннер должен сохранить репорт со всеми
перечисленными полями. Отчёт с `UNKNOWN` в `CpuMs`, `PeakRamMb` или `AccuracyAfter`
не считается финальным — это промежуточные данные.

---

## §7 Правило A-LM-03 — Bootstrap→Trained продвижение

### Суть правила

Bootstrap-юниты (`BootstrapIntentBuilderUnit`, `BootstrapProjectScannerUnit` и другие
из `42.ProjectWikiDomain`) являются **стартовой точкой** для DPO-обучения.
Обученный юнит может заменить Bootstrap-умолчание только после того, как **раннер
публикует валидированные веса**.

Для DCM это означает: CriticUnit и ImproverUnit начинают в Bootstrap-режиме
(детерминированные правила) и переходят в обученный режим только после успешного
прохождения ворот.

Нельзя подменять promotion детерминированным shortcut. Если юнит отвечает за счёт regex, словаря, BM25, static validation,
flow-index или другого lookup без обученного checkpoint и QA/eval evidence, это Bootstrap/scaffold, а не trained feature.

### Ворота продвижения для CriticUnit

CriticUnit готов к продвижению когда:

- `AccuracyAfter` на held-out выборке ≥ порогу тарифа (см. §8);
- оценки вердиктов юнита **достоверно соответствуют реальному набору дефектов**
  (correctness of critique), а не просто имеют высокий accuracy-номер внутри вердикта;
- прошёл ablation-тест: результат юнита при перемешанном порядке кандидатов
  не меняется (нет утечки позиции).
- source/license gate закрыт для всех accepted DPO-пар;
- QA/eval evidence приложена до semantic/product claim.

### Ворота продвижения для ImproverUnit

ImproverUnit готов к продвижению когда:

- Render-QA на held-out дизайнах показывает accuracy ≥ порогу тарифа;
- accuracy выбранного (chosen) результата ≥ accuracy отклонённого (rejected)
  с зазором ≥ 0.05;
- тест на 4 разрешениях (360px, 768px, 1280px, 1920px) пройден.
- source/license gate закрыт для всех accepted DPO-пар;
- QA/eval evidence приложена до semantic/product claim.

### Холодный старт (cold-start) — только именованный путь

Обучение с нуля (без загрузки чекпоинта) допустимо только как явно обозначенный
путь `cold-start`. Раннер должен логировать это как `mode=cold_start`. Раннер,
начинающий с нуля без явной пометки, считается дефектным.

---

## §8 Полный жизненный цикл

### Приоритет загрузки состояния

При старте раннер проверяет источники состояния в следующем порядке:

```
1. Пользовательский кэш / локальный чекпоинт пользователя
   → если найден и валиден: загрузить через LoadCheckpointAsync

2. Репозиторий / общий чекпоинт (Personal нога: пользовательский,
   Global нога: с 25.ModelServer через IModelRepository)
   → если найден и валиден: загрузить через LoadCheckpointAsync

3. Cold-start: создать новую машину (явно логировать mode=cold_start)
```

Инвертировать порядок (загружать сначала общий, потом пользовательский) — ошибка.
Пользовательские предпочтения имеют приоритет над общим репозиторием.

### Полный lifecycle одной ноги (например, Personal)

```
[Старт раннера]
   ↓
[Загрузка] LoadCheckpointAsync(userCachePath)
   → если нет файла → LoadCheckpointAsync(repoPath)
   → если нет файла → cold-start (логировать)
   ↓
[Обучение] TrainBatchAsync × N батчей (каждый по 50 строк)
   → source/license gates проверены до обучения
   → метрики фиксируются после каждого батча
   → дедупликация по ExampleId+UnitId
   ↓
[Backup] backup существующего checkpoint перед перезаписью
   → backupRef зафиксирован или waiver обоснован
   ↓
[Сохранение] SaveCheckpointAsync(userCachePath)
   → полное состояние на диск
   ↓
[Fresh-load] LoadCheckpointAsync(userCachePath)
   → метрики воспроизведены в пределах допуска
   ↓
[Метаданные] ExportAsync()
   → CheckpointRef + ManifestRef (НЕ сохранение состояния)
   ↓
[Продвижение] если ворота A-LM-03 пройдены:
   IModelRepository.UploadAsync(CheckpointRef, ManifestRef)
   ↓
[Инференс] GetOrchestratorAsync()
   → юниты читают обновлённое общее состояние
   ↓
[Continue-training] следующий цикл начинается с LoadCheckpointAsync
   из только что сохранённого чекпоинта
```

### Export/Import/Share

- **Export**: `ExportAsync()` → метаданные. Реальный файл чекпоинта уже на диске
  после `SaveCheckpointAsync`. Метаданные передаются для `UploadAsync`.
- **Import**: `ImportAsync(importData)` → только скаляры (версия, accuracy).
  Не заменяет `LoadCheckpointAsync` при continue-training.
- **Share**: отправка CheckpointRef другому инстансу → тот вызывает `LoadCheckpointAsync`
  по полученному пути.
- **Backup**: копия JSON-файла чекпоинта. Раннер может периодически копировать
  чекпоинт в `backup/` директорию перед каждым новым обучающим циклом.

---

## §9 Пороговые значения тарифов (PROVISIONAL)

> **ВНИМАНИЕ:** Значения ниже являются **ПРЕДВАРИТЕЛЬНЫМИ** и требуют явного решения Любы.
> До подтверждения эти пороги не являются обязательными Gate-значениями раннера.

| Тариф | Render-QA порог | AI-судья | Примечание |
|-------|----------------|----------|------------|
| Free | 0.75 | Только Render-QA, без AI-судьи | PROVISIONAL |
| Paid | 0.85 | До 50 вызовов AI-судьи в месяц | PROVISIONAL |
| Pro | 0.90 | Без ограничений | PROVISIONAL |

Данные пороги согласуются с таблицей из `dcm-dpo-construction-GUIDE.md` §5.

До явного решения Любы раннер может использовать фиксированный порог **0.80** как
рабочее временное значение для всех тарифов во время разработки.

---

## §10 Применимые архитектурные правила

| Правило | Суть для DCM |
|---------|-------------|
| **A-LM-02** | `Train*` только на `ILearningMachine`. CriticUnit и ImproverUnit — чистые инференс-контракты, без `Train*`. |
| **A-LM-03** | Обученный юнит заменяет Bootstrap только после публикации валидированных весов через раннер. |
| **A-LM-04** | Батч = 50 обучающих + 50 held-out строк. Оба числа строгие. |
| **A-LM-07** | Глобальная нога живёт на `25.ModelServer`, endpoint в `15.OrchestratorModel`. |
| **A-LM-08** | Синхронизация Personal → Global только через анонимизированные сигналы. |

### Что НЕ делает этот гайд

- Не описывает структуру DPO-пар — это `dcm-dpo-construction-GUIDE.md`.
- Не описывает Render-QA или AI-судью — это тоже `dcm-dpo-construction-GUIDE.md`.
- Не задаёт финальные пороговые значения тарифов — PROVISIONAL, требуют решения Любы.
- Не описывает реализацию CriticUnit или ImproverUnit — это предмет отдельного плана.

---

## Глоссарий

| Термин | Расшифровка |
|--------|-------------|
| DPO | Direct Preference Optimization — обучение на парах chosen/rejected |
| DCM | DesignConversionDomain — домен конверсии дизайна |
| Render-QA | Quality Assurance рендеринга — пиксельный diff на 4 разрешениях |
| A-LM-xx | Locked decision для ILearningMachine-иерархии (из `DECISIONS-LOCKED.md`) |
| Bootstrap | Стартовые детерминированные реализации юнитов до обучения |
| Cold-start | Старт обучения с нулевого состояния (без загрузки чекпоинта) |
| CheckpointRef | Ссылка на файл чекпоинта (путь или URI) |
| ManifestRef | Ссылка на манифест модели (версия, метаданные, hash) |
| Held-out | Строки батча, не используемые для обновления весов — только для оценки |

---

*Companion: `docs/skills/dcm-dpo-construction-GUIDE.md`*
*Архитектурные решения: `docs/DECISIONS-LOCKED.md` §A-LM-02..A-LM-08*
