# Реестр измеренных результатов и кандидатов исправления

Дата: 20 октября 2026. Это запись существующих результатов и проверка текущего состояния оригинала. **Нового эксперимента здесь нет.** Полезное измеренное улучшение признаётся успехом своей метрики/домена; ухудшения, стоимость и границы вывода показаны рядом. Отсутствие роста общей accuracy не превращает полезный результат в неудачу.

## Состояние кандидатов

| ID / домен | Что доказано | Состояние текущего оригинала | Решение этого аудита |
|---|---|---|---|
| N-KU-01 / C# | Изменение teacher label до native Train улучшило требуемый совет на фиксированной панели | DoStatement ещё отсутствует в ActionLabelGenerator.Label; результат уже зарегистрирован внешними документами | **Успешный измеренный кандидат для решения пользователя**, без нового запуска и без product patch |
| embedding-sft8-01 / SFT по сохранённой ENRU панели | Embedding получает gradient/update; средний online loss улучшился; реальные восемь updates | Backward в текущем Embedding уже есть | Сохранить положительный результат как **уже внесённый fix**; повторное внедрение не предлагать |
| dpo-mask-live8-01 / legacy fractional DPO | Производная согласована с маской; полезное исправление прошло через восемь настоящих LoRA updates | targetScale уже учитывается в BackwardLogProbs | Сохранить как **уже внесённый fix**, ограниченный проверенным профилем |
| translation-memory-ingress-01 / EN→RU | Доставка phrase_mappings меняет реальные обученные maps и повышает composition quality | Current Tools reader читает поле; builder передаёт его в ExecutorOutput | **Уже внесённый fix**; оставить gain и false-accept regression рядом |
| P125-enru-memory-01 / EN→RU | Реальное обучение памяти Executor изменяет ответы обычного Flow; no-train и target-corruption controls | Существующий native learning path | Подтверждённый результат обучения; не считать отсутствие neural update отсутствием обучения |
| P25-enru-cold-01 / EN→RU | Cold loader сохраняет конкретное обученное состояние и его поведение | Проверен named checkpoint; не все старые схемы | Допустимый learning-dependent результат; **сохранение**, не новый gain accuracy |
| E25 / POS patterns | Обученная KNN классификация улучшает retained validation accuracy | Исторический donor; current production equivalence не доказана этим receipt | Сохранить доменный успех и точный caller; не выдавать его за нынешний общий translator |
| E43 / frequency bigram memory | Replay улучшает retained old-transition accuracy | Историческая memory implementation, не текущий LoRA optimizer | Сохранить успех конкретной retention задачи |
| RMS-COORD-ALLOC-01 / C# SFT pilot | Подтверждён лишний per-element coordinate allocation в native learning path | Original содержит FlatToIndices; эксперимент не выполнялся | **Новая проверяемая гипотеза**, пока не успешный кандидат по измерению |

Точные neural источники и перерасчёт — [core_learning.md](core_learning.md) и [neural-history-recalculation.json](neural-history-recalculation.json). История, raw recount и source writer/reader — [historical_knn.md](historical_knn.md), [historical-recount.json](historical-recount.json). Полное состояние каталога — [experiment_audit.md](experiment_audit.md).

## N-KU-01: измеренный кандидат ещё не внедрён

**Original:** 12.SemanticCompiler/Prediction/FileActionKnn.cs, ActionLabelGenerator.Label, строки 50–53; Git blob **ef7a88a3c4c2540569b9ce6f204d8e4750875501**. В проверенном коде присутствуют ForEachStatement, ForStatement, WhileStatement, но нет DoStatement. Между ab583032 и 65d03a73 этот файл не менялся. [Original method][nku-original].

**Предлагаемая точечная коррекция:** включить DoStatement в существующее условие распознавания уже имеющегося цикла. Это исправление teacher до формирования тренировочных labels, а не замена результата Predict после обучения. Полный diff уже сохранён исходным экспериментом: [product-change-proposal.diff][nku-proposal]. Этот аудит diff не применяет и новый запуск не назначает.

**Учебная цепочка:** C# files → ActionLabelGenerator.Label + FeatureVector → FileActionKnn.Train → четыре пары в learned memory → native Predict на четырёх exam inputs. A и B стартуют с пустой памяти; каждое основное плечо делает один Train. Neural updates = 0, но обучаемое состояние и его обычный reader реальны. [Raw A][nku-a], [raw B][nku-b], [gold][nku-gold].

| Полезный показатель | A | B | Вывод |
|---|---:|---:|---|
| Выполнение заданной рубрики до обучения | 0/4 | 0/4 | Одинаковое исходное отсутствие результата |
| После обучения | 1/4 | 3/4 | Успех: +2 ответа из 4 по зафиксированной рубрике |
| Ложный ADD_LOOP при существующем цикле | 3/3 | 0/3 | Успех по типу ошибки |
| Требуемый ADD_LOOP на KQ4 | 1/1 | 0/1 | Ухудшение, обязательное для решения о переносе |
| Запомненные учебные пары | 0→4 | 0→4 | Одинаковый объём native Train |

Для KQ1–KQ3 gold принимает существующий совет, отличный от ADD_LOOP. Поэтому **3/4 не означает доказанную оптимальность каждого совета или 75% качества всего semantic compiler**. k3 majority на четырёх training rows способно сместить выбор к преобладающему классу; удержание нужного ADD_LOOP остаётся конкретным ограничением этого кандидата.

