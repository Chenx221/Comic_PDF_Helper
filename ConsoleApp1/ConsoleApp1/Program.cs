using System;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
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
            //Console.OutputEncoding = Encoding.UTF8;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            Console.OutputEncoding = Encoding.GetEncoding(936);

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
                // 输出开始处理提示信息
                Console.WriteLine($"开始处理漫画文件夹 {comicFolder}...");

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
                string pdfFileName = System.IO.Path.Combine(outputFolder, $"{System.IO.Path.GetFileName(comicFolder)}.pdf");

                // 检查PDF文件是否已经存在，若存在则跳过当前漫画文件夹的处理
                if (File.Exists(pdfFileName))
                {
                    Console.WriteLine($"PDF文件 {pdfFileName} 已存在，跳过。");
                    continue;
                }

                PdfWriter pdfWriter = new PdfWriter(pdfFileName);
                PdfDocument pdfDocument = new PdfDocument(pdfWriter);
                Document doc = new Document(pdfDocument);


                // 进度条相关变量
                int totalImageCount = imageFiles.Length;
                int currentImageIndex = 0;

                // 逐个添加图像到PDF文档
                foreach (string imageFile in imageFiles)
                {
                    // 更新进度条
                    UpdateProgressBar(currentImageIndex, totalImageCount);

                    Image image = new(ImageDataFactory.Create(imageFile));

                    // 合适的缩放
                    float widthRatio = pdfDocument.GetDefaultPageSize().GetWidth() / image.GetImageWidth();
                    float heightRatio = pdfDocument.GetDefaultPageSize().GetHeight() / image.GetImageHeight();
                    float ratio = Math.Min(widthRatio, heightRatio);
                    image.Scale(ratio,ratio);

                    // 将图像添加到页面中间
                    float x = (pdfDocument.GetDefaultPageSize().GetWidth() - image.GetImageScaledWidth()) / 2;
                    float y = (pdfDocument.GetDefaultPageSize().GetHeight() - image.GetImageScaledHeight()) / 2;
                    image.SetFixedPosition(x, y);
                    doc.Add(image);

                    GC.Collect();
                    //image = null;

                    // 在除最后一张图像外的图像后添加空白页面
                    if (currentImageIndex < totalImageCount - 1)
                    {
                        doc.Add(new AreaBreak());
                    }

                    currentImageIndex++;
                }

                // 关闭文档
                //doc.Close(); //这里会导致内存无法自动回收
                pdfDocument.Close();
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
                // Get the file names from the full file paths
                string fileNameX = System.IO.Path.GetFileName(x);
                string fileNameY = System.IO.Path.GetFileName(y);

                // Define the regex pattern to match numbers in the file names
                string pattern = @"(\d+)";

                // Get all the matches of numbers in the file names
                MatchCollection matchesX = Regex.Matches(fileNameX, pattern);
                MatchCollection matchesY = Regex.Matches(fileNameY, pattern);

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
                    int nonNumericComparison = fileNameX.IndexOf(matchesX[i].Value) - fileNameY.IndexOf(matchesY[i].Value);
                    if (nonNumericComparison != 0)
                        return nonNumericComparison;
                }

                // If the numbers are the same up to this point, compare the remaining non-numeric parts
                return fileNameX.CompareTo(fileNameY);
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

