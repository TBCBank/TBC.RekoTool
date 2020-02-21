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

using System.IO;
using Amazon;

sealed class SearchOptions
{
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public RegionEndpoint Region { get; set; }

    public DirectoryInfo Directory { get; set; }
    public string Pattern { get; set; }
    public string CollectionID { get; set; }

    public SearchOptions(
        string accessKey, string secretKey, RegionEndpoint region,
        DirectoryInfo directory, string pattern, string collectionID)
    {
        this.AccessKey = accessKey;
        this.SecretKey = secretKey;
        this.Region = region;

        this.Directory = directory;
        this.Pattern = pattern;
        this.CollectionID = collectionID;
    }
}
