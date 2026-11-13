# Источники, метод проверки и независимый review

Дата:20 октября 2026. Репозитории:

- `lubochka/llm_mvp_core`, ветка `codex/dpo-implementation`, закреплённый commit `883496bca18113bf37e0afe683c75b05a0075048`.
- `lubochka/xiigen-planning`, ветка `llm_plan3`, исходный commit `6bc4b6fba8f14870f5dc4966cf5a344754962d66`.

При первом readback core совпал с883496; затем появился новый core66947785d1318f7cbbc88118f3156cdc75001e16 с пятым результатом и документными исправлениями. Выполнено [адресное дополнение](07-core669-status-and-results.md). Planning продвинулся двумя параллельными commits до `53cbcabd3926cda714bab428d294eb666301e9e4`. Их additions и изменённый README сохранены; пакет согласован с этим новым родителем. Публикация должна добавлять только новый pack и отдельный entry в possibleDifs; предыдущие файлы и protected остаются неизменны. Конкретный commit публикации доступен в истории GitHub и отчёте пользователю; он не используется как задним числом созданный source baseline.

## Источники и предел их чтения

[Repository content manifest](coverage/repository-content-manifest.json) фиксирует все 1313 possibleDifs и 1253 protected entries. Все 747planning Markdown и 334 protected Markdown локально сверены с remote Git blob SHA. Кэш другого чтения принимался только при совпадении blob, не по имени/mtime. Большие двоичные/LFS объекты не выдаются за прочитанные payload.

[Инвентарь RAG/пакета RP01](coverage/rag-source-manifest.json) содержит 46 entries, полученных отдельно; parent primary документы перечислены в [механизмах](01-learning-mechanisms-and-rag.md). RAG использован как навигатор и через consequential sections. Отсюда не следует ручное чтение каждого абзаца всех многомегабайтных RAG или всего репозитория.

[Ветки](coverage/branches-inspected.json) дают inventory15refs, а [исторический source manifest](history/evidence/current-history-source-manifest.json) и [соседние blobs](history/evidence/sibling-source-blobs.json) — конкретные содержательные проверки E07/E09/E14/E22. Наличие ref в inventory не является аудитом каждого файла ветки. Exact donor matrix→kNN в данном проходе не установлен. Новое concurrent receipt фиксирует конкретный kNN→production commit27ae032; это другое, более узкое утверждение о provenance, см. [дополнение](history/05-concurrent-primary-reconciliation.md).

[Два новых фактора и дедупликация](learning/learning-evidence.md), [поисковые данные](learning/learning-novelty.json) фиксируют границу новизны: текущий корпус, ближайший LF-03/RMS предшественник и source mechanism. Первоначальная идея FlatToIndices признана уже известной и не опубликована как новая.

После конкурентных изменений отдельно получены и Git-blob-сверены все 53 added/modified Markdown; [manifest](coverage/concurrent-source-manifest.json) сохраняет identities. Общий updated tree содержит 1400 файлов /799 Markdown до текущей публикации. Семантическая сверка ограничена относящимися к очереди, novelty и primary-readback новыми документами; literal search всех 53 не заменяет полный semantic review.

Два original RP01 `scientific-result.json` на 83590 загружены напрямую как actual JSON. [Сохранённые summaries и identities](history/evidence/original-rp01-primary-summaries.json) подтверждают цитируемые поля; это не независимая реконструкция binaries, individual grades или всей training campaign. Другие source-свидетельства этого дополнения описаны в [согласовании](06-concurrent-reconciliation.md).

## Использованные навыки и обновления пользователя

Применены текущие репозиторные architect-behavior-classifier, phase-planning, plan-review, plan-self-validation, code-examination, learning-path-audit, prompt-patch-authoring, human-readable-architecture-audit и human-communication glossary. Предоставленные документы и старые skill-пакеты прочитаны в относящихся к задаче частях. При конфликте действуют нынешние 14 правил пользователя: небольшая experimental copy, любой полезный metric gain, русский отчёт, минимум действий, никакой product/CI цепочки до решения, immutable old experiments, немедленный NEXT/STOP.

