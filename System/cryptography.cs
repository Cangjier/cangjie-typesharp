using System.Security.Cryptography;
using System.Text;
using TidyHPC.LiteJson;

namespace Cangjie.TypeSharp.System;
public class cryptography
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
        if(key.Is<byte[]>())
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

}
