using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dela.Sample.IdentityService.Configuration
{
    public class InMemoryConfiguration
    {
        public static IConfiguration Configuration { get; set; }
        /// <summary>
        /// Define which APIs will use this IdentityServer
        /// 定义将使用此IdentityServer的API
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new[]
            {
                new ApiResource("clientservice", "Client Service"),
                new ApiResource("productservice", "Product Service"),
                new ApiResource("maoservice", "Mao Service")
            };
        }

        /// <summary>
        /// Define which Apps will use thie IdentityServer
        /// 定义将使用IdentityServer的应用程序
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                new Client
                {
                    ClientId = "client.service.dela",
                    ClientName="客户服务API",
                    ClientSecrets = new [] { new Secret("clientsecret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    AllowedScopes = new [] { "clientservice" }
                },
                new Client
                {
                    ClientId = "product.service.dela",
                    ClientName="产品服务API",
                    ClientSecrets = new [] { new Secret("productsecret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    AllowedScopes = new [] { "clientservice", "productservice" }
                },
                new Client
                {
                    ClientId = "mao.service.dela",
                    ClientName="小毛学英文",
                    ClientSecrets = new [] { new Secret("maosecret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    AllowedScopes = new [] { "maoservice", "clientservice", "productservice" }
                },
                new Client
                {
                    ClientId = "web.mvc.dela",
                    ClientName = "MVC客户端",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    //指定允许URI将令牌或授权代码返回到
                    RedirectUris = { $"{Configuration["Clients:MvcClient:RedirectUri"]}" },
                    //指定允许在注销后重定向到的URI
                    PostLogoutRedirectUris = { $"{Configuration["Clients:MvcClient:PostLogoutRedirectUri"]}" },
                    RequireConsent=false, //不显示授权页
                    AllowedScopes = new [] {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "maoservice", "clientservice", "productservice"
                    },
                    //AccessTokenLifetime = 3600, // one hour
                    AllowAccessTokensViaBrowser = true // can return access_token to this client
                }
            };
        }

        /// <summary>
        /// Define which uses will use this IdentityServer
        /// 定义哪些使用将使用此IdentityServer
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TestUser> GetUsers()
        {
            return new[]
            {
                new TestUser
                {
                    SubjectId = "10001",
                    Username = "admin",
                    Password = "adminpassword"
                },
                new TestUser
                {
                    SubjectId = "10002",
                    Username = "guest",
                    Password = "guestpassword"
                }
            };
        }

        /// <summary>
        /// Define which IdentityResources will use this IdentityServer
        /// 定义将使用此IdentityServer的IdentityResources
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }
    }
}
