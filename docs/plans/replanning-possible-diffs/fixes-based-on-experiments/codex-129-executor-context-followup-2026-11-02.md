# 129. Передаваемый контекст и различающие проверки карточек

Дата: 2 ноября 2026. Основание: приложенный пользователем отчёт об аудите43afaefe и его живые [находки126](codex-126-postmerge-transfer-and-experiment-audit-2026-11-02.md), [адресные исправления127](codex-127-postmerge-addressed-repair-plan-2026-11-02.md), [карта128](codex-128-postmerge-card-coverage-2026-11-02.md). Прочитанный рабочий срез core — `126c093201f187a409efef814a8ab3df582f8652`. Это применение документных исправлений; продукт, hosts и результаты опытов не меняются.

## Что исправлено в передаче задания

Наличие ссылки на договор120 над готовым промптом не гарантирует её попадание исполнителю. Первоначальный поиск выявил13 экспериментальных и28 переносных/общих файлов. Проверка других заголовков и строчных промптов расширила охват до185 различных файлов:147 экспериментальных карточек и38 файлов переноса/общих договоров. Например, готовый промпт13 перечислял семь навыков, а свежий полный стек и последующие ограничения находились выше, за границами копируемой цитаты.

Сначала добавлены268 входов контекста:41 первого этапа (40 блоков и абзац83),135 дополнительных блоков и92 строчных абзаца в83 файлах. Независимое извлечение обнаружило, что143 самостоятельных промпта всё ещё отделены от общего входа пустой строкой или другим блоком. В них дополнительно помещён короткий полный вход непосредственно внутрь передаваемого текста. Это исправление границы копирования в том же наборе185 файлов, а не143 новых опыта. Все прежние задания сохранены; число вставок не называется числом самостоятельных промптов или аудитов.

Вход связывает точный путь своей полной карточки,120§1, роли40§2, проверки42 и приоритет текущей авторизации. Нижние списки навыков дополнительные. Старый императив «реализуй» не разрешает запуск при исследовательском поручении. Полный перечень185 файлов и SHA первых трёх этапов — в [карте покрытия](codex-129-executor-prompt-coverage-2026-11-02.md); независимая проверка actual-блоков и последних143 вставок — в [пакете проверки](reviews/2026-11-02-executor-context-followup.md).

Итоговое отдельное извлечение исследователем подтвердило268 самостоятельных фрагментов:143 исправленных на границе копирования,92 строчных и33 ранее защищённых (18 цитат,14 fenced blocks, абзац83). Во всех извлечённых текстах присутствуют пять обязательных полей. Удаление или подмена поля на документной копии не принимается проверяющим. Это проверка конкретного передаваемого текста, а не доказательство, что будущая модель прочитает и исполнит его правильно.

В11 местах устранён прямой конфликт «не перечитывать уже прочитанное»: BPE-REALIZED-MERGE-COUNT-01, BPE-WEIGHTED-CHUNKS-01, DPO-TOKENIZER-SNAPSHOT-01, METHOD-KNN-TARGET-SCOPE-01, ROPE-TRAIN-TABLE-01, E-DPO-SFT-ORDER, E-DPO-VALID-LENGTH, E-SFT-MOMENT-CONTINUITY, E-SFT-RESPONSE-MASK, перенос49 и отдельный BPE-REALIZED-MERGE-COUNT-01-executor. Правило теперь различает новое задание/аудит, перед которым полное чтение обязательно, и ожидание/оформление внутри того же задания. Существующее ограничение не перечитывать весь стек для каждого плеча одного опыта не удаляется.

[40](../experimets/40-effective-executor-context-2026-11-02.md) теперь возлагает заполнение конкретного назначения на автора карточки; исполнитель проверяет источники, но не выбирает недостающую модель, набор, потребителя или бюджет. В[42](../experimets/codex-42-postmerge-executor-checks-2026-11-02.md) добавлен PM-C10: после извлечения копируемого блока должны сохраниться все точные адреса и границы; удаление входа/подмена карточки — отрицательный документный пример. Это не новый эксперимент обучения и не доказательство поведения будущей модели.

## Адресная приёмка и границы

