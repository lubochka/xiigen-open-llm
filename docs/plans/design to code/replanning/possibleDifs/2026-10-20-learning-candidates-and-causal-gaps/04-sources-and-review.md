# Источники, полнота корпуса и границы доказательств

## Зафиксированные версии

Основной анализ выполнен на `llm_mvp_core:codex/dpo-implementation` **9622b575a6aabb79939b12cc9f5d96313f1bf370** и `xiigen-planning:llm_plan3` **de270758087502a350c4fcf460acdf1638bdd318**. Все первичные source links в основном пакете закреплены на commit SHA, чтобы их смысл не менялся вместе с веткой.

**Последняя адресная сверка: core `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`, planning `f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9`.** N-KU-01 уже зарегистрирован в protected `required-changes.md` внешним коммитом; мы этот реестр не меняли. LF-03 теперь содержит точную fixture и отдельный одношаговый бюджет. В текущем каталоге 135 карточек; исходная полная матрица 130 остаётся привязанной к `9622b575`. Две новые cost-гипотезы повторно проверены на дублирование в поздних документах. См. [финальное дополнение](05-final-publication-delta.md), где отдельно указаны закрытые замечания и оставшиеся ограничения. Новых runtime-измерений и product changes в этой сверке нет.

### Соседние ветки

| Ветка / pin | Как использована |
|---|---|
| `language_translate` — `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` | Адресно прочитаны донорские KNN/PatternClassifier и сопоставлены current. Общие inherited риски не названы новыми регрессиями |
| `local_wiki_planning` — `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a` | Прочитаны два training pipelines и три первичных сохранённых training reports. Сравнена материализация prompt/chosen/rejected |
| `orchestrator-extensions` — `b293045bd89b1eb96d3e32fedd341f1a4170adbc` | Прочитан первичный E43 replay receipt; current restore/admission отдельно прочитан по текущему source pin |
| Остальные ветки | Сохранён полный список15 names/SHAs в [branch-inventory.json](branch-inventory.json). Наличие в списке не означает полный diff или повтор всех опытов этой ветки |

Текст исходника на branch head не является аттестацией исторического бинарника, создавшего May receipt. Поэтому цифры обозначены как сохранённые результаты, а source различия — как отдельно проверенные факты. Мы не восстанавливали все прошлые окружения и не переобучали модели.

## Полнота документов

| Корпус | Что действительно покрыто | Что не заявляется |
|---|---|---|
| `possibleDifs` на исходном planning pin | Все **662 Markdown** получены, проиндексированы, searched для novelty; Git blob identity проверена662/662 | Ручное смысловое прочтение каждого файла целиком; аудит всех non-Markdown артефактов |
| Protected на основном core pin | Все **296 Markdown** присутствуют:92 root +8 experimets root +130 cards +24 small-learning docs +42 result MD; missing0 | Ручная сертификация всех old claims и всех884 result artifacts |
| Карточки и малые документы | Structural inventory всех **130+24**; named subcases/domain profiles/routes; отдельный stage и exact missing fields | Наличие470 subcases не означает470 выполненных или причинно закрытых экспериментов |
| Учебный код | Адресно прочитаны computational/consumer chains для DPO/LoRA/Embedding/MatMul, KNN, translation ingress/restore и выбранных соседних веток | Полный source audit всего repository или всех продуктовых веток |
| Сохранённые результаты | Raw/receipts/source review выбранных positive cases; пересчитаны numerical deltas neural cases; N-KU сверён по arms/gold | Повтор runtime, новый parser/Train/Predict, свежая compiler/ABI проверка или system-level quality claim |

Машиночитаемые документы:

