# Реестр результатов и кандидатов

[Простое объяснение](SIMPLE-EXPLANATION.ru.md) · [Поиск](RETRIEVAL.md) · [Математика](BOOK-FORMULAS.md)

Здесь 49 содержательных разделов сводного списка кандидатов и отдельные карточки исторических механизмов. Совпадающие C- и M-карточки — разные точки входа к одному материалу, а не независимые подтверждения. Статус кандидата не означает переноса правки в продукт. Уровень проверки каждой карточки указан отдельно.

## Быстрый список

| Карточка | Изменение | Тип результата | Проверенное основание |
|---|---|---|---|
| [C-TRANSLATION-MEMORY-INGRESS-01](#c-translation-memory-ingress-01) | Передавать соответствия частей фразы в переводческую память | Рост качества на панели | сводное исследование |
| [C-N-KU-01](#c-n-ku-01) | Учитывать do-while при разметке советов по C# | Выигрыш с потерей | сводное исследование |
| [C-NEURAL-FROZEN-TRANSPOSE-01](#c-neural-frozen-transpose-01) | Повторно использовать транспонированные замороженные веса | Механика без освоения задачи | сводное исследование |
| [C-BPE-NO-MATCH-COPY-01](#c-bpe-no-match-copy-01) | Не копировать BPE-кусок без подходящей пары | Меньше затрат | сводное исследование |
| [C-N62-R1](#c-n62-r1) | Сверять замороженные веса байтовыми диапазонами | Меньше затрат | сводное исследование |
| [C-N62-R2](#c-n62-r2) | Использовать один буфер экспонент на обратный проход DPO | Меньше затрат | сводное исследование |
| [C-E-OPT-STEP](#c-e-opt-step) | Читать шаг AdamW без копирования моментов | Меньше затрат | сводное исследование |
| [C-LF-03](#c-lf-03) | Копировать начальный градиент один раз | Меньше затрат | сводное исследование |
| [C-N63-INGRESS-LENGTH](#c-n63-ingress-length) | Откладывать слишком длинный урок до начала обучения | Механика без освоения задачи | сводное исследование |
| [C-N63-AR-CAP](#c-n63-ar-cap) | Сохранять место каждого домена в памяти повторов | Частичное обучение/перенос | сводное исследование |
| [C-N63-CORRECTION-COLLISION](#c-n63-correction-collision) | Не обучать снова ответ, уже отмеченный неверным | Частичное обучение/перенос | сводное исследование |
| [C-WIKI-TAXONOMY-01](#c-wiki-taxonomy-01) | Добавить недостающие учебные классы wiki | Выигрыш с потерей | сводное исследование |
| [C-WIKI-MARGIN-01](#c-wiki-margin-01) | Считать отступ от другого класса | Механика без освоения задачи | сводное исследование |
| [C-N60-E1](#c-n60-e1) | Не давать проверяющему противоречивый отрицательный урок | Выигрыш с потерей | сводное исследование |
| [C-LC-REF-CACHE](#c-lc-ref-cache) | Кэшировать оценку замороженного эталона DPO | Меньше затрат | первичный отчёт |
| [C-RETENTION-M1-01](#c-retention-m1-01) | Объединять признаки маршрута при новом уроке | Удержание приобретённого | сводное исследование |
| [C-T-TRAIN-CONTEXT-01](#c-t-train-context-01) | Передавать контекст части речи в словарную запись | Рост качества на панели | сводное исследование |
| [C-E-SFT-RESPONSE-MASK](#c-e-sft-response-mask) | Обучать только ответ через маску SFT | Польза не показана | первичный отчёт |
| [C-E-SFT-MOMENT-CONTINUITY](#c-e-sft-moment-continuity) | Переносить моменты AdamW между уроками | Удержание приобретённого | raw и код |
| [C-E-DPO-VALID-LENGTH](#c-e-dpo-valid-length) | Убирать правое дополнение отдельно для каждой стороны DPO | Меньше затрат | первичный отчёт |
| [C-KH-COST-01](#c-kh-cost-01) | Разбирать учебный C#-текст один раз | Меньше затрат | сводное исследование |
| [C-KH-EXT-01](#c-kh-ext-01) | Считать пригодные пары до очистки памяти методов | Удержание приобретённого | сводное исследование |
| [C-DPO-TOKENIZER-SNAPSHOT-01](#c-dpo-tokenizer-snapshot-01) | Делать один закрытый снимок токенизатора на подготовку | Меньше затрат | сводное исследование |
| [C-GRADB-LOCALITY-01](#c-gradb-locality-01) | Обходить правый градиент матричного умножения по строкам | Меньше затрат | первичный отчёт |
| [C-RMS-COORD-ALLOC-01](#c-rms-coord-alloc-01) | Считать индекс broadcast без временного массива | Меньше затрат | первичный отчёт |
| [C-BPE-WEIGHTED-CHUNKS-01](#c-bpe-weighted-chunks-01) | Хранить уникальные BPE-куски с кратностью | Меньше затрат | первичный отчёт |
| [C-WIKI-FEATURES-ONCE-01](#c-wiki-features-once-01) | Строить признаки wiki-урока один раз на все эпохи | Меньше затрат | сводное исследование |
| [C-METHOD-KNN-TARGET-SCOPE-01](#c-method-knn-target-scope-01) | Не смешивать тело метода с вложенными функциями в целевой метке | Рост качества на панели | сводное исследование |
| [C-AST-SIB-CACHE-01](#c-ast-sib-cache-01) | Кэшировать позиции соседних AST-узлов | Меньше затрат | сводное исследование |
| [C-NG01](#c-ng01) | Не путать числовой pattern с проверкой на null | Рост качества на панели | сводное исследование |
| [C-BPE-REALIZED-MERGE-COUNT-01](#c-bpe-realized-merge-count-01) | Выбирать BPE-пару по числу исполнимых замен | Выигрыш с потерей | первичный отчёт |
| [C-GRAD-ACCUM-COPY-01](#c-grad-accum-copy-01) | Убирать лишнюю копию первого вклада градиента | Меньше затрат | первичный отчёт |
| [C-FROZEN-BROADCAST-MAP-01](#c-frozen-broadcast-map-01) | Не строить обратную карту для замороженного broadcast | Меньше затрат | сводное исследование |
| [C-VT-APOSTROPHE-01](#c-vt-apostrophe-01) | Сохранять апостроф внутри термина проверяющего | Выигрыш с потерей | сводное исследование |
| [C-RMS-ROW-REDUCE-01](#c-rms-row-reduce-01) | Суммировать последнюю ось прямым чтением строки | Меньше затрат | сводное исследование |
| [C-ATTN-TRANSPOSE-STRIDES-01](#c-attn-transpose-strides-01) | Переставлять оси голов внимания прямой адресацией | Меньше затрат | сводное исследование |
| [C-JSON-OPTIONS-REUSE-01](#c-json-options-reuse-01) | Повторно использовать настройки JSON для уроков | Меньше затрат | сводное исследование |
| [C-DPO-SHIFTED-CONTEXT-01](#c-dpo-shifted-context-01) | Проверять вместимость по сдвинутому входу DPO | Механика без освоения задачи | сводное исследование |
| [C-IDENTITY-TRAIN-INDEX-01](#c-identity-train-index-01) | Заменить поиск учебной записи дополнительным набором ключей | Польза не показана | сводное исследование |
| [C-SILU-SIGMOID-REUSE-01](#c-silu-sigmoid-reuse-01) | Повторно использовать знаменатель сигмоиды в SiLU | Выигрыш с потерей | сводное исследование |
| [C-HNSW-QUERY-NORM-REUSE-01](#c-hnsw-query-norm-reuse-01) | Считать норму HNSW-запроса один раз | Польза не показана | сводное исследование |
| [C-IDENTITY-M1-01](#c-identity-m1-01) | Искать тело плана только внутри выбранного маршрута | Рост качества на панели | сводное исследование |
| [C-KN-ROUTER-LEARNED-KEY-01](#c-kn-router-learned-key-01) | Одинаково нормализовать названия технологий при обучении и запросе | Рост качества на панели | сводное исследование |
| [C-POS-QUERY-SET-ONCE-01](#c-pos-query-set-once-01) | Строить набор POS-пар запроса один раз | Меньше затрат | сводное исследование |
| [C-FILE-STATIC-RECEIVER-01](#c-file-static-receiver-01) | Не требовать null-проверку для статического поля | Рост качества на панели | сводное исследование |
| [C-RMS-MEAN-BACKWARD-01](#c-rms-mean-backward-01) | Вычислять адрес обратного усреднения делением | Меньше затрат | сводное исследование |
| [C-SFT-EXP-REUSE-01](#c-sft-exp-reuse-01) | Повторно использовать экспоненты softmax в SFT-градиенте | Меньше затрат | сводное исследование |
| [C-PARENT-ADOPTION-01](#c-parent-adoption-01) | Принимать и автоматически находить полный обученный комплект | Частичное обучение/перенос | сводное исследование |
| [C-N63-INGRESS-LENGTH-LEARNED-01](#c-n63-ingress-length-learned-01) | Допускать короткие повторные уроки из обученного старта | Частичное обучение/перенос | сводное исследование |
| [M-E18-DOMAIN-KNN](#m-e18-domain-knn) | E18: выбрать домен запроса по предикатам и соседним примерам | Рост качества на панели | raw и код |
| [M-E22-DATASET-KNN](#m-e22-dataset-knn) | E22: несколько наборов данных выбираются отдельными голосованиями | Рост качества на панели | raw и код |
| [M-E23-TEMPLATE-PROJECTION](#m-e23-template-projection) | E23: отбор признаков перед поиском готового шаблона | Рост качества на панели | raw и код |
| [M-E24-ATTRIBUTED-ROUTING](#m-e24-attributed-routing) | E24: поздний выигрыш сегментации не доказывает пользу обученного KNN | Рост качества на панели | raw и код |
| [M-E25-POS-BIGRAMS-PQ01](#m-e25-pos-bigrams-pq01) | E25/PQ01: память POS-шаблонов и экономия чтения запроса | Частичное обучение/перенос | raw и код |
| [M-E26-CONTEXT-DICTIONARY](#m-e26-context-dictionary) | E26: значение многозначного слова выбирается по контексту | Рост качества на панели | raw и код |
| [M-E27-STRUCTURE-REORDER](#m-e27-structure-reorder) | E27: применить заранее известную схему порядка слов | Рост качества на панели | raw и код |
| [M-E33-STOP-RULE-NO-GAIN](#m-e33-stop-rule-no-gain) | E33: полезное stop-правило уже решало панель без прироста от KNN | Польза не показана | raw и код |
| [M-E40V2-TRUST-KNN](#m-e40v2-trust-knn) | E40V2: уровни доверия по семи признакам источника | Рост качества на панели | raw и код |
| [M-E09-AST-STRUCTURAL-KNN](#m-e09-ast-structural-knn) | E09: структурный сосед AST и несопоставимые поздние знаменатели | Рост качества на панели | raw и код |
| [M-P125-PHRASE-MEMORY-INGRESS](#m-p125-phrase-memory-ingress) | P125 и ingress: готовые части урока должны дойти до памяти перевода | Рост качества на панели | raw и код |
| [M-P25-COLD-MEMORY-RESTORE](#m-p25-cold-memory-restore) | P25: восстановить ту же переводческую память в новом процессе | Удержание приобретённого | первичный отчёт |
| [M-TTRAIN-CONTEXT-ACQUISITION](#m-ttrain-context-acquisition) | T-TRAIN: новые записи различают книгу и бронирование благодаря POS-контексту | Рост качества на панели | raw и код |
| [M-E06-ID-COLLISION-METRIC](#m-e06-id-collision-metric) | E06: число 93,50% было показателем сжимаемости, а не точностью кода | Выигрыш с потерей | raw и код |
| [M-E13B-EXACT-EXPRESSION-MEMORY](#m-e13b-exact-expression-memory) | E13B: точная память выражений с нормализацией и отказом на новом | Частичное обучение/перенос | первичный отчёт |
| [M-E12-SIMD-SPARSE-COST](#m-e12-simd-sparse-cost) | E12: та же арифметика быстрее только в части размеров | Меньше затрат | raw и код |
| [M-E48-EARLY-TRANSFER](#m-e48-early-transfer) | E48: ранняя польза предварительного обучения при равном финале | Частичное обучение/перенос | raw и код |

<a id="c-translation-memory-ingress-01"></a>
## C-TRANSLATION-MEMORY-INGRESS-01: Передавать соответствия частей фразы в переводческую память

**Этап:** memory, knn, pipeline. **Результат:** Рост качества на панели. **Основание:** сводное исследование.

Загрузчик стал передавать заданные в уроке соответствия частей фразы в память Executor. После этого переводчик смог составлять новые сочетания из знакомых частей, а сохранение и повторная загрузка воспроизвели ответы.

**Измерение:** 8 одинаковых уроков: A 11/24 → B 19/24 верных ответов, потерь правильных нет; неверно принятых ответов 1 → 3. Новый пустой экземпляр после 8 уроков: 0/24 → 19/24.

**Граница:** Соответствия phrase_mappings уже заданы в уроках; это обучение памяти, а не LoRA. Три краевых ответа приняты ошибочно; результат относится к закреплённой панели EN→RU.

**Математическая связь:** Из описанного кода, не цитата книги: Сохранённые соответствия частей дают композицию знакомых фрагментов; универсальной формулы качества перевода из этого не следует.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L11-L27)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/independent-review/review.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/independent-review/review.md)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/independent-review/review.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/independent-review/review.md)

<a id="c-n-ku-01"></a>
## C-N-KU-01: Учитывать do-while при разметке советов по C#

**Этап:** labeling, knn. **Результат:** Выигрыш с потерей. **Основание:** сводное исследование.

Учитель меток раньше не замечал уже существующий цикл do-while и учил память советовать добавить цикл. Добавление DoStatement исправило эти метки, но большинство трёх соседей стало пропускать один действительно нужный цикл.

**Измерение:** После 4 уроков: A 1/4, B 3/4 верных; ложный совет добавить цикл 3/3 → 0/3, нужный совет 1/1 → 0/1.

**Граница:** do-while не добавлен в признаки, k=3 неизменен; эффект большинства меток не доказывает структурное обобщение. Нейронного оптимизатора нет.

**Математическая связь:** Из описанного кода, не цитата книги: Ответ KNN зависит от голосования y соседей; меняются учебные y, но не расстояния и не входное представление.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L28-L65)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/product-change-proposal.diff](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/product-change-proposal.diff)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/preregistration.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/preregistration.md)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-A.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-A.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-B.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/arm-B.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/comparison.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/comparison.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/commands.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/commands.md)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/order-check-BA/comparison.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/order-check-BA/comparison.json)

<a id="c-neural-frozen-transpose-01"></a>
## C-NEURAL-FROZEN-TRANSPOSE-01: Повторно использовать транспонированные замороженные веса

**Этап:** lora, state, pipeline. **Результат:** Механика без освоения задачи. **Основание:** сводное исследование.

Вместо повторного разворота замороженной матрицы Linear хранит готовую транспонированную копию. Объём выделений уменьшился при совпадающих сохранённых результатах шага, однако освоения задания этот опыт не показал.

**Измерение:** 4 шага: выделения −1 507 392 байта (−11,0%); 8 промахов и 24 попадания кэша; удерживается 249 856 байт. Точные ответы 0/2 → 0/2.

**Граница:** Отдельные 400 шагов LEARN-SANITY-PROBE дали 1/2, равно постоянному ответу, при другом бюджете. Нужен сброс кэша при изменении, восстановлении или разморозке веса; ускорение не установлено.

**Математическая связь:** Из описанного кода, не цитата книги: Y=XWᵀ остаётся тем же, если W не меняется; кэш заменяет вычисление представления Wᵀ, а не функцию слоя.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L66-L78)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/probe-result.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/probe-result.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/report.md)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/comparison.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/comparison.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/A/code-sign.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/A/code-sign.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/B/code-sign.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/B/code-sign.json)
- [experiments-v2/RP01-MICRO/LFT01/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/one-factor.diff](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/LFT01/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/one-factor.diff)

<a id="c-bpe-no-match-copy-01"></a>
## C-BPE-NO-MATCH-COPY-01: Не копировать BPE-кусок без подходящей пары

**Этап:** tokenizer. **Результат:** Меньше затрат. **Основание:** сводное исследование.

Новый список токенов создаётся только при первом реальном слиянии. Если пары нет, возвращается прежний список, поэтому выученный словарь и разбивка текста остаются прежними при меньшем числе копирований.

**Измерение:** Выделения −71,1% на документации к коду и −69,1% на EN→RU; словарь 256 → 320, 64 слияния, одинаковые хэши состояния, обратное восстановление 4/4.

**Граница:** Прочитанные вызывающие методы не меняют возвращённый список; это не гарантия для будущих потребителей. Причинный выигрыш времени и экономия всей цепочки KNN не измерены.

**Математическая связь:** Из описанного кода, не цитата книги: Если пары p в последовательности s нет, MergeChunk(s,p)=s; устраняется лишняя копия этого неизменного результата.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L79-L91)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/report.md)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/comparison.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/comparison.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/A/code-doc.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/A/code-doc.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/B/code-doc.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/B/code-doc.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/A/en-ru.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/A/en-ru.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/B/en-ru.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/B/en-ru.json)
- [experiments-v2/RP01-MICRO/BPE01/experiments-v2/RP01-MICRO/BPE-NO-MATCH-COPY-01/one-factor.diff](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/BPE01/experiments-v2/RP01-MICRO/BPE-NO-MATCH-COPY-01/one-factor.diff)

<a id="c-n62-r1"></a>
## C-N62-R1: Сверять замороженные веса байтовыми диапазонами

**Этап:** state, lora, dpo. **Результат:** Меньше затрат. **Основание:** сводное исследование.

Проверка неизменности замороженной базы сравнивает диапазоны байтов вместо поэлементного LINQ. Наблюдается небольшое сокращение выделений, а записанные хэши и метрики совпадают.

**Измерение:** 3 764 969 984 → 3 764 879 480 выделенных байт: −90 504 (−0,002404%); GC0/1/2 по +1.

**Граница:** Освоение обучающих целей не измерялось. Большую разницу ApplyOptimizer нельзя приписать этому сравнению; независимого runtime-повтора нет.

**Математическая связь:** N/A: изменена проверка побитового равенства; формула обновления весов не меняется.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L92-L101)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/report.md)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/A/arm-result.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/A/arm-result.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/B/arm-result.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R1-01/B/arm-result.json)

<a id="c-n62-r2"></a>
## C-N62-R2: Использовать один буфер экспонент на обратный проход DPO

**Этап:** dpo, backprop. **Результат:** Меньше затрат. **Основание:** сводное исследование.

Буфер экспонент softmax выделяется один раз на вызов обратного прохода и полностью перезаписывается на каждой позиции. Вычисления chosen и rejected сохраняют отдельные буферы, а записанные результаты совпадают.

**Измерение:** Выделения −312 328 байт (−0,0083%); пик +30 474 240 байт; GC0/1/2 по +1.

**Граница:** Историческая привязка исполнявшегося бинарника к исходнику неполна. Расчёт числа массивов не равен всей измеренной экономии; качество учебных ответов не измерено.

**Математическая связь:** Из описанного кода, не цитата книги: pᵢ=exp(zᵢ)/Σⱼexp(zⱼ): полная перезапись временного массива сохраняет те же pᵢ и градиент.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L102-L112)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/report.md)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/A/arm-result.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/A/arm-result.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/B/arm-result.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/B/arm-result.json)

<a id="c-e-opt-step"></a>
## C-E-OPT-STEP: Читать шаг AdamW без копирования моментов

**Этап:** optimizer, state, dpo. **Результат:** Меньше затрат. **Основание:** сводное исследование.

Для фиксации случайности номер шага читается из проверенных метаданных состояния оптимизатора. Полные массивы моментов продолжают переноситься там, где действительно нужны сохранение и восстановление.

**Измерение:** Выделения −467 944 байта, GC0 −4; пик +204 546 048 байт. Записанные состояния и loss совпали, изменились 16 адаптеров при неизменной базе.

**Граница:** Историческая привязка бинарника неполна, некоторые B-хэши — placeholders. Новая панель качества обучения отсутствует; универсальная эквивалентность и ускорение не доказаны.

**Математическая связь:** N/A: чтение t без копирования m и v не меняет само обновление AdamW.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L113-L123)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/measurement-erratum.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/measurement-erratum.md)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/report.md)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/study.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/study.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/A/arm-result.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/A/arm-result.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/B/arm-result.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/B/arm-result.json)

<a id="c-lf-03"></a>
## C-LF-03: Копировать начальный градиент один раз

**Этап:** backprop. **Результат:** Меньше затрат. **Основание:** сводное исследование.

Убрана внешняя копия начального градиента перед конструктором Tensor. Защитная копия внутри TensorStorage сохраняется, поэтому владение данными не меняется.

**Измерение:** Выделения −646 816 байт, GC0 −3; wall +1908,1644 мс, CPU +609,375 мс. NLL первого токена снизился на 0,000285178, но ранг 221 и неверный ответ сохранились.

**Граница:** Сигнал по первому токену не доказывает качество полного ответа. Один A/B с фиксированным порядком и неполной привязкой бинарника не устанавливает причинность времени.

**Математическая связь:** Из описанного кода, не цитата книги: copy(copy(g)) и copy(g) дают те же значения при сохранённой защитной копии; градиент и правило накопления не меняются.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L124-L134)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/measurement-erratum.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/measurement-erratum.md)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/report.md)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/study.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/study.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/A/arm-result.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/A/arm-result.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/B/arm-result.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/B/arm-result.json)

<a id="c-n63-ingress-length"></a>
## C-N63-INGRESS-LENGTH: Откладывать слишком длинный урок до начала обучения

**Этап:** pipeline, dpo. **Результат:** Механика без освоения задачи. **Основание:** сводное исследование.

Длина урока проверяется до изменения состояния, а неподходящая строка записывается как отложенная. Это позволило обработать короткие уроки, но правильные ответы не появились.

**Измерение:** A: 0 обновлений; B: 2 обновления и 70 целей; длинная строка 221 токен отложена. Правильных токенов ответа и точных ответов — 0.

**Граница:** Учебная цель остаётся открытой; B потребил больше ресурсов, чем отказавшийся A. Более поздний N63IL — отдельный опыт на обученном родителе.

**Математическая связь:** N/A: условие допуска по длине предотвращает преждевременный отказ, но само по себе не гарантирует обучение.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L135-L139)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/N63I/results/arms.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N63I/results/arms.json)

<a id="c-n63-ar-cap"></a>
## C-N63-AR-CAP: Сохранять место каждого домена в памяти повторов

**Этап:** memory, pipeline. **Результат:** Частичное обучение/перенос. **Основание:** сводное исследование.

Буфер повторов оставляет хотя бы одну запись каждого домена. Отдельная большая проба лучше удержала некоторые старые токены, но показала регрессии на других строках и не дала точных полных ответов.

**Измерение:** Исходный опыт: 9 обновлений, правильные токены 0 → 0. Проба: 4600 обновлений; A07 — A 2/49, B 15/49, C 4/49; EO02 — A 1/49, B 15/49, C 3/49; точных полных ответов 0.

**Граница:** У B относительно A обновлений +50%, входных токенов +86,79%; C уравнивает обновления, но не порядок уроков и объём токенов. Есть регрессии A01/N01/N02; причинный эффект ёмкости не отделён от бюджета.

**Математическая связь:** Из описанного кода, не цитата книги: Изменяется распределение уроков для повторения, следовательно и средний градиент; из сохранения одной записи не следует гарантия удержания всего навыка.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L140-L146)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/N63R/results/n63-ar-cap.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N63R/results/n63-ar-cap.json)
- [experiments-v2/RP01-MICRO/N63R-PROBE/results/n63r-probe.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N63R-PROBE/results/n63r-probe.json)

<a id="c-n63-correction-collision"></a>
## C-N63-CORRECTION-COLLISION: Не обучать снова ответ, уже отмеченный неверным

**Этап:** pipeline, labeling, dpo. **Результат:** Частичное обучение/перенос. **Основание:** сводное исследование.

При совпадении домена и запроса старый SFT-ответ, равный rejected, заменяется исправленным источником с сохранением происхождения. В отдельной пробе это лучше удержало предпочтение и часть токенов, но обе ветви потеряли часть исходного качества.

**Измерение:** Проба: 802 обновления; до 22/23 верных токенов, после A 20/23 и B 21/23. Отступ +1,1252952 → A −0,119724035 / B +1,1151558; полных точных ответов нет.

**Граница:** Исходный B не исполнялся. На экзамене 0 верных токенов, NLL B хуже A; текущие исходники не полностью аттестуют исторический бинарник пробы.

**Математическая связь:** Из описанного кода, не цитата книги: SFT повышает log p(ответа); если этот ответ одновременно DPO.Rejected, два учебных сигнала конфликтуют. Удаление противоречия не гарантирует перенос на новые запросы.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L147-L157)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/N63C/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N63C/results/report.md)
- [experiments-v2/RP01-MICRO/N63C/results/candidate.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N63C/results/candidate.md)
- [experiments-v2/RP01-MICRO/N63C/results/A/arm-result.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N63C/results/A/arm-result.json)
- [experiments-v2/RP01-MICRO/N63C-PROBE/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N63C-PROBE/report.md)
- [experiments-v2/RP01-MICRO/N63C-PROBE/results/n63c-probe.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N63C-PROBE/results/n63c-probe.json)

<a id="c-wiki-taxonomy-01"></a>
## C-WIKI-TAXONOMY-01: Добавить недостающие учебные классы wiki

**Этап:** labeling, representation. **Результат:** Выигрыш с потерей. **Основание:** сводное исследование.

Две новые темы wiki получают собственные классы и расшифровку в нужные единицы. Маршрутизатор чаще выбирает нужную тему, однако один отрицательный контроль ухудшился.

**Измерение:** Верных выборов 2/6 → 4/6, неверных 4/6 → 1/6, появился 1 отказ; параметров 570 → 935, снимок 9224 → 14 885 байт.

**Граница:** Контроль «перевод» стал неверным. Загрузка старых снимков с позиционными индексами и весь продуктовый маршрутизатор не проверялись; число обновлений 3 против 7.

**Математическая связь:** Из описанного кода, не цитата книги: Предсказание выбирает y из заданного множества классов: отсутствующий класс нельзя выбрать, пока он не представлен в выходном пространстве.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L158-L164)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/WT01/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/WT01/results/report.md)
- [experiments-v2/RP01-MICRO/WT01/results/candidate.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/WT01/results/candidate.md)
- [experiments-v2/RP01-MICRO/WT01/results/A/report.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/WT01/results/A/report.json)
- [experiments-v2/RP01-MICRO/WT01/results/B/report.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/WT01/results/B/report.json)
- [experiments-v2/RP01-MICRO/WT01/results/comparison.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/WT01/results/comparison.json)

<a id="c-wiki-margin-01"></a>
## C-WIKI-MARGIN-01: Считать отступ от другого класса

**Этап:** representation, labeling. **Результат:** Механика без освоения задачи. **Основание:** сводное исследование.

Лучший конкурент выбирается среди классов, отличных от ожидаемого, а обновление запускается при малом отступе. На однотипных уроках модель стала всегда выдавать единственную учебную метку, поэтому различение маршрутов не подтверждено.

**Измерение:** A: 0 обновлений из 36 предъявлений; B: 1 из 36. B — 2 верных и 2 неверных ответа, как постоянный ответ; два отрицательных контроля перешли от отказа к ошибке.

**Граница:** Учебная цель открыта; параметры 15 → 240, снимок 1256 → 4474 байта. WT01 не является контролем для отделения состава данных от критерия.

**Математическая связь:** Из описанного кода, не цитата книги: margin=s_expected−max(s_other), где other≠expected; это исправляет условие обновления, но не доказывает обученное различение.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L165-L171)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/WM01/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/WM01/results/report.md)
- [experiments-v2/RP01-MICRO/WM01/results/candidate.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/WM01/results/candidate.md)
- [experiments-v2/RP01-MICRO/WM01/results/report-for-user.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/WM01/results/report-for-user.md)
- [experiments-v2/RP01-MICRO/WM01/results/A/report.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/WM01/results/A/report.json)
- [experiments-v2/RP01-MICRO/WM01/results/B/report.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/WM01/results/B/report.json)
- [experiments-v2/RP01-MICRO/WM01/results/comparison.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/WM01/results/comparison.json)

<a id="c-n60-e1"></a>
## C-N60-E1: Не давать проверяющему противоречивый отрицательный урок

**Этап:** labeling, memory. **Результат:** Выигрыш с потерей. **Основание:** сводное исследование.

Отрицательный урок проверяющего пропускается, когда подставленный rejected буквально равен chosen. Противоречивый запрет исчез, но без настоящего отрицательного примера появилась другая ошибка.

**Измерение:** Ложные отказы 1 → 0, ложные принятия 0 → 1, верных вердиктов 3/4 в обоих плечах. Уроков 6 → 5, состояние 6695 → 6544 байта.

**Граница:** Нужен явный отрицательный пример того же маркера. Это обучение правил проверяющей памяти, а не подтверждение нейронного DPO.

**Математическая связь:** Из описанного кода, не цитата книги: Одинаковому входу не следует назначать несовместимые метки chosen и rejected; пропуск противоречия ещё не создаёт знания о правильной границе отказа.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L172-L178)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/report.md)
- [experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/report-for-user.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/report-for-user.md)
- [experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/pair-summary.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/pair-summary.json)
- [experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/A/arm-result.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/A/arm-result.json)
- [experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/B/arm-result.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/B/arm-result.json)

<a id="c-lc-ref-cache"></a>
## C-LC-REF-CACHE: Кэшировать оценку замороженного эталона DPO

**Этап:** dpo, state. **Результат:** Меньше затрат. **Основание:** первичный отчёт.

Оценка эталона повторно используется только для той же пары, токенизатора, режима и неизменной эталонной модели. Текущая обучаемая модель вычисляется заново, поэтому её граф и градиенты остаются свежими.

**Измерение:** По 2 DPO-обновления: выделения 7 331 104 632 → 6 010 567 464 байта (−18,013%); TRAIN 46/46 и 44/44 в обоих плечах, конечные состояния совпали.

**Граница:** Фазовые счётчики несогласованы, устойчивое ускорение не доказано; chosen NLL одинаково немного ухудшился. Общий старт содержит только веса SFT, исторические optimizer/RNG не сохранены.

**Математическая связь:** Из описанного кода, не цитата книги: log p_ref(y|x) постоянно для одинаковых x,y и замороженного reference; log p_policy(y|x) после шага меняется и таким кэшем заменяться не может.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L179-L187)
- [experiments-v2/RP01-MICRO/N61-REF-CACHE/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/report.md)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/N61-REF-CACHE/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/report.md)
- [experiments-v2/RP01-MICRO/N61-REF-CACHE/results/candidate.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/candidate.md)
- [experiments-v2/RP01-MICRO/N61-REF-CACHE/results/report-for-user.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/report-for-user.md)
- [experiments-v2/RP01-MICRO/N61-REF-CACHE/results/pair-summary.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/pair-summary.json)
- [experiments-v2/RP01-MICRO/N61-REF-CACHE/results/common/preparation.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/common/preparation.json)
- [experiments-v2/RP01-MICRO/N61-REF-CACHE/results/A/arm-result.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/A/arm-result.json)
- [experiments-v2/RP01-MICRO/N61-REF-CACHE/results/B/arm-result.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/B/arm-result.json)

<a id="c-retention-m1-01"></a>
## C-RETENTION-M1-01: Объединять признаки маршрута при новом уроке

**Этап:** memory, representation. **Результат:** Удержание приобретённого. **Основание:** сводное исследование.

Новый парафраз дополняет входные признаки маршрута вместо полной замены старых. Система сохраняет прежний правильный ответ и приобретает новый вариант запроса.

**Измерение:** После нового урока A 3/4, B 4/4 верных; входных признаков A 8 → 8, B 8 → 12. В A один прежний верный ответ стал неверным, что скрывает общая доля 3/4.

**Граница:** Память растёт с новыми парафразами. Противоречащие исправления не проверены: объединение может сохранять устаревшие признаки.

**Математическая связь:** Из описанного кода, не цитата книги: w_new(token)=max(w_old(token),w_lesson(token)); прежний входной признак не исчезает из-за отсутствия в очередном уроке.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L188-L194)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/report.md)
- [experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/candidate.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/candidate.md)
- [experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/report-for-user.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/report-for-user.md)
- [experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/A/raw-answers.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/A/raw-answers.json)
- [experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/B/raw-answers.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/B/raw-answers.json)
- [experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/A/states.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/A/states.json)
- [experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/B/states.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/B/states.json)

<a id="c-t-train-context-01"></a>
## C-T-TRAIN-CONTEXT-01: Передавать контекст части речи в словарную запись

**Этап:** representation, memory, knn. **Результат:** Рост качества на панели. **Основание:** сводное исследование.

Две словарные записи получают различающий контекст частей речи. Уже существующий читатель смог выбрать разные значения слова book, когда этот контекст действительно записан.

**Измерение:** На 4 запросах A 2/4, B 4/4; на 3 уникальных входах A 1/3, B 3/3. Цена — 4 дополнительных POS-значения и +1024 выделенных байта после обучения.

**Граница:** Q1 и Q3 повторяют один вход. Потеря контекста реальным продуктовым писателем не доказана; полные фразы, другие слова, шумные POS и польза KNN-голосования не проверены.

**Математическая связь:** Из описанного кода, не цитата книги: Разные значения нельзя устойчиво различить по одинаковому входному вектору; добавление POS-контекста делает наблюдаемые представления различными.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L195-L201)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/report.md)
- [experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/candidate.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/candidate.md)
- [experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/report-for-user.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/report-for-user.md)
- [experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/A/raw.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/A/raw.json)
- [experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/B/raw.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/B/raw.json)
- [experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/stages.txt](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/stages.txt)

<a id="c-e-sft-response-mask"></a>
## C-E-SFT-RESPONSE-MASK: Обучать только ответ через маску SFT

**Этап:** dpo, backprop, pipeline. **Результат:** Польза не показана. **Основание:** первичный отчёт.

Маска исключила вклад запроса в SFT-loss, сохранив исходный знаменатель и граф. На уже насыщенном общем старте полезного сигнала почти не осталось, а импортированные моменты продолжили двигать веса.

**Измерение:** Loss 0,0061 против 0,000042; NLL ответов 0,000156 → 0,000159 и 0,000160 → 0,000164, отступ ухудшился на 3/4 заданий. Отдельная подготовка общего старта: 1000 шагов, teacher-forced точные строки 0/2 → 2/2.

**Граница:** Маска не дала пользы на этом старте, ресурсы почти те же. Свободная генерация нечитаема; результат не доказывает, что response masking вообще бесполезен.

**Математическая связь:** Из описанного кода, не цитата книги: L=−Σ_t mask_t·log p(y_t|prefix_t)/(T−1); при почти нулевом новом градиенте старые моменты AdamW всё ещё влияют на шаг.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L202-L210)
- [experiments-v2/RP01-MICRO/MS01/results/attempt-0001/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/report.md)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/MS01/results/attempt-0001/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/report.md)
- [experiments-v2/RP01-MICRO/MS01/results/attempt-0001/candidate.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/candidate.md)
- [experiments-v2/RP01-MICRO/MS01/results/attempt-0001/report-for-user.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/report-for-user.md)
- [experiments-v2/RP01-MICRO/MS01/results/attempt-0001/pair-summary.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/pair-summary.json)
- [experiments-v2/RP01-MICRO/MS01/results/attempt-0001/A/metrics.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/A/metrics.json)
- [experiments-v2/RP01-MICRO/MS01/results/attempt-0001/B/metrics.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/B/metrics.json)

<a id="c-e-sft-moment-continuity"></a>
## C-E-SFT-MOMENT-CONTINUITY: Переносить моменты AdamW между уроками

**Этап:** optimizer, state, lora. **Результат:** Удержание приобретённого. **Основание:** raw и код.

Новая учебная сессия получает не только веса, но и моменты AdamW с номером шага. На двух конкурирующих уроках это сохранило малую ошибку и предпочтение, которые ухудшились при чистом оптимизаторе.

**Измерение:** При одинаковом старте L2 второго обновления A 0,2299684 против B 0,0179196; NLL D-T1 0,00015603148 → A 0,00031151573 / B 0,0001555981. D1: +0,02023 → A −0,04153 / B +0,02803; 33/33 верных токена в обоих.

**Граница:** Один A/B и две конкурирующие строки; полное свободное владение задачей не установлено. Переносятся вместе m,v,t, поэтому нельзя приписать всё одному второму моменту; цена 426 788 байт и 1,40 мс на границу.

**Математическая связь:** Из описанного кода, не цитата книги: AdamW: m←β₁m+(1−β₁)g, v←β₂v+(1−β₂)g², Δw=−η·m̂/(√v̂+ε). После полного сброса первый шаг даёт m̂=g и v̂=g², то есть примерно −η·sign(g), даже при малом остаточном g, если |g|≫ε.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L211-L217)
- [experiments-v2/RP01-MICRO/MC01/results/attempt-0001/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/MC01/results/attempt-0001/report.md)
- [experiments-v2/RP01-MICRO/MC01/results/attempt-0001/A/metrics.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/MC01/results/attempt-0001/A/metrics.json)
- [experiments-v2/RP01-MICRO/MC01/results/attempt-0001/B/metrics.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/MC01/results/attempt-0001/B/metrics.json)
- [04.TrainingLoop/Optimizers/AdamW.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/04.TrainingLoop/Optimizers/AdamW.cs#L72-L106)
- [35.ExperimentsV2/Experiments/Rp01Micro/MC01/Common/ContinuousLearning/MC01ConsumerBridge.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Experiments/Rp01Micro/MC01/Common/ContinuousLearning/MC01ConsumerBridge.cs#L198-L319)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/MC01/results/attempt-0001/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/MC01/results/attempt-0001/report.md)
- [experiments-v2/RP01-MICRO/MC01/results/attempt-0001/A/metrics.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/MC01/results/attempt-0001/A/metrics.json)
- [experiments-v2/RP01-MICRO/MC01/results/attempt-0001/B/metrics.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/MC01/results/attempt-0001/B/metrics.json)
- [04.TrainingLoop/Optimizers/AdamW.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/04.TrainingLoop/Optimizers/AdamW.cs)
- [35.ExperimentsV2/Experiments/Rp01Micro/MC01/Common/ContinuousLearning/MC01ConsumerBridge.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Experiments/Rp01Micro/MC01/Common/ContinuousLearning/MC01ConsumerBridge.cs)

<a id="c-e-dpo-valid-length"></a>
## C-E-DPO-VALID-LENGTH: Убирать правое дополнение отдельно для каждой стороны DPO

**Этап:** dpo, attention, pipeline. **Результат:** Меньше затрат. **Основание:** первичный отчёт.

Chosen и rejected исполняются по собственной действительной длине вместо общей длины с пустыми позициями. Активные цели и маски сохраняются, а лишняя работа на правом дополнении исчезает.

**Измерение:** Удалено 60 пустых позиций из 616 (−9,7%); выделения −7,75%. Loss, отступ, активные logits и нормы градиентов совпали в заданном допуске.

**Граница:** Итоговые веса не побитово равны; пик +27% в одном запуске. Причинное ускорение и улучшение качества на новых запросах не доказаны.

**Математическая связь:** Из описанного кода, не цитата книги: При причинной маске будущие правые padding-позиции не участвуют в вероятности предыдущего активного токена; удаление допустимо при сохранении префикса, позиций и целевой маски.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L218-L224)
- [experiments-v2/RP01-MICRO/PD01/results/attempt-0001/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/PD01/results/attempt-0001/report.md)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/PD01/results/attempt-0001/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/PD01/results/attempt-0001/report.md)

<a id="c-kh-cost-01"></a>
## C-KH-COST-01: Разбирать учебный C#-текст один раз

**Этап:** representation, labeling, knn. **Результат:** Меньше затрат. **Основание:** сводное исследование.

Одно синтаксическое дерево используется и для учебной метки, и для вектора признаков. Повторный разбор исчез, а метки, признаки и ответы остались прежними.

**Измерение:** Выделения −38 376 байт, подготовка −39,567 мс. Оба плеча обучились с 0/4 до 4/4 против 2/4 у постоянного ответа; метки и признаки побитово равны.

**Граница:** Один запуск на плечо, CPU выше на один шаг счётчика. Улучшение качества после уроков общее для A и B, не эффект устранения второго разбора.

**Математическая связь:** Из описанного кода, не цитата книги: Одинаковый Parse(source) даёт одно и то же дерево для обеих функций; повторное использование не меняет вычисляемые φ(x) и y.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L225-L231)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/KH-COST-01/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/KH-COST-01/report.md)

<a id="c-kh-ext-01"></a>
## C-KH-EXT-01: Считать пригодные пары до очистки памяти методов

**Этап:** pipeline, knn, state. **Результат:** Удержание приобретённого. **Основание:** сводное исследование.

Перед заменой памяти считается число действительно пригодных соседних пар методов одного файла. Набор из нескольких несвязанных методов больше не проходит допуск и не стирает уже выученные ответы.

**Измерение:** 4 метода из разных файлов дают 0 пригодных пар: A потерял 4/4 прежних правильных ответов, B сохранил 4/4. Допуск дороже на 0,358 мс.

**Граница:** Наборы с 1–3 пригодными парами также отклоняются. Это удержание ранее выученного, а не приобретение нового навыка на пустом наборе пар.

**Математическая связь:** Из описанного кода, не цитата книги: Число методов не равно числу пар с одинаковым FileId и последовательным IndexInFile; проверять следует именно множество входов обучателя.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L232-L238)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/KHE/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/KHE/results/report.md)

<a id="c-dpo-tokenizer-snapshot-01"></a>
## C-DPO-TOKENIZER-SNAPSHOT-01: Делать один закрытый снимок токенизатора на подготовку

**Этап:** tokenizer, dpo, state. **Результат:** Меньше затрат. **Основание:** сводное исследование.

Подготовка нескольких пар лениво создаёт один неизменяемый снимок токенизатора после исходных проверок. Пары используют одинаковый снимок, а при наблюдателе токенизации он сбрасывается.

**Измерение:** Копий 2 → 1; выделения −471 336 байт (−11,88%). Отпечатки, длины, loss, отступ, сдвиг весов и конечные веса совпали побитово.

**Граница:** Подготовка +374,8 мс и CPU +125 мс на одном вызове. На одной паре экономии нет; корректность зависит от срока жизни и неизменности снимка.

**Математическая связь:** Из описанного кода, не цитата книги: Одинаковое состояние токенизатора задаёт одинаковое отображение text→token IDs; число копий состояния не должно менять это отображение.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L239-L245)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/TS01/results/attempt-0001/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/TS01/results/attempt-0001/report.md)

<a id="c-gradb-locality-01"></a>
## C-GRADB-LOCALITY-01: Обходить правый градиент матричного умножения по строкам

**Этап:** backprop. **Результат:** Меньше затрат. **Основание:** первичный отчёт.

Изменён порядок вложенных циклов расчёта градиента, чтобы читать соседние элементы памяти подряд. Число арифметических операций и результат обучения сохранены.

**Измерение:** 46 вызовов и 123 420 672 умножения-сложения; участок 558,9 → 209,1 мс и 570,5 → 224,4 мс (−62,6% и −60,7%). Конечные веса побитово равны; выделения +32 456 байт.

**Граница:** Два измеренных обновления, один запуск на плечо; другие формы матриц не проверены. Локальное ускорение участка не равно доказанному ускорению всей системы.

**Математическая связь:** Из описанного кода, не цитата книги: Для Y=XW градиент dW=Xᵀ·dY; меняется обход одной и той же суммы по индексам при сохранённом порядке накопления каждого элемента.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L246-L252)
- [experiments-v2/RP01-MICRO/KGB/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/KGB/results/report.md)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/KGB/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/KGB/results/report.md)

<a id="c-rms-coord-alloc-01"></a>
## C-RMS-COORD-ALLOC-01: Считать индекс broadcast без временного массива

**Этап:** normalization, backprop. **Результат:** Меньше затрат. **Основание:** первичный отчёт.

Координата второго множителя вычисляется делением и остатком, без массива координат на каждый элемент. Значения, проверки форм, граф и обратный проход остаются прежними.

**Измерение:** 4 шага: выделения −1 290 240 байт (−9,46%); веса, loss, ответы и ранги побитово совпали. Время +12,9 мс, CPU +15,7 мс.

**Граница:** Это экономия выделений, не доказанное ускорение. Пик процесса выше на 176 128 байт; измерение длится около 80 мс.

**Математическая связь:** Из описанного кода, не цитата книги: Для оси размером 1 broadcast всегда выбирает координату 0; вычисление того же адреса арифметикой сохраняет a_i·b_map(i).

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L253-L259)
- [experiments-v2/RP01-MICRO/RMS01/a001/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/RMS01/a001/results/report.md)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/RMS01/a001/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/RMS01/a001/results/report.md)

<a id="c-bpe-weighted-chunks-01"></a>
## C-BPE-WEIGHTED-CHUNKS-01: Хранить уникальные BPE-куски с кратностью

**Этап:** tokenizer. **Результат:** Меньше затрат. **Основание:** первичный отчёт.

Повторяющийся кусок текста представлен одним списком токенов и числом повторов. Подсчёт пар учитывает кратность, поэтому словарь и экзаменационная разбивка остаются прежними при меньшей работе.

**Измерение:** Code-doc: 663 → 37 представителей, выделения 3 926 152 → 767 328; EN→RU: 471 → 27, 2 907 816 → 567 672. В обоих профилях −80,5%, одинаковые обученные состояния и выходы.

**Граница:** На почти уникальном корпусе выигрыша может не быть. Это сохранение результата токенизации с меньшими затратами, а не измеренное улучшение качества переводов.

**Математическая связь:** Из описанного кода, не цитата книги: Σ_вхождений n_s(pair)=Σ_уникальных_s multiplicity(s)·n_s(pair); при сохранении порядка первого появления совпадают частоты и выбор слияния.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L260-L266)
- [experiments-v2/RP01-MICRO/BW01/results/attempt-0001/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/BW01/results/attempt-0001/report.md)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/BW01/results/attempt-0001/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/BW01/results/attempt-0001/report.md)

<a id="c-wiki-features-once-01"></a>
## C-WIKI-FEATURES-ONCE-01: Строить признаки wiki-урока один раз на все эпохи

**Этап:** representation. **Результат:** Меньше затрат. **Основание:** сводное исследование.

Признаки каждой учебной строки вычисляются перед циклом эпох и повторно используются в 18 эпохах. Порядок обновлений и обученное состояние не меняются.

**Измерение:** Выделения −1 170 784 байта (−88,6%); время −54,6% в этой паре. Оба плеча 0/4 → 4/4, состояние и уверенности побитово равны.

**Граница:** Один запуск на плечо, CPU ниже шага счётчика. Кэш локален вызову и не создаёт дополнительного выигрыша качества.

**Математическая связь:** Из описанного кода, не цитата книги: Если φ(x) не зависит от эпохи или текущих весов, вычисление φ(x) один раз эквивалентно его повторному построению перед теми же обновлениями.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L267-L273)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/WC01/results/attempt-001/report.ru.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/WC01/results/attempt-001/report.ru.md)

<a id="c-method-knn-target-scope-01"></a>
## C-METHOD-KNN-TARGET-SCOPE-01: Не смешивать тело метода с вложенными функциями в целевой метке

**Этап:** labeling, representation, knn. **Результат:** Рост качества на панели. **Основание:** сводное исследование.

Для целевого следующего метода признаки собираются только в области внешнего метода. Тела локальных функций и лямбд больше не приписываются ему при выборе учебной категории.

**Измерение:** После обучения A 2/4, B 4/4 верных; целевые запросы 0/2 → 2/2, два внешних контроля сохранены. Выделения при обучении +1024 байта.

**Граница:** Входная сторона пары, расстояние Жаккара и k не меняются. Вызываемые локальные функции не проверялись, поэтому граница применимости остаётся узкой.

**Математическая связь:** Из описанного кода, не цитата книги: Меняется определение целевой y следующего метода; KNN ищет тех же соседей, но учится на категории соответствующей синтаксической области.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L274-L280)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001/report.md)

<a id="c-ast-sib-cache-01"></a>
## C-AST-SIB-CACHE-01: Кэшировать позиции соседних AST-узлов

**Этап:** representation, knn. **Результат:** Меньше затрат. **Основание:** сводное исследование.

Позиция и число соседних узлов вычисляются один раз для каждого родителя дерева. Кэш живёт только внутри обучения по одному дереву, а признаки и ответы остаются прежними.

**Измерение:** Память 0 → 5122 и 0 → 4106 пар; 16 ответов и отпечатки состояния совпали. Выделения −93,6% на широком дереве и −13,3% на узком.

**Граница:** Фасад TrainStructuralKnn требует отдельной интеграции. Новое качество ответов этим фактором не получено; результаты относятся к двум формам дерева.

**Математическая связь:** Из описанного кода, не цитата книги: Для неизменного дерева position(node,parent) и childrenCount(parent) постоянны; кэш возвращает те же компоненты φ(node).

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L281-L287)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/AST01/results/attempt-0001/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/AST01/results/attempt-0001/report.md)

