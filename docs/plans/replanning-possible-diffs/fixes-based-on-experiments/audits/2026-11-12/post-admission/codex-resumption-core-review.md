# Продолжение аудита после обрыва: проверка F03 на 6b52307d

Дата: 12 ноября 2026 (Asia/Jerusalem). Reviewer: Codex, `latest_core_review`.
Проверяемая ветка: `codex/dpo-implementation`.
Исходный пакет: `9dc7f71c775506ec7eb0d5186afec82ddada0444`.
Свежий закреплённый срез: `6b52307dc26f982dde161ee1d6f470eec1c84de4`.

**Вывод: пробел в регрессионном прогоне восьми дополнительных классов закрыт сохранёнными результатами 51/51 и отдельным контролем 3/3. Повторять этот прогон как ещё не выполненный не нужно. F03 целиком остаётся открытым: ранний захват на обычном пути, закрытие владельца, границы записи и связь с внешним запускателем ещё не завершены.**

## Область и метод

Между исходным пакетом и свежим срезом четыре коммита. Из C# изменены только `ExperimentEvidenceSink.cs` и `ExperimentAttemptOwnershipTests.cs`; добавлены попытки 0002–0004 и документы. Прочитаны актуальные исходники существенных ветвей и сохранённые результаты. Применены полностью прочитанные `code-examination-SKILL.md` и `systematic-debugging-SKILL.md`.

В этой проверке **не запускались .NET, PowerShell, обучение модели или тесты продукта**. Восемь опубликованных TRX разобраны XML-парсером Python. Считались только элементы `UnitTestResult`; их исходы сопоставлены с `Counters`, идентичностями тестов и определениями классов. Это повторная проверка сохранённых доказательств, а не новый native прогон и не новый результат обучения.

## 1. Что подтверждено в опубликованных TRX

