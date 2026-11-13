# Доказательства восстановления external_domains и проверка новизны

19.10.2026. Подготовлено для родительского исследования. Удалённые репозитории в этой подзадаче только читались. Этот файл — рабочая часть исследования, не заявление об исправленной или обученной модели.

## 1. Что проверено

Зафиксированы `xiigen-planning@a90350f33955f9c4b15707a4619eeeba5d42e863`, `llm_mvp_core@44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76` и donor `external_domains@3036a01acec938f0b83fa4ffbda26202cc35d639`.

| Корпус | Корневые Markdown | Вложенные Markdown | Получено и охвачено поиском |
|---|---:|---:|---:|
| planning `replanning/possibleDifs` | 312 | 77 | 389 |
| core `docs/plans/replanning-possible-diffs` | 92 | 180 | 272 |
| Итого уникальных удалённых Markdown | 404 | 257 | **661** |

Все 661 файла получены через GitHub по immutable refs, без ошибок загрузки. Полные тексты охвачены поиском; не утверждается, что все 661 документа прочитаны вручную от начала до конца. В локальном core дополнительно присутствует alias-копия `README.md`, не входящая в число уникальных источников. Полный список путей, blob SHA и размеров — `CORPUS-MANIFEST.json`; сохранённые результаты адресного поиска — `NOVELTY-SEARCH.json`.

Кроме этого прочитаны исходники и четыре raw-result E09/E25/E26/E43, два итоговых научных RP01 JSON, актуальные предметные разделы RP01 pack, RAG-индексов и conformance, родительских Dynamic Flow и KNN integration документов. Два проектных skill прочитаны по применимым разделам; словарь человеческой коммуникации — полностью. Все локальные файлы лежат внутри `audit_inventory`.

## 2. Реальная положительная история donor

Общий префикс исходников далее: `https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/`.

| Источник и точные строки | Положительное наблюдение | Что именно менялось и что переносить | Ограничение |
|---|---|---|---|
| `35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json`, 5–17 | 0.3438 → 0.875 на 32 validation; 160 examples | `E25PatternClassifier.cs:17–35`: Train на 128 POS-последовательностях, k=3, сравнение правил и соседей на тех же32. `GlobalUsings.cs:6` привязывает SentencePatternClassifier к **SemanticCompiler.PatternClassifier проекта12**. В `12.SemanticCompiler/Translation/PatternClassifier.cs:29–49,72–81` память POS-последовательностей, Jaccard биграмм, голосование по label. | Синтетические POS, небольшой набор шаблонов. Это реальное обучение дискретной памяти, но не проверка свободного перевода предложений и не обучение LoRA. |
| `35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-001.json`, 5–17 | 0.4 → 1.0 на 15 cases | `E26ContextualDictionary.cs:15–42`: один словарь и одинаковые cases, сравниваются priority-only и left/right-context. `Infrastructure/ContextualDictionary.cs:61–108`: context-distance и priority tie-break. Переносить полезное различение значения по соседнему контексту. | `BuildDefault:24–58` заранее заполняет entries, cases65–93 заданы явно. Нет Train, обучения новых слов из DPO или сохранения новой памяти в этом runner. Не приписывать15/15 обобщению. |
| `35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json`, 5–17 | 0.2413 → 0.3507, 5617 tokens, 4492 train pairs | `E09StructuralKnn.cs:25–62`: structural nearest-neighbour memory против bigram. Релевантны C# AST, вложенность, контекст узла. | `45–53` baseline оценивает полный val, `64–72` KNN только первые500. Из raw count и source следуют **1123** baseline transitions против **499** KNN; это не matched gain. В `97–115` кодировка kinds локальна каждому файлу. Нужны один codec, один split и один denominator. |
| `35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json`, 5–17 | 0.3226 → 0.3978 на старых переходах;177 replay | `E43ReplayBuffer.cs:18–50`: old/new source halves, old train/val, сравнение fresh bigram из new и fresh bigram из new+old replay. Это положительная проверка сохранения старых соответствий через смешанный корпус. | Это отдельные count models, не continued update одного LoRA state. `ExtractPairs:73–81` объединяет переходы между файлами. Не переносить30% как универсальную константу, не объявлять checkpoint restore проверенным этим опытом. |

**Проверено дополнительно сейчас:** полные current E25/E26/E43 при SHA44507823 совпадают с donor по тексту после исключения пяти строк copyright. Код этих runner не потерян. Различие ordinary consumer, corpus, representation и состояния всё равно требует собственного trace. E09/StructuralKnn точное сопоставление выполняет соседняя подзадача; её доказательства не дублируются как самостоятельные мои замеры.

Все числа в таблице — архивные измерения. В этой подзадаче новых запусков E09/E25/E26/E43 и новых обучающих обновлений **0**.

## 3. Короткая очередь восстановления для первой главы