<a id="c-ng01"></a>
## C-NG01: Не путать числовой pattern с проверкой на null

**Этап:** labeling, knn. **Результат:** Рост качества на панели. **Основание:** сводное исследование.

Прямое числовое сравнение параметра int больше не считается защитой от пустой ссылки. Разметчик перестаёт скрывать нужный совет о null, сохраняя прочие условия и признаки.

**Измерение:** Учебные метки 9/12 → 12/12 правильных; целевой ответ 0/1 → 1/1, три контроля сохранены. Память в обоих плечах 12 строк; выделения −1424 байта.

**Граница:** Не решены привязка проверки к конкретному получателю, доминирование проверки и составные patterns. Измерен узкий класс прямых сравнений не-nullable int.

**Математическая связь:** Из описанного кода, не цитата книги: Предикат о числовом int не устанавливает непустоту другого ссылочного объекта; поэтому он не должен давать метку «защита от null уже есть».

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L288-L294)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/NG01/r/attempt-0001/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/NG01/r/attempt-0001/report.md)

<a id="c-bpe-realized-merge-count-01"></a>
## C-BPE-REALIZED-MERGE-COUNT-01: Выбирать BPE-пару по числу исполнимых замен

**Этап:** tokenizer. **Результат:** Выигрыш с потерей. **Основание:** первичный отчёт.

