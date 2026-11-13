# 78. Применение новых выводов ко всему реестру переносов

Дата: 27 октября 2026. Основания — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), адресный ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md), исходный полный каталог — [75](75-transfer-catalogue-coverage-2026-10-27.md).

Охвачены **все 62 ID / 56 файлов-владельцев**. Это карта применимости новых защитных требований к планам, а не 62 новых source/runtime аудита. Ошибки подтверждены в названных в 76 файлах; аналогия не доказывает тот же дефект в остальных реализациях. Адресные блоки внесены в 37 родственных владельцев для 42 ID; карточка74 дополнительно получила конкретные требования KDP/KDR. Итого адресно усилены **38 owner-файлов / 44 ID**. Остальные 18 ID используют общий договор W в03: неприменимые им callbacks/DPO не добавляются искусственно. Таким образом, глубина поправки зависит от механизма, но ни один ID не исключён из таблицы.

## Применимые проверки

* **K:** зафиксированные при допуске данные/configuration, защищённые коллекции, cache identity, сохранение выбранного режима при restore. Контрпример из KDP — замена функции после Create и загрузка модели без каталога. Для других механизмов проверяется их собственный carrier; это не требование внедрить KNN повсюду.
* **M:** фаза измеренной метрики, отдельные before каждого этапа, отличие relative reward improvement от policy preference и actual correct. Контрпримеры P02: ×3,22 содержит уже полученный SFT-эффект; T2 неверен при DPO Accuracy=1.
  **Проверенный объём:** P0201/src/PreferenceSeparationReport.cs и его tests разделяют показатели; native DpoMetrics получил несериализуемый alias. Это не общий внедрённый механизм всех владельцев. Writer P0201 и тип агрегации требуют остаточного ремонта по84/85.
  **Уточнение84:** report key P0201 по шагам и native alias по парам имеют разные единицы. Историческое0,75 сохраняется с определением S, не выдаётся за канонический средний Accuracy. Новые поля должны различать R=weighted positive-pair fraction, S=positive-step-mean fraction, panel-pair fraction, preference win и actual correct; общий контракт03 и план85 дают расходящийся oracle. Ни один другой владелец не получает готовую формулу по аналогии.
* **W:** действующий writer воспроизводит исправленный отчёт; attempted/completed различаются; неизвестное измерение nullable; полная сумма вызовов не подменяется частным счётчиком. Контрпримеры: SftWallMs=0d в producer после исправления JSON, P01 dpo_ran=true при0updates, TTQ02 24 вместо84 total Forward.

Положительные результаты и старые runs сохраняются. Применение требований не запускает обучение и не добавляет бюджет к карточке. Unit-контроль коллекции/formatter не объявляется новым экспериментом; самостоятельный новый эксперимент обязательно содержит реальное обучение. Все адресные блоки названы `audit-76-specific-controls`, чтобы исполнителю было понятно, какое уточнение относится к текущему аудиту.