В собственном новом [проекте переносаP108](126-p108-predicate-carrier-transfer-2026-11-02.md) независимое чтение обнаружило два недостающих условия. Они исправлены:
- M06 из127 — явная предпосылка использования measurement PASS, с saved-only required-set/finalizer, полным бюджетом и NOT_OBSERVED/HOLD при отсутствии consumed input. Новый Train для ремонта отчёта не назначается.
- Независимый frozen expected передаётся оценщику, но не learner/consumer. Имя порчи не служит оценщику подсказкой. Прежняя фраза «эталон оценщику не передаётся» была неверной.
-40 завершённых наблюдений не названы40 Execute/Search: в них4cold; оба физических процесса дают64 наблюдения,2Train/16 предъявлений.42 Execute и41 Search имеют разные основания; последний счёт — вывод из исходника.
- Тема «оркестрация» отделена от профиля D07; перестановка входного множества и порядок выходных Steps остаются разными контрактами.

Дополнительно исправлены три карточки вне32 адресов прошлого postmerge-усиления. Родитель прочитал live diff этих вставок; это проверка документов, не запуск опытов:

| Карточка | Плохой пример | Уточнённая положительная/отрицательная проверка | Граница |
|---|---|---|---|
| [P17](../experimets/cards/P17-tokenizer-refit.md) | Новый generation при старом vector и прежних count/width | Сверять actual vector каждой личности с сохранённым перекодированием; старая отличающаяся координатная запись при прежнем размере должна нарушить comparator | Saved-only, без нового Vectorize/Train/Search; при отсутствии данных NOT_OBSERVED |
| [P19](../experimets/cards/P19-mutable-output.md) | Expected делит изменяемый DTO/list с actual | Замороженные route/ordered Steps; замена первого шага при том же количестве обнаруживается | Нативную alias-защиту доказывает назначенная PRIMARY, а не калибровка JSON |
| [P59](../experimets/cards/P59-correction-visibility.md) | Re-export target успешен, но отвечает writer/третий экземпляр | Ответ и invocation принадлежат imported target; подмена reader либо ответа при сохранном re-export обнаруживается | Без нового Flow/Train; реальный broken-reader остаётся M03c, не дописан к старому опыту |

Подробные основания, уже достаточные соседние карточки и независимый разбор — в[пакете проверки](reviews/2026-11-02-executor-context-followup.md).

**Согласование P109:** [карточка](../experimets/cards/P109-gold-validity.md) и Q-индекс теперь показывают уже сохранённый результат126c093. Независимый пересчёт16 наблюдений сохранил выигрыш вероятности TR/E1, но выявляет E2 −21,912% и численные ответы0/3→0/3. Это не новый кандидат переноса.130 уникальных Q теперь содержат109 записанных и21 спроектированную позицию. Появившиеся внешние материалыP110 не аттестованы этой работой и не разрешают новый запуск.

## Что проверено, а что не заявляется

Структурный проход277 файлов каталога cards подтвердил уже имеющиеся ссылки120 и правило STOP/NEXT. Это **не** научная проверка277 экспериментов.25 отдельных executor-файлов уже содержат120; один из них изменён из-за конфликтующей фразы, а не отсутствия ссылки. Для этих входов найден дополнительный разрыв:120 связывал роли40 и проверку41, но не последующие проверки42. В [120](120-executor-context-and-test-repair-contract-2026-11-02.md) добавлено обязательное продолжение42 с ограничением применимости своим PRIMARY и без наследования чужих fixtures/вызовов/бюджетов. Это одна центральная правка, не25 новых проверок. Не назначались повторные научные запуски или новые Q.

Обратная документная проверка сравнивает SHA256 после удаления только новых вставок с зафиксированными bytes до соответствующего этапа. Первый этап:41/41; второй:135/135 после отдельного учёта согласованной параллельной фразы P59 о NOT_DISCRIMINATING; третий:83/83 файлов с92 строчными абзацами. Эти проверки выполнены до заключительной коррекции границы копирования. Последние143 вставки отдельно проверены исследователем обратным сопоставлением и чтением фактически передаваемого блока; результат и границы приведены в его пакете. Окончания строк сохранены. Это проверки сохранности документов, а не число независимых аудитов. Рецепты PRIMARY, числовые бюджеты, команды и исторические raw операцией добавления входа не менялись. Научный смысл адресных условий отдельно проверялся для P17/P19/P59 и проекта P108; вся серия185 не объявляется научно аттестованной.

