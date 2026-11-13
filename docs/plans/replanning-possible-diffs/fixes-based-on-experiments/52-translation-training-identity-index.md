# FIX-IDX — индекс пар идентичности после записи учебного примера

<!-- POST113-CURRENT-2026-11-02:START -->
## После P113 — provenance не заменяет semantic key

PairId остаётся личностью/происхождением урока, но не основанием source floor 0 и не частью canonical semantic key выбранного S04. Actual content/provenance hash сверять из реальных bytes, не принимать новый payload со старой подписью. Positive разрешённый урок/RULE до и после PAIR сохраняются во всех readers. Negative одинаковый новый key с разными несовместимыми payload не допускается к затиранию.

Identity index, history, graph и quality публикуются одной версией. Новый holder уже есть; R01 сериализует writers и привязывает metrics к измеренному состоянию. Count/hash без ordinary tokenizer/executor/verifier и restored target не закрывает перенос.

Основание: [адресный план 135](codex-135-post113-addressed-repair-plan-2026-11-02.md). Срез `21007e25ec3f6f6634dd8733fc8631ae267e518c`, 2 ноября 2026. Этот блок уточняет указанные условия; исторические результаты, собственные данные, единственный фактор, учебный бюджет и очередь сохраняются. Порча копии квитанции не назначает новое обучение или дополнительное чтение модели. Нижние датированные указания применяются в своей исходной области; новые требования не делают старые ненаблюдённые поля измеренными.


**Уточнение перед публикацией, 1aaa3bc:** канонический основной builder, PairId, ряд readers/imports, collision preflight и удаление PAIR-обхода floor уже реализованы в исходнике; native gate UNRUN. Открыты version/default, достижение кандидата N16, RULE/restore и новая N18 (non-object JSON в collision preflight). Действующий остаток — [135/R09](codex-135-post113-addressed-repair-plan-2026-11-02.md), подробная сверка — [отдельный документ](../../../../experiments-v2/_audits/2026-11-02-post113-audit/publication-reconciliation.md). Повторять реализованные части M03 не требуется.
<!-- POST113-CURRENT-2026-11-02:END -->

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски TRANSLATION/STATE:** Сверить builder→learned memory→обычный tokenizer/executor/verifier. Истинный negative и marker-only, exact topic/direction и нормализованные RuleTags не смешиваются; отрицательный смысловой контраст и сохранённый правильный перевод оцениваются раздельно, без подмены gold. Проверить полный typed learned payload и применимые RNG/tokenizer/index/maps, состояние сразу после вмешательства и отдельный fresh-load только по назначенному API. Положительный старый ответ сохраняется; внешняя мутация/чужая identity должна обнаруживаться. Count/hash сами не доказывают восстановление.

**Ожидания тестов:** собственные данные/positive/negative этой карточки и названный ordinary caller имеют приоритет над примерами120. Пример другого семейства не добавляет fixture, Train или новый API. Полный набор outer/nested Train/BPE/Grammar, readouts, restore и этапов остаётся в существующем бюджете. Структурный guard сTrain0 не объявляется новым обучением.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->
<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

**Учебная личность и смысловая индексация имеют разные назначения.** PairId обеспечивает provenance/dedup урока; semantic key обеспечивает смысловой поиск. Наличие номера не является основанием `minimumSourceSimilarity=0`. Выбран S04: вынести PAIR отдельно, сохранить pattern/concepts/проверенные RULE и существующие 0/0.60  для законных оснований.

Состав миграции: writers структурированного урока, semantic corrections, executor lookup, verifier requirements, индексы и serializer/import. Смена одного runtime reader оставляет записанные требования недостижимыми. Legacy migration удаляет только отдельный PAIR-сегмент и выявляет неоднозначные colliding keys до изменения live state; порядок dictionary не выбирает ответ.

Приёмка: два PairId одного смысла; RULE после номера; законная семантика; unknown semantics с source ниже/на/выше.60; конфликт old checkpoint. Нужен ordinary path receipt, исключающий скрытый exact/phrase/word early return в тесте KNN. Вернуть PAIR-bypass намеренной порчей — тест должен упасть. Проверить восстановление требований verifier новым объектом. Исходные checkpoints и красные журналы сохраняются.

Основание: [аудит 123](codex-123-post107-transfer-and-experiment-audit-2026-11-02.md), [план 124](codex-124-post107-addressed-repair-plan-2026-11-02.md), [контекст 41](../experimets/codex-41-post107-executor-checks-2026-11-02.md). Это уточнение приёмки будущего переноса; прежние результаты и назначенная доза сохраняются.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Для ресурсного переноса сравнивать actual native операции на том же обученном состоянии и в одинаковом окне. Managed allocations, peak и CPU различаются; прогрев/дрейф показываются по блокам. P08 продемонстрировал полезный gold-mass эффект при allocations+155,65%: наличие quality gain и кода в продукте не отменяет HOLD по памяти. Константа счётчика не является наблюдением.

Для FIX-IDX исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Стоимость относится к точно названному окну. Контрпример P09: разность allocations четырёх arm windows, делённая на4, включает создание входов/cache/receipts и не является постоянной стоимостью одного Forward. Для этой оптимизации указать размер работы, warmup, что внутри/снаружи счётчиков и одинаковую обвязку A/B; не закреплять точные bytes по одному такому среднему. Проверять существующие tests и дополнять их learned fixture, не объявлять отсутствующими по одному отчёту.

Применимость: FIX-IDX. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Полный ресурсный бюджет обязан включать обязательные диагностические вызовы и неудачные попытки, с отдельными единицами model/layer и optimizer-call/effective-update. Контрпример P11: 40 model Forward основной части +19 контрольных =59; 8 optimizer calls дают лишь4 изменения весов. Oracle/FD, заявленный обязательным, должен влиять на принятие результата, а не только выводиться в JSON. Равенство старому алгоритму подтверждать независимой проверкой, не комментарием в исходнике.

Применимость: FIX-IDX. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-IDX

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** TRANSLATION/STATE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**K — владение допущенным состоянием и восстановление.** Показать, какой объект хранит допущенные параметры, примеры, конфигурацию или ключ кэша; интерфейс IReadOnlyList сам по себе не доказывает неизменность. Контроль после создания/допуска: изменение исходного caller-owned массива или внешнего представления не меняет захваченную запись незаметно. Законное обновление/смена model identity вызывает предусмотренное обновление или инвалидирование кэша, а не запрет обучения. Если reader сохраняется/восстанавливается, сравнить поведение обычного consumer по сохранённым данным и настройкам: reconstruction с default не является эквивалентным restore opt-in режима. Ошибка из KDP: catalogue был проверен, затем изменён через возвращённый массив; schema не содержала catalogue. Для этой карточки проверять её собственный carrier, не добавлять чужой KNN-механизм ради теста. Если изменяемого carrier/сохранения нет, отметить это явно в evidence и не выдумывать лишний runtime.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Разделение индекса и обученного результата, 27 октября 2026.** Новый HashSet — производная от полной пары (ExampleId, UnitId), а не источник сохранённого ответа. Успешный duplicate-key lookup не означает, что повторный урок был исполнен: уже назначенный DuplicateLessonStillTrains должен проверять actual writer/state/output при неизменном числе identities. Import восстанавливает ключи из прочитанных checkpoint rows, без добавления уроков из fixture и без нового Train в restore-стадии.

