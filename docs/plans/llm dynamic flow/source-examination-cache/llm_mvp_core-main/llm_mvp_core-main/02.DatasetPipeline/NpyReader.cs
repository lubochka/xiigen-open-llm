using System.Text;

namespace DatasetPipeline;

public static class NpyReader
{
    public static ushort[] Read(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);

        // Magic: \x93NUMPY (6 bytes)
        var magic = br.ReadBytes(6);
        if (magic[0] != 0x93 || Encoding.ASCII.GetString(magic, 1, 5) != "NUMPY")
            throw new InvalidDataException("Not a valid .npy file");

        // Version (2 bytes), header length (2 bytes)
        br.ReadByte(); br.ReadByte(); // version
        int headerLen = br.ReadUInt16();
        br.ReadBytes(headerLen); // skip header — we trust the shape

        var remaining = (int)((fs.Length - fs.Position) / 2);
        var data = new ushort[remaining];
        for (int i = 0; i < remaining; i++)
            data[i] = br.ReadUInt16();
        return data;
    }
}