- [planning-corpus-manifest.json](planning-corpus-manifest.json):662 путей, Git blob SHA, SHA256, sizes и результат byte match.
- [protected-corpus-inventory.json](protected-corpus-inventory.json): полный список296 Markdown на основном core pin.
- [coverage.json](coverage.json):130 cards,24 small docs,470 subcases,993 domain-profile sections, source lines и статусы.
- [card-source-byte-check.json](card-source-byte-check.json):154 исходных файла карточек/малых документов проверены по pinned Git blob после восстановления исходных LF/CRLF. Поля `local_blob`/`size` относятся к materialized версии до нормализации, `final_git_blob`/`final_sha256` — к проверенным исходным bytes.
- [positive-neural-candidates.json](positive-neural-candidates.json): расчёты по сохранённым JSON. Hashes этих receipt files имеют честный `hash_scope=local_materialized_download`; они не выданы за raw Git blob identity.
- [final-source-byte-check.json](final-source-byte-check.json): 10 последних полных Git blobs на `ab583032`, точные bytes и совпадение Git SHA-1 до и после локальной записи.
- [late-core-diff.json](late-core-diff.json): точечное изменение docs между основным pin и новой редакцией, без повторной интерпретации неизменённого product source.

Граница296 относится к основному immutable core pin. Финальная delta review новой редакции не переписывает исходные hashes/line anchors старой матрицы.

## Как проверялась новизна

Поиск выполнен по полному Markdown-корпусу **662+296**, затем прочитаны релевантные совпадения. Для KNN использованы exact owners, `ParseText`, prepare/train chain и сочетания duplicate/shared/reuse, `_pairs.Clear`, sparse/retention. File-level AND проверял формулировки, разнесённые по разным строкам. Для gradB использованы `gradB`, right operand/adjoint и `AᵀG`.

После дозагрузки последних21 result MD поиск повторён: результаты прежние. Exact gradB query дал3 строки prior art; известный N-ADJOINT исключает gradB. KNN file-level AND дал5 planning и3 core документа, но они описывают другие owners или prior labels. KH-COST остаётся новой узкой гипотезой, KH-EXT — уточнением известного LT-N02. [KNN novelty](knn-novelty-ledger.md); [gradB novelty](dpo-lora-novelty-search.json)

Это ограниченное утверждение о проверенном corpus, не абсолютная новизна в мировой литературе, неполученных чатах или будущих коммитах. Новая редакция N62 добавляет уже известный LF-03 initialGradient-copy factor; он не равен gradB loop order. Финальная delta review отдельно проверяет это различие.

## Применённые навыки и правила

Из запрошенного `docs/skills` использованы релевантные разделы архитектурного классификатора, phase-planning, plan-review, code-examination, generated-code-review, prompt-patch-authoring, human-communication glossary, learning-path-audit, improvement-measurement и trainable-architecture-decision. Их применение означает: назвать actual recipient, original code/consumer, один фактор, измерение, границу домена, недостающее evidence и ясный handoff.

Большие skill/AGENTS документы не объявляются полностью прочитанными только по скачиванию. Старые требования полного production repair/CI или blanket approval не подменяют прямое текущее поручение пользователя об isolated experiments и документации. Конкретный current source важнее старой метки bug; отсутствие measured effect важнее уверенного названия гипотезы.

## Что проверено перед публикацией

Выполнены документные проверки: численные пересчёты, правильность имён исходных классов/методов, причинная цепь candidate, source URL/line bounds на доступных локальных снимках, относительные ссылки пакета, JSON parse и additive-only publication scope. В независимом cross-review исправлены неверные имена `ActionLabelGenerator` и порядок сборки gradB host; источник TensorCore B не должен затираться выходами сборки A.

Это проверка документации, а не новый experiment run. Ничего из описанных будущих csproj/Program/runtime не создавалось в продукте и не исполнялось. Неизвестная доступность архивных binaries/C50 отмечена прямо; будущему исполнителю не поручено выдумать замену.

## Откуда следует положительный статус

Положительность определяется по отдельному полезному показателю на указанной задаче, а не по единому флагу `passed`. Для N-KU это меньше ложных советов и больше выполнения заданного требования; для embedding — восстановленный trainable recipient и меньший online objective; для mask — правильный активный градиент; для ingress — больше правильных композиций; для restore — сохранённая приобретённая способность. Остальные показатели сохранены вместе с улучшением.

При этом отсутствие связи с обучением не лечится переименованием метрики. E26 context-only и неактивный bias в C50 не получают статус нового улучшения обучения этой модели. Source-confirmed KH-COST/GRADB/KH-EXT не получают выдуманных измерений.
