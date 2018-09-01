using System;
using System.Diagnostics;
using System.Drawing;

namespace GhostParty
{
    public class ImageResource
    {

        public static Bitmap CombineImages(Bitmap[] imgs)
        {
            if (imgs.Length <= 0)
            {
                return null;
            }
            else if (imgs.Length == 1)
            {
                return imgs[0];
            }
            else if (imgs.Length <= 9)
            {
                Bitmap tmpImage;
                Bitmap newImage;

                if (imgs.Length <= 4)
                {
                    tmpImage = new Bitmap(imgs[0], imgs[0].Height * 2, imgs[0].Width * 2);
                }
                else
                {
                    tmpImage = new Bitmap(imgs[0], imgs[0].Height * 3, imgs[0].Width * 3);
                }

                ClearImage(Color.White, tmpImage);

                // draw the images full scale onto the new image
                if (imgs.Length >= 1) ReplicateImage(0, 0, ref tmpImage, imgs[0]);
                if (imgs.Length >= 2) ReplicateImage(0, imgs[0].Width, ref tmpImage, imgs[1]);
                if (imgs.Length >= 3) ReplicateImage(imgs[0].Height, 0, ref tmpImage, imgs[2]);
                if (imgs.Length >= 4) ReplicateImage(imgs[0].Height, imgs[0].Width, ref tmpImage, imgs[3]);
                if (imgs.Length >= 5) ReplicateImage(imgs[0].Height * 2, 0, ref tmpImage, imgs[4]);
                if (imgs.Length >= 6) ReplicateImage(imgs[0].Height * 2, imgs[0].Width, ref tmpImage, imgs[5]);
                if (imgs.Length >= 7) ReplicateImage(0, imgs[0].Width * 2, ref tmpImage, imgs[6]);
                if (imgs.Length >= 8) ReplicateImage(imgs[0].Height, imgs[0].Width * 2, ref tmpImage, imgs[7]);
                if (imgs.Length == 9) ReplicateImage(imgs[0].Height * 2, imgs[0].Width * 2, ref tmpImage, imgs[8]);

                // now shrink this image back to the original size
                newImage = new Bitmap(tmpImage, imgs[0].Height, imgs[0].Width);

                return newImage;
            }
            else
            {
                Debug.Assert(false, "Not intended to have more than 9 images compressed into 1");
                return imgs[0];
            }
        }

        private static void ReplicateImage(int sHeight, int sWidth, ref Bitmap desImg, Bitmap srcImg)
        {
            // take the srcImg and set all the appropriate pixels
            //  in desImg starting at sHeightXsWidth

            for (int h = 0; h < srcImg.Height; h++)
            {
                for (int w = 0; w < srcImg.Width; w++)
                {
                    desImg.SetPixel(sHeight + h, sWidth + w, srcImg.GetPixel(h, w));
                }
            }
        }

        private static void ClearImage(Color bc, Bitmap img)
        {
            for (int h = 0; h < img.Height; h++)
            {
                for (int w = 0; w < img.Width; w++)
                {
                    img.SetPixel(h, w, bc);
                }
            }
        }
    }
}