# 160. Что усилено после Q011 и как проверен документный пакет

4 ноября 2026. Основание — [аудит158](codex-158-postq011-transfer-and-experiment-audit-2026-11-04.md), конкретный ремонт — [159](codex-159-postq011-addressed-repair-plan-2026-11-04.md). Исходные core `45bd4e371f511e29c053f8117d61f43365abb460`, planning `9cd53517311be504a89f0e04cdcea0afb011bf8c`. Изменения выполняют запрос на аудит и усиление планов; продуктовые исправления ещё не реализованы этим пакетом.

## 1. Адресное покрытие22 экспериментальных карточек

В каждой перечисленной карточке добавлен самостоятельный верхний блок POSTQ011: предметный факт, существующий владелец, сохраняемый положительный случай, отрицательный контроль и предел доказательства. Старое тело сохранено; нижние датированные выводы не переподписаны новым source. Это не повторная научная аттестация остальных109 карточек.

| Карточка | Причина усиления | Конкретная защита от повторения ошибки |
|---|---|---|
| [P02 — warm base](../experimets/cards/P02-warm-base.md) | Repair и continued training смешиваются; shape-valid import может быть неполон |27 original identities и24 известных updates; trusted parent keyset/step/RNG; NaN/missing-entry refusal; fresh state не выдаётся за parent |
| [P03 — lineage результата](../experimets/cards/P03-historical-score-lineage.md) | УлучшениеT1 скрывало потерянныйE2 и другой знаменатель loss |2/4→1/4, lostE2; одна предъявленнаяT1;21-position pre-update loss отдельно; summary/phase negatives; missing final bundle не реконструируется |
| [P04 — preference contrast](../experimets/cards/P04-preference-contrast.md) | Разные rejected сравнивались как общая accuracy; старый текст о binary repair устарел |8 original checkpoint;5 ухудшений NLL; общий gold/decoder; pair/phase binding; last step не final |
| [P05 — length objective](../experimets/cards/P05-response-length-objective.md) | Маска уже присутствует;400SFT ошибочно можно принять за квалификацию |30 восстановленных paths; actual response positions/denominator;21 панель0/6 UNQUALIFIED; неисполненный EXAM отделён от нулевого результата |
| [P06 — TeachingVector/AdamW](../experimets/cards/P06-teaching-vector-adamw.md) |9/9 внешнего reader не закрывают product import |NativeAccepted/FullStateAccepted и actual object раздельны; numerical guard и trusted completeness; original bundle и lawful sparse/fresh positives |
| [P07 — materialized tokens](../experimets/cards/P07-materialized-token-contrast.md) | BPE boundary ошибочно обобщается на ответное умение; printed deadline не действует |Actual ids/mask/scored positions и no-crossing controls; existing Validate сохранена; fake-child timeout без BPETrain |
| [P08 — answer/sampling](../experimets/cards/P08-answer-boundary.md) | Support/probability принимаются за gained answer или ускорение |T1 support2→1 при unchanged answer; actual token/seed/logits; четыре строки затрат и отдельное окно Generate |
| [P09 — cached forward](../experimets/cards/P09-cached-forward.md) | Empty-cache prefill расширяется на incremental decode; широкая стоимость становится per-call invariant |21 504/84 parity; single nonleading-logit mutation; distinct cache phase;22 928байт не invariant; принятыйR46 сохранён |
| [P10 — composed gradient](../experimets/cards/P10-composed-gradient.md) | Узкий checker не связывает всю objective chain |Honest12/12 и changed-logP scope-probe; raw/applied/fresh moments; missing trace остаётся missing; deadline проверяется без модели |
| [P11 — autograd reachability](../experimets/cards/P11-autograd-reachability.md) | Движение weights/moments ошибочно приравнивается к независимому data gradient |Objective/analytic/FD в одной точке; second moment consistency; shared S1 repair29-D3 без повторной обучающей fixture |
| [P12 — optimizer boundaries](../experimets/cards/P12-optimizer-boundaries.md) | LR/update guard не покрывает import state |Actual AdamW/DpoTrainer; numerical отказ до swap; полнота своего continuation; Step0/sparse проходят только свой разрешённый договор |
| [P18 — state identity](../experimets/cards/P18-state-identity.md) | Чужой guard/hash/count не подтверждает собственный KNN state |Actual vectors/IDs/encoder и reader;64d positive/63d typed refusal; полнота своего bundle. Нейронные optimizer/capability отсутствуют по механизму и не навязываются |
| [P86 — fractional mask](../experimets/cards/P86-fractional-response-mask.md) | Finite поля не связывают weighted loss с backward |Actual mask/denominator/reference; mutation веса только в одной ветви; сохранённые zero-mask/positive cases; native не подменяется проекцией |
| [P87 — mixed objectives](../experimets/cards/P87-mixed-objectives.md) | Ранний отказ не проверяет поздний rollback |Достижимость нужной фазы; homogeneous positive; incompatible second row; полное неизменное состояние вместо константного updates=0 |
| [P100 — topK ties](../experimets/cards/P100-topk-ties.md) | Мощность support смешивается с частотой, качеством и ценой |Exact IDs/probabilities/tie policy; no-tie positive; support/probability negatives; own sampler window |
| [P101 — learning budget](../experimets/cards/P101-learning-budget.md) | Recovery стирает прежний расход; readback fixture скрывает обучение |Donor/initial/recovery/qualification/diagnostic отдельно; NOT_RECONCILED вместо0; not-run vs zero-score; настоящий deadline |
| [P112 — family disjoint](../experimets/cards/P112-family-disjoint-evaluation.md) | Первый content match отвергает lawful ID; Dev/Test flags объединены |Два законных IDs в обоих порядках; typed overlap stage; независимая policy-матрица; expected не ослабляется до любого отказа |
| [P115 — statistics cache](../experimets/cards/P115-bm25-statistics-cache.md) | Null/empty Output теряются до правильного framing |Обычный legal producer и reuse; helper invalid Text недостаточен; existing cache и approved→cancelled сохранены |
| [P117 — direction memory](../experimets/cards/P117-translation-direction-memory.md) | Expected journal может скрыть дефект actual export |Actual serialized memory→restored consumer→whole answer; export mutation при прежнем journal; partial scientific result допустим |
| [P121 — project adapter](../experimets/cards/P121-project-adapter-selection.md) | Checkpoint hash/names не различают настоящие recipients |Same-bytes/different-object, issuer, reissued capability, constructor paths; lawful repeat/sharing/legacy |
| [P127 — ordered route](../experimets/cards/KNN-ORDER-PERMUTATION-01.md) | Label-only checks не связывают payload; другой ordinary consumer |Две honest панели68 rows; route/steps/edges/trace и внешняя authority; exact attempt; consumer остаётся105§5 |
| [P131 — negation](../experimets/cards/P131-negation-training-contrast.md) | Derived raw не привязан к native output; producer/verifier identity смешаны |128/128 saved bindings сохранены; exact native gate после archive migration; coherent mutation; absolute `/evidence` root |

