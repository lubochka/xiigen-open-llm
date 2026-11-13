# Обученный старт, восстановление и принятие родителя

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход перед новым аудитом или заданием

Обязателен [договор120](../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

До действий прочитать выбранную карточку целиком и её локальную секцию120, source/raw и фактический маршрут producer→ordinary caller→learner→consumer; local HEAD, published SHA и dirty bytes различаются. Списки навыков ниже — дополнительные, не освобождение от полного свежего стека120. Исторические Tier3/vocab/split/UI/Git/model правила не расширяют нынешний паспорт и авторизацию.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.

**Сохранение постановки:** exact recorded result не повторять, old NOT_RUN не выше текущей Q-строки. Полный outer/nested бюджет и независимый gold остаются у карточки; пересчёт raw/структурный guard сTrain0 не заменяет обязательный учебный цикл и не получает фиктивный Train. Команды, API и фильтры не додумывать исполнителю. Вход обязателен также в передаваемом промпте выбранного ID.
<!-- EXECUTOR-CONTEXT-120:END -->


**Обновление33: сохранённый старт вместо повторного bootstrap.** Для16назначенных consumers действует [карта12семейств](../reconciliation/shared-learned-starts-2026-10-21.json) и [контракт33](33-shared-learned-starts.md): отдельно заданный producer выполняет common1000/400 один раз; consumer wrapper только проверяет COMPLETE/qualification/identity и восстанавливает S0, без common Train/добавочных readouts. Missing bundle не разрешает Prepare. Старые common-пункты ниже — описание producer, не последовательность каждого consumer. H16/privatePA01 и consumed attempts сохраняют отдельные contracts; текущий LC не перезапускать.


**Обязательное уточнение 21.10: [отсутствие прогресса и валидность обучения](30-failure-recovery-and-learning-validity.md).** Исполнитель находит доступные launch/wait/status и читает их схему; живую операцию ждёт bounded wait того же handle. Два быстрых пустых snapshot требуют перейти в wait; нормальный wait при silent/running продолжается. При неисправном wait — независимая диагностика, без relaunch и вопроса «продолжать ли». Отсутствие log не доказывает отсутствие запуска. 0→0, отсутствие обучения или неполное A/B не закрывают учебную цель: нужен ремонт постановки, а не только отчёт о невалидности. Фактические updates, статус выполнения и учебный результат учитывать раздельно; поздняя ошибка записи не обнуляет обучение. Прямые STOP/NEXT Любы немедленны. Эти правила обязательны для каждой карточки и каждого промпта и заменяют противоречащие указания ниже. [Точные tools/skills и ожидание](31-tool-discovery-and-wait.md).

20 октября 2026. READ_ONLY_RESEARCH + документы. Этот контракт не разрешает запуск, загрузку модели, публикацию checkpoint или правку runtime. Исполнитель будущей разрешённой задачи использует модель своей сессии по умолчанию, без override. Student назначается отдельно конкретной карточкой.

## Как разрешать конфликт о нулях и бюджете

1. Люба просила минимальный набор, показывающий реальное обучение, а не установленное ею число2/4/9 updates. Эти числа были авторским дизайном. Минимальное число строк и минимальное число шагов — разные величины.
2. При0правильных response-токенов и0точных ответов во всех сравниваемых состояниях вердикт об усвоении/сохранении навыка — LEARNING_VERDICT_INVALID_ZERO_TO_ZERO. NLL/rank movement и измеренные механические/ресурсные факты остаются отдельными наблюдениями; они не доказывают освоение.
3. Ненулевые токены — ограниченный сигнал, а не целые верные ответы. Указывать denominator, TRAIN/EXAM отдельно и сравнимый constant baseline, когда он применим. Для сбалансированного sign задания постоянный знак даёт1/2; для многострочного кода «в каждой строке есть один верный токен» не доказывает условное правило.
4. Для retention и restore исходный навык должен быть показан до вмешательства; сравнивать одинаковое состояние A/B, включая tokenizer, LoRA topology, optimizer и применимый RNG. Одинаковый первый loss не идентифицирует всё состояние.
5. Статус прежних неподтверждённых проектов — DESIGNED_REQUIRES_LEARNING_VALIDATION. Cold acquisition допустим с конкретной проверкой приобретённого поведения, но известная нулевая постановка не передаётся на повтор. До retention/restore вмешательства требуется измеренный навык. Достаточность TRAIN и бюджета, назначенный before и единица усвоения — обязанность автора карточки, не поиск исполнителя. Правило «не создаёт pre-run gate» отменено: проверка пригодности before обязательна там, где сам вопрос требует уже выученного навыка.
6. При неподтверждённом before вмешательство retention не начинается. При 0→0 после acquisition или неполном A/B учебная цель остаётся OPEN и comparison invalid. Недостаточно отчитаться этим статусом и считать эксперимент выполненным: автор исправляет конкретный рецепт в разрешённой документной фазе. Реальные raw и расход сохраняются; нет автоматического повторного запуска или steps до PASS.
7. Дополнительную подготовку автор описывает заранее: exact inputs, совместимый checkpoint либо конечный acquisition schedule, критерий/STOP и весь расход Train/Forward. Исторические probes не являются бесплатной подготовкой, source-код probe не доказывает обученный state. Невалидность требует ремонта постановки по контракту30; самостоятельный поиск модели, seed и повтор consumed attempt запрещены. Прямой NEXT Любы немедленный и сохраняет прежнюю цель OPEN.
8. Unit/kNN/BPE обучение из штатного пустого состояния и существующий P25 cold consumer обученного состояния сохраняют свои контракты. Отдельный реальный schema/provenance пробел нового P02.PARENT описан ниже; его нельзя переносить на остальные карточки.

## Что сейчас действительно сохранено

| Состояние | Происхождение и measured signal | Совместимость и ограничение |
|---|---|---|
| H16 SFT after1000 | RESTORE-MINIMAL-SET/results/trained-weights.bin + trained-optimizer.bin; TRAIN3/40response tokens,0/2exact; reload сохраняет панель | V256/context64/H16/L1/Q2/KV1/FFN32,r2/a4; continuation200 дал4/40. Это private raw SFT state |
| H32 SFT WA after1000 | N63R-PROBE/results/wa-trained.bin + wa-trained-optimizer.bin;45/92правильных response-токенов A01/A07,0/2exact | V256/context256/H32/L1/Q4/KV2/FFN64,r8/a16; частичный навык, сильные tradeoffs после новых уроков |
| H64 RMS/ATTN/ROPE recipes | Точные новые configs/fixtures/source pins находятся в своих карточках | Эти карточки не получили квалифицированный learned-state receipt от H16/H32; перенос невозможен по несовпадению формы |
| H512 native DPO B0 | generation-v1.json → rp01-csharp-small/v1, InitialBaselineNoUpdate | B0 не принят за обученный. Private DPO descendants и raw SFT states не подставляются в parent selector без проверенной lineage/custody |

Корень первых двух строк: C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO. Их byte hashes, размеры и полные пути сохранены в [квитанции](../2026-10-20-card-reconciliation/n63-learning-parent-readback.json). Чтение binary bytes для digest не было десериализацией модели. Никакой новый checkpoint не создан этой документной работой.

## Native parent существует, raw SFT import — другой контракт

[Primary evidence исследователя](../2026-10-20-parent-adoption-primary-evidence.md) показывает фактическую цепочку:

- [Publish](C:/Projects/llm_mvp_core/26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointPublicationService.cs:108) сохраняет и seal-ит модель; при parent пишет parent.Step+1 с его identity. Первый обычный обученный root тоже может иметь Step1/parent=null. Только проверенная специальная роль InitialBaselineNoUpdate означает отсутствие update.
- [LoadLatestCompatible](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/RepositoryOwnedModelCheckpointLoader.cs:96) выбирает старший совместимый sealed step; [PrepareStart](C:/Projects/llm_mvp_core/26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointStartService.cs:99) загружает его. Это не основание запускать START сейчас: он может подготовить clone custody.
- [Coordinator](C:/Projects/llm_mvp_core/26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs:2164) требует настоящий same-lineage source attempt, journal, optimizer manifest и RNG. Raw portable optimizer не заменяет source proof.
- [Manifest](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/RepositoryOwnedModelCheckpointManifest.cs:823) требует настоящий sealed DPO dataset и соответствующую applied history. H16/H32 SFT-пробы не получали preference rows. Приклеить им чужой DPO manifest или объявить trained state initial-no-update означало бы исказить происхождение.

«У найденных checkpoints step1/parent=null» не доказывает поломку принятия, причину всех нулей или невозможность всех будущих опытов. Учебные raw пробы не являются проверкой operational readiness native publication.

## Обязательный ремонт: PARENT-ADOPTION-01, затем исправленный №08

**Прежнее решение оставить P02.PARENT без entry было неполным.** Пользователь указала недостающий опыт ремонта, а не попросила только описать schema gap. Этот gap теперь превращён в отдельную активную [карточку PARENT-ADOPTION-01](cards/PARENT-ADOPTION-01.md). Цель: следующий SFT START выбирает принятый обученный bundle и восстанавливает weights/tokenizer/LoRA/AdamW m/v/step; после этого следующий учебный вызов продолжает именно это состояние. Наличие DPO Publish/Load в исходниках не является проверкой SFT caller.

Назначен совместимый с №08 **H16 snapshot RESTORE-MINIMAL-SET после1000**, не H32 WA. Точные bytes/config/panel и future typed SFT manifest находятся в карточке. H=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01; private repositories H/repos/A и H/repos/B; R=H/results. Предлагаемый SftAcceptedParent.cs — будущая реализация в experimental copy. Она честно обозначает ImportedReviewedSftSnapshot, исходный training parent unknown и отдельно adoption ordinal. Raw SFT не выдаётся за DPO; native DPO guards не отключаются. Canonical training-artifacts/models не меняется.

Карточка назначает writer, selector, fresh session factory, закрытый состав проекта, один entry, fixed budgets, проверки полной идентичности optimizer и собственные status/NEXT. Исполнитель больше не должен выбирать недостающий lifecycle. Наличие карточки означает **DESIGNED_NOT_RUN**, а не уже исправленный runtime.

Сохранённая попытка N63I теперь `ATTEMPT_CONSUMED / LEARNING_OBJECTIVE_OPEN`. После parent repair идёт [N63-INGRESS-LENGTH-LEARNED-01](cards/N63-INGRESS-LENGTH-LEARNED-01.md), H=N63IL: A/B читают один accepted000002/optimizer1000, затем A отклоняет overlength безupdates, B делает200SFT updates на двух коротких уроках. 20scoring Forward проверяют TRAIN/EXAM/CONTROL до/после. Старые1000updates не повторяются; два приращения parent-пробы не подменяют замороженный donor1000. Новый ID — исправленная постановка незавершённой задачи, не запрет когда-либо повторно проверить №08.

Учебная цель не закрывается словами CONSUMED, наличием raw, изменением weights или NLL. Corrected08 требует подтвердить partial before и измерить after; exact0/2 не превращается в «два выученных ответа». Если finite budget не дал прироста, зафиксировать отрицательный результат и оставить цель открытой; нового budget/seed/attempt доPASS не назначать. Это не общий запрет для независимых Unit/kNN/BPE опытов. [Машинная запись](../2026-10-20-card-reconciliation/parent-continuation-contract.json) перенаправляет на действующий repair. Старое предложение PC01/H32/entry=null историческое и отменено.

## Немедленный STOP/NEXT и отчёт

«Остановить всё» и прямой NEXT важнее оформления. Старые attempts №01–09 и частичный N63C не перезаписывать. Сначала PARENT-ADOPTION-01 → corrected08; N63C требует исправления учебной постановки и не выдаётся повторно по старой команде. При одиночном поручении — END, без объявления открытой учебной цели закрытой. Runtime этой документной работой не выполняется.

