# KNN / Units: новая учебная ошибка и проверенные исторические результаты

Дата аудита: 2026-10-19. Статус новых экспериментов: **NOT_RUN**. Production-код, модели и защищённая папка docs/plans/replanning-possible-diffs не изменялись. Это ограниченная подзадача анализа KNN/Units; она не заявляет полный аудит всех компонентов.

## 1. Новая находка N-KU-01: E14 обучается рекомендовать цикл, который уже существует

**Сейчас.** ActionLabelGenerator.Label считает методом с циклом только тело, содержащее ForEachStatement, ForStatement или WhileStatement. Для параметра с IEnumerable, [] или List< и отсутствия этих трёх видов возвращается ADD_LOOP. Однако деконструирующий foreach представлен отдельным ForEachVariableStatement, а do…while — DoStatement. Поэтому метод с уже реализованным обходом через эти формы получает учебную метку ADD_LOOP. [Нынешний Label, L41–55][e14-label]

Это code-backed гипотеза о качестве *сигнала обучения*, а не измеренное ухудшение RP01. Исходники не выполнялись. Проверены определения соответствующих видов в официальном Roslyn Syntax.xml: DoStatementSyntax L2572 и ForEachVariableStatementSyntax L2674 на прочитанной версии main; эти номера не используются как immutable evidence проекта. Основное доказательство — явно ограниченный список видов в закреплённом исходнике проекта.

Иллюстрация для чтения исходника, **не готовая train-row и не проведённый эксперимент**:

~~~csharp
class C {
    static int F((int left, int right)[] xs) {
        int sum = 0;
        foreach (var (left, right) in xs)
            sum += left + right;
        return sum;
    }
}
~~~

Тело не содержит SimpleMemberAccessExpression; поэтому более ранняя ветка ADD_NULL_CHECK не маскирует рассматриваемое условие. Параметр содержит [], а ForEachVariableStatement не входит в hasLoop. Синтаксическая классификация делает ADD_LOOP независимо от уже существующего цикла. Для do…while аналогичный контрпример можно построить с индексным доступом xs[i], не вводя member access. [Приоритет null-ветки, L32–38][e14-null]

**Что именно учится.** Это не только ошибочная подпись в отчёте. E14ContextActionKnn строит Features и Label из одного source, передаёт эти примеры в FileActionKnnPredictor.Train и считает правильность Predict против того же производителя меток. Таким образом validation может поощрять точное воспроизведение ошибки учителя. [E14, L34–69][e14-driver]

Есть и native facade-контур: TrainFileActionKnn строит те же пары, после ≥4 примеров записывает их в обучаемую память; Compile может получить PredictedFileAction. Serving зависит от включённого UseFileActionKnn, C# и ≥4 train-примеров. [Facade Train, L193–202][e14-train] [Facade Predict, L327–333][e14-consumer] **Включённый обычный RP01 caller в этой подзадаче не установлен**; дефект нельзя объявлять доказанной причиной конкретного RP01 run.

FileActionKnnPredictor.Train заменяет список памяти, а Predict выполняет cosine kNN с голосованием. Финальный ответ после опыта должен по-прежнему выдавать Predict по обученной памяти; вызов исправленного Label непосредственно на EXAM вместо learner сделал бы опыт невалидным. [Train/Predict, L121–141][e14-memory]

**Почему это новое.** Проверены все 525 .md основного planning/possibleDifs snapshot, защищённый корпус, доступный на момент просмотра, и 11 файлов planning-delta после обновления ветки. Поиск включал ForEachVariable, hasLoop, DoStatement, ADD_LOOP, ActionLabelGenerator и русские/английские варианты «деконструирующий foreach». Найдены следующие соседние, но другие механизмы:

| Уже описано | Отличие N-KU-01 |
|---|---|
| N-K3: MethodKnn.CanonicalType не относит While/Do к Loop | Другая цель и другой producer: E14 action-label создаёт ложный ADD_LOOP; While здесь уже учтён |
| LT-N01: признаки E14 не различают List и Task, хотя labels различаются | Здесь ошибка самой учебной метки даже до векторизации; типовой feature не исправит метку |
| N48: чужой/вложенный null guard влияет на ADD_NULL_CHECK | Здесь потеря двух синтаксических форм в loop-presence rule; null guard удерживается вне опыта |
| E14 majority/kNN history и auto-label caveat | Общая оговорка о разметке не содержит этих конкретных AST forms и causal micro A/B |

