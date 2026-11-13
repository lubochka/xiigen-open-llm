# MICRO37 — короткие причинные проверки трёх новых KNN гипотез без 500-парного переобучения

**Дата:** 2026-10-16. **До запуска:** EXPERIMENT_DESIGN_READY; source-only JS shadow 3 сценария, native .NET NOT_RUN, independent CODEX/Astra review NOT_RUN. [Гипотезы N37](37-NEW-KNN-SIGNATURE-GUARD-MONOTONICITY-CONTAINMENT-AND-CASCADE-2026-10-16.md).

## Контракт экономного исполнения

Объекты: `TranslationKnnStore`, `ExecutorUnit` и `TranslationRoutingFlow`. Использовать **один минимальный тестовый проект/isolated executable**, без запуска Windows CI и без обучения 500 DPO; вся исходная память создаётся в тесте либо загружается из отдельного frozen user-authorized Unit checkpoint. Не менять production default, checkpoint, train labels, архитектуру нейронного RP01 или защищённый каталог. Сначала L0 (математика + native source branch), затем L1 (ordinary consumer), только затем L2 (новый урок и retention), если достигнут реальный эффект. Чтобы отличить retrieval feature от обучения, держать state frozen при A/B inference.

**Обязательные фиксированные параметры:** branch commit SHA, source case IDs, actual signatures full/prefix/structure, source/target/direction, store entry order и hash, caller route, k=5, source-law version, language guard, test seed при случайном порядке, tokenizer version, evaluation rubric. Для L1 каждый case должен записывать `route_entered?`, `signature_hit_level`, `source_overlap`, `pre_target_count`, `post_target_count`, `answer`, `correct-law`, `abstain`, `critical_wrong`. Никаких переписанных labels после чтения outputs.

## L0-A — изменение качества source gate только из-за добавления тега

Вызывать текущий `TranslationKnnStore.FindNearestCascade` дважды с новыми **раздельными store**; в каждый положить source «green vehicle» → target «неподходящий перевод», querySourceText=«blue widget». Установить full key сначала `SVO|NOUN|VERB` (POS-only), затем `SVO|NOUN|VERB|RULE_SYNTAX` на обеих сторонах; prefix/structure заведомо отсутствуют в store. Вход source не менять.

| Arm | full key | Source threshold (source code) | Ожидаемый count |
|---|---|---:|---:|
| A | `SVO|NOUN|VERB` | 0.35 | 0 |
| B | `SVO|NOUN|VERB|RULE_SYNTAX` | 0.0 | 1 |
| C negative | tag only on query (не в store) | exact key miss | 0 |

**Falsifier:** A=1 или B=0 при фактически тех же строках и k — проверить SourceLaw/подписи и скорректировать H37-A, не объявлять успех. Затем доказать, что production `SemanticTokenizerUnit` реально выпускает обе наблюдаемые разновидности ключа; если нет — `UNREACHABLE_IN_ORDINARY_ROUTE`. Быстрый domain pilot: 4 C# AST role-tags, 4 SQL conditional scope signatures, 4 EN→RU agreement/signature tags, из них половина opposite-law. P0 goal не рост количества retrieved, а снижение **wrong accepted** с сохранением lawful recall.

## L0-B — query containment не равен смысловой совместимости

