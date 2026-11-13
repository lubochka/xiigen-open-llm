namespace Tokenizer;

public record VocabEntry(int Id, byte[] Bytes)
{
    public string AsString() => System.Text.Encoding.UTF8.GetString(Bytes);
}
