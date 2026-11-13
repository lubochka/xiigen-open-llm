# Независимый статический review: ATTN и сводные выводы

Reviewer: `historical_knn`. Scope: ATTN-TRANSPOSE-STRIDES-01-CARD.md, ATTN-TRANSPOSE-STRIDES-01-EXECUTOR-PROMPT.md, 01-results-and-candidates.md, 02-learning-paths-and-gaps.md в `2026-10-20-learning-evidence-827a1acd`. Код сверялся с локальными прочитанными primary snapshots669; результаты — с сохранёнными raw и сводкой. **Build/run/tests/training не выполнялись.** Собственная JSON-карточка этого reviewer не оценивается как независимая.

## Вердикт

**PASS для документного статуса NOT_RUN, с двумя адресными уточнениями до передачи ATTN host на исполнение.** Зафиксированный ATTN diff соответствует заявленному одному фактору; выигрыш ещё не измерен. Числа allocation в 01 пересчитаны арифметически; потерянных регрессий и подмены локального ресурcного успеха обязательным accuracy gain не обнаружено. ATTN остаётся TODO_IMPLEMENT_EXPERIMENT_HOST, что честно указано.

## ATTN: что проверено

- Источник `TensorOps.Transpose`, L710–783: rank4 axes1↔2 проходит generic path, `FlatToIndices` создаёт int[4] наscalar; last-two optimization этот случай не покрывает. `CausalSelfAttention.Reshape4D/3D`, L323–352, действительно вызывают этот путь; backward L775–779 вызывает обратный transpose при RequiresGrad. Reachability не подменена голым benchmark.
- Формула source `((b*A+a)*C+c)*D+d` → target `((b*C+c)*A+a)*D+d` корректно описывает swap1↔2. Обратный swap использует ту же формулу на обращённых размерах. Copy order, Tensor graph и cadence4096 совпадают с исходной ветвью. Новые counters одинаковы в A/B и не выделяют массивы внутриscalar loop.
- В карточке 4native updates/arm (T1,T2,T1,T2), всего8. Диагностика4состояния×(2TRAIN+2EXAM)=16forward; дополнительные backward на readout запрещены. Studentconfig, seeds, LoRA/qv, optimizer, tokens, objective и данных разницы между плечами не создают.
- Base/LoRA/optimizer float bits и четырёх online losses достаточно предусмотрены для проверки exact optimization; реальный parameter write и неизменная база указаны отдельно от step counter.
- Статус не обещает готовый runner; исходный host и все36source paths заранее назначены. Новый опыт не выполнялся и не копирует frozen-weight cache из другого опыта.
- Обе карточка/prompt сохраняют metric success и regressions, отдельно causal attribution; русский report, default session model, sourcecopyscope и stop/next включены.

**Rank:** при первом сообщении root подозревалась неполная формула. В прочитанной publishing copy она уже исправлена: `1 + #greater + #equalWithLowerTokenId`; argmax tie-break по меньшему ID. Дальнейшей поправки rank не требуется. Повторный runtime ради этого не нужен.

### Адресное уточнение ATTN-1: readout accounting

В исходном шаблоне `work/attn-base/.../Program.cs::Observe` (L108–127) forward, вычисление NLL и `ToArray` находятся в одной функции. Новая карточка правильно требует метрики/копии за границами readout cost, но шаг «Observe(Train),Observe(Exam)» легко наследует старое измерительное окно.

Нужная конкретизация для автора будущего host: заранее подготовить input tensors/slots; внутри readout allocation window выполнить ровно4forward и сохранить ссылки на их Tensor-результаты; после закрытия counters вычислить NLL/rank/argmax и копировать float bits. Не оборачивать весь старый Observe в counter. Никаких новых forward, другой панели или увеличения бюджета. Это уточнение recipe, не новый factor.

### Адресное уточнение ATTN-2: завершение/переход

Root уже сообщил единый порядок827: ATTN35→JSON36→IDENTITY37→KN38. В ATTN card/prompt пока следующая карточка только определяется реестром, без literal NEXT. Добавить `NEXT=JSON-OPTIONS-REUSE-01` и ссылку на основную строку; не требуется новый запрос пользователю. Обычное завершение отчёта не отменяет немедленный переход по новой прямой команде.

## 01: числа и интерпретация результатов

Проверены совпадения SHA256 raw10файлов NEURAL/BPE/R1/R2 с `saved-results-recomputed.json` и allocated bytes основных NEURAL/BPE. По сохранённым A/B значениям независимо вычислены:

| Пара | Δallocated B−A | %A |
|---|---:|---:|
| frozen transpose | −1 507 392 | −11.0449126% |
| BPE code-doc | −2 777 920 | −71.1436375% |
| BPE EN→RU | −1 999 168 | −69.1078337% |
| R1 | −90 504 | −0.00240384% |
| R2 | −312 328 | −0.00829568% |
| E-OPT | −467 944 | −0.01242881% |

E-OPT arithmetic проверена по опубликованной в пакете source-indexed сводке; exact E-OPT raw binary/history этим reviewer не реконструировались. Отдельный review EOPT в пакете нельзя называть моим собственным воспроизведением. R2/EOPT provenance caveats в01 достаточны: historical wrong metadata, incomplete binary binding, total vs payload не замолчаны.

Сохранены ухудшения: нужный ADD_LOOP теряется в N-KU; frozen-transpose retainedcache иCPU растут; BPE code-docCPU растёт; R1/R2GC+1; R2/EOPTpeakрастёт. Time/CPU не приписаны однозначно фактору. Native0accuracy и отсутствие subject target readout не представлены как отсутствие updates. NLL улучшения в обеих arms обозначены полезным приобретением по этой оси, без преимущества B.

RP01 C#/ENRU summary поля сверены:50+50updates,16LoRA tensors, неизменнаябаза, положительный среднийmargin при0/3и0/4 freeanswers; ENRUначатотC50. Отдельно остаются ограничения summary-only и неразысканных nested checkpoint chains. P125/P25memory успех19/24 и сохранение24outputs не смешаны с neurallearning.

## 02: соответствие первичному коду

Проверены `TranslationStructuredBatchBuilder` L78–150: три отдельные строки с выбором tokenizerpattern, chosenExecutorOutput, verifierFAIL на rejectedcandidate; RULE/PHRASE/AGREEMENT действительно берутсяизpair.Rejected. Семантика chosen для verifier не перевёрнута. `TrainOnExample` соответствует памяти, а не negative-gradient DPO. Связь с обычным adapter проверяется по L124–139.

В `DpoTrainer` L832–910 и1036–1121 действительно противоположные chosen/rejected signs, response weights, next-token shift и SumTargets; общее заявление о якобы отсутствующих маске/знаке не повторено. LoRA/checkpointинформация корректно ограничена названнымcaller; отдельный audit иныхpathsнеподменён.

**Малая ссылка для правки:** во вступлении02 ссылка с подписью «три writers» ведёт только на Executor L404–455. Либо назвать её «Executor writer», либо дать ещё SemanticTokenizer L124–140 и QualityVerifier L1095–1145. Ниже в таблице правильные ссылки уже присутствуют; содержание не ошибочно, но labelссылкиширееёrange.

## Границы

Это статический review документов/кода и пересчёт сохранённых чисел, не независимое native воспроизведение. Нет доказательства actual allocationgain ATTN, успешной сборки нового host, переноса на все домены или полной причины слабого RP01. Пересмотр старых результатов или назначение повторов не требуется.

