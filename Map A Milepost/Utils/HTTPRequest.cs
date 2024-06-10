using Map_A_Milepost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using System.Net.Http;
using System.Security.Policy;
using System.Text.Json;

namespace Map_A_Milepost.Utils
{
    class HTTPRequest
    {
        public static async Task<SOEResponseModel> QuerySOE(SOEArgsModel args)
        {
            var url = new Flurl.Url("https://data.wsdot.wa.gov/arcgis/rest/services/Shared/ElcRestSOE/MapServer/exts/ElcRestSoe/Find%20Nearest%20Route%20Locations");
            Dictionary<string, string> queryParams = new Dictionary<string, string> { 
                {"referenceDate", args.ReferenceDate},
                {"coordinates", $"[{args.X},{args.Y}]"},
                {"searchRadius", args.SearchRadius},
                {"inSR", args.SR.ToString()},
                {"outSR", args.SR.ToString()},
                {"f", "json"},
            };
            url.SetQueryParams(queryParams);
            var response = await url.GetAsync();
            var soeResponse = new SOEResponseModel();
            if (response.StatusCode == 200)
            {
                string responseString = await response.ResponseMessage.Content.ReadAsStringAsync();
                var soeResponses = JsonSerializer.Deserialize<List<SOEResponseModel?>>(responseString);
                soeResponse = soeResponses?.First();
            }
            else
            {
                //handle error
            }
            return soeResponse;
        }
    }
}
