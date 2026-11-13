# Доказательства для исправленной передачи сессии

3 ноября2026. Проверяемый HEAD: `40552b512cb4fd758758bf7ceec703b9d9f50618`. Это чтение сохранённых результатов и исходников, не новый запуск сборки, тестов или обучения. Рабочее дерево при входе было чистым. Текущая публикация remote этой сверкой не проверялась.

## Какие коммиты и документы связаны

- `e5a42713cee754de5b4245413b30cc91e80b14a0` добавил[расписку полного Debug-прогона](2026-11-03-full-test-run-01-receipt.md). Её верхняя поправка в40552b512cb отделяет авторскую запись от независимо проверенного raw.
- `b4af9eef70f52020e3b10558fe86287dfb054023` меняет единственный продуктовый файл —[TranslationLostCandidateArtifact.cs](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationLostCandidateArtifact.cs:143); тестовых файлов в этом коммите нет. Общий Utf8BytesOf используется хэшем и обоими писателями, включая асинхронный. Это доказательство source-изменения.
- [Запись приёмки Q01](../01-registry.md#q01-byte-acceptance) вb4af содержит сведения автора о командах, успешной сборке Release, исходах целевых прогонов и частичных отпечатках сборок. Она не закрывает весь Q01.

<a id="targeted-trx"></a>
## Два реальных целевых TRX прочитаны заново

Файлы разобраны как XML. Сверены Times, Counters, TestDefinitions и каждый UnitTestResult; SHA256 вычислен от существующих байтов. Это локальные файлы результатов: ссылка и хэш не гарантируют их наличие в другом checkout.

| Поле | До исправления | После исправления |
|---|---|---|
| Файл | [targeted-before.trx](C:/Projects/llm_mvp_core/Tests/TestResults/qa-20261103T083609733Z-/targeted-before.trx) | [targeted-after.trx](C:/Projects/llm_mvp_core/Tests/TestResults/qa-after-20261103T083804246Z-e351c91fd6f54315b31036a7683e9f16/targeted-after.trx) |
| Байты | 47983 | 52856 |
| TestRun ID | `ad3317ea-0f35-41f2-a2a5-59b955f7292a` | `8dad988c-4b2b-4209-b73b-91f1fc8396b8` |
| Начало UTC из XML | 08:36:11.3109215 | 08:43:00.8735215 |
| Конец UTC из XML | 08:36:14.6493294 | 08:43:12.6921995 |
| Total / Executed | 29 /29 | 36 /36 |
| Passed / Failed / NotExecuted | 26 /3 /0 | 36 /0 /0 |
| TestDefinitions storage | `Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll` | тот же путь Release |
| SHA256 TRX | `0fa05359c3cd7f3c1dcb381a206e81acd6faa298e7d0a4a6c0383779c973ad15` | `e803e51c859b4726e5a0dca57dc0a6b7c94de9c3d6fa06b78e5e396e24af4883` |

В исходном XML время записано с `+02:00`; выше оно переведено в UTC. Это время тестового прогона из TRX, не длительность сборки или внешнего процесса. Идентичность **байтов DLL в момент запуска** и exit внешней команды из одного TRX не следуют. Значения build exit0 и разных DLL-хэшей остаются сведениями автора квитанции; новая сборка здесь не выполнялась и полная цепочка её происхождения повторно не аттестована.

Сопоставление выполнено по полному имени класса и метода, без потери записей:29 уникальных имён до,36 после; все29 сохранены после и проходят. Семь добавленных — три случая TranslationMachineContractSignatureTests и четыре TranslationStateVersionLostUpdateTests. Нельзя сравнивать26/29 и36/36 как один неизменный набор или как рост точности обучения.

Именно следующие три теста имели Failed до и Passed после:

1. `TranslationLostCandidateArtifactTests.Losing_candidate_is_persisted_and_restorable_while_prior_code_only_promised_it`.
2. `TranslationWriterConflictAcceptanceTests.Late_cancellation_publishes_nothing_and_still_keeps_the_paid_candidate`.
3. `TranslationWriterConflictAcceptanceTests.Late_conflict_applies_each_lesson_exactly_once_and_the_loser_never_touches_the_winner`.

<a id="remaining-four"></a>
## Семь падений и три — разные области, четыре не закрыты этим свидетельством

В полной Debug-расписке семь падений относятся к24 файлам сессии. Три из них находятся в двух классах из таблицы14 добавленных классов; остальные12 классов таблицы зелёные по расписке. Таким образом,7=3+4; число14 считает классы, не падения. Прочитанные целевые Release-TRX подтверждают исправление трёх названных случаев, а четыре следующих метода в них **вообще не присутствуют**:

| Не закрытый этими TRX случай | Источник теста | Ожидалось → получено в Debug-расписке | Что остаётся |
|---|---|---|---|
| ForeignProvenanceInTheKey_DoesNotLiftTheOrdinaryEnRuSourceFloor | [TranslationPairTailSignatureLearningTests.cs:255](C:/Projects/llm_mvp_core/Tests/ExperimentFixes/TranslationPairTailSignatureLearningTests.cs:255) | другой выученный ответ → `alpha beta` | Установить причину и выполнить назначенную адресную приёмку; текущий PASS/FAIL неизвестен |
| VerifiedRuleTags_SurviveWhileTheProvenanceSegmentIsGone | [тот же файл:152](C:/Projects/llm_mvp_core/Tests/ExperimentFixes/TranslationPairTailSignatureLearningTests.cs:152) | `RULE:TENSE_PAST` → строки с меткой нет | Сверить договор chosen-side rule и тестовый fixture; ожидание не заменять actual без основания |
| ImportedCheckpoint_StoresTheLessonUnderACanonicalKeyWithoutProvenance | [тот же файл:567](C:/Projects/llm_mvp_core/Tests/ExperimentFixes/TranslationPairTailSignatureLearningTests.cs:567) | `RULE:TENSE_PAST` → строки с меткой нет | Сверить происхождение ключа/метки при импорте и назначенный контракт; приёмка не подтверждена |
| ExamContentAppearingInTrainUnderANewName_IsRejectedBeforeLearning | [CapabilityMaterialLineageTests.cs:154](C:/Projects/llm_mvp_core/Tests/ExperimentFixes/CapabilityMaterialLineageTests.cs:154) | `CAPABILITY_MATERIAL_FORBIDDEN_CONTENT_OVERLAP` → `CAPABILITY_MATERIAL_LINEAGE_CONTRADICTS_CONTENT` | Оба значения — отказ. Различие кода не доказывает допуск экзамена к обучению; проверить приоритет guards и договор результата |

Соответствие четырёх описаний методам установлено по исходникам и expected/actual расписки. Полный Debug TRX заново не прочитан: `scratchpad/trx/full-run-01.trx` и `scratchpad/test-run-01.txt` по указанным в ней core-relative адресам отсутствуют. Поиск `.trx` в Tests/TestResults (37 файлов) и TestResults (207 файлов) нашёл только два ноябрьских целевых результата выше. Это ограниченный поиск, не доказательство отсутствия Debug-артефакта во всей системе.628 полных падений не пересчитаны. Вычитать три и объявлять «теперь осталось625» нельзя: конфигурации, сборки и состав прогонов различны.

## Почему Q01 остаётся открыт

- [TranslationMachine.cs:193](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:193): TryPublishVersion возвращает bool; observed из операции CAS не передаётся вызывающему. Сообщение и артефакт читают CurrentVersion позже — это не обязательно та же версия, на которой произошёл конфликт.
- [TranslationMachine.cs:1063](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:1063): после BeforeMeasuredAccuracyPublishBarrier идёт публикация измерения. Нужен контроль поздней отмены **Evaluate**; пройденный тест отмены Train его не заменяет.
- [TranslationMachine.cs:170](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:170): UseLostCandidateStore существует. Ограниченный поиск по27.TranslationDomain,18.CapabilityTraining иTests/ExperimentFixes нашёл объявление и тестовые вызовы. Назначение хранилища обычным продуктовым вызывающим **не установлено этой сверкой**. Это не доказательство отсутствия такого пути во всём продукте.

## Проверка самой правки

Историческая часть handover сопоставлена с полным текстом из40552b512cb: содержимое сохранено (сравнение с нормализацией CRLF/LF). В трёх изменённых Markdown проверено680 локальных файловых целей ссылок, отсутствующих0; это не проверка всех автоматически создаваемых якорей. Для новых внутренних переходов кTRX и четырём случаям заданы явные anchors. Независимый readback работника подтвердил устранение всех четырёх исходных неоднозначностей; его замечание о первом шаге P126 учтено. Коммитные SHA проверены как локальные Git-объекты; наличие этих объектов наGitHub не подменяется ссылкой или названием ветки.

Родитель полностью прочитал исходный handover и разобрал оба TRX. `/root/handover_evidence_review` независимо прочитал handover, сверил Git/source и сопоставил четыре остаточных случая; его проверка не является повторным запуском. Новый handover отделяет действующий вход от исторической записи, сохраняет прежний текст и даёт ссылки на эти доказательства. Полное свежее чтение всего большого canonical AGENTS/CLAUDE и стека навыков в этой работе не выполнено; целевые нормы проверены адресно. Полностью перечитаны локальный AGENTS, prompt-patch-authoring, human-communication-GLOSSARY и test-failure-triage. Полная перечитка стека не заявляется.
