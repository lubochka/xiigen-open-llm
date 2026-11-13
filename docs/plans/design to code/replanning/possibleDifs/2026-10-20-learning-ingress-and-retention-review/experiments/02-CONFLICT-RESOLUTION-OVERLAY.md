# Адресное разрешение противоречий 24 карточек

**Поздняя проверка HEAD:** на `67abfabe6def8a002984be80b999824601ad273c` уже 130 карточек. P51/P94/P110 upstream сняли 14 blanket-исключений; актуально 21 карточка/60 spans, а 24/74 ниже — исходный обнаруженный срез. Наши 24 адресных решения сохранены с этой атрибуцией; для трёх уже исправленных это подтверждение и конкретизация, не новая наша правка. P113 также смягчён; P86 отменил повторный допуск. N-KU runner и raw теперь committed. Актуальная разница и две новые N62-карточки — [07-LATE-HEAD-DELTA.md](07-LATE-HEAD-DELTA.md).


Снимок: `ce890a48efda7f00d0ee040f2383cc2d06428ea6`. Это **добавочный work-order overlay**, а не изменение защищённой папки и не новая серия из 24 запусков. Он содержит решения для всех 24 card files, где тело ещё содержит 74 самостоятельных blanket `OUT_OF_LEARNING_SCOPE` указания. Часть исключений верна и сохранена; часть сужена до отсутствующего learning link. Исходные доказательства и результаты не удаляются.

## Действующий порядок

Текущие десять правил пользователя имеют приоритет. Для конкретных мест ниже применяется replacement text, даже если старое тело или его краткая строка в 06 повторяет прежний запрет. Общие новые заголовки карточек уже разрешают зависимые участки; этот документ устраняет противоречие адресно. Одинаковое число 0 neural updates не различает отсутствие обучения и чтение реально обученной памяти.

1. Сначала разблокировать действительное обучение; косвенные участки идут после него.
2. Любое измеренное полезное улучшение — успех именно по показателю и домену; accuracy не обязательна, остальные изменения и ухудшения сообщаются рядом.
3. Не запускать самостоятельный опыт на механизме, никак не участвующем в обучении или использовании его результата.
4. Для загрузки данных, восстановления, выдачи обученного результата или управляющей метрики обязательны конкретные writer→state→reader, файл/метод и одна предполагаемая правка.
5. Порядок: учебные блокеры → противоречия карточек → текущие уточнения пользователя → прочие зависимые участки.
6. Проверять одну правку на изолированной копии исходника; до результата не менять production и его тесты, не запускать полную цепочку/CI. Перенос решает пользователь после отчёта.
7. До запуска заполнить точные source closure, host, команду, входы, начальное состояние, counts и новый output; присвоенное имя каталога само по себе не READY. Автор карточки задаёт доступные поля и полный рецепт; исполнитель только материализует их, без нового личного допуска каждого ID.
8. Старые эксперименты, raw, карточки и bin/obj сохранять; новые версии писать рядом, не перезаписывать прежние.
9. Не придумывать обучение для serving-проверки: общий доказанно обученный state допустим; отсутствие reader — OPEN_LEARNING_LINK только этой карточки, не остановка программы.
10. Экспериментальная оболочка может использовать throw/try-catch/void и простые DTO; продуктовые запреты и публикационный workflow не навязывать ей. Не менять исследуемый алгоритм, данные или бюджет под видом такого послабления.

## Неприкосновенность output и старых материалов

Все новые артефакты этого аудита находятся в planning/possibleDifs, а не в защищённой core-папке. Старое указание каждой карточки писать в `docs/plans/replanning-possible-diffs/experimets/results/...` в текущей сессии не применять: пользователь прямо запретил менять эту папку. Никакой текущий run не запускался. Для отдельного будущего опыта источник кода остаётся core read-only; новый конкретный паспорт должен назначить отдельный новый output вне protected до запуска. Это адресное разрешение output-конфликта, не просьба о повторном допуске.

## Поправки по карточкам

### P25.1–.2 — восстановление результата обучения

**Адрес:** [P25-cold-restore.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P25-cold-restore.md); строки старого исключения: 82, 89; затронуты P25.1, P25.2. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L141).

**Вставляемое вместо blanket-правила предписание:** Убрать безусловное исключение cold/export. Сохранность реально выученной способности — допустимый зависимый опыт. A/B получают один checkpoint с доказанными уроками; меняется один loader/export seam. Новые уроки и выдуманный optimizer не нужны. Сравнить canonical learned state и тот же обычный ответ, route и verdict. P25.3–.5 не закрываются этим доказательством.