Сохранить source «delete records after backup is verified by administrator», спрашивать «delete records». Выбрать POS-only full key (достижимый только при заранее проверенном signature constructor), prefix/structure не использовать. Для двух source sets текущие [L308–323](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationUnitContracts.cs#L308-L323) дают 2/2=1.0 и 2/6=.3333 симметрично.

- A — текущий source containment и неизменный k.
- B — экспериментальный **source-qualification veto**, отдельно от lexical ranking, который отличает «only after backup» от безусловного «delete»; ошибка доменной law recognition обязана давать UNKNOWN, не ложное Contradiction.
- C — symmetric-Jaccard-only shadow, **не** по умолчанию в продукте (проверяет чувствительность, может вредить recall).

На 18 source-disjoint примерах: 6 SQL conditional/transaction, 6 C# authorization/null guards, 6 EN→RU temporal/negation. Для каждого — два допустимых paraphrases и один противоправный qualifier mismatch. Считать correct/incorrect/abstain, false positives среди критических ограничений, lawful recall, retrieval depth, latency. Если B только увеличивает abstain, но не улучшает correct при адекватном coverage, `QUALITY_GAIN_NOT_ESTABLISHED`.

## L0-C — первый tier target-incompatible, второй законный, но недостижимый

Один Store:
- `AddPair(full="FULL", source="return status", target="Return status")`;
- `AddPair(prefix="PREFIX", source="return status", target="Вернуть статус")`;
- query signature triple (`FULL`, `PREFIX`, `STRUCTURE`) и querySourceText «return status».
- L0: `FindNearestCascade` выдаёт англоязычный full и не ищет prefix (ожидаемый список: только `Return status`).
- L1: создать `ExecutorUnit(TranslationDirection.EN_RU, store)`, `SemanticTokenSequence` с указанными signatures и source token words, никакого exact/phrase/word-map обучения; ordinary `Execute` должен проверить target script уже **после** cascade. Фактический выход зафиксировать, не подменять предположением.
- B — экспериментальный wrapper, который перебирает full, prefix, structure и применяет `IsTargetCompatibleForDirection` **до** решения принять tier. Он не меняет корпус; источниковые законы проверяются до k. Только wrapper, не production patch.

Контроль D: prefix имеет заведомо чужой язык — B обязан тоже отвергнуть/отказаться. Контроль E: full target законный — B обязан сохранить обычный ответ. Контроль F: более широкая подпись source-law contradiction — запрещённый случай не должен стать доступен при обходе.

**L1 доменный пилот:** EN→RU 12 новых источников, EN→HE/EN→ES 12, из нескольких signature families; фиксировать достигнуто ли реально KNN после exact-source/phrase-map/word-map. Primary delta — новые lawful correct without new critical wrong; не рапортовать успех, если route не достиг KNN. Если target-eligibility для будущих иных языков недоопределена, использовать `ELIGIBILITY_UNKNOWN` и независимую доменную проверку, не расширять предикат наугад.

## L2 — действительно ли это разница в обучении, а не только в retrieval?

Только если L1 улучшил **обычный** ответ:
1. **CODEX / Astra 6 UltraCode выбирает в человеческой роли** три целевых capability/law и три источниковых семейства на домен; независимо замораживает 12 train и 12 blind case IDs, ambiguity/forbidden labels и rubric, не подсматривая A/B. Новый DPO требуется лишь если имеющиеся законно принятые пары не покрывают урок.
2. Если нужен DPO, **разрешённая внешняя открытая китайская модель создаёт обе стороны chosen и rejected**; CODEX / Astra 6 UltraCode отдельно проверяет происхождение/допустимость, смысловую ошибку rejected, target и возможность обучения, а не выдаёт свои сгенерированные synthetic пары за китайские.
3. В A/B одна и та же `TranslationLearningBatch`, один и тот же начальный model/Unit state, идентичное число добавлений, только изменение retrieval policy из L1; сохранить post-train checkpoint и открыть в новом процессе. Сравнить исходные/новые законы и confidence before/after/after restore; делать отдельный no-train/shuffled-label negative control.
4. **CODEX / Astra 6 UltraCode независимо судит конечные ответы** на запечатанной панели, включая wrong accepted, abstention, regressions и границу domain. Оценка DPO-пар не заменяет оценку Student/Unit.

## Freeze / go-no-go

| Gate | Что зафиксировано | Вердикт |
|---|---|---|
| G0 source | current commit + raw method code + source-law scope | проверено по GitHub |
| G1 L0 native | три числовых/веточных результата совпали с source hypotheses | **NOT_RUN** |
| G2 ordinary route | новая ветка действительно достигается обычным вызывающим кодом | **NOT_RUN** |
| G3 quality | A/B на frozen law panel, seed parity, no regressions | **NOT_RUN** |
| G4 restore | answers parity warm vs reopened checkpoint | **NOT_RUN** |
| G5 CODEX Astra review | независимая защита выбора, DPO, adjudication | **NOT_RUN** |

Обязательная регистрация outcome: `SOURCE_MATH_REPRODUCED`, `NATIVE_BRANCH_CONFIRMED`, `ROUTE_NOT_REACHED`, `LAW_QUALITY_GAIN`, `QUALITY_REGRESSION`, `INCONCLUSIVE`, `UNKNOWN_LEGAL_ADMISSION`. Никакого «PASS learning» только за изменившееся число хитов. Время выполнения мерить, а не обещать секунды на CPU.

## Роль навыков

`human-readable-architecture-audit` — отдельно назвать recipient/consumer и реальное качество; `phase-planning` — L0→L1→L2; `plan-review` — frozen falsifier и несовместимость source/proxy/native; `generated-code-review` — проверить дифф и no hidden shortcuts; `experiment-design` — один фактор на arm, positive/negative controls; `prompt-patch-authoring` — точный ограниченный заказ исполнителю; `conversation-intent-and-self-reflection` — явно сообщить границы и не объявлять фиксы. См. [37-CODEX-ASTRA-IMPLEMENTATION-AND-REVIEW-PROMPT](37-CODEX-ASTRA-IMPLEMENTATION-AND-REVIEW-PROMPT-2026-10-16.md).
