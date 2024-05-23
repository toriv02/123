using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
namespace TBmobile.classes
{
    class FileMain
    {
        public int ID { get; set; }
        public long Size { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public byte[] Data { get; set; } // Включите, если нужно обрабатывать файлы напрямую
        public int Employee_ID { get; set; } // Необходимо, если вам нужен контекст пользователя
    }
}
