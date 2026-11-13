# Карта возможных различий: где искать потерянную пользу обучения

Эта карта охватывает все названные пользователем классы: DPO, LoRA, векторы/токены, KNN, deterministic shortcuts, DPO→под-юниты, реализацию Units и восстановление состояния. Она объединяет прежние находки с шестью конкретными цепочками: пять новых гипотез и локализация одного прежнего общего предположения. [Финальное уточнение новизны](08-FINAL-CORE-DOCS-NOVELTY-CLARIFICATION.md). Полный аудит каждой строки на всех ветках не заявляется: первичная перепроверка этой работы описана в [методологии](06-SOURCES-NOVELTY-AND-REVIEW.md).

## 1. Первый выбор — по реальному пути обучения

| Наблюдаемая проблема | Сначала проверить | Приоритетный маленький опыт |
|---|---|---|
| RP01 изменяет LoRA, но собственные ответы не становятся полезными | Компетентность исходной базы, точную пару/маску, фактический recipient и путь генерации из сохранённого state | [P0 → P4](00-EXTERNAL-DOMAINS-RECOVERY-FIRST.md): сначала consumer/restore, затем два одинаковых урока и одно изменение target set |
| Пропал прежний C# structural KNN | Общие метки, доступные признаки, границы файлов, одинаковый EXAM и подключение настоящего predictor | [P1](00-EXTERNAL-DOMAINS-RECOVERY-FIRST.md), 32 TRAIN occurrences / 16 EXAM |
| Перевод знает слова, но путает роль/порядок/смысл | E25 order memory, E26 context, E27 применение порядка; затем keys и обычный executor | [P2/P3](00-EXTERNAL-DOMAINS-RECOVERY-FIRST.md) и [перепроверенные Unit seams](01B-KNOWN-UNIT-TRANSFORMATION-GAPS.md) |
| Отдельный Unit отвечает правильно, а общая задача не исполняется | Candidate eligibility, source associations, aggregation и фактический handoff | Новые [KU-01](02A-NEW-SEGMENT-AGGREGATION-GAP.md), [KU-02](02B-NEW-CANDIDATE-ELIGIBILITY-GAP.md) |
| После принятого дополнения память хуже выбирает задачу | PT top2 ambiguity и извлечённые смысловые сигналы | Новые [PT-01/PT-02](03-NEW-PROMPT-PATTERN-GAPS.md), первоначально 0 updates |
| Автономный batch использует другие веса или replay, чем предполагалось | Есть ли реальный caller `16.AutonomousLearning`, provider weights и admission cohort | [AUT-WEIGHT-01/AUT-REPLAY-02](04-AUTONOMOUS-LEARNING-CONDITIONAL-GAPS.md), P2 conditional |

