﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YC.ServiceWebApi.Result;
using ServiceWebApi.Dto;
using YC.Common;
using YC.Common.ShareUtils;
using YC.ApplicationService;
using Newtonsoft.Json.Linq;
using YC.ServiceWebApi.Tenant;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.IO;
using YC.Core;
using YC.Model.DbEntity;
using YC.Core;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace YC.ServiceWebApi.Controllers
{


    /// <summary>
    /// 接口服务身份认证
    /// </summary>
    //[ApiVersion("1.0")]
    //[Route("api/v{version:apiVersion}/[controller]")]


    public class IdentityController : BaseController
    {
        public readonly ISysUserAppService _sysUserService;
        public readonly IUserManager _userManager;

        public IdentityController(ISysUserAppService sysUserService, IUserManager userManager)
        {
            _sysUserService = sysUserService;
            _userManager = userManager;

        }
        /// <summary>
        /// 获取token，通过登录
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="pwd">用户密码</param>
        /// <returns></returns>
        [HttpPost]

        public IActionResult GetTokenByLogin([FromBody] LoginUserDto loginUserDto)
        {
            //登录，先去数据库做验证，成功了，说明可以进行token创建，往payLoad字典中加入,如果没有传TenantId 默认就为默认租户
            IApiResult<UserDto> result = _userManager.UserLogin(loginUserDto.UserId, loginUserDto.Pwd, loginUserDto.TenantId == 0 ? 1 : loginUserDto.TenantId);
            return new JsonResult(result);


        }

        [HttpPost]
        public IApiResult RefreshToken(string token)
        {
            var msg = "";
            var tokenStr = _userManager.RefreshToken(token);
            var state = string.IsNullOrWhiteSpace(tokenStr) ? false : true;
            msg = state == true ? "刷新token获取成功！" : "token过期,请重新登录！";
            return ApiResult.Result<string>(state, tokenStr, msg);
        }

     

    }

}