**Конкретное следующее действие:** Конкретный сохранённый положительный EN→RU контроль: P125 B_trained → LoadCheckpointAsync → GetOrchestratorAsync → RunAsync. Уже имеющийся P25 cold дал parity24/24; не заказывать повтор ради нового PASS. Для нового дефекта менять только потерянное поле или один loader branch в копии, а не переобучать модель.

**Состояние после разрешения:** `PREPARABLE`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Проверяемые code refs:** [27.TranslationDomain/TranslationMachine.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationMachine.cs#L261-L358); [27.TranslationDomain/TranslationMachine.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationMachine.cs#L611-L660).

### P45.1–.2 — порядок при чтении выученной словарной памяти

**Адрес:** [P45-learned-reordering.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P45-learned-reordering.md); строки старого исключения: 82, 89; затронуты P45.1, P45.2. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L161).

**Вставляемое вместо blanket-правила предписание:** Fixed E27 reorder сам по себе не обучение. Однако если он обрабатывает слова, пришедшие из TrainOnExample, его влияние на результат обучения допустимо измерять. Использовать trained word-map и один и тот же semantic input; A — текущая перестановка, B — только заданная в выбранном подслучае политика. Ручные oracle-слова без provenance остаются вне задания. Это узкая переводческая политика; не переносить на C# AST.

**Конкретное следующее действие:** Writer — ExecutorUnit.TrainOnExample; reader — Execute, затем ApplyE27Reorder. Править одну перестановку в экспериментальной копии TranslationTrainedUnits.cs; не объявлять её обучаемым reorder-механизмом. Если реальный запрос возвращается раньше по exact/phrase пути, NO_CONTRAST для этого запроса, не нулевой успех.

**Состояние после разрешения:** `PREPARABLE`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Проверяемые code refs:** [27.TranslationDomain/TranslationTrainedUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationTrainedUnits.cs#L296-L330); [27.TranslationDomain/TranslationTrainedUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L455).

### P50.1–.5 — graph closure и отчётные траектории

**Адрес:** [P50-graph-dependencies.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P50-graph-dependencies.md); строки старого исключения: 82, 89, 96, 103, 110; затронуты P50.1, P50.2, P50.3, P50.4, P50.5. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L166).

**Вставляемое вместо blanket-правила предписание:** P50.4 на сохранённом ряду и P50.5 average/last остаются аналитическими проверками, не учебными запусками. P50.1–.3 получают OPEN_LEARNING_LINK до конкретного native learned graph writer/state/consumer; наличие слов «graph» или «knowledge» эту связь не доказывает. Эта локальная незаполненная зависимость не блокирует другие опыты.

**Конкретное следующее действие:** В текущей P50 указан исторический E49/E50, но ordinary learned owner не установлен. Не создавать новый вымышленный graph learner. Следующее действие — связать конкретный существующий caller с обученным состоянием и назвать правку closure; если caller не существует, оставить это исследование в последней очереди.

**Состояние после разрешения:** `OPEN_LEARNING_LINK`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Code link:** отсутствующий конкретный caller прямо сохранён как незаполненная зависимость; универсальный запрет снимается без выдумывания кода.

### P51.1–.8/.10 — ключ выученного перевода

**Уже исправлено upstream на 67ab:** старый запрет снят; ниже сохранён адресный учебный контракт.

**Адрес:** [P51-translation-context.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P51-translation-context.md); строки старого исключения: 82, 89, 96, 103, 110, 117, 124, 131, 145; затронуты P51.1, P51.2, P51.3, P51.4, P51.5, P51.6, P51.7, P51.8, P51.10. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L167).

**Вставляемое вместо blanket-правила предписание:** Frozen query normalization может изменять доступность уже выученного урока и потому допустима. Зафиксировать один фактор topic/language/case/NFC/operator, одинаковые lessons и checkpoint. При изменении key representation обеспечить парность writer и reader; иначе опыт проверяет несовместимость миграции. Не объединять девять различных причин и не считать EN→RU доказательством HE/ES.

**Конкретное следующее действие:** Source keys создаются TrainOnExample/NormalizeMapKey и читаются Execute. Кандидат переноса — один normalizer/key field в копии TranslationTrainedUnits.cs с прежним scope. Зафиксировать actual key/target; golden EXAM не добавлять в maps.