Historical −13.35% Train wall не закрывает общий resource gate: +8 200 training allocated bytes, +38 952 restore bytes и другие ухудшения §1 требуют собственных относительных знаменателей и отдельных окон. Неизвестный процент не считается≤5%; observed_axis_gain и перенос, прошедший правило69, — разные результаты. Контроль использует имеющиеся четыре урока/restore receipts и не расширяет учебный бюджет.

ID: **FIX-IDX**. Основание: **IDENTITY-TRAIN-INDEX-01 / IDX01**, результат `experiments-v2/RP01-MICRO/IDX01/a001/results/`. Проверенный evidence/source срез — [`ebaf7760a36d43172bd88317ff469edfd09a615b`](https://github.com/lubochka/llm_mvp_core/commit/ebaf7760a36d43172bd88317ff469edfd09a615b); исторический source pin опыта — `dd150aa5216e42b8a8bba14955337398ec5f8397`. Статус: **ограниченный кандидат по измеренным полезным time-осям, при ухудшении расходов памяти; native перенос ещё не выполнен**. Текущая работа документная. Порядок [реестра](01-registry.md): **FIX-JO → FIX-IDX → FIX-N60**. Это новый product factor, которого нет в [19 TEST-TMEM](19-translation-memory-coverage.md) и [46 FIX-JO](46-training-json-options-reuse.md).

## 1. Основание: наблюдался полезный результат, хотя исходная allocation-гипотеза не подтвердилась

Внутри настоящего `TranslationMachine.TrainStructuredBatchAsync` после записи каждого урока выполняется линейный поиск пары `(ExampleId, UnitId)` в журнале учебных идентичностей. В плече B этот поиск заменён вспомогательным `HashSet` той же пары. Сам вызов обучения, содержимое урока, память executor и порядок уроков сохранены. Индекс заново строится при импорте полного checkpoint. Это изменение непосредственно участвует в обучении и восстановлении его состояния.

Первичные источники: [исторический report](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/IDX01/a001/results/report.md), [A/receipt](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/IDX01/a001/results/A/receipt.json), [B/receipt](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/IDX01/a001/results/B/receipt.json), [comparison](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/IDX01/a001/results/comparison.json), [score-summary](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/IDX01/a001/results/score-summary.json), [точный diff](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/IDX01/a001/results/one-factor.diff), [manifest](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/IDX01/a001/results/manifest.json). Каталог `IDX01/results` и diff в корне `a001` не существуют; использовать полные paths выше.

| Измеренная ось | A | B | Вывод по этой оси |
|---|---:|---:|---|
| Wall одного настоящего Train |22,2294 мс |**19,2621 мс** |**−2,9673 мс, −13,35%** в единственной паре |
| CPU того же Train |31,25 мс |**15,625 мс** |**−15,625 мс**; разница в один наблюдаемый квант, слабая причинная доказательность |
| Process allocated за Train |483 816 Б |492 016 Б |**Ухудшение +8 200 Б**, около1,69% |
| Thread allocated за Train |486 488 Б |492 760 Б |**Ухудшение +6 272 Б**, около1,29% |
| Wall полного restore |81,5613 мс |73,8270 мс |Наблюдаемое уменьшение7,7343 мс |
| CPU полного restore |78,125 мс |93,750 мс |**Ухудшение +15,625 мс** |
| Process allocated за restore |4 149 032 Б |4 187 984 Б |**Ухудшение +38 952 Б** |
| Wall всей A/B фазы |710,4107 мс |735,4536 мс |**Ухудшение +25,0429 мс** |
| CPU всей фазы |484,375 мс |437,500 мс |Наблюдаемое уменьшение46,875 мс; не приписывать целиком индексу |
| Peak working set процесса |59 891 712 Б |60 420 096 Б |**Ухудшение +528 384 Б** |
| Managed allocation за жизнь процесса |50 194 448 Б |50 246 840 Б |**Ухудшение +52 392 Б** |
| Обычные правильные ответы до→после |**11/24→19/24** |**11/24→19/24** |Есть реальное дополнительное усвоение;8 приобретений,0 потерь |
| Ошибочно принятые / честные краевые отказы |3 /2 |3 /2 |Известные ошибки сохранены, новых wrong accepts нет |
| GC0/1/2 в Train и restore |0/0/0 |0/0/0 |Изменения не наблюдались |

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности для этого не обязателен. Все ухудшения показываются рядом.** Поэтому итоговая отрицательная метка старого отчёта по allocations не отменяет наблюдавшееся уменьшение времени обучения. Этот план сохраняет исторические данные и интерпретирует каждую ось отдельно. Он не переименовывает allocation regression в экономию памяти и не объявляет доказанным устойчивое ускорение.

Опыт выполнен один раз в фиксированном порядке **W→A→B**, без повторов и counterbalanced порядка. JIT, scheduling, состояние ОС и крупность CPU-счётчика не отделены от фактора. Исторический runtime — `.NET 9.0.18` по [W receipt](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/IDX01/a001/results/W/receipt.json). Поэтому проценты — наблюдения этой пары, не гарантированный CI threshold или доказательство причинного выигрыша всех профилей. По силе подтверждения time-часть ограниченнее устойчивого семантического исправления или прямого крупного allocation-снижения.

Счётчики имеют разный смысл. Исторический [Program.Mark:435–462](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/IDX01/a001/Program.cs#L435) использует `GC.GetTotalAllocatedBytes(precise:false)` для узких окон. Его process delta может быть меньше thread delta из-за приблизительного счётчика; числа не исправлять вручную. Во время restore managed thread менялся, поэтому его thread allocations записаны `null`, не0. Train оставался на том же потоке. Общая память компьютера, disk cost и GPU не измерены; peak разных процессов не складывать.

## 2. Native изменение: три участка одного файла, обучение всегда выполняется первым

Единственный разрешённый product file — **`27.TranslationDomain/TranslationMachine.cs`**. Его исторический A blob `fd0e736de3739236b4dde2d6b9d000f5ebae8416`, SHA256 `4745fbd2835f4f99993d447e0e98240bdf56cf8ea182dcdbf749947734112d1c`; проверенный native файл в evidence HEAD содержит этот же исходный алгоритм. Source pin и новая фактическая baseline SHA записываются раздельно: новые A/B идут на назначенной актуальной базе, а не молча переключают checkout на исторический commit.

| Участок | Точная правка | Что сохраняется |
|---|---|---|
| Поля класса, после `_structuredTrainingIdentities`, [source:18–25](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationMachine.cs#L18) |Добавить private instance `readonly HashSet<(string ExampleId, string UnitId)> _structuredTrainingIdentityKeys = [];` |Список остаётся authoritative экспортируемым журналом; индекс не static и не новая модель |
| `TrainStructuredBatchAsync`, [source:106–121](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationMachine.cs#L106) |Заменить только `.Any` условием `_structuredTrainingIdentityKeys.Add((example.ExampleId, example.UnitId))` |Оба прежних вызова `trainer.TrainOnExample(example)` и `trainedUnits.Add(example.UnitId)` остаются **перед** этим условием; held-out/unknown-unit guards прежние |
| `ImportCheckpoint`, [source:375–381](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationMachine.cs#L375) |Одновременно с list.Clear очищать index; в прежнем foreach добавлять запись в список и пару в index |Полная preflight-валидация [330–334](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationMachine.cs#L330) остаётся **до любых мутаций**; null legacy identities дают пустые обе структуры |

Точный новый фрагмент учебного цикла:

~~~csharp
trainer.TrainOnExample(example);
trainedUnits.Add(example.UnitId);
if (_structuredTrainingIdentityKeys.Add((example.ExampleId, example.UnitId)))
    _structuredTrainingIdentities.Add(new TranslationStructuredTrainingIdentityCheckpoint(
        example.ExampleId,
        example.SourceId,
        example.UnitId,
        batch.BatchId));
~~~

Точный новый фрагмент восстановления:

~~~csharp
_structuredTrainingIdentities.Clear();
_structuredTrainingIdentityKeys.Clear();
foreach (var identity in checkpoint.StructuredTrainingIdentities ?? [])
{
    _structuredTrainingIdentities.Add(identity);
    _structuredTrainingIdentityKeys.Add((identity.ExampleId, identity.UnitId));
}
~~~

Нельзя переносить проверку пары до `TrainOnExample` и делать `continue` для знакомого ID: нынешний контракт повторно обучается, но повторно не дописывает identity. Ключ содержит **ровно** `ExampleId` и `UnitId`; `SourceId` и `BatchId` остаются метаданными первой записанной пары. Регистр значим; не добавлять lowercase, alias normalization, сравнение по хешу без полного equality, domain/SourceId в ключ или особое правило повторных ответов. Для нового ключа list и set обновляются в прежнем месте после trainer; для уже известного меняются обучаемая память/history/version согласно текущему learner, а запись identity остаётся первой.

`ImportCheckpoint` не должен дедуплицировать или переписывать старый список. Текущая schema1 допускает несколько записей с одной парой и legacy `StructuredTrainingIdentities=null`; индекс содержит distinct пары, список сохраняет все допустимые записи и metadata. [ExportCheckpoint:296–299](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationMachine.cs#L296) уже сортирует список ordinal по `ExampleId`, затем `UnitId`. Эту сортировку и стабильный порядок дублей не менять; HashSet enumeration никогда не становится порядком экспорта. Сам индекс не сериализуется, schema не меняется.

JSON policy из FIX-JO, executor maps/KNN, tokenizer, verifier, reward telemetry, admission, domain router и native public API здесь не меняются. Если JO уже внедрён, он одинаково присутствует в A и B; запуск FIX-IDX не запускает JO заново. Не добавлять capacity tuning или `EnsureCapacity`, параллелизацию, новый lock, batch grouping или альтернативный comparer: они не входят в три участка измеренного фактора. Идентичность — структурные метаданные обучения; детерминированное обслуживание этого журнала не назначает семантическую authority и не выбирает маршрут prompt.

### Точный путь конструктора и восстановления

Восстановление вызывается через существующий **`TranslationMachine.LoadCheckpointAsync(checkpointPath)`**. На [source:611–650](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationMachine.cs#L611) он читает JSON с `TranslationCheckpointJsonOptions.Standard`, создаёт `new TranslationMachine()` и вызывает `machine.ImportCheckpoint(checkpoint)`. Public default constructor [32–43](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationMachine.cs#L32) делегирует private `TranslationMachine(CreateRuntime())`, получая typed runtime, его shared KNN, executors, tokenizer/verifier и trainer map. Новый индекс создаётся пустым как instance field и заполняется в реальном Import.

Flow-injection constructor `TranslationMachine(TranslationRoutingFlow)` [45–49](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationMachine.cs#L45) не содержит `_runtime` и не подходит для checkpoint-проверки. Scalar `ImportAsync` не восстанавливает обученные units. Ни новый constructor seam, ни friend assembly, ни production accessor индекса не нужны. Проверки private index в§6 выполняет только test helper после измеряемой работы, без мутации поля.

## 3. Математика: инвариант равенства журналов и цена индекса

Пусть `L` — полный список identity records, `key(r)=(r.ExampleId,r.UnitId)`, а `K={key(r):r∈L}`. Исходное условие добавления новой identity равно `key(example)∉K`, поскольку `.Any` сравнивает обе строки ordinal. При сохранённом инварианте новая `HashSet.Add(key)` возвращает true ровно в этом случае. При false старый и новый код оба уже вызвали один и тот же trainer, затем оба сохраняют прежний список; при true оба дописывают одну и ту же запись. Импорт строит `K` из всех записей `L`, в том числе если `L` содержит дубли; null задаёт пустое множество и пустой список. Индукция по последовательности успешных Train/Import показывает одинаковый экспортируемый журнал при этих условиях. Она не даёт права пропускать вызовы обучения или изменять malformed-import поведение.

[Документация ValueTuple.Equals](https://learn.microsoft.com/en-us/dotnet/api/system.valuetuple-2.equals?view=net-9.0) задаёт equality через default comparer каждого компонента; [String.Equals](https://learn.microsoft.com/en-us/dotnet/api/system.string.equals?view=net-9.0) для соответствующего строкового equality описывает ordinal, case-sensitive сравнение. Поэтому tuple из двух строк сохраняет существующее сравнение ключей. Коллизия hash не означает равенство пары: `HashSet` проверяет equality. Runtime hash randomization может менять стоимость между процессами, но не разрешает менять persisted order или semantic results.

Для `N` уже известных identity records и `m` новых уникальных ключей исходный поиск посещает `mN+m(m−1)/2` элементов списка. В зафиксированном main Train `N=512`, `m=4`: **512+513+514+515=2054 посещения записей**. Новый код делает четыре Add; один предварительный restore строит индекс из512 пар. Это **вывод из кода и фактического trace состава**, а не отдельно измеренный аппаратный счётчик. Стоимость строкового hashing/equality зависит также от длины строк; она не нулевая.

При обычной работе hash-table ожидаемая стоимость последовательности становится `O(N+m)` с учётом построения индекса и `O(m)` для последующего batch, вместо `O(mN+m²)` поиска новых ключей; дополнительное хранение — `O(U)` для `U` distinct пар. [HashSet.Add documentation](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1.add?view=net-9.0) отдельно оговаривает `O(n)` при resize. Коллизии, пересоздание/рост таблицы и малый batch не позволяют обещать постоянное время любого отдельного вызова. `Clear` очищает membership, но не обещает вернуть capacity/память ОС.

Эта математика объясняет уменьшение повторного просмотра журнала, **но не доказывает наблюдавшиеся −2,9673 мс и не отменяет allocation regression**. При маленьком журнале, частых restore, длинных ключах или редком продолжении обучения цена набора может превышать выигрыш. Warm128 имеет исторический расход, но отдельного B warm128 сравнения не было; считать его гипотетическую экономию измеренной нельзя. Полезный домен evidence — EN→RU technical memory; структурная эквивалентность применима к тому же API независимо от языка, а стоимость других directions/units не измерена. Синтаксическое дерево, порядок слов, LoRA и нейронный DPO здесь не являются факторами.

`readonly` запрещает заменить ссылку, а не делает set неизменяемым. Существующие list/unit/KNN state изменяются в последовательном Train/Import; новая коллекция не делает их потокобезопасными. Конкурентные Train/Import и новые synchronization semantics не назначены этим планом.

## 4. Готовое обученное «до»: полный checkpoint и точные данные

**Перед запуском ничего не дообучать для подготовки.** Уже существует полный donor [W/warm.checkpoint.json](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/IDX01/a001/results/W/warm.checkpoint.json) в evidence commit. Его подтверждает [W/receipt.json](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/IDX01/a001/results/W/receipt.json).

| Свойство donor | Закреплённое значение |
|---|---|
| Исторический path |`experiments-v2/RP01-MICRO/IDX01/a001/results/W/warm.checkpoint.json` |
| Git blob |`48673d768b117c05cd3bd58e417291d04959c757` |
| SHA256 исходных bytes |`895dfdae27b346264fda655a534e2d013c40a7ac409c73a98a6d11c5cd42bc65` |
| Размер |352 613 Б |
| Native schema/owner |schema1, `translation-machine`, `TranslationDomain`; `HasStructuredTraining=true`, version`0.1.128`, Tatoeba cursor отсутствует |
| Обученная память |history128, identities512; EN_RU exact4, phrase8, word0, surface0; shared KNN1536 |
| Наблюдаемый consumer после restore |11/24 правильных ordinary ответов;14 отвечено,3 wrong accepted,2 safe edge declines |
| Учебная подготовка нового переноса |**0 Train,0 предъявлений** |

Будущий fixture passport — **`Tests/Fixtures/ExperimentFixes/v1/TrainingIdentityIndex01/manifest.json`**. Единственная закреплённая копия donor для native tests — **`Tests/Fixtures/ExperimentFixes/v1/TrainingIdentityIndex01/warm.checkpoint.json`** с теми же bytes/digest. Перенести bytes один раз в common harness commit, записав source commit/blob/receipt; оригинальный опыт не править. Проверка наличия, digest, размера и schema выполняется до native model calls. LFS pointer, неполный JSON, другой donor или несовместимый schema означают `missing_or_incompatible_fixture`, не разрешение искать похожее состояние, вызывать исторический `run.py` или создавать seed заново.

Происхождение донора явно сохранено: [Program.RunWarm:132–227](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/IDX01/a001/Program.cs#L132) взял первые4 P125 урока,128 раз предъявил их четырьмя строками с новыми ID `IDX01-W{4*g+j:D4}::{originalExampleId}`, batch IDs `IDENTITY-TRAIN-INDEX-01-warm-{g:D3}`. Это512 реальных предъявлений и1536 KNN entries, но только4 разных учебных содержания. Эти расходы уже понесены; они не новый setup budget и не доказательство512 независимых языковых примеров. Не дедуплицировать донорские KNN записи или identities ради меньшего файла.

Полный checkpoint содержит KNN, все executors, semantic tokenizer, verifier, history/version/flags и identities. Сохранять/восстанавливать только новый set вместо него нельзя. Нейронные weights, optimizer, LoRA, BPE fit и RNG schedule в этом механизме `not_applicable`; новая модель-учитель не вызывается.

Общие данные берутся из того же fixture, который назначен19 и46: **`Tests/Fixtures/ExperimentFixes/v1/TranslationMemory/`**. Исходный P125 root — `docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/`; bytes закреплены [IDX manifest](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/IDX01/a001/results/manifest.json). Не создавать второй gold или отличающийся scorer под IDX.

| Общий файл | Source Git blob | SHA256 |
|---|---|---|
| `training-batch-B.json` |`0c2a3d9a0da31b7277c0d215fb79ddebda49aa6c` |`2bf441780a1c97af03c64541eaea46cd8882764f88e2e4a143a2b4ed9b7a4e44` |
| `eval-inputs.json` |`585bf30cad84b57b2e1f3fe8c00f171f2348c313` |`0a44c8ecdb5fb2a1ad8d83fbe4da21dc5d2f54bb1ef270f2586fc96b5af9f880` |
| `panel-gold.json` |`af73d89cfd5b42f954380437bc5596b46dae9498` |`cc6b122dc8aa42fad9e30cd90a277dbcfb48926213e028a326d55e1648a36614` |
| Исходный P125 `score.cjs` |`e8b20d471667da397e8d5c59ac63b4e7f947540b` |`9d091163a9d528a25355ed6e260b669ec9ab6719d8c1a81ca999eaf7a3c2d1a6` |

Для измеряемого продолжения обучения взять **`frozen.Examples.Skip(4).Take(4)`**, сохранив исходные IDs, поля, порядок, `SourceIds`, `DomainTag`, `UnitId=executor-en_ru`, `HeldOut=false`; новый batch ID ровно **`IDENTITY-TRAIN-INDEX-01-new04`**, как в [RunArm](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/IDX01/a001/Program.cs#L229). Это уроки upload the image, download the package, save the draft, archive the log. `BuildGeneralBatch`, другие направления/темы, отрицательные учебные пары или поправленные targets не подставлять.

EXAM — ровно24 исходных запросов EN→RU, topic`technical`: exact`x01–x08`, near`n01–n03`, composition`ev01–ev08`, edge`cc01–cc05`. Группы и ожидаемые before→after: exact4/8→8/8; near3/3→3/3; composition4/8→8/8; edge0/5→0/5. Приобретения ровно`x05–x08,ev05–ev08`; потери0, новые wrong accepts0. `cc01/cc03/cc04` остаются ошибочно принятыми, `cc02/cc05` — честными отказами. Это одна тема перевода с разными проверками, а не смешение предметных тем. Композиции собраны из знакомых частей; роста generalization на новых source families этот набор не доказывает.

Общий будущий **`Tests/ExperimentFixes/TranslationMemoryPanelScorer.cs`** переносит frozen P125 `normalize/assess` из19/46; проверить его на уже сохранённых IDX raw/scores с0 новых Train/Flow. Scorer читает gold только после завершённых96 native outputs пары; learner/requests helper gold не получают. Переход списка arms исторического scorer к A/B не меняет predicates/accepted phrases. `LearningBatchResult.AccuracyBefore/After` здесь отражают текущую служебную coverage-логику [MeasureUnitCoverage:674–684](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationMachine.cs#L674); они не заменяют11/24→19/24 и не становятся evidence0→0.

## 5. Один native основной Fact на плечо: restore →4 новых урока →сохранённый after

Новые запланированные файлы: **`Tests/ExperimentFixes/TrainingIdentityIndexTransferTests.cs`**, **`Tests/ExperimentFixes/TrainingIdentityIndexFixture.cs`**. Namespace — `LlmFromScratch.Tests.ExperimentFixes`; основной класс `TrainingIdentityIndexTransferTests`, один Fact **`Restored512Identities_FourNewLessons_PreserveLearningAndMeasureWriterCost`**. Это будущие test API, не уже созданный harness.

Использовать существующий native ProjectReference Tests→project27. Исторический IDX host компилировал9 копий product sources и исключал исходный `27.TranslationDomain.dll`; это историческое evidence, **не будущая native сборка**. До загрузки/обучения проверить `typeof(TranslationMachine).Assembly`, `typeof(ExecutorUnit).Assembly`: AssemblyName из evaluation native csproj, фактические Location/SHA/MVID и соответствие stage build. Копированный `a001/A/src` или экспериментальная DLL не принимаются как native A/B.

Каждый основной Fact выполняется один раз в отдельном OS test process. Последовательность одинакова A/B:

1. Проверить hashes fixture/donor, exactly8 исходных строк/4 selected lessons/24 EXAM и ownership. Подготовить DTO и имя batch до cost окна, gold не открывать. Проверить passport: schema1, полный donor, original digest. **Никакого Warm, пустого Train, seed fit, loop128 или построения состояния по урокам**.
2. Измерить отдельно один `TranslationMachine.LoadCheckpointAsync(fixedDonorPath)`. Дождаться возвращённого Task; сохранить restore cost, thread IDs и actual outcome. При смене managed thread thread allocation=`null`. В B этот load включает реальную цену построения set из512 пар; её нельзя исключить из расходов.
3. На возвращённой машине сохранить snapshot `restored`, затем выполнить24 ordinary before через `GetOrchestratorAsync().RunAsync`. Request использует ровно frozen source и metadata `source_language=en`, `target_language=ru`, `topic_domain=technical`, scenario`TranslationRoutingFlow.ScenarioId`. Сохранить все raw outputs и snapshot `after_before_eval`. Ожидание —11/24, установит последующий scorer; не выполнять extra readiness query.
4. В отдельном узком окне выполнить **один** `TrainStructuredBatchAsync(new04)` с4 назначенными уроками. Результат должен быть success, rows4, promoted units ровно`[executor-en_ru]`. Cost helper портирует исходный Mark/Delta: Stopwatch timestamp, Process.TotalProcessorTime, thread/process allocation, GC0/1/2 и thread IDs. Приблизительный `GetTotalAllocatedBytes(false)` в узком окне сохраняет контракт источника; process-lifetime endpoint отдельно записывается с`true`. Все IO, JSON snapshots, DTO construction, reflection, scoring и report находятся вне окна. Никаких forced GC, warmup Train или повторов.
5. Текущий метод возвращает завершённый Task; helper проверяет `pending.IsCompleted`, затем завершает его один раз. Если async/thread контракт изменился, сначала штатно дождаться **этого** Task, сохранить actual расход и отметить `measurement_contract_changed`; thread delta при смене потока=`null`. Не запускать второй fit и не выдавать ошибочную thread metric за подтверждение. Сохранить `training.json` и snapshot `after_train`.
6. На той же обученной машине выполнить24 ordinary after и сохранить48 native outputs. Последнее состояние сохранить **один раз** существующим `SaveCheckpointAsync` в `state/after_after_eval.checkpoint.json`; этот метод внутри вызывает ExportCheckpoint. Прочитать созданный JSON как четвёртый snapshot, без нового Train/Flow/Load. Остальные три snapshots берутся native ExportCheckpoint и записываются helper с native checkpoint options вне окон. Save не входит в writer/restore time; его стоимость и failure учитываются отдельно в overall receipt.
7. Все assertions состояния/чужих owners/read-only inference используют эти4 snapshots и48 outputs. B-only private-index inspection после измерений сравнивает set с distinct tuple keys экспортированного журнала; не выполнять дополнительный learner ради проверки set. A helper не требует отсутствующее private поле и компилируется с исходным native API. Сохранить `receipt.json`, native identity, actual counters, time/allocations и terminal status даже при падении assertion.
8. A завершает raw часть с `A_RAW_COMPLETE_PAIR_QUALITY_PENDING`; не объявляет ещё вычисленную по gold точность. B после сохранения всех своих outputs читает completed A, затем чистый общий scorer оценивает96 outputs и пишет `pair-quality.json`: acquisition11→19 в каждой руке,8 gain/0loss, одинаковые ответы и известные3 wrong accepts. Это0 Train/Flow. Неуспех этих assertions завершает stage ошибкой. Если B не выполнен, quality пары=`not_evaluated`, исторические19/24 не подставляются вместо нового измерения.

### Полная сохранность состояния и read-only consumer

Канонизация совпадает с [исходным Snapshot:369–400](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/IDX01/a001/Program.cs#L369): в отдельной копии `ExportedAtUtc→UnixEpoch`, `TrainingHistory[].CpuMs/PeakRamMb/ElapsedMs→0`. Только эти4 вида полей исключаются. Raw bytes/SHA/размер сохраняются. Нельзя исключать version, identities, KNN/order, accuracy telemetry, flags, SourceId/BatchId, maps, tokenizer/verifier или Tatoeba. Полный state equality A/B проверять после native exports; старый raw digest checkpoint не сравнивать с новым export timestamp как модельную регрессию.

Для каждой руки: `restored=after_before_eval`, `after_train=after_after_eval` канонически; `before.memory!=after_train.memory`; semantic tokenizer, verifier и все executor directions кроме EN_RU полностью равны до/после. Между руками все четыре состояния и48 пар outputs семантически равны. Source исторический restored canonical SHA — `29c24352d48da18a1cbb5aaedb9b5a06eeb310a8f68b3b936e302eda207e9a7f`, after — `a3a5df192ddcb4b5737c82e3f69d03a9d03a55c212c1a5a0ee5fcd3afd030d42`; они относятся к указанной исходной сериализации. Для проверки этих hashes повторить тот же `JsonSerializerDefaults.Web`+enum-конвертер canonical writer, не путать с snake_case disk serializer. Поля JSON дополнительно сравниваются структурно: изменение формата не скрывает модельную разницу.

Назначенные main изменения: history128→129; identities512→516; exact4→8; phrase8→16; KNN1536→1548; word/surface0→0; version0.1.128→0.1.129; training flag true→true. Индекс B cardinality512→516 выводится из actual journal; это bookkeeping assertion, а не показатель качества. Обычный consumer действительно читает результат `ExecutorUnit.TrainOnExample`: exact/phrase/KNN через trained adapter, затем verifier. Равенство outputs сопоставляет по case ID/phase `success,errorCode,actual,verdict,selectedFlow,adapterId,metrics,warnings`; confidence не исключать. Request IDs/arm/elapsed — отдельные operational поля. Native training IDs, не выдаваемые consumer, отмечать `not_exposed`, не приписывать правильный provenance по gold.

Корень результата — **`artifacts/fbe/FIX-IDX/attempt-0001/`**; main каталоги **`learning/A/`**, **`learning/B/`**, внутри `restore.json`, `training.json`, `raw-outputs.json`, `state/restored.checkpoint.json`, `state/after_before_eval.checkpoint.json`, `state/after_train.checkpoint.json`, `state/after_after_eval.checkpoint.json`, `state-receipts.json`, `provenance.json`, `receipt.json`, `status.json`. Готовый native donor для будущих read-only consumers — **`learning/B/state/after_after_eval.checkpoint.json`**, его паспорт — **`learning/B/provenance.json`**. В паспорте указать полный lineage W, source/actual A/B commits, native DLL hashes, data/source blobs, schema/counts/direction/topic, raw/canonical hashes,24-case result и3 known errors, exact exclusions и все costs.

После основного fit все lifecycle/assertions/report читают эти сохранённые результаты; downstream API — тот же `LoadCheckpointAsync`, **0 нового обучения**, если отдельно назначенный тест не проверяет само повторное предъявление§6. Нельзя заменить исходный W готовым JO/P125 состоянием19/24: пропадёт проверяемое приобретение последних4 уроков и изменится размер истории. TEST-TMEM19 может позднее использовать готовый native after по паспорту без повторного acquisition, но его исторический P25 cold gate не подменяется молча и здесь не запускается. Всего дополнительных вызовов19 и20 по этой карточке —0.

## 6. Побочные эффекты и существующие тесты: проверять только конкретный ремонт

Новый файл — **`Tests/ExperimentFixes/TrainingIdentityIndexContractTests.cs`**, класс того же имени, namespace`LlmFromScratch.Tests.ExperimentFixes`, один Fact **`RestoredAfterState_DuplicateLessonStillTrains_AndImportRebuildsKeys`**. Он выполняется в отдельном процессе **после** main learning своего плеча. Вход — полный сохранённый `learning/<arm>/state/after_after_eval.checkpoint.json` и его completed receipt. Не пересоздавать W и не повторять4-урочный fit.

Назначенная последовательность одного contract Fact:

1. Один native `LoadCheckpointAsync` этого after; проверить фактический digest/passport и counts129/516/1548, exact8, phrase16. Взять пятую исходную строку общего TRAIN (`Examples[4]`) с её уже записанной парой. Сделать только metadata-копию `SourceId="FIX-IDX-contract-source"`; вход/target/signatures/maps/UnitId/HeldOut неизменны. Batch ID **`FIX-IDX-duplicate-existing`**, DomainTag прежний, SourceIds согласованы с этой metadata-копией. Один `TrainStructuredBatchAsync`, **1 урок**. Это намеренный regression probe повторного обучения, не новый экспериментальный quality claim.
2. Проверить на экспортированном результате: history129→130, version0.1.129→0.1.130, KNN1548→1551, identities516→516, exact8/phrase16 без изменения содержимого; прежняя identity сохраняет исходный SourceId/BatchId, несмотря на новое предъявление. Три добавленные KNN записи соответствуют существующим трём `AddPair` в [ExecutorUnit.TrainOnExample:417–423](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationTrainedUnits.cs#L417); [TranslationKnnStore.AddPair:207–216](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationUnitContracts.cs#L207) безусловно добавляет запись в bucket и не дедуплицирует повтор. Это прямо ловит ошибочное размещение index gate до trainer. Никакого дополнительного Flow или другого урока для этого assertion.
3. От сохранённого полного after-duplicate record построить malformed копию, где у первой identity `SourceId=null!`; остальные поля неизменны. Вызвать один `ImportCheckpoint`, ожидать `CHECKPOINT_STRUCTURE_INVALID` по native validation[500–511](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/27.TranslationDomain/TranslationMachine.cs#L500). Полный canonical state должен остаться тем же. В B сохранить и сравнить также полный набор private keys до/после: отказ не очищает индекс.
4. Импортировать valid legacy копию того же record с `StructuredTrainingIdentities=null`. Ожидать success и пустой экспортируемый journal; в B set тоже пустой. Не требовать уменьшения allocated capacity после Clear. Это проверка очистки при повторном импорте уже использованной машины, не только fresh constructor.
5. Импортировать valid contract копию с4 заранее заданными identity records: R1=`(ExampleId="FIX-IDX-Key",UnitId=executor-en_ru,SourceId="FIX-IDX-source-1",BatchId="FIX-IDX-contract-import")`; R2 имеет ту же пару и SourceId`FIX-IDX-source-2`, BatchId`FIX-IDX-contract-import-2`; R3 копирует R1 с ExampleId`fix-idx-key`; R4 копирует R1 с UnitId`executor-ru_en`. Остальной полный state берётся из after-duplicate. Ожидать список4 записей с прежней экспортной ordinal сортировкой и сохранёнными metadata обоих дублей. В B index содержит **3**, а не2 или4 distinct пары. Это synthetic import-contract metadata; они не являются утверждением об обучении RU→EN и не становятся новым donor для качества. Здесь0 Train/Flow.

Reflection helper только B после measured work получает существующий в candidate private `_structuredTrainingIdentityKeys` и материализует read-only копию для assertions; он не меняет поля. В A тот же compiled helper не обращается к ещё отсутствующему field. Сравнение экспортируемого состояния и duplicate-learning assertions одинаковы A/B. Общий harness не ссылается compile-time на будущий product member; нового production accessor или friend declaration нет. Все records контрактов используют уже существующие public checkpoint record types. Индекс проверяется как производная структура, а журнал и обычные результаты остаются evidence обучения.

### Один точный existing guard на плечо

Назначен ровно существующий **`LlmFromScratch.Tests.TranslationDomainTests.ImportCheckpoint_NullNestedCollectionFromSchema1Json_FailsBeforeMutation`**, [Tests source:3889–3905](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/Tests/TranslationDomainTests.cs#L3889). Он создаёт обученную R19 машину через `BuildR19LearnedMachineAsync` [3974–4004](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/Tests/TranslationDomainTests.cs#L3974), портит nested `knn_store.entries`, затем проверяет отказ до мутаций и сохранение обычного выученного ответа через `AssertRejectedCheckpointDoesNotMutateAsync` [4010–4030](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/Tests/TranslationDomainTests.cs#L4010). **Один запуск =1 Train/1 урок/3 ordinary Flow/1 отклонённый direct Import**, без Load/Save. Два stage вместе добавляют2 Train/2 урока/6 Flow. Его небольшой отдельный dataset не смешивается с P125 и не считается новым измеренным улучшением.

Нынешняя документная работа не установила свежих падений Tests. Ремонтировать будущие failures адресно:

| Симптом после product diff | Конкретный ремонт | Чего не менять ради зелёного результата |
|---|---|---|
| Main повторяет11→11 или теряет previously learned ответы |Сначала donor/dataset/native DLL binding, затем порядок `TrainOnExample` перед index Add в `TranslationMachine` |Не добавлять уроки, не менять gold и не объявлять coverage telemetry качеством |
| Duplicate probe не добавляет3 KNN entries |Вернуть безусловный trainer call перед дедупликацией journal |Не обновлять expected на отсутствие обучения |
| Повторный/legacy Import оставляет stale keys |Очищать set рядом со списком и заполнять его из нового journal после successful preflight |Не требовать от пользователя нового checkpoint и не удалять старые результаты |
| Malformed Import меняет состояние/индекс |Вернуть весь structure validation до очистки и nested mutations |Не ослаблять `CHECKPOINT_STRUCTURE_INVALID` или existing ordinary-answer assertions |
| Case/unit control объединяет разные пары |Вернуть точную двухстрочную ordinal tuple semantics |Не lowercase ID и не добавлять SourceId в ключ |
| Экспорт теряет старые дубли/metadata |Сохранить list как ранее, distinct только в index |Не переписать schema или сериализовать HashSet вместо списка |
| Raw/scorer/report отсутствует после состоявшегося fit |Дописать reader/formatter по уже сохранённым raw и state с явной report revision |Не повторять Train/EXAM для отчёта |

Другие existing checkpoint/continued-training/reward/source tests сохранить без удаления/Skip/xfail. Весь `TranslationDomainTests` и solution автоматически не запускать. Если нужен build prerequisite [02§10](02-learning-state-and-fixtures.md#native-build-prerequisite) для случайно включённых в35 архивных исходников, он одинаков в A/B common harness и не исключает native project27 или назначенные assertions.

## 7. Канонический A/B запуск и точный конечный бюджет

Наследуется [03§9](03-test-and-execution-contract.md#native-ab-transfer), с отдельной ветвью **`PlanId=FIX-IDX`** будущих `Tests.Support/ExperimentTransfer/Run-NativeAB.ps1` и `Compare-NativeAB.ps1`. Эти runners, fixtures и tests **планируемые**, нынешняя публикация их не создаёт и не запускает. Единственный SourceRoot **`C:/Projects/llm_mvp_core`**, branch **`codex/dpo-implementation`**. Один checkout, без worktree, sibling source copy, branch switch или reset/checkout старого A после правки.

Сначала common harness/fixtures/scorer commit при прежнем product factor → сохранить `$baselineCommit` → выполнить все A stages. Затем в том же checkout вне runner внести ровно3 участка `TranslationMachine.cs`, review/commit → сохранить `$candidateCommit` → выполнить B. Runner проверяет root/HEAD/branch/ancestry, unchanged test/helper/fixture bytes, donor SHA, runtime и single-product-file allowlist. Принятые ранее изменения, включая FIX-JO, перечисляются в baseline passport и одинаковы A/B. `XIIGEN_FBE_ARM` определяет только provenance/output и B-only assertions, не product implementation.

| Процесс | Exact test | Train / предъявления | Ordinary Flow | Load / direct Import |
|---|---|---:|---:|---:|
| A existing |Один existing guard§6 |1 /1 |3 |0 /1 rejected |
| A learning |Один main Fact§5 |1 /4 |48 |1 /0 |
| A contracts |Один contract Fact§6, input=готовый after A |1 /1 |0 |1 /3:2valid+1rejected |
| B existing |Тот же existing guard |1 /1 |3 |0 /1 rejected |
| B learning |Тот же main Fact, input=исходный W |1 /4 |48 |1 /0 |
| B contracts |Тот же contract Fact, input=готовый after B |1 /1 |0 |1 /3:2valid+1rejected |
| Compare/scorer/report |Только чтение сохранённого |0 /0 |0 |0 /0 |
| **Общий максимум** |**6 test processes;2 Rebuild;2 discovery** |**6 /12** |**102** |**4 Load;8 direct Import** |

Из8 direct Import четыре valid и четыре rejected; каждый из4 native Load дополнительно внутри вызывает свой Import, это не ещё4 загрузки. **Основное измеряемое обучение отдельно:2 Train/8 уроков/96 outputs/2 Load.** Контрактные duplicate probes добавляют2 Train/2 предъявления/2 Load, existing guard —2 Train/2 урока/6 Flow. Native `SaveCheckpointAsync` вызывается2 раза, по одному main after на плечо. Нет neural updates, teacher/generate calls, warm128 preparation, C-arm, второй cold readout панели, повтор19/JO/N60, stochastic seed-search или автоматического repeats.

Порядок: **A Rebuild → discovery3 → existingA → learningA → contractsA; product diff/review/commit; B Rebuild → discovery3 → existingB → learningB → contractsB → Compare**. Контракты требуют собственного completed learning и выполняются после него. В каждом stage discovery должна обнаружить ровно3 названных Fact, не0 и не весь класс. Все test invocations после stage Rebuild используют `-c Release --no-build`; каждый из6 процессов имеет свой PID/TRX/stdout/terminal receipt. Main learning process свежий и не разделяет runtime с existing/contract fit.

Точные filters:

~~~text
FullyQualifiedName=LlmFromScratch.Tests.TranslationDomainTests.ImportCheckpoint_NullNestedCollectionFromSchema1Json_FailsBeforeMutation
FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.TrainingIdentityIndexTransferTests.Restored512Identities_FourNewLessons_PreserveLearningAndMeasureWriterCost
FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.TrainingIdentityIndexContractTests.RestoredAfterState_DuplicateLessonStillTrains_AndImportRebuildsKeys
~~~

Discovery объединяет эти3 FQN оператором`|`; исполнение идёт тремя отдельными exact filters. Runner задаёт `XIIGEN_FBE_SOURCE_ROOT`, `XIIGEN_FBE_OUTPUT`, `XIIGEN_FBE_ARM`, `XIIGEN_FBE_PAIR_ROOT`; outputs `existing/A|B`, `learning/A|B`, `contracts/A|B`. У contract неизменяемый input=passport+after checkpoint собственного learning stage; он не выбирает «последний успешный файл» по времени.

Форма будущих команд после реализации общего harness и отдельного назначения переноса:

~~~powershell
Set-Location C:/Projects/llm_mvp_core
$baselineCommit = (git rev-parse HEAD).Trim()
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-IDX -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit
~~~

После terminal A, единственного product diff, review и commit:

~~~powershell
$candidateCommit = (git rev-parse HEAD).Trim()
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-IDX -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit -CandidateCommit $candidateCommit
~~~

Baseline не принимает CandidateCommit. Candidate читает immutable intent/baseline-stage и создаёт отдельный candidate-binding; старые A receipts не переписываются и A не повторяется. Каждый substantive process имеет CreateNew ownership; existing completed attempt читается, partial actual budget сохраняется. Каталоги не удалять и новый suffix автоматически не выбирать. Deadline5 минут/test process,15 минут/Rebuild — watchdog, не разрешение на дополнительные уроки. При ошибке finally сохраняет completed/not_reached counters, raw/snapshots и terminal/failure receipt до возврата nonzero; отсутствующее «после» не записывается как измеренная0accuracy.

Бюджет не зависит от128 исторических warm calls: ни одна будущая команда не вызывает `RunWarm`, `prepare.py`/`run.py` исходного IDX host или builder, способный незаметно восстановить donor повторным обучением. При no-donor fail остаётся конкретный неисполненный input; данные предыдущих экспериментов не удаляются. Контрактный один duplicate lesson — явно назначенный side-effect test, а не скрытый fit для очередного assertion; все последующие проверки этого экземпляра читают готовый state.

## 8. Приёмка, связь с prompt→domain E2E и отчёт

Отдельно показать **валидность исполнения**, **учебный результат**, **семантическую сохранность**, **полезные ресурсные оси с их ограничениями**. Валидный native перенос требует настоящие4 урока после конкретного trained-before,11/24→19/24 в обоих плечах, expected memory change, parity и terminal receipts с budget/native provenance. Нулевое обучение после не заменяется количеством ключей/margin или ускорением pure-set benchmark. Если актуальный A не воспроизводит закреплённый slice, оформить точный source/donor drift без добавления уроков и подмены gold.

Для семантически прозрачной оптимизации ожидаются A/B equality полного canonical state/outputs и сохранённая duplicate/import semantics. Улучшение любой измеренной полезной cost-оси считается успехом именно этой оси; historical Train wall/CPU — положительное основание этой карточки, несмотря на RAM regression. Новый одиночный native замер также не устанавливает причинность; time numbers не становятся нестабильным обязательным CI процентом. `code_correctness_pass` и `observed_axis_gain` — разные поля результата. Если новая native пара не показывает полезного cost-изменения, это честно фиксируется без обнуления исторического наблюдения или запуска дополнительных попыток. Product rollout/массовые gates не следуют автоматически из кандидата; решение по измеренным выигрышам и потерям принимает пользователь.

Основной consumer path — `GetOrchestratorAsync().RunAsync → InlineTranslationOrchestrator → TranslationRoutingFlow.ExecuteAsync → TrainedTranslationAdapter → executor/tokenizer/verifier`. Он использует учебный результат, но явные scenario/direction metadata не проверяют learned prompt→domain selection. [20 FIX-E2E](20-prompt-domain-e2e.md) владеет внешней маршрутизацией и собственным RU→EN general receiver; его budget и dataset здесь не расширяются. [49 FIX-KR](49-router-learned-key-normalization.md) исправляет чтение learned router keys, а не identity bookkeeping translation receiver. Новый set никогда не становится semantic router. Для будущего отдельно назначенного E2E receiver уже есть точный full native after path§5; нельзя подключить его как RU→EN general по одному слову «перевод». Здесь сохраняется EN→RU technical contract и0 новых routed E2E calls.

Будущие итоговые файлы: **`artifacts/fbe/FIX-IDX/attempt-0001/comparison.json`**, **`report.md`**, **`report-for-user.md` на русском**, **`status.json`**, ссылки на six stage receipts и native model passports. Отчёт указывает: factor/точные code sites → source donor и0 prepTrain → действительные before/after по24case → все8 приобретений/ошибки/отказы → canonical state и unchanged owners → Train/restore/whole-phase wall,CPU,allocations,GC,peak рядом → single-pair causality limits → фактический budget и дефекты → кандидат ремонта. Historical source-copy run и будущий native run разделены. Старый отчёт, карточка и результаты не удаляются и не перезаписываются; новая интерпретация полезных time-осей оформляется этой ссылкой.

## 9. Точный промпт будущему исполнителю

> Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/52-translation-training-identity-index.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.
>
> Выполни только отдельно назначенный **FIX-IDX** по этой карточке и03§9. Модель исполнителя — фактическая default текущей сессии, без самовольной смены model/reasoning. Обучаемый механизм — native TranslationMachine memory EN→RU technical; neural/LoRA/RNG/teacher конфигурация неприменима. Исторические данные заморожены; новые DPO/внешний judge не назначены. Роли CODEX выбора/проверки DPO сохраняют пользовательскую настройку `astra 6 ultracode`, но не создают здесь новые пары.
>
> Прочитай текущие AGENTS.md/CLAUDE.md, эту карточку,02/03 и строку **FIX-IDX** реестра. Skills относительно `docs/skills/`: `architect-behavior-classifier-SKILL.md`, `trainable-architecture-decision-SKILL.md`, `intent-to-plan-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `generated-code-review-SKILL.md`, `test-integrity-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-communication-GLOSSARY.md`, `human-readable-architecture-audit-SKILL.md`. Общение и оба отчёта — по-русски, exact paths§8. Host, запуск, модель, язык, donor, dataset, scorer и budget уже назначены; не ищи альтернативы.
>
> Сначала в canonical C:/Projects/llm_mvp_core подготовь baseline-safe harness и точные fixture bytes. Используй полный опубликованный warm.checkpoint.json с SHA256895dfdae27b346264fda655a534e2d013c40a7ac409c73a98a6d11c5cd42bc65. Ни128 warmTrain, ни повтор JO/19 не выполняй. Основная пара:2 Load W,2 Train по4 новым урокам,96 ordinary outputs; сохранить полный after каждого плеча. Контракты читают этот after и добавляют ровно по1 duplicate lesson/руку; existing guard добавляет ещё по1 маленькому lesson/руку. Весь назначенный максимум:6Train/12предъявлений/102Flow/4Load,8direct Import,2native Save,6test processes,2Rebuild+2discovery. После этих явно назначенных writer steps все assertions/report используют snapshots/raw, без повторного fit.
>
> Product factor только3 участка TranslationMachine.cs: instance HashSet пары, Add **после** настоящего trainer, Clear/rebuild при validated Import. WRONG: пропустить обучение знакомой пары, сменить equality/SourceId-key, экспортировать set вместо journal, убрать дубли старого checkpoint, измерять только set вне обучения, переобучить donor из-за отсутствующего файла, исправлять verifier/JSON policy в этой же паре. CORRECT: прежний learner/identity semantics, полное восстановление,11→19 ordinary answers и полные расходы индекса/restore.
>
> Любое измеренное улучшение полезной оси — успех этой оси и профиля; accuracy обязана подтвердить ненулевое обучение, но не обязана вырасти между A и B. Не отвергай Train wall/CPU только из-за выросших allocations; покажи оба результата рядом. Один fixed-order замер не доказывает устойчивую причинную экономию. Ремонт loader/test/report разрешён только по конкретным путям, связанным с назначенным обучением§5–7; не запускать соседние ID автоматически.
>
> Один запуск — один исходный handle. При exec_command session_id используй write_stdin с пустыми chars; при functions.exec ответе Script running with cell ID — functions.wait с тем же cell_id. Для другого инструмента сначала открой его schema и примени штатный documented wait. Жди порциями≤60секунд в том же ходе до terminal/deadline, сообщая краткий ход. Не завершай раунд словами «жду», не объявляй fail по пустому log/отсутствию раннего файла, не relaunch, не создавай polling loop, второй waiter или дополнительные задачи вместо ожидания. Не выдавай неподтверждённые числа за результат.
>
> **STOP/NEXT имеют немедленный приоритет.** STOP — прекратить действия. NEXT — перейти только к указанному ID или следующему ID актуального реестра; сейчас после FIX-IDX — **FIX-N60**, не старый номер опыта. Запрещено после такого указания продолжать прежнюю карточку, заканчивать её отчёт/tests/commit/cleanup или возвращаться к ней вместо следующей. Сообщи только уже подтверждённое состояние. Старые эксперименты и результаты не удаляй. При отсутствии STOP/NEXT доведи живой назначенный процесс до terminal штатным ожиданием и задокументируй результат по сохранённым данным.
