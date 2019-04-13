// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.Quickstart.UI
{
    public class LoginInputModel
    {
        [Required(ErrorMessage = "会员名称不能为空")]
        public string Username { get; set; }
        [Required(ErrorMessage = "会员密码不能为空")]
        public string Password { get; set; }
        public bool RememberLogin { get; set; }
        public string ReturnUrl { get; set; }
    }
}