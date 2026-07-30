// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// Furion 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// 官方网站：https://furion.net
//
// 使用条款
// 使用本代码应遵守相关法律法规和许可证的要求。
//
// 免责声明
// 对于因使用本代码而产生的任何直接、间接、偶然、特殊或后果性损害，我们不承担任何责任。
//
// 其他重要信息
// Furion 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。
// 有关 Furion 项目的其他详细信息，请参阅位于源代码树根目录中的 COPYRIGHT 和 DISCLAIMER 文件。
//
// 更多信息
// 请访问 https://gitee.com/dotnetchina/Furion 获取更多关于 Furion 项目的许可证和版权信息。
// ------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace Furion.HttpRemote;

/// <summary>
///     据文件扩展名提供内容类型
/// </summary>
/// <remarks>参考文献：https://www.iana.org/assignments/media-types/media-types.xhtml。</remarks>
public sealed class FileTypeMapper
{
    /// <summary>
    ///     <inheritdoc cref="FileTypeMapper" />
    /// </summary>
    public FileTypeMapper() : this(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { ".323", "text/h323" },
        { ".3g2", "video/3gpp2" },
        { ".3gp2", "video/3gpp2" },
        { ".3gp", "video/3gpp" },
        { ".3gpp", "video/3gpp" },
        { ".aac", "audio/aac" },
        { ".aaf", "application/octet-stream" },
        { ".aca", "application/octet-stream" },
        { ".accdb", "application/msaccess" },
        { ".accde", "application/msaccess" },
        { ".accdt", "application/msaccess" },
        { ".acx", "application/internet-property-stream" },
        { ".adt", "audio/vnd.dlna.adts" },
        { ".adts", "audio/vnd.dlna.adts" },
        { ".afm", "application/octet-stream" },
        { ".ai", "application/postscript" },
        { ".aif", "audio/aiff" },
        { ".aifc", "audio/aiff" },
        { ".aiff", "audio/aiff" },
        { ".appcache", "text/cache-manifest" },
        { ".application", "application/x-ms-application" },
        { ".art", "image/x-jg" },
        { ".asd", "application/octet-stream" },
        { ".asf", "video/x-ms-asf" },
        { ".asi", "application/octet-stream" },
        { ".asm", "text/plain" },
        { ".asr", "video/x-ms-asf" },
        { ".asx", "video/x-ms-asf" },
        { ".atom", "application/atom+xml" },
        { ".au", "audio/basic" },
        { ".avi", "video/x-msvideo" },
        { ".axs", "application/olescript" },
        { ".bas", "text/plain" },
        { ".bcpio", "application/x-bcpio" },
        { ".bin", "application/octet-stream" },
        { ".bmp", "image/bmp" },
        { ".c", "text/plain" },
        { ".cab", "application/vnd.ms-cab-compressed" },
        { ".calx", "application/vnd.ms-office.calx" },
        { ".cat", "application/vnd.ms-pki.seccat" },
        { ".cdf", "application/x-cdf" },
        { ".chm", "application/octet-stream" },
        { ".class", "application/x-java-applet" },
        { ".clp", "application/x-msclip" },
        { ".cmx", "image/x-cmx" },
        { ".cnf", "text/plain" },
        { ".cod", "image/cis-cod" },
        { ".cpio", "application/x-cpio" },
        { ".cpp", "text/plain" },
        { ".crd", "application/x-mscardfile" },
        { ".crl", "application/pkix-crl" },
        { ".crt", "application/x-x509-ca-cert" },
        { ".csh", "application/x-csh" },
        { ".css", "text/css" },
        { ".csv", "text/csv" }, // https://tools.ietf.org/html/rfc7111#section-5.1
        { ".cur", "application/octet-stream" },
        { ".dcr", "application/x-director" },
        { ".deploy", "application/octet-stream" },
        { ".der", "application/x-x509-ca-cert" },
        { ".dib", "image/bmp" },
        { ".dir", "application/x-director" },
        { ".disco", "text/xml" },
        { ".dlm", "text/dlm" },
        { ".doc", "application/msword" },
        { ".docm", "application/vnd.ms-word.document.macroEnabled.12" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".dot", "application/msword" },
        { ".dotm", "application/vnd.ms-word.template.macroEnabled.12" },
        { ".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template" },
        { ".dsp", "application/octet-stream" },
        { ".dtd", "text/xml" },
        { ".dvi", "application/x-dvi" },
        { ".dvr-ms", "video/x-ms-dvr" },
        { ".dwf", "drawing/x-dwf" },
        { ".dwp", "application/octet-stream" },
        { ".dxr", "application/x-director" },
        { ".eml", "message/rfc822" },
        { ".emz", "application/octet-stream" },
        { ".eot", "application/vnd.ms-fontobject" },
        { ".eps", "application/postscript" },
        { ".etx", "text/x-setext" },
        { ".evy", "application/envoy" },
        {
            ".exe", "application/vnd.microsoft.portable-executable"
        }, // https://www.iana.org/assignments/media-types/application/vnd.microsoft.portable-executable
        { ".fdf", "application/vnd.fdf" },
        { ".fif", "application/fractals" },
        { ".fla", "application/octet-stream" },
        { ".flr", "x-world/x-vrml" },
        { ".flv", "video/x-flv" },
        { ".gif", "image/gif" },
        { ".gtar", "application/x-gtar" },
        { ".gz", "application/x-gzip" },
        { ".h", "text/plain" },
        { ".hdf", "application/x-hdf" },
        { ".hdml", "text/x-hdml" },
        { ".hhc", "application/x-oleobject" },
        { ".hhk", "application/octet-stream" },
        { ".hhp", "application/octet-stream" },
        { ".hlp", "application/winhlp" },
        { ".hqx", "application/mac-binhex40" },
        { ".hta", "application/hta" },
        { ".htc", "text/x-component" },
        { ".htm", "text/html" },
        { ".html", "text/html" },
        { ".htt", "text/webviewhtml" },
        { ".hxt", "text/html" },
        { ".ical", "text/calendar" },
        { ".icalendar", "text/calendar" },
        { ".ico", "image/vnd.microsoft.icon" },
        { ".ics", "text/calendar" },
        { ".ief", "image/ief" },
        { ".ifb", "text/calendar" },
        { ".iii", "application/x-iphone" },
        { ".inf", "application/octet-stream" },
        { ".ins", "application/x-internet-signup" },
        { ".isp", "application/x-internet-signup" },
        { ".IVF", "video/x-ivf" },
        { ".jar", "application/java-archive" },
        { ".java", "application/octet-stream" },
        { ".jck", "application/liquidmotion" },
        { ".jcz", "application/liquidmotion" },
        { ".jfif", "image/pjpeg" },
        { ".jpb", "application/octet-stream" },
        { ".jpe", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".jpg", "image/jpeg" },
        { ".js", "text/javascript" },
        { ".json", "application/json" },
        { ".jsx", "text/jscript" },
        { ".latex", "application/x-latex" },
        { ".lit", "application/x-ms-reader" },
        { ".lpk", "application/octet-stream" },
        { ".lsf", "video/x-la-asf" },
        { ".lsx", "video/x-la-asf" },
        { ".lzh", "application/octet-stream" },
        { ".m13", "application/x-msmediaview" },
        { ".m14", "application/x-msmediaview" },
        { ".m1v", "video/mpeg" },
        { ".m2ts", "video/vnd.dlna.mpeg-tts" },
        { ".m3u", "audio/x-mpegurl" },
        { ".m4a", "audio/mp4" },
        { ".m4v", "video/mp4" },
        { ".man", "application/x-troff-man" },
        { ".manifest", "application/x-ms-manifest" },
        { ".map", "text/plain" },
        { ".markdown", "text/markdown" },
        { ".md", "text/markdown" },
        { ".mdb", "application/x-msaccess" },
        { ".mdp", "application/octet-stream" },
        { ".me", "application/x-troff-me" },
        { ".mht", "message/rfc822" },
        { ".mhtml", "message/rfc822" },
        { ".mid", "audio/mid" },
        { ".midi", "audio/mid" },
        { ".mix", "application/octet-stream" },
        { ".mjs", "text/javascript" },
        { ".mmf", "application/x-smaf" },
        { ".mno", "text/xml" },
        { ".mny", "application/x-msmoney" },
        { ".mov", "video/quicktime" },
        { ".movie", "video/x-sgi-movie" },
        { ".mp2", "video/mpeg" },
        { ".mp3", "audio/mpeg" },
        { ".mp4", "video/mp4" },
        { ".mp4v", "video/mp4" },
        { ".mpa", "video/mpeg" },
        { ".mpe", "video/mpeg" },
        { ".mpeg", "video/mpeg" },
        { ".mpg", "video/mpeg" },
        { ".mpp", "application/vnd.ms-project" },
        { ".mpv2", "video/mpeg" },
        { ".ms", "application/x-troff-ms" },
        { ".msi", "application/octet-stream" },
        { ".mso", "application/octet-stream" },
        { ".mvb", "application/x-msmediaview" },
        { ".mvc", "application/x-miva-compiled" },
        { ".nc", "application/x-netcdf" },
        { ".nsc", "video/x-ms-asf" },
        { ".nws", "message/rfc822" },
        { ".ocx", "application/octet-stream" },
        { ".oda", "application/oda" },
        { ".odc", "text/x-ms-odc" },
        { ".ods", "application/vnd.oasis.opendocument.spreadsheet" },
        { ".oga", "audio/ogg" },
        { ".ogg", "video/ogg" },
        { ".ogv", "video/ogg" },
        { ".ogx", "application/ogg" },
        { ".one", "application/onenote" },
        { ".onea", "application/onenote" },
        { ".onetoc", "application/onenote" },
        { ".onetoc2", "application/onenote" },
        { ".onetmp", "application/onenote" },
        { ".onepkg", "application/onenote" },
        { ".osdx", "application/opensearchdescription+xml" },
        { ".otf", "font/otf" },
        { ".p10", "application/pkcs10" },
        { ".p12", "application/x-pkcs12" },
        { ".p7b", "application/x-pkcs7-certificates" },
        { ".p7c", "application/pkcs7-mime" },
        { ".p7m", "application/pkcs7-mime" },
        { ".p7r", "application/x-pkcs7-certreqresp" },
        { ".p7s", "application/pkcs7-signature" },
        { ".pbm", "image/x-portable-bitmap" },
        { ".pcx", "application/octet-stream" },
        { ".pcz", "application/octet-stream" },
        { ".pdf", "application/pdf" },
        { ".pfb", "application/octet-stream" },
        { ".pfm", "application/octet-stream" },
        { ".pfx", "application/x-pkcs12" },
        { ".pgm", "image/x-portable-graymap" },
        { ".pko", "application/vnd.ms-pki.pko" },
        { ".pma", "application/x-perfmon" },
        { ".pmc", "application/x-perfmon" },
        { ".pml", "application/x-perfmon" },
        { ".pmr", "application/x-perfmon" },
        { ".pmw", "application/x-perfmon" },
        { ".png", "image/png" },
        { ".pnm", "image/x-portable-anymap" },
        { ".pnz", "image/png" },
        { ".pot", "application/vnd.ms-powerpoint" },
        { ".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12" },
        { ".potx", "application/vnd.openxmlformats-officedocument.presentationml.template" },
        { ".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12" },
        { ".ppm", "image/x-portable-pixmap" },
        { ".pps", "application/vnd.ms-powerpoint" },
        { ".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12" },
        { ".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow" },
        { ".ppt", "application/vnd.ms-powerpoint" },
        { ".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12" },
        { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
        { ".prf", "application/pics-rules" },
        { ".prm", "application/octet-stream" },
        { ".prx", "application/octet-stream" },
        { ".ps", "application/postscript" },
        { ".psd", "application/octet-stream" },
        { ".psm", "application/octet-stream" },
        { ".psp", "application/octet-stream" },
        { ".pub", "application/x-mspublisher" },
        { ".qt", "video/quicktime" },
        { ".qtl", "application/x-quicktimeplayer" },
        { ".qxd", "application/octet-stream" },
        { ".ra", "audio/x-pn-realaudio" },
        { ".ram", "audio/x-pn-realaudio" },
        { ".rar", "application/octet-stream" },
        { ".ras", "image/x-cmu-raster" },
        { ".rf", "image/vnd.rn-realflash" },
        { ".rgb", "image/x-rgb" },
        { ".rm", "application/vnd.rn-realmedia" },
        { ".rmi", "audio/mid" },
        { ".roff", "application/x-troff" },
        { ".rpm", "audio/x-pn-realaudio-plugin" },
        { ".rtf", "application/rtf" },
        { ".rtx", "text/richtext" },
        { ".scd", "application/x-msschedule" },
        { ".sct", "text/scriptlet" },
        { ".sea", "application/octet-stream" },
        { ".setpay", "application/set-payment-initiation" },
        { ".setreg", "application/set-registration-initiation" },
        { ".sgml", "text/sgml" },
        { ".sh", "application/x-sh" },
        { ".shar", "application/x-shar" },
        { ".sit", "application/x-stuffit" },
        { ".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12" },
        { ".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide" },
        { ".smd", "audio/x-smd" },
        { ".smi", "application/octet-stream" },
        { ".smx", "audio/x-smd" },
        { ".smz", "audio/x-smd" },
        { ".snd", "audio/basic" },
        { ".snp", "application/octet-stream" },
        { ".spc", "application/x-pkcs7-certificates" },
        { ".spl", "application/futuresplash" },
        { ".spx", "audio/ogg" },
        { ".src", "application/x-wais-source" },
        { ".ssm", "application/streamingmedia" },
        { ".sst", "application/vnd.ms-pki.certstore" },
        { ".stl", "application/vnd.ms-pki.stl" },
        { ".sv4cpio", "application/x-sv4cpio" },
        { ".sv4crc", "application/x-sv4crc" },
        { ".svg", "image/svg+xml" },
        { ".svgz", "image/svg+xml" },
        { ".swf", "application/x-shockwave-flash" },
        { ".t", "application/x-troff" },
        { ".tar", "application/x-tar" },
        { ".tcl", "application/x-tcl" },
        { ".tex", "application/x-tex" },
        { ".texi", "application/x-texinfo" },
        { ".texinfo", "application/x-texinfo" },
        { ".tgz", "application/x-compressed" },
        { ".thmx", "application/vnd.ms-officetheme" },
        { ".thn", "application/octet-stream" },
        { ".tif", "image/tiff" },
        { ".tiff", "image/tiff" },
        { ".toc", "application/octet-stream" },
        { ".tr", "application/x-troff" },
        { ".trm", "application/x-msterminal" },
        { ".ts", "video/mp2t" },
        { ".tsv", "text/tab-separated-values" },
        { ".ttc", "application/x-font-ttf" },
        { ".ttf", "font/ttf" },
        { ".tts", "video/vnd.dlna.mpeg-tts" },
        { ".txt", "text/plain" },
        { ".u32", "application/octet-stream" },
        { ".uls", "text/iuls" },
        { ".ustar", "application/x-ustar" },
        { ".vbs", "text/vbscript" },
        { ".vcf", "text/x-vcard" },
        { ".vcs", "text/plain" },
        { ".vdx", "application/vnd.ms-visio.viewer" },
        { ".vml", "text/xml" },
        { ".vsd", "application/vnd.visio" },
        { ".vss", "application/vnd.visio" },
        { ".vst", "application/vnd.visio" },
        { ".vsto", "application/x-ms-vsto" },
        { ".vsw", "application/vnd.visio" },
        { ".vsx", "application/vnd.visio" },
        { ".vtx", "application/vnd.visio" },
        { ".wasm", "application/wasm" },
        { ".wav", "audio/wav" },
        { ".weba", "audio/webm" },
        { ".wax", "audio/x-ms-wax" },
        { ".wbmp", "image/vnd.wap.wbmp" },
        { ".wcm", "application/vnd.ms-works" },
        { ".wdb", "application/vnd.ms-works" },
        { ".webm", "video/webm" },
        { ".webmanifest", "application/manifest+json" }, // https://w3c.github.io/manifest/#media-type-registration
        { ".webp", "image/webp" },
        { ".wks", "application/vnd.ms-works" },
        { ".wm", "video/x-ms-wm" },
        { ".wma", "audio/x-ms-wma" },
        { ".wmd", "application/x-ms-wmd" },
        { ".wmf", "application/x-msmetafile" },
        { ".wml", "text/vnd.wap.wml" },
        { ".wmlc", "application/vnd.wap.wmlc" },
        { ".wmls", "text/vnd.wap.wmlscript" },
        { ".wmlsc", "application/vnd.wap.wmlscriptc" },
        { ".wmp", "video/x-ms-wmp" },
        { ".wmv", "video/x-ms-wmv" },
        { ".wmx", "video/x-ms-wmx" },
        { ".wmz", "application/x-ms-wmz" },
        { ".woff", "font/woff" }, // https://www.w3.org/TR/WOFF/#appendix-b
        { ".woff2", "font/woff2" }, // https://www.w3.org/TR/WOFF2/#IMT
        { ".wps", "application/vnd.ms-works" },
        { ".wri", "application/x-mswrite" },
        { ".wrl", "x-world/x-vrml" },
        { ".wrz", "x-world/x-vrml" },
        { ".wsdl", "text/xml" },
        { ".wtv", "video/x-ms-wtv" },
        { ".wvx", "video/x-ms-wvx" },
        { ".x", "application/directx" },
        { ".xaf", "x-world/x-vrml" },
        { ".xaml", "application/xaml+xml" },
        { ".xap", "application/x-silverlight-app" },
        { ".xbap", "application/x-ms-xbap" },
        { ".xbm", "image/x-xbitmap" },
        { ".xdr", "text/plain" },
        { ".xht", "application/xhtml+xml" },
        { ".xhtml", "application/xhtml+xml" },
        { ".xla", "application/vnd.ms-excel" },
        { ".xlam", "application/vnd.ms-excel.addin.macroEnabled.12" },
        { ".xlc", "application/vnd.ms-excel" },
        { ".xlm", "application/vnd.ms-excel" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12" },
        { ".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".xlt", "application/vnd.ms-excel" },
        { ".xltm", "application/vnd.ms-excel.template.macroEnabled.12" },
        { ".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template" },
        { ".xlw", "application/vnd.ms-excel" },
        { ".xml", "text/xml" },
        { ".xof", "x-world/x-vrml" },
        { ".xpm", "image/x-xpixmap" },
        { ".xps", "application/vnd.ms-xpsdocument" },
        { ".xsd", "text/xml" },
        { ".xsf", "text/xml" },
        { ".xsl", "text/xml" },
        { ".xslt", "text/xml" },
        { ".xsn", "application/octet-stream" },
        { ".xtp", "application/octet-stream" },
        { ".xwd", "image/x-xwindowdump" },
        { ".z", "application/x-compress" },
        { ".zip", "application/x-zip-compressed" },
        { ".m3u8", "application/vnd.apple.mpegurl" },
        { ".cdr", "application/cdr" },
        { ".apk", "application/vnd.android.package-archive" },
        { ".pem", "application/x-pem-file" },
        { ".deb", "application/vnd.debian.binary-package" },
        { ".7z", "application/x-7z-compressed" },
        { ".nupkg", "application/vnd.nuget.package" },
        { ".snupkg", "application/vnd.nuget.package" },
        { ".ofd", "application/ofd" },
        { ".jsonl", "application/json-lines" },
        { ".msix", "application/msix" },
        { ".ini", "text/plain" },
        { ".log", "text/plain" },
        { ".toml", "application/toml" },
        { ".yaml", "application/x-yaml" },
        { ".yml", "application/x-yaml" },
        { ".torrent", "application/x-bittorrent" },
        { ".odt", "application/vnd.oasis.opendocument.text" },
        { ".odp", "application/vnd.oasis.opendocument.presentation" },
        { ".odg", "application/vnd.oasis.opendocument.graphics" },
        { ".epub", "application/epub+zip" },
        { ".mobi", "application/x-mobipocket-ebook" },
        { ".azw", "application/vnd.amazon.ebook" },
        { ".avif", "image/avif" },
        { ".heic", "image/heic" },
        { ".heif", "image/heif" },
        { ".mkv", "video/x-matroska" },
        { ".flac", "audio/flac" },
        { ".opus", "audio/opus" },
        { ".srt", "application/x-subrip" },
        { ".vtt", "text/vtt" },
        { ".iso", "application/x-iso9660-image" },
        { ".dmg", "application/x-apple-diskimage" },
        { ".cs", "text/plain" },
        { ".vb", "text/plain" },
        { ".fs", "text/plain" },
        { ".py", "text/plain" },
        { ".rb", "text/plain" },
        { ".go", "text/plain" },
        { ".rs", "text/plain" },
        { ".sql", "text/plain" },
        { ".bat", "text/plain" },
        { ".ps1", "text/plain" }
    })
    {
    }

    /// <summary>
    ///     <inheritdoc cref="FileTypeMapper" />
    /// </summary>
    /// <param name="mapping">文件扩展名及其对应内容类型映射字典</param>
    /// <exception cref="ArgumentNullException"></exception>
    public FileTypeMapper(IDictionary<string, string> mapping)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(mapping);

        Mappings = mapping;
    }

    /// <summary>
    ///     文件扩展名及其对应内容类型映射字典
    /// </summary>
    public IDictionary<string, string> Mappings { get; }

    /// <summary>
    ///     尝试根据文件路径获取扩展名
    /// </summary>
    /// <param name="subpath">文件路径</param>
    /// <param name="contentType">内容类型</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool TryGetContentType(string subpath, [MaybeNullWhen(false)] out string contentType)
    {
        // 根据文件路径解析扩展名
        var extension = ExtractExtension(subpath);

        // 空检查
        if (extension is not null)
        {
            return Mappings.TryGetValue(extension, out contentType);
        }

        contentType = null;
        return false;
    }

    /// <summary>
    ///     根据文件路径获取扩展名
    /// </summary>
    /// <param name="subpath">文件路径</param>
    /// <param name="defaultContentType">默认内容类型</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string GetContentType(string subpath, string defaultContentType = "application/octet-stream") =>
        new FileTypeMapper().TryGetContentType(subpath, out var contentType) ? contentType : defaultContentType;

    /// <summary>
    ///     根据文件路径解析扩展名
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string? ExtractExtension(string path)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var index = path.LastIndexOf('.');
        return index < 0 ? null : path[index..];
    }
}