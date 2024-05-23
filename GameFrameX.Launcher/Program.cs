﻿using System.Collections;
using System.Reflection;
using CommandLine;
using GameFrameX.Launcher.Common.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using GameFrameX.Monitor;

namespace GameFrameX.Launcher
{
    internal static class Program
    {
        private static readonly Dictionary<Type, StartUpTagAttribute> StartUpTypes = new();
        private static readonly List<Task> AppStartUpTasks = new List<Task>();
        private static readonly List<IAppStartUp> AppStartUps = new List<IAppStartUp>();

        static async Task Main(string[] args)
        {
            List<string> environmentVariablesList = new List<string>();
            environmentVariablesList.AddRange(args);
            Console.WriteLine("启动参数：" + string.Join(" ", args));
            Console.WriteLine("当前环境变量START---------------------");
            var environmentVariables = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry environmentVariable in environmentVariables)
            {
                if (environmentVariable.Value == null)
                {
                    continue;
                }

                var key = "--" + environmentVariable.Key;
                if (environmentVariablesList.Contains(key))
                {
                    continue;
                }

                environmentVariablesList.Add(key);
                environmentVariablesList.Add(environmentVariable.Value.ToString());
            }

            Console.WriteLine("当前环境变量END---------------------");
            Console.WriteLine();
            Console.WriteLine();
            var commandLineParser = new Parser(configuration => { configuration.IgnoreUnknownArguments = true; });

            var launcherOptions = commandLineParser.ParseArguments<LauncherOptions>(environmentVariablesList).WithParsed((LauncherOptionsValidate))?.Value;
            var serverType = launcherOptions?.ServerType;
            if (!serverType.IsNullOrEmpty())
            {
                Console.WriteLine("启动的服务器类型 ServerType: " + serverType);
            }

            LoggerHandler.Start(serverType);
            JsonSetting();
            GlobalSettings.Load<AppSetting>($"Configs/app_config.json");
            CacheStateTypeManager.Init();
            ProtoMessageIdHandler.Init();

            var types = Assembly.GetEntryAssembly()?.GetTypes();
            if (types != null)
            {
                foreach (var type in types)
                {
                    if (type.IsClass && type.IsImplWithInterface(typeof(IAppStartUp)) && type.GetCustomAttribute<StartUpTagAttribute>() != null)
                    {
                        var startUpTag = type.GetCustomAttribute<StartUpTagAttribute>();
                        StartUpTypes.Add(type, startUpTag);
                    }
                }
            }

            var sortedStartUpTypes = StartUpTypes.OrderBy(m => m.Value.Priority);

            LogHelper.Info($"----------------------------开始启动服务器啦------------------------------");
            var appSettings = GlobalSettings.GetSettings<AppSetting>();
            if (serverType != null && Enum.TryParse(serverType, out ServerType serverTypeValue))
            {
                var startKv = sortedStartUpTypes.FirstOrDefault(m => m.Value.ServerType == serverTypeValue);
                if (startKv.Value != null)
                {
                    var appSetting = appSettings.FirstOrDefault(m => m.ServerType == serverTypeValue);
                    if (appSetting != null)
                    {
                        LogHelper.Error("从配置文件中找到对应的服务器类型的启动配置,将以配置启动=>" + startKv.Value.ServerType);
                    }
                    else
                    {
                        LogHelper.Error("没有找到对应的服务器类型的启动配置,将以默认配置启动=>" + startKv.Value.ServerType);
                    }

                    Launcher(args, startKv, appSetting);
                }
            }
            else
            {
                foreach (var keyValuePair in sortedStartUpTypes)
                {
                    bool isFind = false;

                    foreach (var appSetting in appSettings)
                    {
                        if (keyValuePair.Value.ServerType == appSetting.ServerType)
                        {
                            Launcher(args, keyValuePair, appSetting);
                            isFind = true;
                            break;
                        }
                    }

                    if (isFind == false)
                    {
                        LogHelper.Error("没有找到对应的服务器类型的启动配置,将以默认配置启动=>" + keyValuePair.Value.ServerType);
                        Launcher(args, keyValuePair);
                    }
                }
            }

            LogHelper.Info($"----------------------------启动服务器结束啦------------------------------");
            ApplicationPerformanceMonitorStart(serverType);
            ConsoleLogo();

            await Task.WhenAll(AppStartUpTasks);
        }

