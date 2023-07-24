﻿using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
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

                // 逐个添加图像到PDF文档
                foreach (string imageFile in imageFiles)
                {
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
                }

                // 关闭文档
                pdfDocument.Close();
                fileStream.Close();
                pdfWriter.Close();

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
    }
}