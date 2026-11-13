---
title: "320. Temporal control operator: скрытое состояние, размерность и условия переноса теории"
source_pdf_pages: [743, 745]
book_pages: [707, 709]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 320. Temporal control operator: скрытое состояние, размерность и условия переноса теории

## Вопрос

Достаточно ли заменить feature domain на temporal lags в Table 18.6, чтобы прежние head-count bounds применялись к контроллеру?

**Таблица задаёт полезное соответствие обозначений, но не проверяет все математические и статистические условия нового оператора.** В этом фрагменте физическая модель и доказательство переноса отнесены во внешнюю работу. [Control setup](../source/0743-0745-ch18-euler-lagrange-control-and-deployment-equivalence.md#section-18-3-1), [Table 18.6](../source/0743-0745-ch18-euler-lagrange-control-and-deployment-equivalence.md#table-18-6).

## Что задаёт (18.3)

Источник определяет $A_{\mathrm{res}}^{\mathrm{temp}}(\tau_z)=\mathbb E[gg^\top]$, где $g=\nabla_{\mathrm{hist}}z(t)$, и указывает размер $W\times W$. Если $g$ — действительно вещественный $W$-вектор с конечным вторым моментом, то оператор PSD:

$$
v^\top A_{\mathrm{res}}^{\mathrm{temp}}v
=\mathbb E[(v^\top g)^2]\ge0.
$$

Это second moment, не centred covariance, если $\mathbb E g\ne0$. И сама формула пока не содержит зависимости от уже захваченного rank $K$, projection или вычитания approximation. Название “residual” требует связать этот task moment с конкретной дефляцией. [Формула (18.3)](../source/0743-0745-ch18-euler-lagrange-control-and-deployment-equivalence.md#equation-18-3).

Размерность тоже нуждается в convention. Если $W$ — число временных samples, а каждый sample содержит $(q,\dot q)\in\mathbb R^4$ двухзвенного механизма, то flattened history имеет размер $4W$, а gradient скалярного $z$ — $4W$ компонент. Если $z$ векторное, возникает Jacobian и требуется выбрать history-space Gram $J^\top J$ либо state-space Gram $JJ^\top$. Получить именно $W\times W$ можно после заданного per-lag aggregation; эта карта здесь не выписана.

## Скрытое состояние и доступность estimator

Тот факт, что история содержит информацию о $z(t)$, не означает, что $z$ однозначно является differentiable function этой конечной истории. Возможны неопределённость начального hidden state, роль управляющих inputs и неполная observability. Для $\nabla_{\mathrm{hist}}z(t)$ нужен конкретный model-based sensitivity, условная оценка или обученный observer с собственными предпосылками.

В симуляторе истинное $z(t)$ и его sensitivities могут быть доступны; это допустимый вариант. Но тогда надо отделять oracle/model-dependent Phase-1 operator от estimator, использующего только наблюдаемые encoder tokens. Runtime далее наблюдает effective rank таких tokens; равенство их спектра спектру (18.3) не следует из названия “on-line analogue”.

Выборка $N=2048$ trajectory points также не становится i.i.d. от своего размера. Для концентрации empirical second moment нужны moment/tail bounds и условия временной зависимости или effective sample size. [Phase-1 estimation](../source/0746-0748-ch18-static-control-failure-and-runtime-growth.md#section-18-3-2).

## Что должно переноситься вместе с notation

Даже при PSD остаются отдельными требования: положительный relevant spectral gap, нормировка и stopping threshold, accuracy eigenvectors, корректность deflation, связь operator residual с tracking objective и изменение data distribution по мере обучения policy. Перечисление трёх неизвестных hypotheses “Theorem ??” не позволяет проверить их по этим страницам. Source утверждает проверку во внешней работе; библиотека сохраняет именно этот статус.

Архитектура выдаёт gains $\theta=(K_d,\Lambda)$ computed-torque law, а не непосредственно torques. Малое изменение выхода attention поэтому не тождественно малому изменению долгосрочной tracking error. Для такого перехода нужны свойства gain map, closed-loop dynamics и области trajectories. На этих страницах нет полного Euler–Lagrange equation, gain constraints или Lyapunov proof, поэтому нельзя добавлять гарантию устойчивости, которой здесь не сформулировано.

Для проекта полезна сама идея diagnostic, учитывающего память задачи. Но feature covariance, temporal sensitivity moment, token effective rank и downstream RMSE — четыре различных объекта. Их связь требуется определить до применения общей head-count формулы.
