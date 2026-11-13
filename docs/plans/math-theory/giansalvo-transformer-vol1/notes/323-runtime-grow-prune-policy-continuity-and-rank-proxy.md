---
title: "323. Runtime grow/prune: точная continuity, rank proxy и неподтверждённый pruning regime"
source_pdf_pages: [748, 749]
book_pages: [712, 713]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 323. Runtime grow/prune: точная continuity, rank proxy и неподтверждённый pruning regime

## Вопрос

Какие гарантии даёт нулевая output projection новой головы и что измеряет runtime effective-rank trigger?

**При явно аддитивной архитектуре нулевая выходная проекция сохраняет текущую функцию в момент grow.** Это полезный точный invariant. Он не доказывает последующую optimisation stability, отсутствие attractors или точность связи token rank с temporal task operator. [Runtime rules](../source/0746-0748-ch18-static-control-failure-and-runtime-growth.md#prescription-0748-runtime-grow-prune-monitor), [траектория роста](../source/0749-0750-ch18-runtime-control-results-curriculum-and-prediction-p5.md#pdf-0749).

## Точное условие для сохранения policy

Если attention output имеет вид $y(x)=y_{\mathrm{old}}(x)+W_{\mathrm{new}}^O h_{\mathrm{new}}(x)$ и $W_{\mathrm{new}}^O=0$, то $y(x)=y_{\mathrm{old}}(x)$ для каждого входа при неизменных остальных частях. Это соответствует source claim о zero-output insertion.

Однако если при изменении числа активных heads меняется общая нормировка, например $y_K=K^{-1}\sum_i y_i$, простое добавление нулевого summand уже меняет старый output. Также надо сохранить ordering projections, normalisation state и старые параметры. На PDF 735 Gaussian soft identity масштаба $10^{-3}$ — другой механизм, обычно дающий приближение вместо точного равенства.

В момент zero-output insertion gradient по внутренним параметрам новой головы содержит множитель $W_{\mathrm{new}}^O$ и может быть нулевым; gradient самой output projection может быть ненулевым и затем включить обучение головы. Поэтому exact continuity не означает мгновенное появление полезного направления. Последующее поведение зависит от features, loss gradients и optimizer.

## Что означает grow diagnostic

Источник использует effective rank последних $N$ encoder tokens с energy threshold $\alpha$ и условие

$$
\widehat\rho_\alpha(t)>K(t)(1+\varepsilon_{\mathrm{grow}}),
$$

после cooldown. Здесь $\alpha$ — порог спектральной энергии; это **не** skew-energy index (18.1), несмотря на одинаковый символ.

Token-matrix effective rank зависит от centering, feature scaling, окна и выбранной cumulative-energy convention. Он не тождественен residual Frobenius norm и не равен автоматически rank latent-memory sensitivity operator (18.3). Для превращения “online analogue” в theorem нужно соответствие этих объектов и оценка sampling error при on-policy drift. Наблюдение по последним 2048 токенам может быть correlated и зависеть от уже обученной policy.

В опыте все runs доходят до cap 8. Это совместимо как с истинной потребностью в восьми направлениях, так и с неудовлетворённым grow criterion после достижения cap. Без значений $\widehat\rho_\alpha$ и stopping residual отличить эти ситуации нельзя. Hard cap — ограничение реализации, не доказанный architectural optimum.

## Prune continuity и stability — отдельные вопросы

Если $\pi_i=\|y_i\|/\sum_j\|y_j\|$, удаление головы даёт абсолютный jump $\|\Delta y\|=\pi_i\sum_j\|y_j\|$. Из $\pi_i<\varepsilon_{\mathrm{prune}}$ следует small **relative-to-sum** contribution, но absolute bound требует контроля суммы. Для другой нормировки $\pi_i$ надо выписать её точное определение; cancellation heads и downstream gain map также имеют значение. Empirical magnitude на одном окне не является uniform bound на всех будущих histories.

Cooldown ограничивает частоту событий, но сам по себе не исключает бесконечного чередования grow/prune. Источник ссылается на дополнительное long-time assumption для $\widehat\rho_\alpha$, не формулируя его здесь. В основной кампании prune trigger вообще не срабатывает, поэтому она не проверяет ни ошибку pruning, ни claimed equilibrium interval при двусторонней динамике.

Для проекта переносимым результатом служит проверяемый function-preserving grow contract. Связь diagnostics с задачей, поведение после grow и весь pruning regime остаются отдельными обязанностями анализа, а не следствиями начального нулевого output.
