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
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System;

namespace AWS.Samples.Amazon.Personalize.Demo.Support
{
    public sealed class AwsParameterStoreClient
    {
        private static readonly Lazy<AwsParameterStoreClient> lazy = new Lazy<AwsParameterStoreClient>(() => new AwsParameterStoreClient());

        public static AwsParameterStoreClient Instance { get { return lazy.Value; } }

        private AwsParameterStoreClient()
        {
        }

        private static readonly RegionEndpoint _region = RegionEndpoint.USEast1;

        private static string GetValue(string parameter)
        {
            try
            {
                var ssmClient = new AmazonSimpleSystemsManagementClient(_region);

                var pName = string.Format("/{0}/{1}", "amz-personalize-demo", parameter);

                var response = ssmClient.GetParameterAsync(new GetParameterRequest
                {
                    Name = pName
                });

                var pramResponse = response.Result;

                return pramResponse.Parameter.Value;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public static string GetImdbApiKey()
        {
            return GetValue("imdb-api-key");
        }

        public static string GetClickStreamTrackingId()
        {
            return GetValue("demo-click-stream-trk-id");
        }

        public static string GetSimsArn()
        {
            return GetValue("demo-sims-arn");
        }

        public static string GetSimsMostPopularArn()
        {
            return GetValue("demo-sims-most-popular");
        }

        public static string GetPersonalRecommendationsArn()
        {
            return GetValue("demo-personal-recommendations");
        }

        public static string GetS3BucketKey()
        {
            return GetValue("demo-s3-bucket-name");
        }

        public static string GetPersonalRankingArn()
        {
            return GetValue("demo-personal-ranking");
        }
    }
}