**Состояние после разрешения:** `PREPARABLE`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Проверяемые code refs:** [27.TranslationDomain/TranslationTrainedUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L285); [27.TranslationDomain/TranslationTrainedUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L455); [27.TranslationDomain/TranslationTrainedUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationTrainedUnits.cs#L1008-L1028).

### P53.3 — readiness после no-op

**Адрес:** [P53-batch-atomicity.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P53-batch-atomicity.md); строки старого исключения: 96; затронуты P53.3. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L169).

**Вставляемое вместо blanket-правила предписание:** Флаг readiness без consumer остаётся отчётной проверкой. Если флаг определяет, будет ли выполнен настоящий Train, выбран checkpoint или использован обученный Unit, это зависимый от обучения участок; тогда измерять пропущенные/лишние updates или ответ. Пока конкретный consumer не указан, P53.3 — OPEN_LEARNING_LINK. Остальные реальные atomic-batch подслучаи не блокируются.

**Конкретное следующее действие:** TrainStructuredBatchAsync и счёт RowsTrained — реальный writer; требуется адрес чтения именно проверяемого readiness/promotion флага. Чинить безусловное повышение статуса только после нахождения этого reader; не выводить качество из счётчика.

**Состояние после разрешения:** `OPEN_LEARNING_LINK`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Проверяемые code refs:** [27.TranslationDomain/TranslationMachine.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationMachine.cs#L89-L153).

### P54.1–.7 — композиция выученных phrase/word mappings

**Адрес:** [P54-phrase-composition.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md); строки старого исключения: 82, 89, 96, 103, 110, 117, 124; затронуты P54.1, P54.2, P54.3, P54.4, P54.5, P54.6, P54.7. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L170).

**Вставляемое вместо blanket-правила предписание:** Отменить запрет только на том основании, что maps уже обучены. TrainOnExample записывает mappings, Execute читает их; это результат обучения. На одном сохранённом trained state менять ровно один composer stage. Запретить подсказки из EXAM и совместное изменение provenance, покрытия и порядка. При выводах отдельно считать полезную coverage и ложные принятые переводы.

**Конкретное следующее действие:** Ремонт имеет адрес ExecutorUnit.TryComposeFromPhraseMappings (либо отдельный выбранный word stage), источник памяти — TrainOnExample. Existing ingress11/24→19/24 и false accepts1→3 служат исторической опорой, не доказательством всех семи поправок.

**Состояние после разрешения:** `PREPARABLE`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Проверяемые code refs:** [27.TranslationDomain/TranslationTrainedUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L277); [27.TranslationDomain/TranslationTrainedUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L455); [27.TranslationDomain/TranslationTrainedUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationTrainedUnits.cs#L798-L857).

### P62.1 — изменение guard после дополнительного signature tag

**Адрес:** [P62-signature-monotonicity.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P62-signature-monotonicity.md); строки старого исключения: 82; затронуты P62.1. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L178).

**Вставляемое вместо blanket-правила предписание:** Нулевые updates не исключают влияние на обученный перевод. На неизменном learned store и запросе изменять только правило threshold при дополнительном теге. Сохранить actual signature, candidates и выбранный target. Проверка произвольных строк signature без actual trained entry не доказывает учебный эффект.

**Конкретное следующее действие:** Writer AddPair → FindNearestCascade/IsLowInformationSignature → Executor.TrySelectKnnCandidate. Исправляемый адрес — выбор fullThreshold в FindNearestCascade, один фактор. Учесть уже существующий candidate-conditioned SourceLaw; не исследовать старую версию как текущую.

**Состояние после разрешения:** `PREPARABLE`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Проверяемые code refs:** [27.TranslationDomain/TranslationUnitContracts.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationUnitContracts.cs#L206-L217); [27.TranslationDomain/TranslationUnitContracts.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationUnitContracts.cs#L246-L300); [27.TranslationDomain/TranslationTrainedUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationTrainedUnits.cs#L333-L360).

### P64.1 — каскад останавливается до target compatibility

**Адрес:** [P64-tier-postfilter.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P64-tier-postfilter.md); строки старого исключения: 82; затронуты P64.1. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L180).

**Вставляемое вместо blanket-правила предписание:** Вариант является допустимым reader-опытом на trained store. A — первый непустой tier, B — переход к следующему tier после отсутствия допустимых targets. Одинаковые state, query, k, source law; меняется только точка принятия каскадного решения. Не добавлять новый урок для формального допуска.

