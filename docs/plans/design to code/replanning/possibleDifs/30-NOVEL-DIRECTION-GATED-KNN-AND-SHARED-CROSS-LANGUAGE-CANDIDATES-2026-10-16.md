# N30 — две независимые потери качества: unreachable ordinary KNN и cross-direction negative transfer

<!-- POST117-CURRENT-2026-11-03:START -->
## После P117: узкий опыт RU_EN исполнен, N30-B остаётся отдельным

[текущая сверка после P117](codex-2026-11-03-POST117-TRANSFER-AND-EXPERIMENT-AUDIT.md) подтверждает P117: обычный RU_EN запрос достиг памяти 0→1 из 5, но полный перевод не улучшился 3/5→3/5, экзамен 0/1. Это узкое развитие N30-A, не проверка всех шести направлений. В продукте на core 879a07b ordinary gate по-прежнему EN_RU; B был экспериментальной копией. N30-B о namespace направления этим опытом не исполнялся.

Проверка сохранения должна выгружать фактический TranslationKnnStore, а не конструировать ExpectedKnnWrites из тех же уроков. Count=9 и hash ожидаемого журнала не обнаруживают same-count порчу target/signature. Существующий internal export доступен Program source-linked host, поскольку исходники включены в одну сборку. Сохранённые actual executor maps и 3 reload-ответа подтверждены, полный product cold-reader ещё не аттестован.

Технический контроль обязан пройти мимо exact-source к заявленной ветви; отрицательный ordinary запрос должен попасть в ту же ячейку и отвергнуться source guard. Обратный порядок candidate pairs отделяет guard от выбора первого. Отдельно оценивать язык и полноту: «the violet lantern glows» при gold с«now» неверно; исходная кириллица с confidence0,49 не является отказом. Общий расходP11723 Execute/9TrainOnExample включает контроль копии. Ниже историческое исследование 16.10; новый запуск не назначается.
<!-- POST117-CURRENT-2026-11-03:END -->


**Дата:** 2026-10-16. **Исследование:** сравнение реальных кодовых ветвей, сохранённых отчётов и независимая новая гипотеза. **Core:** lubochka/llm_mvp_core, codex/dpo-implementation, наблюдавшийся HEAD 79082340f01b744cdf08185616f2bd818dbd06b8. **Planning:** xiigen-planning/llm_plan3. **Статус:** SOURCE-CONFIRMED CONTROL FLOW; NATIVE C# LEARNING-QUALITY EXPERIMENT NOT_RUN; не заявлен независимый CODEX/Astra-review. Это не установленная причина RP01 Student C# 0/3 или ENRU 0/4.

## 1. Что это означает на человеческом языке

Обученный переводческий Executor может аккуратно записать урок в память, но обычный запрос пяти из шести языковых направлений вообще не обратится к этой памяти, если его tokenizer не распознал специальное семантическое понятие. А для технических запросов, где поиск всё же включается, память общая для всех языковых направлений и не хранит направление в ключе. В результате правильный урок перевода на испанский способен стать ответом на запрос перевода на иврит; текущая проверка целевого языка такой ответ может пропустить.

Это два разных отказа:
- **N30-A / LOST POSITIVE LEARNING:** KNN-урок существует, но потребительский запрос не достигает KNN.
- **N30-B / NEGATIVE TRANSFER:** запрос достигает KNN, но получает допустимый по грубой проверке ответ от чужого направления.

Оба отличаются от ранее описанных N16 FIFO/top5, N28 trained-tokenizer-index drift, N26 language-insensitive tokenizer correction и N29 first-token pseudoalignment. Здесь исследуется **route-level eligibility + namespace общей памяти + target-language acceptance**.

## 2. Прямые исходные доказательства

