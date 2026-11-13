# Вносит перенос FIX-KHC по готовой форме карточки: ровно два файла.
# Запускается только после снятой базовой линии полного прогона.
import io, sys
sys.stdout.reconfigure(encoding="utf-8")

# --- Файл первый: разметчик получает перегрузки, принимающие готовое дерево ---
p1 = r"C:\Projects\llm_mvp_core\12.SemanticCompiler\Prediction\FileActionKnn.cs"
s1 = io.open(p1, encoding="utf-8", newline="").read()

# Пространство имён готового дерева: тип узла живёт в Microsoft.CodeAnalysis.
old_using = "using Microsoft.CodeAnalysis.CSharp;\nusing Microsoft.CodeAnalysis.CSharp.Syntax;"
new_using = "using Microsoft.CodeAnalysis;\nusing Microsoft.CodeAnalysis.CSharp;\nusing Microsoft.CodeAnalysis.CSharp.Syntax;"
assert s1.count(old_using) == 1, "список using не найден"
s1 = s1.replace(old_using, new_using, 1)

# Метка: строковый метод сохраняет свой разбор и делегирует; тело переезжает в перегрузку.
old_label = """    public static int Label(string csharpSource)
    {
        var root = CSharpSyntaxTree.ParseText(csharpSource).GetRoot();
        var methods"""
new_label = """    public static int Label(string csharpSource)
    {
        // FIX-KHC transfer, plan 24-file-action-shared-parse.md, from KH-COST-01.
        // Строковый путь сохраняет СВОЙ разбор и прежнюю обработку пустого текста:
        // старые вызывающие ничего не замечают. Тело правила переехало в перегрузку
        // ниже, которая принимает уже разобранное дерево.
        return Label(CSharpSyntaxTree.ParseText(csharpSource).GetRoot());
    }

    // FIX-KHC transfer: то же правило метки по УЖЕ разобранному дереву.
    // Договор перегрузки: передаётся неизменяемое дерево того же исходника с теми
    // же настройками разбора. Тело не разбирает текст заново и дерево не меняет.
    public static int Label(SyntaxNode root)
    {
        var methods"""
assert s1.count(old_label) == 1, "голова метода метки не найдена"
s1 = s1.replace(old_label, new_label, 1)

# Признаки: то же разделение.
old_fv = """    public static float[] FeatureVector(string csharpSource)
    {
        var root = CSharpSyntaxTree.ParseText(csharpSource).GetRoot();
        var nodes"""
new_fv = """    public static float[] FeatureVector(string csharpSource)
    {
        // FIX-KHC transfer: строковый путь сохраняет свой разбор и делегирует.
        return FeatureVector(CSharpSyntaxTree.ParseText(csharpSource).GetRoot());
    }

    // FIX-KHC transfer: тот же вектор признаков по УЖЕ разобранному дереву.
    // Договор перегрузки тот же: дерево того же исходника, без повторного разбора.
    public static float[] FeatureVector(SyntaxNode root)
    {
        var nodes"""
assert s1.count(old_fv) == 1, "голова метода признаков не найдена"
s1 = s1.replace(old_fv, new_fv, 1)

io.open(p1, "w", encoding="utf-8", newline="").write(s1)

# --- Файл второй: подготовка уроков разбирает один раз и отдаёт одно дерево обоим ---
p2 = r"C:\Projects\llm_mvp_core\12.SemanticCompiler\SemanticCompilerFacade.cs"
s2 = io.open(p2, encoding="utf-8", newline="").read()

old_sel = """            .Select(source => (Features: ActionLabelGenerator.FeatureVector(source), Label: ActionLabelGenerator.Label(source)))"""
new_sel = """            // FIX-KHC transfer, plan 24-file-action-shared-parse.md, from KH-COST-01.
            // Исходник урока разбирается ОДИН раз, и одно и то же дерево уходит и в
            // признаки, и в метку. Прежде каждый из двух вызовов разбирал текст сам,
            // то есть одно дерево строилось дважды на каждый урок. Значения не
            // меняются: оба потребителя получают то же дерево того же исходника.
            // Порядок «сначала признаки, затем метка» и условие о четырёх уроках
            // ниже сохранены.
            .Select(source =>
            {
                // Единственный разбор исходника этого урока.
                var root = CSharpSyntaxTree.ParseText(source).GetRoot();

                // Оба потребителя получают одно и то же дерево, в прежнем порядке.
                return (Features: ActionLabelGenerator.FeatureVector(root),
                        Label: ActionLabelGenerator.Label(root));
            })"""
assert s2.count(old_sel) == 1, "участок подготовки уроков не найден"
s2 = s2.replace(old_sel, new_sel, 1)

io.open(p2, "w", encoding="utf-8", newline="").write(s2)
print("перенос внесён в два файла")
