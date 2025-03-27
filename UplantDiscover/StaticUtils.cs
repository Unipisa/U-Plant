using System.Web;
using System.Linq;
using UplantDiscover.Models;
using System.IO;
using System;
using UplantDiscover.Models.DB;


public static class StaticUtils
{
    

    public static string GetImgPath (string individuo, string img , string namefile,Images images)
     {

        string basepath = images.Percorso;
        string pathindividuo = Path.Combine(basepath, individuo);
        string pathimmagine = Path.Combine(pathindividuo, img);

        return pathimmagine;

    }

     public static string SetImgPath (string id, string img, Images images) {

        string basepath = images.Percorso;
        //in windows il path devo metterlo così
        string completepath = Path.Combine(basepath,id);
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
    public static class EncodeImage
    {
        public static string Base64ImageParse(string base64Content, string Tipo)
        {



            if (string.IsNullOrEmpty(base64Content))
            {
                throw new ArgumentNullException(nameof(base64Content));
            }



            int indexOfSemiColon = base64Content.IndexOf(";", StringComparison.OrdinalIgnoreCase);



            string dataLabel = base64Content.Substring(0, indexOfSemiColon);



            string contentType = dataLabel.Split(':').Last();



            var startIndex = base64Content.IndexOf("base64,", StringComparison.OrdinalIgnoreCase) + 7;



            var fileContents = base64Content.Substring(startIndex);



            var bytes = Convert.FromBase64String(fileContents);




            return $"data:{Tipo};base64,{Convert.ToBase64String(bytes)}";
        }
    }

}