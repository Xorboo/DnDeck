using System;
using System.Collections.Generic;
using System.IO;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using Path = System.IO.Path;

namespace PdfPrinter
{
    class Program
    {
        const string SrcDir = "Input";
        const string DstDir = "Output";

        static void Main(string[] args)
        {
            Directory.CreateDirectory(SrcDir);
            Directory.CreateDirectory(DstDir);

            foreach (var srcFile in Directory.EnumerateFiles(SrcDir, "*.pdf"))
            {
                string dstFile = Path.Combine(DstDir, Path.GetFileName(srcFile));
                ManipulatePdf(srcFile, dstFile);
            }
        }

        static void ManipulatePdf(string src, string dst)
        {
            PdfDocument srcDoc = new PdfDocument(new PdfReader(src));
            PdfDocument dstDoc = new PdfDocument(new PdfWriter(dst));

            srcDoc.InitializeOutlines();
            dstDoc.InitializeOutlines();

            int total = srcDoc.GetNumberOfPages();
            if (total % 2 != 0)
            {
                Console.Write($"Unsupported page count: {total}");
                return;
            }

            IList<int> pages = new List<int>();
            for (int i = 1; i <= total; i += 4)
            {
                bool hasSecondPart = i + 3 <= total;
                pages.Add(i);
                pages.Add(hasSecondPart ? i + 2 : i);
                pages.Add(hasSecondPart ? i + 3 : i + 1);
                pages.Add(i + 1);
            }

            for (int i = 0; i < pages.Count; i += 2)
            {
                // Setuup page
                PageSize dstSize = PageSize.A4.Rotate();
                PdfPage page = dstDoc.AddNewPage(dstSize);
                PdfCanvas canvas = new PdfCanvas(page);

                // Scale page
                double sq2 = Math.Sqrt(2);
                AffineTransform transformationMatrix = AffineTransform.GetScaleInstance(1 / sq2, 1 / sq2);
                canvas.ConcatMatrix(transformationMatrix);

                // Add pages
                var page1 = srcDoc.GetPage(pages[i]);
                PdfFormXObject page1Copy = page1.CopyAsFormXObject(dstDoc);
                canvas.AddXObjectAt(page1Copy, 0, 0);

                if (i + 1 < pages.Count)
                {
                    var page2 = srcDoc.GetPage(pages[i + 1]);
                    PdfFormXObject page2Copy = page2.CopyAsFormXObject(dstDoc);
                    canvas.AddXObjectAt(page2Copy, PageSize.A4.GetWidth(), 0);
                }
            }

            //srcDoc.CopyPagesTo(pages, dstDoc);
            srcDoc.Close();

            dstDoc.Close();
        }
    }
}