using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace Bot.Core {
    public class FilmService {
        
        private Uri BaseURL = new Uri("http://www.omdbapi.com/");
        
        public async Task<OMDBRes> GetFilmInfo(string filmName){
            var filmInfo = await MakeRequest(filmName);
            return JsonConvert.DeserializeObject<OMDBRes>(filmInfo);
        }
        
        private async Task<string> MakeRequest(string searchString){
            //http://www.omdbapi.com/?t=the+green+mile&y=&plot=short&r=json
            
            var client = new HttpClient();
            client.BaseAddress = BaseURL;
            var response = await client.GetAsync("?t=" + searchString);
            var resString = await response.Content.ReadAsStringAsync();
            return resString;
        }
    }
}