**Конкретное следующее действие:** Кандидат ремонта: передать критерий final target eligibility в копию FindNearestCascade либо согласованный iterator tiers; выбрать один вариант до запуска. Trace должен доказать, что reject произошёл в IsTargetCompatibleForDirection после раннего return tier. Нужный допустимый сосед обязан существовать в той же памяти.

**Состояние после разрешения:** `PREPARABLE`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Проверяемые code refs:** [27.TranslationDomain/TranslationUnitContracts.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationUnitContracts.cs#L246-L265); [27.TranslationDomain/TranslationTrainedUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/27.TranslationDomain/TranslationTrainedUnits.cs#L333-L363).

### P69.1–.9 — GraphRAG и ContextText

**Адрес:** [P69-graphrag-context.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P69-graphrag-context.md); строки старого исключения: 82, 89, 96, 103, 110, 117, 124, 131, 138; затронуты P69.1, P69.2, P69.3, P69.4, P69.5, P69.6, P69.7, P69.8, P69.9. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L185).

**Вставляемое вместо blanket-правила предписание:** Frozen Retrieve/packing допустим только при доказанном чтении результата обучения. Standalone формирование ContextText не является обучением. Для каждого P69.n назвать learned corpus writer, retriever и фактический получатель ContextText в Train либо ordinary learned answer. Если последний consumer отсутствует, OPEN_LEARNING_LINK этого подслучая. Не запускать восемь packing calls ради отчёта.

**Конкретное следующее действие:** В активном D-context сам план признаёт ordinary consumer незаполненным. Следующее действие — установить конкретный reader и строку передачи context; затем исправлять один seed/packing/score factor. Нельзя из наличия retrieval API вывести, что модель использует его текст.

**Состояние после разрешения:** `OPEN_LEARNING_LINK`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Code link:** отсутствующий конкретный caller прямо сохранён как незаполненная зависимость; универсальный запрет снимается без выдумывания кода.

### P75.1–.3 — stochastic residual

**Адрес:** [P75-sampler-residual.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P75-sampler-residual.md); строки старого исключения: 82, 89, 96; затронуты P75.1, P75.2, P75.3. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L191).

**Вставляемое вместо blanket-правила предписание:** Допустим зависимый эксперимент trained model → logits → stochastic sampler → собственный ответ. Greedy RP01 обходит данный sampler: для него гипотеза неприменима. Искусственные logits без существующего обученного вызова — компонентный контроль, не самостоятельный эксперимент по заданию. Проверять представимый RNG residual, а не невозможное число.

**Конкретное следующее действие:** Нужно привязать конкретную строку native TextGenerator → stochastic Sampler и обученный checkpoint. До этого OPEN_LEARNING_LINK; соответствующий фикс исправляет residual выбор разрешённого token в sampler-копии, не DPO objective.

**Состояние после разрешения:** `OPEN_LEARNING_LINK`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Code link:** отсутствующий конкретный caller прямо сохранён как незаполненная зависимость; универсальный запрет снимается без выдумывания кода.

### P76.1–.3 — UTF-8 и сохранность learned generation

**Адрес:** [P76-utf8-boundary.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P76-utf8-boundary.md); строки старого исключения: 82, 89, 98; затронуты P76.1, P76.2, P76.3. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L192).

**Вставляемое вместо blanket-правила предписание:** Trained TextGenerator → BPE decode — допустимая цепь использования результата обучения. Если подтверждена, сравнить те же generated token IDs до/после одной decoder правки. Завершение неполного UTF-8 tail и добавление новых модельных tokens — разные факторы. Не удалять literal U+FFFD или повреждение в середине под видом успеха.

**Конкретное следующее действие:** Нужно адресно связать decoder из P76 с фактическим consumer обученного checkpoint. Чинить только boundary обработки incomplete tail; дополнительную генерацию не прятать в decoder. Без связи — OPEN_LEARNING_LINK, не blanket stop.

**Состояние после разрешения:** `OPEN_LEARNING_LINK`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Code link:** отсутствующий конкретный caller прямо сохранён как незаполненная зависимость; универсальный запрет снимается без выдумывания кода.

### P80.1–.2 — знаменатель markers

**Адрес:** [P80-marker-denominator.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P80-marker-denominator.md); строки старого исключения: 82, 89; затронуты P80.1, P80.2. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L196).

**Вставляемое вместо blanket-правила предписание:** Самостоятельный пересчёт старого marker-score остаётся вне текущей учебной программы: ни знания, ни их использование не меняются. Исправить интерпретацию исторического числа в документации, сохранив старый результат. Новые calls не заказывать. Если когда-либо этот scalar начнёт управлять Train, потребуется отдельный source-bound опыт.

