# Ограниченная проверка сводных метрик root

**Дата:** 2026-10-20. **Режим:** read-only review уже сохранённых документов и JSON; новое обучение, генерации, сборки и численные прогоны — 0.

## Проверенный scope

Прочитаны корневые `README.md`, `02-retained-results-and-candidates.md` и релевантные записи `evidence/retained-reanalysis.json` этого пакета. Проверка сопоставляла **фактические числа и область выводов**, а не стиль. Проверяющий не изменял root-файлы параллельно с владельцем.

Это второй уровень проверки: исходным материалом численного сопоставления был готовый JSON пересчёта, а не независимый новый обход всех archived raw, gold, harness и score файлов. Полный аудит их сбора, исполнения старых native programs, всех 137 карточек и полного Git HEAD здесь не утверждается.

## Итог сопоставления

| Область | Что сверено | Вывод |
|---|---|---|
| P125 | A0/A1=0/24; B0→B1=0/24→19/24; C1=13/24; wrong accepted B3/C9; exact8/near3/composition8/edge0 | Числа и ограничение памяти/композиции совпадают с JSON. Family-disjoint neural generalization не заявлена. |
| Translation ingress | A1=11/24, B1=19/24; answered12→22; wrong accepted1→3; precision11/12→19/22 | Выигрыш и ухудшение показаны правильно. Before/after обозначают разные trained arms, а не одну хронологическую модель. |
| SFT8 | Mean online pre-update loss7.0169490625→6.9877417; 7 ниже/1 равен/0 выше; changed tensors54/55→55/55; translation0/24; время/CPU/peak/allocations | Числа совпадают. Прямо указаны online pre-update scope и все55 trainable tensors; это не frozen-base RP01 и не held-out loss. |
| DPO8 | Mean loss0.69314783875→0.693147945; 3 ниже/3 выше/2 равны; time36113.99→34532.0568ms;16 changed tensors, base/ref unchanged; saved derivative pass126/162→162/162 | Числа совпадают. Legacy fractional mask, saved derivative diagnostics и actual8Step не смешаны с exact binary RP01. |
| N-KU | A0=0/4,A1=1/4,B0=0/4,B1=3/4; ответы A=ADD_LOOP/B=ADD_METHOD; main/BA time reversal | Binary NOT_ADD_LOOP scope и утрата нужного ADD_LOOP на KQ4 отражены; постоянный ADD_METHOD не объявлен правильным структурным советом. |
| P25 restore | Canonical equality,24 mismatches0,correct19,answered22,acceptedWrong3,abstentions2 | Восстановление установлено для этой memory; новые знания/LoRA optimizer recovery не заявлены. Нужна точность формулировки ниже. |

## Единственное замечание по доказательству

Фразы в README «cold restore сохранил все24ответа» и в таблице02 «24/24сохранённыхответов» могут быть прочитаны как24данных ответа. По JSON правильная формулировка:

**«Сохранены24/24исхода:22ответа и2отказа; качество19/24и3ошибочныхпринятия не изменились».**

Отказы относятся к cc02 и cc05. Замечание передано владельцу root; он подтвердил замену формулировки. Это уточнение области сохранности, а не пересчёт или новый learning result. Здесь исправление root не выполнялось и повторный readback root после изменения не заявляется.

Известные владельцу ошибочные source paths `03.Transformer/Embedding.cs` и `11.DPO/Training/DpoTrainer.cs` также оставлены его правке на `00.TensorCore/Layers/Embedding.cs` и `11.DPO/DpoTrainer.cs`; нового исследования механизма ради этого review не проводилось.

**Итог:** содержательные численные ошибки и необоснованное приписывание RP01 не обнаружены в проверенном scope; одно уточнение restore-outcomes передано владельцу. Это bounded review, не универсальная сертификация пакета и не разрешение на повторный эксперимент.

