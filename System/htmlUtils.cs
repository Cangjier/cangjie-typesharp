using HtmlAgilityPack;
using System.Text;
using TidyHPC.Extensions;

namespace Cangjie.TypeSharp.System;

/// <summary>
/// Html工具集
/// </summary>
public class htmlUtils
{
    /// <summary>
    /// 获取Html文本
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    public static string getText(string html)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);
        return htmlDoc.DocumentNode.InnerText;
    }

    /// <summary>
    /// 获取Html摘要
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    public static string getAbstract(string html)
    {
        // 递归函数，用于从HtmlNode中提取文本
        void ExtractText(HtmlNode node, StringBuilder builder)
        {
            // 如果节点是文本节点，则添加其文本内容
            if (node.NodeType == HtmlNodeType.Text)
            {
                var text = node.InnerText.Trim();
                if (text.Length != 0)
                {
                    builder.AppendLine(node.InnerText.Trim());
                }
            }
            if (node.HasAttributes)
            {
                foreach (var attribute in node.Attributes)
                {
                    var text = attribute.Value.Trim();
                    if (text.Length != 0)
                    {
                        builder.AppendLine(attribute.Value);
                    }
                }
            }
            // 如果节点有子节点，则递归遍历它们
            if (node.HasChildNodes)
            {
                foreach (var child in node.ChildNodes)
                {
                    ExtractText(child, builder);
                }
            }
        }

        // 加载HTML文档
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        // 创建一个StringBuilder来构建最终的文本
        var builder = new StringBuilder();

        // 从文档节点开始提取文本
        ExtractText(htmlDoc.DocumentNode, builder);

        // 返回构建的文本
        return builder.ToString();
    }
}
