---
title: "184. Программный интерфейс: семантика диагностики, pruning и проверяемые вмешательства"
source_pdf_pages: [526, 529]
book_pages: [490, 493]
batch: "011"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 184. Программный интерфейс: семантика диагностики, pruning и проверяемые вмешательства

[Индекс](../README.md) · [Источник и редакционная политика](../SOURCE.md)

## Что процедуры уже определяют

[Procedures 14.1–14.3](../source/0527-0529-ch14-decision-tables-and-procedures.md#procedure-14-1) разделяют диагностику, выбор вмешательства и проверку modified block. Они требуют измерять marginality, coverage, crowding, orbit diversity и residual, защищать ценные directional heads, а pruning принимать только внутри validation bands. Это полезный операционный каркас; автор явно не выдаёт его за алгоритм глобальной оптимизации.

[Tables 14.5–14.6](../source/0527-0529-ch14-decision-tables-and-procedures.md#table-14-5) связывают симптомы с действиями. Низкая single-head marginality и высокий crowding означают кандидата на вмешательство; высокая marginality требует сохранить голову даже при crowding. Самый сильный защитный механизм — повторное измерение после совместного изменения модели, поскольку leave-one-out оценки относительно исходной сети не складываются автоматически.

## Attention matrix и feature-space motor не взаимозаменяемы

Источник разрешает «empirical attention or motor matrix» под одним $M_{\ell h}(\mathcal D)$. Но motor $W_QW_K^\top$ имеет размер $d\times d$, logits $XMX^\top$ и attention — $n\times n$; их S/A-части измеряют разные свойства. В частности, симметричный score не обязан давать симметричный row-softmax:

$$
L=\begin{pmatrix}0&0\\0&\log3\end{pmatrix}=L^\top,
\qquad\operatorname{softmax}_{\mathrm{row}}(L)
=\begin{pmatrix}1/2&1/2\\1/4&3/4\end{pmatrix}.
$$

У attention здесь ненулевой skew-сектор при нулевом skew logits. Поэтому один порог $\tau_A$ нельзя переносить между этими объектами без перекалибровки. Для неотрицательной attention matrix $P$ даже действует дополнительная граница

$$
\|P_a\|_F^2=\tfrac12(\|P\|_F^2-\langle P,P^\top\rangle_F)
\le\tfrac12\|P\|_F^2,
$$

так что squared-energy alpha из [§14.4.1](../source/0512-0515-ch14-sa-channels-and-cost-accounting.md#display-0512-squared-alpha) на таком $P$ не превосходит $1/2$ (с $\varepsilon>0$ — ещё меньше). На произвольном signed motor эта граница не действует. Для пакетной диагностики также нужно задать усреднение по примерам, маскам и длинам; entropy имеет обычный вероятностный смысл для attention, но не автоматически для signed motor.

## Семантика intervention scores и истории

Полная ablation, ослабление gate и first-order score в Procedure 14.1 — разные оценки. При стационарной гладкой loss первая производная может быть нулевой, а конечное удаление давать положительную loss delta. Хранить их под одним полем без указания метода нельзя. Orbit diversity требует checkpoint history: одной trained block и одного validation batch обычно недостаточно для восстановления training-time орбиты. Можно использовать proxy, но это другая диагностика.

В таблицах слово coverage иногда обозначает качество, которое должно расти, тогда как $\rho_\ell$ — residual distortion, которое должно уменьшаться. Условия `coverage < threshold` требуют явно выбрать шкалу. Диагностика FFN domination сама по себе не доказывает, что уменьшение FFN или увеличение attention улучшит loss.

## Gates, бюджет и критерий принятия

Строка [Table 14.6](../source/0527-0529-ch14-decision-tables-and-procedures.md#table-14-6) предлагает при большом residual «lower residual gate or add gate floor». Floor — нижняя граница — может защищать от отключения, но сам по себе не ограничивает чрезмерно большую инъекцию сверху. Кроме того, уменьшение отдельного gate не обязательно уменьшает норму **суммарного** residual из-за cancellation: $R_1=1,R_2=-1$ дают ноль при двух единичных gates и единицу после выключения второго. Нужна фактическая проверка, уже предусмотренная процедурой.

На [PDF 526](../source/0523-0526-ch14-design-recipe-and-static-interface.md#section-14-10) размеры объявлены фиксированными во время процедуры, но далее разрешены pruning, splitting и добавление слотов. Согласованное прочтение — отдельные операции model surgery с фиксацией новой архитектуры между проверками либо заранее резервируемые слоты; это стоит указать при реализации.

Procedure 14.3 не задаёт lexicographic порядок или scalar score для двух ranking criteria, обработку ties и случай, когда protected heads оставляют меньше $q$ кандидатов. «At most q» допускает меньшее удаление. Последовательное восстановление удалённых голов может вернуть исходную модель; при детерминированной проверке и допустимой исходной модели это даёт конечный fallback, но не гарантию достижения заданной compression rate. В этом пакете процедуры транскрибированы и разобраны; обучение, pruning и fine-tuning не запускались.
