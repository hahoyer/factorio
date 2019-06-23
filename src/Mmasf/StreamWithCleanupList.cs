using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ManageModsAndSaveFiles
{
    public sealed class StreamWithCleanupList : Stream
    {
        readonly IDisposable[] Others;
        readonly Stream StreamImplementation;

        public StreamWithCleanupList(Stream streamImplementation, params IDisposable[] others)
        {
            StreamImplementation = streamImplementation;
            Others = others;
        }

        public override void Flush() { StreamImplementation.Flush(); }
        public override long Seek(long offset, SeekOrigin origin) { return StreamImplementation.Seek(offset, origin); }
        public override void SetLength(long value) { StreamImplementation.SetLength(value); }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return StreamImplementation.Read(buffer, offset, count);
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            StreamImplementation.Write(buffer, offset, count);
        }
        public override bool CanRead { get { return StreamImplementation.CanRead; } }
        public override bool CanSeek { get { return StreamImplementation.CanSeek; } }
        public override bool CanWrite { get { return StreamImplementation.CanWrite; } }
        public override long Length { get { return StreamImplementation.Length; } }
        public override long Position
        {
            get { return StreamImplementation.Position; }
            set { StreamImplementation.Position = value; }
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
                foreach(var other in Others)
                    other.Dispose();

            base.Dispose(disposing);
        }
    }
}