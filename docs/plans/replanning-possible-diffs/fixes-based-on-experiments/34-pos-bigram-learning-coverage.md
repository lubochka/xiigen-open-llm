# 34. Закрепить обучение порядку слов по соседним частям речи

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q45](01-registry.md#work-test-pos). Это отдельное задание на проверку обучения. Продуктовый исполнитель берёт его только по назначению на эту строку. Для отдельной проверки POS используйте шесть заданных уроков. Q1 [3,8,7,10] и Q3 [8,3,7,10] различаются порядком одних и тех же признаков. Читать SVO и VSO через обученный классификатор. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски TRANSLATION:** Сверить builder→learned memory→обычный tokenizer/executor/verifier. Истинный negative и marker-only, exact topic/direction и нормализованные RuleTags не смешиваются; отрицательный смысловой контраст и сохранённый правильный перевод оцениваются раздельно, без подмены gold.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Проверять чтение именно памяти, изменённой назначенным Train, с реальными ключами и payload до потребителя и после restore. Законный kNN retrieval сохранён; запрещена подстановка gold/архивного oracle вместо текущего поиска в обученной памяти. Отдельный PASS структуры или счётчик-константа не доказывает этот consumer либо новые updates.

Для TEST-POS исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Сохранять построчные исходы и независимый gold выбранного домена вместе с итоговой долей. Одинаковая общая точность может скрыть потерю прежнего урока и случайный выигрыш другой строки; изменение scorer/support само по себе не доказывает обучение. Для данной карточки подтверждать назначенный Train и чтение его результата реальным потребителем; существующую проверку усиливать сохранённым learned state вместо дублирования искусственного теста.

Применимость: TEST-POS. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Панель должна сохранять точное множество уникальных RowId, Kind и gold. Контрпример P05: [T1,T2]→[T1,T1] имеет тот же размер, но прячет потерянный урок. Контрпример P11: 2/4→2/4 скрывает E1 gain и E2 loss; constant-answer baseline считать отдельно. 1→0 после настоящего обучения остаётся валидной регрессией. Сдвиг весов сам по себе не квалифицирует результат, PARTIAL и нулевой назначенный Train не закрывают учебную цель.

Применимость: TEST-POS. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

**Уточнение исполнения, 27 октября 2026.** Для двух порядков одинаковых частей речи expected labels задаются независимо до Train. Вызов нового Predict/SequenceDistance для построения ожидаемого ответа запрещён. Сравнить сохранённые before/after поштучно и показать constant-class control: число записей памяти не отличает полезное обучение от постоянного ответа. Один назначенный fit и все прежние readouts используются повторно; отдельный FIX-PQ не заменяет этот тест приобретения.

Общие примеры правильной и ошибочной проверки — [контракт03§24](03-test-and-execution-contract.md#transfer-audit-20261027). Это дополнение плана; новые проверки сейчас не запускались.

ID: **TEST-POS**. Статус: **план недостающего учебного покрытия существующего native learner**, без изменения алгоритма продукта. Основание — положительный E25-PATTERN-CLASSIFIER. Проверенный срез: `1fa9bac16ff209646f7abbd29dad2461df3ed538`. Порядок и следующий ID — только [реестр 01](01-registry.md); этот пункт расположен перед TEST-ORDER. Обязательны [состояния и fixtures 02](02-learning-state-and-fixtures.md) и [тесты, skills, ожидание, STOP/NEXT 03](03-test-and-execution-contract.md).

Цель будущего теста — показать, что один fit настоящего `SemanticCompiler.PatternClassifier` приобретает различение двух порядков одних и тех же частей речи. Сейчас добавляется только этот план; новые Train, inference, build и test не выполнялись. Тема обучения — **перевод**; задача — определение абстрактного порядка слов, профиль `pos-order-svo-vso`. SVO/VSO — классы одной задачи; POS, kNN и направление языка не называются отдельными доменами. Тест не оценивает перевод предложения и не назначает POS-механизм для C# AST.

## 1. Доказанное основание и точная новизна

| Первичное свидетельство | Измеренное улучшение | Цена и границы |
|---|---|---|
| [E25 raw, 1 мая](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json) | `top1_accuracy 0.3438 → 0.875`; 160 примеров, 32 validation; hard subset 28, accuracy 0.8571 | Записанное время baseline/enhanced `0.4 → 15.4 ms`. Это время readout/scoring после Train, не стоимость всего обучения. Память не измерена |
| [E25 raw, 7 мая](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E25-PATTERN-CLASSIFIER/results/run-2026-05-07-001.json) | Те же доли и размеры; в самом JSON `tier=1`, несмотря на имя родительской папки | Readout/scoring `0.4 → 6.6 ms`. Второй сохранённый запуск той же синтетической задачи не считается независимым корпусом или проверкой tier4 |

[Runner](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/35.ExperimentsV2/Experiments/E25PatternClassifier.cs#L20-L47) генерирует 160 POS-последовательностей, делит 80/20, действительно вызывает `Train(train)` до `Predict(val)`. Поэтому здесь есть участие обученного состояния в измеряемом результате. Alias [GlobalUsings.cs](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/35.ExperimentsV2/GlobalUsings.cs#L11) указывает на `SemanticCompiler.PatternClassifier` проекта 12. Случайный split не отделяет шаблонные семейства; exact historical таблица, пересечение последовательностей и SHA бинарника в raw не записаны. Процент не переносится на реальные языки, новый corpus или итоговую точность перевода.

Любое измеренное улучшение полезного показателя — успех по этому показателю и профилю; рост accuracy для ресурсного выигрыша не обязателен. Остальные показатели, включая ухудшения, показываются рядом. В данном основании accuracy выросла; это не отменяет увеличившуюся задержку. Будущая панель ниже имеет статус **NOT_MEASURED**: её критерий `2/4 → 4/4` выведен из кода и математики, а не объявлен новым запуском.

[Исследование 17](../17-positive-pos-knn-versus-production-rules.md) уже описывает E25 и различие проектов 12/29. Его аудит не повторяется. Новизна — точный постоянный тест одного native fit на шести строках, четыре заранее заданных новых входа, контроль одинакового набора POS при разном порядке, независимый gold, бюджет и команда.

Существующее покрытие не названо отсутствующим. В [E25E26E27Tests](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/Tests/ExperimentsV2Tests.cs#L1565-L1624) уже есть `E25_Run_Tier1_ProducesResult`, `E25_Run_EnhancedNotWorseThanBaseline`, три rule-теста и проверка генератора. Второй Run действительно обучает kNN; его общий критерий допускает до 0.05 потери относительно baseline. Он не фиксирует before/after на одном экземпляре и не проверяет противоположные порядки при одинаковом наборе POS. В [SemanticCompilerTests](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/Tests/SemanticCompilerTests.cs#L159-L182) проверены три `PredictByRules`. Новая карточка дополняет эти проверки; не заменяет их и не ослабляет assertions. Проверка новизны ограничена 29 документами данного пакета, перечисленными двумя test-файлами и primary learner/runner; полный аудит всех tests не заявлен.

## 2. Реальный механизм и область изменения

В [PatternClassifier.cs:39–54](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/12.SemanticCompiler/Translation/PatternClassifier.cs#L39-L54):

```csharp
public void Train(IReadOnlyList<(int[] PosSeq, int Pattern)> examples)
{
    _train.Clear();
    _train.AddRange(examples);
}
// Predict: OrderBy(SequenceDistance).Take(_k).GroupBy(Pattern)
//          .OrderByDescending(g => g.Count()).First().Key
```

**Сейчас:** `Train` заменяет таблицу размеченных последовательностей; `Predict` читает эту таблицу. При пустой таблице reader вызывает `PredictByRules`. **В будущем покрытии:** тот же публичный API и тот же `k=3`; до fit записать четыре ответа, один раз передать шесть уроков, затем записать четыре ответа тех же входов. Учебные массивы после передачи не менять: `AddRange` сохраняет ссылки на них. Никаких optimizer, LoRA, DPO, генерации учителем или нового числового вектора здесь нет.

Место нового кода — только `Tests/ExperimentFixes/PatternClassifierLearningTests.cs`, назначенный ниже evaluator и fixture JSON. Нативный класс остаётся существующей внутренней деталью semantic compiler, не объявляется новой `ILearningMachine` или новым router. Копия learner, reflection-доступ к `_train`, передача gold в `Predict`, новые production interfaces и переписывание `SequenceDistance` не нужны.

### Почему нельзя одновременно заменить компонент проекта 29

[SemanticCompiler29.PatternClassifier](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/29.SemanticCompiler/Translation/PatternClassifier.cs#L8-L65) принимает слова, выводит POS короткими списками и применяет фиксированные правила; у него нет `Train`. Найденный [fusion consumer:213–238](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L213-L238) использует именно эту версию, добавляя 0.20 за совпадение метки. Historical E25 измерял другую задачу: POS → структура, не text → POS → выбор конечного ответа. Полезное измерение не доказывает, что смена этого fusion-компонента улучшит ответ. TEST-POS не меняет проект 29, fusion coefficient, source normalization или runtime dispatch.

## 3. Математическое основание и доменные пределы

Для последовательности `s` код [78–86](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/12.SemanticCompiler/Translation/PatternClassifier.cs#L78-L86) строит множество соседних упорядоченных пар `B(s)={(s_i,s_(i+1))}`. Расстояние:

`d(a,b) = 1 − |B(a) ∩ B(b)| / |B(a) ∪ B(b)|`.

Три ближайшие строки голосуют за класс. Это сохраняет локальную направленность пар. Контроль `Q1=[3,8,7,10]` и `Q3=[8,3,7,10]` имеет один и тот же multiset `{3,7,8,10}`, но разные пары `(3,8)/(8,7)` и `(8,3)/(3,7)`. Детерминированный classifier, которому доступен лишь этот multiset, обязан дать одинаковый ответ обеим строкам; при противоположном gold хотя бы одна будет ошибочной. То же верно для Q2/Q4. Поэтому 4/4 на панели нельзя получить constant-label или исключительно unigram-признаками. Это ограниченное доказательство выразительности представления и критерий теста, а не измерение нового обучения.

| Вход | Три ближайшие учебные строки в установленном порядке | Расстояния | Заранее ожидаемый итог голосования |
|---|---|---|---|
| Q1 | T1, T2, T3 | 1/3, 4/5, 4/5 | SVO |
| Q2 | T2, T1, T3 | 1/4, 4/5, 5/6 | SVO |
| Q3 | T4, T5, T6 | 1/3, 4/5, 4/5 | VSO |
| Q4 | T5, T4, T2 | 1/4, 4/5, 5/6 | VSO |

В Q2 третий T3 делит расстояние с T5; в Q4 третий T2 — с T6. У обеих строк уже первые два голоса принадлежат нужному классу, поэтому ожидаемая метка не зависит от выбора этой ничьей. Точные neighbor IDs — объяснение математики, не обязательный private-state oracle. Тест проверяет публичные ответы и исходные данные, не копирует внутренний алгоритм для формирования expected.

Множество биграмм не сохраняет число повторов и весь глобальный порядок. Например `[7,8,7]` и `[7,8,7,8,7]` имеют одинаковое множество пар и расстояние 0. Классификация синтетических POS не даёт общей гарантии для отрицания, согласования, всех языков, длинных зависимостей или порядка AST C#. В новую панель эти темы не добавляются; иначе меняются задача и основание эксперимента. Наибольший обоснованный приоритет — учебное покрытие этой конкретной способности, а не универсальная product замена.

## 4. Полностью заданная панель одной темы

Теги из native source: `DET=0`, `PRON=3`, `NOUN=7`, `VERB=8`, `ADJ=9`, `ADV=10`. Классы: `SVO=0`, `VSO=2`; native fallback `IMPERATIVE_V=3` не переименовывается. Формы SVO/VSO имеют тот же структурный смысл, что шаблоны [GenerateExamples:94–103](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/12.SemanticCompiler/Translation/PatternClassifier.cs#L94-L103), но строки ниже — новый авторский regression fixture, не восстановленные historical160.

TRAIN, ровно в порядке T1 → T6:

| ID | POS-массив | Авторская метка |
|---|---|---|
| T1 | `[3,8,7]` | 0 / SVO |
| T2 | `[3,8,9,7]` | 0 / SVO |
| T3 | `[3,8,0,7]` | 0 / SVO |
| T4 | `[8,3,7]` | 2 / VSO |
| T5 | `[8,3,9,7]` | 2 / VSO |
| T6 | `[8,3,0,7]` | 2 / VSO |

EXAM, четыре отдельных входа, ни один не равен TRAIN; examiner получает ID/массив, learner — **только массив**:

| ID | POS-массив | Отдельный gold | Аналитически ожидаемый native BEFORE | Аналитически ожидаемый AFTER |
|---|---|---|---|---|
| Q1 | `[3,8,7,10]` | 0 / SVO | 0 / SVO | 0 / SVO |
| Q2 | `[3,8,9,7,10]` | 0 / SVO | 0 / SVO | 0 / SVO |
| Q3 | `[8,3,7,10]` | 2 / VSO | 3 / IMPERATIVE_V | 2 / VSO |
| Q4 | `[8,3,9,7,10]` | 2 / VSO | 3 / IMPERATIVE_V | 2 / VSO |

Сбалансированные два класса дают constant baseline 2/4. Q1/Q3 и Q2/Q4 — отрицательные контроли потери порядка внутри одной темы; не смешение нескольких предметных наборов. Gold написан здесь до чтения будущих outputs, хранится отдельно и не выводится из `PredictByRules`, ближайшей строки или actual результата. BEFORE берётся реальным `Predict` пустой таблицы, а не искусственной записью этих ожидаемых значений. AFTER — реальным `Predict` после единственного fit. Длина/число примеров, k, теги и метки не подбираются до зелёного результата.

Панель проверяет near-template перенос с добавленным конечным ADV; это не family-disjoint или языковой corpus. Все четыре новых expected ещё не измерены. Не выдавать будущие 4/4 за historical .875, не добавлять скрытый bootstrap и не увеличивать набор для получения нужного процента.

## 5. Состояние и повторное использование без нового поиска

Native `PatternClassifier` в прочитанном полном файле не имеет save/load/export API. Historical trained table в двух raw не сохранена. Поэтому совместимый готовый checkpoint здесь не назначается. Используется ровно один минимальный fit, затем один и тот же неизменяемый результат acquisition переиспользуют оба теста. Это явное исключение к предпочтению восстановления, обусловленное реальным API; оно не разрешает создавать новый lifecycle или выдавать JSON уроков за обученную модель.

| Планируемый путь | Точное содержимое/ответственность |
|---|---|
| `Tests/Fixtures/ExperimentFixes/v1/PosBigram/train.json` | `[{"id":"T1","pos":[3,8,7],"label":0}, …]`, ровно T1–T6 из раздела 4 |
| `Tests/Fixtures/ExperimentFixes/v1/PosBigram/eval-inputs.json` | Ровно Q1–Q4, поля id/pos; без gold/neighbor/route hints |
| `Tests/Fixtures/ExperimentFixes/v1/PosBigram/panel-gold.json` | Ровно Q1–Q4, поля id/label из отдельного gold; evaluator читает после сохранения всех outputs |
| `Tests/Fixtures/ExperimentFixes/v1/PosBigram/provenance.json` | Паспорт ниже; SHA256 реальных записанных bytes трёх data-файлов, без самохэширования manifest |
| `Tests/ExperimentFixes/PatternClassifierLearningTests.cs` | Два `[Fact]` раздела 6; native type указан полным `SemanticCompiler.PatternClassifier`, обычная существующая reference проекта 12 |
| `Tests/ExperimentFixes/PosBigramLearningFixture.cs` | Один xUnit `IClassFixture` для обоих тестов; загружает material, создаёт native k=3, записывает before → Train → after и неизменяемую квитанцию; второго fit нет |
| `Tests/ExperimentFixes/PosBigramPanelScorer.cs` | Только evaluator: независимые row correctness, class support/confusion, сохранение двух старых правильных и приобретение двух новых; без Train или runtime decision |

Паспорт для общего loader документа 02:

| Поле | Значение |
|---|---|
| `fixture_id` | `pos-bigram-native-v1` |
| `source_commit`, `source_paths` | `1fa9bac16ff209646f7abbd29dad2461df3ed538`; точные native/runner/raw пути разделов 1–2 |
| `fixture_authority_card`, `fixture_authority_commit` | Путь этой карточки 34 и реальный commit её опубликованной версии при реализации, не `latest` |
| `topic`, `profile`, `learner` | `перевод`; `pos-order-svo-vso`; `SemanticCompiler.PatternClassifier.Train/Predict` |
| `state_kind`, `donor_path` | `single-fit-in-memory-table`; `not_applicable:no_native_save_load_or_historical_table` |
| `model_identity` | `SemanticCompiler.PatternClassifier`, assembly `SemanticCompiler`, pinned source, `k=3`, tag schema и exact PatternNames из native source |
| `tokenizer_identity` | `not_applicable:author_pos_arrays`; текстовый POS-tagger и BPE не вызываются |
| `optimizer_policy`, `rng_policy` | `not_applicable:no_numeric_optimizer`; `not_applicable:deterministic_no_rng` |
| `train_ids`, `exam_ids`, `sha256` | T1–T6 и Q1–Q4 в порядке раздела 4; hashes настоящих fixture bytes |
| `model_facing_material_digest` | SHA256 bytes `train.json`; отдельно digest ordered `(int[] PosSeq,int Pattern)` после deserialization, до Train |
| `qualified_metric`, `historical_limitations` | Historical E25 top1; future panel `not_measured`; synthetic/non-family-disjoint, timing excludes Train, native checkpoint отсутствует |

Использовать назначенный в02 `Tests/ExperimentFixes/ExperimentFixtureLoader.cs`; если ещё не создан, создать только этот общий loader с branch `single-fit-in-memory-table`. Для данной панели он валидирует JSON/identity и возвращает неизменяемые массивы, не вызывает H512 donor, neural bootstrap, optimizer/RNG loader или внешний teacher. Не сериализовать `_train` reflection-ом. Перезапуск другого test process означает новый cold fit и должен быть учтён как новый запуск, а не назван restore.

## 6. Тесты и конечный бюджет

Один класс `LlmFromScratch.Tests.ExperimentFixes.PatternClassifierLearningTests` использует один `IClassFixture<PosBigramLearningFixture>`. Обе проверки получают уже записанную квитанцию одного acquisition; порядок вызова тестовых методов на обучение не влияет.

1. `NativeFitAcquiresVsoWithoutLosingSvo`: проверить неизменность учебных массивов и IDs; реальные outputs до/после, `train_calls=1`, `train_rows=6`, `predict_calls_before=4`, `predict_calls_after=4`; correctness BEFORE 2/4, AFTER 4/4, SVO 2/2 сохранены, VSO 0/2→2/2. Это критерии будущего теста, не текущие измерения. Число вызовов записывает test fixture на границе реальных public вызовов, без фиктивного learner.
2. `SamePosMultisetWithDifferentOrderKeepsDifferentLearnedLabels`: по тем же receipts проверить равные sorted POS-массивы Q1/Q3 и Q2/Q4, разные назначенные gold и разные реальные AFTER labels; отсутствие exact TRAIN/EXAM дубликатов. Не вызывает Train/Predict заново. Контроль привязан к результату одного обучения, не является самостоятельным учебным экспериментом.

Бюджет новой панели: **1 Train × 6 строк, 4 BEFORE + 4 AFTER = 8 Predict**, 0 дополнительных fit, 0 neural updates, 0 teacher calls, 0 старых E25 Run. Нет acquisition до «первого удачного результата»: шесть строк — весь фиксированный бюджет. Измерить wall/CPU/allocated bytes для Train отдельно от двух readout участков; сохранить любые ухудшения/рост памяти. Бюджет не разрешает warm-up Train, многократный benchmark, выбор лучшего seed или повтор для «более красивого» времени. На таком размере одно измерение времени описательное, без вывода об ускорении.

### Старые проверки и разбор падений

| Существующая проверка/симптом | Решение в пределах TEST-POS |
|---|---|
| `E25_Run_Tier1_ProducesResult`, `E25_Run_EnhancedNotWorseThanBaseline` | Оставить исходники и assertions. Не включать в новый точечный filter: они выполняют дополнительные synthetic Train и не входят в бюджет панели. Это ограничение данного запуска, не Skip/удаление существующих тестов |
| `E25_PredictByRules_ImperativeV_DetectedWhenVerbFirst`, `E25_PredictByRules_SVO_WhenNounBeforeVerb`, `E25_PredictByRules_SOV_WhenVerbLast` | Сохранить и выполнить как три старых native fallback-контракта |
| `SemanticCompiler_PatternClassifier_Imperative_VerbFirst`, `SemanticCompiler_PatternClassifier_SVO_NounVerbNoun`, `SemanticCompiler_PatternClassifier_SOV_EndsWithVerb` | Сохранить и выполнить; не менять rule ожидания на learned labels |
| Новый AFTER равен BEFORE или поменял SVO на VSO | Проверить namespace, один настоящий Train, k=3, JSON числа, порядок/неизменность arrays и раздельность gold. Исправлять test wiring, не подставлять actual как expected и не менять native fallback |
| Production API изменился после pinned source или native читатель не даёт заявленного результата | Сохранить конкретный diff/неудачную квитанцию и назвать невыполненный критерий. Этот test-only план не даёт права заменять алгоритм/данные или маскировать дефект. Измеренный исторический успех остаётся историческим |
| Сборка не доходит до test discovery | Применить только [общую предпосылку02§10](02-learning-state-and-fixtures.md#native-build-prerequisite), одинаковую для всех; не чинить весь solution и не считать ноль tests PASS |

Свежие падающие tests этой документной сессией не установлены. Ни одно имя выше не объявляется красным без нового лога. Общий договор03 об изменении tests действует; допуск 0.05 старого historical test не переносится в новый exact-label тест и не ужесточается скрыто в этом scope.

## 7. Команды, артефакты и ожидание

Корень Windows: `C:/Projects/llm_mvp_core`. Existing Tests project: `Tests/LlmFromScratch.Tests.csproj`, Release/net9.0. До первой команды выполнить только общую предпосылку02§10. Новый test source сначала реализуется; до его появления запуск нового filter не назначается.

**Baseline перед новым test-кодом:** ровно шесть существующих rule-тестов, 0 Train:

```powershell
Set-Location C:/Projects/llm_mvp_core
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName~E25E26E27Tests.E25_PredictByRules_|FullyQualifiedName~SemanticCompilerTests.SemanticCompiler_PatternClassifier_" --logger "trx;LogFileName=TEST-POS-baseline.trx" --results-directory artifacts/fbe/TEST-POS/attempt-0001/baseline
```

**После добавления тестов/fixtures:** восемь tests, включая два новых; только один shared fit:

```powershell
Set-Location C:/Projects/llm_mvp_core
$env:XIIGEN_FBE_SOURCE_ROOT = 'C:/Projects/llm_mvp_core'
$env:XIIGEN_FBE_OUTPUT = 'C:/Projects/llm_mvp_core/artifacts/fbe/TEST-POS/attempt-0001/resources'
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName~PatternClassifierLearningTests|FullyQualifiedName~E25E26E27Tests.E25_PredictByRules_|FullyQualifiedName~SemanticCompilerTests.SemanticCompiler_PatternClassifier_" --logger "trx;LogFileName=TEST-POS.trx" --results-directory artifacts/fbe/TEST-POS/attempt-0001/tests
```

Ноль или отсутствие одного из named tests — не PASS. Discovery сверить с именами, не только итоговым count. Fixture loader получает source root из `XIIGEN_FBE_SOURCE_ROOT`, а output — из `XIIGEN_FBE_OUTPUT`; обе переменные обязательны и проверяются до BEFORE/Train. Под source root должны существовать `Tests/LlmFromScratch.Tests.csproj`, `12.SemanticCompiler/SemanticCompiler.csproj` и ровно назначенные JSON в `Tests/Fixtures/ExperimentFixes/v1/PosBigram/`. Не полагаться на CopyToOutputDirectory, не искать по компьютеру и не подставлять другую папку. Output должен быть именно `source_root/artifacts/fbe/TEST-POS/attempt-0001/resources`; отсутствие/неверность переменной даёт конкретный setup-failure с нулём учебных вызовов. Новые input paths вычисляются только от указанного source root. Native before/after записать в `resources/acquisition.json`, сравнение scorer — `resources/comparison.json`, русский вывод — `report.md` в корне попытки.

До after-command исполнитель проверяет исходный handle и отсутствие прежних учебных outputs. `baseline/` и `tests/`, созданные logger, не блокируют первое обучение. Первое действие fixture после проверки env/data-paths, до BEFORE и Train: атомарный `FileMode.CreateNew`, `FileAccess.Write`, `FileShare.None` для `resources/ownership.json` с PID/runId/startUTC. Существующий ownership или прежние `acquisition.json`/`comparison.json`/`status.json` запрещают новый fit; нельзя удалять marker, перезаписывать результаты или делать autosuffix. В `status.json` записывается running, затем в `finally` terminal success/failure со стадией, exception и реально завершёнными counts. Ошибка setup до захвата ownership выводится в TRX/stdout с нулевыми counts, без создания ложной учебной квитанции. Аварийный выход без finally определяется по исходному tool handle/exit. Отсутствие файла не является доказательством отсутствия запуска.

Runtime receipt содержит source/build identity, manifest/data hashes, реальные counts, outputs по IDs, времена отдельных стадий и ограничения; отчёт формируется из завершённой квитанции без Train. При существующем результате сначала прочитать status/handle; acquisition из-за отсутствия log не повторять. Живой процесс ожидается штатным wait **того же** подтверждённого handle: `write_stdin` для `exec_command.session_id`, `functions.wait` только для `Script running with cell ID`, либо документированный wait текущего PowerShell tool. Интервалы не длиннее60секунд, без фонового polling-loop и без нового процесса-waiter. Не заканчивать ход фразой «жду», пока доступны разрешённые действия ожидания.

## 8. Граница prompt → domain E2E

TEST-POS не обучает `PromptFlowGroundingModel`, не возвращает `FlowRef` и не выбирает предметный домен. `SVO=0` — синтаксическая метка, не domain0. Архитектура [FIX-E2E20](20-prompt-domain-e2e.md), три topic и шесть запросов сохраняются; этому дополнению нужны **0 новых E2E запросов,0 дополнительных Train,0 topics**. Его точка интеграции сейчас — Test → native project12 learner → scorer. Включение этого learner в text receiver/fusion требует отдельного измерения влияния на результат получателя и здесь не выдаётся за доказанный перенос. Code/AST структура и natural-language POS не подменяют друг друга.

## 9. Готовый промпт исполнителю

> Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/34-pos-bigram-learning-coverage.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.
>
> Выполни только назначенный TEST-POS по реестру01. Цель — постоянный native тест приобретения различия SVO/VSO после одного fit; сейчас не переносить learner в продукт29 или fusion. Прочитай AGENTS.md, CLAUDE.md, эту карточку,02/03; skills `docs/skills/architect-behavior-classifier-SKILL.md`, `trainable-architecture-decision-SKILL.md`, `intent-to-plan-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `generated-code-review-SKILL.md`, `test-integrity-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`. Модель — фактическая текущая модель сессии по умолчанию, без override. Используй ровно native `SemanticCompiler.PatternClassifier`, k=3, файлы и JSON-строки разделов4–5, named tests и команды6–7. Сначала общая предпосылка02§10 и baseline шести rule-tests. Затем один shared fixture: 4 реальных BEFORE, один Train шести строк,4 AFTER; оба новых tests читают эту квитанцию без второго fit. Gold отдельный и не входит в learner. Старые E25 Run не запускать, source/results не удалять. WRONG: тестировать другую namespace, переписанную копию алгоритма, готовый answer вместо Train или bag-of-POS, смешивать AST/другие темы, называть .875 или будущие4/4 своим новым измерением. CORRECT: один native acquisition, независимые outputs/gold, одинаковые multisets с противоположным порядком и фактическими разными labels, явная цена. Любой полезный измеренный выигрыш признать по показателю; показать все ухудшения, 0→0 не считать учебным результатом. При падении чинить конкретную wiring/fixture ошибку, не менять gold/k/native алгоритм ради PASS. Состояние native не сериализуется: save/load отсутствует; никакого скрытого bootstrap, reflection restore или поиска другого donor. Живой запуск ожидай штатным wait того же handle до терминального исхода, не заканчивай ход и не создавай polling/relaunch. При прямом STOP/NEXT немедленно прекрати старую карточку: запрещено дописывать её отчёт, tests, code, commit или cleanup под предлогом завершения; сообщи только подтверждённый статус, при NEXT возьми заданный ID реестра. Отчёт — русский по03§8, из завершённых receipts, без повторного обучения; следующий ID только из реестра.
