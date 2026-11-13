# Требования пользователя, источники и граница аудита

> **Граница источников:** основной текст ниже относится к core `fb9175cc`. Перед публикацией отдельно проверен новый commit `d962f958`: [позднее уточнение](10-late-core-delta-d962f958.md), [актуальный реестр](04-registry.md). Исторические номера/статусы и счётчики 163 cards / 8 prompts не являются повторной аттестацией нового среза. NEXT сейчас — №10 PARENT-ADOPTION-01; старые raw и попытки сохраняются.

## Приоритет поручения и использованные skills

Результат — добавочная документация в `xiigen-planning/llm_plan3/llm mvp/design to code session/replanning/possibleDifs/`. Пользователь запретил изменение protected core folder и указал нынешнюю фазу: документировать результаты/кандидаты. Работа не запускает новые эксперименты, не чинит production/tests, не публикует модели. GitHub сохранение этих документов авторизовано самим поручением; отдельное подтверждение доступа не требуется.

Использованы указанные пользователем project skills на core `fb9175cc752d41e1cb525e0609b7c96140940217`:

- [Архитектура](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/skills/architect-behavior-classifier-SKILL.md): настоящий learner/writer/state/consumer, реализованные и декларируемые механизмы.
- [Планирование](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/skills/phase-planning-SKILL.md) и [Проверка планирования](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/skills/plan-review-SKILL.md): владение гипотезой, конкретный finite recipe, зависимости, незаполненные поля и ревью.
- [Проверка кода](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/skills/generated-code-review-SKILL.md): адресное чтение source, одна experimental mutation, точные факты и пределы доказательства.
- [Промпт-инжиниринг](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/skills/prompt-patch-authoring-SKILL.md): самостоятельные рабочие указания, пути/команды/модель/бюджет/статусы без выбора за исполнителем.
- [Понятный архитектурный отчёт](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/skills/human-readable-architecture-audit-SKILL.md) и [Человеческая коммуникация](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/skills/human-communication-GLOSSARY.md): русский язык, связь факта с последствием и конкретным исправлением.
- [Дизайн эксперимента](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/skills/experiment-design-SKILL.md): контролируемое вмешательство и действительные метрики реального обучения.

Полностью прочитаны эти skills распределённо по потокам проверки; по большому AGENTS читались применимые разделы, полный построчный аудит AGENTS не заявлен. Старые skill instructions про mandatory масштабирование, accuracy thresholds, model override или полную production ceremony уступают текущему прямому поручению. Модель исполнителя — default/inherited текущей сессии; никакого нового override. CODEX/Astra selection/judging и external Chinese pair generator — разные роли, применимые к настоящему DPO teaching, а не обязательные calls каждого resource/SFT/KNN опыта.

## Соответствие правилам пользователя

| Пункт | Требование | Как отражено |
|---|---|---|
| 1 | Малый проверяемый эксперимент по доменам | Две новые карточки назначают один factor, native learner, буквальные данные и конечный budget. AUTHOR_SETUP_REQUIRED не скрывается. |
| 2 | Любой useful metric gain — успех; регрессии рядом | 02 и оба prompts; локальные блоки для №08/09 в 06. Accuracy не обязательна. |
| 3 | Не подменять обучение посторонней проверкой | Новые факторы измеряются внутри actual native SFT/Unit.Train; микробенчмарк Exp/cosine не заменяет Train. |
| 4 | Ingress/restore/output допустимы при зависимости результата обучения | 01/02/03/07/08 указывают writer→state→reader и существующий owner. P25 сохраняет выученный P125 state; E26 static-only не назначен новым learning опытом. |
| 5 и 9 | Обучение, затем конфликты и обновления, зависимые consumers в конце | 04 и resolved-dispatch: текущий№10, core10–40, prior/new proposals, consumers41/42. No global production-parent gate. |
| 6 | Изолировать эксперимент; до доказательства не чинить весь продукт/tests | Новых executions0, production changes0; карточки только experimental copies, без adoption/test/CI chain. |
| 7 | Не заставлять искать место, модель, команды, решение конфликта | Каждая новая карточка назначает short workspace, source pin/closure, host filenames, author task, expected launch contract, data, learner config, statuses. Несуществующая CLI не объявляется рабочей. |
| 8 | Не удалять старые эксперименты | Публикация только новых путей; первые 9 consumed; сохранены A1–A3; old raw и reports остаются. |
| 10 | Экспериментальные исключения допустимы | Минимальные host/DTO/explicit exceptions допускаются в isolated research; production rules не навязываются как отдельная цепочка без необходимости. |
| 11 | Только документирование; успешное добавить кандидатом | 02 сохраняет все адресно проверенные useful deltas с trade-offs. New SiLU/HNSW не называются успешными без измерения. |
| 12 | Язык, skills и report format заранее ясны | Русский Markdown; exact project skills/paths и report fields в обоих новых cards/prompts. |
| 13 | Абсолютный STOP/NEXT в каждой карточке | 171/171 existing files имеют explicit markers; новые cards и встроенные prompts содержат полный запрет report/hash/Git/retry после NEXT/STOP. |
| 14 | Порядок виден из registry | 04 и machine JSON с 38 next transitions,9 consumed,5 proposals и single-ID END. |
| 15 | Реальный Train, минимальный set, ненулевой сигнал и честный budget | SiLU400+4+4, HNSW4→8 уроков; нет выдуманного checkpoint. 0→0 не подтверждает освоение; partial NLL/cost success сохраняется отдельно. Нет unbounded loop-until-PASS. |