В серии одинаковых токенов считаются только неперекрывающиеся замены, которые действительно выполнит MergeChunk. На маленьком корпусе выбралось другое слияние, полезное для лямбд, но хуже для комментариев.

**Измерение:** Словарь 256 → 257: A учит //, B учит =>. Учебный корпус 27 → 24 против 27 → 23; lambda 12 → 12 против 12 → 9; comment 9 → 5 против 9 → 9; вся панель A 20 токенов, B 21.

**Граница:** Всего одно слияние; vocab320 и цепочка KNN не проверены. Это компромисс между профилями, а не доказательство ошибки стандартного BPE.

**Математическая связь:** Из описанного кода, не цитата книги: Для серии длины L имеется L−1 соседних одинаковых пар, но MergeChunk с шагом 2 исполнит floor(L/2) замен; меняется критерий выбора пары.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L295-L301)
- [experiments-v2/RP01-MICRO/BR01/r/0001/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/BR01/r/0001/report.md)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/BR01/r/0001/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/BR01/r/0001/report.md)

<a id="c-grad-accum-copy-01"></a>
## C-GRAD-ACCUM-COPY-01: Убирать лишнюю копию первого вклада градиента

**Этап:** backprop. **Результат:** Меньше затрат. **Основание:** первичный отчёт.

