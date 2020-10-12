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


using Amazon;
using Amazon.Personalize;
using Amazon.PersonalizeEvents;
using Amazon.PersonalizeEvents.Model;
using Amazon.PersonalizeRuntime;
using Amazon.PersonalizeRuntime.Model;
using AWS.Samples.Amazon.Personalize.Demo.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User = AWS.Samples.Amazon.Personalize.Demo.Models.User;

namespace AWS.Samples.Amazon.Personalize.Demo.Support
{
    public class PersonalizedRecommendationsService
    {
        private readonly StorageService _storageService = new StorageService();

        public PersonalizedRecommendationsService(ILogger logger)
        {
            AmazonPersonalizeClient = new AmazonPersonalizeClient(RegionEndpoint.USEast1);

            AmazonPersonalizeRuntimeClient = new AmazonPersonalizeRuntimeClient(RegionEndpoint.USEast1);

            Logger = logger;
        }

        public AmazonPersonalizeClient AmazonPersonalizeClient { get; set; }

        public AmazonPersonalizeRuntimeClient AmazonPersonalizeRuntimeClient { get; set; }

        public ILogger Logger { get; }

        public async Task<MoviesRatedByUser> GetAllMoviesRatedByUser(string userId)
        {
            try
            {
                var results = new MoviesRatedByUser();

                var movies = await _storageService.GetAllMoviesRatedByUser(userId);

                var json = JsonConvert.SerializeObject(movies);

                Logger.LogInformation("GetAllMoviesRatedByUser:" + userId + ": " + json);

                List<string> itemIds = movies.Select(s => s.Item_Id).ToList();

                results.Movies = await _storageService.GetMovieData(itemIds.ToArray());

                return results;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void AddEventTracker(string movieItemId, string userId)
        {
            try
            {
                //record events
                var eventRequest = new PutEventsRequest
                {
                    TrackingId =
                        AwsParameterStoreClient.GetClickStreamTrackingId(),
                    UserId = userId, //USER_ID
                    SessionId = Guid.NewGuid().ToString() //SESSION_ID
                };

                var ev1 = new TrackingEvent { itemId = movieItemId };

                var ev = JsonConvert.SerializeObject(ev1);

                var e = new Event
                {
                    //e.EventId = "event1";
                    EventType = "click", //EVENT_TYPE
                    Properties = ev,
                    SentAt = DateTime.Now //TIMESTAMP
                };

                var events = new List<Event> { e };

                eventRequest.EventList = events;

                var amazonPersonalizeEventsClient = new AmazonPersonalizeEventsClient(RegionEndpoint.USEast1);

                amazonPersonalizeEventsClient.PutEventsAsync(eventRequest);

                Logger.LogInformation("Adding a clickstream event for userid:" + userId + ", movieitemid: " + movieItemId);
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        public async Task<SimilarItemViewModel> GetSimilarItems(string movieItemId)
        {
            SimilarItemViewModel results = null;

            try
            {
                //RELATED_ITEMS -- itemId required
                var relateditemsrequest = new GetRecommendationsRequest
                {
                    CampaignArn = AwsParameterStoreClient.GetSimsArn(), //sims-arn
                    ItemId = movieItemId,
                    NumResults = 10
                };

                var relateditemsrequestresponse = await AmazonPersonalizeRuntimeClient.GetRecommendationsAsync(relateditemsrequest);

                var relateditemsrequestrecommendedItems = relateditemsrequestresponse.ItemList;

                var json = JsonConvert.SerializeObject(relateditemsrequestrecommendedItems);

                Logger.LogInformation("GetSimilarItems for movie:" + movieItemId + ": " + json);

                results = new SimilarItemViewModel
                {

                    //selected movie
                    Movie = await _storageService.GetMovieData(movieItemId)
                };


                List<string> itemIds = relateditemsrequestrecommendedItems.Select(s => s.ItemId).ToList();

                results.SimilarItems.Movies = await _storageService.GetMovieData(itemIds.ToArray());

                return results;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<MostPopularItems> GetMostPopularItems(string userId)
        {
            MostPopularItems results = null;

            try
            {
                //USER_PERSONALIZATION -- userid required
                var request = new GetRecommendationsRequest
                {
                    CampaignArn = AwsParameterStoreClient.GetSimsMostPopularArn(), //sims-most-popular
                    UserId = userId,
                    NumResults = 10
                };

                var response = await AmazonPersonalizeRuntimeClient.GetRecommendationsAsync(request);

                var recommendedItems = response.ItemList;

                var json = JsonConvert.SerializeObject(recommendedItems);

                Logger.LogInformation("GetMostPopularItems for userid:" + userId + ": " + json);

                results = new MostPopularItems();

                List<string> itemIds = recommendedItems.Select(s => s.ItemId).ToList();

                results.Movies = await _storageService.GetMovieData(itemIds.ToArray());

                return results;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<PersonalizedRankingResults> GetPersonalizedResults(string userId, List<Movie> movies)
        {
            PersonalizedRankingResults results = null;

            try
            {
                //USER_PERSONALIZATION -- userid required
                var request = new GetPersonalizedRankingRequest //personal-ranking
                {
                    CampaignArn =
                        AwsParameterStoreClient.GetPersonalRankingArn(),
                    UserId = userId,
                    InputList = movies.Select(s => s.Id).ToList()
                };

                var response = await AmazonPersonalizeRuntimeClient.GetPersonalizedRankingAsync(request);

                var recommendedItems = response.PersonalizedRanking;

                var json = JsonConvert.SerializeObject(recommendedItems);

                Logger.LogInformation("GetPersonalizedResults:" + userId + ": " + json);

                results = new PersonalizedRankingResults();

                List<string> itemIds = recommendedItems.Select(s => s.ItemId).ToList();

                results.Movies = await _storageService.GetMovieData(itemIds.ToArray());

                return results;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public PersonalizeViewModel GetUserData(string userId)
        {
            PersonalizeViewModel results = null;

            results = new PersonalizeViewModel { User = new User() { User_Id = userId } };

            return results;
        }

        public async Task<PersonalizeViewModel> GetRecommendations(string userId)
        {
            PersonalizeViewModel results = null;

            try
            {
                //USER_PERSONALIZATION -- userid required
                var request = new GetRecommendationsRequest
                {
                    CampaignArn = AwsParameterStoreClient.GetPersonalRecommendationsArn(), //personal-recommendations-metadata
                    UserId = userId,
                    NumResults = 10
                };

                var response = await AmazonPersonalizeRuntimeClient.GetRecommendationsAsync(request);

                var recommendedItems = response.ItemList;

                var json = JsonConvert.SerializeObject(recommendedItems);

                Logger.LogInformation("GetRecommendations:" + userId + ": " + json);

                results = new PersonalizeViewModel { User = new User() { User_Id = userId } };

                //get movie thumbnail from imdb
                IList<RecommendedItems> recItems = new List<RecommendedItems>();

                List<string> itemIds = recommendedItems.Select(s => s.ItemId).ToList();

                results.RecommendedItems.Movies = await _storageService.GetMovieData(itemIds.ToArray());

                return results;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}