        private static void LauncherOptionsValidate(LauncherOptions options)
        {
            if (!options.ServerType.IsNullOrEmpty() && Enum.TryParse(options.ServerType, out ServerType serverTypeValue))
            {
                options.CheckAPMPort();

                options.CheckServerId();

                options.CheckInnerPort();

                switch (serverTypeValue)
                {
                    case ServerType.Log:
                        break;
                    case ServerType.DataBase:
                    {
                        options.CheckDataBaseUrl();

                        options.CheckDataBaseName();

                        options.CheckOuterIp();

                        options.CheckOuterPort();
                    }
                        break;
                    case ServerType.Cache:
                        break;
                    case ServerType.Gateway:
                    {
                        options.CheckOuterIp();

                        options.CheckOuterPort();
                    }
                        break;
                    case ServerType.Account:
                        break;
                    case ServerType.Router:
                    { 
                        options.CheckOuterIp();
                        options.CheckOuterPort();
                        options.CheckWsPort();
                        options.CheckDiscoveryCenterIp();
                        options.CheckDiscoveryCenterPort();
                    }
                        break;
                    case ServerType.DiscoveryCenter:
                    {
                        options.CheckOuterIp();
                        options.CheckOuterPort();
                    }
                        break;
                    case ServerType.Backup:
                        break;
                    case ServerType.Login:
                        break;
                    case ServerType.Game:
                    {
                        options.CheckOuterIp();
                        options.CheckOuterPort();
                        options.CheckDiscoveryCenterIp();
                        options.CheckDiscoveryCenterPort();
                    }
                        break;
                    case ServerType.Recharge:
                        break;
                    case ServerType.Logic:
                        break;
                    case ServerType.Chat:
                        break;
                    case ServerType.Mail:
                        break;
                    case ServerType.Guild:
                        break;
                    case ServerType.Room:
                        break;
                    case ServerType.All:
                        break;
                }
            }
        }

        private static void Launcher(string[] args, KeyValuePair<Type, StartUpTagAttribute> keyValuePair, AppSetting appSetting = null)
        {
            var task = Start(args, keyValuePair.Key, keyValuePair.Value.ServerType, appSetting, out var startUp);
            LogHelper.Info("启动配置：" + startUp.Setting);
            AppStartUps.Add(startUp);
            AppStartUpTasks.Add(task);
        }

        private static void ApplicationPerformanceMonitorStart(string serverType)
        {
            if (serverType != null && Enum.TryParse(serverType, out ServerType serverTypeValue))
            {
                foreach (var appStartUp in AppStartUps)
                {
                    if (appStartUp.ServerType == serverTypeValue && appStartUp.Setting.APMPort is > 0 and < ushort.MaxValue)
                    {
                        MetricsHelper.Start(appStartUp.Setting.APMPort);
                        return;
                    }
                }
            }

            MetricsHelper.Start();
        }

        /// <summary>
        /// Json 配置
        /// </summary>
        private static void JsonSetting()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,

                NullValueHandling = NullValueHandling.Ignore, // 忽略 null 值
                // Formatting = Formatting.Indented, // 生成格式化的 JSON
                MissingMemberHandling = MissingMemberHandling.Ignore, // 忽略缺失的成员
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter() // 将枚举转换为字符串
                }
            };
        }

        private static Task Start(string[] args, Type appStartUpType, ServerType serverType, BaseSetting setting, out IAppStartUp startUp)
        {
            startUp = (IAppStartUp)Activator.CreateInstance(appStartUpType);
            if (startUp != null)
            {
                bool isSuccess = startUp.Init(serverType, setting, args);
                if (isSuccess)
                {
                    // LogHelper.Info($"启动服务器类型：{keyValuePair.Value.ServerType},配置信息：{JsonConvert.SerializeObject(appSetting)}");
                    LogHelper.Info($"----------------------------START-----{serverType}------------------------------");
                    var task = AppEnter.Entry(startUp);
                    LogHelper.Info($"-----------------------------END------{serverType}------------------------------");
                    return task;
                }
            }

            return Task.CompletedTask;
        }

        private static void ConsoleLogo()
        {
            Console.WriteLine(@" _____                         ______                               __   __");
            Console.WriteLine(@"|  __ \                        |  ___|                              \ \ / /");
            Console.WriteLine(@"| |  \/  __ _  _ __ ___    ___ | |_    _ __   __ _  _ __ ___    ___  \ V / ");
            Console.WriteLine(@"| | __  / _` || '_ ` _ \  / _ \|  _|  | '__| / _` || '_ ` _ \  / _ \ /   \ ");
            Console.WriteLine(@"| |_\ \| (_| || | | | | ||  __/| |    | |   | (_| || | | | | ||  __// /^\ \");
            Console.WriteLine(@" \____/ \__,_||_| |_| |_| \___|\_|    |_|    \__,_||_| |_| |_| \___|\/   \/");
            Console.WriteLine(@"                                                                           ");
        }
    }
}