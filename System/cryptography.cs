using System.Security.Cryptography;
using System.Text;

namespace Cangjie.TypeSharp.System;
public class cryptography
{
    /// <summary>
    /// 计算 HMAC-SHA-256 哈希值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static string computeHmacSha256Hex(string key, string message)
    {
        // 将密钥和消息转换为字节数组
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

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

}
