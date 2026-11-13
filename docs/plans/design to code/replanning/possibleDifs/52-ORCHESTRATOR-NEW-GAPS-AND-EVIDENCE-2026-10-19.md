# N52 — независимые различия именно `orchestrator-extensions` ↔ текущий pipeline: доказательства и ограничения

**Дата:** 2026-10-19. **Роль:** независимый code/architecture/plan review, не исполнение продуктового обучения. **Эталон:** [`orchestrator-extensions@b293045`](https://github.com/lubochka/llm_mvp_core/tree/b293045bd89b1eb96d3e32fedd341f1a4170adbc). **Текущий код:** [`codex/dpo-implementation@4450782`](https://github.com/lubochka/llm_mvp_core/tree/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76). **Цель:** восстановить полезность обучения для настоящего получателя и обычного потребителя. Нативные C# run, обучение модели и независимая репликация в этой работе **НЕ проводились**.

Сначала читать [ГЛАВУ 0 — golden-first](00-ORCHESTRATOR-EXTENSIONS-GOLDEN-RESTORE-P0-2026-10-19.md) и [микроэксперименты GO-01..GO-04](52-ORCHESTRATOR-DOMAIN-MICROEXPERIMENTS-2026-10-19.md). Новизна ниже **узкая**: новые проверенные *source seams* конкретной ветки. Общее отсутствие Unit в RP01 Student, raw DPO truncation, проблемы kNN/E09/E26/E43, EOS/BPE, storage и synthetic accuracy уже есть в [00-MASTER](00-MASTER-SYNTHESIS-AND-CONFLICTS.md), [language-translate](00-LANGUAGE-TRANSLATE-RECOVERY-FIRST-2026-10-19.md), [external-domains](00-EXTERNAL-DOMAINS-SUCCESS-RESTORATION-P0-2026-10-19.md) и документации read-only `llm_mvp_core/docs/plans/replanning-possible-diffs`. **Не считать их вновь открытыми.**

**ПРИОРИТЕТ ПЕРЕД N52:** [уже подтверждённый источниками GOLD-OR-01](52-ORCHESTRATION-REASONING-CHECKPOINT-AND-ADMISSION-GAP-2026-10-19.md) — v1 обученная OrchestrationReasoning память (15 patterns) не допускается current v2 loader, ordinary API переходит к пустой машине. Это более непосредственный golden learning-state reachability seam. [Микропроверка GOLD-OR-A0](52-ORCHESTRATOR-GOLD-DOMAIN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md) должна идти до N52. N52-01/02/03 — **дополнительные**, source-confirmed gaps в Design-to-Code probe и критериях наблюдения, а не замена P0 restore.


## 1. Проверенные находки и самостоятельные гипотезы

### N52-01 — continuation probe зависит от порядка, а не от покрытия получателей (НОВОЕ, source-confirmed)

**Тема обучения:** Design-to-Code (HTML/CSS); **задача:** продолжение обучения нескольких разных Critic/Improver/Unit; **получатель:** именованная единица, а не общий счетчик; **механизм:** sampling для локального continuation probe.

[Прямой C# источник, строки 2588–2633](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/44.DesignConversionDomain/DesignConversionTrainingPipeline.cs#L2588-L2633):
- по batch/record order выбираются `records.Count < 10`, затем немедленно `break`;
- нет стратификации по `UnitId`, correction-category, источнику, сложности или редкому классу;
- при неодинаковых unit labels репорт присваивает одну синтетическую строку `mixed-design-conversion-units`, не сообщает покрытие и качества конкретных получателей.

**Почему важно:** «успешный probe» может исследовать только один простой Unit при наличии остальных; перестановка абсолютно тех же учебных примеров меняет охваченные единицы. Это не баг optimizer и не «учились только на 10 записях»: [стр. 1810–1843](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/44.DesignConversionDomain/DesignConversionTrainingPipeline.cs#L1810-L1843) отдельно собирают **все** `TrainingItems` и передают их в `ProjectFinalOptimizerInputs`; код исключает такой более сильный вывод.

**Выполненная SOURCE-EQUIVALENT аналитическая проба, но не native-модель:** [Python-прокси](52-ORCHESTRATOR-PROBE-ORDER-PROXY-2026-10-19.py) и [JSON](52-ORCHESTRATOR-PROBE-ORDER-PROXY-RESULT-2026-10-19.json) используют 24 искусственных урока: 10 A, 7 B, 7 C; выдуманные toy labels A/C верно, B ошибочно. Sorted order выбирает 10 A, **покрытие 1/3**, условные **10/10**; interleaved выбирает 4 A + 3 B + 3 C, **покрытие 3/3**, условные **7/10**. Общая toy-доля правильных 17/24 не меняется; меняется *видимость* ошибки при cap=10. Это математический контрпример порядку выбора, **никакой C# checkpoint/качество модели не измерены**.

**Гипотеза:** перестановка исходного корпуса меняет репорт продолжения так, что regression более поздних Unit останется невидим. **Быстрая проверка GO-02:** нативно вызвать локальное создание probe для одинаковых 24 записей при двух порядках (через существующий caller/admitted path), сравнить `RecordIds`, `UnitId`, фактические оценки каждого Unit. Отдельно добавить стратифицированное **измерение**; не менять сам train selection. **Фальсификация:** все три Unit представлены в обеих версиях или внешний consumer гарантирует собственную полную независимую проверку и никогда не выводит общий успех из этого probe.

### N52-02 — суммарное «RealTrainedWeightsContinued» может не обнаружить нулевую дельту самих весов (НОВОЕ, source-confirmed predicate / impact-hypothesis)

**Тема:** Design-to-Code, learned Critic/Improver; **получатель:** весовая карта конкретного Unit; **механизм:** критерий доказательства продолжения весов.

[LocalContinueTrainingProbe #L248–L264](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/44.DesignConversionDomain/DesignConversionLocalContinueTrainingProbe.cs#L248-L264) объявляет `realWeightsContinued` истинным, если валидных артефактов > 0, их количество не уменьшилось и **сумма ObservationCount выросла**. [Machine #L1450–L1460](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/44.DesignConversionDomain/DesignConversionMachine.cs#L1450-L1460) считает артефакты/observations только по `CriticUnit` + `ImproverUnit`; [#L2817–L2843](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/44.DesignConversionDomain/DesignConversionMachine.cs#L2817-L2843) подтверждает суммирование `ObservationCount` валидных feature weights, **не сравнивает ни `Weight`, ни результат inference до/после**.

**Новая тонкость:** рост числа обучающих наблюдений — действительно интересный факт о corpus state, но не доказательство ненулевого обучающего эффекта. Теоретическая пара состояний `Weight=0.25, ObservationCount=4` → `Weight=0.25, ObservationCount=5` удовлетворяет count-предикату, хотя численная функция не изменилась. Это **логическое свойство метрики**, не свидетельство, что система реально произвела такое состояние.

**Сильная оговорка:** сам автор пробного отчёта явно ставит `MeasurementGateRun:false`, `PromotionUploadAllowed:false` ([#L266–L294](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/44.DesignConversionDomain/DesignConversionLocalContinueTrainingProbe.cs#L266-L294)). Следовательно, *внутри этого API* нет обещания выросшего heldout quality; риск появляется, если последующая сводка/агент интерпретирует поле как фактическую компетентность. До caller/consumer-trace — `OVERCLAIM_RISK`, не `QUALITY_REGRESSION_CONFIRMED`.

**GO-04A:** три source-equivalent checkpoint-формы: X без наблюдений/весов, Y с наблюдениями+1 и неизменным `Weight`, Z с наблюдениями+1 и изменённым `Weight`; проверить, что predicate Y == Z, но `ΔWeight_Y=0` и domain heldout result может различаться. **GO-04B:** только легальный C# learned Unit, после реального обучения, cold restore, ordinary HTML/CSS consumer: `hash(feature->weight)`, `ObservationCount`, `output score`, blind QA отдельно.

### N52-03 — старый публичный продолжатель закрыт authority-gate; риск только для НЕпрослеженного legacy caller (УЗКАЯ гипотеза)

**Тема:** Design-to-Code, продолжение с существующего checkpoint. В [gold version #L39–L55](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/44.DesignConversionDomain/DesignConversionLocalContinueTrainingProbe.cs#L39-L55) публичный `VerifyFreshLoadedCheckpointAsync` реально выполнял тело. В [current #L49–L79](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/44.DesignConversionDomain/DesignConversionLocalContinueTrainingProbe.cs#L49-L79) старый entrypoint **сразу выдаёт отказ**, потому что нет `server-owned legal training authority`; допускается новый внутренний `VerifyFreshLoadedCheckpointAdmittedAsync`.

**Контрдоказательство против поспешного обвинения:** текущий [`TrainingPipeline` #L2234–L2250](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/44.DesignConversionDomain/DesignConversionTrainingPipeline.cs#L2234-L2250) уже вызывает admitted entrypoint. Значит эта внутренняя цепь *не* подтверждает поломку. Нужен exhaust caller graph по другим реальным потребителям, которые возможно используют старую функцию и делают silent fallback вместо явной ошибки. **Никогда не чинить обходом server-authority/лицензионного gate**; если caller существует, мигрировать его на разрешённую admission-цепь, сохранив security.

**GO-04C:** статический call graph и затем один легальный native call до и после маршрутизации, удостоверившись, что `fail` не скрывается и выученное состояние дойдёт до обычного consumer. Если внешних caller нет — `NOT_APPLICABLE`, гипотезу закрыть без изменений кода.

### N52-04 — разные модели обучения в золотой ветке не дают одного эталонного «accuracy» (НОВОЕ сравнение указанного SHA с архивом)

В `orchestrator-extensions` действительно существуют **несколько** сохранившихся DPO run: [cumulative150](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/experiments-v2/INTERNAL-TRAIN-READY-V191/results/base-wiki-session-01-batch-004-cumulative150-20260515-1025/internal-train-ready-dpo-summary.json) с final minibatch accuracy 0.8333 и margin +0.0158648, [cumulative200](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/experiments-v2/INTERNAL-TRAIN-READY-V191/results/base-wiki-session-01-batch-005-cumulative200-20260515-1045/internal-train-ready-dpo-summary.json) с final accuracy 0 и margin −0.0321613, [V210](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/experiments-v2/INTERNAL-TRAIN-READY-V191/results/train-ready-v210-good-external-plus-internal-guide-full-20260511-014712/internal-train-ready-dpo-summary.json) 0.8333/+0.00570256. Они измеряют **разные последние training batches**, не SAME-INPUT retention и не cold-load free generation. Предыдущие агенты уже предупреждали о несопоставимых run; **новая полезная постановка** — выбрать точно `cumulative150` как *контроль позитивного neural run из явно указанной ветки*, но не считать сам branch универсально работающей конфигурацией.

**GO-01:** frozen `chosen/rejected` sample и реально сохранённое/созданное начальное состояние; 4 arms с равным eval, два типа outcomes (preference AND ordinary generation). Фальсификация: позитивный donor preference не воспроизводится из его frozen source и initial state — сперва установить материальную/историческую тождественность, не «чинить» новый LoRA вслепую.

## 2. Инвентаризация других подозрений и scope

| Ось аудита | Что уже зафиксировано документами | Что проверять дальше | Релевантные темы |
|---|---|---|---|
| DPO: pair loss/objective, wrong rejected, response mask, tokenizer identity | Материал exact v2 отличается от раннего hashed/truncated; текущий `DpoTrainer` действительно обучает allowed LoRA | same-pair digest → model-visible token boundaries → chosen/rejected gradients → tiny overfit → cold generation | Документация, C#/DTC, языковое обучение |
| LoRA: rank/targets, frozen/reference, restore optimizer/RNG | Gold optimizer full parameter, current installed q/v LoRA. Нельзя копировать старый optimizer без проверки | ablation trained-only q/v vs compatible rank+targets vs warm-base, отдельно сложность | Все neural recipients, не memory/kNN-only |
| Семантическая векторизация, KNN, AST, POS, порядок, морфология | E09/E25/E26/E27 и memory functions известны; `TranslationTrainedUnits.cs` в сравниваемых ветках эквивалентен по исполняемому тексту | query→feature identity, domain-specific k, confidence/negative scope, ordinary consumer, checkpoint | AST — программирование, POS/order — перевод |
| Из DPO в typed Unit/Flow/Graph | Хорошая Unit memory ≠ обученная LM; reverse DPO rejected не равен chosen-only Unit objective | проверять отдельные recipients, кардинальности many→many, связь и реальный ответ | Перевод, оркестрация, DTC |
| Восстановление | Metadata-only import vs full restore встречается в других подсистемах; DPO публикация отдельно | separate hash `base/LoRA/optimizer/tokenizer/knn/rules/graph`, fresh-process use | Все — по конкретному типу состояния |

## 3. Матрица доказательств и следующий перевод в статус

- `SOURCE_CONFIRMED`: N52-01 cap10/mixed-id, N52-02 count-based report, N52-03 entrypoint gate; источники и строки приведены.
- `ANALYTIC_PROXY`: N52-01 перестановка 24 искусственных записей подтверждает **только выборку**, не качество.
- `HISTORICAL_RECEIPT`: DPO 150/200/V210 на указанном golden commit — фактические frozen файлы, происхождение текущего загруженного бинарника отдельно не подтверждено.
- `UNPROVED_CAUSE`: ни один N52 не объявлен прямой причиной C#0/3, ENRU0/4 или недостаточного RP01 transfer.
- `NATIVE_NOT_RUN`: GO-01–GO-04 требуют нативного исполнения, независимой экзаменационной панели, отдельного арбитра и reproducible receipts.

**Правило план/код-review:** не подменять source observability тестами, которые «подогнаны» под детерминированный ответ. Любой claimed quality gain требует SAME-INPUT baseline, нового непересекающегося heldout, фактически вызванного обученного consumer, negative controls, состояния до/после и cold restore. Ремонтить **одно causal отличие за опыт**. Закрывать без продукта, если гипотеза останавливается на неиспользуемой функции.
