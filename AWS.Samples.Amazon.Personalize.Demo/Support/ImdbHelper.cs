/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * SPDX-License-Identifier: MIT-0
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify,
 * merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */


using AWS.Samples.Amazon.Personalize.Demo.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AWS.Samples.Amazon.Personalize.Demo.Support
{
    public class ImdbHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<ImdbMovieResponse> FindMovie(string imdbid)
        {
            var imdbMovieid = "tt" + imdbid;

            ImdbMovieResponse imdbObj = null;

            try
            {
                var apiKey = AwsParameterStoreClient.GetImdbApiKey();

                var url = string.Format("http://www.omdbapi.com/?apikey={0}&i={1}", apiKey, imdbMovieid);

                using (var result = await _httpClient.GetAsync(url))
                {
                    string content = await result.Content.ReadAsStringAsync();

                    imdbObj = JsonConvert.DeserializeObject<ImdbMovieResponse>(content);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return imdbObj;
        }
    }
}