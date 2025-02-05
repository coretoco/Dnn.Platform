﻿#region Copyright

// 
// DotNetNuke® - https://www.dnnsoftware.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

#region Usings

using System;

using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;

#endregion

namespace DotNetNuke.Authentication.Twitter.Components
{
    public class TwitterClient : OAuthClientBase
    {
        public TwitterClient(int portalId, AuthMode mode)
            : base(portalId, mode, "Twitter")
        {
            AuthorizationEndpoint = new Uri("https://api.twitter.com/oauth/authorize");
            RequestTokenEndpoint = new Uri("https://api.twitter.com/oauth/request_token");
            RequestTokenMethod = HttpMethod.POST;
            TokenEndpoint = new Uri("https://api.twitter.com/oauth/access_token");
            MeGraphEndpoint = new Uri("https://api.twitter.com/1.1/account/verify_credentials.json");

            AuthTokenName = "TwitterUserToken";

            OAuthVersion = "1.0";

            LoadTokenCookie(String.Empty);
        }
    }
}