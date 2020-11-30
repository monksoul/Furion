using Fur.DependencyInjection;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fur.ViewEngine
{
    /// <summary>
    /// 视图引擎实现类
    /// </summary>
    [SkipScan]
    public class ViewEngine : IViewEngine
    {
        /// <summary>
        /// 编译模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        public IViewEngineCompiledTemplate<T> Compile<T>(string content, Action<IViewEngineCompilationOptionsBuilder> builderAction = null) where T : IViewEngineTemplate
        {
            IViewEngineCompilationOptionsBuilder compilationOptionsBuilder = new ViewEngineCompilationOptionsBuilder();

            compilationOptionsBuilder.AddAssemblyReference(typeof(T).Assembly);
            compilationOptionsBuilder.Inherits(typeof(T));

            builderAction?.Invoke(compilationOptionsBuilder);

            var memoryStream = CreateAndCompileToStream(content, compilationOptionsBuilder.Options);

            return new ViewEngineCompiledTemplate<T>(memoryStream);
        }

        /// <summary>
        /// 编译模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        public Task<IViewEngineCompiledTemplate<T>> CompileAsync<T>(string content, Action<IViewEngineCompilationOptionsBuilder> builderAction = null) where T : IViewEngineTemplate
        {
            return Task.Factory.StartNew(() => Compile<T>(content: content, builderAction: builderAction));
        }

        /// <summary>
        /// 编译模板
        /// </summary>
        /// <param name="content"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        public IViewEngineCompiledTemplate Compile(string content, Action<IViewEngineCompilationOptionsBuilder> builderAction = null)
        {
            IViewEngineCompilationOptionsBuilder compilationOptionsBuilder = new ViewEngineCompilationOptionsBuilder();
            compilationOptionsBuilder.Inherits(typeof(ViewEngineTemplate));

            builderAction?.Invoke(compilationOptionsBuilder);

            var memoryStream = CreateAndCompileToStream(content, compilationOptionsBuilder.Options);

            return new ViewEngineCompiledTemplate(memoryStream);
        }

        /// <summary>
        /// 编译模板
        /// </summary>
        /// <param name="content"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        public Task<IViewEngineCompiledTemplate> CompileAsync(string content, Action<IViewEngineCompilationOptionsBuilder> builderAction = null)
        {
            return Task.Factory.StartNew(() => Compile(content: content, builderAction: builderAction));
        }

        /// <summary>
        /// 将模板内容编译并输出内存流
        /// </summary>
        /// <param name="templateSource"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static MemoryStream CreateAndCompileToStream(string templateSource, ViewEngineCompilationOptions options)
        {
            templateSource = WriteDirectives(templateSource, options);

            var engine = RazorProjectEngine.Create(
                RazorConfiguration.Default,
                RazorProjectFileSystem.Create(@"."),
                (builder) =>
                {
                    builder.SetNamespace(options.TemplateNamespace);
                });

            var fileName = Path.GetRandomFileName();

            var document = RazorSourceDocument.Create(templateSource, fileName);

            var codeDocument = engine.Process(
                document,
                null,
                new List<RazorSourceDocument>(),
                new List<TagHelperDescriptor>());

            var razorCSharpDocument = codeDocument.GetCSharpDocument();

            var syntaxTree = CSharpSyntaxTree.ParseText(razorCSharpDocument.GeneratedCode);

            var compilation = CSharpCompilation.Create(
                fileName,
                new[]
                {
                    syntaxTree
                },
                options.ReferencedAssemblies
                    .Select(ass => MetadataReference.CreateFromFile(ass.Location))
                    .Concat(options.MetadataReferences)
                    .ToList(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var memoryStream = new MemoryStream();

            var emitResult = compilation.Emit(memoryStream);

            if (!emitResult.Success)
            {
                var errors = emitResult.Diagnostics.ToList();

                var exception = new ViewEngineCompilationException($"Unable to compile template: {errors.FirstOrDefault()}")
                {
                    Errors = errors,
                    GeneratedCode = razorCSharpDocument.GeneratedCode
                };

                throw exception;
            }

            memoryStream.Position = 0;

            return memoryStream;
        }

        /// <summary>
        /// 写入Razor 命令
        /// </summary>
        /// <param name="content"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static string WriteDirectives(string content, ViewEngineCompilationOptions options)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"@inherits {options.Inherits}");

            foreach (var entry in options.DefaultUsings)
            {
                stringBuilder.AppendLine($"@using {entry}");
            }

            stringBuilder.Append(content);

            return stringBuilder.ToString();
        }
    }
}