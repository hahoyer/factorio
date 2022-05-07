using System;
using System.IO;

namespace ManageModsAndSaveFiles;

public sealed class StreamWithCleanupList : Stream
{
    readonly IDisposable[] Others;
    readonly Stream StreamImplementation;

    public StreamWithCleanupList(Stream streamImplementation, params IDisposable[] others)
    {
        StreamImplementation = streamImplementation;
        Others = others;
    }

    public override void Flush() => StreamImplementation.Flush();
    public override long Seek(long offset, SeekOrigin origin) => StreamImplementation.Seek(offset, origin);
    public override void SetLength(long value) => StreamImplementation.SetLength(value);

    public override int Read(byte[] buffer, int offset, int count) => StreamImplementation.Read(buffer, offset, count);

    public override void Write
        (byte[] buffer, int offset, int count) => StreamImplementation.Write(buffer, offset, count);

    public override bool CanRead => StreamImplementation.CanRead;
    public override bool CanSeek => StreamImplementation.CanSeek;
    public override bool CanWrite => StreamImplementation.CanWrite;
    public override long Length => StreamImplementation.Length;

    public override long Position
    {
        get => StreamImplementation.Position;
        set => StreamImplementation.Position = value;
    }

    protected override void Dispose(bool disposing)
    {
        if(disposing)
            foreach(var other in Others)
                other.Dispose();

        base.Dispose(disposing);
    }
}