| ID / владелец | Группа | Где уточнено | Конкретная ось проверки |
|---|---|---|---|
| [FIX-KDP](74-knn-domain-constructor-transfer.md) | K, W | 74 + 77/R27-K | Каталог callbacks/Descriptor и параметры встроенных функций; настоящий RU/HE/OR consumer, persisted configuration. |
| [E2E-KDR](74-knn-domain-constructor-transfer.md) | K, W | 74 + 77/R27-K | После выбранного маршрута сохраняется вся связка model + profile + catalogue; TopProfile отдельно от CanSelect. |
| [FIX-MC](21-sft-optimizer-continuity.md) | M, W | Адресный блок в карточке + 03 | SFT optimizer step/moments и состояние измеренного loss; начальное состояние не выводить из имени файла. |
| [FIX-IL](../fixes-based-on-experimentsperiments/55-supervised-ingress-length.md) | M, W | Адресный блок в карточке + 03 | Число admitted/deferred не выдавать за completed updates; after относится к реально обработанному набору. |
| [FIX-SC](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | M, W | Адресный блок в карточке + 03 | Shifted-target этап и разные дозы A/B; pre-update reward не является after correctness. |
| [FIX-SEP](62-supervised-framing-and-learning-readout.md) | M, W | Адресный блок в карточке + 03 | Формат чтения и состояние весов совпадают; улучшение измерителя не переименовывать в новый навык. |
| [FIX-KHE](26-method-knn-training-admission.md) | K, W | Адресный блок в карточке + 03 | После отказа нового TRAIN сохранённые examples/labels остаются теми же; snapshot не отдаёт живой carrier. |
| [FIX-KS](35-method-knn-target-scope.md) | W | 03/W; собственные требования сохранены | Outer-body label gold и фактический consumer; сведения о завершении отражают native fit. |
| [FIX-FSR](../fixes-based-on-experimentsperiments/51-file-label-and-pos-reader.md) | W | 03/W; собственные требования сохранены | Label correctness и цена извлечения отдельно; большие alloc/wall ухудшения не терять в отчёте. |
| [FIX-IM](48-learned-route-body-identity.md) | K, W | Адресный блок в карточке + 03 | Полный body/provenance выбранного примера после обычного чтения и restore, не одна ссылка. |
| [FIX-KR](49-router-learned-key-normalization.md) | K, W | Адресный блок в карточке + 03 | Нормализованные ключи и profile identity сохраняются в native writer/reader, без silent default. |
| [FIX-HCL](53-hierarchical-local-history.md) | W | 03/W; собственные требования сохранены | Собственная история кластера, источник числа событий и фактическая прочитанная строка. |
| [FIX-SVD](../fixes-based-on-experimentsperiments/53-hierarchical-history-and-directed-svd.md) | W | 03/W; собственные требования сохранены | Направленный/симметричный oracle различимы; время/память имеют измерение и не заменяются нулём. |
| [FIX-PD](22-dpo-valid-length.md) | M, W | Адресный блок в карточке + 03 | Принятая длина пары отдельно от фактического update и after качества. |
| [FIX-LC](13-dpo-reference-cache.md) | K, M, W | Адресный блок в карточке + 03 | Ключ reference cache и frozen identity; cache hit не model Forward; метрика сохраняет фазу. |
| [FIX-TS](28-dpo-tokenizer-snapshot.md) | K, M, W | Адресный блок в карточке + 03 | Токенизатор и материал пары захвачены, изменение внешнего массива не меняет admitted snapshot. |
| [FIX-FBC](54-frozen-base-check-scope.md) | M, W | Адресный блок в карточке + 03 | Полный frozen state и фактическое after; число проверенных массивов не учебная метрика. |
| [FIX-OSR](58-optimizer-step-metadata-read.md) | M, W | Адресный блок в карточке + 03 | Optimizer step прочитан без смены семантики; returned DpoMetrics ещё относятся к pre-update. |
| [FIX-BWS](60-backward-softmax-scratch.md) | M, W | Адресный блок в карточке + 03 | Равенство native update и жизненный цикл scratch; actual calls отделены от планового верхнего лимита. |
| [FIX-IGC](61-initial-gradient-owned-copy.md) | M, W | Адресный блок в карточке + 03 | Владение initialGradient и защитная копия; численная эквивалентность после настоящего update. |
| [FIX-LFT](55-frozen-linear-transpose-cache.md) | K, M, W | Адресный блок в карточке + 03 | Кэш транспонированных frozen weights инвалидируется при законной смене identity; не фиксировать старые веса навсегда. |
| [FIX-AGC](41-grad-accum-copy.md) | M, W | Адресный блок в карточке + 03 | Накопленные градиенты и actual optimizer update; одинаковая сумма не доказывается одной подписью step. |
| [FIX-FBM](42-frozen-broadcast-map.md) | M, W | Адресный блок в карточке + 03 | Broadcast-карта и equality результата update; все стадии стоимости имеют собственные поля. |
| [FIX-CS](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | M, W | Адресный блок в карточке + 03 | Причинная маска, restored learner и actual after; pre-update diagnostics не закрывают конечную панель. |
| [FIX-AC](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | M, W | Адресный блок в карточке + 03 | Softmax-copy оптимизация сравнивается в одинаковом этапе с общим стартом, не по разной стадии loss. |
| [FIX-RRD](44-rms-row-reduce.md) | M, W | Адресный блок в карточке + 03 | Обученное before/after и равенство полного состояния; холодный 0→0 не квалификация. |
| [FIX-MB](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | M, W | Адресный блок в карточке + 03 | Backward среднего и реальное обновление; reader-панель отделена от gradient-side метрик. |
| [FIX-ATS](45-attention-transpose-strides.md) | M, W | Адресный блок в карточке + 03 | Перестановка осей и реальные after states; resource window одинаков для двух реализаций. |
| [FIX-RPE](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | M, W | Адресный блок в карточке + 03 | RMS parameter-gradient и завершённые updates; metric stage не выводится из времени возврата функции. |
| [FIX-SSR](47-silu-sigmoid-reuse.md) | M, W | Адресный блок в карточке + 03 | SiLU reuse без лишних readouts; объявленный нулевой backfill не обнуляет исходное обучение. |
| [FIX-SEXP](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | M, W | Адресный блок в карточке + 03 | SFT exponent reuse: training loss стадия и итоговый правильный ответ различаются. |
| [FIX-GB](29-matmul-right-gradient-locality.md) | M, W | Адресный блок в карточке + 03 | Matmul gradient locality: numeric parity не означает больше выученных ответов. |
| [FIX-NH](36-dpo-scored-row-head.md) | M, W | Адресный блок в карточке + 03 | Scored-row head и выбранные target positions; относительная DPO Accuracy не является token accuracy. Общее имя и общий расчёт доступны с 27.10.2026 (`reference_relative_improvement_rate`); собственная проверка этого владельца не выполнялась. |
| [FIX-RMS](30-broadcast-coordinate-allocation.md) | M, W | Адресный блок в карточке + 03 | Broadcast/coordinate оптимизация: совпадающий checkpoint и соответствующая ему конечная панель. |
| [FIX-BPE](12-bpe-no-match-copy.md) | W | 03/W; собственные требования сохранены | Настоящий BPE.Train и отдельная стоимость no-match; ratio повторов не его единственный oracle. |
| [FIX-BW](31-bpe-weighted-chunks.md) | W | 03/W; собственные требования сохранены | Вес повторяющегося фрагмента и ordered merges; writer показывает реальные Train calls. |
| [FIX-BR](40-bpe-realized-merge-objective.md) | W | 03/W; собственные требования сохранены | Realized objective и доменные ухудшения раздельно; один положительный агрегат не снимает ограничения. |
| [FIX-KHC](24-file-action-shared-parse.md) | W | 03/W; собственные требования сохранены | Общий parse действительно использован native fit; число parse не равно числу учебных примеров. |
| [FIX-AST](38-structural-knn-sibling-cache.md) | W | 03/W; собственные требования сохранены | Индекс sibling корректен на wide/narrow дереве; ресурсный writer воспроизводит реальные измерения. |
| [FIX-HNQ](50-hnsw-training-query-norm.md) | K, W | Адресный блок в карточке + 03 | Query/index identity при кэшировании нормы; wall time не переименовывать в CPU, unknown allocation не ноль. |
| [FIX-PQ](51-pos-query-bigram-reuse.md) | W | 03/W; собственные требования сохранены | Query tag pairs один раз на вопрос; стоимость и число native fits имеют правильный источник. |
| [FIX-JO](46-training-json-options-reuse.md) | K, W | Адресный блок в карточке + 03 | Сериализуемый учебный snapshot и options identity; валидный JSON не доказывает сохранение opt-in поведения. |
| [FIX-IDX](52-translation-training-identity-index.md) | K, W | Адресный блок в карточке + 03 | Индекс learned identity и реальные сохранённые Input/Output, без заполнения из fixture при restore. |
| [FIX-N60](11-translation-verifier-admission.md) | W | 03/W; собственные требования сохранены | Admitted pair count, verifier outcome и фактическое обучение не смешаны. |
| [FIX-VA](43-verifier-apostrophe-learning.md) | W | 03/W; собственные требования сохранены | Правильность апострофов и потерянные случаи сохраняются построчно; неверная подпись не успех learner. |
| [FIX-NKU](10-nku-do-while-label.md) | W | 03/W; собственные требования сохранены | Label do/while и consumed learned state; resource unknown не нулевой расход. |
| [FIX-NG](39-null-guard-training-label.md) | W | 03/W; собственные требования сохранены | False-label negative controls и completed native fits; учёт producer вместо готового JSON. |
| [FIX-RET](15-prompt-flow-retention.md) | W | 03/W; собственные требования сохранены | Удержанные ответы по каждому маршруту и реальный новый fit, а не только общий процент. |
| [FIX-WC](33-wiki-training-feature-reuse.md) | K, W | Адресный блок в карточке + 03 | Признаки урока захвачены для соответствующего корпуса; законное новое обучение обновляет snapshot. |
| [FIX-WK](32-wiki-feature-key-isolation.md) | K, W | Адресный блок в карточке + 03 | Feature codec/version переживает restore; label confidence не скрывает ошибку consumer. |
| [FIX-WT](14-wiki-routing-taxonomy.md) | K, W | Адресный блок в карточке + 03 | Taxonomy/feature configuration и обычный admitted route; descriptor/default не заменяет сохранённую конфигурацию. |
| [FIX-E2E](20-prompt-domain-e2e.md) | K, W | Адресный блок в карточке + 03 | Prompt → admitted route → выбранный state/config → output; восстановление не сбрасывает выбранный режим. |
| [FIX-N63C](17-dpo-sft-correction-collision.md) | M, W | Адресный блок в карточке + 03 | Свои before каждой стадии DPO/SFT и независимый правильный gold; последний returned reward не after. |
| [TEST-COLLISION-REPAIR](59-collision-repair-transfer.md) | M, W | Адресный блок в карточке + 03 | Парные приращения относительно начала каждой стадии; общий endpoint не приписывается последнему фактору. |
| [FIX-N63R](16-replay-domain-retention.md) | M, W | Адресный блок в карточке + 03 | Полная история предъявлений и момент чтения; усреднение не скрывает потерю другой строки. |
| [FIX-MS](23-sft-response-mask.md) | M, W | Адресный блок в карточке + 03 | Response-only loss и consumer correctness раздельно; численные изменения цели не требуют equality весов. |
| [TEST-POS](34-pos-bigram-learning-coverage.md) | W | 03/W; собственные требования сохранены | Native POS fit и различение SVO/VSO; тест reader по готовой константе не подтверждает acquisition. |
| [TEST-SK](37-structural-knn-learning-coverage.md) | W | 03/W; собственные требования сохранены | Native structural learner использует выученную память; структурный PASS отдельно от качества. |
| [TEST-ORDER](25-dpo-sft-order-learning-coverage.md) | M, W | Адресный блок в карточке + 03 | SFT/DPO порядок: этапные before/after и одинаковый reference; reward ImprovementRate не preference win. Общее имя и общий расчёт доступны с 27.10.2026 (`policy_preference_win_rate`); собственная проверка этого владельца не выполнялась. |
| [TEST-TCTX](18-contextual-dictionary-learning-coverage.md) | K, W | Адресный блок в карточке + 03 | Context/config и learned dictionaries восстанавливаются в том же профиле; тела ответов берутся из state. |
| [TEST-TMEM](19-translation-memory-coverage.md) | K, W | Адресный блок в карточке + 03 | Persisted translation memory и направление; новый default при load не считается тем же consumer. |
| [TEST-E22](27-predicate-dataset-router-learning-coverage.md) | W | 03/W; собственные требования сохранены | Dataset predicate routes: producer counts, реальный admitted route и конечный output различаются. |

## Проверка полноты

Идентичности и владельцы взяты из75 и действующего реестра, не вычислены из числа Markdown-файлов. Внешний файл41 владеет шестью ID;74 — двумя. Общие требования не приравниваются к внедрённому исправлению. Заключительное readback подтвердило **62 строки,62 уникальных ID,56 owner-путей**, точное содержание всех37 общих адресных блоков и наличие отдельного блока74. Проверка1270 локальных файловых целей ссылок в49 файлах не нашла отсутствующих путей; номера строк и смысл каждой исторической ссылки этой проверкой не доказываются. `git diff --check` с CRLF-aware настройкой вернул0. Новые тестовые прогоны этой документной сессией не выполнялись.

**Пометка 27 октября 2026 (кодовый слайс «разделить три разные величины»).** Предыдущее предложение описывает только документную сессию и больше не описывает состояние целиком: кодовый слайс того же дня прогон выполнил. Точечный запускатель `experiments-v2/_runners/Run-PreferenceSeparationTests.ps1` прошёл на отборе из четырёх классов проверок — разделение величин и три класса обучателя предпочтений — с результатом 103 пройдено, 0 упало, код выхода 0; полный вывод лежит в журнале `experiments-v2/_runners/Run-PreferenceSeparationTests.log`. В отбор входил и сторож побайтовой неизменности квитанции обучателя (`Tests/DpoObjectiveConsistencyTests.cs::LegacyJsonAndPublicAverage_AreUnchanged`), он пройден. Полный прогон всех проверок решения этим слайсом не выполнялся и здесь не заявляется.