Первый вклад градиента передаётся конструктору Tensor без внешнего ToArray. Внутренняя защитная копия остаётся, а следующие вклады по-прежнему суммируются.

**Измерение:** Одно DPO-обновление: 1 847 691 280 → 1 761 735 608 байт (−85 955 672; −4,65%). Веса, loss, отступ и учебная панель побитово совпали.

**Граница:** GC 110/10/4 → 114/14/8; удержанная память сразу после шага выше на 28 461 768 байт, после полной уборки разница 1560. Многошаговая пропускная способность не проверена.

**Математическая связь:** Из описанного кода, не цитата книги: При первом вкладе Grad=copy(g), затем Grad+=g_next; устранение второй одинаковой копии не меняет сумму производных.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L302-L308)
- [experiments-v2/RP01-MICRO/AGC1/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/AGC1/results/report.md)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/AGC1/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/AGC1/results/report.md)

<a id="c-frozen-broadcast-map-01"></a>
## C-FROZEN-BROADCAST-MAP-01: Не строить обратную карту для замороженного broadcast

**Этап:** backprop, normalization. **Результат:** Меньше затрат. **Основание:** сводное исследование.

Если оба множителя не требуют градиента, выполняется только прямое умножение без таблицы индексов для обратного прохода. Ветка с обучаемыми входами остаётся прежней.