Все пути этой таблицы относительны к [AUDIT-F03-ATTEMPT-OWNERSHIP на закреплённом срезе](https://github.com/lubochka/llm_mvp_core/tree/6b52307dc26f982dde161ee1d6f470eec1c84de4/artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP).

| Файл результатов | Прочитанный итог | Область |
|---|---|---|
| `attempt-0002/results/run-F-final-green-after-mutations-reverted.trx` | 86 passed, 0 failed | 14 классов; это не весь проект |
| `attempt-0003/results/run-01-green-oracle-fixed.trx` | 10 passed | Ownership tests |
| `attempt-0003/results/run-02-green-with-source-env-set.trx` | 1 passed | Проверка маркера при заданном source |
| `attempt-0003/results/mutation-a-source-field-removed-red.trx` | 9 passed, 1 failed | Единственное падение — `OwnerMarkerBindsTheRunToItsAddressAndCircumstancesInEveryBindingField` |
| `attempt-0003/results/mutation-b-old-oracle-red.trx` | 0 passed, 1 failed | Та же проверка со старым оракулом |
| `attempt-0003/results/run-03-final-green-after-revert.trx` | 14 passed | 10 ownership + 4 sink |
| `attempt-0004/results/run-01-eight-unverified-callers.trx` | 51 passed, 0 failed | Восемь выбранных классов |
| `attempt-0004/results/run-02-supplementary-control-class.trx` | 3 passed, 0 failed | Отдельный rollback-control класс |

Во всех восьми файлах counters совпали с отдельными результатами, повторов testId внутри файла нет. Для основного прогона attempt-0004 дополнительно сравнен точный набор 51 имён с `logs/list-01-eight-classes.log`: отсутствующих, лишних и повторных имён нет. `logs/build-01-release.exitcode`, `logs/run-01-eight-unverified-callers.exitcode` и `logs/run-02-supplementary-control-class.exitcode` содержат 0.

Восемь классов attempt-0004:

| Класс | Результатов | Passed |
|---|---:|---:|
| `ProbeNeuralNetworkGradientTests` | 8 | 8 |
| `TranslationBatchJsonLearningTests` | 3 | 3 |
| `TranslationBatchTypeMismatchRollbackTests` | 3 | 3 |
| `TranslationExactTopicMemoryTests` | 8 | 8 |
| `TranslationLateCorrectionAnnotationConsistencyTests` | 13 | 13 |
| `TranslationRequiredWholeTermLearningTests` | 3 | 3 |
| `TranslationSourcePolarityLearningTests` | 3 | 3 |
| `TranslationWordRoleConsistencyTests` | 10 | 10 |
| **Итого** | **51** | **51** |

Отдельно `TranslationBatchTypeMismatchRollbackControlTests` дал 3/3. Это важное уточнение: основной класс в том же файле сам sink не вызывает; вызов принадлежит контрольному классу. Фильтр по имени файла или одному классу не доказывает исполнение всех вызывающих. Дополнительный прогон закрывает этот конкретный пропуск.

В записи attempt-0004 назван source срез `ce245ae2d3a5f54011732277550d47460a3e17d8`; последующие изменения до проверенного head не меняют эти C# исходники. Однако 86 и 51+3 не являются единым полным прогоном на одной свежей сборке: их области и попытки сохранены раздельно.

## 2. Какие прежние замечания теперь нужно сузить

### Поля маркера и оракул source

Текущий [ownership test](https://github.com/lubochka/llm_mvp_core/blob/6b52307dc26f982dde161ee1d6f470eec1c84de4/Tests/ExperimentFixes/ExperimentAttemptOwnershipTests.cs), метод `OwnerMarkerBindsTheRunToItsAddressAndCircumstancesInEveryBindingField`, проверяет 11 полей. Source сравнивается с окружением, а при пустом значении — с относительным путём. Неверный прежний оракул исправлен; адресная мутация старого ожидания действительно краснит ровно этот тест.

Граница остаётся у build intent / test project / filter: сохранённая запись attempt-0003 явно ограничивает их проверку fallback-ветвью; отдельное доказательство чтения заданных значений всех этих переменных не получено. Существование полей не тождественно связи с фактической сборкой/дочерним процессом.

### Отказ прав в одноразовой записи

Текущий [sink](https://github.com/lubochka/llm_mvp_core/blob/6b52307dc26f982dde161ee1d6f470eec1c84de4/Tests/ExperimentFixes/ExperimentEvidenceSink.cs) перехватывает `UnauthorizedAccessException` внутри `WriteSingleShotBytes` и очищает собственный временный файл (566–578). Утверждение «этой ветви обработки вообще нет» теперь неверно.

При этом каталог создаётся в строке 523 до try; `WriteOwnerStatus` и `AppendJournalLine` также не оборачивают отказ прав. Сохранённые записи прямо говорят, что permission-ветвь исполнением не подтверждена. Реализация и доказанное исполнение этой ветви — разные статусы.

### Доказательство возврата мутаций

Прежнее утверждение о равенстве хешей сборки attempt-0002 снято автором как неподтверждённое сохранёнными файлами. Это правильно не заменено новым хешем задним числом. Attempt-0003 содержит отдельную новую запись о собственных измерениях. Эта рецензия независимо разобрала её TRX, но не выполняла новую сборку и не подтверждает воспроизводимость бинарного хеша на иной среде.

## 3. Остатки, сохранившиеся в текущем коде

### Захват у P58 всё ещё происходит после обучения

Источник: [P5801CorrectiveLessonCounterExampleArmsTests.cs](https://github.com/lubochka/llm_mvp_core/blob/6b52307dc26f982dde161ee1d6f470eec1c84de4/Tests/ExperimentFixes/P5801CorrectiveLessonCounterExampleArmsTests.cs), blob `a0e4f9156e4b852c37fbf44a8c30df99577b0a1a`.

- `Lazy<ExperimentEvidenceSink>` объявлен в 145–146.
- `Evidence.Value` впервые нужен внутри записи, строка 160.
- Основной Fact выполняет cold read в 593 и setup Train в 612.
- Первая запись `setup-probe.json` идёт в 619.

Следовательно, корректный claim внутри фабрики не означает claim до модели на обычном пути. Сохранённый зелёный regression класса не проверяет отказ проигравшей попытки до первого setup/Train. Следующий ремонт должен назначить ранний entry/fixture claim и отдельный различающий контроль, где loser не делает ни одного модельного вызова.

### Завершение не запрещает дальнейшие записи владельца

В [sink](https://github.com/lubochka/llm_mvp_core/blob/6b52307dc26f982dde161ee1d6f470eec1c84de4/Tests/ExperimentFixes/ExperimentEvidenceSink.cs), строки 484–487, `CompleteAttempt` только пишет одноразовый completion-файл. В `WriteSingleShotBytes`, `ResolvePath`, статусе и журнале нет проверки закрытого состояния. Прежний контрпример «CompleteAttempt → новое имя raw» остаётся актуальным по исходнику.

### Доступ к пути и root обходит одноразовую запись

`ResolvePath` (392–404) возвращает путь без резервации имени; `EnsureWriteRootDirectory` (411–420) отдаёт root. Сериализатор вызывающего может перезаписать существующий файл независимо от `File.Move` в `WriteSingleShotBytes`. Приёмка 51+3 подтверждает работоспособность конкретных текущих сценариев, но не вводит единый immutable-контракт для этих каналов.

### Сравнение пути остаётся без учёта особенностей файловой системы

В 636–644 нормализованный candidate сравнивается с boundary через `StringComparison.OrdinalIgnoreCase`. Прежний остаток с соседними `attempt` и `ATTEMPT` на case-sensitive FS не исправлен. В этой рецензии его source condition подтверждён; новый native/filesystem контрпример не запускался.

### Связь с запускателем и внешним итогом не добавлена

Изменения кода после 9dc7f71c не затронули [ExperimentLauncher.psm1](https://github.com/lubochka/llm_mvp_core/blob/6b52307dc26f982dde161ee1d6f470eec1c84de4/scripts/experiment-launcher/ExperimentLauncher.psm1), blob `c56dffe17a587772783d64992745fe9c0c05611e`. У launcher уже есть собственный claim и ожидание процесса; это нельзя записывать как отсутствующий механизм. Общий owner stdout/stderr/TRX и C# writer ещё не подключён.

Ownership-конкуренция в текущем test использует `Barrier` и восемь `Thread` (200–208), не два OS-процесса. Успех этого теста не закрывает межпроцессную приёмку.

### Missing TRX всё ещё маскируется журналом

В [summarize-test-batches.ps1](https://github.com/lubochka/llm_mvp_core/blob/6b52307dc26f982dde161ee1d6f470eec1c84de4/tools/test-batches/summarize-test-batches.ps1), blob `fc59159efb3fec3da02961f6c1dadfe0576ac2e7`, строки 536–541 берут counts из журнала при null TRX counts; строки 548–557 считают completed-пакет закрытым и readable по state. Прежняя находка missing-TRX остаётся. Это вывод чтения актуальной ветви, а не новый запуск PowerShell.

## 4. Документы следующего шага отстали от опубликованного исполнения

На этом head [доска](https://github.com/lubochka/llm_mvp_core/blob/6b52307dc26f982dde161ee1d6f470eec1c84de4/docs/plans/replanning-possible-diffs/claude-audit-board-2026-11-11.md) заканчивается очередью (784–796), где первым всё ещё стоит «Прогон восьми непрогнанных вызывающих». В тексте нет ссылки на attempt-0004 или нового результата 51/51. Приёмка есть в сообщении самого коммита 6b52307d и в сохранённых raw.

Текущий `post-admission/codex-repair-plan.md` также в последнем абзаце назначает этот уже исполненный прогон первым. Нужно добавить датированную поправку в актуальную навигацию и доску, сохранив старые записи как историю:

1. Прогон восьми дополнительных классов: выполнен, 51/51; контрольный rollback-класс отдельно 3/3.
2. Следующий предметный шаг: ранний захват до setup/Train и явное завершение обычных вызывающих, с полной дозой выбранных тестов.
3. Остальные части R01/F03 и R02 сохраняются открытыми по перечисленным границам.

Не называть весь F03 закрытым и не заменять требуемую проверку lifecycle общим зелёным числом. Новое обучение ради исправления навигационного текста не требуется.

## 5. Ключевые pins для повторного чтения

| Источник | Git blob SHA |
|---|---|
| `Tests/ExperimentFixes/ExperimentEvidenceSink.cs` | `26378fecfd01abafc7ae409fe37a86887e85a9ac` |
| `Tests/ExperimentFixes/ExperimentAttemptOwnershipTests.cs` | `cd4cc3aa7228c7b7c26d7c5d93706e1b58450cfb` |
| `Tests/ExperimentFixes/P5801CorrectiveLessonCounterExampleArmsTests.cs` | `a0e4f9156e4b852c37fbf44a8c30df99577b0a1a` |
| `attempt-0004/results/run-01-eight-unverified-callers.trx` | `010217ba599c9d81c474ffc91da8bfc5cebbdbad` |
| `attempt-0004/results/run-02-supplementary-control-class.trx` | `f45fa1253e5ff2e209c874cec4a7fd0943405493` |

Это ограниченный review продолжения F03 и свежести следующего шага. Весь проект, все production consumers и полная библиотека экспериментов заново не аудированы; прежние доказанные A07/experiment результаты этим review не отменяются.