Вектора E14 также не содержат отдельных координат ForEachVariableStatement и DoStatement. Это **контрольное ограничение**, а не вторая независимая находка: в данном опыте TargetKinds L90–107 неизменён. Если исправленные labels не дают эффекта из-за коллизий или дальних соседей, это результат эксперимента. Одновременное расширение признаков смешало бы причины. [FeatureVector и TargetKinds, L75–107][e14-features]

## 2. Карточка небольшого опыта N-KU-01

### Вопрос, домен и граница вывода

Помогает ли исправление только loop-presence labels научить native FileActionKnnPredictor реже рекомендовать избыточный ADD_LOOP на C# с деконструкцией и do…while?

Домен — классификация требуемого вида правки C# при фиксированной узкой учебной политике. Политика: когда в показанном методе коллекция уже корректно обходится, не предлагать добавить ещё один цикл. ADD_METHOD остаётся fallback-классом существующей таксономии. **Он не объявляется универсально правильной следующей правкой любого такого метода.** Поэтому полезный показатель здесь — снижение ложных ADD_LOOP; общая полезность свободной рекомендации требует отдельного user-task gold.

На перевод EN/RU, SQL, макеты или произвольные языки программирования вывод не переносится. Для других parser-доменов нужны свои виды циклов и своё задание.

### Native контур и единственная переменная

| Элемент | A | B |
|---|---|---|
| Производитель train-label | Текущий ActionLabelGenerator.Label | Тот же код с добавлением ForEachVariableStatement и DoStatement только в hasLoop |
| Features | Текущие 30 координат | Те же 30 координат |
| Learner | FileActionKnnPredictor(k:3) | Тот же learner/k |
| Train-примеры, порядок и бюджеты | Один заранее зафиксированный корпус | Идентичный корпус |
| Gold, EXAM и scorer | Заморожены до первого Train | Те же |
| Предсказание после обучения | Только native Predict | Только native Predict |

Временная экспериментальная B-функция повторяет Label с указанными двумя дополнительными проверками; последующие ветки, null-priority и fallback сохраняются. Проверка правильности самого патча входит в этот же native Train опыт, а не становится самостоятельным дорогостоящим диагностическим заданием.

### Данные и ход обучения

Зафиксировать **8 TRAIN + 8 EXAM**, по 2 строки в каждом из четырёх strata: обычный распознанный цикл; реальное отсутствие требуемого обхода; завершённый деконструирующий foreach; завершённый do…while. Все строки без unrelated member access и без сложного тела, активирующего REFACTOR_EXTRACT. Не смешивать в одном source несколько методов с конкурирующими labels.

TRAIN и EXAM разделяются по алгоритмической/исходной семье; одного переименования класса, переменной или литерала недостаточно. Тексты, SHA, family IDs, target contract и независимый EXAM gold сохраняются до обучения. Если подготовленная семейная разбивка не даёт 8 пригодных EXAM, статус DATA_NOT_READY, а не замена их копиями TRAIN.

Независимый judge задаёт, нужен ли дополнительный обход и какой fallback допускает учебная таксономия; EXAM gold не вычисляется ActionLabelGenerator.Label ни в A, ни в B. Одинаковые gold и denominators применяются ко всем checkpoints.

Последовательность на обоих плечах:

1. L0: Train на 4 одинаковых контролях — обычный цикл и отсутствие цикла. Это общий обученный baseline.
2. L1: Train на тех же 4 плюс 2 деконструирующих foreach.
3. L2: Train на предыдущих 6 плюс 2 do…while.
4. На L0/L1/L2 выполнить одинаковые 8 native Predict и сохранить сырые ответы.

Получается 6 Train calls, списки памяти 4→6→8 в каждом плече, 48 Predict. Восемь уникальных TRAIN предъявляются 18 раз на плечо (4+6+8), всего36 предъявлений. Подготовку labels считать отдельно от стоимости Train(Clear/AddRange); для полной цены приобретения навыка учитывать оба участка. Train заменяет весь список: каждый следующий вызов получает полный accumulated corpus. Это эксперимент последовательного приобретения памяти при полном повторе корпуса, а не утверждение об incremental optimizer или сохранённой LoRA.

### Что сохраняется до/после

| Показатель | L0 A/B | L1 A/B | L2 A/B | Изменение |
|---|---|---|---|---|
| Ложные ADD_LOOP на уже завершённом обходе | Actual/denominator | Actual/denominator | Actual/denominator | Внутри каждого плеча и B−A |
| Recall ADD_LOOP, где обход отсутствует | Actual/denominator | Actual/denominator | Actual/denominator | Включая ухудшения |
| Exact action-label accuracy по узкой политике | Actual/8 | Actual/8 | Actual/8 | Без переименования в semantic quality |
| Сохранение ответов на прежних control families | Raw answers | Raw answers | Raw answers | Забывание/регрессии видны |
| Время Train и Predict, peak RAM, bytes памяти | Actual | Actual | Actual | Сопоставимые границы измерения |
| Количество записей, top-k neighbours и labels | Actual | Actual | Actual | Подтверждение действия сигнала |