**Измерение:** 1 847 680 544 → 1 837 788 968 выделенных байт (−9 891 576; −0,535%); 40 замороженных вызовов. Конечные веса, loss и ответы побитово равны.

**Граница:** Шаг +201,3 мс, CPU +1640,6 мс и больше GC в одном холодном запуске; ускорение не установлено. Длинные последовательности и многошаговое обучение не проверены.

**Математическая связь:** Из описанного кода, не цитата книги: Прямой результат a_i·b_map(i) тот же; если оба RequiresGrad=false, таблица map для будущего обратного прохода не используется.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L309-L315)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/FBM1/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/FBM1/results/report.md)

<a id="c-vt-apostrophe-01"></a>
## C-VT-APOSTROPHE-01: Сохранять апостроф внутри термина проверяющего

**Этап:** labeling, memory. **Результат:** Выигрыш с потерей. **Основание:** сводное исследование.

Разбор замечания извлекает весь термин из точной рамки expected, включая внутренний апостроф. Проверяющий выучил целый термин brother's, но буквальное правило стало отвергать допустимые варианты.

**Измерение:** Сохранение термина A 1/2, B 2/2; ложные принятия 2/3 → 0/3, притяжательные формы 2/4 → 4/4. Ложные отказы 0/5 → 2/5, общая точность 6/8 в обоих.

**Граница:** В исходном опыте ожидается ASCII brother's; допустимый типографский brother’s и перефразировка отклонены. Генератор, полный переводческий поток и другие языки не проверены.

**Математическая связь:** Из описанного кода, не цитата книги: Сохранение целой строковой метки устраняет ошибку парсинга; буквальное равенство терминов не эквивалентно смысловой равнозначности перевода.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L316-L322)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/QVA01/results/a01/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/QVA01/results/a01/report.md)

<a id="c-rms-row-reduce-01"></a>
## C-RMS-ROW-REDUCE-01: Суммировать последнюю ось прямым чтением строки

**Этап:** normalization. **Результат:** Меньше затрат. **Основание:** сводное исследование.

Для подходящей формы тензора сумма последней оси читается последовательно без массивов координат. Порядок сложения сохранён, остальные формы используют прежний путь.

**Измерение:** 4 обновления: 13 640 016 → 12 984 816 байт (−655 200; −4,8%). Loss, веса, optimizer step 404 и 16×256 логитов побитово равны.

**Граница:** Пик +245 760 байт; наблюдаемое снижение времени одной холодной пары не устанавливает причинность. Другие формы, DPO и вывод без обучения не проверены.

**Математическая связь:** Из описанного кода, не цитата книги: mean(x)=Σ_k x_k/D; меняется вычисление адреса x_k, но не порядок суммы, делитель D или обратный проход.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L323-L329)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/RD01/a001/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/RD01/a001/results/report.md)

<a id="c-attn-transpose-strides-01"></a>
## C-ATTN-TRANSPOSE-STRIDES-01: Переставлять оси голов внимания прямой адресацией

**Этап:** attention, backprop. **Результат:** Меньше затрат. **Основание:** сводное исследование.

Для обмена второй и третьей осей четырёхмерного тензора адрес вычисляется арифметически. Сами значения и порядок перестановки сохраняются, поэтому функция внимания не меняется.

**Измерение:** 4 шага: 13 640 016 → 12 457 296 байт (−1 182 720; −8,67%); на чтениях −645 120 байт. Потери, параметры, optimizer и logits побитово равны.

**Граница:** Время обучения +4,26 мс, CPU +31,25 мс в одной холодной паре. Прочие ранги, оси, длины, вывод без обучения и DPO не проверены.

**Математическая связь:** Из описанного кода, не цитата книги: T[b,a,c,d]→T′[b,c,a,d] — та же перестановка индексов; матрица вероятностей внимания и её формула не изменяются.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L330-L336)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/AT01/a001/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/AT01/a001/results/report.md)

<a id="c-json-options-reuse-01"></a>
## C-JSON-OPTIONS-REUSE-01: Повторно использовать настройки JSON для уроков

**Этап:** pipeline, memory. **Результат:** Меньше затрат. **Основание:** сводное исследование.

Executor использует один неизменяемый набор настроек JSON вместо создания нового на каждый урок. Разбор и выученная переводческая память остаются прежними, а служебных объектов становится меньше.

**Измерение:** 8 уроков: 873 496 → 334 336 байт (−539 160; −61,7%). Память 0 → 8 точных соответствий, 16 фраз и 24 троек; оба плеча 19/24 верных, ответы совпали.

**Граница:** Три краевых ответа ошибочно приняты в обоих плечах; пик +1 703 936 байт. Ускорение одной холодной пары не установлено, остальные пути обучения не проверены.

**Математическая связь:** N/A: повторно используется одинаковая конфигурация парсера; математическая модель и содержимое уроков не меняются.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L337-L343)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/JO1/a001/result/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/JO1/a001/result/report.md)

<a id="c-dpo-shifted-context-01"></a>
## C-DPO-SHIFTED-CONTEXT-01: Проверять вместимость по сдвинутому входу DPO

**Этап:** dpo, pipeline. **Результат:** Механика без освоения задачи. **Основание:** сводное исследование.

Для предсказания следующего токена вход содержит все токены кроме последнего, а цели сохраняются полностью. Исправленный допуск позволил исполнить пограничный урок, но нужные конечные цифры не выучились.

**Измерение:** Длина 65 при окне 64: A дважды отказывает и делает 0 обновлений, B — 200. На длине 64 log-prob одинаковы, 224 компоненты градиента в пределах 3·10⁻⁷; точные ответы 0/2 → 0/2.

**Граница:** Учебная цель открыта, несмотря на изменение четырёх адаптеров. Эквивалентность подтверждена только на назначенном контрольном уроке.

**Математическая связь:** Из описанного кода, не цитата книги: Вход x₀…x_{T−2} предсказывает цели x₁…x_{T−1}; необходимое условие вместимости T−1≤contextLength при T≥2.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L344-L350)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/SC01/results/attempt-0001/report.ru.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/SC01/results/attempt-0001/report.ru.md)

<a id="c-identity-train-index-01"></a>
## C-IDENTITY-TRAIN-INDEX-01: Заменить поиск учебной записи дополнительным набором ключей

**Этап:** memory, state. **Результат:** Польза не показана. **Основание:** сводное исследование.

Для проверки уже выученных строк добавлен отдельный набор ключей вместо просмотра журнала. На измеренном размере стоимость создания и восстановления индекса превысила экономию четырёх проверок.

**Измерение:** Качество одинаково: 11/24 → 19/24, все 96 ответов равны. Восстановление 4 149 032 → 4 187 984 байта (+0,94%); обучение 483 816 → 492 016 (+1,69%).

**Граница:** 512 подготовительных предъявлений, где журнал растёт, не включены в A/B. Отрицательный результат относится к этому размеру и не доказывает отсутствие пользы на большом журнале.

**Математическая связь:** Из описанного кода, не цитата книги: Асимптотическое удешевление поиска не устраняет стоимость построения дополнительного индекса; при четырёх запросах она оказалась больше выгоды.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L351-L357)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/IDX01/a001/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/IDX01/a001/results/report.md)

<a id="c-silu-sigmoid-reuse-01"></a>
## C-SILU-SIGMOID-REUSE-01: Повторно использовать знаменатель сигмоиды в SiLU

**Этап:** activation, backprop. **Результат:** Выигрыш с потерей. **Основание:** сводное исследование.

В обучающей ветке сохраняется знаменатель сигмоиды, чтобы не считать экспоненту повторно при обратном проходе. Значения и градиенты совпали, но понадобилась дополнительная память.

**Измерение:** 4 обновления: 22,1668 → 19,9589 мс (−9,96%); +59 232 байта. Четыре loss, 896 весов, 7384 байта optimizer и 2048 логитов побитово равны.

**Граница:** Один прогон A→B; величина ускорения завышена относительно цены удалённых экспонент, подтверждено только наблюдаемое направление. Вывод без обучения не проверялся.

**Математическая связь:** Из описанного кода, не цитата книги: SiLU(x)=xσ(x), производная σ(x)+xσ(x)(1−σ(x)); σ=1/(1+exp(−x)), поэтому тот же сохранённый знаменатель даёт ту же формулу.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L358-L364)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1/report.md)

<a id="c-hnsw-query-norm-reuse-01"></a>
## C-HNSW-QUERY-NORM-REUSE-01: Считать норму HNSW-запроса один раз

**Этап:** knn, retrieval. **Результат:** Польза не показана. **Основание:** сводное исследование.

При обходе графа норма неизменного запроса вычисляется один раз и используется в сравнениях. На графе из восьми узлов уменьшение повторных вычислений не дало измеренной выгоды.

**Измерение:** Построений нормы 56 → 14; все 24 ответа и уверенности равны. Точных 8/8, близких 3/3, составных 0/8, краевых 0/5; время 3,1809 → 3,9071 мс, +56 байт.

**Граница:** Причинность разницы времени отвергнута: этап без сравнений замедлился сходно, тёплая панель дала обратный знак. Большие графы не проверены.

**Математическая связь:** Из описанного кода, не цитата книги: cos(q,x)=(q·x)/(||q||·||x||): ||q|| постоянно при одном запросе, но экономия арифметики не гарантирует меньшую полную стоимость на малом графе.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L365-L371)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/HNQ01/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/HNQ01/results/report.md)

<a id="c-identity-m1-01"></a>
## C-IDENTITY-M1-01: Искать тело плана только внутри выбранного маршрута

**Этап:** retrieval, memory, knn. **Результат:** Рост качества на панели. **Основание:** сводное исследование.

После выбора маршрута поиск готового тела плана ограничивается тем же маршрутом. Это устранило выдачу чужого тела под правильным заголовком при неизменной обученной памяти.

**Измерение:** Верных ответов A 2/3, B 3/3, потерь нет; последний поиск выделил на 22 208 байт меньше. Обученные состояния и расписки обучения совпали.

