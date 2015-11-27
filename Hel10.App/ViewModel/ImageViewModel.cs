using System;
using System.Collections;
using System.Threading.Tasks;
using Hel10.App.Model;
using Hel10.App.ViewModel.Base;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.Collections.Generic;

namespace Hel10.App.ViewModel
{
    public class ImageViewModel:BaseViewModel
{
        public Azure.Search.Base ImageResult { get; set; }
        public Face[] FacesCollection { get; set; }
        public ICollection<ImageView> ImageCollection { get; set; }

        public ImageViewModel(Azure.Search.Base image)
        {
            this.ImageResult = image;
            this.ImageCollection=new List<ImageView>();
        }

        public async Task<Face[]> GetAge()
        {
            var faceServiceClient = new FaceServiceClient("keey");
            var face= await faceServiceClient.DetectAsync(this.ImageResult.Url,true,true,true,true);
            this.FacesCollection = face;
            var image = new ImageView
            {
                Edad = face[0].Attributes.Age.ToString(),
                Nombre = ImageResult.Nombre,
                Url = ImageResult.Url,
                Sexo =  (face[0].Attributes.Gender.Equals("male")?"Hombre":"Mujer")
            };
            
           var  urlComparation = image.Sexo.Equals("Hombre") ?
                    "http://aimworkout.com/wp-content/uploads/2014/11/Chuck_Norris.jpg" :
                    "http://www.beevoz.com/wp-content/uploads/2015/08/angelinajolie.jpg";
            var face1 = await faceServiceClient.DetectAsync(urlComparation);
           var result=await  faceServiceClient.VerifyAsync(face[0].FaceId, face1[0].FaceId);
           image.Similar= (Convert.ToInt32(result.Confidence*100)).ToString();
            ImageCollection.Add(image);
            return face;
        }
    }

   
}