<p></p>
<p></p>

<p align="center">
<img src="https://gitee.com/dotnetchina/Furion/raw/main/handbook/static/img/furlogo.png" height="80"/>
</p>

<div align="center">

[![star](https://gitee.com/dotnetchina/Furion/badge/star.svg?theme=gvp)](https://gitee.com/dotnetchina/Furion/stargazers) [![fork](https://gitee.com/dotnetchina/Furion/badge/fork.svg?theme=gvp)](https://gitee.com/dotnetchina/Furion/members) [![GitHub stars](https://img.shields.io/github/stars/MonkSoul/Fur?logo=github)](https://github.com/MonkSoul/Fur/stargazers) [![GitHub forks](https://img.shields.io/github/forks/MonkSoul/Fur?logo=github)](https://github.com/MonkSoul/Fur/network) [![GitHub license](https://img.shields.io/github/license/MonkSoul/Fur)](https://github.com/MonkSoul/Fur/blob/main/LICENSE) [![nuget](https://img.shields.io/badge/Nuget-1.0.x-blue)](https://www.nuget.org/packages/Fur)

</div>

<div align="center">

`Fur` 是 `.NET 5` 平台下企业应用开发最佳实践框架。

</div>

<p></p>
<p></p>
<p></p>
<p></p>

## 💐 序言

> 无私奉献不是天方夜谭，有时候，我们也可以做到。

## 🍕 名字的由来

> 故事是这样子的：
>
> 起初，想开发一个极易入门、极易维护的框架，开发理念为：`一切从简，只为了更懒`。
>
> 所以自然而然想到了：`Lazier`，也就是 **更懒** 的意思。但是 **更懒** 和 **更烂** 读音相近且中文名没有特色，对此换名问题我苦恼了好几天。
>
> 刚好有一次我在博客园中帮一个博友解答问题，解决后博友赞扬我对 `.NET Core` 颇有了解，我就顺嘴回答了一句：**“略懂皮毛”**。
>
> 就这时，脑瓜子灵机一动，干脆起名为：**“皮毛”**？英文单词 **“`Fur` [fɜː(r)]”**，单词又短而且中文读音既俗气又顺口。😄😎
>
> 所以，**`Fur`** 就诞生了。
>
> 之后就有了 **“小僧不才，略懂皮毛（Fur）。”** 广告语 和 **[furos.cn](https://furos.cn)** 域名。

## 🍔 关于 LOGO

`Fur` LOGO 设计由 `F` `U` `R` 三个单词组成：

<p>
<img src="https://gitee.com/dotnetchina/Furion/raw/main/handbook/static/img/logo2.png" height="120"/>
</p>

我相信很多人看到 `Fur` 的 LOGO 时都会问：“为什么选择奶牛？”，因为 **那些年吹过的牛逼都实现了 🐮**。

之所以选择 **奶牛** 是因为 `牛` 具有脚踏实地，任劳任怨的做事风格，同时 **奶牛** 意味着丰富的营养价值，正如 `Fur` 所能带给你的。

## 🍟 文档地址

- 国内文档：[https://monksoul.gitee.io/fur/](https://monksoul.gitee.io/fur/)
- 国外文档：[https://furos.cn](https://furos.cn)

**目前文档正在逐步完善中。**

## 🥦 系列教程

- **Fur 视频教程：[https://space.bilibili.com/695987967](https://space.bilibili.com/695987967)**
- Fur 使用例子：[https://gitee.com/dotnetchina/Furion-samples](https://gitee.com/dotnetchina/Furion-samples)
- Fur 系列教程：[《学 .NET 5 从 Fur 开始》](https://gitee.com/dotnetchina/Furion/blob/main/tutorials) 系列

## 🌭 开源地址

- Gitee：[https://gitee.com/dotnetchina/Furion](https://gitee.com/dotnetchina/Furion)
- GitHub：[https://github.com/monksoul/Fur](https://github.com/monksoul/Fur)
- Docker：[https://hub.docker.com/r/monksoul/fur](https://hub.docker.com/r/monksoul/fur)
- Nuget：[https://www.nuget.org/packages/Fur](https://www.nuget.org/packages/Fur)

## 🍿 Docker 镜像

- `Docker Hub` 线上镜像

```shell
docker run --name fur -p 5000:80 monksoul/fur:v1.0.0
```

- `手动` 打包镜像

打开 `CMD/Shell/PowerShell` 进入 `Fur` 项目根目录打包 `Fur` 镜像：

```shell
docker build -t fur:v1.0.0 .
```

打包成功后，直接 `docker run`：

```shell
docker run --name fur -p 5000:80 fur:v1.0.0
```

## 🥥 Fur 官方包

| 包类型                                                                                                                            | 名称                                    | 版本   | 描述                |
| --------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------- | ------ | ------------------- |
| [![nuget](https://img.shields.io/badge/Nuget-1.0.x-blue)](https://www.nuget.org/packages/Fur)                                     | Fur                                     | v1.0.2 | Fur 总包            |
| [![nuget](https://img.shields.io/badge/Nuget-1.0.x-blue)](https://www.nuget.org/packages/Fur.Extras.Authentication.JwtBearer)     | Fur.Extras.Authentication.JwtBearer     | v1.0.0 | Fur Jwt 拓展包      |
| [![nuget](https://img.shields.io/badge/Nuget-1.0.x-blue)](https://www.nuget.org/packages/Fur.Extras.DependencyModel.CodeAnalysis) | Fur.Extras.DependencyModel.CodeAnalysis | v1.0.1  | Fur 动态反射 拓展包 |

## 🍄 Fur 脚手架

| 模板类型                                                                                                         | 名称                | 版本   | 描述             |
| ---------------------------------------------------------------------------------------------------------------- | ------------------- | ------ | ---------------- |
| [![nuget](https://img.shields.io/badge/Nuget-1.0.x-yellow)](https://www.nuget.org/packages/Fur.Template.Mvc/)    | Fur.Template.Mvc    | v1.0.3 | Mvc 模板         |
| [![nuget](https://img.shields.io/badge/Nuget-1.0.x-yellow)](https://www.nuget.org/packages/Fur.Template.Api/)    | Fur.Template.Api    | v1.0.3 | WebApi 模板      |
| [![nuget](https://img.shields.io/badge/Nuget-1.0.x-yellow)](https://www.nuget.org/packages/Fur.Template.App/)    | Fur.Template.App    | v1.0.3 | Mvc/WebApi 模板  |
| [![nuget](https://img.shields.io/badge/Nuget-1.0.x-yellow)](https://www.nuget.org/packages/Fur.Template.Razor/)  | Fur.Template.Razor  | v1.0.3 | Razar Pages 模板 |
| [![nuget](https://img.shields.io/badge/Nuget-1.0.x-yellow)](https://www.nuget.org/packages/Fur.Template.Blazor/) | Fur.Template.Blazor | v1.0.3 | Blazor 模板      |

## 🍎 框架特点

- 全新面貌：基于 `.NET 5` 平台，没有历史包袱
- 极易入门：只需要一个 `Inject()` 即可完成配置
- 极速开发：内置丰富的企业应用开发功能
- 极少依赖：框架只依赖三个第三方包
- 极其灵活：轻松面对多变复杂的需求
- 极易维护：采用独特的架构思想，只为长久维护设计
- 完整文档：提供完善的开发文档

## 🥞 架构设计

正在整理中...

## 🥝 功能模块

<p align="center">
<img src="https://gitee.com/dotnetchina/Furion/raw/main/handbook/static/img/furfunctions.png"/>
</p>

## 🥐 框架依赖

`Fur` 为了追求极速入门，极致性能，尽可能的不使用或减少第三方依赖。目前 `Fur` 仅集成了以下三个依赖：

- [Mapster](https://github.com/MapsterMapper/Mapster)：比 `AutoMapper` 还高性能的对象映射
- [MiniProfiler](https://github.com/MiniProfiler/dotnet)：性能分析和监听必备
- [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)：`Swagger` 接口文档

麻雀虽小五脏俱全。`Fur` 即使只集成了这三个依赖，但是主流的 `依赖注入/控制反转`，`AOP` 面向切面编程，`事件总线`，`数据验证`，`数据库操作` 等等一个都不少。

## 🥗 环境要求

- Visual Studio 2019 Preview 16.8 +
- .NET 5 SDK +
- .Net Standard 2.1 +

## 🥪 支持平台

- 运行环境
  - Windows
  - Linux
  - MacOS
  - Docker/K8S/K3S/Rancher
  - Xamarin/MAUI
- 数据库
  - SqlServer
  - Sqlite
  - Azure Cosmos
  - MySql
  - MariaDB
  - PostgreSQL
  - InMemoryDatabase
  - Oracle
  - Firebird
  - 达梦数据库
- 应用部署
  - Kestrel
  - Nginx
  - Jexus
  - IIS
  - Apache
  - PM2
  - Supervisor
  - 单文件
  - 容器（Docker/K8S/K3S/Rancher）

## 🍖 关于性能

`Fur` 目前采用 `Visual Studio 2019 Preview 16.8` 自带性能测试和 `JMeter` 进行测试，由于篇幅有限，只贴部分测试图，测试结果如下：

<img src="https://gitee.com/dotnetchina/Furion/raw/main/handbook/static/img/xncs.png"/>

---

## 🍚 关于作者

一个拥有 12 年开发经验 `.NETer`。喜欢分享，喜欢新技术，在互联网多个技术领域皆有涉猎，百签 MVP/FVC 专家，同时多次获得开源中国 Gitee 授予 GVP 证书。

## 🍤 项目成员

<a href="https://gitee.com/monksoul" target="_blank" margin="5" style='margin:5px'><img src="https://portrait.gitee.com/uploads/avatars/user/324/974299_monksoul_1578937227.png!avatar100" height="40"/></a>
<a href="https://gitee.com/dotnetchina" target="_blank"  margin="5" style='margin:5px'><img src="https://portrait.gitee.com/uploads/avatars/user/2685/8055741_dotnetchina_1599843748.png!avatar100" height="40"/></a>
<a href="https://gitee.com/zero530" target="_blank"  margin="5" style='margin:5px'><img src="https://portrait.gitee.com/uploads/avatars/user/574/1722306_zero530_1578958528.png!avatar100" height="40"/></a>
<a href="https://gitee.com/rgleehom" target="_blank" margin="5" style='margin:5px'><img src="data:image/png;base64,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" height="40"/></a>
<a href="https://gitee.com/KaneLeung" target="_blank" margin="5" style='margin:5px'><img src="https://portrait.gitee.com/uploads/avatars/user/483/1449079_KaneLeung_1600227210.png!avatar100" height="40"/></a>
<a href="https://gitee.com/qd98zhq" target="_blank" margin="5" style='margin:5px'><img src="https://portrait.gitee.com/uploads/avatars/user/205/617984_qd98zhq_1600045204.png!avatar100" height="40"/></a>
<a href="https://gitee.com/andyliuqiurong" target="_blank" margin="5" style='margin:5px'><img src="https://portrait.gitee.com/uploads/avatars/user/19/58386_andyliuqiurong_1600142677.png!avatar100" height="40"/></a>
<a href="https://gitee.com/co1024" target="_blank" margin="5" style='margin:5px'><img src="https://portrait.gitee.com/uploads/avatars/user/627/1883684_co1024_1600418760.png!avatar100" height="40"/></a>
<a href="https://gitee.com/LkyQiuFeng" target="_blank" margin="5" style='margin:5px'><img src="https://portrait.gitee.com/uploads/avatars/user/195/586044_LkyQiuFeng_1594628004.png!avatar100" height="40"/></a>
<a href="https://gitee.com/yzyk126" target="_blank" margin="5" style='margin:5px'><img src="https://portrait.gitee.com/uploads/avatars/user/237/711378_yzyk126_1600742932.png!avatar100" height="40"/></a>
<a href="https://gitee.com/lindexi" target="_blank" margin="5" style='margin:5px'><img src="https://portrait.gitee.com/uploads/avatars/user/262/787628_lindexi_1600869623.png!avatar100" height="40"/></a>
<a href="https://gitee.com/www.fengyunmy.com" target="_blank" margin="5" style='margin:5px'><img src="https://portrait.gitee.com/uploads/avatars/user/33/101022_www.fengyunmy.com_1602044110.png!avatar100" height="40"/></a>
<a href="https://gitee.com/zhouhuasheng2020" target="_blank" margin="5" style='margin:5px'><img src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADwAAAA8CAYAAAA6/NlyAAABiUlEQVRoQ+2YsUoDQRRF78BKstlFv8ZCEYKFCJIihRgkiIiVjUUav8HGwsZKRCRIxCKFCGIhNlr4IdbKJpvgwop2okJ2dnwPxpt63rtzzxlSrHluLOT4Rz/Dwp7bpmHPBYOGadgzAnzSngn9VoeGadgzAnzSngnlnxafNJ+0ZwScPOmZ3T2ES40/RZPeXuHlcL90BgvbIIzXt1Cdr9uMTjwzerhHcn4y8fnfDjoxXPoWggtYWBC2ShQNq2AXDKVhQdgqUc4Mh4vLmN7pwIQ1p0XydIjXowOkdzdO9rKwLcbK7Bzi1iZMpWq74se5fDxC0jvF+OnRyV5nhp3cRmAJCwtAVo2gYVX8AuE0LABZNYKGi+CP1jYQNVswwVSRMeuzefaGQb+HwcWZ9Y5ShuP2NqLVtmzhyy6S7rFO4dpK8/PzrAkC6wsUGcyzDB+fa4fX/SJjX86WMmydqjjIworwRaJpWASzYggNK8IXiaZhEcyKITSsCF8kmoZFMCuG0LAifJFoGhbBrBjyDtay2gX6ZzdZAAAAAElFTkSuQmCC" height="40"/></a>
<a href="https://gitee.com/mabo192" target="_blank" margin="5" style='margin:5px'><img src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADwAAAA8CAYAAAA6/NlyAAAESElEQVRoQ+2ZbYhUVRjH//f9znVaHd3SfbdWcZ0U2SCoT36I0KBI6kNKSFJZWpIvYFZUbIX1RawQKaJUotgkPxSCIn1SJJPEsFJ33V3ZtpwdF3Xdt7lz79yXOCccdozZ2Z1z7oyO936duWee3/N77nmec0cYOj7dxx10CSFwhdsODVe4YISGQ8MVloGwpCtM6P9wQsOh4QrLQFjSFSb01ti0xMg8eGZ3WXJb0pJW6zZAnfMiBFGD2f0anMGfSg5dUmD93u1Qa14CBBnOtSNInV9Z2cCklI2WryEacfjuMKze92AnvyopdEkNEzKtfjO0hm2AqMMd/Q2pP5+A746WDLrkwIIUhbHoIKRoK+DbsP75BFbfh7c/sKjWQqp6GBAV+FYCztCxLJRasw5a07sQJAOe2YNUx7PwUh0lgQ7MsHLPSuj37QAx6lw/itTZJ7NA1HL8AKSqhwDfhZ3cg/TFrZULTMhyLFt/w+x8Hu7Ir4FDczNMSpjsvDc2oIkMEypqefEhSEYcztBxWH3bs8DkXrX2FQhSFcwLa7kmghuwsbAdcuxReGYX0r1tEJRY3pK+oVGe8QjgWxR4/BVZsBdK9QoAPjID+2F2redmnhtwtPVn2l89OwGz8wWIelNB4HwU8qzHEWn+GIJyN3y7H+aFl3M2PRZ6LsDyzOWIzNtFA3RHTmLs92UoVNKFgg7KMhdgtW4j9MY36TCRGWinJcgKrFQ/Db15BwQ5xtUyF+DIgj1Qqp8CvDTSfR/BvvQpMzCpACO+H3JsGQAPdnIf0j1bChVGwc+5AE9bchRSdEmOCVbDtHXNfg7a3PchyNPhcWpdzMA5z+/oGYydWUqzzAOYrDNt8eH/JjZOAwozsNb4FrT6TYCgIHP5W5jdG7gCa/VboDW8TvcH0vJS51cxvTxgBjbu/xHyjKXw3RSsv9pg93/BFZgeKRe2Q4zMB/wMrEu76e8UezEBS3c9CLJhiVoDPKsPZscauKOnuQKTxfTmnVDnrCHzGdzhkxj7Y3mxvGz/Ho4vN2fwCFLnnskGwvIMG/HvIag1cK4cgN3/JX2GtaZ34Fw9CDvxGdP5mclwdjggpXbTuZYFONr6C0SjBb4ziHTPZmSu/FC00ZtvZAImi5ExUJn5GO2T4087xQJL0QcQadkHUWvMjqnu8IlbBzhfJMUC52tzvIiZDfMGJm81taY2+jbEuXaYtiGeV3DA1SugkxOPHJv0lETOyJGWb2ibo4NG4nOke9/mycu2S08USU7/JCdbOwl35FTeHZbAitFWiFotbT9+ZgBm16vcX9YHZpgkQ2t8A1rdJjolTeny0rASZMD4YEq3TebLgQLTmXr2aqg1ayHqcwGIE8fkO/DMTtjJvcgMfDeZ+Kf8ncCBpxxRwDeEwAEnuOzLh4bLriDgAELDASe47MuHhsuuIOAAQsMBJ7jsy4eGy64g4ABCwwEnuOzL33GG/wUT+oIP6lWw9QAAAABJRU5ErkJggg==" height="40"/></a>
<a href="https://gitee.com/yqyx" target="_blank" margin="5" style='margin:5px'><img src="https://portrait.gitee.com/uploads/avatars/user/458/1374721_yqyx_1602561388.png!avatar200" height="40"/></a>
<a href="https://gitee.com/ZYX315" target="_blank" margin="5" style='margin:5px'><img src="https://portrait.gitee.com/uploads/avatars/user/285/856214_ZYX315_1602561602.png!avatar200" height="40"/></a>
<a href="https://gitee.com/hdying" target="_blank" margin="5" style='margin:5px'><img src="https://portrait.gitee.com/uploads/avatars/user/660/1982081_hdying_1602561538.png!avatar200" height="40"/></a>
<a href="https://gitee.com/vincywindy" target="_blank" margin="5" style='margin:5px'><img src="https://portrait.gitee.com/uploads/avatars/user/313/939875_vincywindy_1602575487.png!avatar200" height="40"/></a>
<a href="https://gitee.com/Executor-Cheng" target="_blank" margin="5" style='margin:5px'><img src="https://portrait.gitee.com/uploads/avatars/user/2178/6536098_Executor-Cheng_1583720242.png!avatar200" height="40"/></a>
<a href="https://gitee.com/luzjoyme" target="_blank" margin="5" style='margin:5px'><img src="https://portrait.gitee.com/uploads/avatars/user/1838/5516692_luzjoyme_1605065396.png!avatar200" height="40"/></a>

<p></p>

🎈 欢迎更多的开发者加入 `Fur` 大家庭。

## 🍝 他们都在用

- 百签科技（广东）有限公司
- 码为科技（广州）有限公司
- 广州启顺国际货运代理有限公司
- 森丰供应链服务（广州）有限公司
- 中山赢友网络科技有限公司
- 中山模思软件科技有限公司
- 珠海市恒泰新软件有限责任公司
- 珠海思诺锐创软件有限公司
- 深圳市易胜科技有限公司
- 重庆虫儿飞科技有限公司
- 重庆林木森科技有限公司
- 深圳市品立方科技有限公司
- 上海奥达科股份有限公司

如果您的项目使用到 `Fur` 开发，可以告诉我们。

## 🍻 贡献代码

`Fur` 遵循 `Apache-2.0` 开源协议，欢迎大家提交 `PR` 或 `Issue`。

如果要为项目做出贡献，请查看贡献指南。

## 🍍 捐赠列表

注：排序按捐赠顺序书写

| 捐赠人昵称      | 捐赠金额（元）        | 附语                                         |
| --------------- | --------------------- | -------------------------------------------- |
| 🤴 爱吃油麦菜   | **100**               | 感谢您的开源项目！                           |
| 👳‍♂️ 麦壳饼       | **200**               | 感谢您的开源项目！                           |
| 👨 Sun          | **100**               | 感谢您的开源项目！                           |
| 👶 d617617      | **20**                | 感谢您的开源项目！                           |
| 👦 Diqiguoji008 | **16.66**             | 见贤思齐                                     |
| 👲 nodyang      | **100**               | 感谢您的开源项目！                           |
| 👳‍♀️ mictxd       | **100**               | 吹过的牛都实现。                             |
| 🧓 欧流全       | **10**                | 希望将来超越 Spring                          |
| 👨‍⚕️ lionkon      | **10**                | ...看了框架感觉拿来学习是很不错的...         |
| 😤 好人！       | **10**                | Nice 的小僧，我们的 dotnetchina 马上火起来了 |
| 😮 木木 Woody   | **10**                | 感谢您的开源项目！                           |
| 😚 Joker Hou    | **QQ 超级会员一个月** |                                              |
| 🤠 ccdfz        | **QQ 专属红包 199**   |                                              |
| 🌝 天夫李总     | **支付宝 6666**       | Fur 非常有意思，持续关注                     |
| 😝 六尘子       | **微信红包 199**      | 公司已经使用，小小敬意                       |
| 🤠 ccdfz        | **QQ 专属红包 200**   | 赞助一根内存条                               |
| 🤑 邓亮灯       | **28.88**             | 感谢您的开源项目！                           |
| 😬 天道酬勤     | **微信二维码 188**    | 小小心意                                     |
| 🥰 却月居士     | **QQ 专属红包 100**   | 恭喜发财                                     |
| 🤩 鲁旭         | **100**               | 感谢您的开源项目！                           |
| 😬 散客行       | **微信二维码 666**    | 终于找到一个不错的框架                       |
| 😌 本心         | **100**               | 项目功能很强大                               |

非常感谢您们的支持，正是因为您们，中国开源才可以越走越远，`Fur` 越走越远。

## 🥔 QQ 交流群

<p>
<img src="https://gitee.com/dotnetchina/Furion/raw/main/handbook/static/img/dotnetchina.jpg" height="300"/>
</p>

## 🧆 友情链接

👉 **[Fur](https://gitee.com/dotnetchina/Furion)** 👉 **[SqlSugar](https://github.com/sunkaixuan/SqlSugar)** 👉 **[Layx](https://gitee.com/monksoul/LayX)** 👉 **[t-io](https://gitee.com/tywo45/t-io)** 👉 **[ThinkPHP](http://www.thinkphp.cn/)** 👉 **[Hutool](https://hutool.cn/)** 👉 **[BootstrapAdmin](https://gitee.com/LongbowEnterprise/BootstrapAdmin)** 👉 **[BootstrapBlazor](https://blazor.sdgxgz.com/)** 👉 **[IoTSharp](https://gitee.com/IoTSharp/IoTSharp)**
