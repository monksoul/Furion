namespace Furion.Application;

/// <summary>
/// 测试异常
/// </summary>
public class TestException : IDynamicApiController
{
    public string GetText(string errorCode, bool hideErrorCode = false)
    {
        return Oops.Text("ErrorCodes.z1000", hideErrorCode);
    }

    public string 测试多语言()
    {
        throw Oops.Oh(ErrorCodes.z1000);
    }

    [IfException(1000, ErrorMessage = "我出异常了: {0} 不存在")]
    public async Task<string> 测试IfException异常()
    {
        throw Oops.Oh(1000, "代码");
    }
}

[ErrorCodeType]
public enum ErrorCodes
{
    [ErrorCodeItemMetadata("{0} 不能小于 {1}")]
    z1000,

    [ErrorCodeItemMetadata("数据不存在")]
    x1000,
}