using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            // 设置控制台的输出编码为UTF-8
            Console.OutputEncoding = Encoding.UTF8;

            // 获取上级文件夹路径
            Console.Write("请输入漫画所在的根路径：");
            string parentFolder = Console.ReadLine();

            // 确保上级文件夹存在
            if (!Directory.Exists(parentFolder))
            {
                Console.WriteLine("根路径不存在。");
                PauseBeforeExit();
                return;
            }

            // 获取保存 PDF 的根文件夹路径
            Console.Write("请输入保存PDF位置：");
            string outputFolder = Console.ReadLine();

            // 确保PDF文件夹存在
            if (!Directory.Exists(outputFolder))
            {
                Console.WriteLine("PDF文件夹不存在。");
                PauseBeforeExit();
                return;
            }

            // 获取所有漫画文件夹
            string[] comicFolders = Directory.GetDirectories(parentFolder);

            // 循环处理每个漫画文件夹
            foreach (string comicFolder in comicFolders)
            {
                // 确保漫画文件夹中包含图像文件
                string[] imageFiles = Directory.GetFiles(comicFolder, "*.*", SearchOption.AllDirectories)
                                             .Where(file => file.ToLower().EndsWith(".jpg") || file.ToLower().EndsWith(".png"))
                                             .ToArray();

                if (imageFiles.Length == 0)
                {
                    Console.WriteLine($"在漫画文件夹 {comicFolder} 中未找到任何图像文件 (.jpg or .png)。");
                    continue; // 继续处理下一个漫画文件夹
                }

                // Sort the imageFiles using the NaturalSortComparer
                Array.Sort(imageFiles, new NaturalSortComparer());

                // 生成 PDF 文件名为漫画文件夹的名称
                string pdfFileName = Path.Combine(outputFolder, $"{Path.GetFileName(comicFolder)}.pdf");

                // 检查PDF文件是否已经存在，若存在则跳过当前漫画文件夹的处理
                if (File.Exists(pdfFileName))
                {
                    Console.WriteLine($"PDF文件 {pdfFileName} 已存在，跳过。");
                    continue;
                }

                // 创建一个新的PDF文档
                Document pdfDocument = new Document();
                FileStream fileStream = File.Create(pdfFileName);
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDocument, fileStream);

                // 打开文档
                pdfDocument.Open();

                // 进度条相关变量
                int totalImageCount = imageFiles.Length;
                int currentImageIndex = 0;

                // 逐个添加图像到PDF文档
                foreach (string imageFile in imageFiles)
                {
                    // 更新进度条
                    UpdateProgressBar(currentImageIndex, totalImageCount);

                    // 读取图像文件
                    iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageFile);

                    // 计算图像缩放比例，使其适合于PDF页面
                    float widthRatio = pdfDocument.PageSize.Width / image.Width;
                    float heightRatio = pdfDocument.PageSize.Height / image.Height;
                    float ratio = Math.Min(widthRatio, heightRatio);

                    // 设置图像缩放比例
                    image.ScalePercent(ratio * 100);

                    // 新建一个页面
                    pdfDocument.NewPage();

                    // 将图像添加到页面中间
                    float x = (pdfDocument.PageSize.Width - image.ScaledWidth) / 2;
                    float y = (pdfDocument.PageSize.Height - image.ScaledHeight) / 2;
                    image.SetAbsolutePosition(x, y);

                    // 将图像添加到页面
                    pdfDocument.Add(image);

                    currentImageIndex++;
                }

                // 关闭文档
                pdfDocument.Close();
                fileStream.Close();
                pdfWriter.Close();

                Console.WriteLine(); // 换行，确保进度条后面不会被覆盖
                Console.WriteLine($"漫画文件夹 {comicFolder} 转换完成。");
            }

            PauseBeforeExit();
        }

        // 暂停程序，等待用户输入任意键后退出
        private static void PauseBeforeExit()
        {
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }

        // Custom Natural Sort Comparer
        public class NaturalSortComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                // Define the regex pattern to match numbers in the strings
                string pattern = @"(\d+)";

                // Get all the matches of numbers in the strings
                MatchCollection matchesX = Regex.Matches(x, pattern);
                MatchCollection matchesY = Regex.Matches(y, pattern);

                // Compare the matches one by one
                int matchCount = Math.Min(matchesX.Count, matchesY.Count);
                for (int i = 0; i < matchCount; i++)
                {
                    int numX = int.Parse(matchesX[i].Value);
                    int numY = int.Parse(matchesY[i].Value);

                    int numComparison = numX.CompareTo(numY);
                    if (numComparison != 0)
                        return numComparison;

                    // Compare the non-numeric parts between the matched numbers
                    int nonNumericComparison = x.IndexOf(matchesX[i].Value) - y.IndexOf(matchesY[i].Value);
                    if (nonNumericComparison != 0)
                        return nonNumericComparison;
                }

                // If the numbers are the same up to this point, compare the remaining non-numeric parts
                return x.CompareTo(y);
            }
        }

        // 更新进度条
        private static void UpdateProgressBar(int current, int total)
        {
            int progressBarWidth = 50;
            int progress = (int)Math.Round((double)current / total * progressBarWidth);

            // 检查是否是最后一次更新
            if (current == total - 1)
                progress = progressBarWidth; // 将进度设置为进度条的最大宽度

            string progressBar = "[" + new string('=', progress) + new string(' ', progressBarWidth - progress) + "]";
            Console.Write($"\r{progressBar} {current + 1}/{total}");
        }

    }
}