Родитель читал целевые источники и разделы; полного свежего прочтения всего большого стека родителем здесь не заявляется. Исследователь `/root/post107_card_context` сообщил о полном свежем чтении local/canonical AGENTS, CLAUDE,12 основных и5 ролевых навыков с восстановлением усечений до содержательной проверки; точный охват и результат его проверки — в отдельном пакете. Эти два уровня чтения не смешиваются.

Итоговая документная сверка родителя охватила196 назначенных файлов без review исследователя: ошибок UTF-8 и маркеров незавершённого слияния нет;283 локальные ссылки в основных итоговых артефактах существуют. У P76 символ U+FFFD — намеренный пример UTF-8, не испорченная кодировка. Документный `git diff --check` с разовым `core.whitespace=blank-at-eol,blank-at-eof,space-before-tab,cr-at-eol` завершился exit0; настройка Git не записывалась. Без `cr-at-eol` две изменённые строки CRLF-файла N61 считались trailing whitespace; формат файла сохранён. Исследователь отдельно проверил200 ссылок своего пакета без пропусков. Эти числа относятся к разным пересекающимся поверхностям и не складываются как независимые доказательства.

Продуктовые тесты, сборки, обучение, inference и replay не запускались. Git commit/push/merge в этой работе не выполнялись; наличие ранее опубликованного43afaefe не публикует новые рабочие правки. Planning README уже находился в чужом незавершённом слиянии и в этой работе не использовался для подтверждения публикации.

## Первый этап:41 файл; полный охват185 — в отдельной карте

SHA в последней колонке — исходные bytes первого этапа перед новой вставкой; проверка обратимости сравнивает с ними, а не только ищет маркер.41строка ниже — первоначальная группа файлов, не итоговый охват и не число запусков. [Полная карта185 файлов](codex-129-executor-prompt-coverage-2026-11-02.md) включает последующие блоки и строчные промпты.

