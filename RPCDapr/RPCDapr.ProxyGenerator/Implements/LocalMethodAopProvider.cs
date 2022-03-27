﻿using Autofac;
using Microsoft.Extensions.DependencyInjection;
using RPCDapr.Common.Implements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPCDapr.ProxyGenerator.Implements
{
    /// <summary>
    /// 本地管道AOP提供者
    /// </summary>
    public class LocalMethodAopProvider
    {
        static Func<object, RPCDaprHttpContextWapper, Task> BeforeFunc;
        static Func<object, Task> AfterFunc;
        static Func<Exception, Task<dynamic>> ExceptionFunc;
        static Action<RPCDaprHttpContextWapper> ContextRegister;
        /// <summary>
        /// 为管道注册匿名委托
        /// </summary>
        /// <param name="beforeFunc"></param>
        /// <param name="afterFunc"></param>
        /// <param name="exceptionFunc"></param>
        public static void RegisterPipelineHandler(Action<RPCDaprHttpContextWapper> contextRegister, Func<object, RPCDaprHttpContextWapper, Task> beforeFunc = null, Func<object, Task> afterFunc = null, Func<Exception, Task<object>> exceptionFunc = null)
        {
            if (contextRegister != null)
                ContextRegister = contextRegister;
            if (beforeFunc != null)
                BeforeFunc = beforeFunc;
            if (afterFunc != null)
                AfterFunc = afterFunc;
            if (exceptionFunc != null)
                ExceptionFunc = exceptionFunc;
        }
        /// <summary>
        /// 调用方法前后异常匿名委托
        /// </summary>
        /// <typeparam name="Tin"></typeparam>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="param"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static async Task<Tout> UsePipelineHandler<Tobj, Tin, Tout>(ILifetimeScope scope, Tin param, RPCDaprHttpContextWapper wapper, Func<Tobj, Tin, Task<Tout>> method) where Tin : new() where Tout : class
        {
            try
            {
                Tout result = default;
                if (ContextRegister != null)
                    ContextRegister(wapper);
                if (BeforeFunc != null)
                    await BeforeFunc(param, wapper);
                result = await method(scope.Resolve<IServiceProvider>().GetServices<Tobj>().FirstOrDefault(), param);
                if (AfterFunc != null)
                    await AfterFunc(result);
                return result;
            }
            catch (Exception e)
            {
                if (ExceptionFunc != null)
                    return await ExceptionFunc(e);
                else
                    throw;
            }
        }
        public static async Task<Tout> UsePipelineHandler<Tobj, Tout>(ILifetimeScope scope, RPCDaprHttpContextWapper wapper, Func<Tobj, Task<Tout>> method) where Tout : class
        {
            try
            {
                Tout result = default;
                if (ContextRegister != null)
                    ContextRegister(wapper);
                if (BeforeFunc != null)
                    await BeforeFunc(null, wapper);
                result = await method(scope.Resolve<IServiceProvider>().GetServices<Tobj>().FirstOrDefault());
                if (AfterFunc != null)
                    await AfterFunc(result);
                return result;
            }
            catch (Exception e)
            {
                if (ExceptionFunc != null)
                    return await ExceptionFunc(e);
                else
                    throw;
            }
        }
    }
}