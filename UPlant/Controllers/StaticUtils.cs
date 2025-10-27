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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Image = System.Drawing.Image;

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

            if (Int32.TryParse(successivo, out int value))
            {
                successivo = (Int32.Parse(successivo) + 1).ToString();
                return successivo;
            }
            else
            {
                successivo = "";
                return successivo;
            }



        }
        public static int GeneraSuccessivo(int successivo)
        {
            _ = successivo > 0 ? successivo++ : successivo;
            return successivo;
        }


        public static void ResizeAndSave(string FileNameInput, string fileNamethumb, int maxSideSize, bool makeItSquare)
        {


            Image image = new Bitmap(FileNameInput);
            Bitmap result = makeItSquare ? ResizeSquare(image, maxSideSize) : ResizeByLongestSide(image, maxSideSize);
            var format = ResolveImageFormat(FileNameInput);
            result.Save(fileNamethumb, format);
        }



        private static Bitmap ResizeSquare(Image image, int maxSideSize)
        {
            if (image.Width == maxSideSize && image.Height == maxSideSize)
            {
                return new Bitmap(image);
            }

            var target = new Bitmap(maxSideSize, maxSideSize);

            using (var graphics = Graphics.FromImage(target))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                float scalingX = (float)image.Width / maxSideSize;
                float scalingY = (float)image.Height / maxSideSize;
                float scaling = Math.Min(scalingX, scalingY);
                if (scaling == 0)
                {
                    scaling = 1;
                }

                int newWidth = (int)Math.Round(image.Width / scaling);
                int newHeight = (int)Math.Round(image.Height / scaling);


                if (newWidth < maxSideSize)
                {
                    newWidth = maxSideSize;
                }
                if (newHeight < maxSideSize)
                {
                    newHeight = maxSideSize;
                }
                int shiftX = Math.Max(0, (newWidth - maxSideSize) / 2);
                int shiftY = Math.Max(0, (newHeight - maxSideSize) / 2);

                graphics.DrawImage(image, -shiftX, -shiftY, newWidth, newHeight);
            }

            return target;
        }





        //var extension = Path.GetExtension(FileNameInput);
        private static Bitmap ResizeByLongestSide(Image image, int maxSideSize)
        {
            int oldWidth = image.Width;
            int oldHeight = image.Height;
            int maxSide = Math.Max(oldWidth, oldHeight);

            if (maxSide <= maxSideSize)
            {
                return new Bitmap(image);
            }

            double coefficient = maxSideSize / (double)maxSide;
            int newWidth = (int)Math.Round(oldWidth * coefficient);
            int newHeight = (int)Math.Round(oldHeight * coefficient);

            return new Bitmap(image, newWidth, newHeight);
        }

        /* else if (ImageFormat.Jpeg.Equals(formato))
         {
             newImage.Save(fileNamethumb, ImageFormat.Jpeg);
         }*/
        private static ImageFormat ResolveImageFormat(string fileNameInput)
        {
            var extension = Path.GetExtension(fileNameInput)?.TrimStart('.').ToLowerInvariant();

            return extension switch
            {
                "png" => ImageFormat.Png,
                "jpeg" => ImageFormat.Jpeg,
                "jpg" => ImageFormat.Jpeg,
                "heif" => ImageFormat.Jpeg,
                "hevc" => ImageFormat.Jpeg,
                "heic" => ImageFormat.Jpeg,
                _ => ImageFormat.Jpeg,
            };


        }
    }
}