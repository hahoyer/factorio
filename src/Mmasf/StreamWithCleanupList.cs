using System;
using System.IO;

namespace ManageModsAndSaveFiles;

public sealed class StreamWithCleanupList(Stream streamImplementation, params IDisposable[] others) : Stream
{
    public override void Flush() => streamImplementation.Flush();
    public override long Seek(long offset, SeekOrigin origin) => streamImplementation.Seek(offset, origin);
    public override void SetLength(long value) => streamImplementation.SetLength(value);

    public override int Read(byte[] buffer, int offset, int count) => streamImplementation.Read(buffer, offset, count);

    public override void Write
        (byte[] buffer, int offset, int count) => streamImplementation.Write(buffer, offset, count);

    public override bool CanRead => streamImplementation.CanRead;
    public override bool CanSeek => streamImplementation.CanSeek;
    public override bool CanWrite => streamImplementation.CanWrite;
    public override long Length => streamImplementation.Length;

    public override long Position
    {
        get => streamImplementation.Position;
        set => streamImplementation.Position = value;
    }

    protected override void Dispose(bool disposing)
    {
        if(disposing)
            foreach(var other in others)
                other.Dispose();

        base.Dispose(disposing);
    }
}