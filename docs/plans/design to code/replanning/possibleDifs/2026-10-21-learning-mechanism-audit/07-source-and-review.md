# Источники, глубина проверки и границы результата

> **Финальный cutoff уточнён:** core `003083feab72157e54184bb66d0d940f48d836fd`, planning `8346dfb697be43041a477886db26c508453fe5ce`. Основной корпус ниже относится к d962/fc0dd. После него прочитаны 26 новых planning Markdown, 37 адресных core256-текстов и 5 документов позднего policy003; [08](08-latest-reconciliation.md), [latest-core review](reviews/latest-core-review.md) и [receipt](data/latest-core-readback.json), [wait003 review](reviews/latest-wait-reconciliation.md) и [5-file receipt](data/latest-wait-readback.json) отделяют новую проверку от исторической.

Дата: 21 октября 2026. Фаза — исследование и добавочные документы. Новых сборок, тестов, inference, обучения, десериализации моделей или правок production не было. Python применялся только для чтения текстов, сверки Git blobs, арифметики сохранённых JSON и формирования документов.

## Зафиксированные источники

| Источник | Срез | Что действительно прочитано |
|---|---|---|
| [xiigen-planning](https://github.com/lubochka/xiigen-planning/tree/fc0dd1b04d61a963906cdc1ab29dffd2ee9e520c/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs) | `fc0dd1b04d61a963906cdc1ab29dffd2ee9e520c` | 883 Markdown полностью получены и сверены по Git blob; весь набор участвовал в адресном поиске повторов. Смысловой разбор — затронутые механизмы и ближайшие совпадения, не новое полное ревью всех 883 файлов |
| [Core](https://github.com/lubochka/llm_mvp_core/tree/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19) | `d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19` | Current DPO/LoRA, Tensor/Transformer, tokenizer, optimizer/Trainer, continuous learning, translation memory и их callers; отдельные native POS/consumer/experiment источники. Материализация дерева сама по себе не означает чтение каждого файла |
| [Защищённые эксперименты](https://github.com/lubochka/llm_mvp_core/tree/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs) | тот же core | 335 текстов полностью получены и проверены по blob: 168 карточек, 13 отдельных промптов и управляющие/связанные документы. Поля и правила проверены по всем карточкам; source→raw→card подробно разобран для N63C и отдельных учебных/ресурсных контрастов. Дополнительно прочитаны существующие отчёты результатов |
| [Исходные RP01 результаты](https://github.com/lubochka/llm_mvp_core/tree/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/.kiro/xiigen-slices/work/results) | `83590a462b2f8aedc9d3ab4cb775193eb9c25ce5` | Полные scientific-result JSON C# main500 и EN→RU continuation; фактические updates, tensor changes, сохранение/открытие state и нулевые свободные ответы |
| [language_translate](https://github.com/lubochka/llm_mvp_core/tree/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9) | `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` | E25 runner и первичный JSON 1 мая: реальный Train128, одинаковая validation32, 11/32→28/32; не полный аудит ветки |
| [PrepareToGPL](https://github.com/lubochka/llm_mvp_core/tree/31346d8928be4469b01aaf313780e8f950ff91e2) | `31346d8928be4469b01aaf313780e8f950ff91e2` | Проверена текущая identity ветки; прежние результаты сопоставлялись через конкретные уже существующие разборы. Не заявляется новое полное чтение ветки |
| [DesignToCode-clean](https://github.com/lubochka/llm_mvp_core/tree/e346b88a3cc1fcce48aefa626bfc7b00509eefd6) | `e346b88a3cc1fcce48aefa626bfc7b00509eefd6` | Проверена identity ветки и использованы уже атрибутированные source-crosswalks; не новый полный аудит |
| [Родительский план kNN](https://github.com/lubochka/xiigen-planning/blob/fc0dd1b04d61a963906cdc1ab29dffd2ee9e520c/llm%20mvp/orchestrator%20implementation/PLAN-KNN-MATRIX-INTEGRATION-v3.md#L312-L323) | planning pin | Конкретное перенесение успеха обучаемого E25 на helpers проекта 29; сопоставлено с current fusion и обоими PatternClassifier |

Срезы фиксируют доказательства. Проверка branch head не означает, что все соседние ветки обследованы; более поздняя чужая правка не меняет автоматически смысл уже прочитанного source. Перед публикацией учитывается актуальный planning head, чужие файлы не перезаписываются.

## Порядок исследовательской работы и результаты

| Работа | Полученный результат | Предел |
|---|---|---|
| Сопоставление исходного RP01 и current учебных путей | [Карта 14 механизмов](01-learning-paths.md): DPO, LoRA, токены, восстановление, Units, kNN/AST/POS, replay и стоимость | Карта причинных возможностей, не установление единственной причины RP01 |
| Инвентарь и семантическая проверка конфликтов | [168-card inventory](data/card-inventory.md), [аудит](02-card-audit.md), [реестр 47 исходных и 3 дополнительных ID](04-registry.md) | 168 документов не равны 168 runnable опытам; отсутствие маркера риска не равно готовности |
| Исторический N63C source/raw/card на d962 | Обнаружены consumed-status, несовместимый filename, обнулённые late-error counters; [точные поправки](03-n63c-amendment.md) | В этом старом запуске эффект stale SFT не доказан: B отсутствует, A имеет 0/23 токенов |
| Поздний N63C-PROBE на core256 | [Completed raw и текущая постановка](08-latest-reconciliation.md): acquisition 0/23→22/23, после SFT A=20/23 и B=21/23; [конкретный framing](09-n63c-framing.md) | Полных ответов нет; EXAM нулевой; параметры текущего source отличаются от completed raw. Не объединять со старым N63C |
| Сохранённые положительные результаты | [Результаты и кандидаты](05-results-and-candidates.md), включая ухудшения и границы | Успех ресурса не превращён в освоение ответа; single A/B не даёт устойчивого speedup |
| Поиск новых конкретных факторов | [POS query reuse](cards/POS-QUERY-SET-ONCE-01.md) — новый; [SiLU](cards/SILU-DENOM-REUSE-01.md) объединён с чужим canonical; [дедуп](06-novelty.md) | POS NOT_RUN, host не материализован; SiLU alias без второго execution ID |
| Независимая проверка источников и документов | [Source/design review](reviews/independent-review.md), [review фактических карточек](reviews/final-card-review.md), [финальная сверка](reviews/final-reconciliation-review.md) | Один последовательно работающий отдельный агент `/root/audit_cards`, модель сессии по умолчанию. Это документные проверки, не независимые runtime-репликации |

## Что подтверждают receipts

- [protected-text-readback.json](data/protected-text-readback.json): 335/335 текстовых Git blobs получены точно; содержимое protected не менялось. Это подтверждение текста источников, не 335 смысловых проверок.
- [card-inventory.json](data/card-inventory.json): 168 документов, из них 125 P-семейств и 43 документа на 47 ID; отдельно 13 файлов prompts. Встроенные prompts не считаются ещё одним экспериментом.
- [n63c-primary-receipt.json](data/n63c-primary-receipt.json): native writer, bridge, host, claim, raw/report и их identities. Верхний update-counter сопоставлен с реальными stage receipts, а не принят буквально.
- [existing-cost-readback.json](data/existing-cost-readback.json): восемь полных raw A/B JSON повторно получены в исходных bytes через base64, Git blob совпал; дельты вычислены из полей сохранённых измерений. Это не новый запуск и не закрытие старых source/binary custody gaps.
- [novelty-search.json](data/novelty-search.json): фиксированный корпус 883+335, конкретные запросы, число совпадений, ссылки/строки и семантическое отличие ближайших кандидатов. Новые файлы этого пакета исключены из исходного набора. Ноль lexical hits не принят как доказательство мировой новизны.

## Ограничения данных, которые сохранены явно

Comparison JSON для N-KU-01, BPE-NO-MATCH-COPY-01 и NEURAL-FROZEN-TRANSPOSE-01 доступен через полученный Git срез как **Git LFS pointer**, а не содержимое измерения. Их полные Markdown reports прочитаны; соответствующие цифры атрибутированы этим отчётам. State-equivalence из них не выдаётся за новую проверку binary.

Weights/optimizer частичного H16 и часть N63C checkpoint-файлов также представлены pointers. Известные payload hash/length не означают, что payload получен и десериализован. Не делался запуск ради доступности файла. На исходном d962 код локального `N63C-PROBE` из цитаты пользователя не был получен. На позднем core256 он и completed raw появились и были прочитаны. Это не позволяет смешать completed results, partial results-02 и build-blocked results-03; их отдельная судьба описана в 08.

Числа margin/loss и 0→0 сохранены как факты, но не как доказательство приобретения ответа. Частичные 3/40→4/40 токенов остаются измеренным частичным прогрессом; цель полного ответа 0/2→0/2 открыта. Наличие такой истории не разрешает повторить заведомо недостаточный опыт под другим ID.

## Применённые skills и разрешение конфликтов инструкций

Использован [каталог skills core](https://github.com/lubochka/llm_mvp_core/tree/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/skills), который указала пользователь. Статус чтения — **TARGETED_READ**, не FULL_LOADED: применимые разделы и source-references прочитаны адресно; полный повтор огромных skill-файлов не заявляется.

| Skill | Для чего применён |
|---|---|
| `architect-behavior-classifier-SKILL.md` | Разделение learned mechanism, фиксированного правила, trained-state consumer и фактического владельца обучения |
| `phase-planning-SKILL.md` | Фаза документации; конечный scope; сохранение открытых вопросов без скрытой реализации |
| `plan-review-SKILL.md` | Приоритеты, один фактор, before/after, достаточность минимального набора, конфликты NEXT и статусов |
| `code-examination-SKILL.md` | Прослеживание caller→state→consumer и точных текущих методов, сопоставление с историческими ветками |
| `experiment-design-SKILL.md` | Диагностичность, домен, фиксированные данные, budget, отдельные полезные оси и отрицательные результаты |
| `prompt-patch-authoring-SKILL.md` | Самодостаточные инструкции: default model, host, entry, source, conflicts, wait, report, STOP/NEXT |
| `human-readable-architecture-audit-SKILL.md` | Вывод, доказательство, действие и ограничение рядом |
| `human-communication-GLOSSARY.md` | Ясное описание наблюдений, гипотез и отсутствующих измерений по-русски |

Локальные AGENTS/CLAUDE использованы для phase/scope/ownership. Старые назначения другой модели, обязательных дополнительных прогонов, полного product CI либо переписывания архива не отменяют текущие прямые правила пользователя. Модель исполнителя и проверки — текущая по умолчанию. При NEXT/STOP прекращение прошлого действия выше старого требования любой ценой дописать report/Git. User-facing итог и Git-публикация относятся только к запрошенным документам planning.

## Что ещё не закрыто

Требование «все детали уже правильно оформлены в защищённых экспериментах» не выполнено исходным каталогом: на d962 N63C и квалификация before содержали конкретные противоречия; на 256 часть guards исправлена, но бюджеты/полный before и source-result lineage остаются открыты, новый POS-рецепт там отсутствует, а SiLU уже опубликован в соседнем planning-пакете и объединён без дубликата. Эта папка пользователем защищена от изменения. Поэтому итог этого задания — опубликованные доказательства, реестр и точные добавочные поправки в `possibleDifs`; не фиктивная отметка об исправлении protected карточек или о внедрении в продукт.

Дальнейшая runtime-работа должна начинаться с пригодного учебного состояния и согласованного малого рецепта. H512 probe теперь имеет partial22/23 before, но не полный ответ; H64 sanity имеет 1/2 и рецепт, но не сохранённый reusable checkpoint. Материализация hosts и новые A/B в эту документную фазу не входят.