Нет придуманных чисел, ΔLoRA или optimizer steps. Neural metrics здесь N/A с причиной: обновляется память FileActionKnnPredictor. Если задержка ниже разрешения таймера, указать «неразличимо», а не выигрывать шумом измерителя.

**Правило успеха:** любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности для этого не обязателен. В отчёте рядом показываются изменения остальных показателей, включая ухудшения. «Успех по одному показателю» не равняется общему promotion или доказанной полезности всех ответов.

Если B даёт меньше ложных ADD_LOOP, но хуже распознаёт недостающие циклы, обе дельты публикуются рядом. Если изменились только labels в памяти, но native answers и полезные показатели не изменились, влияние на качество не подтверждено на данной панели.

### Промпт исполнителю

> Выполни N-KU-01 как маленький native эксперимент обучения FileActionKnnPredictor, не как отдельный запуск исправленной эвристики на EXAM. Прочитай закреплённые Label/FeatureVector/Train/Predict и карточку. Подготовь 8 TRAIN/8 EXAM по четырём указанным strata, с family-disjoint split, одинаковыми golden targets и отсутствием ранней null-ветки. Меняй только два loop-kind условия производителя train-label; features, k=3, corpus, порядок и scorer одинаковы. Обучи оба плеча на L0/L1/L2 и запиши 48 реальных Predict с before/after полезными метриками, стоимостью и регрессиями. Не выдумывай нейронное обучение, ответы, gold или успешные результаты. CODEX astra 6 ultracode выбирает задания и оценивает точные сравнения; если для этой подготовки создаётся DPO, обе его стороны генерирует внешняя открытая китайская модель, а точные стороны/оценки/проекция сохраняются. Native E14 memory-probe сам не вызывает DpoTrainer: не добавляй нейронный путь ради названия DPO. Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен. Рядом показывай все другие изменения, включая ухудшения. Не трать отдельный бюджет на zero-training diagnostics, не меняй protected docs/plans/replanning-possible-diffs и не подключай B к production.

Для вывода о RP01 нужен ещё установленный product consumer тех же learned predictions. Это условие ограничивает причинную интерпретацию уже выполненного native опыта; оно не превращает исправление признаков или включение feature flag во вторую скрытую переменную.

## 3. История: положительные сохранённые результаты действительно найдены

Эти числа прочитаны в архивных receipt-файлах; в текущей сессии они не пересчитывались. Присутствие одного и того же архивного запуска в нескольких ветках не создаёт несколько независимых экспериментов.

| Где найдено | Архивный результат | Что это поддерживает | Граница |
|---|---|---|---|
| language_translate f6032aff, E25 | top1 0.3438→0.875, 160 examples, 32 val; hard 0.8571; time 0.4→15.4 ms | Trained POS-bigram KNN на общей validation панели действительно дал полезное улучшение pattern classification | Синтетические POS-последовательности; не извлечение POS из raw языка и не end-to-end перевод; accuracy выросла, скорость ухудшилась |
| external_domains 3036a01a, E26 | disambiguation 0.4→1.0 на15 cases, hard6; time0.8→2.2 ms | Контекст соседних POS может существенно улучшить выбор значения слова в техническом RU→EN | Driver вызывает BuildDefault и не учит словарь новым урокам; успех фиксированного контекстного механизма, не before/after приобретения навыка |
| PrepareToGPL 31346d89, E43 | retention 0.3226→0.3978; old465/new414/replay177; time0.1→5.8 ms | Повтор прежних примеров в обучении count-table сохраняет больше старых переходов, чем new-only table | Обе таблицы строятся заново; деление old/new по половинам списка файлов; neural optimizer/LoRA не проверены |
| DesignToCode 8f20ceb8, ранний E09 | top1 0.3368→0.3472, абсолютная дельта +0.0104; 968 tokens/773 trainPairs; time1→74 ms | Небольшой положительный архивный результат по метрике; FAILED выставлен из-за старого порога ≥5%, а не отсутствия улучшения | По сохранённому driver val=194, поэтому оба плеча проверяют193 одинаковых перехода; нет per-case IDs и hash фактически исполненного driver в receipt |
| local_wiki_planning 219d7ba8, более поздний E09 | top1 0.2413→0.3507; 4492 trainPairs; time14.5→763.5 ms | Сохранённый положительный результат структурного KNN нельзя потерять при планировании | Baseline проверяется на всей val, KNN на val.Take(500); эффект требует одинаковой панели для causal/generalized claim |
| DesignToCode 8f20ceb8, Q3 | 8 TRAIN/4 heldout; trained family accuracy1.0, no-state0, exact0, regex0.5, keyword0.25 | Есть сохранённое улучшение маленького state-dependent family classifier с указанным source-disjoint split | Receipt внутренне ссылается на branch codex/chat-gpt-5.5; это найденный в DesignToCode архив, не независимый run данной ветки; semantic_quality_claimed=false |

