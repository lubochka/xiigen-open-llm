# Последняя дельта core: две адресные проверки новизны

**Вердикт: ACCEPTED_WITH_EXPLICIT_NOVELTY_QUALIFICATION.** Шесть конкретных source findings сохраняются; **PT-01 следует описывать как новую локализацию ранее открытого общего lead**. Общая идея duplicate → top-two self-comparison / margin collapse уже есть в позднем core-документе. Называть её впервые предложенной нашим пакетом нельзя.

## Снимок и граница проверки

Финальный core docs cutoff: `358af351e22a92a3f7938d1f9221d5aeb7ddb053`; исходные доказательства кода остаются привязанными к `44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76`. Planning cutoff — `b0308fbaa783758104987e6544d057062ccf6fc3`. Дельта содержит 13 Markdown версий: **2 новых пути + 11 замен**. Объединение сохранённых слоёв корпуса пересчитано: **689 уникальных Markdown = 415 planning + 274 core**. Последующие изменения репозиториев находятся за границей этого снимка.

Прямо сопоставлены два новых первичных текста: [NK-03, 12:105–113](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/12-additions-wiki-sql-knn-tensor.md#L105-L113) и [открытый duplicate lead, 13:74–80](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/13-additions-feedback-bpe.md#L74-L80), с уже принятыми source chains наших 02B и 03. Это узкая проверка указанных утверждений, без нового полного чтения 689 документов или повторного исполнения кода. Интегратор установил docs-only характер commit и сообщил нулевой точный поиск KU/PT/AUT в 13 файлах; отсутствие точных имён **не устраняет семантическое пересечение**.

## Классификация

| Находка | Что уже заявлено в новом core-тексте | Что остаётся конкретным результатом нашего пакета |
|---|---|---|
| **KU-02** | NK-03: `KnnSkillFilter` получает global topK и лишь затем пересекает его с `skillCandidates`. Общий принцип «допустимость после отсева может удалить всех допустимых кандидатов» известен. | Другой owner и критерий потери: `DeterministicRegistryEdgeCandidateProvider` вызывает whole-registry relative score floor, затем `edge.ToNodeIds`; отказ proposal останавливает learned inference. **Отдельная конкретная вариация известного семейства ordering**, не новая общая идея scope-before-loss. |
| **PT-01** | 13 прямо описывает duplicate winner, самосравнение top-two, margin/abstention и оговорку об intentional multiplicity. Статус **OPEN_RECIPIENT**: класс/метод и достигающий caller не установлены. | `ContinueWithAcceptedPattern` добавляет принятый semantic duplicate с другим ID; `PromptToTaskTaskPatternUnit` получает равные top scores и margin0; выбранный экземпляр использует загруженную память; `Translate` может перейти к generic fallback с изменённым StructuredTask. **Новая конкретная локализация owner + accepted-continuation trigger + consumer**, при явном признании prior generic lead. |

Для PT-01 полная semantic identity остаётся обязательной: не только TaskType, но LearnedSignals, требования, ограничения, результаты, запреты, необходимые hosts/evidence, SplitId и Confidence. IDs/provenance учитываются отдельно. Потребитель должен использовать нужное загруженное состояние; прямую причинность относительно isolated RP01 или восстановление donor neural trainer эта локализация не доказывает. Предложенные в позднем core файле восемь решений также названы будущей проверкой, а не результатом native запуска.

**Допустимая итоговая формулировка:** «Шесть конкретных гипотез, привязанных к исходникам; PT-01 локализует ранее открытое общее предположение о duplicate/margin в accepted continuation и обычном PromptToTask consumer». Остальные четыре finding этой узкой сверкой заново не сертифицируются; действуют границы предыдущего review.

## Точные байты прочитанных источников

| Файл | Git blob SHA | SHA-256 |
|---|---|---|
| `12-additions-wiki-sql-knn-tensor.md` | `69bb7b6c3fc4002b818ddc66bbe73607e3f77786` | `1b9b9b50160b0885ed9ccc6870a58354e7a9356b9a9be7100460aad98fc0313d` |
| `13-additions-feedback-bpe.md` | `82f2ffeb394f74db6d9256075e1f36e591335688` | `7b687f1f498ed0585d4321d521eed03e1d2d1101f6859071125de28ecd914473` |

Оба Git blob SHA повторно вычислены по локальным байтам и совпали с `source-metadata.json`; его SHA-256 — `7b4802da9c7061732adf675326a39c4dabf31a940d67525bdaa9f07da9045b85`. Полные source paths, epochs и границы прочитанных секций сохранены в `core-late-delta-review.json`.

**0 native runs, 0 обучения, 0 внешних model calls, 0 GitHub writes.** Candidate и прежние receipts не изменены; создано только это дополнительное заключение и его JSON. Уточнение новизны относится к финальному cutoff и дополняет исторические receipts на 671/687 документах.
