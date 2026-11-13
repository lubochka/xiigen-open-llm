# Источники, метод проверки и границы ревью

Дата: 2026-10-20. Это журнал документного исследования и публикации дополнения. Он не является журналом нового обучения или заменой результатов старых опытов.

## Проверенная область

| Слой | Получено / проверено | Что не заявляется |
|---|---|---|
| Planning possibleDifs | Полный список и тексты886 Markdown на `0ea8f0b0094e234ffe7734adfa95e2a9e53203f5`; поиск точных механизмов и синонимов, целевое чтение | 886 независимых смысловых ревью или «все причины найдены» |
| Protected experiments/docs | Полный список и тексты362 Markdown на `dd150aa5216e42b8a8bba14955337398ec5f8397`; инвентаризация161 карточки | Runtime readiness каждого host и полная валидация всех подслучаевP01–P125 |
| Сохранённые результаты | Raw A/B первых8; errata/provenance; E09/E22/E25/E43; translation memory/ingress/restore; 400-step probe | Перезапуск, переизмерение, восстановление исторического exact B binary при placeholder hash |
| Current code | Связанные learner→state→reader участки DPO/LoRA/Trainer/TensorOps, checkpoint loader, Unit/kNN/translation/router | Полный code review всех файлов, binary execution или математическое доказательство всех kernels |
| Соседние ветки | Branch inventory; E25 primary raw на `language_translate@f6032aff…` и `DesignToCode-clean@e346b88…` | Полный аудит всех веток и обнаружение всех surviving trained checkpoints |
| RAG/родительские документы | RP01 README; DPO/RAG branch indices; DPO builder, lifecycle, hidden determinism, same-scenario sections | Исполнение исторических команд и принятие исторических status как текущих |

Все1248 Markdown сверены по формуле Git blob SHA1: `sha1("blob " + byteLength + NUL + rawBytes)`. Семь исходников содержат смешанные CR/LF; для точного соответствия сохранены исходные bytes, а не нормализованный текст. Итог совпадений1248/1248. [Манифест](data/source-corpus.ndjson) содержит repository, точный path и git_blob; SHA snapshot заданы выше. Это обеспечивает полноту поискового корпуса и происхождение текста, но не его смысловую правильность.

## Применённые навыки

Использованы запрошенные архитектурный разбор, планирование, проверка планов/кода, prompt engineering и человеческая коммуникация. Источники — приложенный пакет skills и `llm_mvp_core/docs/skills` на указанном SHA.

| Skill | Как использован |
|---|---|
| architect-behavior-classifier | Разделение fixed rule, обучаемого состояния и обычного consumer; не объявлять memory learning не-обучением |
| intent-to-plan / planning-session-startup | Сохранение текущего задания, protected scope и приоритетов; действие по NEXT вместо продолжения прошлого опыта |
| plan-review / experiment-design | Один фактор, точный state/data/budget, отсутствие выдуманных команд и результата; готовность отдельно от описанной идеи |
| code-examination / generated-code-review | Адресные caller/state/consumer и ограничения исходного кода; copy-only proposed correction |
| prompt-patch-authoring | Однозначный исполнительский контракт, модель по умолчанию, точное направление следующей задачи |
| human-communication-GLOSSARY / human-language-gate / output-readability-gate | Русский отчёт, предметный смысл домена, успех и регрессии рядом, явные пределы |
| experiment-data-integrity / dpo-triples-to-experiment-pairs / improvement-measurement из приложенного пакета | Сохранение raw, корректные пары и split, любая полезная ось как успех |

Короткие skills и связанные разделы прочитаны; для крупных core skills применялось целевое чтение. Полная загрузка всех больших файлов не заявляется. Старые нормы об accuracy-only, полной production цепочке или выборе другой executor model не заменяют прямые требования пользователя. Текущий запрет менять protected folder имеет приоритет над исторической policy, в которой упоминалось разрешение другой сессии.

## Разделение работы

Первоначально были выданы три независимые исследовательские задачи. После чтения scoped AGENTS обнаружено ограничение «не более одного активного research worker»; два worker остановлены, дальнейшие задачи выполнялись последовательно. Это изменение процесса зафиксировано; одновременное независимое ревью не заявляется.

| Исследователь | Полученный пакет | Вклад |
|---|---|---|
| `/root/learning_mechanisms` | late-neural-cards-rule15-review; odd-head-dim-candidate-and-dedup; additional novelty packet | Четыре neural-рецепта, B0/source, проверка предполагаемой новизны, дополнительный SFT фактор |
| `/root/historical_evidence` | evidence-packet | Raw первых7, исторические runners/results, соседние ветки и translation memory |
| `/root/units_retrieval` | new-hypothesis-packet | Writer/reader units, kNN и restore; десять точных дублей не продвигались как новые |
| `/root` | Сводные документы, полный корпус, таблицы и публикация | Дедупликация на1248 Markdown, перекрёстные ссылки, исправление номера40 routing, сверка exact blobs |

