﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using LEDE.Domain.Entities;

namespace LEDE.Domain.Concrete
{
    public class CustomUserValidator : UserValidator<User, int>
    {
        public CustomUserValidator(AppUserManager mgr) : base(mgr) { }

        public override async Task<IdentityResult> ValidateAsync(User user)
        {
            IdentityResult result = await base.ValidateAsync(user);
            /*if (!user.Email.ToLower().EndsWith("@example.com"))
            {
                var errors = result.Errors.ToList();
                errors.Add("Only example.com email addresses are allowed");
                result = new IdentityResult(errors);
            }*/
            return result;
        }
    }
}