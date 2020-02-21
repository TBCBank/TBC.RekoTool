/*
 *  The MIT License (MIT)
 *
 *  Copyright (c) TBC Bank
 *
 *  All rights reserved.
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */

using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Presents a <see cref="MemoryStream"/> decoy to Amazon S3/Rekognition API methods.
/// In reality, this is a thin wrapper over other stream instance.
/// </summary>
/// <remarks>
/// Avoids one extra array copy, because <see cref="MemoryStream"/> has to be created from existing byte
/// array and then <see cref="MemoryStream.ToArray"/> method creates one additional byte array.
/// This class avoids this extra copy.
/// </remarks>
public sealed class DecoyMemoryStream : MemoryStream
{
    private readonly Stream innerStream;
    private readonly bool leaveOpen;

    public DecoyMemoryStream(Stream otherStream, bool leaveOpen = true) : base(capacity: 0)
    {
        this.innerStream = otherStream ?? throw new ArgumentNullException(nameof(otherStream));
        this.leaveOpen = leaveOpen;
    }

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
        return this.innerStream.BeginRead(buffer, offset, count, callback, state);
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
        return this.innerStream.BeginWrite(buffer, offset, count, callback, state);
    }

    public override bool CanRead => this.innerStream.CanRead;
    public override bool CanSeek => this.innerStream.CanSeek;
    public override bool CanTimeout => this.innerStream.CanTimeout;
    public override bool CanWrite => this.innerStream.CanWrite;
    public override int Capacity { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override long Length => this.innerStream.Length;
    public override long Position { get => this.innerStream.Position; set => this.innerStream.Position = value; }
    public override int ReadTimeout { get => this.innerStream.ReadTimeout; set => this.innerStream.ReadTimeout = value; }
    public override int WriteTimeout { get => this.innerStream.WriteTimeout; set => this.innerStream.WriteTimeout = value; }

    public override void Close()
    {
        try
        {
            if (!leaveOpen)
            {
                this.innerStream.Close();
            }
        }
        finally
        {
            base.Close();
        }
    }

    public override void CopyTo(Stream destination, int bufferSize)
    {
        this.innerStream.CopyTo(destination, bufferSize);
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        return this.innerStream.CopyToAsync(destination, bufferSize, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            if (!leaveOpen)
            {
                this.innerStream.Dispose();
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }

    public override ValueTask DisposeAsync()
    {
        this.Dispose();
        return default;
    }

    public override int EndRead(IAsyncResult asyncResult)
    {
        return this.innerStream.EndRead(asyncResult);
    }

    public override void EndWrite(IAsyncResult asyncResult)
    {
        this.innerStream.EndWrite(asyncResult);
    }

    public override bool Equals(object obj)
    {
        return this.innerStream.Equals(obj);
    }

    public override void Flush()
    {
        this.innerStream.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        return this.innerStream.FlushAsync(cancellationToken);
    }

    /// <remarks>
    /// <see cref="GetBuffer"/> is not the same as <see cref="ToArray"/>, but its OK for our use case.
    /// </remarks>
    public override byte[] GetBuffer()
    {
        return this.ToArray();
    }

    public override int GetHashCode()
    {
        return this.innerStream.GetHashCode();
    }

    public override object InitializeLifetimeService()
    {
        return this.innerStream.InitializeLifetimeService();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return this.innerStream.Read(buffer, offset, count);
    }

    public override int Read(Span<byte> destination)
    {
        return this.innerStream.Read(destination);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return this.innerStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
    {
        return this.innerStream.ReadAsync(destination, cancellationToken);
    }

    public override int ReadByte()
    {
        return this.innerStream.ReadByte();
    }

    public override long Seek(long offset, SeekOrigin loc)
    {
        return this.innerStream.Seek(offset, loc);
    }

    public override void SetLength(long value)
    {
        this.innerStream.SetLength(value);
    }

    public override byte[] ToArray()
    {
        this.innerStream.Position = 0L;

        return ReadAllBytes(this.innerStream);
    }

    /// <remarks>
    /// <see cref="TryGetBuffer"/> is not the same as <see cref="ToArray"/>, but its OK for our use case.
    /// </remarks>
    public override bool TryGetBuffer(out ArraySegment<byte> buffer)
    {
        buffer = new ArraySegment<byte>(this.ToArray());
        return true;
    }

    public override string ToString()
    {
        return this.innerStream.ToString();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        this.innerStream.Write(buffer, offset, count);
    }

    public override void Write(ReadOnlySpan<byte> source)
    {
        this.innerStream.Write(source);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return this.innerStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
    {
        return this.innerStream.WriteAsync(source, cancellationToken);
    }

    public override void WriteByte(byte value)
    {
        this.innerStream.WriteByte(value);
    }

    public override void WriteTo(Stream stream)
    {
        this.innerStream.CopyTo(stream);
    }

#nullable enable

    private const int MaxByteArrayLength = 0x7FFFFFC7;

    private static byte[] ReadAllBytes(Stream fs)
    {
        long fileLength = fs.Length;

        if (fileLength > int.MaxValue)
        {
            throw new IOException("The stream is too long. This operation is currently limited to supporting streams less than 2 gigabytes in size.");
        }
        else if (fileLength == 0)
        {
            // Some file systems (e.g. procfs on Linux) return 0 for length even when there's content.
            // Thus we need to assume 0 doesn't mean empty.
            return ReadAllBytesUnknownLength(fs);
        }

        int index = 0;
        int count = (int)fileLength;
        byte[] bytes = new byte[count];

        while (count > 0)
        {
            int n = fs.Read(bytes, index, count);
            if (n == 0)
            {
                throw new EndOfStreamException("Unable to read beyond the end of the stream.");
            }

            index += n;
            count -= n;
        }

        return bytes;
    }

    private static byte[] ReadAllBytesUnknownLength(Stream fs)
    {
        byte[]? rentedArray = null;
        Span<byte> buffer = stackalloc byte[512];
        try
        {
            int bytesRead = 0;
            while (true)
            {
                if (bytesRead == buffer.Length)
                {
                    uint newLength = (uint)buffer.Length * 2;
                    if (newLength > MaxByteArrayLength)
                    {
                        newLength = (uint)Math.Max(MaxByteArrayLength, buffer.Length + 1);
                    }

                    byte[] tmp = ArrayPool<byte>.Shared.Rent((int)newLength);
                    buffer.CopyTo(tmp);
                    if (rentedArray != null)
                    {
                        ArrayPool<byte>.Shared.Return(rentedArray);
                    }
                    buffer = rentedArray = tmp;
                }

                Debug.Assert(bytesRead < buffer.Length);
                int n = fs.Read(buffer.Slice(bytesRead));
                if (n == 0)
                {
                    return buffer.Slice(0, bytesRead).ToArray();
                }
                bytesRead += n;
            }
        }
        finally
        {
            if (rentedArray != null)
            {
                ArrayPool<byte>.Shared.Return(rentedArray);
            }
        }
    }

#nullable disable
}
