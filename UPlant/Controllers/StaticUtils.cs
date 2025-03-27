using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UPlant.Models;
using Microsoft.Extensions.Options;
using Aspose.Drawing;
using Aspose.Drawing.Imaging;
using Aspose.Drawing.Drawing2D;

namespace UPlant.Controllers
{
    public static class StaticUtils
    {
        private static IOptions<PathFileStatic> _opt;

        public static void Initialize(IOptions<PathFileStatic> opt)
        {
            _opt = opt;
        }

        public static string GetImgPath(string individuo, string img, string namefile, string basepath)
        {
            
            string pathindividuo = Path.Combine(basepath, individuo);
            int posizione = namefile.LastIndexOf(".");
            string estensione = namefile.Substring(posizione);
            string pathimmagine = Path.Combine(pathindividuo, img + estensione);

            return pathimmagine;

        }
        public static string GetThumbImgPath(string individuo, string img, string namefile, string basepath)
        {
            
            string pathindividuo = Path.Combine(basepath, individuo);
            string paththumbindividuo = Path.Combine(pathindividuo, "thumb");
            int posizione = namefile.LastIndexOf(".");
            string estensione = namefile.Substring(posizione);
            string pathimmagine = Path.Combine(paththumbindividuo, img + estensione);

            return pathimmagine;

        }

        public static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings.
            try
            {
                return Regex.Replace(strIn, @"[^ \w\.@-]", " ",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            // If we timeout when replacing invalid characters,
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return strIn;
            }
        }



        public static string SetImgPath(string id, string img, string basepath)
        {

           
            //in windows il path devo metterlo così
            string completepath = Path.Combine(basepath, id);
            try
            {
                if (!Directory.Exists(completepath))
                {
                    Directory.CreateDirectory(completepath);
                }
            }
            catch (IOException ioex)
            {
                Console.WriteLine(ioex.Message);
            }

            completepath = Path.Combine(completepath, Path.GetFileName(img));
            return completepath;
        }



        public static string SetThumbImgPath(string id, string img, string basepath)
        {

           
            //in windows il path devo metterlo così
            string completepath = Path.Combine(basepath, id);
            string completethumbpath = Path.Combine(completepath, "thumb");
            try
            {
                if (!Directory.Exists(completethumbpath))
                {
                    Directory.CreateDirectory(completethumbpath);
                }
            }
            catch (IOException ioex)
            {
                Console.WriteLine(ioex.Message);
            }

            completethumbpath = Path.Combine(completethumbpath, Path.GetFileName(img));
            return completethumbpath;
        }


        public static string GeneraSuccessivo(string successivo)
        {

            if (Int32.TryParse(successivo, out int value)) {
                successivo = (Int32.Parse(successivo) + 1).ToString();
                return successivo;
            } else {
                successivo = "";
                return successivo;
            }
                
               

        }


            public static void ResizeAndSave(string FileNameInput, string fileNamethumb, int maxSideSize, bool makeItSquare)
        {
            int newWidth2;
            int newHeight2;
            
            Aspose.Drawing.Image image = new Bitmap(FileNameInput);

            int oldWidth = image.Width;
            int oldHeight = image.Height;
            // Bitmap newImage;

            Aspose.Drawing.Image result = null;


            if (makeItSquare)
            {
                if (image.Width != maxSideSize || image.Height != maxSideSize)
                {
                    using (var target = new Bitmap(maxSideSize, maxSideSize))
                    {
                        using (var g = Aspose.Drawing.Graphics.FromImage(target))
                        {
                            g.CompositingQuality = CompositingQuality.HighQuality;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = SmoothingMode.HighQuality;

                            // Scaling
                            float scaling;
                            float scalingY = (float)image.Height / maxSideSize;
                            float scalingX = (float)image.Width / maxSideSize;
                            if (scalingX < scalingY) scaling = scalingX; else scaling = scalingY;

                            int newWidth = (int)(image.Width / scaling);
                            int newHeight = (int)(image.Height / scaling);

                            // Correct float to int rounding
                            if (newWidth < maxSideSize) newWidth = maxSideSize;
                            if (newHeight < maxSideSize) newHeight = maxSideSize;

                            // See if image needs to be cropped
                            int shiftX = 0;
                            int shiftY = 0;

                            if (newWidth > maxSideSize)
                            {
                                shiftX = (newWidth - maxSideSize) / 2;
                            }

                            if (newHeight > maxSideSize)
                            {
                                shiftY = (newHeight - maxSideSize) / 2;
                            }

                            // Draw image
                            g.DrawImage(image, -shiftX, -shiftY, newWidth, newHeight);
                        }

                        result = new Bitmap(target);
                    }
                }
                //vecchio modo di creazione thumb
                /* int smallerSide = oldWidth >= oldHeight ? oldHeight : oldWidth;
                double coeficient = maxSideSize / (double)smallerSide;
                newWidth = Convert.ToInt32(coeficient * oldWidth);
                newHeight = Convert.ToInt32(coeficient * oldHeight);
                Bitmap tempImage = new Bitmap(image, newWidth, newHeight);
                int cropX = (newWidth - maxSideSize) / 2;
                int cropY = (newHeight - maxSideSize) / 2;
                newImage = new Bitmap(maxSideSize, maxSideSize);
                Graphics tempGraphic = Graphics.FromImage(newImage);
                tempGraphic.SmoothingMode = SmoothingMode.AntiAlias;
                tempGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                tempGraphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                // tempGraphic.DrawImage(tempImage, new Rectangle(0, 0, maxSideSize, maxSideSize), cropX, cropY, maxSideSize, maxSideSize, GraphicsUnit.Pixel);
                tempGraphic.DrawImage(tempImage, new Rectangle(0, 0, maxSideSize, maxSideSize), cropX, cropY, maxSideSize, maxSideSize,GraphicsUnit.Point);
            */

            }
            else
            {
                int maxSide = oldWidth >= oldHeight ? oldWidth : oldHeight;

                if (maxSide > maxSideSize)
                {
                    double coeficient = maxSideSize / (double)maxSide;
                    newWidth2 = Convert.ToInt32(coeficient * oldWidth);
                    newHeight2 = Convert.ToInt32(coeficient * oldHeight);
                }
                else
                {
                    newWidth2 = oldWidth;
                    newHeight2 = oldHeight;
                }
                result = new Bitmap(image, newWidth2, newHeight2);
            }




            //var extension = Path.GetExtension(FileNameInput);
            int posizione = FileNameInput.LastIndexOf(".");
            string formato = FileNameInput.Substring(posizione + 1);
            if (formato.ToLower() == "png")
            {
                result.Save(fileNamethumb, ImageFormat.Png);
            }
            else if (formato.ToLower() == "jpeg" || formato == "jpg")
            {
                result.Save(fileNamethumb, ImageFormat.Jpeg);
            }
            else if (formato.ToLower() == "heif" || formato == "hevc" || formato == "heic")
            {
                result.Save(fileNamethumb, ImageFormat.Jpeg);
            }



            /* else if (ImageFormat.Jpeg.Equals(formato))
             {
                 newImage.Save(fileNamethumb, ImageFormat.Jpeg);
             }*/


            image.Dispose();
            result.Dispose();
        }


    }
}