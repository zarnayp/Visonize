using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DupploPulse.UsImaging.Domain.Entities.Algs
{
    internal static class ImageLut
    {
        public static byte[] GetLut(int tintIndex, int graymapIndex)
        {
            byte[] lut = new byte[256*4];

            // gray scale
            double gamma = 0.8;

            switch (graymapIndex)
            {
                case 0:
                    gamma = 0.5;
                    break;
                case 1:
                    gamma = 0.7;
                    break;
                case 2:
                    gamma = 0.9;
                    break;
                case 3:
                    gamma = 1.1;
                    break;
                case 4:
                    gamma = 1.3;
                    break;
                case 5:
                    gamma = 1.5;
                    break;
                case 6:
                    gamma = 1.7;
                    break;
                case 7:
                    gamma = 1.9;
                    break;
                case 8:
                    gamma = 2.1;
                    break;
            }

            for (int i = 0; i < 256; i++)
            {
                double normalized = i / 255.0;
                double corrected = Math.Pow(normalized, gamma);
                lut[i * 4] = (byte)(corrected * 255);
            }

            // tint
            for (int i = 0; i < 256; i++)
            {
                byte source = lut[i * 4];

                byte r = source;
                byte g = source;
                byte b = source;
                byte a = 255;

                lut[i * 4 + 0] = r;
                lut[i * 4 + 1] = g;
                lut[i * 4 + 2] = b;
                lut[i * 4 + 3] = a;
            }

            return lut;
        }

    }
}