Карточка P127 уже называется `KNN-ORDER-PERMUTATION-01.md`; новый дублирующий `P127-*.md` не создан. Эти22 карточки относятся к131 уникальной основной Q-строке; изменение их инструкций не меняет количество исполненных опытов.

## 2. Общие договоры и навигация

| Документ | Выполненная правка |
|---|---|
| [02 — learning state](02-learning-state-and-fixtures.md) | Product import vs external guard; numerical validity vs trusted completeness; совместимость sparse/fresh |
| [101 — persistent bundle](101-persistent-state-bundle-contract-2026-10-30.md) | Expected initialized keyset вне supplied input; finite/nonnegative-v; full applicable before/after |
| [03 — test/execution](03-test-and-execution-contract.md) | Причинные gates, actual consumer, описательный recount и реальный timeout; future runtime отдельно |
| [51 — evidence boundaries](../experimets/codex-51-postq011-evidence-boundaries-2026-11-04.md) | Новый краткий договор и самостоятельный prompt из8 пунктов, с предметным примеромP12 |
| [README переносов](README.md), [реестр](01-registry.md), [README опытов](../experimets/00-README.md), [индекс](../experimets/cards/00-execution-index.md) |11/131 QA,131/131 recorded, Q012 unassigned;92/3358/19/33 Git identities; старые counts явно исторические; queue без перестановки |

