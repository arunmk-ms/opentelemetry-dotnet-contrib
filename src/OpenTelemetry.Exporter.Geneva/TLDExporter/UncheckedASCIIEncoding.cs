// <copyright file="UncheckedASCIIEncoding.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Text;

namespace OpenTelemetry.Exporter.Geneva.TldExporter;

/// <summary>
/// Like ASCIIEncoding but instead of checking for non-ASCII characters, it just
/// ignores the top bits of the character. This is significantly faster than
/// ASCIIEncoding and can be used to improve performance in cases where you know a
/// string only contains ASCII characters.
/// </summary>

#pragma warning disable SA1124
internal sealed class UncheckedASCIIEncoding : Encoding
{
    public static Encoding SharedInstance = new UncheckedASCIIEncoding();

    public UncheckedASCIIEncoding()
        : base(20127)
    {
    }

    #region Required implementation of Encoding abstract methods

    public override int GetMaxByteCount(int charCount)
    {
        return charCount;
    }

    public override int GetMaxCharCount(int byteCount)
    {
        return byteCount;
    }

    public override int GetByteCount(char[] chars, int charIndex, int charCount)
    {
        return charCount;
    }

    public override int GetCharCount(byte[] bytes, int byteIndex, int byteCount)
    {
        return byteCount;
    }

    public unsafe override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
        ValidateArgs(chars, charIndex, charCount, bytes, byteIndex, "char", "byte");
        fixed (char* charPtr = chars)
        {
            fixed (byte* bytePtr = bytes)
            {
                return this.GetBytes(charPtr + charIndex, charCount, bytePtr + byteIndex, bytes.Length - byteIndex);
            }
        }
    }

    public unsafe override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
    {
        ValidateArgs(bytes, byteIndex, byteCount, chars, charIndex, "byte", "char");
        fixed (byte* bytePtr = bytes)
        {
            fixed (char* charPtr = chars)
            {
                return this.GetChars(bytePtr + byteIndex, byteCount, charPtr + charIndex, chars.Length - charIndex);
            }
        }
    }

    #endregion

    #region Required overrides (used by the required implementation)

    public override unsafe int GetBytes(char* charPtr, int charCount, byte* bytePtr, int byteCount)
    {
        if (byteCount < charCount)
        {
            throw new ArgumentOutOfRangeException(nameof(byteCount));
        }

        for (int i = 0; i < charCount; i += 1)
        {
            bytePtr[i] = unchecked((byte)(charPtr[i] & 0x7F));
        }

        return charCount;
    }

    public override unsafe int GetChars(byte* bytePtr, int byteCount, char* charPtr, int charCount)
    {
        if (charCount < byteCount)
        {
            throw new ArgumentOutOfRangeException(nameof(charCount));
        }

        for (int i = 0; i < byteCount; i += 1)
        {
            charPtr[i] = (char)bytePtr[i];
        }

        return byteCount;
    }

    #endregion

    #region Optional overrides (performance/functionality improvement)

    public override bool IsSingleByte => true;

    public override int GetByteCount(string chars)
    {
        if (chars == null)
        {
            throw new ArgumentNullException(nameof(chars));
        }

        return chars.Length;
    }

    public override unsafe int GetByteCount(char* charPtr, int charCount)
    {
        return charCount;
    }

    public override unsafe int GetCharCount(byte* bytePtr, int byteCount)
    {
        return byteCount;
    }

    public unsafe override int GetBytes(string chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
        if (chars == null || bytes == null)
        {
            throw new ArgumentNullException(chars == null ? nameof(chars) : nameof(bytes));
        }
        else if (charIndex < 0 || charCount < 0)
        {
            throw new ArgumentOutOfRangeException(charIndex < 0 ? nameof(charIndex) : nameof(charCount));
        }
        else if (chars.Length - charIndex < charCount)
        {
            throw new ArgumentOutOfRangeException(chars);
        }
        else if (byteIndex < 0 || byteIndex > bytes.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(byteIndex));
        }

        fixed (char* charPtr = chars)
        {
            fixed (byte* bytePtr = bytes)
            {
                return this.GetBytes(charPtr + charIndex, charCount, bytePtr + byteIndex, bytes.Length - byteIndex);
            }
        }
    }

    #endregion

    private static void ValidateArgs<TA, TB>(TA[] a, int aIndex, int aCount, TB[] b, int bIndex, string aName, string bName)
    {
        if (a == null || b == null)
        {
            throw new ArgumentNullException(a == null ? aName + "s" : bName + "s");
        }
        else if (aIndex < 0 || aCount < 0)
        {
            throw new ArgumentOutOfRangeException(aIndex < 0 ? aName + "Index" : aName + "Count");
        }
        else if (a.Length - aIndex < aCount)
        {
            throw new ArgumentOutOfRangeException(aName + "s");
        }
        else if (bIndex < 0 || bIndex > b.Length)
        {
            throw new ArgumentOutOfRangeException(bName + "Index");
        }
    }
}
