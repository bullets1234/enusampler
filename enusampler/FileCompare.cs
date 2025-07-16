using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;

namespace enusampler;
public static class FileComparer
{
    public static bool IsFileChanged(string filePath1, string filePath2)
    {
        byte[] hash1 = GetFileHash(filePath1);
        byte[] hash2 = GetFileHash(filePath2);

        return !StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2);
    }

    public static bool IsFileChanged(string filePath, byte[] hash)
    {
        byte[] currentHash = GetFileHash(filePath);
        return !StructuralComparisons.StructuralEqualityComparer.Equals(currentHash, hash);
    }

    public static byte[] GetFileHash(string filePath)
    {
        using (var sha256 = SHA256.Create())
        using (var stream = File.OpenRead(filePath))
        {
            return sha256.ComputeHash(stream);
        }
    }
}