**Конкретное следующее действие:** Здесь нет доказанного training/learned-result consumer, который надо чинить. Не приписывать отсутствующую причинную связь. Этим решением снят конфликт между исторической диагностикой и нынешним запретом на бесполезные запуски.

**Состояние после разрешения:** `OUT_OF_LEARNING_SCOPE`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Code link:** отсутствующий конкретный caller прямо сохранён как незаполненная зависимость; универсальный запрет снимается без выдумывания кода.

### P88.1–.4 — T35 oracle constants

**Адрес:** [P88-unit-context-consumption.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P88-unit-context-consumption.md); строки старого исключения: 82, 89, 96, 103; затронуты P88.1, P88.2, P88.3, P88.4. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L204).

**Вставляемое вместо blanket-правила предписание:** Смена контекста перед постоянным oracle-ответом не измеряет обучение и не получает новый запуск. Сохранить T35 как историческое отрицательное свидетельство delivery. Если появится настоящий writer→state→reader, оформлять отдельную карточку с ним; не переименовывать прежние восемь вызовов в learning.

**Конкретное следующее действие:** Конкретный изучаемый старый вариант выдаёт constants и не подтверждает trained recipient. Нет честной source-only правки, превращающей его проверку в уже существующий learner; поэтому остаётся последняя очередь без запуска.

**Состояние после разрешения:** `OUT_OF_LEARNING_SCOPE`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Code link:** отсутствующий конкретный caller прямо сохранён как незаполненная зависимость; универсальный запрет снимается без выдумывания кода.

### P89.1 против P89.2 — replay loss и actual stop

**Адрес:** [P89-density-stop.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P89-density-stop.md); строки старого исключения: 82; затронуты P89.1. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L205).

**Вставляемое вместо blanket-правила предписание:** P89.1 frozen loss replay — только аналитический контроль, без самостоятельного запуска. P89.2 допустим, когда конкретный stop monitor обрывает реальные последующие updates. Сравнить actual update count, response/retention и стоимость при одном threshold/window factor; gradient scale одновременно не менять. Уменьшение времени признаётся полезным показателем, ухудшение ответа показывается рядом.

**Конкретное следующее действие:** Привязать monitor к реальному caller из выбранного P89.2, зафиксировать минимальный бюджет, при котором условия остановки различаются. Двух шагов недостаточно для сравнения окна пяти шагов; общий бюджет не расширять вне этого вопроса.

**Состояние после разрешения:** `PREPARABLE`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Code link:** отсутствующий конкретный caller прямо сохранён как незаполненная зависимость; универсальный запрет снимается без выдумывания кода.

### P94.1–.3 — typed cardinality/fan-out/fan-in

**Уже исправлено upstream на 67ab:** старый запрет снят; native owner остаётся открытой зависимостью.

**Адрес:** [P94-typed-many-results.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P94-typed-many-results.md); строки старого исключения: 82, 89, 96; затронуты P94.1, P94.2, P94.3. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L210).

**Вставляемое вместо blanket-правила предписание:** Typed множество выходов может быть consumer результата обучения; отсутствие новых updates не является запретом. Но вымышленные oracle outputs или абстрактный fan-in не квалифицируются. До установления существующего learned Unit, типа output и следующего reader каждый такой вариант — OPEN_LEARNING_LINK. Число выходов не равно числу доменов или уровней обучения.

**Конкретное следующее действие:** Нужен реальный файл Unit output contract и consumer, который теряет допустимые элементы. Править один projection/fan-in в экспериментальной копии после установления адреса; не создавать новый workflow под вид уже существующего.

**Состояние после разрешения:** `OPEN_LEARNING_LINK`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Code link:** отсутствующий конкретный caller прямо сохранён как незаполненная зависимость; универсальный запрет снимается без выдумывания кода.

### P96.1–.4 — async orchestration

**Адрес:** [P96-async-orchestration.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P96-async-orchestration.md); строки старого исключения: 82, 89, 96, 103; затронуты P96.1, P96.2, P96.3, P96.4. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L212).

**Вставляемое вместо blanket-правила предписание:** Параллелизм/отмена допустимы здесь только если реально влияют на выдачу обученных Units или на выполнение Train. Тогда скорость полезна без роста accuracy, но полнота результатов, quota и поздние side effects измеряются рядом. Generic scheduler benchmark без learned calls остаётся вне программы. Статус каждого неизвестного reader — OPEN_LEARNING_LINK.

