using System;
using System.IO;

namespace FastPack.Lib;

internal class SubStream : Stream
{
	private long StartPositionInParentStream { get; }
	private long PositionInParentStream { get; set; }
	private long EndPositionInParentStream { get; }
	private Stream ParentStream { get; }
	private bool IsDisposed { get; set; }
		
	public SubStream(Stream parentStream, long startPosition, long maxLength)
	{
		StartPositionInParentStream = startPosition;
		PositionInParentStream = startPosition;
		EndPositionInParentStream = startPosition + maxLength;
		ParentStream = parentStream;
	}

	public override long Length
	{
		get
		{
			ThrowIfDisposed();
			return EndPositionInParentStream - StartPositionInParentStream;
		}
	}

	public override long Position {
		get
		{
			ThrowIfDisposed();
			return PositionInParentStream - StartPositionInParentStream;
		}
		set
		{
			throw new NotSupportedException("Seeking is not supported!");
		}
	}

	public override bool CanRead => ParentStream.CanRead && !IsDisposed;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	private void ThrowIfDisposed()
	{
		if (IsDisposed)
			throw new ObjectDisposedException(GetType().Name);
	}

	private void ThrowIfCantRead()
	{
		if (!CanRead)
			throw new NotSupportedException("Can not read!");
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		ThrowIfDisposed();
		ThrowIfCantRead();

		if (ParentStream.Position != PositionInParentStream)
			ParentStream.Seek(PositionInParentStream, SeekOrigin.Begin);

		if (PositionInParentStream +  count > EndPositionInParentStream)
			count = (int) (EndPositionInParentStream - PositionInParentStream);

		int readBytes = ParentStream.Read(buffer, offset, count);
		PositionInParentStream += readBytes;

		return readBytes;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		ThrowIfDisposed();
		throw new NotSupportedException("Seeking is not supported.");
	}

	public override void SetLength(long value)
	{
		ThrowIfDisposed();
		throw new NotSupportedException("SetLength requires seeking and writing, which is not supported.");
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		ThrowIfDisposed();
		throw new NotSupportedException("Writing is not supported.");
	}

	public override void Flush()
	{
		ThrowIfDisposed();
		throw new NotSupportedException("Writing is not supported.");
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && !IsDisposed)
			IsDisposed = true;

		base.Dispose(disposing);
	}
}