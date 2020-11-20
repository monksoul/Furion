using Fur.DependencyInjection;
using Fur.Utilities;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

// reference：https://github.com/adoconnection/RazorEngineCore
namespace Fur.ViewEngine
{
    /// <summary>
    /// 视图编译模板实现类
    /// </summary>
    [SkipScan]
    public class ViewEngineCompiledTemplate : IViewEngineCompiledTemplate
    {
        /// <summary>
        /// 内存流
        /// </summary>
        private readonly MemoryStream assemblyByteCode;

        /// <summary>
        /// 模板类型
        /// </summary>
        private readonly Type templateType;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="assemblyByteCode"></param>
        internal ViewEngineCompiledTemplate(MemoryStream assemblyByteCode)
        {
            this.assemblyByteCode = assemblyByteCode;

            var assembly = Assembly.Load(assemblyByteCode.ToArray());
            templateType = assembly.GetType("TemplateNamespace.Template");
        }

        /// <summary>
        /// 保存到流中
        /// </summary>
        /// <param name="stream"></param>
        public void SaveToStream(Stream stream)
        {
            SaveToStreamAsync(stream).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 保存到流中
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Task SaveToStreamAsync(Stream stream)
        {
            return assemblyByteCode.CopyToAsync(stream);
        }

        /// <summary>
        /// 保存到文件
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveToFile(string fileName)
        {
            SaveToFileAsync(fileName).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 保存到文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Task SaveToFileAsync(string fileName)
        {
            using var fileStream = new FileStream(
                path: fileName,
                mode: FileMode.OpenOrCreate,
                access: FileAccess.Write,
                share: FileShare.None,
                bufferSize: 4096,
                useAsync: true);

            return assemblyByteCode.CopyToAsync(fileStream);
        }

        /// <summary>
        /// 执行编译
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string Run(object model = null)
        {
            return RunAsync(model).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 执行编译
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<string> RunAsync(object model = null)
        {
            if (model != null && model.IsAnonymous())
            {
                model = new AnonymousTypeWrapper(model);
            }

            var instance = (IViewEngineTemplate)Activator.CreateInstance(templateType);
            instance.Model = model;

            await instance.ExecuteAsync();

            return instance.Result();
        }

        /// <summary>
        /// 从文件中加载模板
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IViewEngineCompiledTemplate LoadFromFile(string fileName)
        {
            return LoadFromFileAsync(fileName: fileName).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 从文件中加载模板
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<IViewEngineCompiledTemplate> LoadFromFileAsync(string fileName)
        {
            var memoryStream = new MemoryStream();

            using (var fileStream = new FileStream(
                path: fileName,
                mode: FileMode.Open,
                access: FileAccess.Read,
                share: FileShare.None,
                bufferSize: 4096,
                useAsync: true))
            {
                await fileStream.CopyToAsync(memoryStream);
            }

            return new ViewEngineCompiledTemplate(memoryStream);
        }

        /// <summary>
        /// 从流中加载模板
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IViewEngineCompiledTemplate LoadFromStream(Stream stream)
        {
            return LoadFromStreamAsync(stream).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 从流中加载模板
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task<IViewEngineCompiledTemplate> LoadFromStreamAsync(Stream stream)
        {
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return new ViewEngineCompiledTemplate(memoryStream);
        }
    }

    /// <summary>
    /// 泛型视图模板实现类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [SkipScan]
    public class ViewEngineCompiledTemplate<T> : IViewEngineCompiledTemplate<T> where T : IViewEngineTemplate
    {
        /// <summary>
        /// 内存流
        /// </summary>
        private readonly MemoryStream assemblyByteCode;

        /// <summary>
        /// 内存流
        /// </summary>
        private readonly Type templateType;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="assemblyByteCode"></param>
        internal ViewEngineCompiledTemplate(MemoryStream assemblyByteCode)
        {
            this.assemblyByteCode = assemblyByteCode;

            var assembly = Assembly.Load(assemblyByteCode.ToArray());
            templateType = assembly.GetType("TemplateNamespace.Template");
        }

        /// <summary>
        /// 保存到流中
        /// </summary>
        /// <param name="stream"></param>
        public void SaveToStream(Stream stream)
        {
            SaveToStreamAsync(stream).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 保存到流中
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Task SaveToStreamAsync(Stream stream)
        {
            return assemblyByteCode.CopyToAsync(stream);
        }

        /// <summary>
        /// 保存到文件中
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveToFile(string fileName)
        {
            SaveToFileAsync(fileName).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 保存到文件中
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Task SaveToFileAsync(string fileName)
        {
            using var fileStream = new FileStream(
                path: fileName,
                mode: FileMode.CreateNew,
                access: FileAccess.Write,
                share: FileShare.None,
                bufferSize: 4096,
                useAsync: true);
            return assemblyByteCode.CopyToAsync(fileStream);
        }

        /// <summary>
        /// 执行编译
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public string Run(Action<T> initializer)
        {
            return RunAsync(initializer).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 执行编译
        /// </summary>
        /// <param name="initializer"></param>
        /// <returns></returns>
        public async Task<string> RunAsync(Action<T> initializer)
        {
            var instance = (T)Activator.CreateInstance(templateType);
            initializer(instance);

            await instance.ExecuteAsync();

            return instance.Result();
        }

        /// <summary>
        /// 从文件中加载模板
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IViewEngineCompiledTemplate<T> LoadFromFile(string fileName)
        {
            return LoadFromFileAsync(fileName: fileName).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 从文件中加载模板
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<IViewEngineCompiledTemplate<T>> LoadFromFileAsync(string fileName)
        {
            var memoryStream = new MemoryStream();

            using (var fileStream = new FileStream(
                path: fileName,
                mode: FileMode.Open,
                access: FileAccess.Read,
                share: FileShare.None,
                bufferSize: 4096,
                useAsync: true))
            {
                await fileStream.CopyToAsync(memoryStream);
            }

            return new ViewEngineCompiledTemplate<T>(memoryStream);
        }

        /// <summary>
        /// 从流中加载模板
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IViewEngineCompiledTemplate<T> LoadFromStream(Stream stream)
        {
            return LoadFromStreamAsync(stream).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 从流中加载模板
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task<IViewEngineCompiledTemplate<T>> LoadFromStreamAsync(Stream stream)
        {
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return new ViewEngineCompiledTemplate<T>(memoryStream);
        }
    }
}