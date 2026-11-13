# 81. Полнота усиления карточек после KNN repairs и P03–P05

Основания — [79](79-followup-transfers-and-learning-audit-2026-10-27.md), план — [80](80-followup-repair-plan-2026-10-27.md). Изменены только документы; runtime не выполнялся.

## Что исправлено и что означает число карточек

Проверка общего ложного фрагмента нашла21 PRIMARY-карточку. Дополнительный поиск вариантов записи выявил ещё P122: итого **22 карточки с исправленными native DPO defaults**. P01 дополнительно уточнён по границе SFT→DPO configuration/optimizer state: это23-й файл, но не ещё одно совпадение общего фрагмента. SFT-only P08/P09/P75/P76/P81/P99/P100 сохраняют свои настройки; слово DPO в запрете исполнения не превращает их в DPO-постановки.

По новым reader/qualification/closure рискам адресно усилены **8 owner-файлов для9 stable ID** и общие03/63/69. Остальные53 ID охвачены общим контрактом03 и ранее адресными требованиями78; неприменимые им EOS/нейронные optimizer параметры не вписывались искусственно. Это карта применимости всех62 ID, не62 независимых исходниковых аудита. P03/P04/P05 получили актуальные пояснения результата; старые данные сохранены.

Входы00-README,00-execution-index и P-SERIES-CONTINUE согласованы на Q008 по прочитанному3c4c3c771e5; P05 имеет отдельный учебный ремонт. В docs/TESTING.md исправлено живое точное имя census на StaysAtSeven. Это правка selector в документации, не выполненный тест.

## Паспортные исправления

| Карточка | Изменение |
|---|---|
| [P02-warm-base.md](../experimets/cards/P02-warm-base.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P03-historical-score-lineage.md](../experimets/cards/P03-historical-score-lineage.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P04-preference-contrast.md](../experimets/cards/P04-preference-contrast.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P05-response-length-objective.md](../experimets/cards/P05-response-length-objective.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P06-teaching-vector-adamw.md](../experimets/cards/P06-teaching-vector-adamw.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P101-learning-budget.md](../experimets/cards/P101-learning-budget.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P105-frozen-reference.md](../experimets/cards/P105-frozen-reference.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P109-gold-validity.md](../experimets/cards/P109-gold-validity.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P11-autograd-reachability.md](../experimets/cards/P11-autograd-reachability.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P114-zero-weight-pair.md](../experimets/cards/P114-zero-weight-pair.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P119-learning-dynamics.md](../experimets/cards/P119-learning-dynamics.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P12-optimizer-boundaries.md](../experimets/cards/P12-optimizer-boundaries.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P121-project-adapter-selection.md](../experimets/cards/P121-project-adapter-selection.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P13-finite-ingress.md](../experimets/cards/P13-finite-ingress.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P15-retention-replay.md](../experimets/cards/P15-retention-replay.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P24-replay-selection.md](../experimets/cards/P24-replay-selection.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P68-pretrain-pipeline.md](../experimets/cards/P68-pretrain-pipeline.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P78-loss-plateau.md](../experimets/cards/P78-loss-plateau.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P86-fractional-response-mask.md](../experimets/cards/P86-fractional-response-mask.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P87-mixed-objectives.md](../experimets/cards/P87-mixed-objectives.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P89-density-stop.md](../experimets/cards/P89-density-stop.md) | Native DPO decay.01/no clipping; отдельные SFT/абляционные назначения сохранены |
| [P122](../experimets/cards/P122-root-channel-unicode-digits-operators.md) | Исправлена ещё одна форма decay0/clip1 в DPO-бюджете; counts и фактор сохранены |
| [P01](../experimets/cards/P01-trainable-recipient.md) | Continuation moments/step не означает наследование SFT hyperparameters; не повторять800 SFT из-за отказа DPO |

## Применимость по всему реестру

