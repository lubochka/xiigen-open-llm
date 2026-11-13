# Новая гипотеза: русский маркер качества доходит до builder, но исчезает при обучении и вызове verifier

**Статус:** исходный механизм подтверждён чтением закреплённого кода и выполненной ограниченной source-equivalent проверкой; native C# обучение, independent CODEX exam и измерение продуктового прироста — **NOT_RUN**. **Один новый механизм**, два места воздействия одной функции. Репозиторий core и каталог `docs/plans/replanning-possible-diffs` не изменялись.

## Что именно упущено

В `QualityVerifierUnit` функция `LooksLikeMannerMarker` содержит окончания mojibake и `ly`, но не нормальное кириллическое `о`. В соседнем `TranslationStructuredBatchBuilder` одноимённая функция содержит и нормальные русские окончания. Некоторые настоящие русские наречия — здесь **«тихо» и «быстро»** — не распознаются coarse tokenizer как `ADV`, а остаются `NOUN/UNKNOWN`. Когда source содержит обычные теги русского подлежащего и прошедшего времени, fallback verifier обязан пройти через повреждённый predicate. Маркер не находится.

Сгенерированный builder урок при этом вполне содержателен: для неверного candidate выбран правильный verdict `Pass=false`, issue требует `quietly` либо `quickly`. Но после `TrainOnExample` требование не попадает ни в одну из трёх таблиц verifier. Даже если правильные marker rules уже есть в сохранённом состоянии, тот же predicate не найдёт их на inference. Это объясняет возможное отсутствие эффекта **конкретного типа структурированных уроков**, а не доказывает общую причину ухудшения RP01 или ошибку LoRA.

## Прямая цепочка и immutable доказательства

Исследованный current commit: `eb05219c27663835ce927c7bf97cf1274c07cddb`.

