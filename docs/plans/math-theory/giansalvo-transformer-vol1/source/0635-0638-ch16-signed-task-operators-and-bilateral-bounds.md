---
title: "Chapter 16: signed task operators, bilateral dynamics, and class-specific head bounds"
source_pdf_pages: [635, 638]
book_pages: [599, 602]
batch: "013"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 16: signed task operators, bilateral dynamics, and class-specific head bounds

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: collapsed scales and averaging](0631-0634-ch16-collapsed-scales-and-polyak-ruppert-recovery.md#pdf-0634) · [Next: signed matrices and pairing](0639-0642-ch16-signed-residual-matrix-and-spectral-pairing.md)

> Editorial note: Full transcription of PDF 635–638 (printed pages 599–602). The opening paragraph completes §16.11. Statements about bias removal, indefinite task operators, dynamical independence, exact head counts, and the four application families are retained as written, including `[citation]` placeholders and the original spelling “indexs”. Their assumptions and limitations are analysed separately in the Russian notes. The final sentence continues on PDF 639.

<a id="pdf-0635"></a>
## PDF 635 · Book page 599

**Summary.** Modern foundation-model training violates the two-time-scale assumption that underpins the original gate convergence theorem. The collapse introduces a bias proportional to $1/\sqrt{\rho_{\mathrm{sep}}}$ in the gate-derived diagnostics, but the bias is a stationary fluctuation around the saddle point, not a drift away from it. Polyak–Ruppert averaging of the gate iterates removes the bias in expectation at the optimal $1/\sqrt T$ rate, recovering the guarantees of the two-time-scale setting up to a logarithmic burn-in. The three-level framework, the compressed-sensing bound, and the four extensions of the preceding sections all carry through under PR-averaged diagnostics, with the same asymptotic architecture and a $1/\sqrt T$ convergence rate. The framework is therefore compatible not only with adaptive optimisers (this chapter) but also with the collapsed-scale regime in which they are deployed.

<a id="section-16-12"></a>
## 16.12 Indefinite task operators and the signed compressed-sensing bound

The compressed-sensing bound of the theorem stated in this part of INCRT I requires the task operator $\mathcal B$ to be positive semi-definite: assumption (B1) states $\mathcal B\succeq0$ and $\|\mathcal B\|_F=\Gamma_0$. For supervised tasks with cross-entropy or quadratic loss, the assumption holds: the task operator is the expected outer product of the loss gradient, $\mathcal B=\mathbb E_x[\nabla\ell(x)\,\nabla\ell(x)^\top]$, which is positive semi-definite by construction.

A growing class of modern training objectives violates the assumption. Reinforcement learning with policy-gradient or actor–critic algorithms produces a task operator that involves the value function gradient minus the policy gradient [citation], an indefinite combination. Contrastive learning balances positive-pair attraction against negative-pair repulsion, with the implicit operator equal to the graph Laplacian of an augmentation graph [citation]; the Laplacian has eigenvalues of both signs after recentring. Adversarial training, generative adversarial networks [citation], and saddle-point optimisation more broadly produce min–max objectives whose Hessian is indefinite by construction [citation].

In each of these regimes, the task operator admits a *signed spectral decomposition*: the eigenvalues come in two sign classes, with the positive class encoding the directions the architecture must capture and the negative class encoding the directions it must reject. The framework as developed so far is silent on the negative class.

The development that follows extends the framework. The signed decomposition of $\mathcal B$ produces two task operators, $\mathcal B_+$ and $\mathcal B_-$, each positive semi-definite, whose difference recovers the original. The compressed-sensing bound applies to each independently, and the total head count is the sum: positive-class heads capture the directions to retain, negative-class heads capture the directions to suppress. Calibration on representative tasks shows that the split between $K_+^*$ and $K_-^*$ varies systematically with the training regime, recovering empirical observations from the four literatures cited above.

<a id="section-16-12-1"></a>
### 16.12.1 The signed Jordan decomposition

Let $\mathcal B$ be a symmetric matrix (the task operator is symmetric even when indefinite, by definition as the loss-gradient covariance with possibly negative weights). The spectral theorem gives $\mathcal B=\sum_i\lambda_i u_i u_i^\top$ with real $\lambda_i$, and the Jordan decomposition splits the spectrum:

<a id="equation-16-65"></a>
$$
\mathcal B=\mathcal B_+-\mathcal B_-,\qquad
\mathcal B_+=\sum_{i:\lambda_i>0}\lambda_i u_i u_i^\top,\qquad
\mathcal B_-=\sum_{i:\lambda_i<0}|\lambda_i|u_i u_i^\top.
\tag{16.65}
$$

Both $\mathcal B_+$ and $\mathcal B_-$ are positive semi-definite by construction; their supports are orthogonal (the eigenvectors of $\mathcal B$ are mutually orthogonal); and their difference recovers the original.

<a id="pdf-0636"></a>
## PDF 636 · Book page 600

<a id="definition-16-42"></a>
**Definition 16.42 (Signed task operators and complexities).** The *positive task operator* $\mathcal B_+$ encodes the directions to be captured by the architecture; the *negative task operator* $\mathcal B_-$ encodes the directions to be suppressed. The signed directional energies are $\Gamma_0^+=\|\mathcal B_+\|_F$ and $\Gamma_0^-=\|\mathcal B_-\|_F$, with $\Gamma_0^2=(\Gamma_0^+)^2+(\Gamma_0^-)^2$. The signed directional task complexity indexs are $\kappa_{A,+}=\Gamma_0^+/\delta_0^+$ and $\kappa_{A,-}=\Gamma_0^-/\delta_0^-$, where $\delta_0^\pm=\min_{i:\pm\lambda_i>0}|\lambda_i|$.

For purely supervised tasks $\mathcal B_-=0$ and the second complexity is undefined; the framework reduces to the original. For purely contrastive or adversarial tasks the two complexities may be of comparable magnitude, indicating a balanced attraction–repulsion regime.

<a id="section-16-12-2"></a>
### 16.12.2 The bilateral motor in the indefinite setting

The bilateral gate of INCRT I reads two eigenvectors at each step: the dominant captured direction $u^+$ and the dominant uncaptured direction $u^-$, both of $A_{\mathrm{res}}$. In the indefinite setting, the gate generalises to four eigenvectors: the dominant positive captured $(u_+^+)$, the dominant uncaptured of the positive class $(u_-^+)$, and the analogous pair on the negative class $(u_+^-,u_-^-)$. Each pair operates on its own residual operator, $A_{\mathrm{res}}^+=A_{\mathrm{res}}|_{\mathrm{pos}}$ and $A_{\mathrm{res}}^-=A_{\mathrm{res}}|_{\mathrm{neg}}$.

<a id="theorem-16-43"></a>
**Theorem 16.43 (Signed gate convergence).** *Under the assumptions of the theorem stated in this part of INCRT I applied separately to each sign class, the four-channel gate flow contracts the residual energies of both classes at rates*

<a id="equation-16-66"></a>
$$
\dot\Gamma_{\mathrm{res}}^\pm
\le-4c_0^\pm\eta^+\Delta^{+,\pm}\Gamma_{\mathrm{res}}^\pm,
\qquad c_0^\pm=1/\kappa_{A,\pm}^2,
\tag{16.66}
$$

*with the two classes decoupled at the dynamical level.*

*Proof sketch.* The Jordan decomposition of $\mathcal B$ induces a Jordan decomposition of $A_{\mathrm{res}}$ throughout training: the positive eigenspace and the negative eigenspace remain orthogonal under any deflation that respects the sign structure (orthogonal projection onto either class commutes with deflation on the other class). The gate convergence proof of the theorem stated in this part of INCRT I applies to each class independently, with the spectral asymmetry constant of that class. The decoupling is exact for symmetric $\mathcal B$; approximate up to a constant factor for nearly-symmetric task operators that arise in practice. $\square$

The decoupling is the structural simplification that makes the indefinite case tractable. The two classes do not interfere at the dynamical level; the gate on each class operates as if the other did not exist, with its own directional task complexity index.

<a id="section-16-12-3"></a>
### 16.12.3 The signed compressed-sensing bound

The compressed-sensing argument that fixes $K^*$ in the theorem stated in this part of INCRT I extends to each sign class.

<a id="theorem-16-44"></a>
**Theorem 16.44 (Signed compressed-sensing bound).** *Under the assumptions of the theorem stated in this part of INCRT I applied separately to each sign class of the indefinite task operator, the optimal head counts satisfy*

<a id="equation-16-67"></a>
$$
K_+^*=\kappa_{A,+}^2\log\frac{\Gamma_0^+}{\theta_w},
\qquad
K_-^*=\kappa_{A,-}^2\log\frac{\Gamma_0^-}{\theta_w},
\tag{16.67}
$$

*with matching upper and lower bounds. The total head count is*

<a id="equation-16-68"></a>
$$
K_{\mathrm{tot}}^*=K_+^*+K_-^*,
\tag{16.68}
$$

*and is achieved by an INCRT trajectory that grows positive-class heads and negative-class heads independently, terminating each class at its own residual threshold.*

<a id="pdf-0637"></a>
## PDF 637 · Book page 601

*Proof sketch.* The orthogonality of the positive and negative eigenspaces allows the deflation to proceed independently on each class. the theorem stated in this part of INCRT I applied to $\mathcal B_+$ gives $K_+^*$; applied to $\mathcal B_-$ gives $K_-^*$. The total count is their sum because heads in the positive class do not capture negative-class energy and vice versa. The matching lower bound follows from the theorem stated in this part of INCRT I applied to each class independently. $\square$

The bound is the natural generalisation: total head budget equals the sum of the two class budgets. The original bound is recovered when $\mathcal B_-=0$ ($K_-^*=0$, total count equals $K_+^*=K^*$).

**The class-balance ratio.** The ratio $K_-^*/K_+^*$ measures the relative architectural weight of suppression versus capture. A purely supervised task has ratio $0$; a perfectly balanced attraction–repulsion task has ratio $1$. The ratio depends on the training regime through the relative magnitudes of $\kappa_{A,+}$ and $\kappa_{A,-}$ and the relative directional energies $\Gamma_0^\pm$.

<a id="corollary-16-45"></a>
**Corollary 16.45 (Sign-balanced head count).** *For tasks in which the positive and negative classes have comparable spectral structure ($\kappa_{A,+}\approx\kappa_{A,-}$ and $\Gamma_0^+\approx\Gamma_0^-$), the signed total head count satisfies*

<a id="equation-16-69"></a>
$$
K_{\mathrm{tot}}^*\approx2\,\kappa_{A,+}^2\log\frac{\Gamma_0^+}{\theta_w}.
\tag{16.69}
$$

*The architecture is twice as wide as the equivalent supervised problem on the same captured energy, with half the heads allocated to suppression.*

The corollary explains the empirical observation that contrastive and adversarial training require larger architectures than supervised baselines on tasks with comparable spectral structure: the suppression class roughly doubles the head budget.

<a id="section-16-12-4"></a>
### 16.12.4 Recovery of four task families

The signed framework specialises to four training regimes, recovering known structural properties of each.

**Reinforcement learning.** The policy-gradient task operator decomposes into the value function (positive class, captured) and the negative-advantage directions (negative class, suppressed): the architecture must learn to recognise high-value states and to actively avoid low-value ones [citation]. The signed framework predicts $K_+^*$ for value capture and $K_-^*$ for action suppression; calibration on standard control tasks (DeepMind Control Suite) gives ratios in the range $K_-^*/K_+^*\in[0.4,0.8]$, with sparse-reward tasks at the upper end.

**Contrastive learning.** The spectral contrastive loss [citation] implements a graph Laplacian decomposition with positive eigenvalues encoding inter-cluster structure (positive pairs) and negative eigenvalues encoding intra-cluster discrimination (negative pairs). The framework predicts $K_+^*$ for cluster identification and $K_-^*$ for hard-negative discrimination; the empirical observation that hard-negative mining [citation] improves performance disproportionately is recovered as the regime $\kappa_{A,-}>\kappa_{A,+}$, in which the negative class is the bottleneck.

**Generative adversarial networks.** The GAN discriminator [citation] is trained with a min–max objective whose critical points are saddle points of the joint loss. The signed framework treats the generator and discriminator as positive and negative classes respectively:

<a id="pdf-0638"></a>
## PDF 638 · Book page 602

the generator captures the data distribution ($\mathcal B_+$, energy $\Gamma_0^+$) and the discriminator suppresses non-data directions ($\mathcal B_-$, energy $\Gamma_0^-$). The two-time-scale update rule of [citation] corresponds, in the language of the framework, to running the positive and negative gates at asymmetric class learning rates, a specialisation of the signed gate dynamics of the theorem.

**Adversarial training.** Adversarial robust training [citation] optimises against a worst-case perturbation, producing an indefinite operator with positive eigenvalues on the standard task directions and negative eigenvalues on the adversarial perturbation directions. The framework predicts $K_-^*$ heads dedicated to suppressing adversarial directions, in agreement with the empirical finding that adversarially robust models have larger architectures than clean-trained ones at equal accuracy [citation].

<a id="section-16-12-5"></a>
### 16.12.5 Algorithmic prescription

<a id="algorithm-0638-signed-growth-and-deflation"></a>
The INCRT algorithm of INCRT I extends to the indefinite setting with two structural modifications.

First, the residual operator is split into its positive and negative classes at every diagnostic step, using the eigenvalue sign as the classifier. Second, the growth criterion is evaluated on each class independently: a head is grown in the positive class when $\Gamma_{\mathrm{res}}^+>\theta_w$ and in the negative class when $\Gamma_{\mathrm{res}}^->\theta_w$, with the head’s role (captured eigenvector or suppressed eigenvector) marked at growth time.

The deflation step is unchanged: each new head deflates $A_{\mathrm{res}}^+$ if it is a positive-class head and $A_{\mathrm{res}}^-$ if it is a negative-class head. The orthogonality between the two classes (a direct consequence of the symmetry of $\mathcal B$) ensures that the two deflation sequences do not interfere.

**Compatibility with the other extensions.** The signed framework composes with the eight extensions of the preceding sections. In the continual setting (this chapter), the homeostatic equilibrium of the theorem stated in this part in this chapter holds for each class independently, with class-specific discounts $\gamma_{\mathrm{hom}}^\pm=1-1/\kappa_{A,\pm}^2$. In the MoE setting (this chapter), each expert may be positive-class, negative-class, or mixed; the routing function respects the class structure of the input. In the transfer setting (this chapter), the spectral overlap is computed separately on the two classes. In the Adam setting (this chapter), the preconditioner operates within each class and cancels the spectral asymmetry of that class, with the cross-class preconditioning trivial because the classes are orthogonal. The deployment bounds of this chapter apply to the total $K_{\mathrm{tot}}^*$, with the budget split between the two classes proportionally to their CS bounds.

<a id="section-16-12-6"></a>
### 16.12.6 Empirical signature

The signed framework predicts that the architecture grown on an indefinite task allocates heads in the ratio $K_-^*/K_+^*\approx(\kappa_{A,-}/\kappa_{A,+})^2$ between suppression and capture. The prediction can be tested by training on representative tasks in each of the four families and measuring the relative architectural allocation.

**Setup.** Four tasks are considered, each with controlled $\kappa_{A,+}$ and $\kappa_{A,-}$: a SAC continual control task on the DMC *cheetah-run* benchmark (RL), a SimCLR pretraining run on CIFAR-10 augmentations (contrastive), a small WGAN-GP run on CelebA-32 (GAN), and an adversarially-robust ResNet on CIFAR-10 with PGD-10 ($\ell_\infty$-bounded adversary) (adversarial). For each, the signed task operators are estimated from the gradient covariance

> Boundary: This sentence continues on [PDF 639](0639-0642-ch16-signed-residual-matrix-and-spectral-pairing.md#pdf-0639).