**Конкретное следующее действие:** Установить конкретный orchestration caller, learned Unit и места cancellation/aggregation до создания host. Затем выбрать один фактор (parallelism ИЛИ quota ИЛИ cancellation ИЛИ output collection); не запускать сразу все четыре.

**Состояние после разрешения:** `OPEN_LEARNING_LINK`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Code link:** отсутствующий конкретный caller прямо сохранён как незаполненная зависимость; универсальный запрет снимается без выдумывания кода.

### P99.1–.2 — неизвестный token ID

**Адрес:** [P99-unknown-token-id.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P99-unknown-token-id.md); строки старого исключения: 82, 89; затронуты P99.1, P99.2. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L215).

**Вставляемое вместо blanket-правила предписание:** Потеря token в decoding реального trained output — допустимая зависимость; ручной невозможный ID сам по себе не демонстрирует ухудшение обучения. Проверить действующий detailed consumer: если он уже отказывает до потери, это отрицательное свидетельство применимости. Не распространять plain decoder defect на guard-protected генерацию.

**Конкретное следующее действие:** Нужно связать выбранный Decode overload с обученной генерацией. Ремонт — один явный unknown-ID outcome в этом overload, если он действительно используется. Пока caller/occurrence не доказаны, OPEN_LEARNING_LINK.

**Состояние после разрешения:** `OPEN_LEARNING_LINK`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Проверяемые code refs:** [01.Tokenizer/BpeTokenizer.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/01.Tokenizer/BpeTokenizer.cs).

### P100.1–.3 — TopK ties и TopP

**Адрес:** [P100-topk-ties.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P100-topk-ties.md); строки старого исключения: 82, 89, 96; затронуты P100.1, P100.2, P100.3. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L216).

**Вставляемое вместо blanket-правила предписание:** Tie-K и пересечение K/P — разные sampler policies, не автоматически баг обучения. Разрешён один reader-опыт на обученных logits и реальном stochastic generation. Greedy обход — контроль неприменимости. Синтетическая сортировка массива без trained consumer остаётся outside.

**Конкретное следующее действие:** Назвать фактический TextGenerator→sampler overload. Для .1 менять только tie resolution, для .3 только порядок topK/topP; одинаковые RNG и learned checkpoint. До этого OPEN_LEARNING_LINK.

**Состояние после разрешения:** `OPEN_LEARNING_LINK`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Code link:** отсутствующий конкретный caller прямо сохранён как незаполненная зависимость; универсальный запрет снимается без выдумывания кода.

### P103.1–.2 — StackDetector

**Адрес:** [P103-stack-before-normalization.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P103-stack-before-normalization.md); строки старого исключения: 82, 89; затронуты P103.1, P103.2. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L219).

**Вставляемое вместо blanket-правила предписание:** Detector-only marker count остаётся диагностикой. Если StackDetector выбирает обученный adapter/recipient или задаёт TRAIN preprocessing, участок допустим при конкретном caller trace. Нужно показать исходный marker, его потерю, выбранного получателя и фактический результат обучения. Отсутствие такой связи локально OPEN_LEARNING_LINK.

**Конкретное следующее действие:** Чинить порядок detection/normalization только в реально установленном caller, не весь normalizer. Сначала проверить, не находится detection уже до normalization в текущем коде; историческую версию не считать доказанным текущим дефектом.

**Состояние после разрешения:** `OPEN_LEARNING_LINK`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Code link:** отсутствующий конкретный caller прямо сохранён как незаполненная зависимость; универсальный запрет снимается без выдумывания кода.

### P107.1–.4 — aggregation learned neighbors; .5 отдельно

**Адрес:** [P107-lawful-neighbor-aggregation.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P107-lawful-neighbor-aggregation.md); строки старого исключения: 82, 89, 96, 103; затронуты P107.1, P107.2, P107.3, P107.4. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L223).

**Вставляемое вместо blanket-правила предписание:** Frozen selection над доказанно обученным KnnLogicalUnit — допустимое использование результата обучения. Новое приобретение памяти .5 не является обязательной платой за допуск .1–.4. В каждом A/B оставить тот же TRAIN/state и один selection factor; shuffled outputs и law-only — controls, не подмена learner. Не объединять adaptive-k с aggregation.