Личные/сессионные сведения использовались только для поиска источников. Историческая реплика агента или прошлый status не считается фактом текущего кода, пока он не проверен.

## Что сделано для измерений

Нового native Train, C# build, forward/generation, optimizer update или бинарного readback модели в этом аудите не было. Результаты пересчитаны по уже сохранённым arm JSON. Полезные наблюдаемые дельты отделены от causal attribution, post-update quality и доменного переноса. Original reports не переписаны; исправления добавочные.

[Результаты пересчёта](experiments/measured-results-recalculated.json) и [N62-R1 corrigendum](experiments/N62-R1-RESULT-CORRIGENDUM.md) показывают raw basis. Отсутствующий LFS payload обозначен прямо. Идентичность LFS OID не названа независимым бинарным восстановлением.

## Независимая проверка и внесённые поправки

[Review](experiments/INDEPENDENT-PACK-REVIEW.md) исходного 35-row draft проверил новые source-факторы, exact before/after, единую очередь и paths. [Машинная сверка mapping](experiments/independent-root-mapping-review.json) относится к первоначальному draft и сохраняется как свидетельство review, не как hash финальной редакции.

По трём замечаниям внесены точные поправки:

| Review | Что исправлено |
|---|---|
| R-01 | В tensor-договор явно добавлены DPO beta0.1,LR1e-5,AdamW beta1=0.9/beta2=0.95, weightDecay=0.01, epsilon=1e-8 и actual settings assertion. Native source ссылки приложены |
| R-02 | URL fragment `#queue-*` восстановлен вместо ошибочного `%23queue-*`; переходы ведут к конкретным подкарточкам |
| R-03 | Утверждение о буквальной идентичности root STOP-текста заменено утверждением об одинаковом правиле; обе новые карточки и их промпты содержат полный запрет продолжения прежней работы |

[Финальный документный протокол](coverage/final-document-validation.json) проверяет наличие всех файлов/ссылок/якорей, JSON, согласованность counts/queue/унаследованных output assignments, отсутствие новых outputs в protected и область added-only публикации. Это достаточная документная проверка, не замена runtime тестирования и не повод запускать опыты.

Согласование финального 38-ID реестра основано на [concurrent merged registry](experiments/concurrent-merged-registry.json): все 36 inherited ID/order и 32 output assignment сохранены, добавлены 2 ID/output, изменений inherited полей 0. Старые reviews35/31 и38/34 не управляют исполнением. После669 активна проекция38/5completed/33pending; R2 архивирован и NG01 mapping явно заменён, прежние значения сохранены.

## Полнота и следующие решения

Структурный inventory полный для закреплённых папок; адресные semantic gaps документированы. Полная научная аттестация каждой фразы 747 Markdown/470 подслучаев и build всех future hosts не заявлена. [Границы](03-coverage-and-limits.md) содержат точные unresolved cases вместо фиктивного сертификата.

Следующий невыполненный опыт — LF-03; №01–06 завершены. Новые tensor-факторы стоят в общем реестре после прежних прямых учебных пунктов; conditional E07 и неготовые конкретизации не получают скрытого runtime разрешения. Полезный сохранённый N62-R1 оформлен кандидатом. Внедрение в production решает Люба.

## Окончательный cutoff

Срез статуса зафиксирован на core827a1acdbef95d220886e54f9d36bce0b405c8cd:38 ID,6 completed,32 pending,NEXT=LF-03. [Срез08](08-final-core827.md) включает E-OPT actual raw mismatch, exact LF-03 overlay и ROPE/AST/BPE output readback. Дальнейший HEAD не преследовался; source pins неизменяемы. Последняя документная проверка относится к этому cutoff.