| Звено | Исходник и следствие |
|---|---|
| Producer разных unit-уроков | [builder:96–145](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L96-L145): `requiredTerm` берётся из chosen; для неверного candidate создаётся verifier chosen `Pass=false` с `expected '{requiredTerm}' for translated quality marker`. [181–189](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L181-L189) сначала извлекает английское слово на `ly`. |
| Рассогласованный marker predicate | [builder:502–525](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L502-L525) содержит обычное `о`, выбирает `ADV`, затем UNKNOWN token при разрешённом fallback. [verifier:1265–1299](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationTrainedUnits.cs#L1265-L1299) содержит только mojibake и `ly`. |
| Почему пример реально попадает в этот fallback | [tokenizer allowlist:88–96](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationSemanticTokenizer.cs#L88-L96) не содержит `тихо/быстро`; [ClassifyPos:258–294](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationSemanticTokenizer.cs#L258-L294), [adverb:414–430](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationSemanticTokenizer.cs#L414-L430), [UTF8 adverb:574–585](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationSemanticTokenizer.cs#L574-L585) не распознают их окончания. В исследуемых фразах они `NOUN`, без известных concept stems. |
| Почему RuleTags не дают обойти плохую проверку | [BuildRuleTags:153–194](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationSemanticTokenizer.cs#L153-L194) добавляет теги для `он/она` и распознанного глагола прошедшего времени. [Russian verb:543–564](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationSemanticTokenizer.cs#L543-L564) покрывает выбранные глаголы. `RuleTags.Count==0` ложно. |
| Почему не спасает другой parser | [TryParseIssue:1216–1247](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationTrainedUnits.cs#L1216-L1247) ищет имена concept enum в остатке issue после quoted term. Строка ` for translated quality marker` не содержит ни одного [SemanticConcept:59–78](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationUnitContracts.cs#L59-L78). Значит первый parser даёт false, required-term parser даёт true. |
| Где пропадает состояние | [TrainOnExample:1120–1143](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationTrainedUnits.cs#L1120-L1143): `qualityMarkerIssue=true` отключает signature fallback; пустой `ExtractSourceMarker` отключает marker insertion. Concept rule также не создаётся. |
| Где пропадает применение уже выученного | [Verify:1053–1069](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationTrainedUnits.cs#L1053-L1069) извлекает source marker тем же методом и только затем читает marker rules. |
| Реальные вызывающие пути | [Batch029 runner:128–180](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationBatch029Runner.cs#L128-L180) строит structured batch и вызывает машину; [machine:89–145](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationMachine.cs#L89-L145) зовёт `TrainOnExample`, учитывает unit/rows независимо от delta таблиц. [composition:724–757](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationMachine.cs#L724-L757) передаёт verifier адаптерам. [adapter:124–139](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationDirectionAdapters.cs#L124-L139) вызывает `Verify`; high-confidence phrase-map имеет уже известное исключение. |

Checkpoint **не объявляется сломанным**: [export/import:1173–1213](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationTrainedUnits.cs#L1173-L1213) сохраняет/восстанавливает marker rules. Пустое состояние после неусвоенного урока может быть добросовестно сохранено; это иной механизм.

## История и новизна относительно существующих документов

На живой соседней ветке `PrepareToGPL`, закреплённой SHA `31346d8928be4469b01aaf313780e8f950ff91e2`, [весь TranslationTrainedUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationTrainedUnits.cs#L1265-L1299) имеет **тот же Git blob `1742abfc9fb617dfc66e4d618e0c169da01a6ccc`**, что current. [Исторический builder:502–525](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L502-L525) уже содержит обычное `о`. Следовательно, seam **унаследован**, а не установлен как внесённая RP01 регрессия. Сам факт существования на успешной ветке не означает отсутствие ущерба: использованные там слова могли попадать в `ADV`, а уроки и маршруты могли быть иными. Успешность именно этого микросценария на истории не проверена.

Full-text novelty проверена по локальным снимкам 492 planning Markdown и 242 core experiment Markdown, включая вложенные minis/P-карты. Точного `LooksLikeMannerMarker` и формулировок о normal-Cyrillic marker loss там не найдено. Ближайшие существующие темы:

- `28-NEW-VERIFIER-ISSUE-ROLE-PARSER-AND-CONTEXT-LEAK` и P52.3: перестановка concept/context enum-parser. Здесь semantic parser корректно возвращает false и не является вмешательством.
- P56.1–P56.10: контекст, AND-конфликты, direction namespace, Contains, bypass/confidence. Здесь отсутствует сама запись требования и marker lookup; эти правила не меняются.
- P56.11: training tokenizer hardcodes `ru`. Здесь source уже **ru**, направление и DTO не меняются.
- `28-NEW-UNICODE-CANONICAL-KEY-AND-SPANISH-N-FALSE-MOJIBAKE`: exact-source normalization/NFC/NFD и испанская `Ñ`. Здесь нормальные русские слова, без canonical-key collision.
- `29-NEW-FIRST-TOKEN-PSEUDOALIGNMENT-ORDER-CONTAMINATION`: неверный target marker при отсутствии `ly`. Здесь target markers `quietly/quickly` корректны; меняется только кириллический selector verifier.

**Coverage:** отдельного соответствующего вмешательства в проверенном read-only core-каталоге не найдено. P56.11 не покрывает этот RU→EN вариант. Требуется новая малая карточка, а не отметка «покрыто P56» по общей теме verifier.

## Малый эксперимент, который отделяет усвоение от применения

**Контракт задачи:** сохранить явно выраженный способ действия в RU→EN; пропуск `тихо/быстро` — ошибка на этой панели. Ниже — иллюстративные fixtures для разработки проверки. Они **не выдаются за DPO, полученный внешним учителем**, и не являются результатом независимого экзамена.

Две TRAIN-пары, по одной на маркер; `HeldOut=false`. Для builder rejected side включает именно неверный перевод и `[WRONG: adverb_omission]`, без PHRASE/AGREEMENT extras.

| TRAIN frame | Source | Translation chosen | Translation rejected |
|---|---|---|---|
| door_open | Он тихо открыл дверь | He quietly opened the door | He opened the door |
| bag_pack | Она быстро собрала сумку | She quickly packed the bag | She packed the bag |

Из каждого реальный builder создаёт verifier lesson на **ошибочном** candidate. У verifier выбранный output — **`Pass=false`**, Issues `expected 'quietly'/'quickly' for translated quality marker`, correction — правильный перевод; rejected verifier output — `Pass=true`. Не переворачивать human preference только из-за слова FAIL.

EXAM фиксируется до изменений, не используется при training; оба кандидата каждого source остаются вместе в одном frame-family. Те же два выученных manner words, новые verb/object frames. Это перенос правила на иной текст, **не unseen-word generalization**.

| EXAM frame | Source | Candidate | Независимый gold Pass |
|---|---|---|---|
| letter_read | Он тихо прочитал письмо | He quietly read the letter | true |
| letter_read | Он тихо прочитал письмо | He read the letter | false |
| note_write | Она быстро написала записку | She quickly wrote the note | true |
| note_write | Она быстро написала записку | She wrote the note | false |

**Вмешательство B:** только добавить обычное кириллическое `о` в verifier `LooksLikeMannerMarker`, сохраняя существующие suffixes, tokenizer/POS, данные, тексты кандидатов, target-term extraction и остальные правила. Эквивалентный производственный кандидат — согласованный predicate builder/verifier; его полный эффект нельзя смешивать с дополнительной POS-моделью. `о` покрывает и выбранные более длинные русские окончания. Простое «добавить тихо/быстро в tokenizer allowlist» меняет другой слой и в этот A/B не входит.

### Основной опыт: 16 Verify, 4 TrainOnExample

Два независимых пустых verifier: A со старым helper, B с единственным Cyrillic fix. В каждом плече проверить четыре EXAM до обучения, предъявить одинаковые два урока в одинаковом порядке и повторить те же четыре EXAM. Обозначения: A0/B0 — до обучения, A1/B1 — после. **Итого 4 состояния × 4 кандидата = 16 Verify; 2 урока × 2 плеча = 4 TrainOnExample.** Это два уникальных урока, а не четыре разных. Разделять «до» между плечами для экономии вызовов не требуется.

| Состояние | Версия | Обучение | Прогноз marker rules | Прогноз правильных verdict / 4 |
|---|---|---|---:|---:|
| A0 | Старый helper | До | 0 | 2 |
| A1 | Старый helper | После двух уроков | 0 | 2 |
| B0 | Cyrillic fix | До | 0 | 2 |
| B1 | Cyrillic fix | После двух уроков | 2 | 4 |

Основное сравнение — `(B1−B0)−(A1−A0)` на одних и тех же четырёх judgments, вместе с delta learned marker rules, false accepts и false rejects. Прогноз: A не исправляет ни одного verdict, B исправляет два ошибочных acceptance, сохраняя два правильных. Это совместный эффект исправленного helper в обучении и применении; отдельный вклад каждого места основной опыт ещё не измеряет. Native результаты **NOT_RUN**.

### Необязательное продолжение: разделить запись и lookup за ещё 8 Verify

Один helper участвует при записи и чтении. Если после основного опыта требуется причинный разбор этих двух мест, к готовым состояниям A1/B1 добавить только два перекрёстных чтения: состояние после A-training прочитать B-selector, состояние после B-training — A-selector. Это **ещё 2 состояния × 4 кандидата = 8 Verify, 0 новых TrainOnExample**; весь выбранный основной+диагностический набор составит **24 Verify и 4 TrainOnExample**. Данные и learned tables не менять; служебный selector seam должен оставлять все прочие вычисления прежними. Export/import для этого не обязателен и не смешивается с проверкой persistence.

Ниже сохранены четыре post-training ячейки выполненного **source-equivalent** probe. Диагональные ячейки соответствуют A1/B1 основного опыта; перекрёстные — дополнительным восьми вызовам, если это продолжение отдельно выбрано.

| Training selector | Evaluation selector | Ожидание marker rules | Ожидание верных judgments / 4 |
|---|---|---:|---:|
| A старый | A старый | 0 | 2 |
| A старый | B Cyrillic fix | 0 | 2 |
| B Cyrillic fix | A старый | 2 | 2 |
| B Cyrillic fix | B Cyrillic fix | 2 | 4 |

Это **результат source-equivalent probe / прогноз native**, а не native измеренное качество. Все четыре числа нельзя представлять как опубликованный прирост обучения. Только при выборе диагностического продолжения сравнение A/B train при одном B-eval выделяет усвоение, а A/B eval при одном B-train — lookup. До обучения оба evaluator на пустом состоянии должны принять все четыре candidate: 2 верных, 2 ошибочных acceptance. Считать только четыре judgments, не проценты «большой выборки».

### Как быстро получить реальное доказательство

1. Зафиксировать external teacher outputs, approved train pairs и независимый gold EXAM. Разделить TRAIN/EXAM по указанным frame families до материализации sub-units; record hashes. Два кандидата одного EXAM source никогда не делить между train/test.
2. В native тестовом runner вызвать настоящий `BuildRuEnGeneralBatch`, проверить фактические verifier inputs, required terms, POS, RuleTags, identifiers и polarity. Взять **только два verifier урока**, чтобы изменение semantic/executor unit не стало вторым фактором. Если не получены требуемые исходные условия — `NO_CONTRAST`, сначала исправить fixture protocol, не код по ожидаемому gold.
3. Измерить A0/B0: по четыре `Verify` до обучения. Вызвать два настоящих `TrainOnExample` в A и два в B; прочитать delta marker keys/terms и signature/concept rules. В A ожидается ноль во всех таблицах; в B — ровно две marker keys, ноль новых signature/concept rules. Unit coverage или RowsTrained здесь недостаточны. Checkpoint не требуется.
4. Измерить A1/B1: по четыре `Verify` после обучения; записать returned Pass/Issues, false accepts/rejects и counts. Scorer проверяет verdict по отдельному gold, а не повторяет verifier.Contains. Основной опыт заканчивается здесь: **16 Verify + 4 TrainOnExample**. Два верных candidate в панели уже служат контролем сохранения правильных acceptance.

### Отдельные продолжения, не обязательные для малого опыта

- **Разбор записи/lookup:** описанные выше **+8 Verify, +0 TrainOnExample**. С основным опытом — 24 Verify. Не делать cold restore обязательным промежуточным механизмом.
- **Cold restore:** если отдельно требуется проверить сохранение уже найденного эффекта, экспортировать A1/B1, импортировать каждый в новый verifier своей версии и повторить четыре EXAM. Бюджет **2 export + 2 import + 8 Verify, 0 TrainOnExample**; с основным опытом — 24 Verify, а вместе с диагностическим продолжением — 32. Сравнение идёт с уже записанными A1/B1, before повторно не измеряется. Это проверка persistence, не доказательство прежней ошибки checkpoint.
- **Actual ordinary adapter:** отдельный будущий продуктовый вопрос, не часть текущего 16-call протокола. Он требует собственного заранее ограниченного набора actual executor outputs и найденного reader. Для него фиксируются candidate origin/fallback, учитывается high-confidence phrase-map exception, output correctness и coverage считаются отдельно. Меньше ошибочных acceptance с пустым fallback ещё не равно больше правильных переводов.

Optimizer, RNG и LoRA в unit-пробнике не участвуют; добавлять их проверки для этого вопроса не требуется.

Дополнительные контроли `долго` и marker-free contexts не являются обязательными вызовами основного native опыта. В выполненном source-equivalent probe `долго` уже распознаётся ADV. Generic `о` может выбрать существительное: перенос ремонта на весь русский язык потребует отдельной панели ложных маркеров. Конкретная ловушка fixture — **«Она быстро закрыла окно»**: текущий tokenizer классифицирует `окно` как ADV по `но`; обе версии selector выбирают его, и требуемого контраста нет. Это зафиксировано в probe как контроль применимости, не объявлено второй новой гипотезой.

**Falsifier:** actual source tokenizer выбирает другой ADV; lesson идёт через concept issue вместо quality marker; trace не достигает обученного verifier; B не увеличивает осмысленные learned rules и не улучшает основной before/after результат; либо новые false rejects компенсируют пользу. Если отдельно выбран разбор ingress/lookup, его дополнительный falsifier — отсутствие прироста при одном evaluator. Тогда `NO_CONTRAST` или `NO_GAIN`, не общий успех. Четыре judgments — локальная проверка механизма, не оценка domain-wide accuracy.

## Роли и границы уверенности

По пользовательскому контракту **внешняя открытая китайская модель генерирует обе стороны DPO**, а **CODEX с моделью astra 6 ultracode выполняет выбор human-role и оценку качества**. Текущий источник данных — illustrative fixtures автора аудита; эти внешние действия не выполнены. Source review не выдаётся за независимый ultracode экзамен.

Применимость: RU→EN структурированное обучение manner requirements в QualityVerifierUnit; прежде всего обычные UTF-8 слова, которых нет в ADV allowlist/suffix recognizer, при непустых RuleTags. EN→RU, HE/ES, AST/KNN и neural DPO LoRA — **не подтверждены этой находкой**. Для первого RP01 neural slice необходимо отдельно показать этот caller в фактическом воспроизведении деградации. Успех соседних веток требует сопоставления их настоящих данных/метрик; один найденный дефект не отменяет их результаты.

## Проверяемые артефакты

- [verifier_marker_probe.py](evidence/verifier_marker_probe.py) — ограниченная модель выбранных кириллических входов; не полный перенос C# tokenizer.
- [verifier_marker_probe_result.json](evidence/verifier_marker_probe_result.json) — проверенные source hashes, POS/RuleTags/predicate trace и четыре диагностические ячейки; native/CODEX/Chinese явно NOT_RUN.
- [verifier_source_and_novelty_checks.json](evidence/verifier_source_and_novelty_checks.json) — история и проверка ближайших прежних описаний.

Для повторения нужен локальный core checkout с шестью исходными blob из указанного commit. Скрипт принимает `--core-root /path/to/llm_mvp_core` и проверяет каждый blob перед расчётом; примеры запуска — в [журнале проверки](06-source-and-review-record.md). Исходники не подменяются вставленными ответами, native runtime не запускается.
