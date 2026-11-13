# 89. Применимость аудита P05/P10–P12 ко всему каталогу переносов

**Проверка последней редакции:** readback75 документов,236 ссылок, отсутствующих путей0; `git diff --check -- docs/plans/replanning-possible-diffs` завершился exit0. Работник дополнительно сверил учебную секцию P11 в88 с raw/source (версия101 строка, SHA256 `01053b5fd6f4f89a0e5b40eaaa6713a00553c5ab027755c7da9e34b8da003c5e`); нашёл неверный `P1101/src/Program.cs`. Родитель исправил его на root `P1101/Program.cs`, аналогичный P05 адрес сверил с commit files и явно указал P1101 BPE. Эти последующие адресные уточнения — родительская проверка, не дополнительный независимый проход.

**Финальный readback:** родитель нашёл все56 блоков и проверил229 локальных ссылок — отсутствующих путей0. Независимый работник `/root/p08_transfer_audit` подтвердил62 уникальных ID,56 владельцев и все owner-ссылки88. Он также прочитал исправленную редакцию88: AdamW400/scheduler0, точный csproj/S0/BPE/config/reader и бюджет32F сохранены. Это документная проверка, не runtime PASS.

28 октября 2026. Перечень сверён с текущими 62 stable ID и 56 файлами-владельцами из [86](86-reporting-sampling-cache-card-coverage-2026-10-27.md). Во всех владельцах добавлен блок `audit-87-applicability`, относящийся к их механизму. Четыре выполненных PRIMARY и общие входы исполнителя уточнены отдельно. Это полный охват требований документации, **не 62 независимых аудита реализации и не новые результаты запусков**.

Первичные источники новых выводов — [87](87-gradient-warmup-transfer-audit-2026-10-28.md); конкретный ремонт, файлы, контрпримеры и будущие бюджеты — [88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md). В текущей сессии product code, tests, runners и результаты не изменялись; сборки и обучение не запускались.

## Какие требования распространены

| Группа | Новая проверка и причина |
|---|---|
| M | Раздельная идентичность optimizer/scheduler/recipient: P12 импортирует AdamW400 при scheduler0, P10 использует fresh DPO0. Запланированный шаг и эффективное изменение весов не одно и то же. |
| F | Фактический loss, trainable keys и точный состав панели. P11 full-sequence embedding-only нельзя описывать response mask и прежними q/v keys. |
| E | Фактический consumer/config после маршрутизации; direct Warmup4 и factory Warmup0 не один путь. Restore требует содержимого snapshot, одного hash недостаточно. |
| C | Общий бюджет, включая обязательные контроли, исключения и отдельные model/layer calls. P11: 59 model Forward, а не только 40 основной части. FD/oracle должен влиять на приёмку. |
| T | Уникальные контрольные строки, неизменный gold, разные счётчики вызовов/фрагментов/merges и downstream результат реального обучения. |
| Q | Уникальный RowId, неизменные Kind/gold, per-row retention и constant-answer baseline. P05 Count не предотвращает дубликаты; P11 одинаковые 2/4 скрывают потерю E2. |

Для всех групп execution complete, training performed, qualification, hypothesis и transfer admission — разные поля. 1→0 после настоящего обучения является валидной регрессией. Наличие сдвига весов, нулевой exit code или правильный ответ S0 не закрывают автоматически учебную цель. Обязательный контроль проверяется отрицательным примером на том же действующем writer/consumer, а не отдельной копией условий только в тесте.

## Все владельцы