**Прямые evidence:** [E25 receipt, L5–17][e25-receipt] и [E25 driver, L17–43][e25-driver]; [E26 receipt, L5–17][e26-receipt] и [E26 driver, L15–42][e26-driver]; [E43 receipt, L5–17][e43-receipt] и [E43 trainer/eval, L23–55][e43-driver]; [E09 receipt, L5–17][e09-receipt] и [разные панели, L45–72][e09-driver]; [Q3 receipt, L5–31, L55–59][q3-receipt] и [4 per-row результатов][q3-heldout].

Ранний E09 найден при адресной дополнительной проверке уже имевшейся в корпусе ссылки: [primary receipt, L4–19][e09-early-receipt]. Для968 узлов сохранённый [driver, L25–27][e09-early-driver] даёт train774 и val194; ветки L46–53/L65–72 оценивают193 перехода, так как лимит500 не срабатывает. Это вывод по receipt плюс source на том же snapshot, не выполненный нами rerun. Положительная абсолютная дельта0.0104 полезна по этому показателю/домену согласно нынешнему правилу пользователя; одновременно latency ухудшилась1→74ms. Старый FAILED отражает порог относительного прироста5%. Перенос E07→E09 и общий end-to-end эффект этими числами не доказаны.

E25 особенно релевантен потере функции в переносе: нынешний TranslationSemanticTokenizer.Tokenize вызывает приватный ClassifyPattern, который имеет фиксированные ветки по позиции первого VERB и PRON. В этом пути не вызывается обученный E25-классификатор. [Tokenize L98–117][current-tokenize] [ClassifyPattern L351–361][current-pattern] Это **уже описанная в корпусе** потеря, здесь она подкрепляется соседней веткой с реальным успешным receipt, не объявляется новой. Current project12 PatternClassifier.Train действительно меняет память, Predict использует её; совпадение названий между проектами не доказывает общий execution path. [Текущий trained classifier, L34–54][current-e25]

## 4. Что нельзя принять за доказательство улучшения обучения

### Teaching runs измеряли выполнение и расходы, а не held-out skill gain

20-row receipt language_translate сохраняет 20 строк, 18 accepted, 2 resource-review, итог108 flows/77 units. 100-row receipt сохраняет 100 строк,12 accepted,88 resource-review,394 flows/363 units. Это настоящие результаты запусков и наблюдения масштабирования ресурсов, а не доказательство роста accuracy при добавлении Units. В этих summary нет одинаковой полезной панели до/после. [20-row L3–16][teach20] [100-row L3–16][teach100]

### P3 верифицирует другой вход при неактивном discriminator

В receipt P3-C содержит9 packets и потерянный BaseService, P3-V уже10 packets, lost0 и discriminator_activated=false. [Receipt L7–16, L89–100][p3-receipt] В соответствующем historical source BuildVerificationRun сам возвращает original uncorrupted packets, а discriminator activation вычисляется при lostConstraints>0. Итоговый PASS_WITH_GAPS записан прямо в конструкторе; summary baseline0/enhanced1 отражает success-gates. [BuildVerificationRun L363–397][p3-code] [Summary L123–137][p3-summary]

Такой положительный archival status нельзя переименовать в causal learning gain при сохранённом входе: одновременно восстановлен missing packet, а проверяемый discriminator не активен. Запись показывает предусмотренный contract/scenario. Не утверждается, что все P3 опыты или всё соседнее обучение были бесполезны.

## 5. Историческая проверка protected на c3f1: текущий статус вынесен отдельно

**Замечания этого раздела относятся к c3f1. Последующая внешняя правка f50 изменила допуск этих диагностик; актуальное решение см. в [аудите покрытия](03-EXPERIMENT-COVERAGE-AND-VALIDITY.md).**

Прочитан целиком файл experimets/2026-10-19-small-learning-probes/17-additions-direction-evaluation-and-policy.md текущего доступного snapshot. Папка не изменялась.