**Конкретное следующее действие:** Train строит BPE/HNSW/examples; ExecuteAsync читает hits[0]. Ремонт .1 имеет конкретный адрес hits[0] selection: выбрать aggregation только по lawful neighbors и зафиксированному OutputKey. Если output schema не задаёт OutputKey, сначала дописать конкретный контракт, не считать arbitrary ToString семантическим label.

**Состояние после разрешения:** `PREPARABLE`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Проверяемые code refs:** [21.AgentInfrastructure/Logical/KnnLogicalUnit.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L103-L164); [21.AgentInfrastructure/Logical/KnnLogicalUnit.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L201-L240).

### P110.1/.2 kernels и P110.3 actual consumer

**Уже исправлено upstream на 67ab:** reader-only разрешён; конкретный kernel caller ещё нужен.

**Адрес:** [P110-numeric-matrix-cost.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P110-numeric-matrix-cost.md); строки старого исключения: 82, 89; затронуты P110.1, P110.2. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L226).

**Вставляемое вместо blanket-правила предписание:** Изолированный MatVec timing не становится обучением. Для .3 допускается измерять стоимость настоящего update либо обычного потребителя обученного state; рост accuracy не обязателен. Нужен конкретный callsite kernel в learner/consumer и сохранение численной функции. .1/.2 остаются вспомогательными controls внутри такого опыта, не отдельной серией.

**Конкретное следующее действие:** Не существует установленного current caller для универсальной P110-карточки. Выбрать существующий kernel и source closure, после чего менять только scalar/vectorized implementation или один sparsity threshold. Не подменять это исследованием произвольной численной матрицы.

**Состояние после разрешения:** `OPEN_LEARNING_LINK`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Code link:** отсутствующий конкретный caller прямо сохранён как незаполненная зависимость; универсальный запрет снимается без выдумывания кода.

### P115.1–.2 — BM25 cache trained corpus

**Адрес:** [P115-bm25-statistics-cache.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P115-bm25-statistics-cache.md); строки старого исключения: 82, 89; затронуты P115.1, P115.2. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L231).

**Вставляемое вместо blanket-правила предписание:** ComputeBm25LikeScore вызывается при выборе ответа из обученных примеров; поэтому ускорение его повторного corpus scan допустимо измерять на том же learned state. Accuracy не обязана расти; scores/ranking должны быть показаны, возможные ошибки invalidation — отдельно. .1 cache on/off и .2 invalidation — разные факторы.

**Конкретное следующее действие:** Train создаёт examples/index; ExecuteAsync→ScoreCandidates→ComputeBm25LikeScore многократно читает corpus. Кандидат ремонта: вычислять неизменные corpus statistics один раз на generation обученного корпуса, invalidation привязать к его фактическому пересозданию. Не менять формулу BM25 или fusion weights.

**Состояние после разрешения:** `PREPARABLE`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Проверяемые code refs:** [18.CapabilityTraining/LlmIngredientLogicalUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L98-L165); [18.CapabilityTraining/LlmIngredientLogicalUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L169-L273).

### P121.1 — project→trained adapter; .2 отдельно

**Адрес:** [P121-project-adapter-selection.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P121-project-adapter-selection.md); строки старого исключения: 82; затронуты P121.1. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L237).

**Вставляемое вместо blanket-правила предписание:** Выбор уже обученного адаптера может определять результат обучения; frozen adapters не исключают такой опыт. Однако требуется конкретный request→project identity→registry→adapter load→answer trace. Пока он не задан, .1 OPEN_LEARNING_LINK; .2 actual isolated update остаётся отдельным вопросом. Не требовать обновить чужой adapter для допуска selection test.

**Конкретное следующее действие:** Нужно назвать current selector/registry method и один ошибочный key/fallback, затем менять только его в копии. Не выдавать абстрактную проверку project ID за качество модели.

**Состояние после разрешения:** `OPEN_LEARNING_LINK`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Code link:** отсутствующий конкретный caller прямо сохранён как незаполненная зависимость; универсальный запрет снимается без выдумывания кода.

### P122.2 — semantic roots и active-count-only consumer

**Адрес:** [P122-root-channel-unicode-digits-operators.md](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/P122-root-channel-unicode-digits-operators.md); строки старого исключения: 89; затронуты P122.2. Mirror: [строка 06](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L238).

**Вставляемое вместо blanket-правила предписание:** Frozen objective replay не является самостоятельным учебным опытом. Если StepVector читает только active count, перестановка или смысл значений при том же count не достигают learner: это отрицательный контроль, не новый semantic gain. Настоящее сравнение .1 допустимо только через actual update или receiver, читающий изменённую координату. Различие вектора не равно изменению знания.