**Граница:** Без фильтра появляется постоянная цена +8 байт. Разница времени первого обучения +8,2 мс не приписывается фактору; панель мала.

**Математическая связь:** Из описанного кода, не цитата книги: argmax similarity(query,candidate) берётся по кандидатам с flowRef выбранного маршрута; исключаются несовместимые, хотя и похожие тела.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L372-L378)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/IDENTITY-M1-01/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/IDENTITY-M1-01/report.md)

<a id="c-kn-router-learned-key-01"></a>
## C-KN-ROUTER-LEARNED-KEY-01: Одинаково нормализовать названия технологий при обучении и запросе

**Этап:** representation, retrieval, memory. **Результат:** Рост качества на панели. **Основание:** сводное исследование.

Читатель заменяет C# на csharp и .NET на dotnet так же, как писатель учебного профиля. Уже выученные ключи стали находиться в пользовательском запросе без изменения памяти.

**Измерение:** Верных маршрутов 2/4 → 4/4, отказов 2 → 0, неверных 0. Уверенность C# .NET: 0,200 → 0,440 при пороге 0,40; память одинаковая, 426 байт.

**Граница:** Цена двух замен не измерена надёжно. Разница времени обучения не относится к фактору, поскольку обучение плеч одинаково.

**Математическая связь:** Из описанного кода, не цитата книги: Согласование φ_train(text)=φ_query(text) для одинаковых названий предотвращает искусственное несовпадение ключей; порог и правило оценки сохранены.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L379-L385)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/KR01/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/KR01/results/report.md)

<a id="c-pos-query-set-once-01"></a>
## C-POS-QUERY-SET-ONCE-01: Строить набор POS-пар запроса один раз

**Этап:** representation, knn. **Результат:** Меньше затрат. **Основание:** сводное исследование.

Набор пар соседних POS-меток запроса строится один раз перед сравнением со всеми уроками. Расстояния и голосование не меняются, поэтому ответы памяти сохраняются.

**Измерение:** Память 0 → 128 строк; 28/32 верных в обоих плечах, все 64 ответа равны. Выделения чтения 3 050 752 → 1 799 040 байт (−41%); время 20,5 → 16,1 мс.

**Граница:** 26 из 32 экзаменационных строк дословно встречаются в обучении, поэтому перенос навыка не доказан. Время измерено одной парой прогонов.

**Математическая связь:** Из описанного кода, не цитата книги: При фиксированном запросе множество Q одинаково для всех кандидатов; повторное использование сохраняет пересечение и объединение в вычислении расстояния.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L386-L392)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/report.md)

<a id="c-file-static-receiver-01"></a>
## C-FILE-STATIC-RECEIVER-01: Не требовать null-проверку для статического поля

**Этап:** labeling, knn. **Результат:** Рост качества на панели. **Основание:** сводное исследование.

Разметчик исключает доступ к полю, которое Roslyn определил как статическое, из признака обращения к получателю. Ложных советов о null стало меньше, но разметка существенно подорожала.

**Измерение:** Верных ответов 7/11 → 10/11, ложных null-требований 4 → 1; изменились 3 назначенные метки. Разметка 9,88 → 916,02 мс и 85 000 → 3 329 632 байта.

**Граница:** Весь выигрыш получен на обученных строках. Контроль коллизии всё ещё неверен, поскольку входные признаки не видят static; цена семантического анализа велика.

**Математическая связь:** Из описанного кода, не цитата книги: Доступ к статическому полю не требует непустого экземпляра-получателя; такая операция сама по себе не обосновывает метку «добавить null-проверку».

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L393-L399)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/FSR01/a001/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/FSR01/a001/results/report.md)

<a id="c-rms-mean-backward-01"></a>
## C-RMS-MEAN-BACKWARD-01: Вычислять адрес обратного усреднения делением

**Этап:** normalization, backprop. **Результат:** Меньше затрат. **Основание:** сводное исследование.

Для усреднения последней сохранённой оси адрес входящего градиента получается делением плоского индекса. Два временных массива координат исчезают, а множитель и накопление градиента остаются прежними.

**Измерение:** 4 обновления: 13 642 496 → 12 782 336 байт, −860 160 = 21 504×40 байт. Обучение побитово совпало; время 74,021 → 77,774 мс.

**Граница:** Ускорение не заявляется. Оптимизирована только охраняемая ветка согласованных форм; остальные формы сохраняют общий код.

**Математическая связь:** Из описанного кода, не цитата книги: Для mean по D элементам каждый получает upstream/D; srcFlat//D выбирает тот же upstream при сохранённой последней оси.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L400-L406)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/MB01/a001/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/MB01/a001/results/report.md)

<a id="c-sft-exp-reuse-01"></a>
## C-SFT-EXP-REUSE-01: Повторно использовать экспоненты softmax в SFT-градиенте

**Этап:** backprop, pipeline. **Результат:** Меньше затрат. **Основание:** сводное исследование.

Экспоненты запоминаются при суммировании знаменателя и затем используются в цикле градиента. Повторное вычисление исчезает при сохранении порядка суммирования и всех обучающих результатов.

**Измерение:** За 4 обновления убрано 35 840 из 71 680 экспонент; время 39,647 → 27,281 мс, на тёплых обновлениях −1,245 мс. Обучение побитово совпало; расчётная добавка 4192 байта.

**Граница:** Одна пара с жёстким порядком, общая разница завышена холодным стартом; добавка памяти не разрешается измеренным разбросом. Старт имеет только 3/40 правильных знака, прироста точности от фактора нет.

**Математическая связь:** Из описанного кода, не цитата книги: pᵢ=eᵢ/Σⱼeⱼ, eᵢ=exp(zᵢ−max z); для cross-entropy ∂L/∂zᵢ=pᵢ−tᵢ, поэтому те же eᵢ можно использовать в обоих циклах.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L407-L413)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/SEXP01/results/attempt-0001/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/SEXP01/results/attempt-0001/report.md)

<a id="c-parent-adoption-01"></a>
## C-PARENT-ADOPTION-01: Принимать и автоматически находить полный обученный комплект

**Этап:** state, pipeline. **Результат:** Частичное обучение/перенос. **Основание:** сводное исследование.

Проверенный кандидат принимается полным комплектом весов, моментов, шага, словаря и случайности. Обнаружение выбирает последний принятый комплект по порядку принятия и действительно находит обученный старт.

**Измерение:** Выбранный комплект 1 → 2, шаг AdamW 0 → 1000; NLL учебных ответов 6,689562 → 4,766696, ранг 139,300 → 51,825, верных знаков 0/40 → 3/40.

**Граница:** Полных ответов 0/2 в обоих плечах, экзамен и контроль не улучшились. Это частичный обученный старт и механизм выбора артефакта; перенос в продукт не выполнен.

**Математическая связь:** N/A: изменён жизненный цикл и выбор сохранённого состояния, а не формула обучения. Номер шага не заменяет порядок принятия артефакта.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L414-L420)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/PA01/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/PA01/results/report.md)

<a id="c-n63-ingress-length-learned-01"></a>
## C-N63-INGRESS-LENGTH-LEARNED-01: Допускать короткие повторные уроки из обученного старта

**Этап:** pipeline, state. **Результат:** Частичное обучение/перенос. **Основание:** сводное исследование.

Длинная строка откладывается до вызова обучателя, а каждое предъявление получает уникальный номер в происхождении источника. От общего обученного родителя это позволило выполнить короткие уроки и немного улучшить их токены.

**Измерение:** A: отказ и 0 обновлений; B: 200 обновлений. Верные знаки 3/42 → 4/42, NLL 4,800814 → 4,727502; ранг контрольной строки ухудшился 96,52 → 100,97.

**Граница:** Полных ответов 0/2, на экзамене 0 верных знаков, 100 предъявлений длинной строки остались необученными. Менялись два фактора вместе: допуск и уникальность источника.

**Математическая связь:** N/A: допуск и различение предъявлений разрешают исполнение уроков, но сами по себе не гарантируют правильный полный ответ.

