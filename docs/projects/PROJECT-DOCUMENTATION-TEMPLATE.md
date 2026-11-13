# Project Documentation Template

Статус: общий стандарт D0. Этот файл задаёт обязательный формат для будущих `docs/projects/*.md` и extension `docs/PROJECT-DOC.md`. Он не является документацией конкретного проекта.

## 0. Краткая карта для Лубы

Напиши 3-7 предложений простым русским языком:

- что это за проект или репозиторий;
- какой пользовательский или системный поток через него проходит;
- какие данные входят и выходят;
- что уже работает;
- что является skeleton, bootstrap, partial, legacy или planned.

Нельзя заменять это списком классов. Луба должна понять роль проекта без чтения кода.

## 1. Простая UML для draw.io

Диаграмма обязательна. Используй PlantUML или Mermaid, импортируемый в draw.io. Предпочтительный формат:

```plantuml
@startuml
title <project-id> - <human title>

class "Входной компонент" as InputComponent {
  принимает запрос
}

class "Основная машина" as MainMachine {
  выполняет текущую логику
}

class "Результат" as OutputComponent {
  возвращает ответ
}

InputComponent --> MainMachine : <DTO/input data>
MainMachine --> OutputComponent : <DTO/output data>

note right of InputComponent
СТАТУС: <полная/частичная/bootstrap/skeleton>
Что делает человеческим языком.
end note

note right of MainMachine
СТАТУС: <полная/частичная/bootstrap/skeleton>
Какая логика реально работает сейчас.
end note

note right of OutputComponent
СТАТУС: <полная/частичная/bootstrap/skeleton>
Что получает следующий слой.
end note
@enduml
```

После диаграммы обязательна легенда:

| Узел | Что делает | Статус | Где живёт |
|---|---|---|---|
| `<ClassOrComponent>` | Человеческое объяснение | `Полная/Частичная/Bootstrap/Skeleton/Legacy/Planned` | `relative/path` |

### 1.1 Симуляции по каждой связи UML

Каждая стрелка диаграммы обязана иметь пример входа, branch/simulation и ожидаемый результат.

| Связь | Пример входа | Симуляция ветки | Ожидаемый результат | Что запрещено принимать за результат |
|---|---|---|---|---|
| `A -> B` | `Prompt="добавь doc"` | `A` строит request и передаёт `B` | `B` получает typed DTO, не raw guess | JSON/report не заменяет реальный вызов |

Правило: если стрелка есть в UML, но нет строки симуляции, документ не принят.

## 2. Алгоритмы без кода, по-русски

Для каждого важного алгоритма заполни отдельный блок.

### 2.N <человеческое название алгоритма>

Аналогия: объясни алгоритм на бытовом примере.

Паспорт текущей реализации:

| Поле | Значение |
|---|---|
| Класс | `<ExactClass>` |
| Метод/функция | `<ExactMethod>(<parameters>)` |
| Файл и строка | `relative/path/File.cs:line` |
| Входные параметры/DTO | `<RequestDto>`: поля и пример значений |
| Выходной DTO/результат | `<ResponseDto>`: поля и пример значений |
| Статус | `Полная/Частичная/Bootstrap/Skeleton/Legacy/Planned` |

Шаги текущего алгоритма:

1. Что приходит на вход, с примером значения.
2. Какие поля или internal state читаются.
3. Какая ветка выбирается и почему.
4. Что создаётся или меняется.
5. Что возвращается наружу.

Ветки и симуляции:

| Ветка | Пример входа | Почему выбирается | Ожидаемый результат | Проверка/QA |
|---|---|---|---|---|
| `<branch>` | `<concrete value>` | `<condition>` | `<output>` | `<test/e2e/manual gap>` |

Что алгоритм НЕ делает:

- не использует regex/BM25/KNN/dictionary как скрытую trainable semantic authority;
- не выдаёт skeleton/bootstrap/JSON/report за реальный код или обученную модель;
- не принимает host-side ranking как финальное semantic решение, если core должен решать сам;
- не заявляет checkpoint/model readiness без manifest, locator, fresh-load и continue-training evidence.

## 3. Где логика сидит в коде

Нужно объяснить, где находится логика и почему именно эти файлы важны.

| Логика | Файл | Почему это место важно | Что доказывает | Чего не доказывает |
|---|---|---|---|---|
| `<algorithm>` | `relative/path/File.cs:line` | `<why>` | `<current behavior>` | `<missing evidence>` |

## 4. Короткие важные цитаты кода

Цитаты должны быть короткими. Перед цитированием файл должен быть прочитан. Каждая цитата обязана иметь точную ссылку и объяснение WHY/HOW/WHAT.

Пример:

> **Почему важно:** эта строка показывает честный skeleton status: машина сохраняет безопасные поля, но не делает semantic inference.
>
> **Что входит:** `PromptToTaskRequest` с hash и constraints.
>
> **Что выходит:** response со статусом `not_ready`.
>
> **Чего не доказывает:** наличие обученной модели или качество inference.

```csharp
// relative/path/File.cs:42
TaskType = "not_ready_no_semantic_task";
```

Обязательные правила:

- ссылка формата `relative/path/File.ext:line` или `relative/path/File.ext:line-line`;
- не ссылаться на будущий код;
- не цитировать длинные блоки;
- если код вызывает другой метод, объяснить, что происходит внутри или дать отдельную цитату;
- если логика skeleton/bootstrap/partial, написать это прямо рядом с цитатой.

## 5. Test, e2e и QA mapping

Каждая UML-связь и каждая ветка алгоритма должна иметь проверку или честный gap.

| Поведение | Уровень проверки | Текущий evidence | Статус | Следующий work order если gap |
|---|---|---|---|---|
| `<branch/connection>` | `unit/integration/e2e/host/QA/negative/untrained/trained/regression` | `<test/report/manual gap>` | `covered/gap/not applicable` | `<DOC-WO-XX or code WO>` |

Если требуется реальный код или алгоритм, нельзя закрывать строку только JSON, PowerShell, smoke report или статическим валидатором.

## 6. Anti-false-done checks

Перед принятием документа проверь:

| Gate | Должно быть видно |
|---|---|
| Human-readable | Русское объяснение понятно без чтения кода первым шагом |
| UML | PlantUML/Mermaid импортируется в draw.io |
| UML simulations | Каждая стрелка имеет пример входа и ожидаемый результат |
| Algorithms | Каждый алгоритм имеет class, method/function, parameters, output и текущие шаги |
| Code refs | Короткие цитаты имеют точные line refs и WHY/HOW/WHAT объяснение |
| Status honesty | Skeleton/bootstrap/partial/legacy названы прямо |
| No JSON-only done | JSON/PowerShell/report/smoke evidence не заменяет реальный код |
| No hidden deterministic shortcut | BM25/KNN/regex/dictionary не спрятаны как trainable unit |
| Trainable claim | Есть checkpoint, manifest, locator, fresh-load, continue-training и metrics, если claim = trained |
| Testability | Каждая ветка связана с unit/integration/e2e/QA/negative/untrained/trained/regression или честным gap |

## 7. Status table

| Компонент | Файл | Статус | Что реально работает сейчас |
|---|---|---|---|
| `<Class>` | `relative/path/File.cs` | `✅ Полная / ⚠️ Частичная / 🔶 Скелет / ✅ Bootstrap / ✅ Hardcoded / Legacy / Planned` | Конкретные методы, return values и gaps |

## 8. Hierarchy, если есть ILearningMachine/ILogical*

Если документ касается `ILearningMachine`, `ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit` или host/plugin path, раздел обязателен.

```text
Уровень 5: ILearningMachine<TDomain>
  Принимает: LearningBatch<TDomain> / LearningEvaluationSet<TDomain>
  Возвращает: LearningBatchResult / LearningEvaluationResult / LearningMachineExport
  Роль: обучает, оценивает, экспортирует и импортирует trainable domain.

Уровень 4: ILogicalOrchestrator
  Принимает: LogicalOrchestratorRequest
  Возвращает: LogicalOrchestratorResult
  Роль: выбирает и запускает flow.

Уровень 3: ILogicalFlow<TInput>
  Принимает: LogicalFlowRequest<TInput>
  Возвращает: LogicalFlowResult
  Роль: ведёт сценарий end-to-end.

Уровень 2: ILogicalUnit<TInput,TOutput>
  Принимает: LogicalUnitRequest<TInput>
  Возвращает: LogicalUnitResult<TOutput>
  Роль: выполняет один шаг цепочки.

Уровень 1: ILearningMachinePair<TPersonal,TGlobal>
  Принимает: personal/global machine legs
  Возвращает: sync/pull OperationResult<bool>
  Роль: разделяет user adaptive и common/global state.
```

Добавь sequence/flow diagram для host path и подпиши тип данных на каждой стрелке.

## 9. Источники и drift check

Перед финальным статусом документа сверить:

- `docs/SUBPROJECT-MAP.md`;
- `docs/ARCHITECTURE.md`;
- `docs/COMPONENT-INTERFACES.md`;
- source files, которые цитируются;
- tests/QA evidence, если делается coverage claim.

Финальная строка документа:

```text
Documentation status: <draft/pass/repair-required>
Last verified against source: <date>
Known gaps: <short list or none>
```
