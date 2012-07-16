using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UploadModal.ViewModel
{
    public class EditorInputViewModel
    {
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Imagem { get; set; }
        public string Rotulo { get; set; }
    }
}