| Файл | Тип входа | Исходный SHA256 |
|---|---|---|
| [AST-SIB-CACHE-01.md](../experimets/cards/AST-SIB-CACHE-01.md) | Эксперимент | 77c0baa81add6e2564152fb7c6315304e543c07b14e270724d75ceda0f23a5cf |
| [E-DPO-SFT-ORDER.md](../experimets/cards/E-DPO-SFT-ORDER.md) | Эксперимент | ef467c6bbef8301264b96c4adb7ef0da50adc55daba4c77edc3698e3fe56bb91 |
| [E-DPO-VALID-LENGTH.md](../experimets/cards/E-DPO-VALID-LENGTH.md) | Эксперимент | c522bc0a16d53b1e38e853e85100657af3f026d65d2bb28404f8f375c1af1fdf |
| [E-SFT-MOMENT-CONTINUITY.md](../experimets/cards/E-SFT-MOMENT-CONTINUITY.md) | Эксперимент | 659e265e332c9eec53be124d62cab8a48356e914a6130ee228d4383008f0d100 |
| [E-SFT-RESPONSE-MASK.md](../experimets/cards/E-SFT-RESPONSE-MASK.md) | Эксперимент | 70bae349f992f66513c103b13ec6ca5f3870e4cfa8efbf9d0933e3c738f14400 |
| [FROZEN-BROADCAST-MAP-01.md](../experimets/cards/FROZEN-BROADCAST-MAP-01.md) | Эксперимент | f800567c246a8c1f927919c68dca9d57196ebbefd1d349af7079690920a131a2 |
| [GRAD-ACCUM-COPY-01.md](../experimets/cards/GRAD-ACCUM-COPY-01.md) | Эксперимент | 3730b6f0cff24e1b5565ebbfc1cca7906af8eda60e499d82318a03fbaa9037c5 |
| [N-KU-01-do-loop-label.md](../experimets/cards/N-KU-01-do-loop-label.md) | Эксперимент | be0ffa9abdb5eb557bc16e643cda30011f5c4531772481f91ce3fc45082854fa |
| [N60-e1-verifier-admission.md](../experimets/cards/N60-e1-verifier-admission.md) | Эксперимент | d370d5bb6fe511d83deae5b87b078e39b6a682062eee03e25f076db6d772c092 |
| [ROPE-TRAIN-TABLE-01.md](../experimets/cards/ROPE-TRAIN-TABLE-01.md) | Эксперимент | 97494d09a3e2239b2468aa6ce099e8d038513da95955862399389aad38ce450a |
| [SILU-SIGMOID-REUSE-01.md](../experimets/cards/SILU-SIGMOID-REUSE-01.md) | Эксперимент | f508b86628d905a71469be7e93a82cbfc2e18e4c815baa80a33b01be17fff062 |
| [WIKI-FEATURE-KEYS-01.md](../experimets/cards/WIKI-FEATURE-KEYS-01.md) | Эксперимент | dad4d46d7e92c9c4e14872a3a7b2fdb7f9b202d0a8354a6eab57af2e3ba9fd2a |
| [WIKI-FEATURES-ONCE-01.md](../experimets/cards/WIKI-FEATURES-ONCE-01.md) | Эксперимент | 0c3c23268322437829d7ea4a784135b63075e48db4a5d46773fbf5d659ff4f27 |
| [03-test-and-execution-contract.md](03-test-and-execution-contract.md) | Перенос / договор | bf2b5cdf029344d6a699fbd40e1d5abe5e5dd5e16fad2c19d3d81e6e87e9db13 |
| [114-post88-addressed-repair-plan-2026-11-01.md](114-post88-addressed-repair-plan-2026-11-01.md) | Перенос / договор | 7150e2fa1b891ff051fc06c3a42aa20318633576eac9b44fc4c1c98b4a4a02be |
| [118-post101-addressed-repair-plan-2026-11-02.md](118-post101-addressed-repair-plan-2026-11-02.md) | Перенос / договор | c754e1f053295441e3465d090ba9b3b13db9d2fb987c2682d4c87ada8a93c7f1 |
| [12-bpe-no-match-copy.md](12-bpe-no-match-copy.md) | Перенос / договор | 4717d80cb8b8a19872d880715b18f288469ef9a7001095d5ac1cbac482b15788 |
| [13-dpo-reference-cache.md](13-dpo-reference-cache.md) | Перенос / договор | 8d01abe9543cdd5d16964144ad998e4f5ad64f8331050930012cc13f0ebb71c0 |
| [16-replay-domain-retention.md](16-replay-domain-retention.md) | Перенос / договор | fdbeb3fe6d3ddc26c7466a7ae7090eedef3b45a6d82df1fed544449ab7f278d8 |
| [18-contextual-dictionary-learning-coverage.md](18-contextual-dictionary-learning-coverage.md) | Перенос / договор | d490485c12cc84129d23615998fbc5f08fb5f63cc5c8421e27006ad6cc5fcc7a |
| [27-predicate-dataset-router-learning-coverage.md](27-predicate-dataset-router-learning-coverage.md) | Перенос / договор | 5806d911409f8f9ebc6cf2fa2bd0bbf36d8ba29c00fe036bc191e3ffeff00763 |
| [29-matmul-right-gradient-locality.md](29-matmul-right-gradient-locality.md) | Перенос / договор | f1b21bff48b515624c91ab2f93692b19ac2a32144db4082e1243f88a40f9e248 |
| [30-broadcast-coordinate-allocation.md](30-broadcast-coordinate-allocation.md) | Перенос / договор | 19a91cc7bcde4eff657925b5c5694adf47c2c797c46227b2b0756a17ce81b840 |
| [31-bpe-weighted-chunks.md](31-bpe-weighted-chunks.md) | Перенос / договор | 47d8a66351abeb171bc570f547f775a3f752328da1b2dc1ab332f31eb8b01791 |
| [34-pos-bigram-learning-coverage.md](34-pos-bigram-learning-coverage.md) | Перенос / договор | cfec667b34fa5c0f276450287f192b81ad478601ba5dede6511434e3dfb7d4bc |
| [36-dpo-scored-row-head.md](36-dpo-scored-row-head.md) | Перенос / договор | 26a5618b9fc6c1fc49e5834840e6965694d4cd86cf560ea203f25a6ff34041c6 |
| [37-structural-knn-learning-coverage.md](37-structural-knn-learning-coverage.md) | Перенос / договор | b7b3e619af803e08e4563ed3ffd1e217dfb9efe53eb55a763def45ffd2568ee4 |
| [38-structural-knn-sibling-cache.md](38-structural-knn-sibling-cache.md) | Перенос / договор | dbcd61f5ad47656594edbcfeab8fde18ca49fe20c7e4c719046487452b70707f |
| [39-null-guard-training-label.md](39-null-guard-training-label.md) | Перенос / договор | 3c6fdc5460359a7b454f96dd2c88025bccd8bf98813faec775c41079780a101b |
| [40-bpe-realized-merge-objective.md](40-bpe-realized-merge-objective.md) | Перенос / договор | e5501f93f8ad7eb8e749784aec4697b906e035e377adffe0032da7ca5978c274 |
| [42-frozen-broadcast-map.md](42-frozen-broadcast-map.md) | Перенос / договор | e374ed4ce110e689408190a93d1ea511881721dbad1784b9f80acd3aeacde0c3 |
| [44-rms-row-reduce.md](44-rms-row-reduce.md) | Перенос / договор | e342a34d2c23f53ccd00535d4d5b8a73c6e20ffaec205e939ec7551d99f2e8db |
| [46-training-json-options-reuse.md](46-training-json-options-reuse.md) | Перенос / договор | ae14d843a25b68bae76edb7e240d57e7f4bd8433775bbecf99be4796e5cb01ef |
| [47-silu-sigmoid-reuse.md](47-silu-sigmoid-reuse.md) | Перенос / договор | 4357b2a99b9cec828b1a1d27b4e388dd62fb4ca196246fa2805d488e1deda258 |
| [48-learned-route-body-identity.md](48-learned-route-body-identity.md) | Перенос / договор | e722a96aef9ce303ef1aa7b9e05061d8fb1d62b68c3db0f625381a7c0dfc20a2 |
| [49-router-learned-key-normalization.md](49-router-learned-key-normalization.md) | Перенос / договор | 2dc4d7beb265f692ddbe53b7f4b4c02c4eed39d81239cc7d3a01e8587dd23167 |
| [52-translation-training-identity-index.md](52-translation-training-identity-index.md) | Перенос / договор | 8a2bdc03115bcf4620a937abd6f6a189ebc0de5aa21851fadd7c7bbe38891d74 |
| [53-hierarchical-local-history.md](53-hierarchical-local-history.md) | Перенос / договор | 6b5bec7d5af7584628657e5e10571e81c814da3991e9c9561dd255b1af527a4a |
| [62-supervised-framing-and-learning-readout.md](62-supervised-framing-and-learning-readout.md) | Перенос / договор | 3c945542d20512b85c56f9f4972fda3900049d4889afdbb9e1a03198a093ae94 |
| [83-learned-bpe-boundary-transfer-addendum.md](83-learned-bpe-boundary-transfer-addendum.md) | Перенос / договор | b8a6c9b36274dd4bb6736ec9397fea0738d639c4618568c52a2b8fade5b1e885 |
| [codex-127-postmerge-addressed-repair-plan-2026-11-02.md](codex-127-postmerge-addressed-repair-plan-2026-11-02.md) | Перенос / договор | a2eec39f981d0728b0c9deb0fd4fbf94e1ea3dd773bd102fa1391784900aad77 |

Дополнительные адресные редакции конфликтов чтения: [BPE card](../experimets/cards/BPE-REALIZED-MERGE-COUNT-01.md), [tokenizer card](../experimets/cards/DPO-TOKENIZER-SNAPSHOT-01.md), [MethodKNN card](../experimets/cards/METHOD-KNN-TARGET-SCOPE-01.md), [ROPE card](../experimets/cards/ROPE-TRAIN-TABLE-01.md), [BPE executor](../experimets/prompts/BPE-REALIZED-MERGE-COUNT-01-executor.md). ROPE уже входит в41строку: эти множества не складываются как независимые файлы.

