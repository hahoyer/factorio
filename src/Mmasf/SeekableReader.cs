using System;
using System.IO;
using hw.DebugFormatter;

namespace ManageModsAndSaveFiles;

public sealed class SeekableReader : Stream
{
    readonly Stream Stream;
    readonly long? PredefinedLength;
    readonly MemoryStream Buffer = new();
    long PositionValue;

    public SeekableReader(Stream stream, long? predefinedLength = null)
    {
        Stream = stream;
        PredefinedLength = predefinedLength;
        Stream.CanRead.Assert();
    }

    public override void Flush() => Stream.Flush();

    public override long Seek(long offset, SeekOrigin origin)
    {
        var position = GetEffectivePosition(offset, origin);
        Position = position;
        return position;
    }

    public override void SetLength(long value) => Stream.SetLength(value);

    public override int Read(byte[] buffer, int offset, int count)
    {
        AlignBuffer(Position + count);
        Buffer.Position = Position;
        return Buffer.Read(buffer, offset, count);
    }

    public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => PredefinedLength ?? Stream.Length;

    public override long Position
    {
        get => PositionValue;
        set
        {
            AlignBuffer(value);
            PositionValue = value;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if(disposing)
            Buffer.Dispose();
        base.Dispose(disposing);
    }

    long GetEffectivePosition(long offset, SeekOrigin origin)
    {
        switch(origin)
        {
            case SeekOrigin.Begin:
                return offset;
            case SeekOrigin.Current:
                return offset + Buffer.Position;
            case SeekOrigin.End:
                return offset + Buffer.Length;
            default:
                throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
        }
    }

    void AlignBuffer(long value)
    {
        while(Buffer.Length < value)
        {
            Buffer.Seek(0, SeekOrigin.End);
            var buffer = new byte[1000000];

            var count = Stream.Read(buffer, 0, buffer.Length);
            Buffer.Write(buffer, 0, count);
            if(count < buffer.Length)
                return;
        }
    }
}