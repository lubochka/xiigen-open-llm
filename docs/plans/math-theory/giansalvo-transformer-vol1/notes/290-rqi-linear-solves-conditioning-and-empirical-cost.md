---
title: "290. Стоимость RQI: indefinite solves, обусловленность и предел эмпирической таблицы"
source_pdf_pages: [697, 698]
book_pages: [661, 662]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 290. Стоимость RQI: indefinite solves, обусловленность и предел эмпирической таблицы

## Вопрос

Следует ли из локального cubic rate стоимость $O(d^2/\sqrt{\Delta_+})$ на шаг и завершение за 2–4 итерации?

**Нет: число внешних RQI шагов и стоимость почти сингулярной линейной системы — отдельные вопросы.** Указанный PCG аргумент не устанавливает требуемые свойства shifted operator, а Table 17.1 показывает один описанный опыт без данных для воспроизведения. [Cost per iteration](../source/0696-0698-ch17-shifted-power-and-rayleigh-quotient-iteration.md#pdf-0698), [Table 17.1](../source/0696-0698-ch17-shifted-power-and-rayleigh-quotient-iteration.md#table-17-1).

## Почему нельзя подставить обычную SPD-оценку PCG

RQI решает $(A-\mu I)x=u$, $\mu=u^\top Au$. Даже если $A$ положительно определена, $\mu$ обычно лежит между eigenvalues, поэтому shifted matrix indefinite. Например,

$$
A=\operatorname{diag}(3,2),\qquad \mu=2.9,\qquad
A-\mu I=\operatorname{diag}(0.1,-0.9).
$$

Напечатанный ratio $(\lambda_{\max}-\mu)/(\lambda_{\min}^{>0}-\mu)$ равен $-1/9$, что не является condition number. Спектральная обусловленность по singular values здесь равна $9$. Стандартный SPD-контракт conjugate gradients нарушен; знак шага и positivity energy norm из него уже не следуют.

Кроме того, при $\mu\to3$ истинная обусловленность $\max_j|\lambda_j-\mu|/\min_j|\lambda_j-\mu|$ расходится, хотя eigengap $3-2=1$ остаётся постоянным. Поэтому $\kappa=O(1/\Delta_+)$ не контролирует близость shift к target. При локальной RQI она связана с текущей угловой ошибкой через $|\lambda-\mu|=O(\sin^2\theta)$.

Нужны solver, подходящий определённости системы, preconditioner с доказанным эффектом и tolerance внутреннего решения, совместимый с внешней cubic convergence. Даже для SPD-итерации число шагов зависит от требуемой точности решения; запись $O(\sqrt\kappa)$ скрывает этот параметр. Источник не выводит здесь uniform complexity для нужных RQI систем. Введение symbol $d$ вместо фактического active dimension также не заменяет analysis solver cost.

## Rank и размерность активного пространства

Если $Q$ имеет $r_Q$ ортонормированных столбцов, то $P^\perp AP^\perp$ действует в пространстве размерности $d-r_Q$, но его rank лишь **не превосходит** эту величину. Он может иметь дополнительное ядро. Если kernel vectors уже включены в $Q$, повторное вычитание $|\ker_{\mathrm{num}}|$ дважды учитывает одни и те же направления. Если block event добавляет несколько columns, head count $K$ также не равен $r_Q$. Для реализации нужны раздельные counters и явный active basis.

## Проверка Table 17.1

По опубликованным rounded значениям

$$
0.296^3=0.025934336,\qquad
0.0323^3=3.3698267\cdot10^{-5},\qquad
(3.69\cdot10^{-5})^3=5.0243409\cdot10^{-14}.
$$

Первые два отношения $\sin\theta_{t+1}/\sin^3\theta_t$ равны примерно $1.24545$ и $1.09501$, что согласуется с rounded $1.25$ и $1.09$ таблицы. Это совместимо с cubic behaviour на данном участке, но не доказывает universal coefficient $1$ и не исправляет proof из [карточки 289](289-rqi-cubic-core-and-invalid-two-dimensional-proof.md).

Ожидаемый следующий масштаб $5\cdot10^{-14}$ существенно **выше** double machine epsilon $2.22\cdot10^{-16}$. Табличное “machine zero” не доказывает реального нулевого угла: при вычислении через $\arccos(u^\top v)$ скалярное произведение округляется к единице уже для углов порядка $\sqrt{\varepsilon_{\mathrm{mach}}}$. Нужны метод измерения, eigenpair residual, исходная матрица, начальный вектор и настройки арифметики. Эти данные в таблице не приведены.

Наконец, фиксированная Lanczos depth $8$ или $16$ не гарантирует попадание в RQI basin для любого спектра и overlap. Такая гарантия требует residual/separation criterion. Локальная оценка допускает $O(\log\log(1/\varepsilon))$ внешних шагов при подходящих исходных условиях, но «2–4 всегда» и конкретное сокращение training runtime из этого не следуют.