Источники:
- [docs/plans/replanning-possible-diffs/required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/required-changes.md#L421-L426)

Первичные пути для проверки (наличие ссылки не повышает указанный уровень проверки):
- [experiments-v2/RP01-MICRO/N63IL/results-after-fix/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/N63IL/results-after-fix/report.md)

<a id="m-e18-domain-knn"></a>
## M-E18-DOMAIN-KNN: E18: выбрать домен запроса по предикатам и соседним примерам

**Этап:** representation, memory, knn. **Результат:** Рост качества на панели. **Основание:** raw и код.

Простое правило по ключевым словам сравнили с памятью размеченных предикатов запроса: пять ближайших примеров по Жаккару голосуют за домен. Улучшился выбор класса задачи, а не свободная генерация кода; положительный майский результат следует объяснять исторической реализацией, поскольку поздний одноимённый runner изменён.

**Измерение:** Run 2026-05-01-002: 33/40 → 38/40 (82,5% → 95%); всего 200 синтетических примеров, 160 train и 40 validation.

**Граница:** 25 шаблонов со слотами, без разделения по независимым семействам. Предикаты выделяются простыми английскими группами признаков; это не выученный нейронный encoder. Исторический код проверен на SHA530c1e0; полная идентичность всех старых binaries не восстановлена.

**Математическая связь:** Формула из алгоритма: J(P,Q)=|P∩Q|/|P∪Q|; класс — большинство среди k=5 ближайших. BOOK-FORMULAS B04 объясняет зависимость похожести от признаков; книга не доказывает численный gain E18.

Источники:
- [35.ExperimentsV2/results/E18-DOMAIN-CLASSIFIER/results/run-2026-05-01-002.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/results/E18-DOMAIN-CLASSIFIER/results/run-2026-05-01-002.json)
- [35.ExperimentsV2/Infrastructure/DomainClassifier.cs](https://github.com/lubochka/llm_mvp_core/blob/530c1e0581fa854574bd8608a7576a80236f2ebd/35.ExperimentsV2/Infrastructure/DomainClassifier.cs#L25-L67)
- [35.ExperimentsV2/Experiments/E18DomainClassifier.cs](https://github.com/lubochka/llm_mvp_core/blob/530c1e0581fa854574bd8608a7576a80236f2ebd/35.ExperimentsV2/Experiments/E18DomainClassifier.cs#L70-L95)

[Связанный раздел объяснения](SIMPLE-EXPLANATION.ru.md#selection).

<a id="m-e22-dataset-knn"></a>
## M-E22-DATASET-KNN: E22: несколько наборов данных выбираются отдельными голосованиями

**Этап:** representation, labeling, memory, knn. **Результат:** Рост качества на панели. **Основание:** raw и код.

Вместо трёх самых частых наборов данных выбирают соседние учебные примеры по предикатам и голосуют отдельно за каждый набор. Обучение сохраняет примеры с несколькими метками; эти метки создаёт AutoLabel, поэтому измеряется согласие с данной разметкой.

**Измерение:** Late run 2026-05-07-002: macro-F1 0,4709 → 0,5646; train 360, validation 90. F1: Generic 1,00; CSharp 0,99; TypeScript 0; BugFix 0,82; Orchestration 0,57; Tests 0.

**Граница:** Это средняя F1, не процент правильных свободных ответов. Два класса имеют F1=0; labels автоматические. Результат E22 не означает, что его обученный объект использовался в E24 или в конкретном пользовательском Flow.

**Математическая связь:** Фактическое правило: y_j=1[Σ_{i∈N3(q)} y_ij ≥2]; соседи определяются расстоянием 1−Jaccard. Это формула кода, не theorem книги; связь с BOOK-FORMULAS B04.

Источники:
- [35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json)
- [22.AgentAlgorithms/Context/PredicateDatasetRouter.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/22.AgentAlgorithms/Context/PredicateDatasetRouter.cs#L25-L90)
- [35.ExperimentsV2/Experiments/E22DatasetRouter.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Experiments/E22DatasetRouter.cs)

[Связанный раздел объяснения](SIMPLE-EXPLANATION.ru.md#selection).

<a id="m-e23-template-projection"></a>
## M-E23-TEMPLATE-PROJECTION: E23: отбор признаков перед поиском готового шаблона

**Этап:** representation, retrieval, pipeline. **Результат:** Рост качества на панели. **Основание:** raw и код.

Входные предикаты ограничивают словарём фиксированной библиотеки, затем выбирают шаблон с максимальным Жаккаром. Это подбор готового шаблона без обучения нейронных весов; сравнение выполнено со случайным выбором шаблона подходящего стека.

**Измерение:** Run 2026-05-01-002: template match 6/56 → 56/56, библиотека 14 шаблонов. Отдельная syntax-parse метрика 96,4% → 100%.

**Граница:** Ожидаемый template строится из тех же predicates. Random baseline слабый, A/B не выделяет отдельный вклад projection. Parse без ошибок не доказывает выполнение программы или правильность её смысла.

**Математическая связь:** BOOK-FORMULAS B05: при R=P∩V, N=P\V, T⊆V имеем J(P,T)=|R∩T|/(|R∪T|+|N|), а после projection знаменатель без |N|. Каждый score не уменьшается, но правильное ранжирование всех кандидатов не гарантировано; это вывод из кода.

Источники:
- [35.ExperimentsV2/results/E23-BEAUTIFIER/results/run-2026-05-01-002.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/results/E23-BEAUTIFIER/results/run-2026-05-01-002.json)
- [35.ExperimentsV2/Experiments/E23Beautifier.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Experiments/E23Beautifier.cs#L57-L155)

[Связанный раздел объяснения](SIMPLE-EXPLANATION.ru.md#selection).

<a id="m-e24-attributed-routing"></a>
## M-E24-ATTRIBUTED-ROUTING: E24: поздний выигрыш сегментации не доказывает пользу обученного KNN

**Этап:** representation, retrieval, pipeline. **Результат:** Рост качества на панели. **Основание:** raw и код.

Ранний E24 ухудшал показатель шума, но существует поздний положительный receipt нового пути сегментации и маршрутизации. В прочитанном runner обучают PredicateDatasetRouter, а измеряют отдельный AttributedSegmentRouter без передачи ему этой памяти; известный ExistingDomain имеет приоритет.

**Измерение:** May1: noise 0,4121 → 0,5626, attribution 11/16. May7: noise 0,4121 → 0,125, attribution 12/16, passed=true.

**Граница:** Только пять фиксированных случаев. Noise зависит от длин, attribution проверяет первое слово; короткий неверный фрагмент может получить noise=0. Установлен выигрыш узкой метрики нового пути, не причинный эффект KNN обучения и не сохранение всего смысла.

**Математическая связь:** Из кода: noise=max(0,1−|expected|/|actual|). BOOK-FORMULAS B09 даёт общую идею зависимости результата от изменённого состояния; конкретное отсутствие передачи trained object устанавливает source trace, не дифференцирование.

Источники:
- [35.ExperimentsV2/results/E24-ATTRIBUTED-ROUTER/results/run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/results/E24-ATTRIBUTED-ROUTER/results/run-2026-05-01-001.json)
- [35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E24-ATTRIBUTED-ROUTER/results/run-2026-05-07-001.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E24-ATTRIBUTED-ROUTER/results/run-2026-05-07-001.json)
- [35.ExperimentsV2/Experiments/E24AttributedRouter.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Experiments/E24AttributedRouter.cs#L28-L101)
- [23.SemanticCompilerAdapter/AttributedSegmentRouter.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/23.SemanticCompilerAdapter/AttributedSegmentRouter.cs#L82-L100)

[Связанный раздел объяснения](SIMPLE-EXPLANATION.ru.md#selection).

<a id="m-e25-pos-bigrams-pq01"></a>
## M-E25-POS-BIGRAMS-PQ01: E25/PQ01: память POS-шаблонов и экономия чтения запроса

**Этап:** representation, memory, knn. **Результат:** Частичное обучение/перенос. **Основание:** raw и код.

Rule baseline заменён памятью POS-последовательностей с голосованием трёх соседей по множествам соседних POS-пар. Поздний PQ01 показал, что прирост качества его панели целиком относится к знакомым последовательностям, и отдельно сократил выделения памяти, строя множество запроса один раз.

**Измерение:** E25: 11/32 → 28/32. В PQ01 знакомые: 9/26 → 26/26, новые: 2/6 → 2/6. При одинаковых ответах A/B allocations обученного чтения: 3 050 752 → 1 799 040 bytes (−41,0%).

**Граница:** E25 меняет всю комбинацию памяти, признаков и classifier; отдельная причинность bigrams не изолирована. POS-пары не хранят всю последовательность, кратность или род. PQ01 overlap и allocations проверены по первичному отчёту; family-heldout перевод не измерен.

**Математическая связь:** B(x)={(p_i,p_{i+1})}, d=1−J(B(x),B(z)); затем majority top3. BOOK-FORMULAS B06/Proposition5.15 объясняет важность различимости порядка, но не доказывает успешность этих признаков. PQ01 сохраняет ту же функцию, переиспользуя готовый B(query).

Источники:
- [35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json)
- [12.SemanticCompiler/Translation/PatternClassifier.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/12.SemanticCompiler/Translation/PatternClassifier.cs#L22-L113)
- [35.ExperimentsV2/Experiments/E25PatternClassifier.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Experiments/E25PatternClassifier.cs#L20-L47)
- [experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/report.md)

[Связанный раздел объяснения](SIMPLE-EXPLANATION.ru.md#pos).

<a id="m-e26-context-dictionary"></a>
## M-E26-CONTEXT-DICTIONARY: E26: значение многозначного слова выбирается по контексту

**Этап:** representation, memory, retrieval. **Результат:** Рост качества на панели. **Основание:** raw и код.

Baseline выбирал наиболее приоритетный перевод слова, а улучшенный reader учитывал части речи слева и справа. Подходящая запись находится по минимальному штрафу контекста в заранее подготовленном словаре; этот результат проверяет выбор значения, а не приобретение новых смыслов нейросетью.

**Измерение:** Run 2026-05-01-003: disambiguation 6/15 → 15/15; hard cases 6/6.

**Граница:** BuildDefault создаёт готовые словарные записи; сравнение не доказывает DPO acquisition. Реальный код выбирает ближайшую запись с приоритетом при ничьей, а не голосование k записей. Только 15 заданных случаев.

**Математическая связь:** Из кода: y(q)=y_argmin_e d(context(q),context(e)); context distance — сумма явных штрафов. Книга не доказывает правильность default dictionary; общая связь с B04 — значимость представления для поиска.

Источники:
- [35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-003.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-003.json)
- [35.ExperimentsV2/Experiments/E26ContextualDictionary.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs)
- [35.ExperimentsV2/Infrastructure/ContextualDictionary.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Infrastructure/ContextualDictionary.cs#L27-L112)

[Связанный раздел объяснения](SIMPLE-EXPLANATION.ru.md#context).

<a id="m-e27-structure-reorder"></a>
## M-E27-STRUCTURE-REORDER: E27: применить заранее известную схему порядка слов

**Этап:** representation, pipeline. **Результат:** Рост качества на панели. **Основание:** raw и код.

Baseline оставляет слова на прежних местах; улучшенный алгоритм применяет фиксированное преобразование по уже заданному SourcePattern и POS. Это полезное переиспользование схемы порядка, но распознавание схемы из сырого текста и обучение новых правил данным опытом не проверены.

**Измерение:** Run 2026-05-01-001: exact match 2/7 → 7/7. Семь случаев, шесть структур; отдельный IsSVO=4/7.

**Граница:** POS и SourcePattern вручную даны в случаях. IMPERATIVE_VS удаляет местоимение, поэтому не все ветви являются перестановками. Современная минимальная E27 correction в TranslationTrainedUnits решает другую, более узкую задачу.

**Математическая связь:** BOOK-FORMULAS B07: для чистого изменения порядка Y=P_sX; для всей реализации Y=T_s(X), поскольку некоторые ветви удаляют элементы. Это условное преобразование при верной входной схеме, не theorem об универсальной грамматике.

Источники:
- [35.ExperimentsV2/results/E27-PATTERN-REORDERER/results/run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/results/E27-PATTERN-REORDERER/results/run-2026-05-01-001.json)
- [35.ExperimentsV2/Experiments/E27PatternReorderer.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Experiments/E27PatternReorderer.cs#L24-L112)
- [35.ExperimentsV2/Infrastructure/PatternReorderer.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Infrastructure/PatternReorderer.cs#L12-L84)

[Связанный раздел объяснения](SIMPLE-EXPLANATION.ru.md#selection).

<a id="m-e33-stop-rule-no-gain"></a>
## M-E33-STOP-RULE-NO-GAIN: E33: полезное stop-правило уже решало панель без прироста от KNN

**Этап:** representation, knn, pipeline. **Результат:** Польза не показана. **Основание:** raw и код.

Простая baseline и enhanced classifier дали одинаковую максимальную точность в сохранённой панели. Статус failed относится к отсутствию требуемого прироста; он не отменяет полезность уже существующего правила остановки.

**Измерение:** Run 2026-05-01-002: binary accuracy 100% → 100%, false-continue 0 → 0, passed=false; runtime 0 → 8,9 ms.

**Граница:** Generator повторяет 13 фиксированных шаблонов; универсальность правила не доказана. Фраза notes про +1% false_continue_reduction противоречит основным полям и не используется как результат.

**Математическая связь:** Элементарная граница Acc≤1 объясняет, почему на панели с baseline Acc=1 прирост невозможен. Это арифметика метрики, а не доказательство качества на всех будущих случаях.

Источники:
- [35.ExperimentsV2/results/E33-STOP-SIGNAL-DETECTOR/results/run-2026-05-01-002.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/results/E33-STOP-SIGNAL-DETECTOR/results/run-2026-05-01-002.json)
- [35.ExperimentsV2/Experiments/E33StopSignalDetector.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Experiments/E33StopSignalDetector.cs)
- [35.ExperimentsV2/Compatibility/StopSignalDetector.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Compatibility/StopSignalDetector.cs)

[Связанный раздел объяснения](SIMPLE-EXPLANATION.ru.md#evidence).

<a id="m-e40v2-trust-knn"></a>
## M-E40V2-TRUST-KNN: E40V2: уровни доверия по семи признакам источника

**Этап:** representation, labeling, memory, knn. **Результат:** Рост качества на панели. **Основание:** raw и код.

Простое правило «внутренний или официальный» сравнили с KNN по семи признакам, включая CI, тесты, свежесть и соответствие теме. Labels HIGH/MEDIUM/LOW заданы вручную в 14 шаблонах, которые повторяются в выборке; входного показателя confidence здесь нет.

**Измерение:** Run 2026-05-01-002: 14/21 → 21/21; train 79, validation 21; LOW 4/4, MEDIUM 8/8, HIGH 9/9.

**Граница:** Синтетические повторяемые templates и stratified split. Успех не доказывает истинность произвольного web source или ответа модели. Baseline и k5 различаются; «labels из confidence» — неверное описание этого E40V2.

**Математическая связь:** Из кода: d(x,z)=sqrt(Σ_j(x_j−z_j)^2), top5 и голосование. BOOK-FORMULAS B04 объясняет зависимость решения от метрики и features; математической гарантии реального доверия источнику нет.

Источники:
- [35.ExperimentsV2/results/E40V2-TRUST-SCORER/results/run-2026-05-01-002.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/results/E40V2-TRUST-SCORER/results/run-2026-05-01-002.json)
- [35.ExperimentsV2/Experiments/E40TrustScorerV2.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Experiments/E40TrustScorerV2.cs#L22-L140)

[Связанный раздел объяснения](SIMPLE-EXPLANATION.ru.md#selection).

<a id="m-e09-ast-structural-knn"></a>
## M-E09-AST-STRUCTURAL-KNN: E09: структурный сосед AST и несопоставимые поздние знаменатели

**Этап:** representation, memory, knn. **Результат:** Рост качества на панели. **Основание:** raw и код.

Вместо n-gram/co-occurrence baseline следующий ID типа узла предсказывается по ближайшим структурным векторам: роли, родителям, глубине и позиции. Малый запуск дал небольшой положительный результат; в большем сравнении baseline и KNN оценивались на разных количествах переходов.

**Измерение:** Tier1 2026-04-30-001: 33,68% → 34,72%, 968 узлов, 773 train pairs. Tier2: 24,13% → 35,07%, но current runner даёт baseline 1123 перехода против KNN 499.

**Граница:** Поле BpeId в runner — file-local ID SyntaxKind, не BPE-токен текста. Поздний процент не matched A/B. Малый runtime 0,5 → 54 ms; качество и скорость — разные показатели. Полная historical binary lineage не восстановлена.

**Математическая связь:** Из кода: d(x,z)=Σ_j w_j δ_j(x_j,z_j), затем большинство top5. Это weighted search по признакам, не softmax attention. Общая математическая связь — BOOK-FORMULAS B04.

Источники:
- [35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-04-30-001.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-04-30-001.json)
- [35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json)
- [35.ExperimentsV2/Experiments/E09StructuralKnn.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Experiments/E09StructuralKnn.cs)
- [12.SemanticCompiler/Prediction/StructuralKnn.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/12.SemanticCompiler/Prediction/StructuralKnn.cs#L27-L35)

[Связанный раздел объяснения](SIMPLE-EXPLANATION.ru.md#structure).

<a id="m-p125-phrase-memory-ingress"></a>
## M-P125-PHRASE-MEMORY-INGRESS: P125 и ingress: готовые части урока должны дойти до памяти перевода

**Этап:** memory, retrieval, knn, pipeline. **Результат:** Рост качества на панели. **Основание:** raw и код.

Восемь уроков записывают exact, phrase и KNN память, которую читает обычный Flow; соответствия частей заранее предоставлены учителем. Исправление ingress перестало терять phrase_mappings, поэтому существующий composer смог собирать новые сочетания знакомых действий и объектов; neural updates здесь нет.

**Измерение:** P125 B: 0/24 → 19/24; контроль A без уроков 0/24, target-swapped C 13/24. Ingress: 11/24 → 19/24; composition 0/8 → 8/8. Exact=8, phrases=16, KNN triples=24, WordMaps=0.

**Граница:** Ученик не вывел alignment самостоятельно. Actual inference: exact→phrase→word→KNN→fallback. Edges 0/5; ошибочно принятых после ingress 1→3: отрицание и вопрос превращаются в команду. Ни BPE, ни LoRA, ни нейронная матрица вероятностей не изменены.

**Математическая связь:** Формализация кода: M[s]=t; T(a,o)=M_action[a] || M_object[o] при подходящей схеме и сохранении всех важных частей. BOOK-FORMULAS B09 объясняет необходимость связи writer→state→consumer; это не нейронный градиент и не theorem общей корректности перевода.

Источники:
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/train.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/train.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/score-summary.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/score-summary.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/before/raw-outputs.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/before/raw-outputs.json)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/after/raw-outputs.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/after/raw-outputs.json)
- [27.TranslationDomain.Tools/Program.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/27.TranslationDomain.Tools/Program.cs#L4442-L4469)
- [27.TranslationDomain.Tools/Program.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/27.TranslationDomain.Tools/Program.cs#L4682-L4699)
- [27.TranslationDomain/TranslationTrainedUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L330)
- [27.TranslationDomain/TranslationTrainedUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L455)
- [27.TranslationDomain/TranslationTrainedUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/27.TranslationDomain/TranslationTrainedUnits.cs#L798-L858)

[Связанный раздел объяснения](SIMPLE-EXPLANATION.ru.md#translation).

<a id="m-p25-cold-memory-restore"></a>
## M-P25-COLD-MEMORY-RESTORE: P25: восстановить ту же переводческую память в новом процессе

**Этап:** state, memory, pipeline. **Результат:** Удержание приобретённого. **Основание:** первичный отчёт.

Новый процесс загрузил сохранённый P125 snapshot через native LoadCheckpointAsync и выполнил обычные запросы без новых уроков. Авторская и независимая проверки сообщают равенство состояния и всех 24 продуктовых ответов; сохранены как правильные переводы, так и прежние ошибки.

**Измерение:** 24/24 ответов совпали; качество осталось 19/24, принято 22/24. Exact=8, phrases=16, KNN triples=24; training, lessons, neural updates=0.

**Граница:** A — сохранённые observations, а не одновременно живой старый процесс. Cold означает новый процесс/owner, не очищенный OS file cache. Другие snapshots, языки, длительное удержание и tokenizer/corpus-hash rejection не проверялись; raw checkpoints заново не сравнивались этим проходом.

**Математическая связь:** Условная запись: D(E(M))=M ⇒ f(q,D(E(M)))=f(q,M) при прежнем consumer и остальных условиях. Это объяснение восстановления и проверяемое равенство состояния; не новый optimizer step.

Источники:
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md)
- [docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/independent-review/review.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/independent-review/review.md)

[Связанный раздел объяснения](SIMPLE-EXPLANATION.ru.md#translation).

<a id="m-ttrain-context-acquisition"></a>
## M-TTRAIN-CONTEXT-ACQUISITION: T-TRAIN: новые записи различают книгу и бронирование благодаря POS-контексту

**Этап:** representation, memory, retrieval. **Результат:** Рост качества на панели. **Основание:** raw и код.

Обе версии записали book→книга и book→бронировать; в A контексты пусты, в B сохранены части речи слева и справа. Reader и код одинаковы: различающий контекст меняет расстояния до записей, поэтому прежняя ничья разрешается в пользу нужного смысла.

**Измерение:** До обучения оба 0/4; после A 2/4, B 4/4. По уникальным входам 1/3 → 3/3, поскольку один запрос повторён. Для A расстояния 2 и 2, для B на проверенных контекстах 0 и 3.

**Граница:** Одно слово, два смысла, готовые массивы POS, три уникальных контекста. Это nearest-entry dictionary; поле k не используется для голосования. Потеря этих полей конкретным production caller не установлена; grammar/gender и нейронное обучение не измерены.

**Математическая связь:** Из кода: argmin_e d(C(query),C(entry)). Общее логическое следствие: одинаковые φ(x) дают одинаковый ответ одного детерминированного reader; новые контекстные координаты делают два смысла различимыми. B06 даёт аналогичную границу потери информации о порядке, не theorem данного словаря.

Источники:
- [experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/report.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/report.md)
- [experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/A/raw.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/A/raw.json)
- [experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/B/raw.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/B/raw.json)
- [35.ExperimentsV2/Experiments/Rp01Micro/TTrainContext/ContextualDictionary.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Experiments/Rp01Micro/TTrainContext/ContextualDictionary.cs#L68-L112)

[Связанный раздел объяснения](SIMPLE-EXPLANATION.ru.md#context).

<a id="m-e06-id-collision-metric"></a>
## M-E06-ID-COLLISION-METRIC: E06: число 93,50% было показателем сжимаемости, а не точностью кода

**Этап:** representation, pipeline. **Результат:** Выигрыш с потерей. **Основание:** raw и код.

E06 в 34.Experiments сравнивает символьный поток с потоком типов AST и записывает нормированный показатель сжимаемости в поле accuracy. В 35.ExperimentsV2 номер E06 означает PageRank: короткий ID не идентифицирует механизм, поэтому результат нужно искать по пути, реализации и run.

**Измерение:** RESULT-E06-scale3: accuracy_pct 49,53 → 93,50, фактически bpc proxy; bpeBpc=3,533, astBpc=0,4547. Время 24,9 → 716 ms, allocations 0,94 → 10,75 MB.

**Граница:** AST поток содержит node.Kind(), теряя имена и литералы. Это не полный lossless размер исходника и не вероятность правильной генерации. Улучшение proxy сопровождается большими временем и выделениями; смешивать два E06 нельзя.

**Математическая связь:** Source metric: accuracy_pct=100·clamp(1−bpc/7,0,1), а astBpc=CE_per_node·nodes_per_char. Это определение вычисленного proxy из кода; формулы книги не превращают его в accuracy предсказания.

Источники:
- [experiments/results/RESULT-E06-scale3.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments/results/RESULT-E06-scale3.json)
- [34.Experiments/Experiments/E06AstRoslyn.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/34.Experiments/Experiments/E06AstRoslyn.cs#L21-L115)
- [docs/plans/replanning-possible-diffs/01-historical-experiments-and-comparability.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/01-historical-experiments-and-comparability.md)

[Связанный раздел объяснения](SIMPLE-EXPLANATION.ru.md#evidence).

<a id="m-e13b-exact-expression-memory"></a>
## M-E13B-EXACT-EXPRESSION-MEMORY: E13B: точная память выражений с нормализацией и отказом на новом

**Этап:** normalization, memory, retrieval. **Результат:** Частичное обучение/перенос. **Основание:** первичный отчёт.

Core записывает нормализованную левую часть выражения и готовое продолжение; при чтении использует точное совпадение ключа. Регистр и пробелы могут меняться, но неизвестная формула вызывает отказ, поэтому успешное воспроизведение знакомых записей не является выведением нового правила.

**Измерение:** Исходный retrieval 30/30 учебных строк. Later heldout: known 20/20; новых completions 0/10; корректных отказов 10/10; проверенные normalization variants корректны.

**Граница:** Heldout report считает отказ корректным поведением, а не правильным заполнением. Confidence=1 отмечает exact match. Первичные Markdown результаты и source прочитаны; новые runtime и независимое обобщение на неизвестные выражения не заявляются.

**Математическая связь:** Из кода: M[n(partial)]=completion; lookup(q)=M[n(q)] при наличии ключа, иначе отказ. Нормализация задаёт эквивалентность написаний, а не алгебраический вывод; общая связь с B09 — actual reader должен потреблять сохранённую память.

Источники:
- [experiments-v2/E13B-EXPRESSION-PREFIX-COMPLETION/results/run-2026-05-07-001715.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/E13B-EXPRESSION-PREFIX-COMPLETION/results/run-2026-05-07-001715.md)
- [experiments-v2/E13B-EXPRESSION-PREFIX-COMPLETION/results/heldout-2026-05-07-075333/e13b-heldout-validation.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/experiments-v2/E13B-EXPRESSION-PREFIX-COMPLETION/results/heldout-2026-05-07-075333/e13b-heldout-validation.md)
- [23.SemanticCompilerAdapter/ExpressionCompletionCore.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/23.SemanticCompilerAdapter/ExpressionCompletionCore.cs#L59-L104)

[Связанный раздел объяснения](SIMPLE-EXPLANATION.ru.md#memory).

<a id="m-e12-simd-sparse-cost"></a>
## M-E12-SIMD-SPARSE-COST: E12: та же арифметика быстрее только в части размеров

**Этап:** pipeline. **Результат:** Меньше затрат. **Основание:** raw и код.

Сравнили плотную и разреженную n-gram таблицы, а также обычное и SIMD умножение матрицы на вектор. Общая сумма времени уменьшилась за счёт большого матричного случая; два меньших подопыта стали медленнее, а языковое качество не измерялось.

**Измерение:** Run 2026-05-01-001: total 480,5661 → 275,6392 ms (−42,64%). Sparse 8,5→21,8 ms; SIMD 64×64 68,0→115,6 ms; SIMD 512×64 404,1→138,3 ms, лучший speedup≈2,9×.

**Граница:** Это микробенчмарки, не время полного обучения. Floating-point группировка может менять округление; результат не доказывает безусловную выгоду SIMD или sparse storage. Комментарий «4–8×» не подтверждён данным raw.

**Математическая связь:** y_i=Σ_j W_ij x_j: SIMD группирует те же произведения по блокам, sparse не хранит нулевые вклады. Эквивалентность в точной арифметике не означает одинаковое время; это объяснение source kernel, не новый способ обучения весов.

Источники:
- [35.ExperimentsV2/results/E12-MATRIX-OPT/results/run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/results/E12-MATRIX-OPT/results/run-2026-05-01-001.json)
- [35.ExperimentsV2/Experiments/E12MatrixOpt.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Experiments/E12MatrixOpt.cs)
- [35.ExperimentsV2/Core/SimdMatMul.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Core/SimdMatMul.cs#L17-L52)
- [35.ExperimentsV2/Core/SparseNgramTable.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Core/SparseNgramTable.cs)

[Связанный раздел объяснения](SIMPLE-EXPLANATION.ru.md#internals).

<a id="m-e48-early-transfer"></a>
## M-E48-EARLY-TRANSFER: E48: ранняя польза предварительного обучения при равном финале

**Этап:** representation, backprop, state. **Результат:** Частичное обучение/перенос. **Основание:** raw и код.

Маленькая сеть Embedding → Linear → ReLU → Linear сначала училась на прежних предикатах. На части новых данных предварительный старт давал больше правильных переходов, однако финальная точность совпала. Это ранний перенос, а не успешная LoRA или Transformer.

**Измерение:** После 56 новых предикатов около14% →34%; финал30,36% в обоих вариантах; warm дополнительно использовал600 прежних предикатов.

**Граница:** Маленькая панель переходов предикатов; прежний backward использует уже обновлённые матрицы. Экономия новых данных не равна экономии всей тренировки. passed=false относится к финалу.

**Математическая связь:** Теорема9.5 книги даёт градиент линейного выхода; вывод переноса через скалярное произведение требует фиксированных признаков и одного SGD-шага. Он не является точной формулой этой полной сети.

Источники:
- [35.ExperimentsV2/results/E48-DOMAIN-BOOTSTRAPPER/results/run-2026-05-01-001.json](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/results/E48-DOMAIN-BOOTSTRAPPER/results/run-2026-05-01-001.json)
- [35.ExperimentsV2/Experiments/E48DomainBootstrapper.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Experiments/E48DomainBootstrapper.cs)
- [35.ExperimentsV2/Core/ProbeNeuralNetwork.cs](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/35.ExperimentsV2/Core/ProbeNeuralNetwork.cs)
- [docs/plans/replanning-possible-diffs/54-e48-early-learning-and-probe-gradient-boundary.md](https://github.com/lubochka/llm_mvp_core/blob/916c93cc96a674fd65d82b382c5277532fc5b717/docs/plans/replanning-possible-diffs/54-e48-early-learning-and-probe-gradient-boundary.md)

[Связанный раздел объяснения](SIMPLE-EXPLANATION.ru.md#transfer).
