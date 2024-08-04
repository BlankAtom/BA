using SKIT.FlurlHttpClient.Wechat;
using SKIT.FlurlHttpClient.Wechat.TenpayV3;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Events;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Models;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Settings;

/* 平台证书管理器，具体用法请参见文档的高级技巧 */
var manager = new InMemoryCertificateManager();
/* 仅列出必须配置项。也包含一些诸如超时时间、UserAgent 等的配置项 */
var options = new WechatTenpayClientOptions()
{
    MerchantId = "微信商户号",
    MerchantV3Secret = "微信商户 v3 API 密钥",
    MerchantCertificateSerialNumber = "微信商户证书序列号",
    MerchantCertificatePrivateKey = "-----BEGIN PRIVATE KEY-----微信商户证书私钥-----END PRIVATE KEY-----",
    PlatformCertificateManager = manager
};
var client = new WechatTenpayClient(options);

/* 以 JSAPI 统一下单接口为例 */
var request = new CreatePayTransactionJsapiRequest()
{
    OutTradeNumber = "商户订单号",
    AppId = "微信 AppId",
    Description = "订单描述",
    ExpireTime = DateTimeOffset.Now.AddMinutes(15),
    NotifyUrl = "https://example.com",
    Amount = new CreatePayTransactionJsapiRequest.Types.Amount()
    {
        Total = 100
    },
    Payer = new CreatePayTransactionJsapiRequest.Types.Payer()
    {
        OpenId = "用户 OpenId"
    }
};

/* 以 JSAPI 统一下单接口为例 */
var response = await client.ExecuteCreatePayTransactionJsapiAsync(request);
if (response.IsSuccessful())
{
    Console.WriteLine("PrepayId：" + response.PrepayId);
}
else
{
    Console.WriteLine("HTTP 状态：" + response.GetRawStatus());
    Console.WriteLine("错误代码：" + response.ErrorCode);
    Console.WriteLine("错误描述：" + response.ErrorMessage);
}

/* 一般情况下可以跳过验证响应的签名 */
bool valid = client.VerifyResponseSignature(response);

/* 字典结构，包含客户端 JS-SDK 调起支付所需的完整参数 */
var paramMap = client.GenerateParametersForJsapiPayRequest(request.AppId, response.PrepayId);

string callbackJson = "{ 微信商户平台发来的 JSON 格式的通知内容 }";
string callbackTimestamp = "微信回调通知中的 Wechatpay-Timestamp 标头";
string callbackNonce = "微信回调通知中的 Wechatpay-Nonce 标头";
string callbackSignature = "微信回调通知中的 Wechatpay-Signature 标头";
string callbackSerialNumber = "微信回调通知中的 Wechatpay-Serial 标头";

valid = client.VerifyEventSignature(callbackTimestamp, callbackNonce, callbackJson, callbackSignature, callbackSerialNumber);
if (valid)
{
    /* 将 JSON 反序列化得到通知对象 */
    /* 你也可以将 WechatTenpayEvent 类型直接绑定到 MVC 模型上，这样就不再需要手动反序列化 */
    var callbackModel = client.DeserializeEvent(callbackJson);
    if ("TRANSACTION.SUCCESS".Equals(callbackModel.EventType))
    {
        /* 根据事件类型，解密得到支付通知敏感数据 */
        var callbackResource = client.DecryptEventResource<TransactionResource>(callbackModel);
        string outTradeNumber = callbackResource.OutTradeNumber;
        string transactionId = callbackResource.TransactionId;
        Console.WriteLine("订单 {0} 已完成支付，交易单号为 {1}", outTradeNumber, transactionId);
    }
}
