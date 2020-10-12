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
using AWS.Samples.Amazon.Personalize.Demo.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AWS.Samples.Amazon.Personalize.Demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger logger;

        public HomeController(ILogger<HomeController> logger)
        {
            HomeControllerService = new HomeControllerService(logger);

            this.logger = logger;
        }

        public HomeControllerService HomeControllerService { get; set; }

        public async Task<IActionResult> Index(string userId)
        {
            //usage
            var randomUserIdGenerator = new RandomUserIdGenerator();

            if (string.IsNullOrEmpty(userId)) userId = randomUserIdGenerator.GetUser();

            logger.LogInformation("START: getting recommendations for userid: " + userId);

            var results = await HomeControllerService.GetRecommendations(userId);

            logger.LogInformation("END: getting recommendations for userid: " + userId);

            return View(results);
        }

        public async Task<IActionResult> Similar(string movieId, string userId)
        {
            SimilarViewPageModel results;

            results = await HomeControllerService.GetSimilarItems(movieId, userId);

            results.SimilarItemViewModel.UserId = userId;

            return View("~/Views/Home/similar-items.cshtml", results);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}