Наблюдаемые allocations учебного интервала Label + FeatureVector + Train составили **1 179 032 → 1 169 344 bytes**; время **195,8347 → 123,8038 ms**. Время четырёх последующих ответов выросло **22,4050 → 26,0613 ms**. В retained report отдельно указаны лишние warmup/equivalence/BA действия вне основного бюджета. При обратном порядке время A/B равно приблизительно **98,7/133,1 ms**, поэтому стабильное причинное ускорение одной правки не установлено. Эти измеренные значения сохраняются; повторение ради красивой скорости не предлагается. [Полный report][nku-report].

comparison.json в Git — LFS pointer, его payload не получен. Ключевые числа независимо восстановлены из полных arm-A.json, arm-B.json и gold.json. [Пересчёт без запуска learner](nku-retained-recheck.json).

**Статус регистрации:** N-KU уже появился в protected required-changes.md до окончания этого аудита. Мы не изменяли тот реестр. Здесь добавлено независимое подтверждение и указано, что original fix ещё не внесён; новая заявка на «впервые найденный» опыт не создаётся. [Поздний planning статус][late-registration].

## EMB и fractional mask: нельзя повторно «исправлять» уже исправленный код

### Embedding

Retained embedding-sft8-01 сравнивает восемь настоящих SFT updates при одинаковых inputs. Средний online train loss **7,0169490625 → 6,9877417**, изменяемые tensors **54/55 → 55/55**, embedding coordinates начинают получать update. Экзамен остаётся **0/24 → 0/24**.

Сумма measured training-step intervals **24 505,3504 → 24 391,5929 ms** локально уменьшилась. При этом total harness приблизительно **142,08 → 149,44 s**, peak RAM **1,738 → 1,827 GB** выросли. Online loss измеряется в процессе шага и не является одинаковым post-training heldout loss; train time и полное время — разные области. Эти различия не отменяют полезный loss/gradient результат, но должны сопровождать его. [Retained report][emb-report], [current Embedding][emb-source].

**Current:** Embedding создаёт backward graph и накапливает gradient. Кандидат классифицирован как уже внесённый. В frozen-base q/v-LoRA обучаемость embedding не назначена; перенос full-model вывода на этот путь был бы ошибкой.

### Fractional response mask

Retained dpo-mask-live8-01 показывает derivative agreement **126/162 → 162/162** и восемь реальных updates. Экзамен **0/24 → 0/24**. Средний online loss ухудшился примерно на **0,00000010625**; сумма training-step intervals уменьшилась **36 113,9900 → 34 532,0568 ms**. Полные harness time/CPU/peak RAM также показаны в исходном отчёте и [нейронном разборе](core_learning.md); process-lifetime allocations нельзя переименовывать в allocations одного Train. [Retained report][mask-report].

**Current:** BackwardLogProbs умножает градиент на targetScale; это исправление уже присутствует. Результат относится к fractional legacy profile. В exact RP01 с binary mask та же ошибка могла не проявляться, поэтому этот успешный fix не становится доказанной общей причиной неудачи RP01.

## Обучение памяти и доставка subunits

P125 и translation-memory-ingress должны оставаться отдельными результатами.

В **P125** проверяется приобретение способности от пустой памяти: B получает восемь правильных уроков и меняется **0/24 → 19/24**; A без уроков остаётся **0/24**; C с переставленными targets получает **13/24**. Это показывает зависимость ответа от того, чему учили. Ordinary Flow читает действительные exact/KNN/word/phrase maps. [P125 report][p125-report].

В **ingress** оба плеча получают по восемь уроков, но старый reader теряет phrase_mappings. Доставка этого поля даёт **11/24 → 19/24**, композиции **0/8 → 8/8**, learned phrase entries **0 → 16**. Accepted wrong растут **1 → 3**, correct/answered падает **11/12 → 19/22**. Current reader/builder уже исправлены. [Ingress report][ingress-report], [native source chain и recount](historical_knn.md).

В **cold restore** получены **24/24 одинаковых ответов**, включая **19 correct** и известные ошибки. Это сохранение доказанного результата обучения, допустимое без нового Train. Здесь прочитан retained comparison; отдельного нового пересчёта cold raw не было. [Cold report][cold-report].

## Что является следующим решением, а что не является

Этот реестр не даёт команд на повторное выполнение успешно задокументированных опытов. Полезные результаты уже достаточны, чтобы представить кандидат пользователю вместе с регрессиями. Product adoption, расширение панели и новый experiment run — разные отдельные назначения.

RMS остаётся NOT_RUN. Его будущая проверка задана в [точной карточке](RMS-COORD-ALLOC-01-CARD.md); значений A1/B1 для неё пока нет. Не следует добавлять его в список «доказанно улучшает обучение» до результата.

**Немедленный stop/next обязателен:** после команды перейти к следующему опыту не продолжать этот реестр, не читать прежние raw, не исправлять commands/report и не делать финальную публикацию прошлого опыта. После команды остановиться прекратить действия; сообщить лишь известное состояние без новых проверок. Старые результаты сохраняются как есть.

[nku-original]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/12.SemanticCompiler/Prediction/FileActionKnn.cs#L46-L55
[nku-proposal]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/product-change-proposal.diff
[nku-a]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-A.json
[nku-b]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-B.json
[nku-gold]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/gold.json
[nku-report]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md
[late-registration]: ../2026-10-20-learning-candidates-and-causal-gaps/05-final-publication-delta.md
[emb-report]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/report.md
[emb-source]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/00.TensorCore/Layers/Embedding.cs
[mask-report]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/report.md
[p125-report]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md
[ingress-report]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md
[cold-report]: https://github.com/lubochka/llm_mvp_core/blob/65d03a73a76810a0425a41c33d3c7eeaf4cdbc48/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md

