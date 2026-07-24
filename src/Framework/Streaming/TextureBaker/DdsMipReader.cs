// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Collections.Generic;
using System.IO;
using BCnEncoder.Shared;

namespace TextureBaker
{
    /// <summary>
    /// Reads back the per-mip raw block bytes from a DDS stream produced by BCnEncoder.NET.
    /// Deliberately hand-rolled instead of relying on a library-specific "give me raw mip
    /// bytes" call: the DDS header layout is a stable, publicly documented Microsoft format
    /// (magic + 124-byte DDS_HEADER, mips stored largest-first with no padding between them
    /// for the legacy FourCC codes this baker uses), so parsing it directly is the more
    /// robust choice here than depending on exact method names in a fast-moving library.
    ///
    /// Only handles BC1/BC3 (S3TC, legacy FourCC "DXT1"/"DXT5") - no DX10 header extension to
    /// worry about, which is one of the reasons this baker sticks to those two formats rather
    /// than BC7 (see TextureBakerCore for the rest of that reasoning).
    /// </summary>
    internal static class DdsMipReader
    {
        private const int HeaderSize = 128; // 4-byte "DDS " magic + 124-byte DDS_HEADER

        public static List<(int Width, int Height, byte[] Bytes)> ReadMips( byte[] ddsBytes, CompressionFormat format )
        {
            if ( ddsBytes.Length < HeaderSize || ddsBytes[ 0 ] != (byte)'D' || ddsBytes[ 1 ] != (byte)'D' || ddsBytes[ 2 ] != (byte)'S' || ddsBytes[ 3 ] != (byte)' ' )
                throw new InvalidDataException( "Not a DDS stream - BCnEncoder.NET should have produced one; check OutputOptions.FileFormat." );

            int height = BitConverter.ToInt32( ddsBytes, 12 );
            int width = BitConverter.ToInt32( ddsBytes, 16 );
            int mipCount = BitConverter.ToInt32( ddsBytes, 28 );
            if ( mipCount <= 0 )
                mipCount = 1;

            int bytesPerBlock = format switch
            {
                CompressionFormat.Bc1 => 8,
                CompressionFormat.Bc3 => 16,
                _ => throw new NotSupportedException( $"{format} isn't handled by this baker - only Bc1/Bc3 (S3TC) are supported by design, see TextureBakerCore." ),
            };

            var mips = new List<(int, int, byte[])>( mipCount );
            int offset = HeaderSize;
            int w = width;
            int h = height;

            for ( int i = 0; i < mipCount; i++ )
            {
                int blocksWide = Math.Max( 1, ( w + 3 ) / 4 );
                int blocksHigh = Math.Max( 1, ( h + 3 ) / 4 );
                int size = blocksWide * blocksHigh * bytesPerBlock;

                if ( offset + size > ddsBytes.Length )
                    throw new InvalidDataException( $"DDS stream is shorter than its own mip {i} claims - header mip count or dimensions don't match the data." );

                var bytes = new byte[ size ];
                Buffer.BlockCopy( ddsBytes, offset, bytes, 0, size );
                mips.Add( (w, h, bytes) );

                offset += size;
                w = Math.Max( 1, w / 2 );
                h = Math.Max( 1, h / 2 );
            }

            return mips; // largest (mip 0) first, matching DDS's own layout
        }
    }
}
