using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client_app
{
    class PiImage
    {
        public BufferImage image;
        public long time;
    }

    class BufferImage
    {
        public string type;
        public byte[] data;
    }

}
