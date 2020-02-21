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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.Runtime;

internal sealed class CollectCommand
{
    private static readonly List<string> detectionAttributes = new List<string>
    {
        { "ALL" }, { "DEFAULT" }
    };

    private static readonly EnumerationOptions enumerationOptions = new EnumerationOptions
    {
        IgnoreInaccessible       = true,
        MatchCasing              = MatchCasing.CaseInsensitive,
        MatchType                = MatchType.Simple,
        RecurseSubdirectories    = false,
        ReturnSpecialDirectories = false,
    };

    private readonly CollectOptions options;

    public CollectCommand(CollectOptions options)
    {
        this.options = options;
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

        try
        {
            await client.DescribeCollectionAsync(
                new DescribeCollectionRequest { CollectionId = this.options.CollectionID }).ConfigureAwait(false);
        }
        catch (ResourceNotFoundException)
        {
            await client.CreateCollectionAsync(
                new CreateCollectionRequest { CollectionId = this.options.CollectionID }).ConfigureAwait(false);
        }

        var files = this.options.Directory.EnumerateFiles(this.options.Pattern, enumerationOptions);

        // Output CSV data:
        Console.WriteLine("FileName,FaceID");

        foreach (FileInfo file in files)
        {
            using var stream = new DecoyMemoryStream(file.OpenRead(), leaveOpen: false);

            var result = await client.IndexFacesAsync(new IndexFacesRequest
            {
                CollectionId        = this.options.CollectionID,
                DetectionAttributes = detectionAttributes,
                MaxFaces            = 1,
                QualityFilter       = QualityFilter.AUTO,
                Image               = new Image { Bytes = stream }
            }).ConfigureAwait(false);

            if (result.FaceRecords != null && result.FaceRecords.Count > 0)
            {
                Console.WriteLine("{0},{1}", file.Name, result.FaceRecords[0].Face.FaceId);
            }
            else
            {
                Console.WriteLine("{0},{1}", file.Name, "NotDetected");
            }
        }
    }
}
