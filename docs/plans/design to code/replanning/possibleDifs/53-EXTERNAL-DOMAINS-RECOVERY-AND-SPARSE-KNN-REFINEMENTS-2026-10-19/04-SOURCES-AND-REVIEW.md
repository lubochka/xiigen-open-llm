# Источники, метод проверки и предел результата

19 октября 2026. Этот пакет отвечает на вопрос, какие различия стоит проверить для восстановления работавшего обучения и какие дополнительные механизмы ещё не выделены в прежних исследованиях. Он содержит исследование, воспроизводимые математические контрпримеры и проекты маленьких native/предметных опытов. Он не сообщает о выполненном ремонте или измеренном повышении качества RP01.

## 1. Неизменяемые срезы

| Назначение | Репозиторий/ветка | Commit | Фактическая глубина использования |
|---|---|---|---|
| Нынешний код | `llm_mvp_core / codex/dpo-implementation` | `44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76` | DPO, LoRA, AdamW, exact/continuation, KNN, classifiers, ordinary consumers; защищённый каталог только для чтения |
| Приоритетный успешный донор | `llm_mvp_core / external_domains` | `3036a01acec938f0b83fa4ffbda26202cc35d639` | Первичный код названных механизмов, 23 сохранённые JSON-квитанции, targeted file parity |
| Соседняя ветка перевода | `llm_mvp_core / language_translate` | `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` | Адресное сравнение DpoTrainer, LoRA и AdamW; не повтор всей экспериментальной программы |
| Соседняя ветка code/design | `llm_mvp_core / DesignToCode` | `8f20ceb8a9bfbadab2ad309226c47dd76e714548` | Адресное сравнение DpoTrainer, LoRA и AdamW; наличие ветки не объявлено успешным native rerun |
| RAG/первый PR и прежние planning-гипотезы | `xiigen-planning / llm_plan3` | `a90350f33955f9c4b15707a4619eeeba5d42e863` | RAG-guide, первичные родительские требования, RP01 source/diagnostic, 389 Markdown прежнего possibleDifs |
| Первая сверенная дельта planning | `xiigen-planning / llm_plan3` | `0715844069a0c9b0242958c663464f552614ad84` | 9 изменённых файлов, из них 8 Markdown-версий; свежие материалы52 |
| Заключительная сверенная дельта planning | `xiigen-planning / llm_plan3` | `02532653562a5f1e27fd75b1b03d4ae23e930e2f` | Ещё 9 изменённых путей, 7 Markdown-версий; 13 head/base blobs проверены; прямых дублей нет |

Ссылки в доказательных утверждениях ведут на полный commit, а не только на движущуюся ветку. Перед записью целевой ветки её состояние проверяется заново; сопутствующая публикационная дельта учитывается отдельно от первоначального источникового среза. Core HEAD при заключительной проверке оставался `44507823`.

## 2. Что именно является доказательством

| Уровень | Что здесь выполнено | Какой вывод допустим |
|---|---|---|
| Первичный исходный код | Прочитаны условие, вычисление, state owner и ближайший caller | Локальный механизм существует; наличие класса не означает вызов в RP01 |
| Сохранённый исторический результат | Повторно получены 23 raw JSON из `external_domains` | Указанный архив сообщает конкретный результат на своей панели; это не новый запуск |
| Механическая проба | Выполнен Python3 standard-library script для трёх принятых условий, включая отрицательные контроли | Формула/ветвление способны различить заданные входы; native MathF/Roslyn и качество не измерены |
| Точная транспортная копия | Сверены Git blob SHA для полного Markdown-корпуса и 15 KNN-файлов | Сохранены именно байты указанного Git объекта; истинность всех утверждений внутри из этого не следует |
| Дедупликация | Полнотекстовые запросы + чтение ближайших owner/trigger/failure совпадений | Новизна ограничена зафиксированным корпусом; будущие commits и неизвестные документы не покрыты |
| Native компонент, новое обучение, внешний Teacher | **Не выполнялись** | Положительное native/quality заключение отсутствует |

[Исторические E07/E09/E14/E25/E26/E43](evidence/external-success-receipts.json) и [E40-v1/E40V2](evidence/e40-versioned-receipts.json) сохраняют исходные метрики, notes, дату, path и commit. Слово success в имени первого файла обозначает программу исследования: в нём намеренно оставлены также отрицательные и нейтральные receipts. Они нужны, чтобы не отбирать только удобные результаты.

