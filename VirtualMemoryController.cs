using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1
{
    public class Page
    {
        public int PageNumber { get; set; }
        public bool IsModified { get; set; }
        public byte[] Data { get; set; }
        public byte[] BitMap { get; set; }

        public Page(int size)
        {
            Data = new byte[size];
            BitMap = new byte[size / sizeof(int)];
        }

        public int Read(int index)
        {
            int offset = index * sizeof(int);
            return BitConverter.ToInt32(Data, offset);
        }

        public void Write(int index, int value)
        {
            int offset = index * sizeof(int);
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, Data, offset, bytes.Length);
            BitMap[index / 8] |= (byte)(1 << (index % 8));
            IsModified = true;
        }
    }

    public class VirtualMemoryController
    {
        private const int PageSize = 512;
        private const int PageCount = 3;  // Количество страниц в оперативной памяти
        private const string Signature = "VM";
        private string fileName;
        private FileStream swapFile;
        private Page[] pages;
        private int arraySize;

        public VirtualMemoryController(string fileName, int arraySize)
        {
            this.fileName = fileName;
            this.arraySize = arraySize;

            InitializeSwapFile();
            pages = new Page[PageCount];

            for (int i = 0; i < PageCount; i++)
            {
                pages[i] = new Page(PageSize);
            }
        }

        private void InitializeSwapFile()
        {
            if (!File.Exists(fileName))
            {
                using (swapFile = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    byte[] sig = System.Text.Encoding.ASCII.GetBytes(Signature);
                    swapFile.Write(sig, 0, sig.Length);
                    byte[] buffer = new byte[PageSize];
                    int totalPages = (arraySize * sizeof(int) + PageSize - 1) / PageSize;
                    for (int i = 0; i < totalPages; i++)
                    {
                        swapFile.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            else
            {
                swapFile = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
            }
        }

        private int GetPageIndex(int index)
        {
            return index / PageSize;
        }

        public int Read(int index)
        {
            int pageIndex = GetPageIndex(index);
            Page page = LoadPage(pageIndex);
            return page.Read(index % (PageSize / sizeof(int)));
        }

        public void Write(int index, int value)
        {
            int pageIndex = GetPageIndex(index);
            Page page = LoadPage(pageIndex);
            page.Write(index % (PageSize / sizeof(int)), value);
        }

        private Page LoadPage(int pageIndex)
        {
            Page page = pages[pageIndex % PageCount];

            if (page.PageNumber != pageIndex)
            {
                if (page.IsModified)
                {
                    SavePage(page);
                }
                LoadPageFromDisk(page, pageIndex);
            }

            return page;
        }

        private void SavePage(Page page)
        {
            swapFile.Seek(page.PageNumber * PageSize + Signature.Length, SeekOrigin.Begin);
            byte[] data = page.Data;
            swapFile.Write(data, 0, data.Length);
            page.IsModified = false;
        }

        private void LoadPageFromDisk(Page page, int pageIndex)
        {
            swapFile.Seek(pageIndex * PageSize + Signature.Length, SeekOrigin.Begin);
            byte[] data = new byte[PageSize];
            swapFile.Read(data, 0, data.Length);
            page.Data = data;
            page.PageNumber = pageIndex;
        }
    }

}
