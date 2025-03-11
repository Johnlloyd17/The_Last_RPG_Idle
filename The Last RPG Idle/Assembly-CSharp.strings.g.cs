using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

    class e_
    {        
        public static e_ E_ = new e_();
        public string this[int y_] 
        { 
            get 
            { 
                if (w_ == null)
                {
                    using (var Q9_ = new MemoryStream(Convert.FromBase64String(@"AAEAAAD/////AQAAAAAAAAARAQAAAAcAAAAGAgAAABlSaWdpZEJvZHkyRCBpcyBub3QgZm91bmQuBgMAAAAMQ29tYm9Db3VudGVyBgQAAAAWQW5pbWF0b3IgaXMgbm90IGZvdW5kLgYFAAAAH0NhcHN1bGVDb2xsaWRlcjJEIGlzIG5vdCBmb3VuZC4GBgAAABZXYWxsIGNoZWNrIGlzIGlnbm9yZWQuBgcAAAANIHdhcyBkYW1hZ2VkIQYIAAAAGEVudGVyZWQgY29sbGlzaW9uIHdpdGg6IAs=")))
                        w_ = (string[])new BinaryFormatter().Deserialize(Q9_); 
                }
                return w_[y_]; 
            } 
        }
        private string[] w_;
    }
