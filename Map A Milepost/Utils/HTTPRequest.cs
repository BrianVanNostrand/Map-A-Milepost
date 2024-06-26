﻿using Map_A_Milepost.Models;
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
using System.Windows;

namespace Map_A_Milepost.Utils
{
    class HTTPRequest
    {
        /// <summary>
        /// -   Uses Flurl to execute an HTTP Get request, with URL parameters generated using the SOE arguments passed from the MapPointViewModel and MapLineViewModel.
        /// -   Deserializes the HTTP response to an array of SOEResponseModels and parses that array to return the appropriate value, depending on if this method
        ///     was invoked by Map A Point or Map A Line.
        /// -   The initial deserialization to an array is performed because the SOE Always returns an array, even if only one response is found. This array is ordered 
        ///     based on proximity to the geometry of the argument passed in via URL params, so the first response in the array is used.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<Dictionary<string,object>> QuerySOE(SOEArgsModel args)
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
            var responseObject = new Dictionary<string, object>();
            var soeResponse = new SOEResponseModel();
            if (response.StatusCode == 200)
            {
                string responseString = await response.ResponseMessage.Content.ReadAsStringAsync();
                var soeResponses = JsonSerializer.Deserialize<List<SOEResponseModel?>>(responseString);
                if(soeResponses.Count > 0)
                {
                    responseObject.Add("soeResponse", soeResponses.First());
                    responseObject.Add("message", "success");
                }
                else
                {
                    MessageBox.Show($"No results found within {args.SearchRadius} feet of clicked point.");
                    responseObject.Add("message","failure");
                }
            }
            else
            {
                MessageBox.Show(response.ResponseMessage.ToString());
                responseObject.Add("message","failure");
            }
            return responseObject;
        }
    }
}
