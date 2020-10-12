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


using System;
using AWS.Samples.Amazon.Personalize.Demo.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AWS.Samples.Amazon.Personalize.Demo.Support
{
    public class HomeControllerService
    {
        private readonly ILogger _logger;

        private readonly StorageService _storageService = new StorageService();

        public HomeControllerService(ILogger logger)
        {
            _logger = logger;

            RandomUserIdGenerator = new RandomUserIdGenerator();
            PersonalizedRecommendationsService = new PersonalizedRecommendationsService(_logger);
        }

        public RandomUserIdGenerator RandomUserIdGenerator { get; set; }

        public PersonalizedRecommendationsService PersonalizedRecommendationsService { get; set; }

        public async Task<PersonalizeViewModel> GetRecommendations(string userid)
        {
            try
            {
                var randomUserIdGenerator = new RandomUserIdGenerator();

                if (string.IsNullOrEmpty(userid)) userid = randomUserIdGenerator.GetUser();

                PersonalizeViewModel results = new PersonalizeViewModel();

                //user recommendations
                _logger.LogInformation("START: getting recommended items");
                var recommendations = await PersonalizedRecommendationsService.GetRecommendations(userid);
                results.User = recommendations.User;
                results.RecommendedItems = recommendations.RecommendedItems;
                _logger.LogInformation("END: getting recommended items");

                //movies rated by user
                _logger.LogInformation("START: getting movies rated by user");
                results.MoviesRatedByUser = await PersonalizedRecommendationsService.GetAllMoviesRatedByUser(userid);
                _logger.LogInformation("END: getting movies rated by user");

                //most popular items
                _logger.LogInformation("START: getting most popular items");
                results.MostPopularItems = await PersonalizedRecommendationsService.GetMostPopularItems(userid);
                _logger.LogInformation("END: getting most popular items");

                //recommendations by ranking for most popular items
                _logger.LogInformation("START: getting personalized-ranking for most popular items");
                results.PersonalizedRankingResults = await PersonalizedRecommendationsService.GetPersonalizedResults(userid, results.MostPopularItems.Movies);
                _logger.LogInformation("END: getting personalized-ranking for recommended items");


                //recommendations by ranking for likely recommendations
                _logger.LogInformation("START: getting personalized-ranking for recommended items");
                results.PersonalizedRankingResultsForRecommendations = await PersonalizedRecommendationsService.GetPersonalizedResults(userid, results.RecommendedItems.Movies);
                _logger.LogInformation("END: getting personalized-ranking for recommended items");

                return results;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<SimilarViewPageModel> GetSimilarItems(string movieId, string userId)
        {
            try
            {
                var results = new SimilarViewPageModel();

                //add click event
                PersonalizedRecommendationsService.AddEventTracker(movieId, userId);

                results.SimilarItemViewModel = await PersonalizedRecommendationsService.GetSimilarItems(movieId);

                results.SimilarItemViewModel.UserId = userId;

                //get user info
                results.SimilarItemViewModel.User = new User() { User_Id = userId };

                results.MostPopularItems = await PersonalizedRecommendationsService.GetMostPopularItems(userId);

                return results;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<SimilarItemViewModel> GetSimilarItems1(string movieId, string userId)
        {
            try
            {
                //add click event
                PersonalizedRecommendationsService.AddEventTracker(movieId, userId);

                var results = await PersonalizedRecommendationsService.GetSimilarItems(movieId);

                results.UserId = userId;

                //get user info
                results.User = new User() { User_Id = userId };

                return results;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}