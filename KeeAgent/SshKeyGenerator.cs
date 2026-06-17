// SPDX-License-Identifier: GPL-2.0-only
using System;
using System.IO;
using System.Text;
using KeePassLib.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace KeeAgent
{
  internal static class SshKeyGenerator
  {
    internal static void Generate(string comment,
      out byte[] privateKeyBytes, out byte[] publicKeyBytes)
    {
      if (comment == null) comment = string.Empty;
      if (comment.IndexOfAny(new char[] { '\r', '\n', '\0' }) >= 0) {
        throw new ArgumentException("comment must not contain newlines or NUL", "comment");
      }
      // No-arg constructor seeds from RNGCryptoServiceProvider.
      // SetSeed() ADDS entropy — it does not replace existing seeding.
      var secureRandom = new SecureRandom();
      secureRandom.SetSeed(CryptoRandom.Instance.GetRandomBytes(64));

      var privateParams = new Ed25519PrivateKeyParameters(secureRandom);
      var seed = privateParams.GetEncoded();                     // 32 bytes
      var pub  = privateParams.GeneratePublicKey().GetEncoded(); // 32 bytes

      try {
        privateKeyBytes = BuildPrivateKey(seed, pub, comment);
        publicKeyBytes  = BuildPublicKey(pub, comment);
      }
      finally {
        Array.Clear(seed, 0, seed.Length);
      }
    }

    static byte[] BuildPrivateKey(byte[] seed, byte[] pub, string comment)
    {
      var commentBytes = Encoding.UTF8.GetBytes(comment);
      var checkInt = CryptoRandom.Instance.GetRandomBytes(4);

      using (var blob = new MemoryStream()) {
        // magic
        blob.Write(Encoding.ASCII.GetBytes("openssh-key-v1"), 0, 14);
        blob.WriteByte(0);

        WriteString(blob, "none"); // cipher
        WriteString(blob, "none"); // kdf
        WriteString(blob, new byte[0]); // kdf options (empty)

        WriteUInt32(blob, 1); // number of keys

        // public key blob
        using (var pubBlob = new MemoryStream()) {
          WriteString(pubBlob, "ssh-ed25519");
          WriteString(pubBlob, pub);
          WriteString(blob, pubBlob.ToArray());
        }

        // private key blob
        using (var privBlob = new MemoryStream()) {
          privBlob.Write(checkInt, 0, 4); // check_int repeated twice
          privBlob.Write(checkInt, 0, 4);
          WriteString(privBlob, "ssh-ed25519");
          WriteString(privBlob, pub);
          var privMaterial = new byte[64]; // seed (32) || pubkey (32)
          try {
            Buffer.BlockCopy(seed, 0, privMaterial,  0, 32);
            Buffer.BlockCopy(pub,  0, privMaterial, 32, 32);
            WriteString(privBlob, privMaterial);
          }
          finally {
            Array.Clear(privMaterial, 0, privMaterial.Length);
          }
          WriteString(privBlob, commentBytes);
          for (int pad = 1; privBlob.Length % 8 != 0; pad++) {
            privBlob.WriteByte((byte)pad);
          }
          WriteString(blob, privBlob.ToArray());
        }

        var b64 = Convert.ToBase64String(blob.ToArray());
        var sb = new StringBuilder();
        sb.Append("-----BEGIN OPENSSH PRIVATE KEY-----\n");
        for (int i = 0; i < b64.Length; i += 70) {
          sb.Append(b64.Substring(i, Math.Min(70, b64.Length - i))).Append('\n');
        }
        sb.Append("-----END OPENSSH PRIVATE KEY-----\n");
        return Encoding.ASCII.GetBytes(sb.ToString());
      }
    }

    static byte[] BuildPublicKey(byte[] pub, string comment)
    {
      using (var keyBlob = new MemoryStream()) {
        WriteString(keyBlob, "ssh-ed25519");
        WriteString(keyBlob, pub);
        var b64 = Convert.ToBase64String(keyBlob.ToArray());
        return Encoding.UTF8.GetBytes("ssh-ed25519 " + b64 + " " + comment);
      }
    }

    static void WriteString(Stream s, string value)
    {
      WriteString(s, Encoding.UTF8.GetBytes(value));
    }

    static void WriteString(Stream s, byte[] value)
    {
      WriteUInt32(s, (uint)value.Length);
      s.Write(value, 0, value.Length);
    }

    static void WriteUInt32(Stream s, uint value)
    {
      s.WriteByte((byte)(value >> 24));
      s.WriteByte((byte)(value >> 16));
      s.WriteByte((byte)(value >> 8));
      s.WriteByte((byte)value);
    }
  }
}
