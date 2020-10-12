using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using AWS.Samples.Amazon.Personalize.Demo.Models;
using Newtonsoft.Json;
using System;
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


using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.Samples.Amazon.Personalize.Demo.Support
{
    public class StorageService
    {
        private readonly IAmazonS3 _client;

        private readonly ImdbHelper _imdbHelper = new ImdbHelper();

        public StorageService()
        {
            _client = new AmazonS3Client(RegionEndpoint.USEast1);
        }

        public async Task<List<UserRating>> GetAllMoviesRatedByUser(string userId)
        {
            try
            {
                var query = string.Format("select * from S3Object s where s.USER_ID ='{0}' LIMIT 15", userId);

                var response = await _client.SelectObjectContentAsync(new SelectObjectContentRequest
                {
                    Bucket = AwsParameterStoreClient.GetS3BucketKey(),
                    Key = "ratings/ratings.csv",
                    ExpressionType = ExpressionType.SQL,
                    Expression = query,
                    InputSerialization = new InputSerialization
                    {
                        CSV = new CSVInput
                        {
                            FileHeaderInfo = FileHeaderInfo.Use,
                            FieldDelimiter = ","
                        }
                    },
                    OutputSerialization = new OutputSerialization
                    {
                        JSON = new JSONOutput()
                    }
                });

                var payload = response.Payload;

                var result = "";

                using (payload)
                {
                    foreach (var ev in payload)
                        if (ev is RecordsEvent records)
                            using (var reader = new StreamReader(records.Payload, Encoding.UTF8))
                            {
                                result = reader.ReadToEnd();
                            }
                }

                var umresult = result.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                var userResult = new List<UserRating>();

                foreach (var o in umresult) userResult.Add(JsonConvert.DeserializeObject<UserRating>(o));

                return userResult;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Link> GetImdbMovieId(string movieId)
        {
            try
            {
                var query = $"select * from S3Object s where s.movieId ='{movieId}'";

                var response = await _client.SelectObjectContentAsync(new SelectObjectContentRequest
                {
                    Bucket = AwsParameterStoreClient.GetS3BucketKey(),
                    Key = "links/links.csv",
                    ExpressionType = ExpressionType.SQL,
                    Expression = query,
                    InputSerialization = new InputSerialization
                    {
                        CSV = new CSVInput
                        {
                            FileHeaderInfo = FileHeaderInfo.Use,
                            FieldDelimiter = ","
                        }
                    },
                    OutputSerialization = new OutputSerialization
                    {
                        JSON = new JSONOutput()
                    }
                });

                var payload = response.Payload;

                var result = "";

                using (payload)
                {
                    foreach (var ev in payload)
                        if (ev is RecordsEvent records)
                            using (var reader = new StreamReader(records.Payload, Encoding.UTF8))
                            {
                                result = reader.ReadToEnd();
                            }
                }

                var movieResult = JsonConvert.DeserializeObject<Link>(result);

                return movieResult;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<Link>> GetImdbMovieIds(string[] movieIds)
        {
            try
            {
                string movieIds1 = string.Join(',', movieIds);

                string rmovieIds = string.Join(",", movieIds1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());

                var query = $"select * from S3Object s where s.movieId IN ({rmovieIds})";

                var response = await _client.SelectObjectContentAsync(new SelectObjectContentRequest
                {
                    Bucket = AwsParameterStoreClient.GetS3BucketKey(),
                    Key = "links/links.csv",
                    ExpressionType = ExpressionType.SQL,
                    Expression = query,
                    InputSerialization = new InputSerialization
                    {
                        CSV = new CSVInput
                        {
                            FileHeaderInfo = FileHeaderInfo.Use,
                            FieldDelimiter = ","
                        }
                    },
                    OutputSerialization = new OutputSerialization
                    {
                        JSON = new JSONOutput()
                    }
                });

                var payload = response.Payload;

                List<Link> results = new List<Link>();

                using (payload)
                {
                    foreach (var ev in payload)
                        if (ev is RecordsEvent records)
                            using (var reader = new StreamReader(records.Payload, Encoding.UTF8))
                            {
                                string s = reader.ReadToEnd();

                                var jsonReader = new JsonTextReader(new StringReader(s))
                                {
                                    SupportMultipleContent = true
                                };

                                var jsonSerializer = new JsonSerializer();

                                while (jsonReader.Read())
                                {
                                    results.Add(jsonSerializer.Deserialize<Link>(jsonReader));
                                }
                            }
                }

                return results;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Item> GetItemMovieData(string movieId)
        {
            try
            {
                var query = $"select * from S3Object s where s.item_Id ='{movieId}'";

                var response = await _client.SelectObjectContentAsync(new SelectObjectContentRequest
                {
                    Bucket = AwsParameterStoreClient.GetS3BucketKey(),
                    Key = "items/items.csv",
                    ExpressionType = ExpressionType.SQL,
                    Expression = query,
                    InputSerialization = new InputSerialization
                    {
                        CSV = new CSVInput
                        {
                            FileHeaderInfo = FileHeaderInfo.Use,
                            FieldDelimiter = ","
                        }
                    },
                    OutputSerialization = new OutputSerialization
                    {
                        JSON = new JSONOutput()
                    }
                });

                var payload = response.Payload;

                var result = "";

                using (payload)
                {
                    foreach (var ev in payload)
                        if (ev is RecordsEvent records)
                            using (var reader = new StreamReader(records.Payload, Encoding.UTF8))
                            {
                                result = reader.ReadToEnd();
                            }
                }

                var userResult = JsonConvert.DeserializeObject<Item>(result);

                return userResult;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<Item>> GetItemMovieData(string[] movieIds)
        {
            try
            {
                string movieIds1 = string.Join(',', movieIds);

                string rmovieIds = string.Join(",", movieIds1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());

                var query = $"select * from S3Object s where s.item_Id IN ({rmovieIds})";

                var response = await _client.SelectObjectContentAsync(new SelectObjectContentRequest
                {
                    Bucket = AwsParameterStoreClient.GetS3BucketKey(),
                    Key = "items/items.csv",
                    ExpressionType = ExpressionType.SQL,
                    Expression = query,
                    InputSerialization = new InputSerialization
                    {
                        CSV = new CSVInput
                        {
                            FileHeaderInfo = FileHeaderInfo.Use,
                            FieldDelimiter = ","
                        }
                    },
                    OutputSerialization = new OutputSerialization
                    {
                        JSON = new JSONOutput()
                    }
                });

                var payload = response.Payload;

                List<Item> results = new List<Item>();

                using (payload)
                {
                    foreach (var ev in payload)
                        if (ev is RecordsEvent records)
                            using (var reader = new StreamReader(records.Payload, Encoding.UTF8))
                            {
                                string s = reader.ReadToEnd();

                                var jsonReader = new JsonTextReader(new StringReader(s))
                                {
                                    SupportMultipleContent = true
                                };

                                var jsonSerializer = new JsonSerializer();

                                while (jsonReader.Read())
                                {
                                    results.Add(jsonSerializer.Deserialize<Item>(jsonReader));
                                }
                            }
                }

                return results;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<Movie>> GetMovieData(string[] movieIds)
        {
            try
            {
                List<Movie> results = new List<Movie>();

                var imdbMovieIds = await GetImdbMovieIds(movieIds);

                foreach (var imdbMovieId in imdbMovieIds)
                {
                    var imdbResponse = await _imdbHelper.FindMovie(imdbMovieId.ImdbId);

                    var result = new Movie
                    {
                        Id = imdbMovieId.MovieId,
                        ImdbId = imdbResponse.ImdbId,
                        Title = imdbResponse.Title,
                        Rating = imdbResponse.ImdbRating,
                        Year = imdbResponse.Year,
                        PosterUrl = imdbResponse.Poster,
                        Genre = imdbResponse.Genre
                    };

                    results.Add(result);
                }

                return results;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Movie> GetMovieData(string movieId)
        {
            try
            {
                var linkResult = await GetImdbMovieId(movieId);

                var imdbResponse = await _imdbHelper.FindMovie(linkResult.ImdbId);

                var result = new Movie
                {
                    Id = movieId,
                    ImdbId = imdbResponse.ImdbId,
                    Title = imdbResponse.Title,
                    Rating = imdbResponse.ImdbRating,
                    Year = imdbResponse.Year,
                    PosterUrl = imdbResponse.Poster,
                    Genre = imdbResponse.Genre
                };

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}