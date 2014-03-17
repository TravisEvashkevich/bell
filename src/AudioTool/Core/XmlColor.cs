using System;
using System.Windows.Media;
using System.Xml.Serialization;

namespace AudioTool.Core
{
    public class XmlColor
    {
        private Color color_ = Colors.Black;

        public XmlColor() { }
        public XmlColor(Color c) { color_ = c; }


        public Color ToColor()
        {
            return color_;
        }

        public void FromColor(Color c)
        {
            color_ = c;
        }

        public static implicit operator Color(XmlColor x)
        {
            return x.ToColor();
        }

        public static implicit operator XmlColor(Color c)
        {
            return new XmlColor(c);
        }

        [XmlAttribute]
        public string Web
        {
            get { return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color_.A, color_.R, color_.G, color_.B); }
            set
            {
                try
                {
                    //if (Alpha == 0xFF) // preserve named color value if possible
                        color_ = (Color)ColorConverter.ConvertFromString(value);
                    //else
                    //    color_ = Color.FromArgb(Alpha, (Color)ColorConverter.ConvertFromString(value));
                }
                catch (Exception)
                {
                    color_ = Colors.Black;
                }
            }
        }

        //[XmlAttribute]
        //public byte Alpha
        //{
        //    get { return color_.A; }
        //    set
        //    {
        //        if (value != color_.A) // avoid hammering named color if no alpha change
        //            color_ = Color.FromArgb(value, color_);
        //    }
        //}

        //public bool ShouldSerializeAlpha() { return Alpha < 0xFF; }
    }
}
