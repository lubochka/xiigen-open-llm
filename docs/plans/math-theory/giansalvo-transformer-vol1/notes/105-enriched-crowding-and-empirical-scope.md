---
title: "105. Расширенное crowding и границы empirical proof of mechanism"
source_pdf_pages: [360, 364]
book_pages: [324, 328]
batch: "008"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 105. Расширенное crowding и границы empirical proof of mechanism

[Индекс библиотеки](../README.md) · [Предыдущая карточка](104-sa-interventions-and-orientation-criterion.md) · [Следующая карточка](106-training-claims-and-chapter-eleven-entry.md)

## Предлагаемая диагностика избыточности

§10.14 объединяет три вида evidence:

$$
\text{signature crowding}+\text{S–A motor crowding}+\text{functional marginality}.
$$

В enriched coordinates предложены $\alpha_{\mathrm{SA}},\Gamma_s,\Gamma_a,\lVert G_s\rVert_F,\lVert G_a\rVert_F,\lVert d\Sigma_s\rVert,\lVert d\Sigma_a\rVert$. Идея полезна: головы с близкими attention signatures могут различаться gradient pressure и локальным откликом. [PDF 360, §10.14](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#section-10-14).

Но на этих страницах не заданы единая метрика, веса разномасштабных coordinates, нормировки gradients, probe distribution или threshold functional marginality. Следовательно, это **направление диагностики**, а не готовая теорема или реализованный алгоритм pruning. [Crowding observables](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#display-10-crowding-observables).

## Равенство norms не означает одинаковую динамику

Редакторское уточнение source фразы о «similar gradient pressure and signature velocity»: два matrices $G$ и $-G$ имеют одинаковую Frobenius norm, но противоположные действия. Аналогично скорости $v$ и $-v$ имеют равные нормы, хотя сигнатуры движутся в разные стороны. Поэтому близость перечисленных **скалярных** observables не устанавливает близости полных motors или velocity vectors. При задаче замены нужно либо сравнивать сами векторы в согласованных координатах, либо проверять функциональный эффект. [§10.14](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#section-10-14).

Также одинаковые attention maps не фиксируют values и output projection: вклад головы содержит $S XW_VW_O^{(h)}$. Для удаления важна дополнительная потеря функции в присутствии оставшихся голов, а не только близость descriptor. Связь с прежним signature crowding сохранена в [карточке 83](83-signature-crowding-and-chapter-eight-boundaries.md).

## Как автор ограничивает SARS v8 результат

В §§10.15–10.16 явно разрешены выводы об измеримости quantities, неодинаковом изменении heads, различии symmetric/skew scales и наблюдаемом локальном flip effect выбранной головы. Автор явно **не** заявляет competitive SARS classification, global necessity skew component, важность каждой high-alpha head или inevitability observed maps. [PDF 361–362, два списка выводов](../source/0361-0364-ch10-empirical-status-and-synthesis.md#pdf-0361).

Source заявляет, что notebook exports checkpoint trajectories и что tables/run archive находятся в пакете `sources/`. При текущем преобразовании доступны PDF и изображённые/напечатанные данные; внешние arrays и notebook не проверены. Поэтому в проектном отчёте корректная атрибуция — «автор сообщает», а не «мы воспроизвели» или «Xiigen подтвердил». [Archive statement PDF 362](../source/0361-0364-ch10-empirical-status-and-synthesis.md#pdf-0362).

## Полный diagnostic пока не измерен

Table 10.3 содержит attention/output deltas, но не signature drift, loss change или task-metric change, хотя Definition 10.19 допускает их. §10.12 и итоговый пункт 12 прямо откладывают canonical $\mu_\ell(t),\rho_{\ell,h}(t)$; §11.1 затем уточняет, что compact SARS experiments всё ещё не измеряют полный $\Sigma(A(t))$. Нельзя считать signature-side empirical validation завершённой по самому переходу к главе с названием Signature Orbits. [Table 10.3](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#table-10-3); [PDF 364](../source/0361-0364-ch10-empirical-status-and-synthesis.md#pdf-0364); [PDF 367](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#pdf-0367).

## Практический формат записи для проекта

Как редакторское предложение для будущей реализации, каждый head report полезно связывать с checkpoint, fixed inputs, formula IDs для всех balances/energies, полной intervention specification и отдельно measured/unmeasured observables. Для crowding следует сохранять scale conventions и направление derivative, для task marginality — выбранный контроль и uncertainty оценки. Это спецификация требуемого evidence; на данных данного PDF она не превращается в выполненный эксперимент. [Совместный diagnostic](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#definition-10-14); [intervention profile](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#definition-10-19).