| ID / владелец | Новое требование и способ применения |
|---|---|
| [FIX-KDP](74-knn-domain-constructor-transfer.md) | Адресный блок audit-79-followup в owner + общий03; точный механизм и граница указаны в самой карточке |
| [E2E-KDR](74-knn-domain-constructor-transfer.md) | Адресный блок audit-79-followup в owner + общий03; точный механизм и граница указаны в самой карточке |
| [FIX-MC](21-sft-optimizer-continuity.md) | Адресный блок audit-79-followup в owner + общий03; точный механизм и граница указаны в самой карточке |
| [FIX-IL](../fixes-based-on-experimentsperiments/55-supervised-ingress-length.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-SC](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-SEP](62-supervised-framing-and-learning-readout.md) | Адресный блок audit-79-followup в owner + общий03; точный механизм и граница указаны в самой карточке |
| [FIX-KHE](26-method-knn-training-admission.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-KS](35-method-knn-target-scope.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-FSR](../fixes-based-on-experimentsperiments/51-file-label-and-pos-reader.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-IM](48-learned-route-body-identity.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-KR](49-router-learned-key-normalization.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-HCL](53-hierarchical-local-history.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-SVD](../fixes-based-on-experimentsperiments/53-hierarchical-history-and-directed-svd.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-PD](22-dpo-valid-length.md) | Адресный блок audit-79-followup в owner + общий03; точный механизм и граница указаны в самой карточке |
| [FIX-LC](13-dpo-reference-cache.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-TS](28-dpo-tokenizer-snapshot.md) | Адресный блок audit-79-followup в owner + общий03; точный механизм и граница указаны в самой карточке |
| [FIX-FBC](54-frozen-base-check-scope.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-OSR](58-optimizer-step-metadata-read.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-BWS](60-backward-softmax-scratch.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-IGC](61-initial-gradient-owned-copy.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-LFT](55-frozen-linear-transpose-cache.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-AGC](41-grad-accum-copy.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-FBM](42-frozen-broadcast-map.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-CS](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-AC](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-RRD](44-rms-row-reduce.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-MB](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-ATS](45-attention-transpose-strides.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-RPE](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-SSR](47-silu-sigmoid-reuse.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-SEXP](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-GB](29-matmul-right-gradient-locality.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-NH](36-dpo-scored-row-head.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-RMS](30-broadcast-coordinate-allocation.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-BPE](12-bpe-no-match-copy.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-BW](31-bpe-weighted-chunks.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-BR](40-bpe-realized-merge-objective.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-KHC](24-file-action-shared-parse.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-AST](38-structural-knn-sibling-cache.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-HNQ](50-hnsw-training-query-norm.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-PQ](51-pos-query-bigram-reuse.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-JO](46-training-json-options-reuse.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-IDX](52-translation-training-identity-index.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-N60](11-translation-verifier-admission.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-VA](43-verifier-apostrophe-learning.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-NKU](10-nku-do-while-label.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-NG](39-null-guard-training-label.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-RET](15-prompt-flow-retention.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-WC](33-wiki-training-feature-reuse.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-WK](32-wiki-feature-key-isolation.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-WT](14-wiki-routing-taxonomy.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-E2E](20-prompt-domain-e2e.md) | Адресный блок audit-79-followup в owner + общий03; точный механизм и граница указаны в самой карточке |
| [FIX-N63C](17-dpo-sft-correction-collision.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [TEST-COLLISION-REPAIR](59-collision-repair-transfer.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [FIX-N63R](16-replay-domain-retention.md) | Адресный блок audit-79-followup в owner + общий03; точный механизм и граница указаны в самой карточке |
| [FIX-MS](23-sft-response-mask.md) | Адресный блок audit-79-followup в owner + общий03; точный механизм и граница указаны в самой карточке |
| [TEST-POS](34-pos-bigram-learning-coverage.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [TEST-SK](37-structural-knn-learning-coverage.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [TEST-ORDER](25-dpo-sft-order-learning-coverage.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [TEST-TCTX](18-contextual-dictionary-learning-coverage.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [TEST-TMEM](19-translation-memory-coverage.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |
| [TEST-E22](27-predicate-dataset-router-learning-coverage.md) | Общий03: qualified learning/неизменный consumer/достоверный receipt/валидный selector, когда соответствующий путь участвует; собственные требования78 сохранены |

## Границы проверки

Здесь не утверждается, что во всех карточках найден один и тот же product defect. Подтверждённые ошибки и закрытия имеют первичные ссылки в79 и независимом review. Число строк — охват требований, не число запусков или независимых ревью.

Обратное чтение подтвердило все23 параметрические правки (22 исправления и уточнение P01), по одному новому адресному блоку в11 плановых файлах (8 owners+03/63/69),62 уникальных ID/56 владельцев в матрице и Q008 во всех трёх входах очереди. Проверены2245 локальных файловых целей ссылок в48 документах — отсутствующих нет. Это проверка существования файлов, не semantic proof всех исторических ссылок или будущих команд. Read-only git diff --check HEAD с учётом CRLF завершился кодом0. Тесты, сборки, inference и обучение не выполнялись.