| Stable ID | Действующий документ | Группа |
|---|---|---|
| FIX-KDP | [74-knn-domain-constructor-transfer.md](74-knn-domain-constructor-transfer.md) | E |
| E2E-KDR | [74-knn-domain-constructor-transfer.md](74-knn-domain-constructor-transfer.md) | E |
| FIX-MC | [21-sft-optimizer-continuity.md](21-sft-optimizer-continuity.md) | M |
| FIX-IL | [55-supervised-ingress-length.md](../fixes-based-on-experimentsperiments/55-supervised-ingress-length.md) | F |
| FIX-SC | [41-neural-learning-resource-deltas.md](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | C |
| FIX-CS | [41-neural-learning-resource-deltas.md](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | C |
| FIX-AC | [41-neural-learning-resource-deltas.md](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | C |
| FIX-MB | [41-neural-learning-resource-deltas.md](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | C |
| FIX-RPE | [41-neural-learning-resource-deltas.md](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | C |
| FIX-SEXP | [41-neural-learning-resource-deltas.md](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | C |
| FIX-SEP | [62-supervised-framing-and-learning-readout.md](62-supervised-framing-and-learning-readout.md) | F |
| FIX-KHE | [26-method-knn-training-admission.md](26-method-knn-training-admission.md) | Q |
| FIX-KS | [35-method-knn-target-scope.md](35-method-knn-target-scope.md) | Q |
| FIX-FSR | [51-file-label-and-pos-reader.md](../fixes-based-on-experimentsperiments/51-file-label-and-pos-reader.md) | E |
| FIX-IM | [48-learned-route-body-identity.md](48-learned-route-body-identity.md) | E |
| FIX-KR | [49-router-learned-key-normalization.md](49-router-learned-key-normalization.md) | E |
| FIX-HCL | [53-hierarchical-local-history.md](53-hierarchical-local-history.md) | Q |
| FIX-SVD | [53-hierarchical-history-and-directed-svd.md](../fixes-based-on-experimentsperiments/53-hierarchical-history-and-directed-svd.md) | Q |
| FIX-PD | [22-dpo-valid-length.md](22-dpo-valid-length.md) | M |
| FIX-LC | [13-dpo-reference-cache.md](13-dpo-reference-cache.md) | M |
| FIX-TS | [28-dpo-tokenizer-snapshot.md](28-dpo-tokenizer-snapshot.md) | F |
| FIX-FBC | [54-frozen-base-check-scope.md](54-frozen-base-check-scope.md) | C |
| FIX-OSR | [58-optimizer-step-metadata-read.md](58-optimizer-step-metadata-read.md) | C |
| FIX-BWS | [60-backward-softmax-scratch.md](60-backward-softmax-scratch.md) | C |
| FIX-IGC | [61-initial-gradient-owned-copy.md](61-initial-gradient-owned-copy.md) | C |
| FIX-LFT | [55-frozen-linear-transpose-cache.md](55-frozen-linear-transpose-cache.md) | C |
| FIX-AGC | [41-grad-accum-copy.md](41-grad-accum-copy.md) | C |
| FIX-FBM | [42-frozen-broadcast-map.md](42-frozen-broadcast-map.md) | C |
| FIX-RRD | [44-rms-row-reduce.md](44-rms-row-reduce.md) | C |
| FIX-ATS | [45-attention-transpose-strides.md](45-attention-transpose-strides.md) | C |
| FIX-SSR | [47-silu-sigmoid-reuse.md](47-silu-sigmoid-reuse.md) | C |
| FIX-GB | [29-matmul-right-gradient-locality.md](29-matmul-right-gradient-locality.md) | C |
| FIX-NH | [36-dpo-scored-row-head.md](36-dpo-scored-row-head.md) | M |
| FIX-RMS | [30-broadcast-coordinate-allocation.md](30-broadcast-coordinate-allocation.md) | C |
| FIX-BPE | [12-bpe-no-match-copy.md](12-bpe-no-match-copy.md) | T |
| FIX-BW | [31-bpe-weighted-chunks.md](31-bpe-weighted-chunks.md) | T |
| FIX-BR | [40-bpe-realized-merge-objective.md](40-bpe-realized-merge-objective.md) | T |
| FIX-KHC | [24-file-action-shared-parse.md](24-file-action-shared-parse.md) | C |
| FIX-AST | [38-structural-knn-sibling-cache.md](38-structural-knn-sibling-cache.md) | C |
| FIX-HNQ | [50-hnsw-training-query-norm.md](50-hnsw-training-query-norm.md) | C |
| FIX-PQ | [51-pos-query-bigram-reuse.md](51-pos-query-bigram-reuse.md) | C |
| FIX-JO | [46-training-json-options-reuse.md](46-training-json-options-reuse.md) | C |
| FIX-IDX | [52-translation-training-identity-index.md](52-translation-training-identity-index.md) | C |
| FIX-N60 | [11-translation-verifier-admission.md](11-translation-verifier-admission.md) | Q |
| FIX-VA | [43-verifier-apostrophe-learning.md](43-verifier-apostrophe-learning.md) | Q |
| FIX-NKU | [10-nku-do-while-label.md](10-nku-do-while-label.md) | Q |
| FIX-NG | [39-null-guard-training-label.md](39-null-guard-training-label.md) | Q |
| FIX-RET | [15-prompt-flow-retention.md](15-prompt-flow-retention.md) | Q |
| FIX-WC | [33-wiki-training-feature-reuse.md](33-wiki-training-feature-reuse.md) | C |
| FIX-WK | [32-wiki-feature-key-isolation.md](32-wiki-feature-key-isolation.md) | E |
| FIX-WT | [14-wiki-routing-taxonomy.md](14-wiki-routing-taxonomy.md) | E |
| FIX-E2E | [20-prompt-domain-e2e.md](20-prompt-domain-e2e.md) | E |
| FIX-N63C | [17-dpo-sft-correction-collision.md](17-dpo-sft-correction-collision.md) | M |
| TEST-COLLISION-REPAIR | [59-collision-repair-transfer.md](59-collision-repair-transfer.md) | M |
| FIX-N63R | [16-replay-domain-retention.md](16-replay-domain-retention.md) | M |
| FIX-MS | [23-sft-response-mask.md](23-sft-response-mask.md) | F |
| TEST-POS | [34-pos-bigram-learning-coverage.md](34-pos-bigram-learning-coverage.md) | Q |
| TEST-SK | [37-structural-knn-learning-coverage.md](37-structural-knn-learning-coverage.md) | Q |
| TEST-ORDER | [25-dpo-sft-order-learning-coverage.md](25-dpo-sft-order-learning-coverage.md) | M |
| TEST-TCTX | [18-contextual-dictionary-learning-coverage.md](18-contextual-dictionary-learning-coverage.md) | Q |
| TEST-TMEM | [19-translation-memory-coverage.md](19-translation-memory-coverage.md) | Q |
| TEST-E22 | [27-predicate-dataset-router-learning-coverage.md](27-predicate-dataset-router-learning-coverage.md) | E |

## Прямые карточки и навигация

P05: основной receipt repair `cb3c9dc...` признан; полная учебная цель остаётся OPEN, остаточные входные/статусные случаи описаны отдельно. P10: записанный положительный градиентный контроль сохранён, уточнены снятие градиента и арифметика запаса допуска. P11: native scatter-add исправен; FD 9/9 сохранён, обозначены неполная терминальная приёмка, полный бюджет, потеря E2 и предел embedding-only вывода. P12: только step0-поправка, AdamW400/scheduler0, direct/factory различие и отсутствие готового S1.

DESIGNED в историческом паспорте не является следующим заданием. Индекс сохраняет RESULT_RECORDED и текущий порядок Q; исправление описания не переоткрывает выполненный прогон. Общие 03, 69, программы 34/36 и промпт продолжения закрепляют те же ограничения. Новая самостоятельная программа включает реальное обучение; исправление производного отчёта существующего обученного attempt делает 0 новых model calls и ссылается на его исходный учебный этап.

Ни одному из 62 ID этой таблицей не добавлен скрытый модельный бюджет. Старые файлы и результаты сохранены. У каждого владельца остаются его точные source, fixture, команды и лимиты; если для нового контроля понадобится модель, будущий автор обязан заранее пересчитать весь бюджет. Доказанный дефект одного пути не переносится автоматически на все группы.

## Проверка и границы

Работник `/root/p08_transfer_audit` выполнил независимое чтение P12 и первой редакции 88; нашёл ошибочное назначение fresh optimizer и неверное имя test project. План исправлен по его source evidence. Первичный packet — [аудит warmup](reviews/2026-10-28-warmup-first-update-source-audit.md). Это один независимый адресный обзор; не серия независимых проверок всех 62 реализаций. Итоговая статическая сверка записанных блоков и ссылок фиксируется после readback, без запуска product checks.

