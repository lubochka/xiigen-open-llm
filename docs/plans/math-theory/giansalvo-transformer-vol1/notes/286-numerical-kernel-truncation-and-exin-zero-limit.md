---
title: "286. Численное ядро: линейное подпространство, цена truncation и предел EXIN"
source_pdf_pages: [693, 694]
book_pages: [657, 658]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 286. Численное ядро: линейное подпространство, цена truncation и предел EXIN

## Вопрос

Корректно ли задано numerical kernel и действительно ли EXIN делит на исчезающий Rayleigh quotient?

**Rank truncation может быть полезным ограничением активной задачи, но определение ядра и объяснение сингулярности в этом фрагменте требуют исправления.** Записанное ранее обновление EXIN умножает на Rayleigh quotient; при его исчезновении исчезает поправка, а не появляется деление на ноль. [Definition 17.5 и объяснение Type C](../source/0693-0695-ch17-kernel-detection-and-relative-gap-test.md#definition-17-5), [обновление (17.2)](../source/0685-0688-ch17-gate-rates-rayleigh-noise-and-orthogonality.md#equation-17-2).

## Почему напечатанное множество не является ядром

Автор отождествляет span малых eigenvalues с множеством

$$
\{v:\|Av\|<\theta_{\mathrm{rank}}\|v\|\}.
$$

При строгом неравенстве это множество даже не содержит $v=0$. Оно не замкнуто по сложению. Для $A=\operatorname{diag}(0,1)$, $\theta=0.1$ векторы $v=(1,0.05)$ и $w=(-1,0.05)$ удовлетворяют условию: $0.05<0.1\sqrt{1.0025}$. Но $v+w=(0,0.1)$ ему не удовлетворяет: $0.1\not<0.01$.

Корректное спектральное **подпространство** можно определить отдельно:

$$
V_{\mathrm{num}}(\theta)
=\operatorname{span}\{v_j:|\lambda_j|\le\theta\}.
$$

Для всех его векторов $\|Av\|\le\theta\|v\|$. Обратное включение множества всех векторов с малым Rayleigh-type residual в этот span неверно: допустимы малые примеси направлений с большими eigenvalues. Следует также явно определить строгую/нестрогую границу.

Порог (17.14) равен $\sqrt{\varepsilon_{\mathrm{mach}}}\lambda_1$. Он осмыслен как положительная шкала при $\lambda_1>0$. При $A=0$ порог нулевой и строгое сравнение не распознаёт даже точный kernel; при отрицательной $\lambda_1$ порог отрицателен. Для signed оператора естественна отдельно объявленная шкала $\|A\|_2$ или max absolute eigenvalue с абсолютным tolerance, но это уже редакторский вариант, не исходная формула. [Формула (17.14)](../source/0693-0695-ch17-kernel-detection-and-relative-gap-test.md#equation-17-14).

## Что происходит с записанным EXIN

При $\|u\|=1$ поправка равна $-\eta R(u)(Au-R(u)u)$. Знаменатель исходной (17.2) — $\|u\|^2$, а не $R(u)$. На точном kernel eigenvector она равна нулю. При приближении к нему может возникать очень медленная динамика: для PSD задачи около нулевого endpoint линейный множитель равен единице, как показано в [карточке 282](282-gate-updates-versus-power-method-rates.md). Но заявленное в прозе «divide by a vanishing quantity» не описывает эту формулу. Другой алгоритм с обратным Rayleigh quotient потребовал бы отдельной записи.

## Цена удаления малых направлений

Для точных eigenvectors отбросить блок $J$ с $|\lambda_j|\le\theta$ означает изменить оператор на $\sum_{j\in J}\lambda_jv_jv_j^\top$. Его operator norm не превосходит $\theta$, а квадрат Frobenius norm равен

$$
\sum_{j\in J}\lambda_j^2\le |J|\theta^2.
$$

Только **точно нулевые** eigenvalues имеют нулевую spectral energy. Приближённые kernel directions требуют бюджета truncation error и оценки eigenpair residual; маленькое Ritz value само по себе не удостоверяет kernel. [Rank-truncation prescription](../source/0693-0695-ch17-kernel-detection-and-relative-gap-test.md#prescription-0693-rank-truncation).

“Zero-cost” здесь может означать «без дополнительной обучаемой головы». Добавленные столбцы всё равно занимают память и участвуют в orthogonalisation. Поэтому число голов $K$, число столбцов $r_Q=\operatorname{rank}Q$ и количество отброшенных numerical directions надо хранить отдельно. Утверждение $\theta_{\mathrm{rank}}\ll\theta_w$ также требует явного выбора двух tolerances. Упомянутое наблюдение SARS-CoV-2 отнесено к будущей главе 18 и в этих страницах не проверено; оно не заменяет указанных алгебраических условий.