В planning обновлены README и четыре адресных документа — `02-DPO-DATA-OBJECTIVE-GRADIENT.md`, `03-LORA-TOKENIZATION-VECTOR-SPACE.md`, `06-CHECKPOINT-RESTORE-ADOPTION.md`, `26-INDEPENDENT-STOP-TARGET-AND-SAMPLER-2026-10-16.md`; добавлен `codex-2026-11-04-POSTQ011-TRANSFER-AND-EXPERIMENT-AUDIT.md`. Это6 planning файлов. Старые результаты planning остаются на своих источниках; новый пакет ссылается на core158/159/160/51 как на действующее дополнение.

## 3. Новые проверяемые материалы

Папка [postq011-2026-11-04](../../../../experiments-v2/_audits/postq011-2026-11-04/README.md) содержит:

- source/commit manifests и projection current Git trees для фиксированного universe3502;
- сохранённый вывод пяти Node checks, независимых Float32/objective/panel/resource пересчётов и43 новых original identities;
- независимый source audit product call chains и183 verified blobs;
- P127/P131 JSON recount со скриптом и source hashes;
- журнал проверки документов и точный перечень публикуемых файлов.

Эти Python-скрипты читают готовые JSON и вызывают указанные Node checkers; они не импортируют модель и не запускают C#/PowerShell. Значения `model_calls=0`, `native_calls=0`, `training_updates=0` относятся к этой работе. Описательный `transfer-saved-recount.py` следует оценивать по counts/mismatches/panel, а не одному exit0. LFS backing bytes эта среда заново не материализовала.

## 4. Review и достаточность проверки

Независимый исследователь проверил product source и nine-file усиление02/101/P06/P12/P112/P115/P121/P127/P131. Основной аудитор прочитал добавленные разделы, сверил source links и уточнил P12: lawful fresh/sparse не должны автоматически проходить специфический Q007 Step401 contract. По финальному review P18 адресован к собственным KNN vectors/IDs/encoder и query-dimension consumer; DPO optimizer/capability не навязаны чужой карточке. Анализ P127/P131 повторён основным аудитором; его JSON совпадает с результатом исследователя.

Исследователь отдельно прочитал158/159/51 и подтвердил product boundaries, owners, сохранение Q04 pause, Q02 PAIR и old positives. Две рекомендации внесены: (1) available saved objective chain отделена от несохранённой полной per-evaluation provenance; (2) descriptive recount exit0 отделён от integrity verdict. Runner deadline вывод проверен по current P0701/P1001 source. Конечный review охвата и автоматические проверки фиксируются в [журнале](../../../../experiments-v2/_audits/postq011-2026-11-04/codex-document-validation.json).

Документная проверка сверяет новые links/anchors, UTF-8 и whitespace, уникальность вставок, неизменность исходного тела редактируемых карточек,131 Q-строку и все121 work ID/строку реестра (114 базовых и7 поздних), отсутствие production/raw/binary изменений в публикуемом manifest. Научные данные и отрицательные результаты не заменены ради успешного статуса. Full audit всех114 product implementations либо новый native PASS не заявляются.

**Что остаётся открытым:** реализация и нативная приёмка адресных исправлений159 у их существующих владельцев;120 следующих QA-позиций по прямому назначению; exact payload/native readback конкретных ещё непригодных donors. Сам пакет завершает документационный аудит и усиление карточек, а не эти будущие работы.