Итоговый корпус дедупликации — **676 версий / 671 различный Markdown-путь**. Первая сводка поиска и заключительная дельта сохранены раздельно: [основные запросы](evidence/dedup-search-receipt.json), [запросы дельты025](evidence/dedup-delta2-search.json), [проверка и пределы дельты](evidence/dedup-delta2-review.json).

## 3. Воспроизведение выполненной части

Из каталога пакета:

```bash
python evidence/mechanical-probes.py > mechanical-results-rerun.json
```

Скрипт использует стандартную библиотеку, выводит JSON и сам не меняет файлы. Shell перенаправление создаёт только указанный новый результат. Сеть, модели, DPO-датасет, .NET и дополнительные Python-пакеты не нужны. Зафиксированный результат: [mechanical-results.json](evidence/mechanical-results.json).

| Контроль | Наблюдение |
|---|---|
| NEW-AGE: первый carrier после 49 чужих updates | Первый scalar decrement `4.3188970266783144e−6` против `9.999999747378752e−6`; отношение `0.43188971281051636` |
| NEW-AGE: оба carrier активны все 50 шагов, включая 10 явных нулей у второго | Максимальная разница weight/m/v между двумя формулами возраста равна 0; значения конечны |
| NEW-SCORE: конечные mixed-sign cosine scores | Confidence `59.99987479097672`; изменение слабейшего score через нулевую сумму переключает fallback при прежних IDs и рангах |
| NEW-SCORE: доминирующие и равномерные положительные scores | Current и диагностический contract дают одинаковый выбор в соответствующих отрицательных контролях |
| NEW-POSITION: диагностические позиции 255/256 | Current выбирает `[10,10]`; снятие только position cap — `[10,20]`; контроль 254/255 совпадает |

Это не «падение качества AdamW на 57%», не «KNN стал вдвое точнее» и не performance benchmark. В первом случае измерен scalar update, в третьем — искусственные метки. FP32 casts повторяют порядок существенных операций; различия native `MathF.Pow/Sqrt` и реализаций runtime не проверялись.

[15 KNN source objects](evidence/knn-source-integrity.json) были сверены с Git blob SHA до первоначальной пробы. Пять пар файлов (`KnnSkillFilter`, `AstRelevanceRanker`, `ContextWindowManager`, `StructuralKnn`, `SemanticCompilerFacade`) равны по телу после пяти строк нового лицензионного заголовка. Этот факт ограничивает утверждение о регрессии: возврат этих файлов к `external_domains` не изменит исследуемую формулу. Полные source-копии здесь не дублируются; ссылки и хэши позволяют получить те же объекты из репозитория.

## 4. Современные исправления, которые сохраняются