| ID | Исходный факт | Строгое доказательство |
| --- | --- | --- |
| S30-01 | Создаются шесть ExecutorUnit, но каждому передаётся одна ссылка knnStore. RU_EN, EN_RU, HE_EN, EN_HE, ES_EN, EN_ES различаются объектами Executor, но **не** пространством KNN. | [TranslationMachine.CreateRuntime L716–784](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationMachine.cs#L716-L784) |
| S30-02 | KNN сохраняет записи по строке signature: Dictionary<string, List<(Source, Target)>>; AddPair не принимает direction, sourceLang, targetLang, topicDomain, unitId. | [TranslationUnitContracts L183–214](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationUnitContracts.cs#L183-L214) |
| S30-03 | FindNearestCascade принимает full/prefix/structure signature и sourceText, но не target direction. Источник охраняется лишь узким SourceLaw или lexical overlap. | [TranslationUnitContracts L241–310](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationUnitContracts.cs#L241-L310) |
| S30-04 | TrainOnExample записывает chosen в shared KNN по example.SemanticSignature, затем sourceTokens.PrefixSignature и StructureSignature. Обучение Executor одного направления меняет память, читаемую другим. | [TranslationTrainedUnits L403–425](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L425) |
| S30-05 | Execute вызывает KNN только при hasSemanticConcept или signature с PAIR; иначе ordinary KNN разрешён **только** _direction == EN_RU. Exact/phrase/word maps идут раньше. | [TranslationTrainedUnits L264–365](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L365) |
| S30-06 | Для простых слов EN/HE/ES без TECH_TERM и CODE_IDENTIFIER tokenizer возвращает UNKNOWN concepts; концептуальный словарь существует в основном для русского, lang == ru. | [TranslationSemanticTokenizer L99–144](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationSemanticTokenizer.cs#L99-L144), [L268–361](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationSemanticTokenizer.cs#L268-L361) |
| S30-07 | RU_EN и EN_RU имеют специальные target-предикаты, остальные четыре направления попадают в default: только отсутствие кириллицы. Испанский target может пройти в EN_HE. | [TranslationTrainedUnits L924–956](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationTrainedUnits.cs#L924-L956) |
| S30-08 | TrainedTranslationAdapter возвращает candidate при confidence >= 0.5 и verifier PASS; KNN присваивает 0.88; verifier без выученных matching requirements возвращает PASS=true. | [TranslationDirectionAdapters L111–151](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationDirectionAdapters.cs#L111-L151), [TranslationTrainedUnits L1040–1094](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationTrainedUnits.cs#L1040-L1094) |
| S30-09 | Narrow SourceLaw проверяет только ограниченные English copular clauses, вне шаблона возвращает Unknown, а не contradiction; для технического source null field не срабатывает veto. | [TranslationMemorySourceLaw L11–57](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationMemorySourceLaw.cs#L11-L57) |

**Контрпример, предсказание по коду, НЕ нативное измерение.** Урок EN_ES: source «null property» → chosen «propiedad nula». Технический concept NULL_LITERAL порождает общий structural signature. Затем ordinary EN_HE query «null field» (другой source, targetLanguage=he, без EN_HE урока) может получить «propiedad nula» через shared KNN: EN_HE direction проверяет лишь, что target не содержит кириллицу, и learned verifier без требований PASS. Проверить конечные подписи и adapter ID нативно, а не объявлять этот контрпример выполненным. Контроль: если убрать чужой урок или разнести store по direction, кандидат не должен возникать. Другой контроль: дать EN_HE собственные подтверждённые уроки и сравнить **качество**, а не только отсутствие ответа.

## 3. N30-A: зависимость доступности памяти от направления

Для обычного source без распознанных SemanticConcept и PAIR:
- EN_RU: KNN доступен при выполнении дополнительных similarity predicates.
- RU_EN, HE_EN, EN_HE, ES_EN, EN_ES: KNN вообще **не вызывается** по этой ветке.
- Для RU технических понятий, ASCII CODE_IDENTIFIER или «null/true/false» маршрут иной: это **условный** дефект, не универсальная характеристика этих направлений.
- ExactSourceMap / PhraseMap / WordMap могут дать полезный перевод и замаскировать отсутствие KNN; именно поэтому измерять source-disjoint запросы с нулевым попаданием в эти более ранние ветки.

**Откуда может возникать ложное ощущение обучения:** TrainStructuredBatchAsync считает проведённые уроки и отмечает Unit как обученный; наличие KNN AddPair не доказывает, что обычный flow позже вызовет FindNearestCascade. [TranslationMachine L89–145](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationMachine.cs#L89-L145).

## 4. N30-B: столкновение разных языковых направлений в общей памяти

Связь:
1. EN_ES обучил допустимую пару и записал (signature, source, target) в общую память.
2. EN_HE получает техническую source-фразу с такой же full или prefix/structure signature; его собственная exact/phrase/word-map не обучена этому source.
3. Lookup не знает, что кандидат пришёл из EN_ES; первый подходящий pair попадает в EN_HE Executor.
4. EN_HE использует default-предикат «не содержит кириллицы». Испанский латинский ответ принят.
5. Независимых выученных verifier rules может не быть, тогда verifier проверяет пустой набор требований и PASS.
6. Выход получает уверенный adapter provenance «executor-en_he-knn», хотя фактический target был уроком EN_ES.

**Условия, которые могут разрушить контрпример:** иной вычисленный signature; ранний exact/phrase/word lookup; SourceLaw veto; более ранний кандидат; scope-specific verifier term; отсутствие реального вызова TrainedTranslationAdapter. Нативный эксперимент обязан записать эти пути. Сам по себе факт общей ссылки не доказывает все пользовательские ошибки.

## 5. Где проблема применима, а где нет

| Домен / consumer | N30-A | N30-B |
| --- | --- | --- |
| TranslationDomain, обычная речь RU↔EN/HE↔EN/ES↔EN | Условно высокий приоритет для 5 направлений, когда semantic concept нет | Вероятен при разделяемых structural signatures и доступном KNN; отдельно проверять |
| Technical translation (null, true, code identifiers) | Concept-ветка достигает KNN, поэтому N30-A может не проявиться | Более высокий риск общего signature между направлениями |
| DesignToCode: Roslyn AST / C# | Этот конкретный Executor не является основным потребителем, переносить вывод нельзя | Проверить только если тот же shared KNN вызывается на реальном C# route |
| RP01 Student exact-v2 DPO → LoRA → free token generation | **Не проходит через этот translation Unit** в ранее просмотренной симуляции | **Не объявлять причинной регрессией RP01 0/3 или 0/4** без actual call trace |
| Домен с отдельными строго namespace-isolated индексами | Нет переносимости дефекта | Не переносить гипотезу на другое хранилище |

## 6. Непосредственная связь с историческими успешными механизмами

Исторические E26 lexical disambiguation, E09 structural KNN, E14 context-action routing, E43 replay — **разные источники полезного локального сигнала**, а не одна обученная сеть. [Сводка положительных evidence и границ сопоставимости](26-INDEPENDENT-FULL-GAP-REGISTER-AND-HISTORICAL-LEARNING-LINEAGE-2026-10-16.md), [матрица → KNN и неверно прочитанные negative experiments](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [соседние ветки](11-BRANCH-ARCHAEOLOGY-AND-LEARNING-PROOFS.md). Если вновь вводить обучаемые структурные признаки, необходимо сначала гарантировать **достижимость и namespace**, затем оптимизировать веса. В противном случае даже удачные прежние функции не помогут обычному ответу.

**Не заменяет ранее известные:** [N16 FIFO/TopicDomain](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md), [N28 upstream/downstream tokenizer drift](28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT-2026-10-16.md), [N23-POS](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md), [N29 rejected annotations](29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md). В доступных прочитанных аудитах отдельная сквозная пара N30-A/N30-B не была оформлена с этим проверяемым cross-direction сценарием; не утверждается исчерпывающий поиск каждого файла.

## 7. Самая короткая проверка до изменения продакшна

**[N30 micro A/B и исполнительский контракт](30-MICRO-SIX-DIRECTION-KNN-LEARNING-QUALITY-EXPERIMENT-2026-10-16.md)** — один источник обучения, frozen train order, одинаковые запросы, native A/B (shared vs direction-partitioned index; current gate vs conditional KNN), 2 дополнительных негативных контроля, 2 области (ordinary/technical), cold restore, blind source-disjoint semantic quality. Не нужны 500 новых DPO пар, GPU, обобщённый Windows CI или переобучение Student.

## 8. Evidence integrity / review

- **SOURCE-CONFIRMED:** сигнатура KNN без direction, общий store, условное подключение ordinary KNN, default некириллический validator.
- **PREDICTED FROM SOURCE:** испанский ответ на Hebrew route, инвариант отсутствия generic-KNN у пяти non-semantic направлений; требуют native logs.
- **NOT_RUN:** .NET unit/translation-flow consumer, качество source-disjoint, save/fresh-load, independent bilingual CODEX/Astra judge.
- **UNPROVED:** N30-A/N30-B стали причиной RP01 neural free-generation regression; раннее «всё было лучше» не является matched causal eval.
- **Scope:** только дополняющие документы в xiigen-planning/llm_plan3. Core docs/plans/replanning-possible-diffs неизменны.