1. **Перевод: вернуть получателю E25 обученное распознавание порядка.** Сначала на одних POS и labels сравнить rule fallback и trained neighbour store. Затем отдельно включить этот candidate в настоящий переводческий consumer. Разделять pattern accuracy и правильность предложения; cold-load проверяет ту же память. Негативный контроль: пустая/переставленная память. Положительный: сохранённый160/32-профиль с подтверждённым codec.
2. **Перевод: вернуть E26 контекстное различение значения.** Сравнить одинаковые entries и queries с/без left/right context; затем заменить oracle POS настоящими извлечёнными признаками. Новые слова должны поступать через учебную запись с явным сохранением; само BuildDefault не является continued learning. Отрицательный контроль — неизвестное/несовместимое окружение; опровержение — соседний контекст не улучшает обычный ответ.
3. **Документирование/программирование C#: вернуть structural KNN до ответа.** Одна и та же frozen candidate pool, label schema и новые семейства файлов; меняется только structural signal. Не повторять старое несоответствие1123/499. Результат — правильное пользовательское действие или объяснение; next-kind top1 дополнителен.
4. **Продолжение обучения: проверить old/new retention на конкретном получателе.** Отдельные micro-опыты для bigram, memory Unit и LoRA. При фиксированном общем бюджете сравнить новый материал с разрешённым ограниченным повтором старого, отдельно old accuracy и new acquisition. Это проект исследовательского опыта; он не изменяет действующую политику повторного применения DPO.

Это известные восстановительные механизмы, а не новые открытия. Новые узкие preprocessing/completion кандидаты следует помещать после них и явно отмечать как унаследованные риски до доказанного current caller.

## 4. Что говорят первичные документы о потере смысла при переносе

Planning SHA для всех строк этого раздела: `a90350f33955f9c4b15707a4619eeeba5d42e863`.

- `llm mvp/orchestrator implementation/PLAN-KNN-MATRIX-INTEGRATION-v3.md:312–328` связывает E25 с helper-классификацией и фиксированной смесью `0.45/0.35/0.20`; строка489 прямо называет коэффициенты первоначальными, ожидающими калибровки. Это не описание trained POS-neighbour store из E25. Название эксперимента и процент не доказывают тождество механизма.
- Соседний `experiment_knn_state.json:169–191` назначает E25/E26 классы проекта29 и берёт `result_source` из **комментариев**; точный E25 runner через alias использует проект12. Поэтому запись `wired_pattern_fusion` — повод проверить путь, не доказательство восстановления экспериментального алгоритма.
- `llm mvp/llm dynamic flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md:10–14` требует ограничить pool предметной темой до ранжирования; `24–25` разделяет retrieval signals до генерации и acceptance после неё; `53–61,77–96` требует сохранять положительные, отрицательные и предупреждающие соседи с разными правами использования. Это источник контракта, не результат запуска.
- `llm mvp/design to code session/09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md:13–24` разделяет Chinese author, CODEX selector, CODEX exact-pair judge и Student evaluator; `34–45` отделяет восстановимое состояние и причинное улучшение. Роли соответствуют текущему указанию пользователя: CODEX/Astra6 UltraCode выполняет выбор и отдельную оценку качества, внешняя китайская открытая модель создаёт обе DPO-стороны.

Возможный предметный вывод: при переносе надо восстановить **сигнал и его обучаемого получателя**, а не только воспроизвести название E25/E26 или прежний процент. Этот вывод опирается на точное различие исходных алгоритмов и документа интеграции; причинный эффект ещё требует эксперимента.

## 5. Почему нынешний RP01 нельзя оценивать числом изменённых тензоров

В current SHA44507823 прочитаны непосредственно два итоговых JSON:

- `.kiro/xiigen-slices/work/results/RP01-simulation-main500-execution-20261015/scientific-result.json:8–16` —500 пар,50 updates,39 base tensors unchanged,16 adapter tensors changed, сохранение/restore; `36–53` — **0/3→0/3** свободных полезных ответов; `69–74` — research-only, не принято в production.
- `.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json:11–21` —500 пар,50 новых updates,16 LoRA finite changed, состояние reopened; `33–55` — **0/4→0/4**; `74–83` — сохранение тех же неверных C# ответов не удостоверяет удержание изначально полезного навыка.

Это не опровергает наличие успешного обучения в `external_domains`: там измерялись другие получатели и другие задачи. Для причинной parity сначала указать common input, common consumer, изменяемое состояние и правильный ответ.

## 6. Проверка новизны: что принято и что исключено

Результат полного поиска661 Markdown сохранён в `NOVELTY-SEARCH.json`. Отсутствие имени не доказывает отсутствия абстрактной идеи, поэтому совпадающие по смыслу разделы проверялись дополнительно.

