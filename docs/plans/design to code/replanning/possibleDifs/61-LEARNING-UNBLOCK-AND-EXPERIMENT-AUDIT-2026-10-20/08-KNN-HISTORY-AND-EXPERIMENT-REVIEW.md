# История kNN и доменных функций: что действительно сохранилось

**Статус: source audit и чтение исторических receipts, NOT_RUN.** Здесь нет новых запусков обучения. Данные из старых JSON сохранены с их исходными метриками, включая проигрыши. Задача этого документа — не потерять полезный исторический сигнал и не приписать его другому обучаемому получателю.

Код проверялся на core `bf9b543e149ef197da85a767821d98c9b67916d2`. Во время аудита появился `f5dd532045716495a2c7668573250e5d7cc46810`: в прочитанном compare изменены152 документа защищённого плана, исходные code files ниже не менялись. Актуальная политика20.10 допускает обучение и связанные с ним загрузку/подготовку/восстановление/чтение состояния при точной зависимости; нового Train ради формальности не требуется. Результаты обучения не переносим между доменами без проверки.

## 1. Какие соседние ветки проверены

| Ветка | Закреплённый commit |
|---|---|
| [local_wiki_planning](https://github.com/lubochka/llm_mvp_core/tree/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a) | `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a` |
| [language_translate](https://github.com/lubochka/llm_mvp_core/tree/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9) | `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` |
| [DesignToCode](https://github.com/lubochka/llm_mvp_core/tree/8f20ceb8a9bfbadab2ad309226c47dd76e714548) | `8f20ceb8a9bfbadab2ad309226c47dd76e714548` |

У всех трёх snapshots одинаковые Git tree SHA для `12.SemanticCompiler`, `22.AgentAlgorithms`, `33.ExperimentMatrix`, `34.Experiments`, `35.ExperimentsV2`. Проверенные ветки здесь не являются тремя независимыми реализациями этих алгоритмов.

| Поддерево | Общий sibling tree SHA |
|---|---|
| 12.SemanticCompiler | `66eb13b71f12d4881b92fe551e24bdd62fcc74a7` |
| 22.AgentAlgorithms | `8ff8e9c543d7d90c30c06c22f4a85c4af811e91a` |
| 33.ExperimentMatrix | `21f5a33a5b63e97ecb9ce27dbd1127e8433bcda0` |
| 34.Experiments | `82ffd0208fe99ecf1abc280598ecb32b81b67632` |
| 35.ExperimentsV2 | `aa5c7d5d6f3a1ecb1f221755dae61f03141d62da` |

Root-tree receipts и поддеревья прочитаны при исследовании; закреплённые refs и граница чтения указаны в `inventory/source-scope.json`. В опубликованный пакет включена сводка сравнений `inventory/selected-sibling-parity.json`; полные деревья не объявляются повторно прочитанным исходным кодом.

В `35.ExperimentsV2` у current и sibling по1246 blobs, 1132 имеют тот же Git SHA, 114 отличаются; отсутствующих относительно sibling файлов нет. Эти114 — C# файлы. **Проверено содержимое семи конкретных файлов**, а не всех114: каждый совпадает с sibling после удаления ровно первых пяти строк лицензии current. Полные SHA256 и результат сравнения находятся в [selected-sibling-parity.json](inventory/selected-sibling-parity.json).

1. `12.SemanticCompiler/Prediction/StructuralKnn.cs`.
2. `22.AgentAlgorithms/Context/PredicateDatasetRouter.cs`.
3. `35.ExperimentsV2/Experiments/E09StructuralKnn.cs`.
4. `35.ExperimentsV2/Experiments/E22DatasetRouter.cs`.
5. `35.ExperimentsV2/Experiments/E26ContextualDictionary.cs`.
6. `35.ExperimentsV2/Experiments/E34HybridMatcher.cs`.
7. `35.ExperimentsV2/Experiments/E43ReplayBuffer.cs`.

**Вывод этой проверки:** перечисленные экспериментальные реализации не исчезли при переходе на current. Следующая возможная потеря — конкретный production caller, состояние, признаки или контракт передачи результата. Наличие файла не доказывает, что RP01 вызывает его. Отсутствие доказанного вызова не означает, что успешного обучения не было.

## 2. Все прочитанные raw receipts, без отбора только побед

Прочитаны47 canonical JSON шести семейств; агрегат с путём каждого исходного JSON и всеми полями находится в [historical-selected-raw.json](inventory/historical-selected-raw.json). Counts относятся к сохранённым файлам, а не к независимым статистическим повторам. В подборку не включались вложенные дубли каталогов `results/35.ExperimentsV2/...`.

| Семейство | Receipts | Enhanced primary выше | Равно | Ниже |
|---|---:|---:|---:|---:|
| E09 Structural kNN | 12 | 12 | 0 | 0 |
| E22 Dataset router | 18 | 5 | 9 | 4 |
| E26 Contextual dictionary | 9 | 9 | 0 | 0 |
| E34 Episode matcher | 2 | 0 | 0 | 2 |
| E34V2 Hybrid matcher | 4 | 4 | 0 | 0 |
| E43 Replay buffer | 2 | 2 | 0 | 0 |

Все primary в этой подборке имеют `lower_is_better=false`. Положительный знак — полезный наблюдённый показатель конкретного receipt. Он не устраняет несопоставимость панелей или неизвестный перенос на другого learner.

## 3. Наблюдаемые результаты и настоящие получатели

### E09: полезный AST-сигнал с важными пределами измерителя

[Raw tier1, 30.04 run001](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-04-30-001.json): top1 `.3368→.3472`, то есть **+1.04 процентного пункта**; в receipt время `.5→54 ms`. Это сохранённый локальный выигрыш primary, рядом существенная цена вычислений. В этой tier1 панели968 узлов,773 training transitions; обе стороны проверяют193 validation transitions, ограничение500 ещё не отрезает enhanced.

Настоящий learner — `StructuralKnnPredictor.Train(trainingPairs)`: [E09 L30–78](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L30-L78). Writer `_training.Clear/AddRange`, reader `Predict` с weighted structural distance и majority vote: [StructuralKnn L116–161](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/12.SemanticCompiler/Prediction/StructuralKnn.cs#L116-L161). Baseline — частотные переходы AST-kind IDs, несмотря на комментарий «bigram BPE». Это не градиентный next-token LLM.

[Raw tier2, 01.05 run001](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json): `.2413→.3507`, apparent +10.94pp, время14.5→763.5ms. Однако на5617 узлах baseline оценивается на1123 transitions всей validation, enhanced — только499 transitions первых500 узлов. Доказанный source-разрыв [L50–77](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L50-L77) не позволяет трактовать эту большую разницу как чистое парное improvement.

Ещё один старый предел: `kindMap` создаётся отдельно для каждого файла, [L107–120](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L107-L120). IDs могут менять смысл между файлами. Даже matched tier1 не доказывает безусловную переносимость codec. Это уже известный механизм [P27.4](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/cards/P27-structural-codec.md#L70-L72); новый дефект здесь не заявляется.

**Область:** C# AST и его роли/глубина/порядок обхода. Для RU/EN-предложений эти Roslyn признаки не определены. Малые active C и N57-ARG адресуют именно C# компонент и не должны оцениваться свободной генерацией чужой модели.

### E22: старый FAILED не отменяет малого полезного macro-F1

[Raw tier3, 01.05 run002](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/results/E22-DATASET-ROUTER/results/run-2026-05-01-002.json): macro-F1 `.4892→.4944`, **+0.52pp**, но `passed=false` и `next_action=TUNE_OR_ABANDON`. По текущему правилу пользователя этот локальный measured gain полезен по этой оси; старая пороговая метка не превращает положительный знак в ноль.

Получатель — `AgentAlgorithms.PredicateDatasetRouter`; raw фиксирует260 TRAIN и65 validation из325 examples. Native [Train L41–47](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/Experiments/E22DatasetRouter.cs#L41-L47), чтение [Predict L84–100](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/Experiments/E22DatasetRouter.cs#L84-L100). Метки создаёт `AutoLabel`, это не независимая ручная оценка полезности routing для конечного задания.

Raw `.5→196.9ms` нельзя назвать сравнением стоимости обучения: `router.Train` выполнен до обоих таймеров, baseline-таймер включает построение frequency model и оценку, enhanced — Predict/оценку. Источник [L45–80](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/Experiments/E22DatasetRouter.cs#L45-L80). Для новой ресурсной оценки нужны одинаковые интервалы.

**Область:** выбор dataset bucket по предикатам исходного кода. Полученные Generic/CSharp/BugFix F1 не доказывают нейронное усвоение предметной документации. Четыре отрицательных receipts и девять равных остаются в полном реестре рядом с пятью положительными.

### E26: полезный контекстный выбор, отдельный от усвоения новых уроков

[Raw 01.05 run001](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-001.json): disambiguation `.4→1.0`, **+60pp** на15 статических cases, hard6/6; время `.8→2.2ms`.

Source [E26 L20–47](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs#L20-L47) создаёт `ContextualDictionary.BuildDefault()`, затем сравнивает baseline/contextual readers. Записи уже прописаны в исходниках. Это evidence, что для этих русских технических слов POS-контекст помогает выбирать заранее сохранённое значение; это не measured получение словаря из новых текстовых уроков.

Актуальный protected T явно создаёт новые supervised записи через AddEntry и отдельно меняет query POS. По правилу20.10 это допустимая проверка использования усвоенного состояния. [T-TRAIN-CONTEXT-01](05-T-TRAIN-CONTEXT-SMALL-EXPERIMENT.md) — другой, более прямой учебный фактор: стирать/сохранять POS в TRAIN entries при фиксированном query reader. Он не повторяет E26 и не объявляется новой production-регрессией.

**Область:** неоднозначные слова при явной POS-схеме и связанной разметке. Any-overlap не проверяет порядок предложения; AST-роли, семантика целой фразы и языки без собственного материала не переносятся автоматически.

### E34 и E34V2: нельзя объяснять разворот одной только «функцией kNN»

[Plain E34 raw 01.05 run001](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/results/E34-EPISODE-QUERY-MATCHER/results/run-2026-05-01-001.json): hit@1 `.6111→.3889`, **−22.22pp**, train72/val18, время1.8→5.1ms. Этот отрицательный результат сохраняется.

[Hybrid E34V2 raw 01.05 run001](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/results/E34V2-HYBRID-MATCHER/results/run-2026-05-01-001.json): apparent hit@1 `.7222→.7778`, **+5.56pp**, hit@3 `.8889→.8889`, время10.5→106.7ms, bestAlpha=.10.

Точная source-граница: baseline считается на всех18 val, alpha обучается/подбирается на первых9, enhanced считается на последних9. [E34HybridMatcher L27–59](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/Experiments/E34HybridMatcher.cs#L27-L59). **Это не прямое обучение alpha на enhanced TEST.** Проблема — разные панели baseline/enhanced и включение tuning в enhanced timer. Из двух scalar numbers нельзя восстановить matched baseline на второй половине.

Получатель BM25 — `Index(train)`; kNN-компонент сравнивает query с TRAIN family tags, [L88–111](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/Experiments/E34HybridMatcher.cs#L88-L111). Не наблюдалось обновления LoRA/Transformer. Чтобы проверить полезность исправления этого измерителя, достаточно исходного обученного state, одной фиксированной панели и того же original reader — новые декоративные уроки не нужны. Повторять сейчас старый experiment целиком для красивого положительного числа не требуется.

**Область:** отбор типов эпизодов/инструкций, не качество свободного перевода или генерации программы. E34→E34V2 меняет семейство сравнения и evaluation contract; это не установленная цепочка «та же матрица неудачна → та же функция kNN успешна».

### E43: replay улучшил настоящий частотный learner, не измерял LoRA forgetting

[Raw 01.05 run001](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json): old-domain retention_accuracy `.3226→.3978`, **+7.52pp**;465 old pairs,414 new,177 replay; время `.1→5.8ms`.

Source [L34–55](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L34-L55) строит три отдельные bigram dictionaries: old-only, new-only и new+replay. Writer увеличивает counts в [BuildBigram L89–98](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L89-L98), reader выбирает самый частый successor. Это настоящий усвоенный частотный state; положительный результат не надо обесценивать из-за отсутствия нейронной сети.

Однако noReplay не продолжает old weights, а заново строится только на новых парах. Source не измеряет забывание при последовательном обновлении одного optimizer. Новый-domain quality здесь не оценён; этот пропуск нельзя назвать «без регрессий». Для переноса replay на LoRA требуется отдельный native recipient и первоначально подтверждённый старый навык.

## 4. Что можно и чего нельзя заключить о переносе матрицы в функцию

Уточнение пользователя об успешном переезде в kNN принято как исходное историческое свидетельство; отсутствие точной реконструкции здесь не опровергает его. Прочитанные receipts подтверждают полезные локальные эффекты функций и сохранение соответствующих source файлов в current и трёх sibling snapshots. **Конкретный migration commit, связывающий один и тот же материал, один target metric и обе реализации «матрица→функция kNN», этим подаудитом не установлен.** Поэтому названные выигрыши не выдаются за тот самый пользовательский случай.

Практическая единица восстановления — `учебный материал → writer состояния → конкретная функция чтения → прежний доменный output`. Нельзя переносить только название E09/E26/E43 или старый threshold passed. Сначала проверяется используемый recipient и контракт; для consumer/restore допускается существующий обученный state, если source-зависимость доказана.

## 5. Дедупликация и статус новых гипотез

История E09/E22/E26/E34/E43 и ограничения codec/panel/статических defaults уже присутствуют в full possibleDifs corpus и protected P-карточках. Они **не заявляются новыми открытиями**. Для дедупликации выполнен текстовый поиск по полному локальному narrative corpus555MD, подробно прочитаны выбранные совпадения и active protected C/T snapshots; это не заявление о сплошном чтении всех555 файлов. Текущая проверка добавляет pinned sibling parity и сверку raw/source для семи конкретных файлов.

Новые дополнения этого пакета: самодостаточная маленькая учебная постановка T-TRAIN-CONTEXT и актуальная семантическая классификация C/T по policy20.10. Дополнительная находка `QualityVerifierUnit chosen.Pass=true` no-op оставлена в evidence note [review §4](09-C-T-EXPERIMENT-REVIEW.md#4-verifier-positive-correction--только-наблюдение-кода); из неё не построен дорогостоящий эксперимент без доказанного production correction caller и новизны относительно P56.2.