**Конкретное следующее действие:** Зафиксировать TrainingVectorBuilder→actual StepVector input→gradient/optimizer и читать, используется ли значение либо только count. Менять один root extractor только при достижимом различии; не добавлять новый semantic consumer и одновременно называть это ремонтом tokenizer.

**Состояние после разрешения:** `PREPARABLE`; это статус назначения/зависимости, не отметка выполненного или собранного host.

**Code link:** отсутствующий конкретный caller прямо сохранён как незаполненная зависимость; универсальный запрет снимается без выдумывания кода.

## Другие адресные конфликты

- [D04, старый executor prompt](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md#L160-L162): повторное личное разрешение после заполнения B1–B6 отменено текущим поручением и execution policy. Остаётся техническая подготовка source closure/команды; её выполняет исполнитель. Production B2 не менять: экспериментальная копия. Не называть незаполненный tiny caller готовым.

- [D-context](https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md#L198-L215): blanket «никто не учится» заменён решением P69 выше. Если ContextText не читается learned consumer, не выполнять packing proxy; если читатель установлен — зависимый опыт допустим.

- `84-check-source-coverage.md` отсутствует в pinned protected tree, хотя на него 20 ссылок, в том числе master README. Это broken navigation, не причина останавливать Train. Для coverage использовать полный источник 06 и приложенный card/subcase inventory; не создавать фиктивный missing файл в protected.

- Старые абсолютные Windows DLL refs доказывают устройство исторического runner, но не наличие нового host в любом checkout. `experiments-v2/RP01-MICRO` и `35.ExperimentsV2/Experiments/Rp01Micro` отсутствуют в pinned contents. Объявление пути не закрывает source closure/build.

## Готовый промпт исполнителю этого overlay

Текущая работа — подготовить один конкретный паспорт уже выбранного подслучая, следуя его адресной поправке выше. Не запускать все 24 карточки. Сначала прочитай указанные body/mirror/code refs; выпиши writer, state и actual reader либо точную открытую зависимость. Создай только новый документ с буквальными путями source copies, полным Compile Include closure, командой build/run и новым output вне protected. Существующие карточки и результаты read-only. Не называй запись будущей команды выполнением или READY. Если входы доступны, автор карточки готовит их до передачи исполнителю; отсутствие unrelated reader не останавливает другие учебные карточки.

**Десять обязательных правил этого промпта:**

1. Сначала разблокировать действительное обучение; косвенные участки идут после него.
2. Любое измеренное полезное улучшение — успех именно по показателю и домену; accuracy не обязательна, остальные изменения и ухудшения сообщаются рядом.
3. Не запускать самостоятельный опыт на механизме, никак не участвующем в обучении или использовании его результата.
4. Для загрузки данных, восстановления, выдачи обученного результата или управляющей метрики обязательны конкретные writer→state→reader, файл/метод и одна предполагаемая правка.
5. Порядок: учебные блокеры → противоречия карточек → текущие уточнения пользователя → прочие зависимые участки.
6. Проверять одну правку на изолированной копии исходника; до результата не менять production и его тесты, не запускать полную цепочку/CI. Перенос решает пользователь после отчёта.
7. До запуска заполнить точные source closure, host, команду, входы, начальное состояние, counts и новый output; присвоенное имя каталога само по себе не READY. Автор карточки задаёт доступные поля и полный рецепт; исполнитель только материализует их, без нового личного допуска каждого ID.
8. Старые эксперименты, raw, карточки и bin/obj сохранять; новые версии писать рядом, не перезаписывать прежние.
9. Не придумывать обучение для serving-проверки: общий доказанно обученный state допустим; отсутствие reader — OPEN_LEARNING_LINK только этой карточки, не остановка программы.
10. Экспериментальная оболочка может использовать throw/try-catch/void и простые DTO; продуктовые запреты и публикационный workflow не навязывать ей. Не менять исследуемый алгоритм, данные или бюджет под видом такого послабления.

Проверка завершения: конкретный выбранный ID/domain; один фактор; actual learner/зависимость; источники и будущий repair address; одинаковые inputs/state; A0/A1/B0/B1 либо явно trained-state reader A/B; реальные ограничения counts; exact host/command/output; критерий успеха по любому полезному показателю с регрессиями; старые материалы сохранены. Результат этой документной работы — паспорт, не новый measured claim.