| Проверяемое | Результат | Решение для родителя |
|---|---|---|
| RegexPreprocessor / XmlDocs / LineComments / StringLiterals / UseRegexPreprocessor |0 совпадений | Конкретный raw TRAIN→stripped inference, comment marker inside string кандидат не найден в исследованном корпусе. Сам дефект и caller проверяет knn_domains. |
| ExtractMethodCalls / ExtractLeafIdentifier / MemberBindingExpression / conditional access |0 совпадений | Конкретные receiver-path collapse и пропуск условного вызова не найдены. |
| MethodFrequencyTable |3 строки в1 документе, LT-N03 | Уже описаны повторные Compile→избыточный count. Новый symbol extraction/identity механизм не следует выдавать за повторное открытие общей frequency memory. |
| covariance / ковариа / ParseGraph / process.state |0 совпадений | По этим точным термам повтор не найден; это только novelty lead, не верификация самой гипотезы. |
| raw Executor tokenizer vs corrected SemanticTokenizerUnit |22 строки в21 документе | **Известно**: `28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT...`, P88; не новая находка. |
| RememberCompiledSource / EnsureGrammarMaskFilter |10 строк в5 документах | **Известно**: LT-N03; не новая self-training finding. |
| E13 ExpressionBody collapse |18 строк в12 документах | **Известно**: P83 / planning38. |
| E09 SiblingCount omission |49 строк в24 документах | **Известно**: planning35/core73/P26. |

Дополнительно исключены совпадающие E14 guard/scope/receiver гипотезы N48-01 и feature-label alias LT-N01, LoRA A-factor scaling из language-restore/04a и AdamW finite-gradient→Inf-v из N48-02. Их общие имена недостаточно было искать по README; точные свежие тела объяснили пересечения.

## 7. Метод и границы

Применённые skill-требования: `phase-planning:51–70,737–750` — свежесть текущего задания, scope, конкретный артефакт; `plan-review:1580–1623,2115–2148,2290–2313,3603–3629` — материал, запрет выдачи lookup за generalization, ограниченный audit scope, предметная метрика. Это исследование с документацией и проектами опытов; прежние30/50 readiness циклы и governance edits не активировались.

Точное ограничение: весь Markdown обоих указанных каталогов получен и поисково проверен на названные механизмы; весь код репозитория и все non-Markdown evidence файлы не прочитаны. Время snapshot фиксировано ref, более поздние параллельные коммиты требуют финальной проверки родителя. Ни число документов, ни количество поисков не равно числу независимых подтверждений. Самостоятельных native learning runs здесь0; фактический CODEX/Astra независимый evaluator и Chinese author здесь не запускались.

Более широкий аудит ещё может найти другие callers тех же классов, случаи в скрытых бинарных checkpoints, нефункциональные конфигурации, данные из других веток и ошибки под другими именами. Поэтому корректный итог — новые **конкретные** кандидаты относительно полностью проверенного Markdown-среза, а не утверждение, что найдены все причины падения качества.

## 8. Проверка параллельных публикаций до 82d146cd

Перед публикацией родитель указал новую planning вершину `82d146cd7f1a628dbb95bc1989c341ae2e8e6e1d`; core остаётся на44507823. Выполнен API compare `a90350f33955f9c4b15707a4619eeeba5d42e863...82d146cd7f1a628dbb95bc1989c341ae2e8e6e1d`: **17 commits,15 изменённых путей**, все внутри разрешённого planning `possibleDifs`. Из них10 новых Markdown,2 изменённых Markdown,2 Python и1 JSON. Все15 новых версий получены и охвачены поиском; substantive N52 finding sections прочитаны отдельно. Список с SHA и строками изменений — `LATEST-DELTA-MANIFEST.json`, фактический поиск — `LATEST-DELTA-SEARCH.json`.

**RX1/RX2:** в новой дельте не найдены `RegexPreprocessor`, `UseRegexPreprocessor`, `XmlDocs`, `LineComments`, `StringLiterals` и конкретная проблема маркера комментария/интерполяции внутри C# строки. **OBS1 по уточнению родителя:** только пропуск `MemberBindingExpression` (`cache?.Add`) в `MethodFrequencyTable.ExtractMethodCalls` и знаменатель E02. Эти имена и этот механизм в новых файлах также не найдены.

Новые N52 документы действительно описывают соседние вопросы **других** компонентов: первые10 records в `DesignConversionTrainingPipeline` и `ObservationCount` вместо дельты весов в `DesignConversionLocalContinueTrainingProbe`. Это не пересечение с OBS1; совпадение слов «наблюдения», «probe», «проверка» не создаёт одинаковый механизм. Другие N52 касаются OrchestrationReasoning checkpoint-v1→v2, admission, generated manifests и маршрута trained Unit.

После этой дельты полный текущий Markdown-срез двух каталогов содержит **671 уникальный путь**: начальные661 +10 новых. Проверено673 Markdown-версии (661 исходная +12 изменённых/новых);2 изменённых версии не объявляются новыми независимыми документами. Два дополнительных Python иJSON не входят в эти Markdown-счётчики. Следующий изменившийся ref потребует нового короткого delta check, а не повторной загрузки всего корпуса.
