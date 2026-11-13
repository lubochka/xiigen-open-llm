---
title: "190. Относительная минимальность и корректные условия water-filling"
source_pdf_pages: [541, 542]
book_pages: [505, 506]
batch: "011"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 190. Относительная минимальность и корректные условия water-filling

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Что именно минимально

Definition 14.21 задаёт rationalised class через пять обязательных возможностей: representatives, S–A split, crowding control, marginal preservation и residual constraint. Proposition 14.22 затем утверждает необходимость каждой возможности относительно этого класса. В таком чтении удаление обязательного свойства выводит блок из определённого класса. Это не нижняя граница на число параметров, FLOPs, heads или независимых программных модулей. [Определение и proposition](../source/0541-0546-ch14-static-closure-and-incrt-handoff.md#definition-14-21).

Фраза proof о каждом компоненте как «unique carrier» соответствующего ограничения — дополнительное предположение, не выведенное из списка свойств. Один механизм может реализовывать несколько свойств, а определение прямо разрешает recoverable split, assignment rule и equivalent preservation weights. Поэтому полезный вывод — необходимость заданных **функций контроля внутри выбранной спецификации**, без доказательства уникальности конкретной реализации. Автор сам исключает глобальную уникальность среди всех архитектур.

## Задача распределения

Для $p=(p_S,p_A,p_M)$ на simplex источник минимизирует

$$
F(p)=E_S(p_S)+E_A(p_A)+E_M(p_M)+\eta\Phi(p),
\qquad p_i\ge0,\quad \sum_i p_i=1.
$$

Убывающая $E_i$ описывает уменьшение измеренного остатка при увеличении доли capacity. Proposition 14.23 связывает внутренний optimum с равенством marginal reductions. [Постановка](../source/0541-0546-ch14-static-closure-and-incrt-handoff.md#display-0541-allocation-objective), [water-filling](../source/0541-0546-ch14-static-closure-and-incrt-handoff.md#proposition-14-23).

## Полная форма KKT

Если все используемые функции дифференцируемы, условия имеют вид

$$
E_i'(p_i)+\eta\,\partial_i\Phi(p)+\lambda-\nu_i=0,\qquad
\nu_i\ge0,\quad p_i\ge0,\quad \nu_i p_i=0.
$$

Для активных координат $p_i>0$ величины $E_i'(p_i)+\eta\partial_i\Phi(p)$ равны $-\lambda$. Это равенство **полных регуляризованных производных**. Равенство только $-E_i'$ из напечатанного display требует нулевого regulariser либо одинаковых его частных производных в optimum. Слова автора «after including the derivative of the regulariser» позволяют восстановить этот смысл, но сами дополнительные члены в display не показаны.

Внутренняя stationarity необходима и без выпуклости. Для достаточности KKT как условия глобального optimum нужна выпуклость всего $F$, в частности подходящие условия на $\eta\Phi$; в proposition они явно не перечислены. Для boundary solutions требуется определить $E_i$ при $p_i=0$ или их расширение предельными значениями, поскольку исходный домен указан как $(0,1]$.

Выпуклость также не гарантирует единственности. При $E_i(p_i)=1-p_i$ и $\Phi=0$ objective равен 2 на всём simplex: все распределения оптимальны, хотя три $E_i$ неотрицательны, дифференцируемы, выпуклы и строго убывают.

## Проверяемый пример и связь с FFN

В специальной модели $E_i(p_i)=a_i/p_i$, $a_i>0$, $\Phi=0$ optimum внутренний и

$$
p_i^\star=\frac{\sqrt{a_i}}{\sum_j\sqrt{a_j}}.
$$

Действительно, $-E_i'(p_i^\star)=a_i/(p_i^\star)^2=(\sum_j\sqrt{a_j})^2$ для всех каналов. Это пример, где water-filling даёт конкретное правило после задания error curves; книга не устанавливает, что измеренные Transformer residuals обязаны иметь такую форму.

§14.14.3 переносит принцип на FFN, концептуально разделяя $R_{\mathrm{ff},S}$, $R_{\mathrm{ff},A}$ и $R_{\mathrm{ff},M}$. Целые ширины, совместное обучение каналов и возможное parameter sharing требуют дискретного выбора и проверки actual loss. Непрерывная convex relaxation сама по себе не доказывает оптимальности округлённой архитектуры. [FFN design](../source/0541-0546-ch14-static-closure-and-incrt-handoff.md#section-14-14-3).