## Источники и метод

| Слой | Срез / объём | Что устанавливает |
|---|---|---|
| Current core | `fb9175cc752d41e1cb525e0609b7c96140940217`; protected subtree `4b26620eb0ee4b69e244dc5cee7f55fa4c3f9af4` |372 Markdown, текущие 42ID,9 consumed,33 future |
| Current planning | `fc0dd1b04d61a963906cdc1ab29dffd2ee9e520c`; possibleDifs subtree `9845f10c0623f61507527d9936f017cdc2e78a4b` |929 Markdown+23 таблицы, существующие владельцы и отсутствие найденного дубликата новых факторов |
| Native code | Названные Tensor/Transformer/Trainer/AdamW/LoRA/DPO/BPE/Unit/HNSW/translation spans на core SHA | Actual call chains и возможность узкой experimental copy; не runtime PASS |
| Historical sibling | `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` | Сохранённые положительные E09/E25/E43 и static E26, исходные runner; shared results tree не независимая репликация |
| Saved JSON | Raw paths и fields в `saved-metric-reconciliation.json` | Арифметическая сверка уже измеренного; без новой генерации/обучения |
| RP01 и родительские RAG | Sources/architecture/current-code/examination/recovery, teacher roles, Arbiter amendment | Архитектурное намерение и ссылки на родительские области; не доказательство реализации всех контрактов |

Полный скачанный corpus имеет 1324/1324 побайтовых совпадения с Git blob SHA. [Corpus manifest](data/corpus-manifest.tsv) относится к документам. Дополнительные native sources/raw/history имеют собственные ссылки и SHA в 07/08, cards и [source snapshot](data/source-snapshot.json). Не добавлять rawresults к 1324 для искусственного увеличения заявленного документного покрытия.

Machine inventory163 cards+8 prompts показывает role/queue mapping, marker hits и semantic scope per row. Default/STOP markers171/171, useful-axis explicit rule169/171. Маркерная проверка не исполняет смысл текста: наличие `Train` не доказывает learning path, команда в документе не доказывает существование host. 125Pfamilies и 470 старых subcases не превращены этой инвентаризацией в 470certified experiments.

## Воспроизводимость проверки без новых токенов обучения

Для повторного **документного** чтения достаточно pinned GitHub links, манифеста, таблицы 171 строки и численных полей в saved-metric-reconciliation. Все проценты в 02 вычислены как `(B−A)/A×100`; процентный пункт точности использует общий явно указанный знаменатель. Response target counts не заменяются полной длиной prompt+answer. В N63R `TrainedTokens` содержит входы, поэтому next-token target count рассчитан как inputtokens−updates по указанному однострочному native recipe; A/B/C имеют соответственно 62800/117600/94200 targets. Никаких дополнительных learner calls для этой арифметики не нужно.

До/после означает одну и ту же frozen panel при разных состояниях; A/B означает независимые arms с одинаковым стартом и одним фактором. Loss на двух разных последовательных уроках сам не является before/after показателем одной строки. Teacher-forced exact, single-token completion и свободная многотокенная генерация обозначаются различно. Идентичность saved panels после restore проверяет сохранённые данные, не новый live LoadCheckpoint.

## Что остаётся незакрытым

Нет доказанной единой причины снижения качества всего RP01. Нет независимой семантической аттестации всех 470 подслучаев или всех 131k+строк code. Нет measured результатов двух новых факторов; hosts не запускались. Нет доказанной causal speedup по одному fixed-order времени. Нет доказательства, что structural/POS успех одинаково переносится во все домены. Нет оснований считать private raw restore полноценной production adoption или считать отсутствие adoption запретом исследования.

Эти ограничения не обнуляют уже измеренные полезные результаты и не создают автоматическую программу новых запусков. Все изменения результата этого поручения доступны как новые кандидатные документы. Проверка перед публикацией и точный охват исправлений фиксируются отдельно в [09](09-publication-review.md).