`Domain` в API может быть ключом адаптера, меткой вида задачи или направлением перевода. Это не одно и то же. В протоколе сначала называют предмет (документация C#, перевод EN→RU, перевод RU→EN, оркестрация), затем профиль закона внутри него (исключения, отрицание, порядок, роли). AST-признаки C# и правила русского рода не распространяются автоматически на SQL, Figma или английскую прозу.

## 2. Сводный каталог известных вопросов и быстрых различающих проверок

Статус `KNOWN` означает сохранённый вопрос из прежних отчётов. Он не означает ни доказанный дефект каждого caller, ни необходимость применять исправление. Ссылки ведут к исходному владельцу; новые записи отдельно перечислены в следующем разделе.

| Область / возможное отличие | Что именно различать | Малый опыт и смысл отрицательного результата | Предыдущий владелец |
|---|---|---|---|
| **Содержательное качество DPO** | Корректная chosen/rejected пара против формально валидной, но шаблонной/неоднозначной; usable pair против просто winner | 8 пар одного закона: слепая оценка обеих сторон по исходному материалу; TIE/BOTH_BAD не принуждать к preference. Только затем сравнивать два одинаковых tiny curricula. Плохой материал не исправляется ростом rank | [Данные и оценка][k14], [DPO/data/state][k02] |
| **Objective и длина ответа** | Sum response logP, mean logP, число target tokens и reference-relative margin | На одних сохранённых per-case scores восстановить sums/means/lengths. Менять reduction отдельно от tokenizer/LR. Различие числа не доказывает неправильный ответ | [DPO/data/state][k02], [граница ответа][k31] |
| **Prompt/response mask** | Реальные tokens и response-only targets; presence legacy helper против его actual use | Одна точная пара: source bytes → BPE → IDs → target mask → выбранный overload. Не возвращать старые side markers Chosen/Rejected и hash truncation ради более высокой старой метрики | [Exact DPO][k02], [gradient chain][k41] |
| **Место и ёмкость LoRA** | Q/V-only adaptation поверх данной базы; rank/alpha/dropout; trained target set против всех перечисленных parameters | [P4](00-EXTERNAL-DOMAINS-RECOVERY-FIRST.md): один target-set contrast, одинаковые policy/reference/tokenizer/данные; считать реальные deltas. Изменение rank вместе с base или decoder не идентифицирует причину | [Neural donor][k25], [code-trace donor][k28] |
| **Autograd и scalar teaching vector** | Связность logits→A/B; frozen base передаёт gradient; vector как scalar не учит собственную семантическую проекцию | На адресном recipient проверить analytic chain / уже сохранённые deltas. Два вектора с одинаковым числом ненулевых элементов дают один multiplier; если ожидалась другая функция, нужен другой конкретный learner, не переименование поля | [gradient chain][k41], [teaching vector][k30] |
| **Стартовая база и декодирование** | Случайный cold state, компетентная база, head expressivity, stop/length и actual answer path | До updates 4 коротких собственных ответа и identity checkpoint. После — те же критерии на новых запросах. Ни DPO loss, ни факт изменения весов не заменяют этот ответ | [Student→generation][k12], [answer boundary][k31] |
| **BPE identity / re-train** | Один словарь/merge version для памяти и запроса; изменения кода tokenizer против изменения индекса | Сохранить одну старую пару и её IDs; принять новый материал; повторить старый запрос и cold load. Пересборка embedding/KNN должна соответствовать версии признаков | [BPE identity][k12b], [pretraining boundaries][k69] |
| **Длинный контекст** | 64-token embedding cap против full lexical text; pooling/prefix/UTF-8 tail против decoder limit | Два запроса с одинаковым префиксом и различным поздним условием; записать каждый channel и конечный выбор. Lexical может компенсировать vector loss | [Первичный разбор пяти Units](01B-KNOWN-UNIT-TRANSFORMATION-GAPS.md), [prefix][k21], [UTF-8][k62] |
| **KNN representation / distance** | AST key/table, structural metric, label codec, domain scope и reuse разных signatures | Общие TRAIN/EXAM occurrences и label mapping, допустимые на inference признаки; один distance factor. Matrix FAIL не отменяет другой distance algorithm | [P1](00-EXTERNAL-DOMAINS-RECOVERY-FIRST.md), [E09 transfer][k38], [structural scope][k73] |
| **KNN fusion и память** | Реальный vector сигнал против lexical alias, BM25 normalization, pattern score, candidate support и независимые lessons | 2–4 controlled neighbors; record raw channels, shortlist и output. Не считать несколько signatures одного урока независимой поддержкой | [memory/fusion][k15], [replay consumer][k52], [signature alias][k60] |
| **DPO→под-юниты** | Сохранились ли выбранное решение, rejected contrast, роли, condition и typed input/output relation | 2 задачи одному subphase с различным смыслом: source pair→projected pair→stored output→обычное исполнение. Generic label допустим только когда контракт требует label | [Первичная Unit crosswalk](01B-KNOWN-UNIT-TRANSFORMATION-GAPS.md), [DPO→subunits][k13], [roles/keys][k44] |
| **Ключи translation memory** | Domain/direction/context/RuleTags; коллизия learned correction и различие training/inference key builders | Один grammar contrast, одна tokenizer correction, два неидентичных новых source. Exact-source shortcut не должен маскировать проверяемый KNN seam | [Unit crosswalk](01B-KNOWN-UNIT-TRANSFORMATION-GAPS.md), [language scope][k23], [composition][k53] |
| **Порядок и morphology** | Learned order E25; fixed reorder E27; punctuation alignment; ограниченная область gender aliases | POS fixture отдельно от raw language; noun/homonym negatives отдельно от правильных verb pairs. Сохранять полезное доменное правило, ограничивать неверную область его применения | [P2](00-EXTERNAL-DOMAINS-RECOVERY-FIRST.md), [Unit crosswalk](01B-KNOWN-UNIT-TRANSFORMATION-GAPS.md) |
| **Арбитр / feedback** | Warning memory, positive target, quality veto и принятие нового урока — разные решения | Один правильный и один неправильный ответ на одинаковую задачу; проверить зависимость verdict от самого ответа и дальнейший consumer. Наличие score без learning recipient не замыкает цикл | [feedback consumer][k75], [E52 veto][k40], [родительский F amendment][f-arbiter] |
| **Состояние Unit/модели** | Parameters/adapters, optimizer/scheduler/RNG/counters, tokenizer/schema, memory/index/graph, acceptance identities | [P0/P5](00-EXTERNAL-DOMAINS-RECOVERY-FIRST.md). Cold read и один одинаковый следующий update. Защитный отказ incompatible state отличать от silent reinitialization | [unit identity][k19], [HNSW continuation][k63], [реальный memory continuation][k78] |
| **Curriculum/replay и ранняя кривая** | Выбранные source IDs/порядок, single-pair novelty, удержание старого закона; early gain против final-only FAIL | Один новый шаг, старые4/новые4 EXAM; одинаковый фактический budget. Раннюю пользу E48 не удалять при равной последней точке | [E43][k68], [E48][k54], новые [AUT](04-AUTONOMOUS-LEARNING-CONDITIONAL-GAPS.md) |
| **Оценка доказательств** | Тот же вопрос/denominator, independent family, native output, saved state и final utility | Сначала реконструировать историческую панель. Flag PASS, checkpoint count, source hash или unit accuracy не равны правильному свободному ответу | [историческая глава](00-EXTERNAL-DOMAINS-RECOVERY-FIRST.md), [validation programme][k81] |

## 3. Шесть конкретных цепочек: пять новых гипотез и одна локализация

| ID | Новый механизм | Отношение к донору / RP01 | Документ |
|---|---|---|---|
| **KU-01** | Different source segment IDs превращают согласованные learned decisions одного target в конфликт | Прямое отличие learned aggregation от старого baseline merge; trained seam условно подключён | [02A](02A-NEW-SEGMENT-AGGREGATION-GAP.md) |
| **KU-02** | Global relative floor удаляет allowed edge target до позднего membership filter | Новое взаимодействие старого scorer с более узким current consumer; до learned inference | [02B](02B-NEW-CANDIDATE-ELIGIBILITY-GAP.md) |
| **PT-01** | Accepted semantic duplicate с другим ID обнуляет top2 margin | Нынешняя память PromptToTask; прямой neural RP01 не установлен | [03](03-NEW-PROMPT-PATTERN-GAPS.md) |
| **PT-02** | Generic substring gate создаёт missing_gates до выбора памяти | Нынешний fixed feature extractor; эффект зависит от loaded patterns | [03](03-NEW-PROMPT-PATTERN-GAPS.md) |
| **AUT-WEIGHT-01** | Single-domain admission + GroupBy(Domain) делает maxWeight=1 и cap исходного веса | Унаследованное ограничение усилено current guard; built-in Weight=1 не страдает | [04](04-AUTONOMOUS-LEARNING-CONDITIONAL-GAPS.md) |
| **AUT-REPLAY-02** | Cohort попадает в replay до последующего quality reject адаптера | Унаследованный порядок; failed adapter не означает bad lesson; нет текущего non-test composition root | [04](04-AUTONOMOUS-LEARNING-CONDITIONAL-GAPS.md) |

Ни одна из этих шести цепочек пока не имеет нового native измерения прироста качества. Первые четыре показывают способы потерять полезность уже имеющейся памяти/решения; последние две меняют потенциальный учебный поток. Это разные места причинной цепи, и их нельзя складывать как шесть независимых причин одного неудачного запуска.

## 4. Что сохранить при восстановлении старого успеха

У `external_domains` старый `BackwardLogProbs` записывает gradient в logits, но не вызывает дальнейший `Backward`; старые высокие числа сами по себе не оправдывают перенос этого trainer. В текущем исходнике есть явный `logits.Backward(logits.Grad)`, response-only exact material и ограниченный фактический LoRA-recipient. Это сильное контрдоказательство версии «всё нынешнее DPO не обновляет никакие параметры». Сохранять эту исправленную цепь при проверке места адаптации. [Old backward:220–263][old-backward] [Current backward:1072–1120][new-backward] [Primary current review][k41]

Возвращать нужно **полезную функцию вместе с её представлением, памятью, задачей и consumer**, а не весь snapshot. Контекст AST может улучшать structural KNN и одновременно быть бесполезен в другой матрице. Точный переводческий словарь полезен, даже если он ещё не обучается из DPO. Явный integrity guard полезен, но ошибочный semantic shortlist способен лишить trained unit возможности ответить. Эти различия и определяют границы опытов.

Родительские источники поддерживают такое разделение: F требует scoped KNN retrieval и памяти positive/negative/warning; G04 задаёт learned edge authority без утечки deterministic router output; G06 сохраняет source/target связи many-to-many; G07 сохраняет точные chosen/rejected, происхождение, роли и split. Это архитектурные требования, а не runtime receipts. [F][f-arbiter] [G04][g04] [G06][g06] [G07][g07]

[k02]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/02-dpo-lora-tokenization-and-state.md
[k12]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/12-student-update-to-generation-path.md
[k12b]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/12-bpe-retrain-identity-and-consumer-causal-gap.md
[k13]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/13-domain-context-and-dpo-subunits.md
[k14]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/14-data-quality-and-evaluation-alignment.md
[k15]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/15-memory-growth-fusion-and-bpe.md
[k19]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/19-unit-state-identity-and-partial-learning.md
[k21]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/21-transformer-knn-prefix-and-fallback.md
[k23]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/23-language-context-and-learned-rule-scope.md
[k25]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/25-historical-neural-learning-donor.md
[k28]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/28-code-trace-learning-donor.md
[k30]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/30-teaching-vector-adamw-and-current-recipient.md
[k31]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/31-answer-boundary-and-decoding-scope.md
[k38]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/38-e09-transfer-labels-and-opt-in-consumer.md
[k40]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/40-e52-contract-success-and-answer-independent-veto.md
[k41]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/41-current-dpo-gradient-chain-source-review.md
[k44]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/44-structured-lessons-roles-keys-and-word-alignment.md
[k52]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/52-replay-ranking-consumer-and-proxy-parity.md
[k53]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/53-translation-composition-and-direction-consumers.md
[k54]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/54-e48-early-learning-and-probe-gradient-boundary.md
[k60]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/60-knn-signature-alias-and-candidate-budget.md
[k62]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/62-utf8-tail-hypothesis-and-retained-student-answers.md
[k63]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/63-hnsw-random-state-and-continuation-consumers.md
[k68]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/68-e43-replay-boundaries-and-reachable-quality-question.md
[k69]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/69-pretraining-data-boundaries-and-consumer-scope.md
[k73]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/73-rare-class-evidence-and-structural-feature-scope.md
[k75]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/75-translation-feedback-learning-consumer.md
[k78]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/78-translation-restored-memory-positive-receipt.md
[k81]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/81-validation-program.md
[old-backward]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/11.DPO/DpoTrainer.cs#L220-L263
[new-backward]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/11.DPO/DpoTrainer.cs#L1072-L1120
[f-arbiter]: https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/llm%20dynamic%20flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md
[g04]: https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/dpo%20legal%20build/codex-plan-04-dynamicgraph-machine.md
[g06]: https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/dpo%20legal%20build/codex-plan-06-async-combinations.md
[g07]: https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/dpo%20legal%20build/codex-plan-07-dpo-builder.md