- **EVAL-DIR, L7–22:** прямо указано, что память не обучается; 0 Train/optimizer. Это проверка измерителя. Для текущего пользовательского запрета standalone zero-learning run не подходит.
- **POLICY-SPACE, L40–48:** 0 updates; подготовка исходной памяти возможна отдельно. Сравнивается frozen-state source-law, не приобретение навыка.
- **METRIC-STATE нельзя целиком назвать «без Train».** Основной план L26–30 содержит0 Train, но L32–34 предлагает необязательный follow-up с2 TrainStructuredBatchAsync и12 вложенными Unit.TrainOnExample. Он фиксирует report AccuracyBefore при одинаковом обновлении и ответах. Покрытие задачи частичное: Train-stage существует, однако optional, и влияние места хранения scalar на полезный результат обучения/его решения в карточке не установлено.
- L3 честно пишет NOT_RUN и отделяет эти проверки от нейронного D. Это снижает риск ложного заявления о LoRA, но не делает zero-training probe соответствующим запрету пользователя на такие задания.

Coverage verdict должен сохранять этот контекст и не объявлять baseline-сегмент всей карточкой. Новый N-KU-01 содержит обязательные native updates и одинаковую экзаменационную панель во время обучения.

## 6. Граница полноты

Новый механизм: **1**, независимых новых причинных экспериментов выполнено: **0**. Новая micro-card: **1**. Исторические результаты — сохранённые source receipts, не новые measurements. Изучены KNN, переводные Units, structural/method/file-action predictors, replay fusion и адресные соседние ветки; большинство найденных швов уже описано (P24 replay, U-N1/U-N2 phase corpus, LT-N01, N48, N50, E25/E26 wiring). Для сохранения новизны они не размножены в новые номера.

Факты кода подтверждены на 8e020052fae7efa694b8d33f90768648d6b67af9. Root сообщил, что поздняя core delta до c3f1 меняет protected markdown, а не источники; окончательное metadata-binding поздней delta остаётся у root. Planning-delta дополнительно проверен по точным ключам N-KU-01 без новых совпадений. Полную semantic-coverage матрицу protected corpus ведёт отдельный coverage-agent.

[e14-label]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/12.SemanticCompiler/Prediction/FileActionKnn.cs#L41-L55
[e14-null]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/12.SemanticCompiler/Prediction/FileActionKnn.cs#L32-L38
[e14-driver]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/35.ExperimentsV2/Experiments/E14ContextActionKnn.cs#L34-L69
[e14-train]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/12.SemanticCompiler/SemanticCompilerFacade.cs#L193-L202
[e14-consumer]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/12.SemanticCompiler/SemanticCompilerFacade.cs#L327-L333
[e14-memory]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/12.SemanticCompiler/Prediction/FileActionKnn.cs#L121-L141
[e14-features]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/12.SemanticCompiler/Prediction/FileActionKnn.cs#L75-L107
[e25-receipt]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json#L5-L17
[e25-driver]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E25PatternClassifier.cs#L17-L43
[e26-receipt]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-001.json#L5-L17
[e26-driver]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs#L15-L42
[e43-receipt]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json#L5-L17
[e43-driver]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L23-L55
[e09-receipt]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json#L5-L17
[e09-driver]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L45-L72
[e09-early-receipt]: https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/experiments-v2/E09-STRUCTURAL-KNN/results/run-2026-04-30-001.md#L4-L19
[e09-early-driver]: https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L25-L72
[q3-receipt]: https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/artifacts/smart-skills-quality-uplift/q3/prompt-to-task-q3-01-training-report.json#L5-L59
[q3-heldout]: https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/artifacts/smart-skills-quality-uplift/q3/prompt-to-task-q3-01-held-out-report.json#L5-L51
[current-tokenize]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/27.TranslationDomain/TranslationSemanticTokenizer.cs#L98-L117
[current-pattern]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/27.TranslationDomain/TranslationSemanticTokenizer.cs#L351-L361
[current-e25]: https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/12.SemanticCompiler/Translation/PatternClassifier.cs#L34-L54
[teach20]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/experiments-v2/TEACHING-20-ROWS-CURRENT/results/run-2026-05-07-000802/teaching-20-row-current-summary.md#L3-L16
[teach100]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/experiments-v2/TEACHING-20-ROWS-CURRENT/results/rows-100/run-2026-05-07-003342/teaching-100-row-current-summary.md#L3-L16
[p3-receipt]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/experiments-v2/P3-LEARNING-FROM-TRANSITION-FAILURE/results/run-2026-05-11-194300.json#L7-L100
[p3-code]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/P3LearningFromFailureProof.cs#L363-L397
[p3-summary]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/P3LearningFromFailureProof.cs#L123-L137

