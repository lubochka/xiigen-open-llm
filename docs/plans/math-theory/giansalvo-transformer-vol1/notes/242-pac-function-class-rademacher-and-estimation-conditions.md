---
title: "242. PAC-постановка: класс функций, Rademacher bound и фиксированность task operator"
source_pdf_pages: [625, 626]
book_pages: [589, 590]
batch: "013"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 242. PAC-постановка: класс функций, Rademacher bound и фиксированность task operator

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Нужен полностью определённый класс функций

В §16.9.1 заданы $k$ голов, ограниченный input norm, ограничение на skew motors $M_a$, NTK-optimal scale values и Lipschitz classifier. Proposition 16.33 утверждает Rademacher bound, пропорциональный $\rho_{\max}k/\sqrt m$. В доказательстве дополнительно появляется near-uniform softmax assumption, не включённое в саму короткую формулировку. [Класс функций](../source/0625-0627-ch16-pac-estimation-and-architecture-selection.md#section-16-9-1), [утверждение и proof](../source/0625-0627-ch16-pac-estimation-and-architecture-selection.md#proposition-16-33).

Для всего Transformer нужно определить глубину, residual paths, FFN, выходные проекции, нормировки, область classifier и loss. Норма одной кососимметричной части не ограничивает всю функцию. В частности, $M_a=0$ не означает нулевой attention output: равномерное attention может передавать среднее ненулевых values.

Редакторский пример внутри простейшего attention класса: один token, одна голова, нулевые logits, непустой value map фиксированного масштаба $a>0$, два допустимых знака этого map. На выборке одинаковых единичных inputs класс содержит функции с выходами $+a$ и $-a$. Его empirical Rademacher complexity равна

$$
\frac a m\,\mathbb E_\epsilon\left|\sum_{i=1}^m\epsilon_i\right|>0.
$$

Все $M_a$ равны нулю, поэтому ограничение можно взять $\rho_{\max}=0$ или сколь угодно малым положительным. Правая часть (16.46) тогда равна нулю или стремится к нулю, хотя функциональный класс сохраняется. Если автор имел в виду **только направленную добавку** после удаления base output, это должен быть иной явно определённый класс. [Формула 16.46](../source/0625-0627-ch16-pac-estimation-and-architecture-selection.md#equation-16-46).

Ортогональность motor directions также не означает статистическую независимость outputs на общей выборке. Линейная верхняя оценка сложности суммы может выводиться субаддитивностью без independence, но требует корректного single-head bound; это не восстанавливает пропущенные предпосылки.

## SRM и выбор архитектуры по данным

Вес $p_k=6/(\pi^2k^2)$ суммируется в 1 на **всех** положительных целых $k$; конечная сумма до $K^*$ меньше 1, что достаточно для union bound. Для адаптивного выбора $k$ нужны события, на которых оценки одновременно верны для всех функций соответствующих классов. Сам выбор архитектуры после наблюдения данных допускается при такой равномерности. [SRM и формула 16.47](../source/0625-0627-ch16-pac-estimation-and-architecture-selection.md#section-16-9-2).

Но нужно определить loss, его диапазон или tail assumptions и точную convention Rademacher term. Lipschitz classifier сам не даёт bounded loss. «Model-selection penalty» в (16.47) стоит под квадратным корнем и делится на $2m$; выражение $2\log k+O(\log(1/\delta))$ описывает числитель, а не весь additive error. Его малость относительно другого слагаемого зависит от остальных констант.

## Что концентрируется в Lemma 16.34

Proof заменяет прежний положительный task operator на $\operatorname{sym}(X^\top XM_a)$. При skew $M_a$ этот оператор имеет нулевой след и может быть indefinite. Это не Gram-оператор $(XM_a)^\top(XM_a)$, на котором раньше строилась PSD-deflation. Population quantity следует определить как $\mathbb E[\operatorname{sym}(X^\top XM_a)]$, а не как функцию одного случайного $X$. [Лемма и определение внутри доказательства](../source/0625-0627-ch16-pac-estimation-and-architecture-selection.md#lemma-16-34); сравнение типов — [210](210-positive-task-operators-and-gate-semantics.md).

Для фиксированного $M_a$ и i.i.d. $X_i$ summands действительно независимы и имеют norm bound $B_X^2\rho_{\max}$. Если $M_a$ обучен на тех же $X_i$, условие i.i.d. для полученных summands автоматически не сохраняется. Возможные постановки: независимое diagnostic window после фиксации модели, uniform bound по заданному классу motors либо отдельный adaptive-data analysis.

Наконец, из напечатанного tail $2d\exp[-m\varepsilon^2/(2B_X^4\rho_{\max}^2)]$ непосредственное решение даёт множитель $\sqrt{2\log(2d/\delta)/m}$, без дополнительного $\sqrt d$. Более слабую оценку с $\sqrt d$ можно записать, но она не является точным результатом указанного решения и не обосновывает заявленную далее необходимость линейного $d$.