Пакет от агента не равен автоматической приёмке: root прочитал выводы и связанные источники; odd HeadDim был отклонён после полного поиска, несмотря на первоначальное предположение о новизне. Подтверждения нового runtime результата от агентов не принимались, поскольку таких запусков не было.

## Воспроизводимость поиска новизны

Для каждого кандидата сначала прочитаны текущий source и путь до реального learner/consumer, затем точные имена API и русские/английские описания причины искались в обоих полных корпусах. Найденный ранний owner имеет приоритет над новым именем. В качестве отрицательных примеров сохранены:

- DPO FP32 sum → DLT-2 в PREPARETOGPL-DPO-LORA-TOKENS-2026-10-18.
- odd RoPE head dimension → protected47-current-attention-backprop-and-position-contract, строка38, плюс planning50 novelty ledger.
- nested KnnExamples → P24.7; tokenizer/executor keys → P106.3/.4.
- source language key → P51.8/P25.5; role-blind gender → P55.3.
- verifier context/substring/accumulation → P56.1/.7/.8.
- no-op readiness / duplicate lesson → P53.3/.4; metadata carrier → P108.
- learned router key canonicalization → KN-ROUTER-LEARNED-KEY-01, исторический core№40; согласованный текущий№42.

Отсутствие точного аналога в этом корпусе обозначается «новый кандидат относительно проверенного корпуса», а не мировая новизна и не гарантия отсутствия упоминания в непрочитанных ветках/бинарных артефактах. Связь новой гипотезы с существующим соседним фактором раскрывается в06.

## Проверка самой публикации

Содержательные проверки: текущие приоритеты; запрет product/protected edits; отсутствие нового результата без raw; раздельные метрики/домены; правильные номера очереди; отсутствие переобъявления известных механизмов новыми; точный статус незавершённой diagnostic readiness; STOP/NEXT в новой карточке и её промпте.

Технические проверки документации: существование относительных ссылок внутри дополнения, соответствие ссылок на карточки pinned inventory, JSON/NDJSON parse, исходный счёт161/125/40 и поздние163карточки/1281источник/42существующихID, UTF-8 и readback опубликованных файлов. Это не product tests и не новые learning experiments.

Итог независимого документного ревью записан в [финальном receipt](10-publication-review.md). Точный SHA публикации и удалённый readback сообщаются пользователю после записи; до этого нельзя считать файлы опубликованными только по этой странице.

## Поздняя дельта81141d57

После базового корпуса1248/161 проверены ещё5protected Markdown и2карточки: итог1253Markdown и163карточки. Добавлены exact commit/git blob записи в source-corpus.ndjson; все5текстов совпадают с Git blob. В base161выполнена инвентаризация, у двух новых адресно разобраны источники, lifecycle и конфликты. Полного построчного смыслового ревью676строк JSONhost не заявляется.

RESTORE-MINIMAL-SET: прочитаны полный Program.cs, report.md, raw JSON, project и launcher; пересчитаны агрегаты только из raw, сравнены все сериализованные панели prior/restore. LFS pointers получены, payload не загружались. Source/report исправления и полезный результат в11, без runtime и повторного опыта. Независимое review baseпакета не выдаётся за отдельное полное ревью этой поздней дельты.

## Поздний planning2d174126 и первичный №08

Перед записью другой агент опубликовал36 новых файлов, из них28Markdown. Все28 получены по неизменяемому `2d1741261e9e17c0e4dec94a480b5711f5ad47d3`, локальные bytes совпали с Git blob SHA; добавлены в source-corpus.ndjson. Полный итог **1281Markdown:914planning +367protected**. Новизна SFT-EXP повторно проверена по этим28 полным текстам теми же точными запросами; они описывают другие факторы. Их два предложения A1/A2 сохранены и не присвоены нашему исследованию.

По первичным N63I arms/status/per-row исправлен ошибочный NEXT08, который ранний индекс и наше предварительное ревью повторяли. Root прочитал arms/status и пересчитал TRAIN/EXAM показатели из per-row; №01–08 завершены, NEXT09. Дополнительный readback исторического исследователя проверен по первоисточникам. Независимое base review предшествовало этой поправке и не является доказательством актуальности старого NEXT. Согласованный источник — [registry](https://github.com/lubochka/xiigen-planning/blob/2d1741261e9e17c0e4dec94a480b5711f5ad47d3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-dd150aa/REGISTRY.md); точные evidence/пределы — [документ12](12-current-queue-and-n63-evidence.md).
