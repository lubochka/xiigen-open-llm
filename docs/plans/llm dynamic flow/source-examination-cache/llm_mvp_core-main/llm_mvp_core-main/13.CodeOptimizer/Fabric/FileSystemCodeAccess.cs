using System.Text;

namespace CodeOptimizer;

public sealed class FileSystemCodeAccess : ICodeReader, ICodeWriter
{
    public OperationResult<string> ReadSource(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return OperationResult<string>.Failure("Path must not be empty");
        if (!File.Exists(path))
            return OperationResult<string>.Failure($"File not found: {path}");
        try
        {
            return OperationResult<string>.Success(File.ReadAllText(path, Encoding.UTF8));
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Failure(ex.Message);
        }
    }

    public OperationResult<IReadOnlyList<string>> ListSourceFiles(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return OperationResult<IReadOnlyList<string>>.Failure($"Directory not found: {directoryPath}");

        char sep = Path.DirectorySeparatorChar;
        var files = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{sep}bin{sep}") && !f.Contains($"{sep}obj{sep}"))
            .ToList();

        return OperationResult<IReadOnlyList<string>>.Success(files);
    }

    public OperationResult<bool> WriteOptimized(string path, string optimizedCode, bool overwrite)
    {
        if (!overwrite && File.Exists(path))
            return OperationResult<bool>.Failure($"File already exists: {path}");
        try
        {
            File.WriteAllText(path, optimizedCode, Encoding.UTF8);
            return OperationResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Failure(ex.Message);
        }
    }
}
