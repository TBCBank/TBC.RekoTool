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
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;

public static class Program
{
    public static Task Main(string[] args)
    {
        var collectCommand = new Command("collect")
        {
            new Option<DirectoryInfo>("--directory", getDefaultValue: () => new DirectoryInfo(Directory.GetCurrentDirectory())),
            new Option<string>("--pattern", getDefaultValue: () => "*.jpg"),
            new Option<string>("--collectionID") { Required = true },
        };

        var searchCommand = new Command("search")
        {
            new Option<DirectoryInfo>("--directory", getDefaultValue: () => new DirectoryInfo(Directory.GetCurrentDirectory())),
            new Option<string>("--pattern", getDefaultValue: () => "*.jpg"),
            new Option<string>("--collectionID") { Required = true },
        };

        collectCommand.Handler = CommandHandler.Create<CollectOptions>(options => new CollectCommand(options).ExecuteAsync());
        searchCommand.Handler = CommandHandler.Create<SearchOptions>(options => new SearchCommand(options).ExecuteAsync());

        var rootCommand = new RootCommand();

        rootCommand.Add(collectCommand);
        rootCommand.Add(searchCommand);

        rootCommand.AddGlobalOption(new Option<string>("--access-key", "Amazon Rekognition access key") { Required = true });
        rootCommand.AddGlobalOption(new Option<string>("--secret-key", "Amazon Rekognition secret key") { Required = true });

        var regionOption = new Option<RegionEndpoint>("--region",
            new ParseArgument<RegionEndpoint>(target => RegionEndpoint.GetBySystemName(target.Tokens[0].Value)));

        regionOption.Description = "System name of an AWS region";
        regionOption.Required = true;
        regionOption.WithSuggestions(RegionEndpoint.EnumerableAllRegions
            .Where(r => r.SystemName.StartsWith("eu-", StringComparison.OrdinalIgnoreCase))
            .Select(r => r.SystemName)
            .ToArray());

        rootCommand.AddGlobalOption(regionOption);

        rootCommand.Description = "Amazon Rekognition testing tool";

        return rootCommand.InvokeAsync(args);
    }
}
