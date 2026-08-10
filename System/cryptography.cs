using System.Security.Cryptography;
using System.Text;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
public class cryptography
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
{
    /// <summary>
    /// 计算 HMAC-SHA-256 哈希值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Json computeHmacSha256Hex(Json key, Json message)
    {
        // 将密钥和消息转换为字节数组
        byte[] keyBytes;
        if (key.Is<byte[]>())
        {
            keyBytes = key.As<byte[]>();
        }
        else if (key.IsString)
        {
            keyBytes = Encoding.UTF8.GetBytes(key.AsString);
        }
        else
        {
            throw new Exception("key must be byte[] or string");
        }

        byte[] messageBytes;
        if (message.Is<byte[]>())
        {
            messageBytes = message.As<byte[]>();
        }
        else if (message.IsString)
        {
            messageBytes = Encoding.UTF8.GetBytes(message.AsString);
        }
        else
        {
            throw new Exception("message must be byte[] or string");
        }

        // 使用 HMAC-SHA-256 计算哈希值
        using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
        {
            byte[] hashBytes = hmac.ComputeHash(messageBytes);

            // 将哈希值转换为十六进制字符串
            StringBuilder hex = new StringBuilder(hashBytes.Length * 2);
            foreach (byte b in hashBytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }
    }

    /// <summary>
    /// 计算 HMAC-SHA-256 哈希值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Json computeHmacSha256(Json key, Json message)
    {
        // 将密钥和消息转换为字节数组
        byte[] keyBytes;
        if (key.Is<byte[]>())
        {
            keyBytes = key.As<byte[]>();
        }
        else if (key.IsString)
        {
            keyBytes = Encoding.UTF8.GetBytes(key.AsString);
        }
        else
        {
            throw new Exception("key must be byte[] or string");
        }

        byte[] messageBytes;
        if (message.Is<byte[]>())
        {
            messageBytes = message.As<byte[]>();
        }
        else if (message.IsString)
        {
            messageBytes = Encoding.UTF8.GetBytes(message.AsString);
        }
        else
        {
            throw new Exception("message must be byte[] or string");
        }

        // 使用 HMAC-SHA-256 计算哈希值
        using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
        {
            return new Json(hmac.ComputeHash(messageBytes));
        }
    }

    public static Json computeSha256Hex(Json message)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] messageBytes;
            if (message.Is<byte[]>())
            {
                messageBytes = message.As<byte[]>();
            }
            else if (message.IsString)
            {
                messageBytes = Encoding.UTF8.GetBytes(message.AsString);
            }
            else
            {
                throw new Exception("message must be byte[] or string");
            }

            // 计算哈希值
            byte[] hashBytes = sha256.ComputeHash(messageBytes);

            // 将哈希值转换为十六进制字符串
            StringBuilder hex = new StringBuilder(hashBytes.Length * 2);
            foreach (byte b in hashBytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }
    }

    public static Json computeSha256(Json message)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] messageBytes;
        if (message.Is<byte[]>())
        {
            messageBytes = message.As<byte[]>();
        }
        else if (message.IsString)
        {
            messageBytes = Encoding.UTF8.GetBytes(message.AsString);
        }
        else
        {
            throw new Exception("message must be byte[] or string");
        }

        return new Json(sha256.ComputeHash(messageBytes));
    }

    public static void writeEncryptedFile(string filePath, Guid keyGuid, string content)
    {
        byte[] key = keyGuid.ToByteArray();
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV(); // 随机生成 IV

        using var fs = new FileStream(filePath, FileMode.Create);
        // 先写入 IV（16 字节）
        fs.Write(aes.IV, 0, aes.IV.Length);

        using var encryptor = aes.CreateEncryptor();
        using var cryptoStream = new CryptoStream(fs, encryptor, CryptoStreamMode.Write);
        using var writer = new StreamWriter(cryptoStream, Util.UTF8);
        writer.Write(content); // 流式写入，不会将整个明文加载到内存
    }

    public static string readEncryptedFile(string filePath, Guid keyGuid)
    {
        byte[] key = keyGuid.ToByteArray();

        using var fs = new FileStream(filePath, FileMode.Open);
        // 读取 IV（前 16 字节）
        byte[] iv = new byte[16];
        if (fs.Read(iv, 0, 16) != 16)
            throw new InvalidDataException("File is too short to contain an IV.");

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        using var cryptoStream = new CryptoStream(fs, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cryptoStream, Util.UTF8);
        return reader.ReadToEnd(); // 流式解密，但最终会将整个明文返回为字符串
    }

}
