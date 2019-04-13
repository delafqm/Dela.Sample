// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Quickstart.UI
{
    /// <summary>
    /// ��ʾ��������Ϊ���غ��ⲿ�ʻ�ʵ�ֵ��͵ĵ�¼/ע��/�ṩ�������̡�
    /// ��¼�����װ�����û����ݴ洢�Ľ����� �����ݴ洢�����ڴ��У���������������
    /// ��������ΪUI�ṩ��һ������ݷ�����ͨ���Խ�����֤�������ļ����ķ���
    /// </summary>
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly TestUserStore _users;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;

        public AccountController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            TestUserStore users = null)
        {
            // ���TestUserStore����DI�У���ô���ǽ�ֻʹ��ȫ���û����ϣ�
            // �����������Լ����Զ�����ݹ����ĵط�������ASP.NET��ݣ�
            _users = users ?? new TestUserStore(TestUsers.Users);

            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
        }

        /// <summary>
        /// �����¼�������̵���ڵ�
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // ����һ��ģ�ͣ��Ա�����֪���ڵ�¼ҳ������ʾʲô
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // ����ֻ��һ����¼ѡ�����һ���ⲿ�ṩ��
                return RedirectToAction("Challenge", "External", new { provider = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        /// <summary>
        /// �����û���/�����¼�Ļط�
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            // ��������Ƿ�����Ȩ�������������
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // �û�����ˡ�ȡ������ť
            if (button != "login")
            {
                if (context != null)
                {
                    // ����û�ȡ�����򽫽�����ͻ�IdentityServer���������Ǿܾ�ͬ��һ������ʹ�˿ͻ�����Ҫͬ�⣩��
                    // �⽫��ͻ��˷��ؾܾ����ʵ�OIDC������Ӧ��
                    await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                    // ���ǿ�������model.ReturnUrl����ΪGetAuthorizationContextAsync���ط�null
                    if (await _clientStore.IsPkceClientAsync(context.ClientId))
                    {
                        // ����ͻ�����PKCE����ô���Ǽ�������ԭ���ģ�
                        // �����η�����Ӧ�����ָı���Ϊ�����û��ṩ���õ��û����顣
                        return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                    }
                    var refererUrl = Request.Headers["Referer"].ToString();
                    return Redirect(refererUrl);
                    //return Redirect(model.ReturnUrl);
                }
                else
                {
                    // ��Ϊ����û����Ч�������ģ���������ֻ�践����ҳ
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                // ��֤�ڴ�洢�е��û���/����
                if (_users.ValidateCredentials(model.Username, model.Password))
                {
                    var user = _users.FindByUsername(model.Username);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.SubjectId, user.Username));

                    // ����û�ѡ�񡰼�ס�ҡ�������ڴ�������ȷ�ĵ���ʱ�䡣
                    // ��������������cookie�м�������õĵ��ڡ�
                    AuthenticationProperties props = null;
                    if (AccountOptions.AllowRememberLogin && model.RememberLogin)
                    {
                        props = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                        };
                    };

                    // ʹ������ID���û������������֤Cookie
                    await HttpContext.SignInAsync(user.SubjectId, user.Username, props);

                    if (context != null)
                    {
                        if (await _clientStore.IsPkceClientAsync(context.ClientId))
                        {
                            // ����ͻ�����PKCE����ô���Ǽ�������ԭ���ģ�
                            // �����η�����Ӧ�����ָı���Ϊ�����û��ṩ���õ��û����顣
                            return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                        }

                        // ���ǿ�������model.ReturnUrl����ΪGetAuthorizationContextAsync���ط�null
                        return Redirect(model.ReturnUrl);
                    }

                    // ���󱾵�ҳ��
                    if (Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect("~/");
                    }
                    else
                    {
                        // �û������Ѿ�����˶������� - Ӧ�ñ���¼
                        throw new Exception("��Ч�ķ�����ַ");
                    }
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "��Ч֤��"));
                ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
            }

            // ���˵����⣬��ʾ�д���ı��
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }


        /// <summary>
        /// ��ʾע��ҳ��
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            #region ��ʾע��ҳ
            //  **ע��** ��ʾע��ҳ��InMemoryConfiguration������Ϣ��PostLogoutRedirectUris ���Ա�����
            // ����ģ�ͣ��Ա�ע��ҳ��֪��Ҫ��ʾ������
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // �����IdentityServer��ȷ��֤��ע��������ô
                // ���ǲ���Ҫ��ʾ��ʾ��ֻ��ֱ�ӽ��û�ע����
                return await Logout(vm);
            }

            return View(vm);
            #endregion

            #region ����ʾע��ҳ
            // **ע��** ��ʾע��ҳ��InMemoryConfiguration������Ϣ��PostLogoutRedirectUris ����Ҫע�͵�
            //var logout = await _interaction.GetLogoutContextAsync(logoutId);


            //if (User?.Identity.IsAuthenticated == true)
            //{
            //    // ɾ�����������֤cookie
            //    await HttpContext.SignOutAsync();

            //    //await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            //}

            //if (logout.PostLogoutRedirectUri != null)
            //{
            //    return Redirect(logout.PostLogoutRedirectUri);
            //}
            //var refererUrl = Request.Headers["Referer"].ToString();
            //return Redirect(refererUrl);
            #endregion
        }

        /// <summary>
        /// �����˳�ҳ��ط�
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // ����ģ�ͣ��Ա�ע����ҳ��֪��Ҫ��ʾ������
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // ɾ�����������֤cookie
                await HttpContext.SignOutAsync();

                // ���ע���¼�
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // ��������Ƿ���Ҫ����������ṩ�̴�����ע��
            if (vm.TriggerExternalSignout)
            {
                // ����һ������URL���Ա����û�ע���������ṩ�����ض�������ǡ�
                // ��ʹ���ǿ�����ɵ���ǳ�����
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // ��ᴥ���ض����ⲿ�ṩ�����Խ���ע��
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }



        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/
        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null)
            {
                // ����ζ�Ŷ�·UI���ҽ�����һ���ⲿIdP
                return new LoginViewModel
                {
                    EnableLocalLogin = false,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                    ExternalProviders = new ExternalProvider[] { new ExternalProvider { AuthenticationScheme = context.IdP } }
                };
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // ����û�δ���������֤����ֻ��ʾ��ע����ҳ��
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // �Զ�ע���ǰ�ȫ��
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // ��ʾע����ʾ�� 
            // ����Է�ֹ�û�����һ��������ҳ�Զ�ע���Ĺ�����
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // ��ȡ��������Ϣ������ע���Ŀͻ������ƣ�����ע���ض���URI��iframe��
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // ���û�е�ǰ��ע�������ģ�
                            // ������Ҫ��ע��֮ǰ����һ���ӵ�ǰ��¼�û������Ҫ��Ϣ���ض����ⲿIdP�Խ���ע��
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }
    }
}