Исторический `DpoTrainer.BackwardLogProbs` заполнял `logits.Grad`, но не выполнял обратный проход по модели. Текущий путь вызывает `logits.Backward` с `(oneHot-softmax)` и сохраняет graph policy-forward. Exact objective использует response log-prob sums и устойчивый softplus. Поэтому перенос положительных результатов дискретной памяти из старой ветки не означает откат нынешнего neural core. [Исторический DPO](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/11.DPO/DpoTrainer.cs#L219-L263); [текущий backward и exact loss](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/11.DPO/DpoTrainer.cs#L1072-L1150).

Нынешний `LoraLinear.Forward` применяет attached A/B, attention использует реальные adapters. Эти части нельзя объявлять «неподключённой LoRA» по старому экспериментальному файлу или устаревшему dossier. [LoRA](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/10.LoRA/LoraLinear.cs#L94-L125); [attention](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/03.TransformerModel/Layers/CausalSelfAttention.cs#L124-L158).

В профиле RP01 EN→RU сохранена проверка полного optimizer map из 16 tensors/53 248 scalars. NEW-AGE не доказывает отсутствие этих entries в выполненном RP01. Zero gradient carrier, возникающий при нулевом LoRA B, не равен отсутствующему carrier. [Guard состояния](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/14.ContinuousLearning/Training/SimulationEnRuContinuation.cs#L250-L266).

Адресная сверка соседних веток: `external_domains` и `language_translate` имеют одинаковый blob `DpoTrainer.cs` — `8990add820d077a0ee841e529f514c0e1db4f356`; во всех трёх исторических ветках совпадают `LoraLinear.cs` — `71e7a3a3388262983bd13b99db1b4d5c2471f7da` и `AdamW.cs` — `0c63291ee99accf6403f0d471b03017b7c37d2c0`. Это сравнение указанных файлов, не всех возможностей веток. [External LoRA](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/10.LoRA/LoraLinear.cs); [Language LoRA](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/10.LoRA/LoraLinear.cs); [DesignToCode LoRA](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/10.LoRA/LoraLinear.cs).

## 5. Как применены запрошенные навыки

Навыки взяты из [указанного пользователем `docs/skills`](https://github.com/lubochka/llm_mvp_core/tree/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/skills). Для больших файлов прочитаны относящиеся к задаче разделы; чтение каждой строки всей библиотеки навыков не заявляется. Human-communication glossary прочитан полностью.

| Навык | Конкретный результат его применения |
|---|---|
| `architect-behavior-classifier`, `trainable-architecture-decision` | Для каждой находки указаны изменяемое состояние, владелец и настоящий consumer; внутренняя математика AdamW не превращена в лишний semantic Unit |
| `phase-planning` | Требование пользователя связано с первичным кодом, проверяемым условием, маленьким вмешательством и явным результатом |
| `plan-review` | Разделены составные historical comparisons и однофакторные A/B, обозначены данные/границы/caller и условия остановки |
| `generated-code-review` | Проверены реальные guards, отрицательные контроли, null против zero carrier, позиционная граница, scores и fallback |
| `prompt-patch-authoring`, `planning-dpo-authoring` | В [плане опыта](02-SMALL-DOMAIN-EXPERIMENTS.md) есть конкретное задание исполнителю, происхождение DPO, отдельные pair-judge и Student evaluator |
| `human-communication-GLOSSARY`, `human-readable-architecture-audit` | Домены описаны предметно; EN→RU и parser-профиль не переименованы в отдельные универсальные домены; факт выполнения отделён от гипотезы |

Первичные RAG-основания: [legal-build guide](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-RAG-01-dpo-legal-build.md), [R16 role/lesson contract](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-02-teaching-chinese-model.md), [родительское Unit/Flow/Orchestrator требование](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/dpo%20legal%20build/active-codex-execution-original-task.md#L1-L16), [RP01 source/architecture](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/SOURCE-AND-ARCHITECTURE.md).

## 6. Существенные замечания независимого review и внесённые исправления

| Риск раннего проекта | Исправление в итоговом плане |
|---|---|
| R0 меняет владельца и предполагает совместимость state | Сначала trace/compatibility; A/B адресации лишь после доказанной общей схемы |
| R1 подменяет rules-класс обучаемым с другим encoder/labels | Чистый rules/memory опыт на одинаковых POS; отдельный adapter/integration этап |
| После загрузки `Train([new])` может стереть память | Новая версия old+new; replacement semantics указана явно |
| E07/E09 разные цели, панели и признаки | Archive replay отделён от нового общего harness; для места применения сигнала нужен общий feature schema |
| E40V2 сравнивался не с нынешней weighted sum | Сохранён исходный baseline, нынешняя сумма вынесена в отдельный контраст |
| E43 count rebuild описан как continuation общего parent | Count rebuild и neural continuation разделены |
| Equal-budget replay уменьшает новые предъявления; R16 ограничивает повтор пары | Назван бюджетный компромисс, exact replay и новые пары на старые навыки различены |
| Аффинный cosine shift выглядит готовой починкой | Он обозначен иллюстрацией; основной B проверяет допустимость score-массы и сохраняет положительные контроли |
| Совпадение тела кода/неактивный default скрывают применимость | Inherited status, PassThrough и opt-in StructuralKnn вынесены в условия до native/quality |

Проверка ссылок, JSON и воспроизводимой пробы выполняется перед публикацией; её машинная квитанция — [package-checks.json](evidence/package-checks.json). Она удостоверяет согласованность deliverable, а не correctness нейронного learner. Все запланированные native/предметные опыты сохраняют статус `NOT_RUN` до собственного фактического выполнения.
