// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Profiling;
using StackExchange.Profiling.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// 监听 EFCore 操作进程
    /// </summary>
    internal class RelationalDiagnosticListener : IMiniProfilerDiagnosticListener
    {
        /// <summary>
        /// 监听进程名
        /// </summary>
        public string ListenerName => "Microsoft.EntityFrameworkCore";

        /// <summary>
        /// 操作命令集合
        /// </summary>
        private readonly ConcurrentDictionary<Guid, CustomTiming>
            _commands = new(),
            _opening = new(),
            _closing = new();

        private readonly ConcurrentDictionary<Guid, CustomTiming>
            _readers = new();

        /// <summary>
        /// 操作完成监听
        /// </summary>
        public void OnCompleted() { }

        /// <summary>
        /// 操作错误监听
        /// </summary>
        /// <param name="error"></param>
        public void OnError(Exception error)
        {
            Trace.WriteLine(error);
        }

        /// <summary>
        /// 操作过程监听
        /// </summary>
        /// <param name="kv"></param>
        public void OnNext(KeyValuePair<string, object> kv)
        {
            if (!App.CanBeMiniProfiler()) return;

            var key = kv.Key;
            var val = kv.Value;

            // 监听命令执行前
            if (key == RelationalEventId.CommandExecuting.Name)
            {
                if (val is CommandEventData data)
                {
                    var timing = data.Command.GetTiming(data.ExecuteMethod + (data.IsAsync ? " (Async)" : null), MiniProfiler.Current);
                    if (timing != null)
                    {
                        _commands[data.CommandId] = timing;
                    }
                }
            }
            // 监听命令执行完成
            else if (key == RelationalEventId.CommandExecuted.Name)
            {
                if (val is CommandExecutedEventData data && _commands.TryRemove(data.CommandId, out var current))
                {
                    if (data.Result is RelationalDataReader)
                    {
                        _readers[data.CommandId] = current;
                        current.FirstFetchCompleted();
                    }
                    else
                    {
                        current.Stop();
                    }
                }
            }
            // 监听命令执行异常
            else if (key == RelationalEventId.CommandError.Name)
            {
                if (val is CommandErrorEventData data && _commands.TryRemove(data.CommandId, out var command))
                {
                    command.Errored = true;
                    command.Stop();
                }
            }
            // 监听读取数据释放事件
            else if (key == RelationalEventId.DataReaderDisposing.Name)
            {
                if (val is DataReaderDisposingEventData data && _readers.TryRemove(data.CommandId, out var reader))
                {
                    reader.Stop();
                }
            }
            // 监听连接事件
            else if (key == RelationalEventId.ConnectionOpening.Name)
            {
                var profiler = MiniProfiler.Current;
                if (val is ConnectionEventData data && (profiler?.Options == null || profiler.Options.TrackConnectionOpenClose))
                {
                    var timing = profiler.CustomTiming("sql",
                        data.IsAsync ? "Connection OpenAsync()" : "Connection Open()",
                        data.IsAsync ? "OpenAsync" : "Open");
                    if (timing != null)
                    {
                        _opening[data.ConnectionId] = timing;
                    }
                }
            }
            // 监听连接完成事件
            else if (key == RelationalEventId.ConnectionOpened.Name)
            {
                if (val is ConnectionEndEventData data && _opening.TryRemove(data.ConnectionId, out var openingTiming))
                {
                    openingTiming?.Stop();
                }
            }
            // 监听连接关闭事件
            else if (key == RelationalEventId.ConnectionClosing.Name)
            {
                var profiler = MiniProfiler.Current;
                if (val is ConnectionEventData data && (profiler?.Options == null || profiler.Options.TrackConnectionOpenClose))
                {
                    var timing = profiler.CustomTiming("sql",
                        data.IsAsync ? "Connection CloseAsync()" : "Connection Close()",
                        data.IsAsync ? "CloseAsync" : "Close");
                    if (timing != null)
                    {
                        _closing[data.ConnectionId] = timing;
                    }
                }
            }
            // 监听连接关闭完成事件
            else if (key == RelationalEventId.ConnectionClosed.Name)
            {
                if (val is ConnectionEndEventData data && _closing.TryRemove(data.ConnectionId, out var closingTiming))
                {
                    closingTiming?.Stop();
                }
            }
            // 监听连接异常事件
            else if (key == RelationalEventId.ConnectionError.Name)
            {
                if (val is ConnectionErrorEventData data)
                {
                    if (_opening.TryRemove(data.ConnectionId, out var openingTiming))
                    {
                        openingTiming.Errored = true;
                    }
                    if (_closing.TryRemove(data.ConnectionId, out var closingTiming))
                    {
                        closingTiming.Errored = true;
                    }
                }
            }
        }
    }
}