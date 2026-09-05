using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public abstract class BaseEntity
    {
        public readonly string RepresentationType;

        protected BaseEntity(string representationType)
        {
            RepresentationType = representationType;
        }
    }
}
