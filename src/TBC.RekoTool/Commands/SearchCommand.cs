/*
 *  The MIT License (MIT)
 *
 *  Copyright (c) TBC Bank
 *
 *  All rights reserved.
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.Runtime;

internal sealed class SearchCommand
{
    private readonly EnumerationOptions enumerationOptions;
    private readonly SearchOptions options;

    public SearchCommand(SearchOptions options)
    {
        this.options = options;
        this.enumerationOptions = new EnumerationOptions
        {
            IgnoreInaccessible       = true,
            MatchCasing              = MatchCasing.CaseInsensitive,
            MatchType                = MatchType.Simple,
            RecurseSubdirectories    = this.options.Recurse,
            ReturnSpecialDirectories = false,
        };
    }

    public async Task ExecuteAsync()
    {
        var credentials = new BasicAWSCredentials(this.options.AccessKey, this.options.SecretKey);
        var clientConfig = new AmazonRekognitionConfig
        {
            RegionEndpoint          = this.options.Region,
            AllowAutoRedirect       = true,
            DisableLogging          = true,
            MaxConnectionsPerServer = null,
            LogMetrics              = false,
            LogResponse             = false,
            UseDualstackEndpoint    = false,
        };

        clientConfig.Validate();

        using var client = new AmazonRekognitionClient(credentials, clientConfig);

        var files = this.options.Directory.EnumerateFiles(this.options.Pattern, enumerationOptions);

        // CSV column headers:
        Console.WriteLine("FileName,FileSize,TimeTaken,FaceID,Similarity");

        var watch = new Stopwatch();

        foreach (FileInfo file in files)
        {
            using var stream = new DecoyMemoryStream(file.OpenRead(), leaveOpen: false);

            watch.Restart();

            var result = await client.SearchFacesByImageAsync(new SearchFacesByImageRequest
            {
                CollectionId       = this.options.CollectionID,
                FaceMatchThreshold = 90f,
                QualityFilter      = QualityFilter.AUTO,
                MaxFaces           = 1,
                Image              = new Image { Bytes = stream }
            }).ConfigureAwait(false);

            watch.Stop();

            // CSV values:
            Console.Write("{0},{1},{2},", file.Name, file.Length, watch.ElapsedMilliseconds);

            if (result.FaceMatches != null && result.FaceMatches.Count > 0)
            {
                Console.WriteLine("{0},{1}", result.FaceMatches[0].Face.FaceId, result.FaceMatches[0].Similarity);
            }
            else
            {
                Console.WriteLine("null,0");
            }
        }
    }
}
